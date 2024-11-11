using System;
using System.Linq;
using SmsGate.Balance.Components.Consumers;
using SmsGate.Balance.Hosting.Configurations;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SmsGate.Shared.ConfigDtos;

[assembly: HostingStartup(typeof(ConfigureMassTransit))]

namespace SmsGate.Balance.Hosting.Configurations;

public class ConfigureMassTransit : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<BalancePaymentConsumer>();
                x.AddConsumer<BalanceCancelPaymentConsumer>();
                var massTransitConfig = new MassTransitConfig();
                context.Configuration.GetSection("MassTransitConfig").Bind(massTransitConfig);
                if (massTransitConfig.IsUseGrpc)
                    x.UsingGrpc((c, cfg) =>
                    {
                        cfg.Host(h =>
                        {
                            h.Host = massTransitConfig.GrpcConfig.Host;
                            h.Port = massTransitConfig.GrpcConfig.Port;
                            if (!massTransitConfig.GrpcConfig.AddServer) return;
                            foreach (var server in massTransitConfig.GrpcConfig.Servers)
                                h.AddServer(new Uri(server));
                        });
                        cfg.ConfigureEndpoints(c);
                    });
                else
                    x.UsingRabbitMq((c, cfg) =>
                    {
                        cfg.AutoStart = true;
                        cfg.Host(massTransitConfig.RabbitMqConfig.Host, massTransitConfig.RabbitMqConfig.VirtualHost,
                            h =>
                            {
                                h.Username(massTransitConfig.RabbitMqConfig.Username);
                                h.Password(massTransitConfig.RabbitMqConfig.Password);
                                h.UseCluster(p =>
                                {
                                    foreach (var server in massTransitConfig.RabbitMqConfig.Clusters.Split(";")
                                                 .ToList())
                                        p.Node(server);
                                });
                            });
                        cfg.UseInMemoryOutbox();
                        cfg.ConfigureEndpoints(c);
                    });
            });
        });
    }
}