using System;
using System.Collections.Generic;
using Funq;
using SmsGate.Balance.Domain.Repositories;
using SmsGate.Balance.Domain.Services;
using SmsGate.Balance.Hosting.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Xml;
using ServiceStack;
using ServiceStack.Api.OpenApi;
using ServiceStack.Text;
using SmsGate.Balance.Components.Services;
using SmsGate.Balance.Models.Exceptions;
using HostConfig = ServiceStack.HostConfig;

[assembly: HostingStartup(typeof(AppHost))]

namespace SmsGate.Balance.Hosting.Configurations;

public class AppHost : AppHostBase, IHostingStartup
{
    public AppHost() : base("Smsgate_Balance", typeof(MainService).Assembly)
    {
    }

    public void Configure(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(services =>
            {
                // services.AddMvcCore().AddAuthorization();
                services.AddTransient<MainService>();
                services.AddTransient<IBalanceRepository, BalanceRepository>();
                services.AddTransient<ITransactionRepository, TransactionRepository>();
                services.AddTransient<ITransactionService, TransactionService>();
                services.AddTransient<IBalanceService, BalanceService>();
            })
            .ConfigureAppHost(appHost => { })
            .Configure(app =>
            {
                app.UseAuthentication();
                // Configure ASP .NET Core App
                if (!HasInit)
                    app.UseServiceStack(new AppHost());
            });
    }


    public override void Configure(Container container)
    {
        SetConfig(new HostConfig
        {
            DefaultContentType = MimeTypes.Json,
            DebugMode = true,//AppSettings.Get(nameof(HostConfig.DebugMode), false),
            UseSameSiteCookies = true,
            GlobalResponseHeaders = new Dictionary<string, string>
            {
                { "Server", "nginx/1.4.7" },
                { "Vary", "Accept" },
                { "X-Powered-By", "Zota_Balance" }
            },
            MapExceptionToStatusCode = {
                { typeof(BalanceException), 403 },
            },
            EnableFeatures = Feature.All.Remove(
                Feature.Csv | Feature.Soap11 | Feature.Soap12) // | Feature.Metadata),
            
        });

        ConfigurePlugin<PredefinedRoutesFeature>(feature => feature.JsonApiRoute = null);
        Plugins.Add(new GrpcFeature(App));
        Plugins.Add(new OpenApiFeature());

        JsConfig.Init(new Config
        {
            ExcludeTypeInfo = true
        });
    }

    public void OnExceptionTypeFilter1(Exception ex, ResponseStatus responseStatus)
    {
        var argEx = ex as ArgumentException;
        var isValidationSummaryEx = argEx is ValidationException;
        if (argEx != null && !isValidationSummaryEx && argEx.ParamName != null)
        {
            var paramMsgIndex = argEx.Message.LastIndexOf("Parameter name:");
            var errorMsg = paramMsgIndex > 0
                ? argEx.Message.Substring(0, paramMsgIndex)
                : argEx.Message;

            responseStatus.Errors.Add(new ResponseError
            {
                ErrorCode = ex.GetType().Name,
                FieldName = argEx.ParamName,
                Message = errorMsg,
            });
        }
    }
}