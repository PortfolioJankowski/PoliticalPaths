namespace PoliticalPaths.Domain.Enums;

public enum ElectoralChamber
{
    Sejm = 0,
    Senate = 1,
    RegionalAssembly = 2
}

public enum ElectionScope
{
    National = 0,
    Voivodeship = 1,
    European = 2
}

public enum ElectionProfile
{
    SejmProportional = 0,
    SenateMajoritarian = 1,
    RegionalAssemblyProportional = 2,
    Presidential = 3,
    EuropeanParliament = 4
}

public enum ElectionKind
{
    General = 0,
    Supplementary = 1,
    Repeat = 2
}

public enum ElectoralCommitteeType
{
    Party = 0,
    Coalition = 1,
    VotersCommittee = 2,
    Other = 3
}

public enum TerritorialUnitLevel
{
    Voivodeship = 0,
    Powiat = 1,
    Gmina = 2,
    City = 3,
    Other = 4
}

public enum TerritoryCoverageType
{
    Primary = 0,
    Partial = 1,
    Excluded = 2
}

public enum CollegialBodyType
{
    Sejm = 0,
    Senate = 1,
    RegionalAssembly = 2
}

public enum MandateAcquisitionType
{
    InitialElection = 0,
    SubstituteFromList = 1,
    SupplementaryElection = 2,
    Other = 3
}

public enum MandateStatus
{
    Active = 0,
    Terminated = 1,
    NeverAssumed = 2,
    RenouncedBeforeStart = 3
}

public enum MandateTerminationReason
{
    Death = 0,
    LossOfEligibility = 1,
    TribunalStrippedMandate = 2,
    Resignation = 3,
    IncompatibleOfficeHeld = 4,
    BecamePresident = 5,
    IncompatibleAppointment = 6,
    ElectedToEuropeanParliament = 7,
    RefusedOath = 8,
    Unknown = 9
}

public enum MandateEventType
{
    MandateAllocated = 0,
    OathTaken = 1,
    TerminationDeclared = 2,
    SubstituteNotified = 3,
    SubstituteAccepted = 4,
    SubstituteDeclined = 5,
    SupplementaryElectionCalled = 6,
    ManualCorrection = 7
}

public enum IdentityMatchStatus
{
    Pending = 0,
    Confirmed = 1,
    Rejected = 2,
    NeedsManualReview = 3
}

public enum ManualMappingCategory
{
    Territory = 0,
    ElectoralDistrict = 1,
    Politician = 2,
    ElectoralCommittee = 3,
    Party = 4,
    Other = 5
}
