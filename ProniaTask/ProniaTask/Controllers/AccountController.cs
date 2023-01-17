using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProniaTask.Models;
using ProniaTask.Utilities.Enums;
using ProniaTask.ViewModels;

namespace ProniaTask.Controllers
{
    public class AccountController : Controller
    {
        UserManager<AppUser> _userManager { get; }
        SignInManager<AppUser> _signInManager { get; }
        RoleManager<IdentityRole> _roleManager { get; }

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async  Task<IActionResult> Register(UserRegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View();

            AppUser user = await _userManager.FindByNameAsync(registerVM.Username);
            if (user != null)
            {
                ModelState.AddModelError("Username", "This user name is already taken");
                return View();
            }
             user = new AppUser
            {
                FirstName = registerVM.Name,
                LastName = registerVM.Surname,
                Email = registerVM.Email,
                UserName = registerVM.Username
            };
            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View();
            }
            await _userManager.AddToRoleAsync(user,Roles.Member.ToString());
            _signInManager.SignInAsync(user, true);
            return RedirectToAction("Index","Home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginVM loginVM,string? ReturnUrl)
        {
            if (!ModelState.IsValid) return View();
            AppUser user = await _userManager.FindByNameAsync(loginVM.UsernameOrEmail);
            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(loginVM.UsernameOrEmail);
                if (user is null)
                {
                    ModelState.AddModelError("", "Login or password is incorrect!");
                    return View();
                }
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsPersistance, true);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Login or password is incorrect!");
                return View();
            }
            if (ReturnUrl is null)
            {
            return RedirectToAction("Index","Home");
            }
            else
            {
                return Redirect(ReturnUrl);
            }
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> AddRoles()
        {
            foreach (var item in Enum.GetValues(typeof(Roles)))
            {
                if (!await _roleManager.RoleExistsAsync(item.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = item.ToString() });
                }
            }
            return View();
        }
        public async Task<IActionResult> Test()
        {
            var user = await _userManager.FindByNameAsync("Ehtiram00");
            await _userManager.AddToRoleAsync(user,Roles.Member.ToString());
            user = await _userManager.FindByNameAsync("Admin");
            await _userManager.AddToRoleAsync(user, Roles.Admin.ToString());
            return View();
        }
        public IActionResult AccessDenied()
		{
            return View();
		}

    }
}
