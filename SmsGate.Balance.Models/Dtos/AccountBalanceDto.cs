using System;
using System.Collections.Generic;
using Orleans;
using SmsGate.Balance.Models.Enums;
using SmsGate.Shared.Utils;

namespace SmsGate.Balance.Models.Dtos;

[GenerateSerializer]
public class AccountBalanceDto
{
    [Id(0)]
    public long Id { get; set; }
    [Id(1)]
    public decimal AvailableBalance
    {
        get => Balance + LimitOverDraft - MinBalance - BlockedMoney;
        set {}
    }

    [Id(2)]
    public decimal LimitOverDraft { get; set; } = 0;

    [Id(3)]
    public decimal MinBalance { get; set; } = 0;

    [Id(4)]
    public decimal BlockedMoney { get; set; } = 0;

    [Id(5)]
    public string AccountCode { get; set; }
    [Id(6)]
    public string CurrencyCode { get; set; }
    [Id(7)]
    public decimal Balance { get; set; } = 0;

    [Id(8)]
    public string LastTransCode { get; set; } = "Init";

    [Id(9)]
    public BalanceStatus Status { get; set; } = BalanceStatus.Active;

    [Id(10)]
    public DateTime? ModifiedDate { get; set; }
    [Id(11)]
    public string AccountType { get; set; }
    [Id(12)]
    public string CheckSum { get; set; }
    // [Id(13)]
    // public HashSet<RecentTrans> RecentTrans { get; set; } = new();

    [Id(13)]
    public Queue<string> ShardQueue { get; set; } = new();

    [Id(14)] public ushort ShardCounter { get; set; } = 0;
    [Id(15)] public ushort CurrentShardCounter { get; set; } = 0;

    public string ToCheckSum()
    {
        //chỗ này check sum thêm BlockedMoney,LimitOverDraft,MinBalance
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

[GenerateSerializer]
public class RecentTrans
{
    [Id(0)]
    public DateTime CreatedDate { get; set; }
    [Id(1)]
    public decimal Amount { get; set; }
    [Id(2)]
    public string TransCode { get; set; }
}