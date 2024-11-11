using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using SmsGate.Balance.Models.Dtos;

namespace SmsGate.Balance.Models.Grains;

public interface IBalanceGrain : IGrainWithStringKey//, IRemindable
{
    [AlwaysInterleave]
    Task<decimal> GetBalance();
    Task<decimal> GetBlockMoney();
    Task<ushort> GetShardCounter();
    Task<AccountBalanceDto> GetBalanceAccount();
    Task<bool> BlockBalance(decimal amount, string transcode);
    Task<bool> UnBlockBalance(decimal amount, string transCode);

    //[Transaction(TransactionOption.CreateOrJoin)]
    //Task<ResponseMessageBase<object>> BalanceUpdateAsync(TransactionDto transaction, SettlementDto settlementDto, string transRef = "");

    // Task<SettlementDto> ModifyBalance(SettlementDto settlement);
    Task<SettlementDto> Withdraw(SettlementDto settlement);
    Task<SettlementDto> Deposit(SettlementDto settlement);
    Task RevertBalanceModification(string transCode);
    // [AlwaysInterleave]
    Task<string> GetShardAccount();
    [AlwaysInterleave]
    Task AddShardAccount(string account);
}