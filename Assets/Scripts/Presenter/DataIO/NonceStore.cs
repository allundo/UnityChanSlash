using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

public class NonceStore
{
    private byte[] key;
    private byte[] tagHashKey;
    private Dictionary<string, byte[]> nonceStore;

    public KeyValuePair<string, byte[]> GetNanceData(string tag)
    {
        var tagHash = hash.String(tag, tagHashKey);
        return new KeyValuePair<string, byte[]>(tagHash, nonceStore[tagHash]);
    }

    public void SetNanceData(string hash, byte[] nonce)
    {
        nonceStore[hash] = nonce;
    }

    private SHA256Hash hash = new SHA256Hash();

    public NonceStore(byte[] key, byte[] tagHashKey, Dictionary<string, byte[]> nonceStore = null)
    {
        this.key = key;
        this.tagHashKey = tagHashKey;
        this.nonceStore = nonceStore ?? new Dictionary<string, byte[]>();
    }

    public byte[] this[string tag]
    {
        get
        {
            byte[] nonce;

            if (!nonceStore.TryGetValue(hash.String(tag, tagHashKey), out nonce))
            {
                throw new CryptographicException("復号失敗: 不正タグ");
            }
            return Decrypt(nonce);
        }
        set
        {
            nonceStore[hash.String(tag, tagHashKey)] = Encrypt(value);
        }
    }

    public byte[] Encrypt(byte[] src)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = tagHashKey;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(Convert.ToBase64String(src));
                    }
                    return msEncrypt.ToArray();
                }
            }
        }
    }

    public byte[] Decrypt(byte[] src)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = tagHashKey;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(src))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return Convert.FromBase64String(srDecrypt.ReadToEnd());
                    }
                }
            }
        }
    }
}