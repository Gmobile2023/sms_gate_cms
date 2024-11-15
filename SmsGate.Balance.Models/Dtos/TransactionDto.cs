using System;
using System.Collections.Generic;

using Orleans;
using SmsGate.Balance.Models.Enums;
using SmsGate.Shared.Common;

namespace SmsGate.Balance.Models.Dtos;

public class TransactionDto
{
    public long Id { get; set; }
    public string TransactionCode { get; set; }
    public string SrcAccountCode { get; set; }
    public string DesAccountCode { get; set; }
    public string TransRef { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public TransactionType TransType { get; set; }
    public TransStatus Status { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Description { get; set; }
    public string TransNote { get; set; }
    public string RevertTransCode { get; set; }
    public decimal Fee { get; set; }
    public List<SettlementDto> Settlements { get; set; }
}

[GenerateSerializer]
public class SettlementDto
{
    [Id(0)]
    public long Id { get; set; }
    [Id(1)]
    public decimal Amount { get; set; }
    [Id(2)]
    public string CurrencyCode { get; set; }
    [Id(3)]
    public string SrcAccountCode { get; set; }
    [Id(4)]
    public string SrcShardAccountCode { get; set; }
    [Id(5)]
    public string DesAccountCode { get; set; }
    [Id(6)]
    public string DesShardAccountCode { get; set; }
    [Id(7)]
    public decimal SrcAccountBalance { get; set; }
    [Id(8)]
    public decimal DesAccountBalance { get; set; }
    [Id(9)]
    public decimal SrcAccountBalanceBeforeTrans { get; set; }
    [Id(10)]
    public decimal DesAccountBalanceBeforeTrans { get; set; }
    [Id(11)]
    public string TransRef { get; set; }
    [Id(12)]
    public string TransCode { get; set; }
    [Id(13)]
    public SettlementStatus Status { get; set; }
    [Id(14)]
    public DateTime? ModifiedDate { get; set; }
    [Id(15)]
    public DateTime CreatedDate { get; set; }
    [Id(16)]
    public string Description { get; set; }
    [Id(17)]
    public bool ReturnResult { get; set; }
    [Id(18)]
    public string PaymentTransCode { get; set; }//Mã gọi sang từ đối tác
    [Id(19)]
    public TransactionType TransType { get; set; }
    [Id(20)]
    public int Order { get; set; }
}