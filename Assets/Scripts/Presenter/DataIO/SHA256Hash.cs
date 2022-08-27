using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

public class SHA256Hash
{
    private static readonly SHA256 sha256 = SHA256.Create();

    public bool Check(byte[] hash, byte[] raw, byte[] key)
        => Convert.ToBase64String(hash).Equals(String(raw, key));

    public bool Check(string hash, string raw, byte[] key)
        => hash.Equals(String(raw, key));

    public byte[] Bytes(byte[] src) => sha256.ComputeHash(src);

    public byte[] Bytes(byte[] src, byte[] key) => Bytes(key.Concat(src).ToArray());

    public byte[] Bytes(string text)
         => sha256.ComputeHash(Encoding.UTF8.GetBytes(text));

    public byte[] Bytes(string text, byte[] key) => Bytes(key.Concat(Encoding.UTF8.GetBytes(text)).ToArray());

    public string String(byte[] src) => Convert.ToBase64String(Bytes(src));
    public string String(byte[] src, byte[] key) => String(key.Concat(src).ToArray());

    public string String(string text) => Convert.ToBase64String(Bytes(text));
    public string String(string text, byte[] key) => String(key.Concat(Encoding.UTF8.GetBytes(text)).ToArray());
}
