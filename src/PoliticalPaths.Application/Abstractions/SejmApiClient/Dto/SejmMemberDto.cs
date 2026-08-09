using System.Text.Json.Serialization;

namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

public record SejmMemberDto(
    [property: JsonPropertyName("accusativeName")] string AccusativeName,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("birthDate")] DateOnly BirthDate,
    [property: JsonPropertyName("birthLocation")] string BirthLocation,
    [property: JsonPropertyName("club")] string Club,
    [property: JsonPropertyName("districtName")] string DistrictName,
    [property: JsonPropertyName("districtNum")] int DistrictNum,
    [property: JsonPropertyName("educationLevel")] string EducationLevel,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("firstLastName")] string FirstLastName,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("genitiveName")] string GenitiveName,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("lastFirstName")] string LastFirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("numberOfVotes")] int NumberOfVotes,
    [property: JsonPropertyName("profession")] string Profession,
    [property: JsonPropertyName("secondName")] string SecondName,
    [property: JsonPropertyName("voivodeship")] string Voivodeship,
    [property: JsonPropertyName("inactiveCause")] string InactiveCause,
    [property: JsonPropertyName("waiverDesc")] string InactiveReason
);