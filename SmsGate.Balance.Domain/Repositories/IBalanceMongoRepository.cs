using System;
using System.Threading.Tasks;
using HLS.Paygate.Balance.Models.Dtos;
using HLS.Paygate.Shared;
using MongoDbGenericRepository;

namespace HLS.Paygate.Balance.Domain.Repositories;

public interface IBalanceMongoRepository : IBaseMongoRepository
{
    Task<ResponseMessageBase<object>> BalanceUpdateAsync(TransactionDto transaction,SettlementDto settlementDto, string transRef = "");
    Task<ResponseMessageBase<object>> UpdateBlockMoney(string accountCode, string currencyCode, decimal amount);
}