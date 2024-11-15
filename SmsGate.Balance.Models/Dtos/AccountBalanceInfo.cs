namespace SmsGate.Balance.Models.Dtos;

public class AccountBalanceInfo
{
    public decimal AvailableBalance { get; set; }
    public decimal Balance { get; set; }
    public decimal BlockedMoney { get; set; }
}