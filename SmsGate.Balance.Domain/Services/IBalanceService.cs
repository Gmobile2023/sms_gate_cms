using System.Collections.Generic;
using System.Threading.Tasks;
using SmsGate.Balance.Models.Dtos;
using SmsGate.Balance.Models.Enums;
using SmsGate.Balance.Models.Requests;
using SmsGate.Shared.Common;
using SmsGate.Shared.Contract.Grpc.Balance;

namespace SmsGate.Balance.Domain.Services;

public interface IBalanceService
{
    Task<AccountBalanceDto> AccountBalanceGetAsync(string accountCode, string currencyCode, BalanceStatus balanceStatus = BalanceStatus.Active);
    Task<AccountBalanceDto> AccountBalanceCreateAsync(AccountBalanceDto accountBalance);
    Task<bool> AccountBalanceUpdateAsync(AccountBalanceDto accountBalanceDto);
    Task<ResponseMessageBase<object>> CheckCurrencyAsync(string currencyCode);
    Task CurrencyCreateAsync(string currencyCode);

    Task<ResponseMessageBase<object>> TransferAsync(BalanceTransferRequest transferRequest);
    Task<ResponseMessageBase<object>> CurrencyExchangeAsync(CurrencyExchangeRequest exchangeRequest);
    Task<ResponseMessageBase<object>> DepositAsync(BalanceDepositRequest depositRequest);
    Task<ResponseMessageBase<object>> CashOutAsync(CashOutRequest cashOutRequest);
    Task<ResponseMessageBase<object>> MasterTopupAsync(MasterTopupRequest masterTopupRequest);
    Task<ResponseMessageBase<object>> PaymentAsync(BalancePaymentRequest paymentRequest);
    Task<ResponseMessageBase<object>> RevertAsync(BalanceRevertRequest paymentRequest);
    Task<ResponseMessageBase<object>> CancelPaymentAsync(BalanceCancelPaymentRequest paymentRequest);
    Task<decimal> AccountBalanceCheckAsync(AccountBalanceCheckRequest accountBalanceCheckRequest);
    Task<ResponseMessageBase<object>> AdjustmentAsync(AdjustmentRequest request);
    Task<ResponseMessageBase<object>> BlockBalanceAsync(BlockBalanceRequest request);
    Task<ResponseMessageBase<object>> UnBlockBalanceAsync(UnBlockBalanceRequest request);
    Task<ResponseMessageBase<object>> TransferSystemAsync(TransferSystemRequest transferRequest);
    Task<ResponseMessageBase<string>> GetSettlementListHistory(BalanceHistoriesRequest request);
    bool CheckAccountSystem(string accountCode);
    Task TransferShardAccountToMain(string accountCode, CurrencyCode currencyCode);
    Task<ResponseMessageBase<object>> ChargeFeeAsync(BalanceChargeFeeRequest request);
}