using System.Threading.Tasks;
using SmsGate.Balance.Domain.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceStack;
using SmsGate.Shared.Contract.Command;
using SmsGate.Shared.Contract.Grpc.Balance;

namespace SmsGate.Balance.Components.Consumers;

public class BalancePaymentConsumer : IConsumer<IBalancePaymentCommand>
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<IBalancePaymentCommand> _logger;

    public BalancePaymentConsumer(ILogger<IBalancePaymentCommand> logger, IBalanceService balanceService)
    {
        _logger = logger;
        _balanceService = balanceService;
    }

    public async Task Consume(ConsumeContext<IBalancePaymentCommand> context)
    {
        var message = context.Message;
        _logger.LogInformation("IBalancePaymentCommand recevied: " + message.ToJson());
        var response = await _balanceService.PaymentAsync(context.Message.ConvertTo<BalancePaymentRequest>());
        _logger.LogInformation("IBalancePaymentCommand return: " + response.ToJson());
        await context.RespondAsync(response);
    }
}