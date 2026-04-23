using Core.Enums;
using DomainEnum = Core.Enums.Domain;

namespace Srd.Ingestion.Domain;

public sealed record AbilityCard(
    string Name,
    DomainEnum Domain,
    int Level,
    int RecallCost,
    AbilityType Type,
    string Text);


