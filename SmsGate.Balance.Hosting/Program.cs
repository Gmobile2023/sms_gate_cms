using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SmsGate.Balance.Domain;
using SmsGate.Balance.Models.Grains;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Sagas;
using Serilog;
using SmsGate.Shared.Utils;

ServiceStackHelper.SetLicense();
ServicePointManager.ServerCertificateValidationCallback +=
    (_, _, _, _) => true;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseSagas().Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromMinutes(1))
        .UseTransactions().AddMemoryGrainStorage("SagasStorage");

    siloBuilder.AddAccountBalanceStorage("balance-grains-storage");

    siloBuilder.Configure<SiloMessagingOptions>(opt =>
        {
            opt.ResponseTimeout = TimeSpan.FromMinutes(3);
            opt.SystemResponseTimeout = TimeSpan.FromMinutes(3);
        }).AddMemoryGrainStorageAsDefault()
        .UseInMemoryReminderService();

#if DEBUG
    siloBuilder.UseLocalhostClustering();
#else
    siloBuilder.UseRedisClustering(otp =>
    {
        otp.ConnectionString = builder.Configuration["Silo:RedisCluster"];
        otp.Database = int.Parse(builder.Configuration["Silo:RedisClusterDatabase"]);
    });
#endif

    var name = Dns.GetHostName(); // get container id
    var ip = Dns.GetHostEntry(name).AddressList
        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

    siloBuilder.ConfigureEndpoints(ip, //(IPAddress.Parse(configuration["Silo:AdvertisedIP"]),
            int.Parse(builder.Configuration["Silo:SiloPort"]),
            int.Parse(builder.Configuration["Silo:GatewayPort"]))
        .Configure<ClusterOptions>(opts =>
        {
            opts.ClusterId = builder.Configuration["Silo:ClusterId"];
            opts.ServiceId = builder.Configuration["Silo:ServiceId"];
        });
});


builder.Logging.AddSerilog();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
    //app.Services.Resolve<AlarmAppVersion>().AlarmVersion();
}

await Task.Factory.StartNew(async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(30));
    var client = app.Services.GetRequiredService<IGrainFactory>();
    var grain = client.GetGrain<IAutoTransferGrain>("AutoTransferGrainKey");
    await grain.Start();
});

await app.RunAsync();
Console.WriteLine("Silo is ready!");