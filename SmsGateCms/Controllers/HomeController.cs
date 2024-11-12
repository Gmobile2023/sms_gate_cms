using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmsGateCms.Models;
using Microsoft.AspNetCore.Authorization;
using ServiceStack.Mvc;

namespace SmsGateCms.Controllers;

public class HomeController : ServiceStackController
{
    [HttpGet]
    public IActionResult Index()
    {
        
            // Chuyển hướng từ Index tới Providers
            return RedirectToAction("Messages");
        
    }


    [HttpGet]
    [Authorize(Roles = "Manager")]
    public IActionResult Providers() => View();

    [HttpGet]
    [Authorize(Roles = "Manager")]
    public IActionResult Partners() => View();


    [HttpGet]
    [Authorize]
    public IActionResult MessageTemplates() => View();
    
    [HttpGet]
    [Authorize]
    public IActionResult Messages() => View();

    [HttpGet]
    public IActionResult AuthExamples() => View();

    [HttpGet]
    [Authorize]
    public IActionResult RequiresAuth() => View();

    [HttpGet]
    [Authorize(Roles = "Manager")]
    public IActionResult RequiresRole() => View();

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult RequiresAdmin() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}