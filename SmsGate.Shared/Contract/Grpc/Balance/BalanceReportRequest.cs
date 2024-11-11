using System;
using System.Runtime.Serialization;

using ServiceStack;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance;


[DataContract]
[Route("/api/v1/wallet/report/balance_history", "GET")]
public class BalanceHistoriesRequest : IGet, IReturn<ResponseMessageBase<string>>
{
    [DataMember(Order = 1)] public DateTime FromDate { get; set; }
    [DataMember(Order = 2)] public DateTime ToDate { get; set; }
    [DataMember(Order = 3)] public string AccountCode { get; set; }
    [DataMember(Order = 4)] public string CurrencyCode { get; set; }
    [DataMember(Order = 5)] public string TransCode { get; set; }
    [DataMember(Order = 6)] public string TransRef { get; set; }
  
}

[DataContract]
[Route("/api/v1/wallet/report/balance_account", "GET")]
public class BalanceAccountCodesRequest
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
}    

[DataContract]
public class SettlementItemDto
{
    [DataMember(Order = 1)] public decimal Amount { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
    [DataMember(Order = 3)] public string SrcAccountCode { get; set; }
    [DataMember(Order = 4)] public string DesAccountCode { get; set; }
    [DataMember(Order = 5)] public decimal SrcAccountBalance { get; set; }
    [DataMember(Order = 6)] public decimal DesAccountBalance { get; set; }
    [DataMember(Order = 7)] public decimal SrcAccountBalanceBeforeTrans { get; set; }
    [DataMember(Order = 8)] public decimal DesAccountBalanceBeforeTrans { get; set; }
    [DataMember(Order = 9)] public string TransRef { get; set; }
    [DataMember(Order = 10)] public DateTime CreatedDate { get; set; }
    [DataMember(Order = 11)] public string Description { get; set; }
    [DataMember(Order = 12)] public bool ReturnResult { get; set; }
    [DataMember(Order = 13)] public string PaymentTransCode { get; set; }//Mã gọi sang từ đối tác
    [DataMember(Order = 14)] public TransactionType TransType { get; set; }
}
