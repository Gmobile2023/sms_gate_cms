using SmsGate.Balance.Hosting.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;


[assembly: HostingStartup(typeof(ConfigureGrpc))]

namespace SmsGate.Balance.Hosting.Configurations;

public class ConfigureGrpc : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) => { services.AddServiceStackGrpc(); })
            .ConfigureAppHost(appHost => { appHost.GetApp().UseRouting(); });
    }
}