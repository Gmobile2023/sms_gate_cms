using System.Collections.Generic;
using System.Threading.Tasks;
using SmsGate.Balance.Models.Enums;
using SmsGate.Balance.Models.Requests;
using SmsGate.Balance.Domain.Entities;
using SmsGate.Shared.Common;
using SmsGate.Shared.Contract.Grpc.Balance;

namespace SmsGate.Balance.Domain.Services;

public interface ITransactionService
{
    Task<ResponseMessageBase<object>> DepositAsync(BalanceDepositRequest depositRequest);

    //Task<string> CodeGenGetAsync(string prefix, string key);
    Task<ResponseMessageBase<object>> TransferAsync(BalanceTransferRequest transferRequest);
    Task<ResponseMessageBase<object>> TransferCurrencyAsync(CurrencyExchangeRequest exchangeRequest);
    Task SettlementsInsertAsync(List<Settlement> settlements);
    Task<ResponseMessageBase<object>> CashOutAsync(CashOutRequest cashOutRequest);
    Task<ResponseMessageBase<object>> PaymentAsync(BalancePaymentRequest paymentRequest);
    Task<ResponseMessageBase<object>> CancelPaymentAsync(BalanceCancelPaymentRequest request);
    Task<ResponseMessageBase<object>> MasterTopupAsync(MasterTopupRequest masterTopupRequest);

    Task<ResponseMessageBase<object>> RevertAsync(Transaction transactionRevert);
    Task<ResponseMessageBase<object>> AdjustmentAsync(AdjustmentRequest request);
    Task<ResponseMessageBase<object>> BlockBalanceAsync(BlockBalanceRequest request);
    Task<ResponseMessageBase<object>> UnBlockBalanceAsync(UnBlockBalanceRequest request);
    Task<ResponseMessageBase<object>> TransferSystemAsync(TransferSystemRequest transferRequest);
    Task<bool> CheckCancelPayment(string transCode);
    Task UpdateTransactionStatus(string transCode, TransStatus status);
    Task<ResponseMessageBase<object>> ExchangeAsync(CurrencyExchangeRequest currencyExchangeRequest);

    Task<ResponseMessageBase<string>> GetSettlementListHistory(BalanceHistoriesRequest request);
    Task<ResponseMessageBase<object>> ChargeFeeAsync(BalanceChargeFeeRequest request);
}