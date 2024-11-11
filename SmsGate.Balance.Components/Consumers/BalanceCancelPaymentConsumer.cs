using System.Threading.Tasks;
using SmsGate.Balance.Domain.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceStack;
using SmsGate.Shared.Contract.Command;
using SmsGate.Shared.Contract.Grpc.Balance;

namespace SmsGate.Balance.Components.Consumers;

public class BalanceCancelPaymentConsumer : IConsumer<IPaymentCancelCommand>
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<IPaymentCancelCommand> _logger;
    public BalanceCancelPaymentConsumer(ILogger<IPaymentCancelCommand> logger, IBalanceService balanceService)
    {
        _logger = logger;
        _balanceService = balanceService;
    }

    public async Task Consume(ConsumeContext<IPaymentCancelCommand> context)
    {
        var message = context.Message;
        _logger.LogInformation("IPaymentCancelCommand recevied: " + message.ToJson());
        var response = await _balanceService.CancelPaymentAsync(new BalanceCancelPaymentRequest
        {
            TransRef = message.TransCode,
            RevertAmount = message.RevertAmount,
            TransNote = message.TransNote,
            Description = message.TransNote,
            TransactionCode = message.PaymentTransCode,
            CurrencyCode = message.CurrencyCode,
            AccountCode = message.AccountCode
        });
        _logger.LogInformation("CancelPaymentConsumer return: " + response.ToJson());
    }
}