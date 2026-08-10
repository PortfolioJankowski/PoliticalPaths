using PoliticalPaths.Shared.Enums;

namespace PoliticalPaths.Shared.Dtos.Domain;

public sealed record WyboryDto
{
    public Guid Id { get; init; }
    public Guid RodzajWyborowId { get; init; }
    public RodzajeWyborowDto? Rodzaj { get; init; }
    public DateOnly? DataOgloszenia { get; init; }
    public DateOnly DataWyborow { get; init; }
    public OrdynacjaWyborcza Ordynacja { get; init; }
    public TuraWyborow? Tura { get; init; }
    public bool CzyPrzedterminowe { get; init; }
    public string? Kadencja { get; init; }
}
