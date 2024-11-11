namespace SmsGate.Shared.Contract.Command
{
    public interface IBalancePaymentCommand : ICommand
    {
        public string AccountCode { get; set; }
        public decimal PaymentAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string TransRef { get; set; }
        public string TransCode { get; set; }
        public string Description { get; set; }
        public string MerchantCode { get; set; }
        public string TransNote { get; set; }
        public decimal Fee { get; set; }
    }
}