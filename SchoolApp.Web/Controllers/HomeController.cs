using Microsoft.AspNetCore.Mvc;
namespace SchoolApp.Web.Controllers;
public class HomeController : Controller
{
    public IActionResult Error() => View();
}
