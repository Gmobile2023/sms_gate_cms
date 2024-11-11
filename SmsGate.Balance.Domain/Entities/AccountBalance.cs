using System;
using SmsGate.Balance.Models.Enums;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using SmsGate.Shared.Utils;

namespace SmsGate.Balance.Domain.Entities;

[Schema("public")]
[Alias("account_balance")]
[CompositeIndex("AccountCode", "CurrencyCode", "Status", Name = "AccountIdx", Unique = true)]
[CompositeIndex("CurrencyCode", Name = "a_currency_code_Idx", Unique = false)]
public class AccountBalance : IHasLongId
{
    [AutoIncrement]
    [Alias("id")]
    public long Id { get; set; }
    [Computed]
    [Ignore]
    public decimal? AvailableBalance => Balance + LimitOverDraft - MinBalance - BlockedMoney;
    [Alias("limit_over_draft")]
    public decimal LimitOverDraft { get; set; } = 0;

    [Alias("min_balance")]
    public decimal MinBalance { get; set; } = 0;

    [Alias("blocked_money")]
    public decimal BlockedMoney { get; set; } = 0;

    [StringLength(30)]
    [Alias("account_code")]
    public string AccountCode { get; set; }
    [StringLength(10)]
    [Alias("currency_code")]
    public string CurrencyCode { get; set; }
    [Alias("balance")]
    public decimal Balance { get; set; } = 0;

    [StringLength(100)]
    [Alias("last_trans_code")]
    public string LastTransCode { get; set; } = "Init";

    [Alias("status")]
    public BalanceStatus Status { get; set; } = BalanceStatus.Active;

    [StringLength(500)]
    [Alias("check_sum")]
    public string CheckSum { get; set; }
    [Alias("modified_date")]
    public DateTime? ModifiedDate { get; set; }
    [Alias("account_type")]
    [StringLength(50)]
    public string AccountType { get; set; }
    [Alias("shard_counter")]
    public int ShardCounter { get; set; }

    public string ToCheckSum()
    {
        var plantText =
            $"{AccountCode}{CurrencyCode}{Balance:0.0000}{LastTransCode}3468897034623418";
        return plantText.HashSHA256();
    }

    public bool IsValid()
    {
        try
        {
            //return true;
            return String.Equals(ToCheckSum(), CheckSum, StringComparison.CurrentCultureIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }
}