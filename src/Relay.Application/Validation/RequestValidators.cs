using System.Text.RegularExpressions;
using Relay.Application.Contracts;

namespace Relay.Application.Validation;

public static class RequestValidators
{
    private static readonly Regex Email = new("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<string> Validate(RegisterRequest request)
    {
        var errors = new List<string>();
        if (!Email.IsMatch(request.Email.Trim())) errors.Add("A valid email address is required.");
        if (request.DisplayName.Trim().Length is < 2 or > 120) errors.Add("Display name must be between 2 and 120 characters.");
        if (!IsStrongPassword(request.Password)) errors.Add("Password must be at least 12 characters and include upper, lower, number, and symbol characters.");
        return errors;
    }

    public static IReadOnlyList<string> Validate(LoginRequest request) => Email.IsMatch(request.Email.Trim()) && !string.IsNullOrWhiteSpace(request.Password) ? [] : ["Email and password are required."];

    public static IReadOnlyList<string> Validate(CreateCampaignRequest request)
    {
        var errors = ValidateText(request.Name, "Campaign name", 2, 160);
        errors.AddRange(ValidateText(request.AdvertiserName, "Advertiser name", 2, 200));
        errors.AddRange(ValidateText(request.Description, "Description", 1, 4000));
        errors.AddRange(ValidateText(request.Advertisement, "Advertisement", 1, 4000));
        errors.AddRange(ValidateText(request.Category, "Category", 1, 120));
        errors.AddRange(ValidateText(request.Location, "Location", 1, 180));
        if (!Uri.TryCreate(request.DestinationUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) errors.Add("Destination URL must be a valid HTTP or HTTPS URL.");
        if (request.Platforms.Length == 0) errors.Add("At least one target platform is required.");
        if (request.MinimumAudience <= 0 || request.MaximumAudience < request.MinimumAudience) errors.Add("Audience bounds are invalid.");
        if (request.DurationDays <= 0) errors.Add("Campaign duration must be greater than zero.");
        if (request.EndDate < request.StartDate) errors.Add("Campaign end date must not precede its start date.");
        if (request.Cpc <= 0) errors.Add("CPC must be greater than zero.");
        if (request.Budget <= 0) errors.Add("Budget must be greater than zero.");
        if (request.MaximumCommunities <= 0) errors.Add("Maximum communities must be greater than zero.");
        if (request.Cpc * request.MaximumCommunities > request.Budget) errors.Add("Campaign budget is too low for the specified community limit and CPC.");
        return errors;
    }

    public static IReadOnlyList<string> Validate(CreateCommunityRequest request)
    {
        var errors = ValidateText(request.Name, "Community name", 2, 160);
        errors.AddRange(ValidateText(request.Category, "Category", 1, 120));
        errors.AddRange(ValidateText(request.Location, "Location", 1, 180));
        errors.AddRange(ValidateText(request.AudienceDescription, "Audience description", 1, 4000));
        if (request.Members <= 0) errors.Add("Audience size must be greater than zero.");
        if (!string.IsNullOrWhiteSpace(request.CommunityLink) && (!Uri.TryCreate(request.CommunityLink, UriKind.Absolute, out var link) || link.Scheme is not ("http" or "https"))) errors.Add("Community link must be a valid HTTP or HTTPS URL.");
        return errors;
    }

    private static readonly Regex Phone = new("^[0-9+()\\-\\s]{7,32}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<string> Validate(SaveProfileRequest request)
    {
        var errors = new List<string>();
        errors.AddRange(ValidateOptionalText(request.BusinessName, "Business name", 160));
        errors.AddRange(ValidateOptionalText(request.Industry, "Industry", 120));
        errors.AddRange(ValidateOptionalText(request.Location, "Location", 180));
        errors.AddRange(ValidateOptionalText(request.PrimaryGoal, "Primary goal", 240));
        errors.AddRange(ValidateOptionalText(request.AvatarKey, "Avatar", 300));
        errors.AddRange(ValidateOptionalText(request.CommunityName, "Community name", 160));
        errors.AddRange(ValidateOptionalText(request.CommunityPlatform, "Community platform", 40));
        errors.AddRange(ValidateOptionalText(request.CommunityCategory, "Community category", 120));
        if (!string.IsNullOrWhiteSpace(request.Website) && !IsAcceptableWebsite(request.Website.Trim())) errors.Add("Website must be a valid HTTP or HTTPS address.");
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !Phone.IsMatch(request.PhoneNumber.Trim())) errors.Add("Phone number may contain only digits, spaces, +, ( ), and - and must be 7 to 32 characters.");
        if (request.CommunityMembers is < 0 or > 2_000_000_000) errors.Add("Community size is out of range.");
        return errors;
    }

    private static List<string> ValidateOptionalText(string? value, string label, int max) => value is not null && value.Trim().Length > max ? [$"{label} must be at most {max} characters."] : [];

    private static bool IsAcceptableWebsite(string value)
    {
        if (value.Length > 300 || value.Any(char.IsWhiteSpace)) return false;
        if (!value.Contains(':')) return true;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is ("http" or "https");
    }

    private static List<string> ValidateText(string value, string label, int min, int max) => value.Trim().Length < min || value.Trim().Length > max ? [$"{label} must be between {min} and {max} characters."] : [];
    private static bool IsStrongPassword(string password) => password.Length >= 12 && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit) && password.Any(ch => !char.IsLetterOrDigit(ch));
}
