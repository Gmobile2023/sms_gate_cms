namespace SmsGate.Shared.Contract.Command
{
    public interface ICheckBalanceCommand : ICommand
    {
        string AccountCode { get; set; }
        string CurrencyCode { get; set; }
    }
}