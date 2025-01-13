using System;

namespace ChatBase.Backend.Domain.Identity.Dto
{
    public class AuthorizedUserResponse
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string UserName { get; set; }
        public string RoleNames { get; set; }
        public string? PhoneNumber { get; set; }
        public string Token { get; set; }
        public DateTime TokenValidTo { get; set; }
    }
}
