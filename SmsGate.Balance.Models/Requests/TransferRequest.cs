using System.Collections.Generic;
using System.Runtime.Serialization;

using ServiceStack;
using ServiceStack.DataAnnotations;
using SmsGate.Balance.Models.Enums;

namespace SmsGate.Balance.Models.Requests;

[DataContract]
[Route("/api/v1/wallet/deposit", "POST")]
[Route("/api/v1/wallet/deposit/{AccountCode}/{CurrencyCode}/{Amount}/{TransRef}", "POST")]
[Route("/api/v1/wallet/deposit/{AccountCode}/{CurrencyCode}/{Amount}/{TransRef}/{Description}", "POST")]
public class DepositRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
    [DataMember(Order = 3)] public decimal Amount { get; set; }
    [DataMember(Order = 4)] public string TransRef { get; set; }
    [DataMember(Order = 5)] public string Description { get; set; }
    [DataMember(Order = 6)] public string TransNote { get; set; }
    [DataMember(Order = 7)] public string ExtraInfo { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/cashout", "POST")]
[Route("/api/v1/wallet/cashout/{AccountCode}/{CurrencyCode}/{Amount}/{TransRef}", "POST")]
[Route("/api/v1/wallet/cashout/{AccountCode}/{CurrencyCode}/{Amount}/{TransRef}/{Description}", "POST")]
public class CashOutRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
    [DataMember(Order = 3)] public decimal Amount { get; set; }
    [DataMember(Order = 4)] public string TransRef { get; set; }
    [DataMember(Order = 5)] public string Description { get; set; }
    [DataMember(Order = 6)] public string TransNote { get; set; }
}

// [DataContract]
// [Route("/api/v1/wallet/payment", "POST")]
// [Route("/api/v1/wallet/payment/{AccountCode}/{CurrencyCode}/{PaymentAmount}", "POST")]
// [Route("/api/v1/wallet/payment/{AccountCode}/{CurrencyCode}/{PaymentAmount}/{Description}", "POST")]
// public class PaymentRequest : IPost, IReturn<object>
// {
//     [DataMember(Order = 1)][Required] public string AccountCode { get; set; }
//     [DataMember(Order = 2)] public decimal PaymentAmount { get; set; }
//     [DataMember(Order = 3)] public string CurrencyCode { get; set; }
//     [DataMember(Order = 4)] public string TransRef { get; set; }
//     [DataMember(Order = 5)] public string Description { get; set; }
//     [DataMember(Order = 6)] public string MerchantCode { get; set; }
//     [DataMember(Order = 7)] public string TransNote { get; set; }
// }

[DataContract]
[Route("/api/v1/wallet/priority_payment", "POST")]
public class PriorityPaymentRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public string TransactionCode { get; set; }
    [DataMember(Order = 2)] public string TransRef { get; set; }
    [DataMember(Order = 3)] public decimal Amount { get; set; }
    [DataMember(Order = 4)] public string TransNote { get; set; }
    [DataMember(Order = 5)] public string Description { get; set; }
    [DataMember(Order = 6)] public string AccountCode { get; set; }
    [DataMember(Order = 7)] public string CurrencyCode { get; set; }
}

// [DataContract]
// [Route("/api/v1/wallet/correct", "POST")]
// [Route("/api/v1/wallet/correct/{TransactionCode}/{Amount}/{TransRef}", "POST")]
// [Route("/api/v1/wallet/correct/{TransactionCode}/{Amount}/{Reason}/{TransRef}", "POST")]
// public class CorrectRequest : IPost, IReturn<object>
// {
//     [DataMember(Order = 1)] public string TransactionCode { get; set; }
//     [DataMember(Order = 2)] public string Reason { get; set; }
//     [DataMember(Order = 3)] public decimal Amount { get; set; }
//     [DataMember(Order = 4)] public string TransRef { get; set; }
//     [DataMember(Order = 5)] public string TransNote { get; set; }
//     [DataMember(Order = 6)] public string AccountCode { get; set; }
// }

