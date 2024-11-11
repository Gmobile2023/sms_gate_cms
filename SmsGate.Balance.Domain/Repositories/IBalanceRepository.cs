using System.Threading.Tasks;
using SmsGate.Balance.Domain.Entities;
using SmsGate.Balance.Models.Dtos;
using SmsGate.Balance.Models.Enums;

namespace SmsGate.Balance.Domain.Repositories;

public interface IBalanceRepository
{
    Task<AccountBalance> AccountBalanceCreateAsync(AccountBalance accountBalance);
    Task<bool> AccountBalanceUpdateAsync(AccountBalance accountBalance);
    Task<AccountBalance> AccountBalanceSelectOneAsync(int id);
    Task<AccountBalance> AccountBalanceSelectOneAsync(string accountCode, string currencyCode, BalanceStatus balanceStatus = BalanceStatus.Active);
    Task<CurrencyDto> CurrencyCreateAsync(CurrencyDto currency);
    Task<CurrencyDto> CurrencyGetAsync(string currencyCode);
    Task<bool> AccountBlockUpdateOnlyAsync(AccountBalance accountBalance);
    Task<bool> AccountBalanceUpdateOnlyAsync(AccountBalance accountBalance);
    Task<bool> AccountBalanceShardCounterUpdateOnlyAsync(AccountBalance accountBalance);
}