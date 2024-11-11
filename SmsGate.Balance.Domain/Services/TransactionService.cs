using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsGate.Balance.Models.Enums;
using SmsGate.Balance.Models.Requests;
using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceStack;
using SmsGate.Balance.Domain.Entities;
using SmsGate.Balance.Domain.Repositories;
using SmsGate.Shared.Common;
using SmsGate.Shared.Contract.Grpc.Balance;

namespace SmsGate.Balance.Domain.Services;

public class TransactionService(
    ITransactionRepository transactionRepository,
    ILogger<TransactionService> logger,
    IBus bus)
    : ITransactionService
{
    //private readonly Logger _logger = LogManager.GetLogger("TransactionService");

    public async Task<ResponseMessageBase<object>> DepositAsync(BalanceDepositRequest depositRequest)
    {
        var createdDate = DateTime.Now;
        var transaction = new Entities.Transaction
        {
            Amount = depositRequest.Amount,
            CreatedDate = createdDate,
            TransRef = depositRequest.TransRef,
            CurrencyCode = depositRequest.CurrencyCode,
            DesAccountCode = depositRequest.AccountCode,
            SrcAccountCode = BalanceConst.MASTER_ACCOUNT,
            Description = depositRequest.Description,
            Status = TransStatus.Done,
            TransType = TransactionType.Deposit,
            TransNote = depositRequest.TransNote,
            Fee = depositRequest.Fee,
            TransactionCode =  Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{depositRequest.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement()
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransCode = Guid.NewGuid().ToString(),
            TransType = transaction.TransType,
            Order = 1
        };
        transaction.Settlements = [settlement];
        if (transaction.Fee > 0)
        {
            var settlementFee = new Settlement
            {
                CreatedDate = DateTime.Now,
                Amount = transaction.Fee,
                TransRef = transaction.TransactionCode,
                Status = SettlementStatus.Done,
                SrcAccountCode = depositRequest.AccountCode,
                DesAccountCode = BalanceConst.FEE_ACCOUNT,
                CurrencyCode = depositRequest.CurrencyCode,
                TransCode = Guid.NewGuid().ToString(),
                ModifiedDate = DateTime.Now,
                TransType = TransactionType.Fee,
                Order = 2
            };
            transaction.Settlements.Add(settlementFee);
        }

        return ResponseMessageBase<object>.Success(transaction);
    }

    public async Task<ResponseMessageBase<object>> ExchangeAsync(CurrencyExchangeRequest currencyExchangeRequest)
    {
        var createdDate = DateTime.Now;
        var transaction1 = new Transaction
        {
            Amount = currencyExchangeRequest.SrcAmount,
            CreatedDate = createdDate,
            TransRef = currencyExchangeRequest.TransRef,
            CurrencyCode = currencyExchangeRequest.SrcCurrencyCode,
            DesAccountCode = BalanceConst.MASTER_ACCOUNT,
            SrcAccountCode = currencyExchangeRequest.AccountCode,
            Description = "EXCHANGE",
            Status = TransStatus.Done,
            TransType = TransactionType.Exchange,
            TransNote = "EXCHANGE",
            TransactionCode = Guid.NewGuid().ToString()
        };

        var transaction2 = new Transaction
        {
            Amount = currencyExchangeRequest.DesAmount,
            CreatedDate = createdDate,
            TransRef = currencyExchangeRequest.TransRef,
            CurrencyCode = currencyExchangeRequest.DesCurrencyCode,
            DesAccountCode = currencyExchangeRequest.AccountCode,
            SrcAccountCode = BalanceConst.MASTER_ACCOUNT,
            Description = "EXCHANGE",
            Status = TransStatus.Done,
            TransType = TransactionType.Exchange,
            TransNote = "EXCHANGE",
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            var trans = await transactionRepository.TransactionsCreateAsync([transaction1, transaction2]);

            if (trans.Count > 1)
            {
                var settlement1 = new Settlement
                {
                    CreatedDate = createdDate,
                    Amount = trans[0].Amount,
                    TransRef = trans[0].TransactionCode,
                    PaymentTransCode = trans[0].TransRef,
                    Status = SettlementStatus.Init,
                    SrcAccountCode = trans[0].SrcAccountCode,
                    SrcShardAccountCode = trans[0].SrcAccountCode,
                    DesAccountCode = trans[0].DesAccountCode,
                    DesShardAccountCode = trans[0].DesAccountCode,
                    CurrencyCode = trans[0].CurrencyCode,
                    TransType = trans[0].TransType,
                    TransCode = Guid.NewGuid().ToString()
                };
                trans[0].Settlements = [settlement1];

                var settlement2 = new Settlement
                {
                    CreatedDate = createdDate,
                    Amount = trans[1].Amount,
                    TransRef = trans[1].TransactionCode,
                    PaymentTransCode = trans[1].TransRef,
                    Status = SettlementStatus.Init,
                    SrcAccountCode = trans[1].SrcAccountCode,
                    SrcShardAccountCode = trans[1].SrcAccountCode,
                    DesAccountCode = trans[1].DesAccountCode,
                    DesShardAccountCode = trans[1].DesAccountCode,
                    CurrencyCode = trans[1].CurrencyCode,
                    TransCode = Guid.NewGuid().ToString(),
                    TransType = trans[1].TransType
                };
                trans[1].Settlements = [settlement2];

                return ResponseMessageBase<object>.Success(trans);
            }
        }
        catch (Exception e)
        {
            logger.LogError($"{currencyExchangeRequest.TransRef}-Create transaction error: " + e.Message);
        }

        return ResponseMessageBase<object>.Error();
    }

    public async Task<ResponseMessageBase<object>> TransferAsync(BalanceTransferRequest transferRequest)
    {
        var transaction = new Transaction
        {
            Amount = transferRequest.Amount,
            TransRef = transferRequest.TransRef,
            CurrencyCode = transferRequest.CurrencyCode,
            DesAccountCode = transferRequest.DesAccount,
            SrcAccountCode = transferRequest.SrcAccount,
            Description = transferRequest.Description,
            Status = TransStatus.Done,
            TransType = transferRequest.TransactionType == TransactionType.Default
                ? TransactionType.Transfer
                : transferRequest.TransactionType,
            CreatedDate = DateTime.Now,
            TransNote = transferRequest.TransNote,
            Fee = transferRequest.Fee,
            TransactionCode = Guid.NewGuid().ToString(),
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{transferRequest.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = DateTime.Now,
            Amount = transferRequest.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransType = transaction.TransType,
            TransCode = Guid.NewGuid().ToString(),
            ModifiedDate = DateTime.Now,
            Order = 1
        };

        transaction.Settlements = [settlement];
        if (transaction.Fee > 0)
        {
            var settlementFee = new Settlement
            {
                CreatedDate = DateTime.Now,
                Amount = transferRequest.Fee,
                TransRef = transaction.TransactionCode,
                Status = SettlementStatus.Done,
                SrcAccountCode = transferRequest.FeeType == FeeType.OutsideFee
                    ? transferRequest.DesAccount
                    : transferRequest.SrcAccount,
                DesAccountCode = BalanceConst.FEE_ACCOUNT,
                CurrencyCode = transaction.CurrencyCode,
                TransCode = Guid.NewGuid().ToString(),
                ModifiedDate = DateTime.Now,
                TransType = TransactionType.Fee,
                Order = 2
            };
            transaction.Settlements.Add(settlementFee);
        }

        return ResponseMessageBase<object>.Success(transaction);
    }

    public Task<ResponseMessageBase<object>> TransferCurrencyAsync(CurrencyExchangeRequest exchangeRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ResponseMessageBase<object>> CashOutAsync(CashOutRequest cashOutRequest)
    {
        var createdDate = DateTime.Now;
        var transaction = new Transaction
        {
            CreatedDate = createdDate,
            Amount = cashOutRequest.Amount,
            TransRef = cashOutRequest.TransRef,
            CurrencyCode = cashOutRequest.CurrencyCode,
            SrcAccountCode = cashOutRequest.AccountCode,
            DesAccountCode = BalanceConst.CASHOUT_ACCOUNT,
            Description = cashOutRequest.Description,
            Status = TransStatus.Done,
            TransType = TransactionType.Cashout,
            TransNote = cashOutRequest.TransNote,
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{cashOutRequest.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransType = transaction.TransType,
            TransCode = Guid.NewGuid().ToString(),
            ModifiedDate = DateTime.Now
        };

        transaction.Settlements = [settlement];
        return ResponseMessageBase<object>.Success(transaction);
    }

    public async Task<ResponseMessageBase<object>> PaymentAsync(BalancePaymentRequest paymentRequest)
    {
        var createdDate = DateTime.Now;
        var transaction = new Transaction
        {
            CreatedDate = createdDate,
            Amount = paymentRequest.PaymentAmount,
            TransRef = paymentRequest.TransRef,
            CurrencyCode = paymentRequest.CurrencyCode,
            SrcAccountCode = paymentRequest.AccountCode,
            DesAccountCode = !string.IsNullOrEmpty(paymentRequest.MerchantCode)
                ? paymentRequest.MerchantCode
                : BalanceConst.PAYMENT_ACCOUNT,
            Description = paymentRequest.Description,
            Status = TransStatus.Done,
            TransType = TransactionType.Payment,
            TransNote = paymentRequest.TransNote,
            Fee = paymentRequest.Fee,
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"{paymentRequest.TransRef}-Create transaction error: " + e.Message);

            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransCode = Guid.NewGuid().ToString(),
            TransType = transaction.TransType,
            ModifiedDate = DateTime.Now,
            Order = 1
        };
        transaction.Settlements = [settlement];
        if (transaction.Fee > 0)
        {
            var settlementFee = new Settlement
            {
                CreatedDate = DateTime.Now,
                Amount = transaction.Fee,
                TransRef = transaction.TransactionCode,
                Status = SettlementStatus.Done,
                SrcAccountCode = paymentRequest.AccountCode,
                DesAccountCode = BalanceConst.FEE_ACCOUNT,
                CurrencyCode = paymentRequest.CurrencyCode,
                TransCode = Guid.NewGuid().ToString(),
                ModifiedDate = DateTime.Now,
                TransType = TransactionType.Fee,
                Order = 2
            };
            transaction.Settlements.Add(settlementFee);
        }

        return ResponseMessageBase<object>.Success(transaction);
    }

    public async Task<ResponseMessageBase<object>> CancelPaymentAsync(BalanceCancelPaymentRequest request)
    {
        if (await CheckCancelPayment(request.TransRef))
            return ResponseMessageBase<object>.Error("Không thể hoàn tiền cho giao đã được hoàn trước đó");

        var paymentTransaction =
            await transactionRepository.TransactionSelectOneAsync(request.TransactionCode);
        if (paymentTransaction == null)
        {
            return ResponseMessageBase<object>.Error("Giao dịch không tồn tại");
        }

        var createdDate = DateTime.Now;
        var transaction = new Transaction
        {
            CreatedDate = createdDate,
            Amount = request.RevertAmount,
            TransRef = request.TransRef,
            CurrencyCode = request.CurrencyCode,
            SrcAccountCode = paymentTransaction.DesAccountCode,
            DesAccountCode = request.AccountCode,
            Description = request.Description,
            Status = TransStatus.Done,
            TransType = TransactionType.CancelPayment,
            TransNote = request.TransNote,
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{request.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransCode = Guid.NewGuid().ToString(),
            TransType = transaction.TransType,
            ModifiedDate = DateTime.Now
        };


        transaction.Settlements = [settlement];
        return ResponseMessageBase<object>.Success(transaction);
    }

    public async Task<ResponseMessageBase<object>> MasterTopupAsync(MasterTopupRequest masterTopupRequest)
    {
        var createdDate = DateTime.Now;
        var transaction = new Transaction
        {
            CreatedDate = createdDate,
            Amount = masterTopupRequest.Amount,
            TransRef = masterTopupRequest.TransRef,
            CurrencyCode = masterTopupRequest.CurrencyCode,
            DesAccountCode = BalanceConst.MASTER_ACCOUNT,
            Status = TransStatus.Done,
            TransType = TransactionType.MasterTopup,
            TransNote = masterTopupRequest.TransNote,
            TransactionCode = Guid.NewGuid().ToString()
        };
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{masterTopupRequest.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement1 = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            DesAccountCode = BalanceConst.CONTROL_ACCOUNT,
            DesShardAccountCode = BalanceConst.CONTROL_ACCOUNT,
            CurrencyCode = transaction.CurrencyCode,
            TransCode = Guid.NewGuid().ToString(),
            TransType = transaction.TransType,
            ModifiedDate = DateTime.Now
        };

        var settlement2 = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            TransType = transaction.TransType,
            TransCode = Guid.NewGuid().ToString(),
            ModifiedDate = DateTime.Now
        };

        transaction.Settlements = [settlement1, settlement2];
        return ResponseMessageBase<object>.Success(transaction);
    }
    public async Task<ResponseMessageBase<object>> RevertAsync(Transaction transactionRevert)
    {
        var createdDate = DateTime.Now;
        var transaction = new Transaction
        {
            CreatedDate = createdDate,
            Amount = transactionRevert.Amount,

            TransRef = transactionRevert.TransRef,
            CurrencyCode = transactionRevert.CurrencyCode,
            DesAccountCode = transactionRevert.SrcAccountCode,
            SrcAccountCode = transactionRevert.DesAccountCode,
            Description = transactionRevert.Description,
            Status = TransStatus.Done,
            TransType = TransactionType.Revert,
            TransNote = transactionRevert.Description,
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{transactionRevert.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransCode = Guid.NewGuid().ToString(),
            TransType = transaction.TransType,
            ModifiedDate = DateTime.Now
        };

        transaction.Settlements = [settlement];
        return ResponseMessageBase<object>.Success(transaction);
    }

    public async Task SettlementsInsertAsync(List<Settlement> settlements)
    {
        try
        {
            await transactionRepository.SettlementCreateManyAsync(settlements);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error insert settlement: " + ex.Message);
            await Task.Run(() =>
            {
                // bus.Publish<AlarmMessageCommand>(new
                // {
                //     Module = "Balance",
                //     MessageType = BotMessageType.Error,
                //     Title = $"Error insert settlement \n" +
                //             $"{ex.Message}\n" +
                //             $"Statement: \n" +
                //             $"{settlements.ToJson()}"
                // });
            });
        }
    }

    public async Task<ResponseMessageBase<object>> AdjustmentAsync(AdjustmentRequest request)
    {
        var transaction = new Transaction
        {
            Amount = request.Amount,
            TransRef = request.TransRef,
            CurrencyCode = request.CurrencyCode,
            DesAccountCode = request.AdjustmentType == AdjustmentType.Decrease
                ? BalanceConst.MASTER_ACCOUNT
                : request.AccountCode,
            SrcAccountCode = request.AdjustmentType == AdjustmentType.Decrease
                ? request.AccountCode
                : BalanceConst.MASTER_ACCOUNT,
            Description = request.TransNote,
            Status = TransStatus.Done,
            TransType = request.AdjustmentType == AdjustmentType.Decrease
                ? TransactionType.AdjustmentDecrease
                : TransactionType.AdjustmentIncrease,
            CreatedDate = DateTime.Now,
            TransNote = request.TransNote,
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{request.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = DateTime.Now,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransCode = Guid.NewGuid().ToString(),
            TransType = transaction.TransType,
            ModifiedDate = DateTime.Now,
        };


        transaction.Settlements = [settlement];
        return ResponseMessageBase<object>.Success(transaction);
    }
    

    public async Task<ResponseMessageBase<object>> BlockBalanceAsync(BlockBalanceRequest request)
    {
        var createdDate = DateTime.Now;
        var transaction = new Transaction
        {
            Amount = request.BlockAmount,

            CreatedDate = createdDate,
            TransRef = request.TransRef,
            CurrencyCode = request.CurrencyCode,
            SrcAccountCode = request.AccountCode,
            Description = request.TransNote,
            Status = TransStatus.Done,
            TransType = TransactionType.Block,
            TransNote = request.TransNote,
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{request.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransType = transaction.TransType,
            TransCode = Guid.NewGuid().ToString()
        };


        transaction.Settlements = [settlement];
        return ResponseMessageBase<object>.Success(transaction);
    }

    public async Task<ResponseMessageBase<object>> UnBlockBalanceAsync(UnBlockBalanceRequest request)
    {
        var createdDate = DateTime.Now;
        var transaction = new Transaction
        {
            Amount = request.UnBlockAmount,

            CreatedDate = createdDate,
            TransRef = request.TransRef,
            CurrencyCode = request.CurrencyCode,
            DesAccountCode = request.AccountCode,
            Description = request.TransNote,
            Status = TransStatus.Done,
            TransType = TransactionType.Unblock,
            TransNote = request.TransNote,
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{request.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            DesAccountCode = transaction.SrcAccountCode,
            DesShardAccountCode = transaction.SrcAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransType = transaction.TransType,
            TransCode = Guid.NewGuid().ToString()
        };


        transaction.Settlements = [settlement];
        return ResponseMessageBase<object>.Success(transaction);
    }

    public async Task<ResponseMessageBase<object>> TransferSystemAsync(TransferSystemRequest transferRequest)
    {
        var transaction = new Transaction
        {
            Amount = transferRequest.Amount,
            TransRef = transferRequest.TransRef,
            CurrencyCode = transferRequest.CurrencyCode,
            DesAccountCode = transferRequest.DesAccount,
            SrcAccountCode = transferRequest.SrcAccount,
            Status = TransStatus.Done,
            TransType = TransactionType.SystemTransfer,
            CreatedDate = DateTime.Now,
            TransNote = transferRequest.TransNote,
            TransactionCode = Guid.NewGuid().ToString()
        };

        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{transferRequest.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = DateTime.Now,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransCode = Guid.NewGuid().ToString(),
            TransType = transaction.TransType,
            ModifiedDate = DateTime.Now,
        };


        transaction.Settlements = [settlement];
        return ResponseMessageBase<object>.Success(transaction);
    }

    public async Task<bool> CheckCancelPayment(string transCode)
    {
        var item = await transactionRepository.TransactionSelectOneAsync(transCode, TransactionType.CancelPayment);
        return item != null;
    }

    public async Task UpdateTransactionStatus(string transCode, TransStatus status)
    {
        await transactionRepository.TransactionUpdateAsync(new Transaction
            { TransactionCode = transCode, Status = status });
    }

    public async Task<ResponseMessageBase<string>> GetSettlementListHistory(BalanceHistoriesRequest request)
    {
        try
        {
            var list = await transactionRepository.GetSettlementSelectByAsync(request.FromDate, request.ToDate,
                request.AccountCode, request.CurrencyCode, request.TransCode, request.TransRef);
            return ResponseMessageBase<string>.Success(list.ToJson());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ResponseMessageBase<string>.Error();
        }
    }

    public async Task<ResponseMessageBase<object>> ChargeFeeAsync(BalanceChargeFeeRequest request)
    {
        var createdDate = DateTime.Now;
        var transaction = new Transaction
        {
            Amount = request.Fee,
            CreatedDate = createdDate,
            TransRef = request.TransRef,
            CurrencyCode = request.CurrencyCode,
            DesAccountCode = !string.IsNullOrEmpty(request.DesAccount)
                ? request.DesAccount
                : BalanceConst.FEE_ACCOUNT,
            SrcAccountCode = request.SrcAccount,
            Description = request.Description,
            Status = TransStatus.Done,
            TransType = request.TransactionType,
            TransNote = request.TransNote,
            Fee = request.Fee,
            TransactionCode = Guid.NewGuid().ToString()
        };
        try
        {
            transaction = await transactionRepository.TransactionCreateAsync(transaction);
        }
        catch (Exception e)
        {
            logger.LogError($"{request.TransRef}-Create transaction error: " + e.Message);
            return ResponseMessageBase<object>.Error();
        }

        var settlement = new Settlement
        {
            CreatedDate = createdDate,
            Amount = transaction.Amount,
            TransRef = transaction.TransactionCode,
            Status = SettlementStatus.Init,
            SrcAccountCode = transaction.SrcAccountCode,
            SrcShardAccountCode = transaction.SrcAccountCode,
            DesAccountCode = transaction.DesAccountCode,
            DesShardAccountCode = transaction.DesAccountCode,
            CurrencyCode = transaction.CurrencyCode,
            PaymentTransCode = transaction.TransRef,
            TransCode = Guid.NewGuid().ToString(),
            TransType = transaction.TransType,
            Order = 1
        };
        transaction.Settlements = [settlement];
        return ResponseMessageBase<object>.Success(transaction);
    }
}