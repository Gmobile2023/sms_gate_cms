using System.Threading.Tasks;
using SmsGate.Balance.Domain.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceStack;
using SmsGate.Shared.Contract.Command;
using SmsGate.Shared.Contract.Grpc.Balance;

namespace SmsGate.Balance.Components.Consumers;

public class CheckPaymentConsumer : IConsumer<ICheckBalanceCommand>
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<ICheckBalanceCommand> _logger;

    public CheckPaymentConsumer(ILogger<ICheckBalanceCommand> logger, IBalanceService balanceService)
    {
        _logger = logger;
        _balanceService = balanceService;
    }

    public async Task Consume(ConsumeContext<ICheckBalanceCommand> context)
    {
        var message = context.Message;
        _logger.LogInformation("ICheckBalanceCommand recevied: " + message.ToJson());
        var response = await _balanceService.AccountBalanceCheckAsync(new AccountBalanceCheckRequest
        {
            AccountCode = context.Message.AccountCode,
            CurrencyCode = context.Message.CurrencyCode
        });
        _logger.LogInformation("ICheckBalanceCommand return: " + response.ToJson());
        await context.RespondAsync(response);
    }
}