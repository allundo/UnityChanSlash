using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

public class SHA256Hash
{
    public bool Check(byte[] hash, byte[] raw, byte[] key)
        => Convert.ToBase64String(hash).Equals(String(raw, key));

    public bool Check(string hash, string raw, byte[] key)
        => hash.Equals(String(raw, key));

    public byte[] Bytes(byte[] src)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            using (MemoryStream msSHA256 = new MemoryStream(src))
            {
                return sha256.ComputeHash(msSHA256);
            }
        }
    }

    public byte[] Bytes(byte[] src, byte[] key) => Bytes(key.Concat(src).ToArray());

    public byte[] Bytes(string text)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            using (MemoryStream msSHA256 = new MemoryStream())
            {
                using (StreamWriter swRaw = new StreamWriter(msSHA256))
                {
                    swRaw.Write(text);

                    msSHA256.Position = 0;
                    return sha256.ComputeHash(msSHA256);
                }
            }
        }
    }

    public byte[] Bytes(string text, byte[] key) => Bytes(Convert.ToBase64String(key) + text);

    public string String(byte[] src) => Convert.ToBase64String(Bytes(src));
    public string String(byte[] src, byte[] key) => String(key.Concat(src).ToArray());

    public string String(string text) => Convert.ToBase64String(Bytes(text));
    public string String(string text, byte[] key) => String(Convert.ToBase64String(key) + text);

}