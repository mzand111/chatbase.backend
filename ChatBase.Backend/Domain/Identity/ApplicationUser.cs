using Microsoft.AspNetCore.Identity;
using System;

namespace ChatBase.Backend.Domain.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int UserNameChangeLimit { get; set; } = 10;
        public DateTimeOffset? RemoveTime { get; set; }
        public Guid? CurrentProfileImageId { get; set; }

        public string DisplayTitle()
        {
            string s = "";

            if (!String.IsNullOrWhiteSpace(FirstName))
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    s += " ";
                }
                s += FirstName;
            }
            if (!String.IsNullOrWhiteSpace(LastName))
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    s += " ";
                }
                s += LastName;
            }


            return s;
        }

    }
}
