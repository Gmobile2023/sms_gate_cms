using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmsGate.Shared.Common;


[DataContract]
public class BalanceResponse
{
    [DataMember(Order = 1)] public decimal SrcBalance { get; set; }
    [DataMember(Order = 2)] public decimal DesBalance { get; set; }
    [DataMember(Order = 3)] public string TransactionCode { get; set; }
    [DataMember(Order = 4)] public List<BalanceAfterTransDto> BalanceAfterTrans { get; set; }
}
[DataContract]
public class BalanceAfterTransDto
{
    [DataMember(Order = 1)] public string SrcAccount { get; set; }
    [DataMember(Order = 2)] public decimal SrcBalance { get; set; }
    [DataMember(Order = 3)] public decimal SrcBeforeBalance { get; set; }
    [DataMember(Order = 4)] public string DesAccount { get; set; }
    [DataMember(Order = 5)] public decimal DesBalance { get; set; }
    [DataMember(Order = 6)] public decimal DesBeforeBalance { get; set; }
    [DataMember(Order = 7)] public decimal Amount { get; set; }
    [DataMember(Order = 8)] public string TransCode { get; set; }
    [DataMember(Order = 9)] public string CurrencyCode { get; set; }
}