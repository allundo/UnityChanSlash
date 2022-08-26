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
    private byte[] GetBytes(int size)
    {
        byte[] ret = new byte[size];
        rngCSP.GetBytes(ret);
        return ret;
    }

    public MyAesGcm(byte[] key, byte[] tagHashKey = null)
    {
        this.key = key;
        nonceStore = new NonceStore(tagHashKey ?? key);
    }

    public KeyValuePair<string, string> Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            byte[] nonce = GetBytes(aesAlg.BlockSize / 8);

            aesAlg.Key = key;
            aesAlg.IV = nonce;

            var tag = hash.String(plainText, nonce);
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

    private class NonceStore
    {
        private byte[] key;
        private Dictionary<string, byte[]> nonceStore;
        private SHA256Hash hash = new SHA256Hash();

        public NonceStore(byte[] key, Dictionary<string, byte[]> nonceStore = null)
        {
            this.key = key;
            this.nonceStore = nonceStore ?? new Dictionary<string, byte[]>();
        }

        public byte[] this[string tag]
        {
            get
            {
                byte[] nonce;
                if (!nonceStore.TryGetValue(hash.String(tag, key), out nonce))
                {
                    throw new CryptographicException("復号失敗: 不正タグ");
                }
                return nonce;
            }
            set
            {
                nonceStore[hash.String(tag, key)] = value;
            }
        }
    }

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
}
