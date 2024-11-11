using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HLS.Paygate.Balance.Domain.Entities;
using HLS.Paygate.Balance.Domain.Repositories;
using HLS.Paygate.Balance.Models.Dtos;
using HLS.Paygate.Shared;
using MassTransit;
using Microsoft.Extensions.Logging;
using Paygate.Contracts.Commands.Commons;
using Paygate.Contracts.Requests.Commons;
using Paygate.Discovery.Requests.Balance;
using ServiceStack;

namespace HLS.Paygate.Balance.Domain.Services;

public class TransactionReportService : ITransactionReportService
{
    private readonly IBalanceMongoRepository _balanceMongoRepository;

    //private readonly Logger _logger = LogManager.GetLogger("TransactionReportService");
    private readonly ILogger<TransactionReportService> _logger;

    private readonly IBus _bus;

    public TransactionReportService(IBalanceMongoRepository balanceMongoRepository,
        ILogger<TransactionReportService> logger, IBus bus)
    {
        _balanceMongoRepository = balanceMongoRepository;
        _logger = logger;
        _bus = bus;
    }

    public async Task<MessageResponseBase> BalanceHistoryCreateAsync(TransactionDto transaction,
        SettlementDto settlement)
    {
        BalanceHistories balanceHistory = null;
        try
        {
            balanceHistory = GetBalanceHistoryInsert(transaction, settlement);
            await _balanceMongoRepository.AddOneAsync(balanceHistory);
            return new MessageResponseBase
            {
                ResponseCode = "01",
                ResponseMessage = "Thành công"
            };
        }
        catch (Exception ex)
        {
            Task.Run(() =>
            {
                _bus.Publish<SendBotMessage>(new
                {
                    BotType = BotType.Dev,
                    Module = "Balance",
                    MessageType = BotMessageType.Error,
                    Title = $"Error insert balance history \n" +
               $"{ex.Message}\n" +
               $"Statement: \n" +
               $"{balanceHistory.ToJson()}"
                });
            });
            _logger.LogError($"BalanceHistoryCreateAsync error: {ex}");
            return new MessageResponseBase
            {
                ResponseCode = ResponseCodeConst.Error,
                ResponseMessage = "BalanceHistoryCreateAsync error: " + ex
            };
        }
    }

