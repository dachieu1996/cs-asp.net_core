using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkyApi.Models;
using ParkyApi.Models.Repository.IRepository;

namespace ParkyApi.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/users")]
    [ApiVersion("1.0")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repository;

        public UsersController(IUserRepository repository)
        {
            _repository = repository;
        }
        
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticationUser obj)
        {
            var user = _repository.Authenticate(obj.UserName, obj.Password);
            if (user == null)
                return BadRequest(new {message = "Username or password is incorrect"});
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(AuthenticationUser user)
        {
            if (!_repository.IsUniqueUser(user.UserName))
                return BadRequest(new {message = "Username already exists"});
            
            var userResult = _repository.Register(user);
            if (userResult == null)
                return BadRequest(new {message = "Error while registering"});
            
            return Ok(userResult);
        }
    }
}