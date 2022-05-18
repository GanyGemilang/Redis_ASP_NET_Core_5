using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Redis_ASP_NET_Core.Models;
using Redis_ASP_NET_Core.Repositories;
using Redis_ASP_NET_Core.Utilities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Redis_ASP_NET_Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserRepository userRepository;
        private readonly LogicRepository logicRepository;
        private readonly jwtToken jwttoken;
        private readonly ILogger Log;
        private readonly IDistributedCache _cache;
        public UserController(IConfiguration configuration, ILogger<UserController> Log, UserRepository userRepository, jwtToken jwttoken, LogicRepository logicRepository, IDistributedCache cache)
        {
            this.configuration = configuration;
            this.userRepository = userRepository;
            this.jwttoken = jwttoken;
            this.logicRepository = logicRepository;
            this.Log = Log;
            _cache = cache;
        }

        [HttpPost("Registrasi")]
        public async Task<IActionResult> Registrasi(modelInputUser user)
        {
            try
            {
                Log.LogWarning($"Username : {user.username}");
                Log.LogError($"Email : {user.email}");
                if (user.username == null)
                {
                    return StatusCode(400, Utilities.Response.ResponseMessage("400", "False", "username cannot be null", user));
                }
                else if (user.password == null)
                {
                    return StatusCode(400, Utilities.Response.ResponseMessage("400", "False", "password cannot be null", user));
                }

                //Cek Logic Register
                var logic = await logicRepository.LogicRegister(user);
                if (logic.Code != "200")
                {
                    return StatusCode(int.Parse(logic.Code), Utilities.Response.ResponseMessage(logic.Code, "False", logic.Hasil, null));
                }
                user.password = logic.Hasil;

                //Insert Data User
                var insertUser = await userRepository.InsertUser(user);
                if (insertUser < 1)
                {
                    return StatusCode(404, Utilities.Response.ResponseMessage("404", "False", "Insert User Failed", user));
                }

                //Delete data in redis
                var key = "user";
                await _cache.RemoveAsync(key);
                return Ok(Utilities.Response.ResponseMessage("200", "True", "Insert User Successful", user));
            }
            catch (Exception e)
            {
                return StatusCode(500, Utilities.Response.ResponseMessage("500", "False", e.Message, null));
            }

        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                if (username == null)
                {
                    return StatusCode(400, Utilities.Response.ResponseMessage("400", "False", "username cannot be null", username));
                }

                var data = await userRepository.DeleteUser(username);
                var Hasil = data.FirstOrDefault();
                if (Hasil.Code != "200")
                {
                    return StatusCode(int.Parse(Hasil.Code), Utilities.Response.ResponseMessage(Hasil.Code, "False", Hasil.Message, null));
                }
                return Ok(Utilities.Response.ResponseMessage("200", "True", Hasil.Message, username));
            }
            catch (Exception e)
            {
                return StatusCode(500, Utilities.Response.ResponseMessage("500", "False", e.Message, null));
            }
        }

        [HttpGet("GetUserByUsername")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                if (username == null)
                {
                    return StatusCode(400, Utilities.Response.ResponseMessage("400", "False", "username cannot be null", null));
                }

                var data = await userRepository.GetUserbyusername(username);
                if (data.Count < 1)
                {
                    return StatusCode(404, Utilities.Response.ResponseMessage("404", "False", "Filed Update Login", null));
                }
                return Ok(Utilities.Response.ResponseMessage("200", "True", "Get data User Successful", data.FirstOrDefault()));
            }
            catch (Exception e)
            {
                return StatusCode(500, Utilities.Response.ResponseMessage("500", "False", e.Message, null));
            }
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                //Get data in Redis
                var key = "user";
                var GetUserRedis = await _cache.GetAsync(key);
                if (GetUserRedis != null)
                {
                    var serializeduser = Encoding.UTF8.GetString(GetUserRedis);
                    var userList = JsonConvert.DeserializeObject<List<modelInputUser>>(serializeduser);
                    return Ok(Utilities.Response.ResponseMessage("200", "True", "Get data User from Redis Successful",userList));
                }
                else
                {
                    var data = await userRepository.GetUser();
                    if (data.Count < 1)
                    {
                        return StatusCode(404, Utilities.Response.ResponseMessage("404", "False", "Filed Update Login", null));
                    }
                    // Insert data to Redis
                    var serializedUserList = JsonConvert.SerializeObject(data);
                    var redisUserList = Encoding.UTF8.GetBytes(serializedUserList);
                    var options = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(DateTime.Now.AddMinutes(30))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(25));
                    await _cache.SetAsync(key, redisUserList, options);
                    return Ok(Utilities.Response.ResponseMessage("200", "True", "Get data User From DB Successful", data));
                }
                
            }
            catch (Exception e)
            {
                return StatusCode(500, Utilities.Response.ResponseMessage("500", "False", e.Message, null));
            }
        }
        
    }
}
