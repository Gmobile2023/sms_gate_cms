using System.Security.Cryptography;
using System.Text;

namespace SmsGateCms.ServiceModel.Utils;

public static class EncryptUtils
{
    public static string Sha256(this string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}