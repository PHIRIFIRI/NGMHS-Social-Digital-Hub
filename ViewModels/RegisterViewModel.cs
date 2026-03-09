using System.ComponentModel.DataAnnotations;

namespace NGMHS.ViewModels;

public class RegisterViewModel
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
