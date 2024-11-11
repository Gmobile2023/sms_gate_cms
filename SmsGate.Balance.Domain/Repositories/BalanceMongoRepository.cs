using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HLS.Paygate.Balance.Domain.Entities;
using HLS.Paygate.Balance.Models.Dtos;
using HLS.Paygate.Balance.Models.Response;
using HLS.Paygate.Shared;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDbGenericRepository;
using ServiceStack;

namespace HLS.Paygate.Balance.Domain.Repositories;

public class BalanceMongoRepository : BaseMongoRepository, IBalanceMongoRepository
{
    //private Logger _logger = LogManager.GetLogger("BalanceMongoRepository");
    private readonly ILogger<BalanceMongoRepository> _logger;

    public BalanceMongoRepository(IMongoDbContext dbContext, ILogger<BalanceMongoRepository> logger) : base(dbContext)
    {
        _logger = logger;
    }


    public async Task<ResponseMessageBase<object>> BalanceUpdateAsync(TransactionDto transaction,
        SettlementDto settlementDto, string transRef = "")
    {
        //todo log detail
        var transLog = !string.IsNullOrEmpty(transRef) ? transRef : settlementDto.TransRef;

        _logger.LogInformation(
            $"BalanceUpdateAsync : {transLog}|{settlementDto.SrcAccountCode}|{settlementDto.DesAccountCode}|{settlementDto.CurrencyCode}|{settlementDto.Amount}|{settlementDto.TransCode}|{settlementDto.TransRef}|{settlementDto.TransactionType}");

        try
        {
            using var session =
                await MongoDbContext.Client.StartSessionAsync(new ClientSessionOptions {CausalConsistency = true});
            try
            {
                session.StartTransaction(new TransactionOptions(
                    ReadConcern.Majority,
                    writeConcern: WriteConcern.WMajority
                ));

                if (!string.IsNullOrEmpty(settlementDto.SrcAccountCode))
                {
                    _logger.LogInformation(
                        $"BalanceUpdateAsync : {transLog} update src account {settlementDto.SrcAccountCode}");
                    var rs = await UpdateBalance(session, settlementDto.SrcAccountCode, settlementDto.CurrencyCode,
                        -1 * settlementDto.Amount, transLog);
                    _logger.LogInformation(
                        $"BalanceUpdateAsync : {transLog} update src account {settlementDto.SrcAccountCode} => {rs.ToJson()}");
                    if (rs.ResponseStatus.ErrorCode != ResponseCodeConst.Success)
                    {
                        await session.AbortTransactionAsync();
                        return rs;
                    }

                    if (rs.Results is AccountBalance balance)
                    {
                        settlementDto.SrcAccountBalance = balance.Balance;
                        settlementDto.SrcAccountBalanceBeforeTrans = balance.Balance + settlementDto.Amount;
                    }
                }

                if (!string.IsNullOrEmpty(settlementDto.DesAccountCode))
                {
                    _logger.LogInformation(
                        $"BalanceUpdateAsync : {transLog} update des account {settlementDto.DesAccountCode}");
                    var rs = await UpdateBalance(session, settlementDto.DesAccountCode, settlementDto.CurrencyCode,
                        settlementDto.Amount, transLog);
                    _logger.LogInformation(
                        $"BalanceUpdateAsync : {transLog} update src account {settlementDto.DesAccountCode} => {rs.ToJson()}");
                    if (rs.ResponseStatus.ErrorCode != ResponseCodeConst.Success)
                    {
                        await session.AbortTransactionAsync();
                        return rs;
                    }

                    if (rs.Results is AccountBalance balance)
                    {
                        settlementDto.DesAccountBalance = balance.Balance;
                        settlementDto.DesAccountBalanceBeforeTrans = balance.Balance - settlementDto.Amount;
                    }
                }

                //Insert settllment
                _logger.LogInformation($"BalanceUpdateAsync : {transLog}  insert Settlement");
                await MongoDbContext.GetCollection<Settlement>()
                    .InsertOneAsync(session, settlementDto.ConvertTo<Settlement>());
                _logger.LogInformation($"BalanceUpdateAsync : {transLog}  insert Settlement done");

                //var balanceHistory = GetBalanceHistoryInsert(transaction, settlementDto);

                //_logger.LogInformation($"BalanceUpdateAsync : {transLog}  insert BalanceHistories ");
                //await MongoDbContext.GetCollection<BalanceHistories>().InsertOneAsync(session, balanceHistory);
                //_logger.LogInformation($"BalanceUpdateAsync : {transLog}  insert BalanceHistories done ");


                //INsert History

                await session.CommitTransactionAsync();
                return ResponseMessageBase<object>.Success(new BalanceReponse
                {
                    DesBalance = settlementDto.DesAccountBalance,
                    SrcBalance = settlementDto.SrcAccountBalance
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"BalanceUpdateAsync : {transLog}  =>{e.Message}");
                await session.AbortTransactionAsync();
                return ResponseMessageBase<object>.Error();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"BalanceUpdateAsync error: {e}");
            return ResponseMessageBase<object>.Error();
        }
    }

    private async Task<ResponseMessageBase<object>> UpdateBalance(IClientSessionHandle session, string accountCode,
        string currencyCode, decimal transAmount, string transCode)
    {
        try
        {
            _logger.LogInformation($"Update balance : {transCode}| {accountCode} | {currencyCode} | {transAmount}");
            var options = new FindOptions<AccountBalance, AccountBalance>
            {
                Limit = 1
            };
            Expression<Func<AccountBalance, bool>> query = p =>
                p.AccountCode == accountCode && p.CurrencyCode == currencyCode;
            var search = await MongoDbContext.GetCollection<AccountBalance>().FindAsync(session, query);
            var accountBalance = search.FirstOrDefault();
            bool isupdate = true;
            if (accountBalance == null)
            {
                isupdate = false;
                //Create Wallet;
                accountBalance = GetAccountBalance(new AccountBalanceDto
                {
                    AccountCode = accountCode,
                    CurrencyCode = currencyCode,
                });
                accountBalance.CheckSum = accountBalance.ToCheckSum();
            }

            if (!accountBalance.IsValid())
            {
                _logger.LogWarning(
                    $"Update balance : {transCode}| {accountCode} | {currencyCode} | {transAmount} => TK {accountCode} bị thay đổi ngoài hệ thống");
                return ResponseMessageBase<object>.Error($"TK {accountCode} bị thay đổi ngoài hệ thống");
            }

            accountBalance.Balance += transAmount;
            accountBalance.LastTransCode = transCode;
            accountBalance.ModifiedDate = DateTime.Now;
            accountBalance.CheckSum = accountBalance.ToCheckSum();
            if (accountBalance.AvailableBalance < 0)
            {
                _logger.LogWarning(
                    $"Update balance : {transCode}| {accountCode} | {currencyCode} | {transAmount} => không đủ số dư");
                return ResponseMessageBase<object>.Error(ResponseCodeConst.ResponseCode_Balance_Not_Enough,
                    "Số dư không đủ");
            }

            if (isupdate)
            {
                var builder = Builders<AccountBalance>.Filter;
                var filter = builder.Eq(p => p.AccountCode, accountBalance.AccountCode) &
                             builder.Eq(p => p.CurrencyCode, accountBalance.CurrencyCode) &
                             builder.Eq(p => p.Version, accountBalance.Version);

                var queryFilterUpdate = Builders<AccountBalance>.Filter.And(filter);

                accountBalance.UpdateVersion();


                var updateResult = await MongoDbContext.GetCollection<AccountBalance>().UpdateManyAsync(session,
                    queryFilterUpdate,
                    Builders<AccountBalance>.Update.Set(p => p.Balance, accountBalance.Balance)
                        .Set(p => p.LastTransCode, accountBalance.LastTransCode)
                        .Set(p => p.CheckSum, accountBalance.CheckSum)
                        .Set(p => p.ModifiedDate, accountBalance.ModifiedDate)
                        .Set(p => p.Version, accountBalance.Version),
                    new UpdateOptions {IsUpsert = false});
                _logger.LogInformation(
                    $"Update balance : {transCode}| {accountCode} | {currencyCode} | {transAmount} => ModifiedCount :  {updateResult.ModifiedCount}");

                if (updateResult.ModifiedCount != 1)
                {
                    return ResponseMessageBase<object>.Error();
                }

                _logger.LogInformation(
                    $"Update balance : {transCode}| {accountCode} | {currencyCode} | {transAmount} => Success");
                return ResponseMessageBase<object>.Success(accountBalance);
            }

            _logger.LogInformation(
                $"Update balance : {transCode}| {accountCode} | {currencyCode} wallet not exist  create wallet");

            await MongoDbContext.GetCollection<AccountBalance>().InsertOneAsync(session, accountBalance);

            _logger.LogInformation(
                $"Update balance : {transCode}| {accountCode} | {currencyCode} Create wallet success");

            return ResponseMessageBase<object>.Success(accountBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Update balance : {transCode}: ==> {ex.Message}");
        }

        return ResponseMessageBase<object>.Error();
    }

    private AccountBalance GetAccountBalance(AccountBalanceDto accountBalanceDto)
    {
        var accountBalance = accountBalanceDto.ConvertTo<AccountBalance>();
        if (accountBalance.AccountCode is BalanceConst.MASTER_ACCOUNT or BalanceConst.CASHOUT_ACCOUNT
            or BalanceConst.CONTROL_ACCOUNT or BalanceConst.PAYMENT_ACCOUNT
           )
            accountBalance.AccountType = BalanceAccountTypeConst.SYSTEM;
        else if (accountBalance.AccountCode.StartsWith(BalanceAccountTypeConst.TEMP))
        {
            accountBalance.AccountType = BalanceAccountTypeConst.TEMP;
        }
        else
            accountBalance.AccountType = BalanceAccountTypeConst.CUSTOMER;

        accountBalance.CheckSum = accountBalance.ToCheckSum();

        return accountBalance;
    }

    public async Task<ResponseMessageBase<object>> UpdateBlockMoney(string accountCode, string currencyCode,
        decimal amount)
    {
        _logger.LogInformation($"UpdateBlockMoney :  {accountCode} | {currencyCode} | {amount}");


        try
        {
            using var session =
                await MongoDbContext.Client.StartSessionAsync(new ClientSessionOptions {CausalConsistency = true});
            try
            {
                session.StartTransaction(new TransactionOptions(
                    ReadConcern.Majority,
                    writeConcern: WriteConcern.WMajority
                ));


                var options = new FindOptions<AccountBalance, AccountBalance>
                {
                    Limit = 1
                };
                Expression<Func<AccountBalance, bool>> query = p =>
                    p.AccountCode == accountCode && p.CurrencyCode == currencyCode;

                var search = await MongoDbContext.GetCollection<AccountBalance>().FindAsync(session, query);
                var accountBalance = search.FirstOrDefault();

                if (accountBalance == null)
                {
                    _logger.LogInformation(
                        $"UpdateBlockMoney :  {accountCode} | {currencyCode} | {amount} => wallet not exist");
                    return ResponseMessageBase<object>.Error();
                }

                if (!accountBalance.IsValid())
                {
                    _logger.LogInformation(
                        $"UpdateBlockMoney :  {accountCode} | {currencyCode} | {amount} => wallet is modified out of system");
                    return ResponseMessageBase<object>.Error($"TK {accountCode} bị thay đổi ngoài hệ thống");
                }


                accountBalance.BlockedMoney += amount;
                accountBalance.ModifiedDate = DateTime.Now;

                accountBalance.CheckSum = accountBalance.ToCheckSum();

                if (accountBalance.BlockedMoney < 0)
                {
                    _logger.LogInformation(
                        $"UpdateBlockMoney :  {accountCode} | {currencyCode} | {amount} => BlockedMoney is not enough");
                    return ResponseMessageBase<object>.Error(ResponseCodeConst.ResponseCode_Balance_Not_Enough,
                        "Số dư không đủ");
                }


                var builder = Builders<AccountBalance>.Filter;
                var filter = builder.Eq(p => p.AccountCode, accountBalance.AccountCode) &
                             builder.Eq(p => p.CurrencyCode, accountBalance.CurrencyCode) &
                             builder.Eq(p => p.Version, accountBalance.Version);

                var queryFilterUpdate = Builders<AccountBalance>.Filter.And(filter);


                accountBalance.UpdateVersion();


                var updateResult = await MongoDbContext.GetCollection<AccountBalance>().UpdateManyAsync(session,
                    queryFilterUpdate,
                    Builders<AccountBalance>.Update.Set(p => p.BlockedMoney, accountBalance.BlockedMoney)
                        .Set(p => p.CheckSum, accountBalance.CheckSum)
                        .Set(p => p.ModifiedDate, accountBalance.ModifiedDate)
                        .Set(p => p.Version, accountBalance.Version),
                    new UpdateOptions {IsUpsert = false});

                _logger.LogInformation(
                    $"UpdateBlockMoney :  {accountCode} | {currencyCode} | {amount}  update => {updateResult.ModifiedCount}");

                if (updateResult.ModifiedCount != 1)
                {
                    //lôi:
                    return ResponseMessageBase<object>.Error();
                }

                await session.CommitTransactionAsync();
                _logger.LogInformation(
                    $"UpdateBlockMoney :  {accountCode} | {currencyCode} | {amount}  => Success");
                return ResponseMessageBase<object>.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"UpdateBlockMoney :  {accountCode} | {currencyCode} | {amount}  => {e.Message}");
                await session.AbortTransactionAsync();
                return ResponseMessageBase<object>.Error();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"UpdateBlockMoney error: {e}");
            return ResponseMessageBase<object>.Error();
        }
    }
}