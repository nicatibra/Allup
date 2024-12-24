using AllupPraktika.Models;
using AllupPraktika.Utilities.Enums;
using AllupPraktika.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllupPraktika.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        #region Register
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser appUser = new()
            {
                Name = userVM.Name,
                Surname = userVM.Surname,
                UserName = userVM.UserName,
                Email = userVM.Email
            };

            //User yaratmaq + ugurlu olmasa error gostermek
            IdentityResult result = await _userManager.CreateAsync(appUser, userVM.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    return View();
                }
            }

            //yaradilan user-e avtomatik member rolu verilmesi
            await _userManager.AddToRoleAsync(appUser, UserRole.Member.ToString());

            //register olanda avtomatik login olmasi
            await _signInManager.SignInAsync(appUser, isPersistent: false);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        #endregion




        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM userVM, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //context-den ceke bilmirik, usermanager istifade edirik
            //bizim daxil etdiyimiz username ve ya emaile uygun olani databazada varmi?
            AppUser user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userVM.UsernameOrEmail || u.Email == userVM.UsernameOrEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                return View();
            }

            //sign in olmaq ucun
            var result = await _signInManager.PasswordSignInAsync(user, userVM.Password, userVM.IsPersistent, true);
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked, please try again later.");
                return View();
            }

            //ugurlu giris olmasa:
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect");
                return View();
            }

            if (returnUrl is null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return Redirect(returnUrl);

        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


        public async Task<IActionResult> CreateRoles()
        {
            //if eyni rolun bir nece defe yaranmamasinin qarsisini alir
            //foreach ile Enumda olan adlar ile rollari yaradiriq
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = role.ToString() });
                }
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

    }
}
