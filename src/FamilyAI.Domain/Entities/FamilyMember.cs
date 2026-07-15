using System;
using FamilyAI.Domain.Enums;

namespace FamilyAI.Domain.Entities;

public class FamilyMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Guid? UserId { get; set; } // Nullable if child or member without a separate login account
    public string FullName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty; // e.g. Son, Daughter, Father, Mother
    public MemberType MemberType { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string PreferredLanguage { get; set; } = "English";
    
    // Child specific fields
    public string ClassName { get; set; } = string.Empty;
    public string SchoolBoard { get; set; } = string.Empty;
    
    // Serialized JSON metadata for flexibility
    public string InterestsJson { get; set; } = "[]"; 
    public string LearningGoalsJson { get; set; } = "[]";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Family? Family { get; set; }

    // Helper property to calculate age
    public int Age
    {
        get
        {
            if (DateOfBirth == null) return 0;
            var today = DateTime.UtcNow.Date;
            var age = today.Year - DateOfBirth.Value.Year;
            if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
