using Dapper;
using Microsoft.Extensions.Configuration;
using Redis_ASP_NET_Core;
using Redis_ASP_NET_Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Redis_ASP_NET_Core.Repositories
{
    public class UserRepository
    {
        private readonly SqlConnection db;
        private readonly DynamicParameters parameters = new DynamicParameters();
        private readonly CommandType mysp = CommandType.StoredProcedure;
        public UserRepository(IConfiguration Configuration, Connectionstring connection)
        {
            db = new SqlConnection(connection.Value);
        }

        public async Task<List<modelUser>> GetUserbyusername(string username)
        {
            string SP = "SP_Getuserbyusername";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("username", username);
            var data = await db.QueryAsync<modelUser>(SP, parameters, commandType: mysp);
            return data.ToList();
        }

        public async Task<List<modelInputUser>> GetUser()
        {
            string SP = "SP_GetUser";
            var data = await db.QueryAsync<modelInputUser>(SP, parameters, commandType: mysp);
            return data.ToList();
        }

        public async Task<int> InsertUser(modelInputUser user)
        {
            string SP = "SP_InsertUser";
            DynamicParameters parameters = new DynamicParameters();
            parameters.AddDynamicParams(user);
            var data = await db.ExecuteAsync(SP, parameters, commandType: mysp);
            return data;
        }
        
        public async Task<List<dynamic>> DeleteUser(string username)
        {
            string SP = "SP_DeleteUser";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("username", username);
            var data = await db.QueryAsync<dynamic>(SP, parameters, commandType: mysp);
            return data.ToList();
        }
        
        
        public async Task<List<dynamic>> CekUser(string username, string email)
        {
            string SP = "SP_GetuserbyusernameorEmail";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("username", username);
            parameters.Add("email", email);
            var data = await db.QueryAsync<dynamic>(SP, parameters, commandType: mysp);
            return data.ToList();
        }
        
    }
}
