using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ParkyApi.Data;
using ParkyApi.Models.Repository.IRepository;

namespace ParkyApi.Models.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly AppSettings _appSettings;
        public UserRepository(ApplicationDbContext db, IOptions<AppSettings> appSettings)
        {
            _db = db;
            _appSettings = appSettings.Value;
        }

        public bool IsUniqueUser(string userName)
        {
            var user = _db.Users.SingleOrDefault(x => x.UserName == userName);
            if (user == null)
                return true;
            return false;
        }

        public User Authenticate(string userName, string password)
        {
            var user = _db.Users.SingleOrDefault(x => x.UserName == userName && x.Password == password);
            if (user == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,user.Id.ToString()),
                    new Claim(ClaimTypes.Role,user.Role), 
                }),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            


            return user;
        }

        public User Register(AuthenticationUser user)
        {
            var userObj = new User()
            {
                UserName = user.UserName,
                Password = user.Password,
                Role = "User"
            };

            _db.Users.Add(userObj);
            _db.SaveChanges();
            userObj.Password = "";
            return userObj;
        }
    }
}
