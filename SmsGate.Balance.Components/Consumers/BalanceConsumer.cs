using System;
using System.Threading.Tasks;
using HLS.Paygate.Balance.Models.Events;
using HLS.Paygate.Balance.Models.Requests;
using HLS.Paygate.Shared;
using HLS.Paygate.Shared.ConfigDtos;
using HLS.Paygate.Shared.Helpers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceStack;

namespace HLS.Paygate.Balance.Components.Consumers;

public class BalanceConsumer : IConsumer<BalanceChanging>, IConsumer<BalanceChanged>, IConsumer<BalanceDepositMessage>
{
        
    private readonly ILogger<BalanceConsumer> _logger;
    private readonly ServiceUrlConfig _serviceUrlConfig;
    private readonly GrOcClientHepper _grOcClient;

    public BalanceConsumer(ILogger<BalanceConsumer> logger,
         IConfiguration configuration,
         GrOcClientHepper grOcClient)
    {
        _logger = logger;
        _grOcClient = grOcClient;
        _serviceUrlConfig = new ServiceUrlConfig();
        configuration.GetSection("ServiceUrlConfig").Bind(_serviceUrlConfig);
    }

    public async Task Consume(ConsumeContext<BalanceChanged> context)
    {
        try
        {
            _logger.LogInformation($"Consume BalanceChanged Message: {context.Message.TransRef}");
            var request = context.Message;
            if (request.TransactionType == TransactionType.CardCharges)
            {                
                var response = await _grOcClient.GetClientCluster(_serviceUrlConfig.GrpcServices.Backend).SendAsync(new ChargingRequest
                {
                    AccountCode = request.AccountCode,
                    CurrencyCode = CurrencyCode.VND.ToString("G"),
                    TransRef = request.TransCode,
                    TransNote = request.TransNote,
                    Amount = request.Amount,
                    TransactionCode = request.TransCode
                });
                _logger.LogInformation("ChargingRequest return: " + response.ToJson());
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e,$"Consume BalanceChanged error: {e.Message}");
        }
    }

    public Task Consume(ConsumeContext<BalanceChanging> context)
    {
        throw new NotImplementedException();
    }

    public async Task Consume(ConsumeContext<BalanceDepositMessage> context)
    {
        try
        {
            _logger.LogInformation($"Consume BalanceDeposit Message: {context.Message.TransRef}");
            var request = context.Message;           
            var response = await _grOcClient.GetClientCluster(_serviceUrlConfig.GrpcServices.Backend).SendAsync(new DepositRequest
            {
                TransRef = request.TransRef,
                TransNote = request.TransNote,
                Description = request.Description,
                CurrencyCode = request.CurrencyCode,
                AccountCode = request.AccountCode,
                Amount = request.Amount
            });
            _logger.LogInformation("Deposit Auto return: " + response.ToJson());
        }
        catch (Exception e)
        {
            _logger.LogInformation($"Consume BalanceChanged error: {e}");
        }
    }
}