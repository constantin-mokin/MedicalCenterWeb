using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;

namespace MedicalCenterWeb.Services
{
    public class AuthenticationService
    {
        private readonly ApplicationDbContext _context;

        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username
                                       && u.Password == password
                                       && u.IsActive);

            if (user != null)
            {
                user.LastLogin = AppClock.Now;
                await _context.SaveChangesAsync();
            }

            return user;
        }

        public static string GenerateRandomPassword(int length = 10)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string all = upper + lower + digits;

            if (length < 4)
                length = 4;

            var chars = new char[length];
            chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            for (var i = 3; i < length; i++)
                chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

            for (var i = length - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }

        public async Task<string> GenerateUniqueUsernameAsync(string firstName, string lastName)
        {
            var baseName = $"{Transliterate(firstName)}.{Transliterate(lastName)}".Replace(" ", "");
            var username = baseName;
            var i = 1;
            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                username = $"{baseName}{i}";
                i++;
            }
            return username;
        }

        private static string Transliterate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var map = new Dictionary<char, string>
            {
                ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d",
                ['е'] = "e", ['ё'] = "e", ['ж'] = "zh", ['з'] = "z", ['и'] = "i",
                ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m", ['н'] = "n",
                ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t",
                ['у'] = "u", ['ф'] = "f", ['х'] = "h", ['ц'] = "ts", ['ч'] = "ch",
                ['ш'] = "sh", ['щ'] = "sch", ['ъ'] = "", ['ы'] = "y", ['ь'] = "",
                ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
                ['і'] = "i", ['ў'] = "u", ['’'] = ""
            };

            var sb = new System.Text.StringBuilder(text.Length);
            foreach (var ch in text.Trim().ToLowerInvariant())
            {
                sb.Append(map.TryGetValue(ch, out var replacement) ? replacement : ch.ToString());
            }

            return sb.ToString();
        }
    }
}
