using System.Text.RegularExpressions;

namespace FactoryQueueSystem.Api.Services;

public static partial class ContactValidation
{
    public const string InvalidEmailMessage = "Geçerli bir e-posta adresi giriniz.";
    public const string InvalidPhoneMessage = "Geçerli bir Türkiye cep telefonu numarası giriniz.";
    public const string InvalidPlateMessage = "Geçerli bir Türkiye plakası giriniz.";
    public const string MissingContactMessage = "E-posta veya telefon numarasından en az biri girilmelidir.";

    public static string? NormalizeEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    public static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim()
            .Replace(" ", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace("-", string.Empty);

        if (normalized.StartsWith("+90", StringComparison.Ordinal))
        {
            return normalized;
        }

        if (normalized.StartsWith("90", StringComparison.Ordinal))
        {
            return $"+{normalized}";
        }

        if (normalized.Length == 11 && normalized.StartsWith("05", StringComparison.Ordinal))
        {
            return $"+90{normalized[1..]}";
        }

        return normalized.Length == 10 && normalized.StartsWith('5')
            ? $"+90{normalized}"
            : normalized;
    }

    public static string? ToPhoneLocalPart(string? value)
    {
        var normalized = NormalizePhone(value);
        return normalized != null && IsValidPhone(normalized) ? normalized[3..] : normalized;
    }

    public static bool IsValidEmail(string email) => EmailRegex().IsMatch(email);

    public static bool IsValidPhone(string? phone) =>
        phone is { Length: 13 } &&
        phone.StartsWith("+905", StringComparison.Ordinal) &&
        phone[1..].All(char.IsDigit);

    public static string? NormalizePlate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToUpperInvariant();
    }

    public static bool IsValidPlate(string? plate)
    {
        if (plate == null)
        {
            return false;
        }

        var match = PlateRegex().Match(plate);
        if (!match.Success || !int.TryParse(match.Groups["province"].Value, out var province))
        {
            return false;
        }

        return province is >= 1 and <= 81;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^(?<province>\d{2})[A-ZÇĞİÖŞÜ]{1,3}\d{2,5}$", RegexOptions.CultureInvariant)]
    private static partial Regex PlateRegex();
}
