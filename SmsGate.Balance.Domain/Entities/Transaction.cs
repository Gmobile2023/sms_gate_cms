using System;
using System.Collections.Generic;
using SmsGate.Balance.Models.Enums;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using SmsGate.Shared.Common;

namespace SmsGate.Balance.Domain.Entities;

[Schema("public")]
[Alias("transaction")]
[CompositeIndex("TransactionCode", Name = "TransactionCodeIdx", Unique = true)]
[CompositeIndex("TransRef", Name = "TransRefIdx", Unique = false)]
[CompositeIndex("RevertTransCode", Name = "RevertTransCodeIdx", Unique = false)]
public class Transaction : IHasLongId
{
    [AutoIncrement] [Alias("id")] public long Id { get; set; }

    [StringLength(50)]
    [Alias("transaction_code")]
    public string TransactionCode { get; set; }

    [StringLength(30)]
    [Alias("src_account_code")]
    public string SrcAccountCode { get; set; }

    [StringLength(30)]
    [Alias("des_account_code")]
    public string DesAccountCode { get; set; }

    [StringLength(50)]
    [Alias("trans_ref")]
    public string TransRef { get; set; }

    [Alias("amount")] public decimal Amount { get; set; }
    [Alias("fee")] public decimal Fee { get; set; }

    [Alias("currency_code")]
    [StringLength(10)]
    public string CurrencyCode { get; set; }

    [Alias("trans_type")] public TransactionType TransType { get; set; }
    [Alias("status")] public TransStatus Status { get; set; }
    [Alias("modified_date")] public DateTime? ModifiedDate { get; set; }
    [Alias("created_date")] public DateTime? CreatedDate { get; set; }

    [StringLength(30)]
    [Alias("modified_by")]
    public string ModifiedBy { get; set; }

    [StringLength(30)]
    [Alias("created_by")]
    public string CreatedBy { get; set; }

    [StringLength(200)]
    [Alias("description")]
    public string Description { get; set; }

    [StringLength(50)]
    [Alias("revert_trans_code")]
    public string RevertTransCode { get; set; }

    [StringLength(200)]
    [Alias("trans_note")]
    public string TransNote { get; set; }
    [Ignore]
    public List<Settlement> Settlements { get; set; }
}