namespace MedicalCenterWeb.Services
{
    public static class CiSearch
    {
        public static bool HasQuery(string? search) => !string.IsNullOrWhiteSpace(search);

        public static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
    }
}
