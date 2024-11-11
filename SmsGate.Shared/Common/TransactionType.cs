using ServiceStack.DataAnnotations;

namespace SmsGate.Shared.Common;

[EnumAsInt]
public enum TransactionType
{
    Default = 0,
    Transfer = 1,
    Deposit = 2,
    Cashout = 3,
    Payment = 4,
    Revert = 5,
    MasterTopup = 6,
    SystemTransfer = 7,
    MasterTopdown = 8,
    CorrectUp = 9,
    CorrectDown = 10,
    Block = 11,
    Unblock = 12,
    Fee = 13,
    Exchange = 14,
    CancelPayment = 15,
    AdjustmentDecrease = 15,
    AdjustmentIncrease = 16
}