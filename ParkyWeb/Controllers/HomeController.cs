using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkyWeb.Models;
using ParkyWeb.Repository.Interface;

namespace ParkyWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountRepository _accountRepository;

        public HomeController(ILogger<HomeController> logger, IAccountRepository accountRepository)
        {
            _logger = logger;
            _accountRepository = accountRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login()
        {
            var user = new User();
            

            return View(user);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User user)
        {
            var userResult = await _accountRepository.LoginAsync(StaticDetails.AccountAPIUrl + "authenticate/", user);

            if (userResult.Token == null)
            {
                ModelState.AddModelError(string.Empty,"Username or Password is incorrect!");
                return View();
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, userResult.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Role, userResult.Role));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetString("JWToken", userResult.Token);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Register()
        {
            var user = new User();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            var result = await _accountRepository.RegisterAsync(StaticDetails.AccountAPIUrl + "register/", user);

            if (result == false)
                return View();

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout(User user)
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.SetString("JWToken", "");
            return RedirectToAction("Index");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
