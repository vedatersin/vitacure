using Microsoft.AspNetCore.Identity;

namespace vitacure.Domain.Entities;

public class AppRole : IdentityRole<int>
{
    public bool IsBackOfficeRole { get; set; }
}
