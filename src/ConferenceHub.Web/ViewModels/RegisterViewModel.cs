using System.ComponentModel.DataAnnotations;

namespace ConferenceHub.Web.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "User name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "User name must be between 2 and 50 characters")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;

}
