using System.ComponentModel.DataAnnotations;

namespace ChatBase.Backend.Domain.Identity.Dto
{
    public class ServiceLoginModel
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
