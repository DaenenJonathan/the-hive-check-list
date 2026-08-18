using System.Security.Cryptography;

namespace TheHive.Infrastructure.Common;

public static class PasswordGenerator
{
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string All = Uppercase + Lowercase + Digits;

    public static string Generate(int length = 12)
    {
        var chars = new char[length];
        chars[0] = Pick(Uppercase);
        chars[1] = Pick(Lowercase);
        chars[2] = Pick(Digits);
        for (var i = 3; i < length; i++)
            chars[i] = Pick(All);

        // Shuffle so the guaranteed-category characters aren't always in the first 3 positions.
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char Pick(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}
