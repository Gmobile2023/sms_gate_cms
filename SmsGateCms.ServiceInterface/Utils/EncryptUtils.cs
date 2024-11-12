using System.Security.Cryptography;
using System.Text;

namespace Sms.ServiceInterface.Utils;

public class EncryptUtils
{
    public static string Sha256(string rawData)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        StringBuilder builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}