using System;

namespace SmsGate.Balance.Models.Dtos;

public class CurrencyDto
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime? ModifiedDate { get; set; }
}