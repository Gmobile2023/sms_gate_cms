using System;
using System.Threading.Tasks;
using SmsGate.Balance.Models.Dtos;
using SmsGate.Balance.Models.Enums;
using SmsGate.Balance.Models.Exceptions;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.OrmLite;
using SmsGate.Balance.Domain.Entities;

namespace SmsGate.Balance.Domain.Repositories;

public class BalanceRepository(ILogger<BalanceRepository> logger, IBalanceConnectionFactory balanceConnectionFactory)
    : IBalanceRepository
{
    public async Task<AccountBalance> AccountBalanceCreateAsync(AccountBalance accountBalance)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            var id = await data.InsertAsync(accountBalance, true);
            accountBalance.Id = id;
            return accountBalance;
        }
        catch (Exception e)
        {
            logger.LogError("Error insert AccountBalance {@Account} exception: {Ex}, ", accountBalance, e.Message);
        }

        return null;
    }

    public async Task<bool> AccountBalanceUpdateAsync(AccountBalance accountBalance)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            await data.UpdateAsync(accountBalance);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Error update AccountBalance {@Account}", e.Message);
        }

        return false;
    }
    
    public async Task<bool> AccountBalanceUpdateOnlyAsync(AccountBalance accountBalance)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            var result = await data.UpdateOnlyAsync(() => new AccountBalance
                {
                    Balance = accountBalance.Balance,
                    ModifiedDate = accountBalance.ModifiedDate,
                    LastTransCode = accountBalance.LastTransCode,
                    CheckSum = accountBalance.ToCheckSum()
                },
                tr => tr.AccountCode == accountBalance.AccountCode && tr.CurrencyCode == accountBalance.CurrencyCode &&
                      tr.CheckSum == accountBalance.CheckSum);
            
            if (result == 1)
                return true;
            throw new BalanceException(31,
                $"Account {accountBalance.AccountCode} currency {accountBalance.CurrencyCode} has been modified outside the system");
        }
        catch (Exception e)
        {
            logger.LogError("Error update Only Balance {@Account}", e.Message);
            throw;
        }
    }
    
    public async Task<bool> AccountBalanceShardCounterUpdateOnlyAsync(AccountBalance accountBalance)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            var result = await data.UpdateOnlyAsync(() => new AccountBalance
                {
                    ShardCounter = accountBalance.ShardCounter
                },
                tr => tr.AccountCode == accountBalance.AccountCode && tr.CurrencyCode == accountBalance.CurrencyCode);
            
            if (result == 1)
                return true;
        }
        catch (Exception e)
        {
            logger.LogError("Error update Only Balance {@Account}", e.Message);
            throw;
        }

        return false;
    }
    
    public async Task<bool> AccountBlockUpdateOnlyAsync(AccountBalance accountBalance)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            var result = await data.UpdateOnlyAsync(() => new AccountBalance
                {
                    BlockedMoney = accountBalance.BlockedMoney,
                    ModifiedDate = accountBalance.ModifiedDate,
                    CheckSum = accountBalance.CheckSum
                },
                tr => tr.AccountCode == accountBalance.AccountCode && tr.CurrencyCode == accountBalance.CurrencyCode &&
                      tr.CheckSum == accountBalance.CheckSum);
            
            if (result == 1)
                return true;
            throw new BalanceException(31,
                $"Account {accountBalance.AccountCode} currency {accountBalance.CurrencyCode} has been modified outside the system");
        }
        catch (Exception e)
        {
            logger.LogError("Error update block Only Balance {@Account}", e.Message);
            throw;
        }
    }

    public async Task<AccountBalance> AccountBalanceSelectOneAsync(int id)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        var kppAccount = await data.SingleByIdAsync<AccountBalance>(id);

        return kppAccount?.ConvertTo<AccountBalance>();
    }

    public async Task<AccountBalance> AccountBalanceSelectOneAsync(string accountCode, string currencyCode,
        BalanceStatus balanceStatus = BalanceStatus.Active)
    {
        try
        {
            using var data = await balanceConnectionFactory.OpenAsync();
            var kppAccount = await data.SingleAsync<AccountBalance>(p =>
                p.AccountCode == accountCode && p.CurrencyCode == currencyCode && p.Status == balanceStatus);
            return kppAccount?.ConvertTo<AccountBalance>();
        }
        catch (Exception e)
        {
            logger.LogError("Error select AccountBalance {E}", e.Message);
            return null;
        }
    }
    
    public async Task<CurrencyDto> CurrencyCreateAsync(CurrencyDto currency)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            var id = await data.InsertAsync(currency.ConvertTo<Currency>(), true);
            currency.Id = (int)id;
            return currency;
        }
        catch (Exception e)
        {
            logger.LogError("Error insert Currency {@Account} exception: {Ex}, ", currency, e.Message);
        }

        return null;
    }

    public async Task<CurrencyDto> CurrencyGetAsync(string currencyCode)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            var currency = await data.SingleAsync<Currency>(p => p.CurrencyCode == currencyCode);
            return currency?.ConvertTo<CurrencyDto>();
        }
        catch (Exception e)
        {
            logger.LogError("Error update Currency {@Account}", e.Message);
        }

        return null;
    }
}