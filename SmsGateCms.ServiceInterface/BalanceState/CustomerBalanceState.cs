namespace SmsGateCms.ServiceInterface.BalanceState;

public class CustomerBalanceState
{
    public long CustomerId { get; set; }
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; }
}