    public async Task<MessagePagedResponseBase> BalanceHistoriesGetAsync(BalanceHistoriesRequest request)
    {
        try
        {
            Expression<Func<BalanceHistories, bool>> query = p => true;
            if (!string.IsNullOrEmpty(request.TransCode))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.TransCode == request.TransCode;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.TransRef))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.TransRef == request.TransRef;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.TransNote))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.TransNote == request.TransNote;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.SrcAccount))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.SrcAccountCode == request.SrcAccount;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.DesAccount))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.DesAccountCode == request.DesAccount;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.AccountTrans))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p =>
                    p.DesAccountCode == request.AccountTrans || p.SrcAccountCode == request.AccountTrans;
                query = query.And(newQuery);
            }

            if (request.TransType != null && request.TransType != TransactionType.Default)
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.TransType == request.TransType;
                query = query.And(newQuery);
            }

            if (request.FromDate != null)
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p =>
                    p.CreatedDate >= request.FromDate.Value.ToUniversalTime();
                query = query.And(newQuery);
            }

            if (request.ToDate != null)
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p =>
                    p.CreatedDate <= request.ToDate.Value.ToUniversalTime();
                query = query.And(newQuery);
            }

            var total = await _balanceMongoRepository.CountAsync(query);
            // var lst = await _balanceMongoRepository.GetPaginatedAsync<BalanceHistories>(query, request.Offset,
            //     request.Limit);

            var lst = await _balanceMongoRepository.GetSortedPaginatedAsync<BalanceHistories, Guid>(query,
                s => s.CreatedDate, false,
                request.Offset, request.Limit);
            return new MessagePagedResponseBase
            {
                ResponseCode = "01",
                ResponseMessage = "Thành công",
                Total = (int)total,
                Payload = lst.ConvertTo<List<BalanceHistoryDto>>()
            };
        }
        catch (Exception e)
        {
            _logger.LogError("TransactionReportsGetAsync error " + e);
            return new MessagePagedResponseBase
            {
                ResponseCode = ResponseCodeConst.Error
            };
        }
    }


    public async Task<MessageResponseBase> BalanceHistoryGetAsync(BalanceHistoryRequest request)
    {
        var item = await _balanceMongoRepository.GetOneAsync<BalanceHistories>(x =>
            x.TransCode == request.TransCode);

        _logger.LogInformation("BalanceHistoryGetAsync item => " + item.ToJson());
        return new MessageResponseBase
        {
            ResponseCode = "01",
            Payload = item.ConvertTo<BalanceHistoryDto>()
        };
    }

    public async Task<decimal> GetBalanceDateGetAsync(BalanceMaxDateRequest request)
    {
        return await _balanceMongoRepository.GetAccountBalanceMaxDateAsync(request.AccountCode, request.MaxDate);
    }

    private BalanceHistories GetBalanceHistoryInsert(TransactionDto transaction, SettlementDto settlement)
    {
        //Chỗ này k dùng automap vì 1 số tham số đặc biệt
        var item = new BalanceHistories
        {
            TransRef = transaction.TransRef,
            Amount = transaction.Amount,
            CurrencyCode = settlement.CurrencyCode,
            Status = transaction.Status,
            ModifiedDate = transaction.ModifiedDate,
            ModifiedBy = transaction.ModifiedBy,
            CreatedBy = transaction.CreatedBy,
            CreatedDate = transaction.CreatedDate,
            Description = transaction.Description,
            TransNote = transaction.TransNote,
            RevertTransCode = transaction.RevertTransCode,
            TransactionType = transaction.TransType.ToString("G"),
            SrcAccountCode = settlement.SrcAccountCode,
            DesAccountCode = settlement.DesAccountCode,
            SrcAccountBalance = settlement.SrcAccountBalance,
            DesAccountBalance = settlement.DesAccountBalance,
            SrcAccountBalanceBeforeTrans = settlement.SrcAccountBalanceBeforeTrans,
            DesAccountBalanceBeforeTrans = settlement.DesAccountBalanceBeforeTrans,
            TransCode = transaction.TransactionCode,
            TransType = transaction.TransType
        };
        if (item.TransType == TransactionType.MasterTopup)
        {
            item.SrcAccountBalanceBeforeTrans = 0;
            item.SrcAccountBalance = 0;
        }

        return item;
    }

    #region Sys

    public async Task<List<BalanceHistories>> GetListBalanceHistories(BalanceHistoriesRequest request)
    {
        try
        {
            Expression<Func<BalanceHistories, bool>> query = p => true;
            if (!string.IsNullOrEmpty(request.TransCode))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.TransCode == request.TransCode;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.TransRef))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.TransRef == request.TransRef;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.TransNote))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.TransNote == request.TransNote;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.SrcAccount))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.SrcAccountCode == request.SrcAccount;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.DesAccount))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.DesAccountCode == request.DesAccount;
                query = query.And(newQuery);
            }

            if (!string.IsNullOrEmpty(request.AccountTrans))
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p =>
                    p.DesAccountCode == request.AccountTrans || p.SrcAccountCode == request.AccountTrans;
                query = query.And(newQuery);
            }

            if (request.TransType != null && request.TransType != TransactionType.Default)
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p => p.TransType == request.TransType;
                query = query.And(newQuery);
            }

            if (request.FromDate != null)
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p =>
                    p.CreatedDate >= request.FromDate.Value.ToUniversalTime();
                query = query.And(newQuery);
            }

            if (request.ToDate != null)
            {
                Expression<Func<BalanceHistories, bool>> newQuery = p =>
                    p.CreatedDate <= request.ToDate.Value.ToUniversalTime();
                query = query.And(newQuery);
            }


            var lst = await _balanceMongoRepository.GetAllAsync(query);
            return lst;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task UpdateBalanceHistories(string account, DateTime fromDate, DateTime todate,
        decimal balanceBefore, bool isDesAccount = true)
    {
        var lst = await GetListBalanceHistories(new BalanceHistoriesRequest
        {
            AccountTrans = account,
            FromDate = fromDate,
            ToDate = todate
        });
        if (lst.Count > 0)
            foreach (var item in lst.OrderBy(x => x.CreatedDate))
                if (isDesAccount)
                {
                    item.DesAccountBalanceBeforeTrans = balanceBefore;
                    item.DesAccountBalance = item.DesAccountBalanceBeforeTrans + item.Amount;
                    balanceBefore = item.DesAccountBalance; //Gán lại số dư sau giao dịch
                }
                else
                {
                    item.DesAccountBalanceBeforeTrans = balanceBefore;
                    item.DesAccountBalance = item.DesAccountBalanceBeforeTrans - item.Amount;
                    balanceBefore = item.DesAccountBalance; //Gán lại số dư sau giao dịch
                }
    }

    #endregion
}