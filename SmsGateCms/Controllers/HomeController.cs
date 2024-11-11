using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmsGateCms.Models;
using Microsoft.AspNetCore.Authorization;
using ServiceStack.Mvc;
using SmsGateCms.ServiceInterface.BalanceGrain;

namespace SmsGateCms.Controllers;

public class HomeController : ServiceStackController
{
    private readonly IGrainFactory _grainFactory;

    public HomeController(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Bookings()
    {
        var grain = _grainFactory.GetGrain<ICustomerBalanceGrain>(111 + "|" + "VND");
        decimal amount = 10000;
        await grain.DepositAsync(amount);

        return View();
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