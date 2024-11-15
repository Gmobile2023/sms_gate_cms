namespace SmsGate.Shared.Contract.Command
{
    public interface IPaymentCancelCommand : ICommand
    {
        string TransCode { get; set; }
        string PaymentTransCode { get; set; }
        string AccountCode { get; }
        decimal RevertAmount { get; }
        string TransRef { get; }
        string TransNote { get; set; }
        string CurrencyCode { get; set; }
    }
}