using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskAuthenticationAuthorization.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TaskAuthenticationAuthorization.Controllers
{
    public class AccountController : Controller
    {
            private readonly ShoppingContext _context;

            public AccountController(ShoppingContext context)
            {
                _context = context;
            }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.Include(u=>u.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
                if (user != null)
                {
                    await Authenticate(user); // authentication

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Login or Password not valid");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    // add to db
                    user = new User { Email = model.Email, Password = model.Password };
                    Role userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "buyer");
                    if (userRole != null)
                        user.Role = userRole;
                    user.BuyerType = BuyerType.regular;
                    _context.Users.Add(user);


                    await _context.SaveChangesAsync();

                    await Authenticate(user); // authentication

                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", "Login or Password not valid");
            }
            return View(model);
        }

        private async Task Authenticate(User user)
        {
            // create  claims
            var claims = new List<Claim>
            {
              new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
              new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Name),
              new Claim (ClaimTypes.UserData, user.BuyerType.ToString())
  

        };
            // create obj ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
           
            // install auth cookies
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

    }
}
