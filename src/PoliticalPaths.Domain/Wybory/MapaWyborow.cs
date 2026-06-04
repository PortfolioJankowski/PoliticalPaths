using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Domain.Wybory;

public sealed class MapaWyborow
{
    public Guid Id { get; set; }
    public Guid RodzajWyborowId { get; set; }
    public DateOnly? DataOgloszenia { get; set; }
    public DateOnly DataWyborow { get; set; }
    public OrdynacjaWyborcza Ordynacja { get; set; }
}