[DataContract]
[Route("/api/v1/wallet/collect_discount", "POST")]
public class CollectDiscountRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public string Reason { get; set; }
    [DataMember(Order = 2)] public decimal Amount { get; set; }
    [DataMember(Order = 3)] public string TransRef { get; set; }
    [DataMember(Order = 4)] public string TransNote { get; set; }
    [DataMember(Order = 5)] public string AccountCode { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/transfer", "POST")]
[Route("/api/v1/wallet/transfer/{SrcAccount}/{DesAccount}/{CurrencyCode}/{Amount}", "POST")]
[Route("/api/v1/wallet/transfer/{SrcAccount}/{DesAccount}/{CurrencyCode}/{Amount}/{Description}", "POST")]
public class TransferRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public decimal Amount { get; set; }
    [DataMember(Order = 2)] public string SrcAccount { get; set; }
    [DataMember(Order = 3)] public string CurrencyCode { get; set; }
    [DataMember(Order = 4)] public string DesAccount { get; set; }
    [DataMember(Order = 5)] public string TransRef { get; set; }
    [DataMember(Order = 6)] public string Description { get; set; }
    [DataMember(Order = 7)] public string TransNote { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/currency_exchange", "POST")] //Viết tạm hàm này dùng chung
public class CurrencyExchangeRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public decimal SrcAmount { get; set; }
    [DataMember(Order = 3)] public decimal DesAmount { get; set; }
    [DataMember(Order = 4)] public string SrcCurrencyCode { get; set; }
    [DataMember(Order = 5)] public string DesCurrencyCode { get; set; }
    [DataMember(Order = 6)] public string TransRef { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/master_topup", "POST")]
[Route("/api/v1/wallet/master_topup/{CurrencyCode}/{Amount}/{TransRef}/{BankCode}", "POST")]
public class MasterTopupRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public string CurrencyCode { get; set; }
    [DataMember(Order = 2)] public decimal Amount { get; set; }
    [DataMember(Order = 3)] public string TransRef { get; set; }
    [DataMember(Order = 4)] public string BankCode { get; set; }
    [DataMember(Order = 5)] public string TransNote { get; set; }
}

// [DataContract]
// [Route("/api/v1/wallet/master_topdown", "POST")]
// [Route("/api/v1/wallet/master_topdown/{CurrencyCode}/{Amount}/{TransRef}/{BankCode}", "POST")]
// public class MasterTopdownRequest : IPost, IReturn<object>
// {
//     [DataMember(Order = 1)] public string CurrencyCode { get; set; }
//     [DataMember(Order = 2)] public decimal Amount { get; set; }
//     [DataMember(Order = 3)] public string TransRef { get; set; }
//     [DataMember(Order = 4)] public string BankCode { get; set; }
//     [DataMember(Order = 5)] public string TransNote { get; set; }
// }

// [DataContract]
// [Route("/api/v1/wallet/charging", "POST")]
// public class ChargingRequest : IPost, IReturn<object>
// {
//     [DataMember(Order = 1)] public string TransactionCode { get; set; }
//     [DataMember(Order = 2)] public decimal Amount { get; set; }
//     [DataMember(Order = 3)] public string TransRef { get; set; }
//     [DataMember(Order = 4)] public string TransNote { get; set; }
//     [DataMember(Order = 5)] public string AccountCode { get; set; }
//     [DataMember(Order = 6)] public string CurrencyCode { get; set; }
// }

[DataContract]
[Route("/api/v1/wallet/adjustment", "POST")]
public class AdjustmentRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
    [DataMember(Order = 3)] public decimal Amount { get; set; }
    [DataMember(Order = 4)] public string TransRef { get; set; }
    [DataMember(Order = 5)] public string TransNote { get; set; }
    [DataMember(Order = 6)] public AdjustmentType AdjustmentType { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/clear_debt", "POST")]
public class ClearDebtRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public decimal Amount { get; set; }
    [DataMember(Order = 3)] public string TransRef { get; set; }
    [DataMember(Order = 4)] public string TransNote { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/sale_deposit", "POST")]
public class SaleDepositRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] [Required] public string AccountCode { get; set; }

    [DataMember(Order = 2)] [Required] public string SaleCode { get; set; }

    //public string CurrencyCode { get; set; }
    [DataMember(Order = 3)] [Required] public decimal Amount { get; set; }
    [DataMember(Order = 4)] [Required] public string TransRef { get; set; }
    [DataMember(Order = 5)] [Required] public string TransNote { get; set; }
}
[DataContract]
[Route("/api/v1/wallet/pay_batch", "POST")]
public class PaybatchRequest : IPost, IReturn<object>
{
    [DataMember(Order = 1)] public List<PaybatchAccount> Accounts { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
    [DataMember(Order = 3)] public string TransRef { get; set; }
    [DataMember(Order = 4)] public string TransNote { get; set; }
}

[DataContract]
public class PaybatchAccount
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public decimal Amount { get; set; }
    [DataMember(Order = 3)] public bool Success { get; set; }
    [DataMember(Order = 4)] public string TransRef { get; set; }
    [DataMember(Order = 5)] public string TransNote { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/transfer_shard", "POST")]
public class TransferShardRequest : IPost, IReturnVoid
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/transfer_system", "POST")]
public class TransferSystemRequest : IPost
{
    [DataMember(Order = 1)] public decimal Amount { get; set; }
    [DataMember(Order = 2)] public string SrcAccount { get; set; }
    [DataMember(Order = 3)] public string CurrencyCode { get; set; }
    [DataMember(Order = 4)] public string DesAccount { get; set; }
    [DataMember(Order = 5)] public string TransRef { get; set; }
    [DataMember(Order = 6)] public string TransNote { get; set; }
}