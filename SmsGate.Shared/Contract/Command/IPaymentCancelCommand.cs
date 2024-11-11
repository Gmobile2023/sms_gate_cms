namespace SmsGate.Shared.Contract.Command
{
    public interface IPaymentCancelCommand : ICommand
    {
        public string TransCode { get; set; }
        public string PaymentTransCode { get; set; }
        string AccountCode { get; }
        decimal RevertAmount { get; }
        string TransRef { get; }
        string TransNote { get; set; }
        string CurrencyCode { get; set; }
    }
}