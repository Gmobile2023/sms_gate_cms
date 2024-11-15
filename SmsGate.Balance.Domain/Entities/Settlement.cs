using System;
using SmsGate.Balance.Models.Enums;

using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using SmsGate.Shared.Common;

namespace SmsGate.Balance.Domain.Entities;

[Schema("public")]
[Alias("settlement")]
[CompositeIndex("TransRef", Name = "s_transRefIdx", Unique = false)]
[CompositeIndex("TransCode", Name = "s_transcode_Idx", Unique = true)]
[CompositeIndex("CurrencyCode", Name = "s_currency_code_Idx", Unique = false)]
[CompositeIndex("SrcAccountCode", Name = "s_src_acc", Unique = false)]
[CompositeIndex("DesAccountCode", Name = "d_des_acc", Unique = false)]
[CompositeIndex("PaymentTransCode", Name = "d_payment_transcode", Unique = false)]
public class Settlement : IHasLongId
{
    [AutoIncrement] [Alias("id")] public long Id { get; set; }
    [Alias("amount")] public decimal Amount { get; set; }

    [StringLength(10)]
    [Alias("currency_code")]
    public string CurrencyCode { get; set; }

    [StringLength(30)]
    [Alias("src_account_code")]
    public string SrcAccountCode { get; set; }
    [StringLength(30)]
    [Alias("src_shard_account_code")]
    public string SrcShardAccountCode { get; set; }

    [StringLength(30)]
    [Alias("des_account_code")]
    public string DesAccountCode { get; set; }
    
    [StringLength(30)]
    [Alias("des_shard_account_code")]
    public string DesShardAccountCode { get; set; }

    [Alias("src_account_balance")] public decimal SrcAccountBalance { get; set; }
    [Alias("des_account_balance")] public decimal DesAccountBalance { get; set; }

    [Alias("src_account_balance_before_trans")]
    public decimal SrcAccountBalanceBeforeTrans { get; set; }

    [Alias("des_account_balance_before_trans")]
    public decimal DesAccountBalanceBeforeTrans { get; set; }

    [Alias("trans_ref")]
    [StringLength(50)]
    public string TransRef { get; set; }

    [StringLength(50)]
    [Alias("trans_code")]
    public string TransCode { get; set; }
    [Ignore]
    public SettlementStatus Status { get; set; }

    [StringLength(150)]
    [Alias("payment_trans_code")]
    public string PaymentTransCode { get; set; } //Mã gọi sang từ đối tác nên để dài chút

    [Alias("created_date")] public DateTime CreatedDate { get; set; }
    [Alias("modified_date")] public DateTime? ModifiedDate { get; set; }
    [Alias("trans_type")] public TransactionType TransType { get; set; }
    [Ignore]
    public int Order { get; set; }
    [Ignore]
    public string Description { get; set; }
    [Ignore]
    public string ReturnResult { get; set; }
}