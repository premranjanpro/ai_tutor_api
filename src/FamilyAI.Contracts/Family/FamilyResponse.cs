using System;
using System.Collections.Generic;

namespace FamilyAI.Contracts.Family;

public record FamilyResponse(
    Guid Id,
    string FamilyName,
    string PrimaryLanguage,
    string SecondaryLanguage,
    string Country,
    string TimeZone,
    List<FamilyMemberResponse> Members
);
