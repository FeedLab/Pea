using System.Security.Cryptography;

namespace Pea.Meter.Helper;

public interface IEncryptionHelper
{
    public string Encrypt(string plainText);
    public string Decrypt(string cipherText);
}

public class EncryptionHelper : IEncryptionHelper
{
    private static readonly byte[] key = "i9uygf3lmna46tfgvb3e6qpmbaxuty2w"u8.ToArray();
    private static readonly byte[] iv = "iopqwertyuq1234u"u8.ToArray();

    public string Encrypt(string plainText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new();
        using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            using StreamWriter swEncrypt = new(csEncrypt);
            swEncrypt.Write(plainText);
        }
        
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new(Convert.FromBase64String(cipherText));
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt);
        
        return srDecrypt.ReadToEnd();
    }
}