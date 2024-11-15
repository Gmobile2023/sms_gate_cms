using System;
using System.Collections.Generic;
using System.Net.Http;
using Funq;
using HLS.Paygate.Balance.Components.Services;
using HLS.Paygate.Balance.Domain.Services;
using HLS.Paygate.Shared;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using ServiceStack.Api.OpenApi;

namespace HLS.Paygate.Balance.Hosting;

public class AppHost : AppHostBase
{
    // private Container _container;
    private readonly IConfiguration _configuration;

    public AppHost(IConfiguration configuration) : base("BalanceService", typeof(MainService).Assembly)
    {
        _configuration = configuration;
    }

    public override void Configure(Container container)
    {
        Plugins.Add(new GrpcFeature(App));
        Plugins.Add(new OpenApiFeature());

        AfterInitCallbacks.Add(host =>
        {
            var balanceService = host.TryResolve<IBalanceService>();
            balanceService.CurrencyCreateAsync(CurrencyCode.VND.ToString("G"));
            balanceService.CurrencyCreateAsync(CurrencyCode.DEBT.ToString("G"));
            balanceService.CurrencyCreateAsync(CurrencyCode.ZOTOP.ToString("G"));
        });
    }
}