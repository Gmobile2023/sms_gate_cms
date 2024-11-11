﻿using ServiceStack.DataAnnotations;

namespace SmsGate.Balance.Models.Enums;

[EnumAsInt]
public enum BalanceStatus
{
    Active = 1,
    Init = 0,
    Locked = 2,
    Deleted = 3
}