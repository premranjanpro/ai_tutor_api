using System;

namespace FamilyAI.Domain.Entities;

public class FamilyMemory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Guid MemberId { get; set; }
    public string MemoryType { get; set; } = "learning_goal"; // learning_goal, interest
    public string Content { get; set; } = string.Empty;
    public double Importance { get; set; } = 0.5;
    public string Sensitivity { get; set; } = "low"; // low, medium, high
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Expired, Deleted
    public bool RequiresParentApproval { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}
