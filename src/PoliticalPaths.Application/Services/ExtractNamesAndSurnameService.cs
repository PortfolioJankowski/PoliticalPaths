namespace PoliticalPaths.Application.Services;
public static class ExtractNamesAndSurnameService
{
    public static NamesSurnameDto Extract(string excelValue, NameExtractingOptions options)
    {
        var split = excelValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (split.Length < 2)
        {
            throw new InvalidOperationException(
                $"Missing politician data: '{excelValue}'");
        }

        if (split.Length == 2)
        {
            return new NamesSurnameDto(
                split[options.NameIndex],
                "Nieznane",
                split[options.NameIndex == 0 ? 1 : 0]);
        }

        if (split.Length <= options.SurnameIndex)
        {
            throw new InvalidOperationException(
                $"Invalid name format: '{excelValue}'");
        }

        return new NamesSurnameDto(
            split[options.NameIndex],
            split[options.SecondNameIndex],
            split[options.SurnameIndex]);
    }
}

public record NamesSurnameDto(string Name, string SecondName, string Surname);

public struct NameExtractingOptions(int name, int secondName, int surname)
{
    public int NameIndex { get; set; } = name;
    public int SurnameIndex { get; set; } = surname;
    public int SecondNameIndex { get; set; } = secondName;

    public static NameExtractingOptions GetDefault() => new NameExtractingOptions(0, 1, 2);
}