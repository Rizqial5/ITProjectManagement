using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel;

namespace ProjectManagement.App.Controllers
{
    public class AccountController : Controller
    {
        private IAuthRepository _authRepository;

        public AccountController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authRepository.RegisterAsync(model);

            if (result.Succeeded)
                return RedirectToAction("Login");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authRepository.LoginAsync(model);
            if (user != null)
                return RedirectToAction("Index", "Home"); // setelah login sukses

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authRepository.LogoutAsync();

            return RedirectToAction("Login");
        }
    }
}
