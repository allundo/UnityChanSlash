using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Collections.Generic;
using System;
using System.Linq;

public class MyAesGcm
{
    private byte[] key;
    private SHA256Hash hash = new SHA256Hash();
    private NonceStore nonceStore;

    private RNGCryptoServiceProvider rngCSP = new RNGCryptoServiceProvider();
    public byte[] GetBytes(int size)
    {
        byte[] ret = new byte[size];
        rngCSP.GetBytes(ret);
        return ret;
    }

    public MyAesGcm(byte[] key, NonceStore nonceStore = null)
    {
        this.key = key;
        this.nonceStore = nonceStore ?? new NonceStore(key, key.Take(key.Length / 2).ToArray());
    }

    public KeyValuePair<string, string> Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            byte[] nonce = GetBytes(key.Length / 2);

            aesAlg.Key = key;
            aesAlg.IV = nonce;

            var tag = hash.String(plainText, nonce);
            nonceStore[tag] = nonce;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (var gzipEncrypt = new GZipStream(msEncrypt, CompressionMode.Compress))
                {
                    using (CryptoStream csEncrypt = new CryptoStream(gzipEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }

                        return new KeyValuePair<string, string>(tag, Convert.ToBase64String(msEncrypt.ToArray()));
                    }
                }
            }
        }
    }

    public string Decrypt(string cipherText, string tag)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = nonceStore[tag];

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (var gzipDecrypt = new GZipStream(msDecrypt, CompressionMode.Decompress))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(gzipDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            var plainText = srDecrypt.ReadToEnd();

                            if (!hash.Check(tag, plainText, aesAlg.IV))
                            {
                                throw new CryptographicException("復号失敗: 改ざん疑惑");
                            }

                            return plainText;
                        }
                    }
                }
            }
        }
    }
}
