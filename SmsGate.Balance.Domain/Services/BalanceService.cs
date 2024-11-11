using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmsGate.Balance.Models;
using SmsGate.Balance.Models.Dtos;
using SmsGate.Balance.Models.Enums;
using SmsGate.Balance.Models.Exceptions;
using SmsGate.Balance.Models.Grains;
using SmsGate.Balance.Models.Requests;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orleans;
using ServiceStack;
using SmsGate.Balance.Domain.Activities;
using SmsGate.Balance.Domain.Entities;
using SmsGate.Balance.Domain.Repositories;
using SmsGate.Shared.Common;
using SmsGate.Shared.Contract.Grpc.Balance;

namespace SmsGate.Balance.Domain.Services;

public class BalanceService(
    ITransactionService transactionService,
    IBus busReport,
    ILogger<BalanceService> logger,
    IBalanceRepository balanceRepository,
    IGrainFactory grainFactory,
    ITransactionRepository transactionRepository)
    : IBalanceService
{
    public async Task<AccountBalanceDto> AccountBalanceGetAsync(string accountCode, string currencyCode,
        BalanceStatus balanceStatus = BalanceStatus.Active)
    {
        var grain = grainFactory.GetGrain<IBalanceGrain>(accountCode + "|" + currencyCode);
        var account = await grain.GetBalanceAccount();
        return account;
    }

    public async Task<AccountBalanceDto> AccountBalanceCreateAsync(AccountBalanceDto accountBalance)
    {
        try
        {
            var code = accountBalance.AccountCode.Split('*')[0];
            if (code is BalanceConst.MASTER_ACCOUNT or BalanceConst.CASHOUT_ACCOUNT
                or BalanceConst.CONTROL_ACCOUNT or BalanceConst.PAYMENT_ACCOUNT or BalanceConst.COMMISSION_ACCOUNT
               ) //Chỗ này chia tạm theo loại tk
                accountBalance.AccountType = BalanceAccountTypeConst.SYSTEM;
            else if (code.StartsWith(BalanceAccountTypeConst.TEMP)
                    ) //Chỗ này chia tạm theo loại tk
                accountBalance.AccountType = BalanceAccountTypeConst.TEMP;
            else
                accountBalance.AccountType = BalanceAccountTypeConst.CUSTOMER;

            accountBalance.CheckSum = accountBalance.ToCheckSum();
            var accountBalanceTemp = await balanceRepository.AccountBalanceCreateAsync(accountBalance.ConvertTo<AccountBalance>());
            
            return accountBalanceTemp?.ConvertTo<AccountBalanceDto>();
        }
        catch (Exception ex)
        {
            logger.LogError("Create account balance error: " + ex.Message);
        }

        return null;
    }

    public async Task<bool> AccountBalanceUpdateAsync(AccountBalanceDto accountBalanceDto)
    {
        logger.LogInformation(
            $"AccountBalanceUpdateAsync request: {accountBalanceDto.AccountCode}-{accountBalanceDto.Balance}");
        // var accountBalance = accountBalanceDto.ConvertTo<AccountBalance>();

        var accountBalance = await
            balanceRepository.AccountBalanceSelectOneAsync(accountBalanceDto.AccountCode,
                accountBalanceDto.CurrencyCode);

        if (accountBalance == null)
            throw new BalanceException(31, $"Account {accountBalanceDto.AccountCode} has been null");

        if (!accountBalance.IsValid())
            throw new BalanceException(31,
                $"Account {accountBalance.AccountCode} currency {accountBalanceDto.CurrencyCode} has been modified outside the system");

        if (accountBalanceDto.CheckSum != accountBalance.CheckSum)
            throw new BalanceException(31, $"Account {accountBalance.AccountCode} has not valid checksum");

        accountBalanceDto.CheckSum = accountBalanceDto.ToCheckSum();
        try
        {
            var result = await balanceRepository.AccountBalanceUpdateAsync(accountBalanceDto.ConvertTo<AccountBalance>());
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError($"Update balance for {accountBalanceDto.AccountCode} Error: " + ex.Message);
            return false;
        }
    }

    public async Task<ResponseMessageBase<object>> CheckCurrencyAsync(string currencyCode)
    {
        var checkCurrencyExist =
            await balanceRepository.CurrencyGetAsync(currencyCode);
        return checkCurrencyExist == null
            ? ResponseMessageBase<object>.Error($"CurrencyCode {currencyCode} does not exist")
            : ResponseMessageBase<object>.Success();
    }

    public async Task CurrencyCreateAsync(string currencyCode)
    {
        await balanceRepository.CurrencyCreateAsync(new CurrencyDto
        {
            CurrencyCode = currencyCode,
            ModifiedDate = DateTime.Now
        });
    }

    public async Task<ResponseMessageBase<object>> TransferAsync(BalanceTransferRequest transferRequest)
    {
        try
        {
            logger.LogInformation("Received TransferRequest: " + transferRequest.TransRef);
            var responseMessage = await CheckCurrencyAsync(transferRequest.CurrencyCode);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (transferRequest.Amount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            if (transferRequest.FeeType == FeeType.OutsideFee)
            {
                var checkDes = await GetBalance(transferRequest.DesAccount, transferRequest.CurrencyCode);
                if (checkDes.ResponseStatus.ErrorCode != "00")
                {
                    responseMessage.ResponseStatus.Message = "Giao dịch không thành công";
                    responseMessage.ResponseStatus.ErrorCode = "01";
                    return responseMessage;
                }

                if (transferRequest.Amount + checkDes.Results - transferRequest.Fee < 0)
                {
                    responseMessage.ResponseStatus.Message =
                        "Giao dịch không thành công. Vui lòng kiểm tra lại số tiền nhận và phí chuyển tiền";
                    responseMessage.ResponseStatus.ErrorCode = ResponseCodeConst.BalanceNotEnough;
                    return responseMessage;
                }
            }
            else
            {
                var checkBalance = await CheckBalance(transferRequest.SrcAccount, transferRequest.CurrencyCode,
                    transferRequest.Amount + transferRequest.Fee);
                if (checkBalance.ResponseStatus.ErrorCode != "00")
                    return checkBalance;
            }

            responseMessage = await transactionService.TransferAsync(transferRequest);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transaction = (Transaction)responseMessage.Results;

            //var settlement = transaction.Settlements[0];

            responseMessage = await TransferMoneyAsync(transaction.Settlements);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;

            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception ex)
        {
            logger.LogError($"{transferRequest.ToJson()}-TransferRequest error: {ex.Message}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> CurrencyExchangeAsync(CurrencyExchangeRequest exchangeRequest)
    {
        try
        {
            logger.LogInformation("Received CurrencyExchangeRequest: " + exchangeRequest.TransRef);
            var responseMessage = await CheckCurrencyAsync(exchangeRequest.SrcCurrencyCode);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            responseMessage = await CheckCurrencyAsync(exchangeRequest.DesCurrencyCode);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (exchangeRequest.SrcAmount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");
            if (exchangeRequest.DesAmount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            var checkBalance = await CheckBalance(exchangeRequest.AccountCode, exchangeRequest.SrcCurrencyCode,
                exchangeRequest.SrcAmount);
            if (checkBalance.ResponseStatus.ErrorCode != "00")
                return checkBalance;

            responseMessage = await transactionService.ExchangeAsync(exchangeRequest);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transactions = (List<Transaction>)responseMessage.Results;

            var settlements = new List<Settlement>();

            settlements.AddRange(transactions[0].Settlements);
            settlements.AddRange(transactions[1].Settlements);

            responseMessage = await TransferMoneyAsync(settlements);

            logger.LogInformation($"Deposit return: {responseMessage.ResponseStatus.ToJson()}");
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                foreach (var transaction in transactions)
                    await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);

                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;

            await PublishBalanceHistoryMessage(transactions[0].ConvertTo<TransactionDto>(), balanceResponse);
            await PublishBalanceHistoryMessage(transactions[1].ConvertTo<TransactionDto>(), balanceResponse);

            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception ex)
        {
            logger.LogError($"{exchangeRequest.ToJson()} - ExchangeRequest error: {ex}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> DepositAsync(BalanceDepositRequest depositRequest)
    {
        try
        {
            logger.LogInformation("Received DepositRequest: " + depositRequest.TransRef);
            var responseMessage = await CheckCurrencyAsync(depositRequest.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (depositRequest.Amount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            var checkBalance = await CheckBalance(BalanceConst.MASTER_ACCOUNT, depositRequest.CurrencyCode,
                depositRequest.Amount + depositRequest.Fee);
            if (checkBalance.ResponseStatus.ErrorCode != "00")
                return checkBalance;

            responseMessage = await transactionService.DepositAsync(depositRequest);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transaction = (Transaction)responseMessage.Results;
            responseMessage = await TransferMoneyAsync(transaction.Settlements);

            logger.LogInformation($"Deposit return: {responseMessage.ResponseStatus.ToJson()}");
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;
            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse,
                depositRequest.ExtraInfo);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception ex)
        {
            logger.LogError($"{depositRequest.ToJson()}-DepositRequest error: {ex}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> CashOutAsync(CashOutRequest cashOutRequest)
    {
        try
        {
            logger.LogInformation("Received CashOutRequest: " + cashOutRequest.ToJson());
            var responseMessage = await CheckCurrencyAsync(cashOutRequest.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (cashOutRequest.Amount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            var checkBalance = await CheckBalance(cashOutRequest.AccountCode, cashOutRequest.CurrencyCode,
                cashOutRequest.Amount);
            if (checkBalance.ResponseStatus.ErrorCode != "00")
                return checkBalance;

            responseMessage = await transactionService.CashOutAsync(cashOutRequest);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transaction = (Transaction)responseMessage.Results;


            responseMessage = await TransferMoneyAsync(transaction.Settlements);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;

            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception ex)
        {
            logger.LogError($"{cashOutRequest.ToJson()}-CashOutRequest error: {ex}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> MasterTopupAsync(MasterTopupRequest masterTopupRequest)
    {
        logger.LogInformation("Received MasterTopupRequest: " + masterTopupRequest.ToJson());
        var responseMessage = await CheckCurrencyAsync(masterTopupRequest.CurrencyCode);

        if (responseMessage.ResponseStatus.ErrorCode != "00")
            return responseMessage;

        if (masterTopupRequest.Amount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

        responseMessage = await transactionService.MasterTopupAsync(masterTopupRequest);
        if (responseMessage.ResponseStatus.ErrorCode != "00")
            return responseMessage;

        var transaction = (Transaction)responseMessage.Results;
        var response = await TransferMoneyAsync(transaction.Settlements);

        if (response.ResponseStatus.ErrorCode != "00")
        {
            await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
            return response;
        }

        var balanceResponse = (BalanceResponse)response.Results;
        await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse);
        return ResponseMessageBase<object>.Success(balanceResponse);
    }

    public async Task<ResponseMessageBase<object>> PaymentAsync(BalancePaymentRequest paymentRequest)
    {
        try
        {
            logger.LogInformation(
                $"BalancePaymentRequest:{paymentRequest.TransCode}-{paymentRequest.TransRef}-{paymentRequest.AccountCode}");
            var responseMessage = await CheckCurrencyAsync(paymentRequest.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (paymentRequest.PaymentAmount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            var checkBalance = await CheckBalance(paymentRequest.AccountCode, paymentRequest.CurrencyCode,
                paymentRequest.PaymentAmount + paymentRequest.Fee);
            if (checkBalance.ResponseStatus.ErrorCode != "00")
                return checkBalance;

            responseMessage = await transactionService.PaymentAsync(paymentRequest);
            logger.LogInformation(
                $"PaymentAsync:{responseMessage.ResponseStatus.ErrorCode}-{responseMessage.ResponseStatus.Message}-{paymentRequest.TransCode}-{paymentRequest.TransRef}");
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;
            var transaction = (Transaction)responseMessage.Results;


            responseMessage = await TransferMoneyAsync(transaction.Settlements);
            logger.LogInformation(
                $"TransferMoneyAsync:{responseMessage.ResponseStatus.ErrorCode}-{responseMessage.ResponseStatus.Message}-{paymentRequest.TransCode}-{paymentRequest.TransRef}");
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;
            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception e)
        {
            logger.LogError($"{paymentRequest.ToJson()}-PaymentRequest error: {e}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> RevertAsync(BalanceRevertRequest revertRequest)
    {
        try
        {
            var transactionRevert =
                await transactionRepository.TransactionSelectOneAsync(revertRequest.TransactionCode);
            if (transactionRevert == null)
                return ResponseMessageBase<object>.Error("Giao dịch không tồn tại");
            if (transactionRevert.Status != TransStatus.Done)
                return ResponseMessageBase<object>.Error("Giao dịch có trạng thái không thể revert");

            if (revertRequest.RevertAmount <= 0)
                return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            transactionRevert.Description = revertRequest.TransNote;
            var responseMessage = await transactionService.RevertAsync(transactionRevert);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transaction = (Transaction)responseMessage.Results;
            responseMessage = await TransferMoneyAsync(transaction.Settlements);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;
            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception e)
        {
            logger.LogError($"{revertRequest.ToJson()}-RevertRequest error: {e}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> CancelPaymentAsync(BalanceCancelPaymentRequest request)
    {
        try
        {
            //_logger.LogInformation("Received CancelPaymentAsync: " + request.ToJson());
            var responseMessage = await CheckCurrencyAsync(request.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (request.RevertAmount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            responseMessage = await transactionService.CancelPaymentAsync(request);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transaction = (Transaction)responseMessage.Results;
            responseMessage = await TransferMoneyAsync(transaction.Settlements);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                if (responseMessage.ResponseStatus.ErrorCode == ResponseCodeConst.BalanceNotEnough &&
                    transaction.SrcAccountCode.StartsWith(BalanceAccountTypeConst.TEMP))
                {
                    //Tạo 2 GD điều chuyển nội bộ
                }

                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }


            var balanceResponse = (BalanceResponse)responseMessage.Results;
            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception e)
        {
            logger.LogError($"{request.ToJson()}-CancelPaymentAsync error: {e}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<decimal> AccountBalanceCheckAsync(
        AccountBalanceCheckRequest accountBalanceCheckRequest)
    {
        //_logger.LogInformation("Received AccountBalanceCheckRequest: " + accountBalanceCheckRequest.ToJson());
        var responseMessage = await CheckCurrencyAsync(accountBalanceCheckRequest.CurrencyCode);

        if (responseMessage.ResponseStatus.ErrorCode != "00")
            return 0;

        return await grainFactory
            .GetGrain<IBalanceGrain>(accountBalanceCheckRequest.AccountCode + "|" +
                                     accountBalanceCheckRequest.CurrencyCode).GetBalance();
    }

    public async Task TransferShardAccountToMain(string accountCode, CurrencyCode currencyCode)
    {
        var accountGrain =
            grainFactory.GetGrain<IBalanceGrain>(accountCode + "|" + currencyCode.ToString("G"));

        var paymentAccount = await accountGrain.GetBalanceAccount();

        if (paymentAccount.ShardCounter > 0)
            for (var i = 1; i <= paymentAccount.ShardCounter; i++)
            {
                var shardAccountGrain = grainFactory.GetGrain<IBalanceGrain>(accountCode + "*" + i +
                                                                             "|" + currencyCode.ToString("G"));
                var balance = await shardAccountGrain.GetBalance();
                logger.LogInformation("AUTO_SYSTEM_TRANSFER {acc} balance {bal}",
                    accountCode + "*" + i, balance);
                if (await shardAccountGrain.GetBalance() > 0)
                {
                    var transResult = await TransferSystemAsync(new TransferSystemRequest
                    {
                        DesAccount = accountCode,
                        SrcAccount = accountCode + "*" + i,
                        CurrencyCode = currencyCode.ToString("G"),
                        TransNote = "Auto trans " + DateTime.Now.ToString("yyMMddHHmmss"),
                        TransRef = "Autotrans",
                        Amount = balance
                    });

                    logger.LogInformation("AUTO_SYSTEM_TRANSFER {acc} result {result}",
                        accountCode + "*" + i, transResult.ToJson());
                }
            }
    }

    public async Task<ResponseMessageBase<object>> ChargeFeeAsync(BalanceChargeFeeRequest request)
    {
        try
        {
            logger.LogInformation("BalanceChargeFeeRequest: " + request.TransRef);
            var responseMessage = await CheckCurrencyAsync(request.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (request.Fee <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            var checkBalance = await CheckBalance(request.SrcAccount, request.CurrencyCode,
                request.Fee);
            if (checkBalance.ResponseStatus.ErrorCode != "00")
                return checkBalance;

            responseMessage = await transactionService.ChargeFeeAsync(request);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transaction = (Transaction)responseMessage.Results;
            responseMessage = await TransferMoneyAsync(transaction.Settlements);

            logger.LogInformation($"BalanceChargeFee return: {responseMessage.ResponseStatus.ToJson()}");
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;
            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse,
                request.ExtraInfo);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception ex)
        {
            logger.LogError($"{request.ToJson()}-BalanceChargeFeeRequest error: {ex}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> AdjustmentAsync(AdjustmentRequest request)
    {
        try
        {
            logger.LogInformation("Received AdjustmentAsync: " + request.ToJson());
            var responseMessage = await CheckCurrencyAsync(request.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (request.Amount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            var accountCheck = request.AdjustmentType == AdjustmentType.Decrease
                ? request.AccountCode
                : BalanceConst.MASTER_ACCOUNT;

            var checkBalance = await CheckBalance(accountCheck, request.CurrencyCode,
                request.Amount);
            if (checkBalance.ResponseStatus.ErrorCode != "00")
                return checkBalance;


            responseMessage = await transactionService.AdjustmentAsync(request);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transaction = (Transaction)responseMessage.Results;


            responseMessage = await TransferMoneyAsync(transaction.Settlements);
            logger.LogInformation($"Adjustment return: {responseMessage.ToJson()}");
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;

            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception ex)
        {
            logger.LogError($"{request.ToJson()}-AdjustmentAsync error: {ex}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> BlockBalanceAsync(BlockBalanceRequest request)
    {
        try
        {
            logger.LogInformation("Received BlockBalanceAsync: " + request.ToJson());
            var responseMessage = await CheckCurrencyAsync(request.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;
            if (!request.IsBlockAllBalance && request.BlockAmount <= 0)
                return ResponseMessageBase<object>.Error("Số tiền phong tỏa không hợp lệ");

            var checkBalance = await GetBalance(request.AccountCode, request.CurrencyCode);
            if (checkBalance.ResponseStatus.ErrorCode != "00")
                return ResponseMessageBase<object>.Error("Giao dịch không thành công");

            if (request.IsBlockAllBalance) request.BlockAmount = checkBalance.Results;

            if (request.BlockAmount > checkBalance.Results)
                return ResponseMessageBase<object>.Error("Số dư không đủ để phong tỏa");
            //Chỗ này k tạo transaction
            // responseMessage = await _transactionService.BlockBalanceAsync(request);
            // if (responseMessage.ResponseStatus.ErrorCode != "00")
            //     return responseMessage;

            //var transaction = (Transaction) responseMessage.Results;

            //var settlement = transaction.Settlements[0];
            var grain = grainFactory.GetGrain<IBalanceGrain>(request.AccountCode + "|" + request.CurrencyCode);
            var isBlock = await grain.BlockBalance(request.BlockAmount, request.TransRef);
            if (!isBlock)
            {
                responseMessage.ResponseStatus.Message = "Phong tỏa số dư không thành công";
                responseMessage.ResponseStatus.ErrorCode = "01";
                responseMessage.Results = null;
                return responseMessage;
            }


            //var balanceInfo = await AccountBalanceStateInfo(request.AccountCode, request.CurrencyCode);
            return ResponseMessageBase<object>.Success();
        }
        catch (Exception ex)
        {
            logger.LogError($"{request.ToJson()}-BlockBalance error: {ex}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> UnBlockBalanceAsync(UnBlockBalanceRequest request)
    {
        try
        {
            logger.LogInformation("Received UnBlockBalanceAsync: " + request.ToJson());
            var responseMessage = await CheckCurrencyAsync(request.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (!request.IsUnBlockAllBalance && request.UnBlockAmount <= 0)
            {
                responseMessage.ResponseStatus.ErrorCode = "01";
                responseMessage.ResponseStatus.Message = "Số tiền giải phong tỏa không hợp lệ";
                return responseMessage;
            }

            var getBlockedAmount = await GetBlockMoney(request.AccountCode, request.CurrencyCode);
            if (request.IsUnBlockAllBalance) request.UnBlockAmount = getBlockedAmount;

            if (getBlockedAmount - request.UnBlockAmount < 0)
            {
                responseMessage.ResponseStatus.ErrorCode = "01";
                responseMessage.ResponseStatus.Message =
                    "Không thành công. Số tiền giải phong tỏa lớn hơn số tiền phong tỏa hiện tại";
                return responseMessage;
            }

            var grain = grainFactory.GetGrain<IBalanceGrain>(request.AccountCode + "|" + request.CurrencyCode);
            var isUnBlock = await grain.UnBlockBalance(request.UnBlockAmount, request.TransRef);
            if (!isUnBlock)
            {
                responseMessage.ResponseStatus.Message = "Giải phong tỏa số dư không thành công";
                responseMessage.Results = null;
                responseMessage.ResponseStatus.ErrorCode = "01";
                return responseMessage;
            }

            //var balanceInfo = await AccountBalanceGetAsync(request.AccountCode, request.CurrencyCode);
            return ResponseMessageBase<object>.Success();
        }
        catch (Exception ex)
        {
            logger.LogError($"{request.ToJson()}-UnBlockBalance error: {ex}");
            return ResponseMessageBase<object>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> TransferSystemAsync(TransferSystemRequest transferRequest)
    {
        try
        {
            logger.LogInformation("Received TransferSystemRequest: " + transferRequest.ToJson());
            if (!CheckAccountSystem(transferRequest.SrcAccount) || !CheckAccountSystem(transferRequest.DesAccount))
                return ResponseMessageBase<object>.Error();

            var responseMessage = await CheckCurrencyAsync(transferRequest.CurrencyCode);

            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            if (transferRequest.Amount <= 0) return ResponseMessageBase<object>.Error("Số tiền không hợp lệ");

            var checkBalance = await CheckBalance(transferRequest.SrcAccount, transferRequest.CurrencyCode,
                transferRequest.Amount);
            if (checkBalance.ResponseStatus.ErrorCode != "00")
                return checkBalance;

            responseMessage = await transactionService.TransferSystemAsync(transferRequest);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
                return responseMessage;

            var transaction = (Transaction)responseMessage.Results;


            responseMessage = await TransferMoneyAsync(transaction.Settlements);
            if (responseMessage.ResponseStatus.ErrorCode != "00")
            {
                await transactionService.UpdateTransactionStatus(transaction.TransactionCode, TransStatus.Error);
                return responseMessage;
            }

            var balanceResponse = (BalanceResponse)responseMessage.Results;
            await PublishBalanceHistoryMessage(transaction.ConvertTo<TransactionDto>(), balanceResponse);
            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception ex)
        {
            logger.LogError($"{transferRequest.ToJson()}-TransferSystemAsync error: {ex}");
            return ResponseMessageBase<object>.Error();
        }
    }
    public bool CheckAccountSystem(string accountCode)
    {
        return accountCode.Split('*')[0] is BalanceConst.MASTER_ACCOUNT or BalanceConst.CASHOUT_ACCOUNT
            or BalanceConst.CONTROL_ACCOUNT or BalanceConst.PAYMENT_ACCOUNT
            or BalanceConst.FEE_ACCOUNT or BalanceConst.HUB
            or BalanceConst.CHARGING or BalanceConst.COMMISSION_ACCOUNT;
    }


    public async Task<ResponseMessageBase<string>> GetSettlementListHistory(BalanceHistoriesRequest request)
    {
        try
        {
            var list = await transactionService.GetSettlementListHistory(request);
            return list;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ResponseMessageBase<string>.Error();
        }
    }

    private async Task<ResponseMessageBase<object>> TransferMoneyAsync(List<Settlement> settlements)
    {
        var paymentTransCode = settlements[0].PaymentTransCode;
        try
        {
            var balanceResponse = new BalanceResponse
            {
                SrcBalance = -1,
                DesBalance = -1,
                TransactionCode = settlements[0].TransRef //Mã gd của Transaction
            };
            var sagaBuilder = grainFactory.CreateSaga();
            foreach (var settlement in settlements.ConvertTo<List<SettlementDto>>())
            {
                if (settlement.SrcAccountCode == settlement.DesAccountCode)
                    return ResponseMessageBase<object>.Error(
                        "Source account and Destination account must not the same");

                if (!string.IsNullOrEmpty(settlement.SrcAccountCode) &&
                    !string.IsNullOrEmpty(settlement.DesAccountCode))
                    sagaBuilder = sagaBuilder
                        .AddActivity<BalanceWithdrawActivity>
                        (
                            x => { x.Add(BalanceWithdrawActivity.SETTLEMENT, settlement); }
                        )
                        .AddActivity<BalanceDepositActivity>
                        (
                            x => { x.Add(BalanceWithdrawActivity.SETTLEMENT, settlement); }
                        );
                else if (!string.IsNullOrEmpty(settlement.SrcAccountCode))
                    sagaBuilder = sagaBuilder
                        .AddActivity<BalanceWithdrawActivity>
                        (
                            x => { x.Add(BalanceWithdrawActivity.SETTLEMENT, settlement); }
                        )
                        .AddActivity<NoneAccountActivity>
                        (
                            x => { x.Add("HasResult", settlement.ReturnResult); }
                        );
                else if (!string.IsNullOrEmpty(settlement.DesAccountCode))
                    sagaBuilder = sagaBuilder
                        .AddActivity<NoneAccountActivity>
                        (
                            x => { x.Add("HasResult", settlement.ReturnResult); }
                        )
                        .AddActivity<BalanceDepositActivity>
                        (
                            x => { x.Add(BalanceWithdrawActivity.SETTLEMENT, settlement); }
                        );
            }

            if (sagaBuilder != null)
            {
                var saga = await sagaBuilder.ExecuteSagaAsync();
                var result = await saga.WaitForTransferResult(settlements.ConvertTo<List<SettlementDto>>(), 100);
                if (result.Count > 0 && result.Count == settlements.Count)
                {
                    await transactionService.SettlementsInsertAsync(result.ConvertTo<List<Settlement>>());
                    if (result.Exists(p => p.Status != SettlementStatus.Done))
                    {
                        var error = await saga.GetSagaError();
                        logger.LogError($"{paymentTransCode} - Transfer error: {error}");
                        // await saga.Dispose();

                        return new ResponseMessageBase<object>("01", error);
                    }

                    balanceResponse.SrcBalance = result[0].SrcAccountBalance;
                    balanceResponse.DesBalance = result[^1].DesAccountBalance;
                    balanceResponse.BalanceAfterTrans = (from x in result
                        select new BalanceAfterTransDto
                        {
                            SrcAccount = x.SrcAccountCode,
                            SrcBeforeBalance = x.SrcAccountBalanceBeforeTrans,
                            SrcBalance = x.SrcAccountBalance,
                            DesAccount = x.DesAccountCode,
                            DesBeforeBalance = x.DesAccountBalanceBeforeTrans,
                            DesBalance = x.DesAccountBalance,
                            Amount = x.Amount,
                            CurrencyCode = x.CurrencyCode,
                            TransCode = x.TransCode
                        }).ToList();

                    await saga.Dispose();
                }
                else
                {
                    var error = await saga.GetSagaError();
                    logger.LogError($"{paymentTransCode} - Transfer error with result: {error}");
                    await saga.Dispose();
                    return new ResponseMessageBase<object>("01", error);
                }
            }

            return ResponseMessageBase<object>.Success(balanceResponse);
        }
        catch (Exception e)
        {
            logger.LogError($"{paymentTransCode}-TransferMoneyAsync error: {e}");
            return ResponseMessageBase<object>.Error();
        }
    }

    // private async Task<bool> BlockMoneyAsync(string accountCode, decimal amount, string currencyCode)
    // {
    //     try
    //     {
    //         var accountBalance = await _balanceRepository.AccountBalanceSelectOneAsync(accountCode, currencyCode);
    //
    //         if (accountBalance == null)
    //             return false;
    //
    //         accountBalance.BlockedMoney += amount;
    //         accountBalance.CheckSum = accountBalance.ToCheckSum();
    //
    //         var response = await _balanceRepository.AccountBlockUpdateOnlyAsync(accountBalance);
    //
    //         _logger.LogInformation($"BlockMoneyAsync return:{response}");
    //         return response;
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.LogError("Error BlockMoneyAsync: " + e);
    //         return false;
    //     }
    // }
    //
    // private async Task<bool> UnBlockMoneyAsync(string accountCode, decimal amount, string currencyCode)
    // {
    //     try
    //     {
    //         var accountBalance = await _balanceRepository.AccountBalanceSelectOneAsync(accountCode, currencyCode);
    //
    //         if (accountBalance == null)
    //             return false;
    //
    //         accountBalance.BlockedMoney -= amount;
    //         accountBalance.CheckSum = accountBalance.ToCheckSum();
    //
    //         var response = await _balanceRepository.AccountBlockUpdateOnlyAsync(accountBalance);
    //
    //         _logger.LogInformation($"BlockMoneyAsync return:{response}");
    //         return response;
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.LogError("Error UnBlockMoneyAsync: " + e);
    //         return false;
    //     }
    // }

    private async Task<decimal> GetBlockMoney(string accountCode, string currencyCode)
    {
        try
        {
            //var srcBalance = await AccountBalanceGetAsync(accountCode, currencyCode);
            //return srcBalance?.BlockedMoney ?? 0;
            var grain = grainFactory.GetGrain<IBalanceGrain>(accountCode + "|" + currencyCode);
            return await grain.GetBlockMoney();
        }
        catch (Exception e)
        {
            return 0;
        }
    }

    private async Task<ResponseMessageBase<object>> CheckBalance(string srcAccount, string currencyCode,
        decimal requestAmount)
    {
        try
        {
            var srcBalance = await grainFactory.GetGrain<IBalanceGrain>(srcAccount + "|" + currencyCode).GetBalance();


            if (srcBalance - requestAmount < 0)
                return ResponseMessageBase<object>.Error(ResponseCodeConst.BalanceNotEnough,
                    "Số dư tài khoản không đủ");
            return ResponseMessageBase<object>.Success();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError($"CheckBalance error:{e}");
            return ResponseMessageBase<object>.Error();
        }
    }

    private async Task<ResponseMessageBase<decimal>> GetBalance(string srcAccount, string currencyCode)
    {
        try
        {
            var srcBalance = await grainFactory.GetGrain<IBalanceGrain>(srcAccount + "|" + currencyCode).GetBalance();
            return ResponseMessageBase<decimal>.Success(srcBalance);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError($"CheckBalance error:{e}");
            return ResponseMessageBase<decimal>.Error();
        }
    }

    private async Task PublishBalanceHistoryMessage(TransactionDto request, BalanceResponse response,
        string extraInfo = "", string dataInfo = "")
    {
        // try
        // {
        //     foreach (var data in request.Settlements.OrderBy(c => c.Order))
        //     {
        //         logger.LogInformation(
        //             $"PublishBalanceHistory => TransCode={request.TransactionCode}|Amount= {data.Amount}|SrcAccountCode= {data.SrcAccountCode}|DesAccountCode= {data.DesAccountCode}|TransType= {request.TransType.ToString()}");
        //         var transItemDto = request.ConvertTo<TransactionReportDto>();
        //         transItemDto.CreatedDate = data.CreatedDate;
        //         transItemDto.TransCode = data.TransCode;
        //         transItemDto.TransRef = data.TransRef;
        //         transItemDto.RefCode = request.TransRef;
        //         transItemDto.SrcAccountCode = data.SrcAccountCode;
        //         transItemDto.DesAccountCode = data.DesAccountCode;
        //         transItemDto.Price = data.Amount;
        //         transItemDto.Amount = data.Amount;
        //         transItemDto.ExtraInfo = extraInfo;
        //         transItemDto.DataInfo = dataInfo;
        //         if (response.BalanceAfterTrans != null)
        //         {
        //             if (transItemDto.TransType == TransactionType.MasterTopup)
        //             {
        //                 var item = response.BalanceAfterTrans.FirstOrDefault(c => c.DesAccount == request.DesAccountCode
        //                     && c.TransCode == request.TransactionCode && c.CurrencyCode == request.CurrencyCode);
        //                 if (item != null)
        //                 {
        //                     transItemDto.DesAccountBalance = item.DesBalance;
        //                     transItemDto.DesAccountBalanceBefore = item.DesBeforeBalance;
        //                 }
        //
        //                 if (transItemDto.DesAccountCode == "CONTROL")
        //                     transItemDto.TransCode = $"C.{transItemDto.TransCode}";
        //             }
        //             else
        //             {
        //                 var item = response.BalanceAfterTrans.FirstOrDefault(c => c.TransCode == data.TransCode &&
        //                     c.CurrencyCode == request.CurrencyCode
        //                     && c.DesAccount == transItemDto.DesAccountCode &&
        //                     c.SrcAccount == transItemDto.SrcAccountCode);
        //                 if (item != null)
        //                 {
        //                     transItemDto.SrcAccountBalance = item.SrcBalance;
        //                     transItemDto.SrcAccountBalanceBefore = item.SrcBeforeBalance;
        //                     transItemDto.DesAccountBalance = item.DesBalance;
        //                     transItemDto.DesAccountBalanceBefore = item.DesBeforeBalance;
        //                 }
        //                 else
        //                 {
        //                     transItemDto.SrcAccountBalance = response.SrcBalance;
        //                     transItemDto.DesAccountBalance = response.DesBalance;
        //                     transItemDto.SrcAccountBalanceBefore = transItemDto.SrcAccountBalance + transItemDto.Price;
        //                     transItemDto.DesAccountBalanceBefore = transItemDto.DesAccountBalance - transItemDto.Price;
        //                 }
        //
        //                 if (request.CurrencyCode == CurrencyCode.VND.ToString("G"))
        //                 {
        //                     var itemGxu = response.BalanceAfterTrans.FirstOrDefault(c =>
        //                         c.CurrencyCode == CurrencyCode.GXU.ToString("G"));
        //                     if (itemGxu != null)
        //                         request.Amount = itemGxu.Amount;
        //                 }
        //
        //                 if (transItemDto.DesAccountCode == "FEE" && request.Fee > 0)
        //                 {
        //                     transItemDto.Price = request.Fee;
        //                     transItemDto.Amount = request.Fee;
        //                     if (request.TransType == TransactionType.Give ||
        //                         request.TransType == TransactionType.Transfer)
        //                         transItemDto.ExtraInfo = request.DesAccountCode;
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             if (transItemDto.DesAccountCode == "FEE" && request.Fee > 0)
        //             {
        //                 transItemDto.Price = request.Fee;
        //                 transItemDto.Amount = request.Fee;
        //                 if (request.TransType == TransactionType.Give || request.TransType == TransactionType.Transfer)
        //                     transItemDto.ExtraInfo = request.DesAccountCode;
        //             }
        //
        //             transItemDto.SrcAccountBalance = response.SrcBalance;
        //             transItemDto.DesAccountBalance = response.DesBalance;
        //             transItemDto.SrcAccountBalanceBefore = transItemDto.SrcAccountBalance + transItemDto.Price;
        //             transItemDto.DesAccountBalanceBefore = transItemDto.DesAccountBalance - transItemDto.Price;
        //         }
        //
        //         await busReport.Publish<ReportBalanceHistoriesMessage>(new
        //         {
        //             Transaction = transItemDto
        //         });
        //     }
        // }
        // catch (Exception e)
        // {
        //     logger.LogError(
        //         $"TransCode= {request.TransactionCode}|TransRef= {request.TransRef} PublishBalanceHistoryMessage error: {e}");
        // }
    }
}