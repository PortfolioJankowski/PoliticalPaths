namespace PoliticalPaths.Domain.Formacje;

/// <summary>
/// Klub, do którego nale¿¹ pos³owie. Mo¿e byæ to klub parlamentarny, ale te¿ klub w ramach partii politycznej, np. klub PiS, klub PO, klub Lewicy itp.
/// </summary>
public sealed class Kluby
{
    public Guid Id { get; set; }
    public string Nazwa { get; set; } = default!;
    public string? Skrot { get; set; }
}
