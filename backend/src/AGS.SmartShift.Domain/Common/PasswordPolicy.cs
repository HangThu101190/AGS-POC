using System.Text.RegularExpressions;

namespace AGS.SmartShift.Domain.Common;

/// <summary>AGS baseline password rules (login + future change-password).</summary>
public static partial class PasswordPolicy
{
    public const int MinLength = 10;
    public const int MaxLength = 128;

    public static bool TryValidate(string? password, out string? error)
    {
        if (string.IsNullOrEmpty(password))
        {
            error = "Password is required.";
            return false;
        }

        if (password.Length < MinLength || password.Length > MaxLength)
        {
            error = $"Password must be {MinLength}–{MaxLength} characters.";
            return false;
        }

        if (!Uppercase().IsMatch(password))
        {
            error = "Password must include at least one uppercase letter.";
            return false;
        }

        if (!Lowercase().IsMatch(password))
        {
            error = "Password must include at least one lowercase letter.";
            return false;
        }

        if (!Digit().IsMatch(password))
        {
            error = "Password must include at least one digit.";
            return false;
        }

        if (!Special().IsMatch(password))
        {
            error = "Password must include at least one special character.";
            return false;
        }

        error = null;
        return true;
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex Uppercase();

    [GeneratedRegex("[a-z]")]
    private static partial Regex Lowercase();

    [GeneratedRegex("[0-9]")]
    private static partial Regex Digit();

    [GeneratedRegex(@"[^A-Za-z0-9]")]
    private static partial Regex Special();
}
