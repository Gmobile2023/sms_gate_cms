using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsGate.Balance.Domain.Entities;
using SmsGate.Balance.Models.Dtos;
using SmsGate.Shared.Common;
using Entities_Settlement = SmsGate.Balance.Domain.Entities.Settlement;
using Entities_Transaction = SmsGate.Balance.Domain.Entities.Transaction;
using Settlement = SmsGate.Balance.Domain.Entities.Settlement;
using Transaction = SmsGate.Balance.Domain.Entities.Transaction;

namespace SmsGate.Balance.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Entities_Transaction> TransactionCreateAsync(Entities_Transaction transaction);
    Task<bool> TransactionUpdateAsync(Entities_Transaction transaction);
    Task<Entities_Transaction> TransactionSelectOneAsync(long id);
    Task<Entities_Transaction> TransactionSelectOneAsync(string transCode);
    Task<Entities_Settlement> SettlementCreateAsync(Entities_Settlement settlementDto);
    Task<bool> SettlementUpdateAsync(Entities_Settlement transaction);
    Task<Entities_Settlement> SettlementSelectOneAsync(long id);
    Task<Entities_Settlement> SettlementSelectOneAsync(string transCode);
    Task<List<Entities_Settlement>> SettlementSelectByTransactionAsync(string transCode);
    Task<Entities_Transaction> TransactionSelectOneAsync(string transCode, TransactionType transactionType);
    Task SettlementCreateManyAsync(List<Entities_Settlement> settlements);
    Task<List<Entities_Transaction>> TransactionsCreateAsync(List<Entities_Transaction> transactions);
    Task<List<Entities_Settlement>> GetSettlementSelectByAsync(DateTime fromDate, DateTime toDate,
        string accountCode, string currencyCode, string transCode,string transRef);
}