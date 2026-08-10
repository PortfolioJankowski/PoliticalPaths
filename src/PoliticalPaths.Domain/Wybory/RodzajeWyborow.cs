using PoliticalPaths.Shared.Enums;

namespace PoliticalPaths.Domain.Wybory;

public sealed class RodzajeWyborow
{
    public Guid Id { get; set; }
    public string Nazwa { get; set; } = default!;
    public PoziomWyborow Poziom { get; set; }
    public ICollection<Wybory> Wybory { get; set; } = new List<Wybory>();
}
