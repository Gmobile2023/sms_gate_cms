using System.ComponentModel.DataAnnotations;

namespace SmsGateCms.Models.AccountViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}