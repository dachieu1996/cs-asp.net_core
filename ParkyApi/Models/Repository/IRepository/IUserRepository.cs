using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyApi.Models.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string userName);
        User Authenticate(string userName, string password);
        User Register(AuthenticationUser user);
    }
}
