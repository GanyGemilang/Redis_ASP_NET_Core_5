using Redis_ASP_NET_Core.Models;
using Redis_ASP_NET_Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redis_ASP_NET_Core.Repositories
{
    public class LogicRepository
    {
        private readonly UserRepository userRepository;
        private readonly jwtToken jwttoken;
        public LogicRepository(UserRepository userRepository, jwtToken jwttoken)
        {
            this.userRepository = userRepository;
            this.jwttoken = jwttoken;
        }
        
        public async Task<dynamic> LogicRegister(modelInputUser user)
        {
            //Cek Username and Email
            var getuser = await userRepository.CekUser(user.username, user.email);

            if (getuser.Count > 0)
            {
                return new { Code = "404", Hasil = getuser.FirstOrDefault().message };
            }

            //Encrypt Password
            string passwordhash = BCrypt.Net.BCrypt.HashPassword(user.password);
            return new { Code = "200", Hasil = passwordhash };

        }
    }
}
