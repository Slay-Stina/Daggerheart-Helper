using Core.Enums;

namespace Srd.Ingestion.Domain;

public sealed record AbilityCard(
    string Name,
    DomainType Domain,
    int Level,
    int RecallCost,
    AbilityType Type,
    string Text);


