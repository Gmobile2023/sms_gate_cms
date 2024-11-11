using System;
using System.Linq;
using System.Threading.Tasks;
using SmsGate.Balance.Models.Dtos;
using SmsGate.Balance.Models.Enums;
using SmsGate.Balance.Models.Grains;

using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Sagas;

namespace SmsGate.Balance.Domain.Activities;

public class BalanceWithdrawActivity : IActivity
{
    public const string SETTLEMENT = "Settlement";

    private readonly ILogger<BalanceWithdrawActivity> _logger;

    public BalanceWithdrawActivity(ILogger<BalanceWithdrawActivity> logger)
    {
        _logger = logger;
    }

    private static bool ReturnResult { get; }

    public async Task Execute(IActivityContext context)
    {
        var settlement = context.SagaProperties.Get<SettlementDto>(SETTLEMENT);
        _logger.LogInformation(
            "BalanceWithdrawActivity: {SettlementPaymentTransCode}-{SettlementTransRef}-{SettlementTransCode}",
            settlement.PaymentTransCode, settlement.TransRef, settlement.TransCode);
        var sourceAccountGrain =
            context.GrainFactory.GetGrain<IBalanceGrain>(settlement.SrcAccountCode + "|" + settlement.CurrencyCode);
        
        // if (new[] { TransactionType.CancelPayment }.Contains(settlement.TransType))
        // {
        //     var shard = await sourceAccountGrain.GetShardAccount();
        //
        //     if (shard != sourceAccountGrain.GetPrimaryKeyString()) //Check nếu là tk Shard thì tạo grain mới
        //     {
        //         sourceAccountGrain = context.GrainFactory.GetGrain<IBalanceGrain>(shard);
        //         settlement.SrcShardAccountCode = shard;
        //         _logger.LogTrace(
        //             "BalanceWithdrawActivity create new shared account:{Shard}-{SettlementPaymentTransCode}-{SettlementTransRef}-{SettlementTransCode}",
        //             shard, settlement.PaymentTransCode, settlement.TransRef, settlement.TransCode);
        //     }
        // }
        //
        settlement = await sourceAccountGrain.Withdraw(settlement);
        settlement.ModifiedDate = DateTime.Now;
        context.SagaProperties.Add(settlement.TransCode, settlement);
    }

    public async Task Compensate(IActivityContext context)
    {
        var settlement = context.SagaProperties.Get<SettlementDto>(SETTLEMENT);
        var sourceAccount =
            context.GrainFactory.GetGrain<IBalanceGrain>(settlement.SrcAccountCode + "|" +
                                                         settlement.CurrencyCode);
        await sourceAccount.RevertBalanceModification(settlement.TransCode);
        _logger.LogInformation(
            $"BalanceWithdrawActivity RevertBalanceModification: {settlement.PaymentTransCode}-{settlement.TransRef}-{settlement.TransCode} reason: " +
            context.GetSagaError());
        if (settlement.Status != SettlementStatus.Error)
        {
            settlement.Status = SettlementStatus.Rollback;
            context.SagaProperties.Add(settlement.TransCode, settlement);
        }
    }

    public Task<bool> HasResult()
    {
        return Task.FromResult(ReturnResult);
    }
}