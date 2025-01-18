using ChatBase.Backend.Helper;
using System.ComponentModel.DataAnnotations;

namespace ChatBase.Backend.Domain.Identity.Dto;

public class ProfileUserDto
{
    public string FirstName { get; set; }
    [MaxLength(100)]
    public string LastName { get; set; }
    [NullablePhoneAttribute]
    [MaxLength(100)]
    public string? PhoneNumber { get; set; }

}