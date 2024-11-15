using System;
using System.Linq;
using System.Threading.Tasks;
using SmsGate.Balance.Models.Dtos;
using SmsGate.Balance.Models.Enums;
using SmsGate.Balance.Models.Exceptions;
using SmsGate.Balance.Models.Grains;

using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Sagas;
using SmsGate.Shared.Common;

namespace SmsGate.Balance.Domain.Activities;

public class BalanceDepositActivity : IActivity
{
    public const string SETTLEMENT = "Settlement";
    private static bool ReturnResult { get; set; }
    
    private readonly ILogger<BalanceDepositActivity> _logger;

    public BalanceDepositActivity(ILogger<BalanceDepositActivity> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IActivityContext context)
    {
        var settlement = context.SagaProperties.Get<SettlementDto>(SETTLEMENT);
        var transCode = settlement.TransCode;

        if (context.SagaProperties.ContainsKey(transCode))
        {
            var result = context.SagaProperties.Remove(transCode, out settlement);
            if (!result)
                throw new BalanceException(6007, $"Exception when process {transCode}");
        }
        var destinationAccount =
            context.GrainFactory.GetGrain<IBalanceGrain>(settlement.DesAccountCode + "|" + settlement.CurrencyCode);

        if (!new[] { TransactionType.MasterTopup, TransactionType.SystemTransfer }.Contains(settlement.TransType))
        {
            var shard = await destinationAccount.GetShardAccount();

            if (shard != destinationAccount.GetPrimaryKeyString()) //Check nếu là tk Shard thì tạo grain mới
            {
                destinationAccount = context.GrainFactory.GetGrain<IBalanceGrain>(shard);
                settlement.DesShardAccountCode = shard;
                _logger.LogTrace(
                    "BalanceDepositActivity create new shared account:{Shard}-{SettlementPaymentTransCode}-{SettlementTransRef}-{SettlementTransCode}",
                    shard, settlement.PaymentTransCode, settlement.TransRef, settlement.TransCode);
            }
        }
        
        settlement.ModifiedDate = DateTime.Now;
        try
        {
            settlement = await destinationAccount.Deposit(settlement);
            context.SagaProperties.Add(transCode, settlement);
            if (settlement.Status == SettlementStatus.Error)
                throw new Exception("Error transfer money");
        }
        catch (Exception e)
        {
            settlement.Description = e.Message;
            settlement.Status = SettlementStatus.Error;
            context.SagaProperties.Add(transCode, settlement);
            throw;
        } 
    }

    public async Task Compensate(IActivityContext context)
    {
        var settlement = context.SagaProperties.Get<SettlementDto>(SETTLEMENT);
        
        var transCode = settlement.TransCode;

        if (context.SagaProperties.ContainsKey(transCode))
        {
            var result = context.SagaProperties.Remove(transCode, out settlement);
            if (!result)
                throw new BalanceException(6007, $"Exception when process {transCode}");
        }
        
        var sourceAccount =
            context.GrainFactory.GetGrain<IBalanceGrain>(settlement.DesShardAccountCode + "|" + settlement.CurrencyCode);
        await sourceAccount.RevertBalanceModification(settlement.TransCode);
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