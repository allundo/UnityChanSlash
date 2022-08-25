using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Collections.Generic;
using System;

public class MyAesGcm
{
    private byte[] key;
    private RNGCryptoServiceProvider rngCSP = new RNGCryptoServiceProvider();
    private byte[] GetBytes(int size)
    {
        byte[] ret = new byte[size];
        rngCSP.GetBytes(ret);
        return ret;
    }

    private Dictionary<byte[], byte[]> nonceStore = new Dictionary<byte[], byte[]>();

    public MyAesGcm(byte[] key)
    {
        this.key = key;
    }

    public KeyValuePair<byte[], string> Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            byte[] nonce = GetBytes(aesAlg.BlockSize / 8);

            aesAlg.Key = key;
            aesAlg.IV = nonce;

            var tag = SHA256Hash(Convert.ToBase64String(nonce) + plainText);
            nonceStore[tag] = nonce;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
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

                        return new KeyValuePair<byte[], string>(tag, Convert.ToBase64String(msEncrypt.ToArray()));
                    }
                }
            }
        }
    }

    public string Decrypt(string cipherText, byte[] tag)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = nonceStore[tag];

            // Create a decryptor to perform the stream transform.
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

                            if (!Convert.ToBase64String(tag).Equals(Convert.ToBase64String(SHA256Hash(Convert.ToBase64String(nonceStore[tag]) + plainText))))
                            {
                                throw new CryptographicException("復号失敗");
                            }

                            return plainText;
                        }
                    }
                }
            }
        }
    }

    private byte[] SHA256Hash(string text)
    {
        using (MemoryStream msSHA256 = new MemoryStream())
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                using (StreamWriter swRaw = new StreamWriter(msSHA256))
                {
                    swRaw.Write(text);
                }
            }
            return msSHA256.ToArray();
        }
    }
}