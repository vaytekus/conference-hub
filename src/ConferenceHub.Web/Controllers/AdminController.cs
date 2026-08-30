using ConferenceHub.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
