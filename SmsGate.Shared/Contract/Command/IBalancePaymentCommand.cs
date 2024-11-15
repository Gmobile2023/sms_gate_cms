namespace SmsGate.Shared.Contract.Command
{
    public interface IBalancePaymentCommand : ICommand
    {
        string AccountCode { get; set; }
        decimal PaymentAmount { get; set; }
        string CurrencyCode { get; set; }
        string TransRef { get; set; }
        string TransCode { get; set; }
        string Description { get; set; }
        string MerchantCode { get; set; }
        string TransNote { get; set; }
        decimal Fee { get; set; }
    }
}