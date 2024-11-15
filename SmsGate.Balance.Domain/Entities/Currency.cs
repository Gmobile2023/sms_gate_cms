using System;
using MongoDbGenericRepository.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace SmsGate.Balance.Domain.Entities;

[Schema("public")]
[Alias("currency")]
public class Currency : IHasId<int>
{
    [AutoIncrement]
    [Alias("id")]
    public int Id { get; set; }
    [StringLength(10)]
    [Alias("currency_code")]
    public string CurrencyCode { get; set; }
    [Alias("modified_date")]
    public DateTime? ModifiedDate { get; set; }
}