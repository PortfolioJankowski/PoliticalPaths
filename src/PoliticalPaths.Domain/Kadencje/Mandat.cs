using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.StartyWyborcze;

namespace PoliticalPaths.Domain.Kadencje;

public sealed class Mandat
{
    public Guid Id { get; set; }
    public Guid PolitykId { get; set; }
    public Polityk Polityk { get; set; }
    public Guid StartWyborczyId { get; set; }
    public StartWyborczy StartWyborczy { get; set; } = default!;
    public DateOnly DataOd { get; set; }
    public DateOnly? DataDo { get; set; }
    
    public StatusMandatu Status { get; set; }
    public TypObjeciaMandatu TypObjecia { get; set; }

    public ICollection<ZdarzenieMandatowe> Zdarzenia { get; set; } = new List<ZdarzenieMandatowe>();
}
