using System.Threading.Tasks;
using HLS.Paygate.Balance.Models.Dtos;
using HLS.Paygate.Balance.Models.Grains;
using Orleans.Sagas;

namespace HLS.Paygate.Balance.Domain.Activities;

public class ModifyBalanceActivity : IActivity
{
    public const string SETTLEMENT = "Settlement";
    private static bool ReturnResult { get; set; }

    public async Task Execute(IActivityContext context)
    {
        var settlement = context.SagaProperties.Get<SettlementDto>(SETTLEMENT);
        var sourceAccount =
            context.GrainFactory.GetGrain<IBalanceGrain>(settlement.DesAccountCode + "|" + settlement.CurrencyCode);

        settlement = await sourceAccount.ModifyBalance(settlement);
        // ReturnResult = settlement.ReturnResult;
        // if (settlement.ReturnResult)
        context.SagaProperties.Add(SETTLEMENT, settlement);
    }

    public async Task Compensate(IActivityContext context)
    {
        var settlement = context.SagaProperties.Get<SettlementDto>(SETTLEMENT);
        var sourceAccount =
            context.GrainFactory.GetGrain<IBalanceGrain>(settlement.DesAccountCode + "|" + settlement.CurrencyCode);
        await sourceAccount.RevertBalanceModification(settlement.TransCode);
    }

    public Task<bool> HasResult()
    {
        return Task.FromResult(ReturnResult);
    }
}