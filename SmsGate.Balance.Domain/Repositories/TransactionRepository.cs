using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsGate.Balance.Models.Dtos;

using SmsGate.Balance.Domain.Entities;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using SmsGate.Shared.Common;
using Entities_Settlement = SmsGate.Balance.Domain.Entities.Settlement;
using Entities_Transaction = SmsGate.Balance.Domain.Entities.Transaction;
using Settlement = SmsGate.Balance.Domain.Entities.Settlement;
using Transaction = SmsGate.Balance.Domain.Entities.Transaction;

namespace SmsGate.Balance.Domain.Repositories;

public class TransactionRepository(
    IBalanceConnectionFactory balanceConnectionFactory,
    ILogger<BalanceRepository> logger)
    : ITransactionRepository
{
    public async Task<Entities_Transaction> TransactionCreateAsync(Entities_Transaction transaction)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            var id = await data.InsertAsync(transaction, true);
            transaction.Id = id;
            return transaction;
        }
        catch (Exception e)
        {
            logger.LogError("Error insert Transaction {@Transaction}", e.Message);
        }

        return null;
    }

    public async Task<List<Entities_Transaction>> TransactionsCreateAsync(List<Entities_Transaction> transactions)
    {
        using var data = await balanceConnectionFactory.OpenAsync();

        using var trans = data.OpenTransaction();
        try
        {
            await data.InsertAllAsync(transactions);
            trans.Commit();
            return transactions;
        }
        catch (Exception e)
        {
            logger.LogError("Error insert Transactions {@Transaction}", e.Message);
            trans.Rollback();
        }

        return null;
    }

    public async Task<bool> TransactionUpdateAsync(Entities_Transaction transaction)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            await data.UpdateOnlyAsync(() => new Entities_Transaction
            {
                Status = transaction.Status,
                ModifiedDate = DateTime.Now
            }, tr => tr.TransactionCode == transaction.TransactionCode);

            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Error update Transaction {@Transaction}", e.Message);
        }

        return false;
    }

    public async Task<Entities_Transaction> TransactionSelectOneAsync(long id)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        var transaction = await data.LoadSingleByIdAsync<Entities_Transaction>(id);
        return transaction;
    }

    public async Task<Entities_Transaction> TransactionSelectOneAsync(string transCode)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        var query = data.From<Entities_Transaction>().Where(p => p.TransactionCode == transCode)
            .OrderByDescending(p => p.CreatedDate);
        var transaction = await data.SingleAsync(query);
        return transaction;
    }

    public async Task<Entities_Transaction> TransactionSelectOneAsync(string transCode, TransactionType transactionType)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        var query = data.From<Entities_Transaction>().Where(p => p.TransactionCode == transCode && p.TransType == transactionType)
            .OrderByDescending(p => p.CreatedDate);
        var transaction = await data.SingleAsync(query);
        return transaction;
    }

    public async Task<Entities_Settlement> SettlementCreateAsync(Entities_Settlement settlement)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            var id = await data.InsertAsync(settlement, true);
            settlement.Id = id;
            return settlement;
        }
        catch (Exception e)
        {
            logger.LogError("Error insert Settlement {@Transaction}", e.Message);
        }

        return null;
    }

    public async Task SettlementCreateManyAsync(List<Entities_Settlement> settlements)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            await data.InsertAllAsync(settlements.ConvertTo<List<Entities_Settlement>>());
        }
        catch (Exception e)
        {
            logger.LogError("Error insert Settlements {@Transaction}", e.Message);
        }
    }

    public async Task<bool> SettlementUpdateAsync(Entities_Settlement settlement)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        try
        {
            await data.UpdateOnlyAsync(() => new Entities_Settlement
            {
                // Balance = Transaction.Balance,
                // KppTransId = Transaction.KppTransId,
                // Status = Transaction.Status,
                // KppResponse = Transaction.KppResponse,
                // EndedDate = Transaction.EndedDate,
                // DiscountRate = Transaction.DiscountRate,
                // TransAmount = Transaction.TransAmount,
                // TppType = Transaction.TppType,
                // AccType = Transaction.AccType,
                // DiscountAmount = Transaction.DiscountAmount,
                // ServiceCode = Transaction.ServiceCode
            }, tr => tr.Id == settlement.Id);

            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Error update Settlement {@Settlement}", e.Message);
        }

        return false;
    }

    public async Task<Entities_Settlement> SettlementSelectOneAsync(long id)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        var settlement = await data.LoadSingleByIdAsync<Entities_Settlement>(id);
        return settlement;
    }

    public async Task<Entities_Settlement> SettlementSelectOneAsync(string transCode)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        var query = data.From<Entities_Settlement>().Where(p => p.TransCode == transCode)
            .OrderByDescending(p => p.CreatedDate);
        var settlement = await data.SingleAsync(query);
        return settlement;
    }

    public async Task<List<Entities_Settlement>> SettlementSelectByTransactionAsync(string transCode)
    {
        using var data = await balanceConnectionFactory.OpenAsync();
        var query = data.From<Entities_Settlement>().Where(p => p.TransRef == transCode)
            .OrderByDescending(p => p.CreatedDate);
        var kppAccount = await data.SingleAsync(query);
        return kppAccount?.ConvertTo<List<Entities_Settlement>>();
    }

    public async Task<List<Entities_Settlement>> GetSettlementSelectByAsync(DateTime fromDate, DateTime toDate,
        string accountCode, string currencyCode, string transCode, string transRef)
    {
        try
        {
            logger.LogInformation($"FromDate= {fromDate}|ToDate= {toDate}|AccountCode= {accountCode}|CurrencyCode= {currencyCode}|TransCode= {transCode}");

            using var data = await balanceConnectionFactory.OpenAsync();


            var query =  data.From<Entities_Settlement>()
                      .Join<Entities_Settlement, Entities_Transaction>((z, y) => z.TransRef == y.TransactionCode)
                      .Where<Entities_Transaction>(p => p.Status == SmsGate.Balance.Models.Enums.TransStatus.Done);

            //var query = from x in data.From<Settlement>()
            //            where x.CreatedDate >= fromDate && x.CreatedDate <= toDate
            //            select x;

            query = query.Where(x => x.CreatedDate >= fromDate && x.CreatedDate <= toDate);
            
            if (!string.IsNullOrEmpty(accountCode))
                query = query.Where(c => c.DesAccountCode == accountCode || c.SrcAccountCode == accountCode);

            if (!string.IsNullOrEmpty(accountCode))
                query = query.Where(c => c.CurrencyCode == currencyCode);

            if (!string.IsNullOrEmpty(currencyCode))
                query = query.Where(c => c.CurrencyCode == currencyCode);

            if (!string.IsNullOrEmpty(transCode))
                query = query.Where(c => c.TransRef == transCode);

            if (!string.IsNullOrEmpty(transRef))
                query = query.Where(c => c.PaymentTransCode == transRef);

            var list = data.Select(query);

            return list.ConvertTo<List<Entities_Settlement>>();
        }
        catch (Exception ex)
        {
            logger.LogError($"FromDate= {fromDate}|ToDate= {toDate}|AccountCode= {accountCode}|CurrencyCode= {currencyCode}|TransCode= {transCode} =>GetSettlementSelectByAsync Exception: {ex}");
            return null;
        }
    }
}