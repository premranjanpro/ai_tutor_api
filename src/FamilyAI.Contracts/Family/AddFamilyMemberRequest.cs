using System;
using System.Collections.Generic;

namespace FamilyAI.Contracts.Family;

public record AddFamilyMemberRequest(
    string FullName,
    string Nickname,
    string Relation,
    string MemberType, // Child, Parent, Adult, Grandparent, Guardian
    DateTime? DateOfBirth,
    string PreferredLanguage,
    string ClassName,
    string SchoolBoard,
    List<string> Interests,
    List<string> LearningGoals
);
