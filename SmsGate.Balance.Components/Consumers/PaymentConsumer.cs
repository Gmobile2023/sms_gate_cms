using System.Threading.Tasks;
using HLS.Paygate.Balance.Models.Requests;
using HLS.Paygate.Shared.ConfigDtos;
using HLS.Paygate.Shared.Contracts.Commands.Balance;
using HLS.Paygate.Shared.Helpers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HLS.Paygate.Shared.DiscoveryRequests.Balance;
using ServiceStack;

namespace HLS.Paygate.Balance.Components.Consumers;

public class PaymentConsumer : IConsumer<PaymentProcessCommand>
{
    

    private readonly ILogger<PaymentConsumer> _logger;
    private readonly ServiceUrlConfig _serviceUrlConfig;
    private readonly GrOcClientHepper _grOcClient;

    public PaymentConsumer(ILogger<PaymentConsumer> logger,
         IConfiguration configuration,
         GrOcClientHepper grOcClient)
    {
        _logger = logger;
        _grOcClient = grOcClient;
        _serviceUrlConfig = new ServiceUrlConfig();
        configuration.GetSection("ServiceUrlConfig").Bind(_serviceUrlConfig);
    }

    public async Task Consume(ConsumeContext<PaymentProcessCommand> context)
    {
        var saleRequest = context.Message;
        _logger.LogInformation("PaymentConsumer_recevied: " + saleRequest.ToJson());
        var response = await _grOcClient.GetClientCluster(_serviceUrlConfig.GrpcServices.Balance).SendAsync(new PaymentRequest
        {
            AccountCode = saleRequest.AccountCode,
            CurrencyCode = saleRequest.CurrencyCode,
            TransRef = saleRequest.TransRef,
            PaymentAmount = saleRequest.PaymentAmount,
            TransNote = saleRequest.TransNote
        });        
        _logger.LogInformation($"{saleRequest.TransRef}-PaymentConsumer_responose: " + response.ToJson());
        await context.RespondAsync(response);
    }
}