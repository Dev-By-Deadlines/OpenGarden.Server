using System.Security.Cryptography;
using System.Text;

namespace OpenGarden.Server.Utils;

public static class RefreshTokenHasher
{
    public static string Hash(string token)
    {
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
