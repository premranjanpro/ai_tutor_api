using System;
using System.Collections.Generic;

namespace FamilyAI.Domain.Entities;

public class Family
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FamilyName { get; set; } = string.Empty;
    public string PrimaryLanguage { get; set; } = "English";
    public string SecondaryLanguage { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public string ParentCustomInstructions { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FamilyMember> Members { get; set; } = new List<FamilyMember>();
}
