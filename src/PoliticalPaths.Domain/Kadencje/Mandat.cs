using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Domain.Kadencje;

public sealed class Mandat
{
    public Guid Id { get; set; }
    public Guid PolitykId { get; set; }
    public Guid StartWyborczyId { get; set; }
    public Guid WyboryId { get; set; }
    
    public DateOnly DataOd { get; set; }
    public DateOnly? DataDo { get; set; }
    
    public StatusMandatu Status { get; set; }
    public TypObjeciaMandatu TypObjecia { get; set; }

    public ICollection<ZdarzenieMandatowe> Zdarzenia { get; set; } = new List<ZdarzenieMandatowe>();
}
