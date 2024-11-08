using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username)
    {
        HttpContext.Session.SetString("Username", username);
        return RedirectToAction("Profile");
    }

    public IActionResult Profile()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null)
        {
            return RedirectToAction("Login");
        }
        ViewBag.Username = username;
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("Username");
        return RedirectToAction("Login");
    }
}
