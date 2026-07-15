using System;
using System.Collections.Generic;

namespace FamilyAI.Contracts.Family;

public record FamilyMemberResponse(
    Guid Id,
    Guid FamilyId,
    string FullName,
    string Nickname,
    string Relation,
    string MemberType,
    DateTime? DateOfBirth,
    int Age,
    string PreferredLanguage,
    string ClassName,
    string SchoolBoard,
    List<string> Interests,
    List<string> LearningGoals
);
