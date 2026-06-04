using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Domain.Kadencje;

public sealed class Mandat
{
    public Guid Id { get; set; }
    public Guid PolitykId { get; set; }
    public Guid KadencjaId { get; set; }
    public DateOnly DataOd { get; set; }
    public DateOnly? DataDo { get; set; }
    public StatusMandatu Status { get; set; }
}
