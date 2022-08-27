using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

public class CryptoTest
{
    [Test]
    /// <summary>
    /// string encryption testing
    /// </summary>
    public void _001_MyAesGcmEncryptTest()
    {
        // setup
        var nonceStore = new NonceStore(Encoding.UTF8.GetBytes("32byteSizeNonceKeyForUnitTesting"), Encoding.UTF8.GetBytes("16byteTagHashKey"));
        var aesGcm = new MyAesGcm(Encoding.UTF8.GetBytes(@"<5s=-%s'2faHjUWs>?sd#aoY#f1GUfEa"), nonceStore);
        var hexValueStr = "F0458";

        // when
        var encrypted = aesGcm.Encrypt(hexValueStr);
        var encrypted2 = aesGcm.Encrypt(hexValueStr);
        var extractedStr = aesGcm.Decrypt(encrypted.Value, encrypted.Key);

        var fabricated = Convert.ToBase64String(Convert.FromBase64String(encrypted.Value).Select(value => (byte)(value / 2)).ToArray());
        var exception = Assert.Throws<CryptographicException>(() => aesGcm.Decrypt(fabricated, encrypted.Key));
        var exception2 = Assert.Throws<CryptographicException>(() => aesGcm.Decrypt(encrypted.Value, nonceStore.GetNanceData(encrypted.Key).Key));

        // then
        Assert.AreEqual(hexValueStr, extractedStr);
        Assert.AreNotEqual(encrypted.Value, encrypted2.Value, "The other encryption results should not match.");
        Assert.AreNotEqual(encrypted.Key, encrypted2.Key, "The other encryption tags should not match.");
        StringAssert.StartsWith("Bad PKCS7 padding. Invalid length", exception.Message);
        StringAssert.StartsWith("Bad PKCS7 padding. Invalid length", exception.Message);
    }

    private struct TestScores
    {
        public UInt16 value1;
        public UInt32 value2;
        public UInt64 value3;
    }

    [Test]
    /// <summary>
    /// string compress testing
    /// </summary>
    public void _002_JsonConvertTest()
    {
        // setup
        var aesGcm = new MyAesGcm(Encoding.UTF8.GetBytes(@"<5s=-%s'2faHjUWs>?sd#aoY#f1GUfEa"));
        var sut = new TestScores
        {
            value1 = 62222,
            value2 = 1948987999,
            value3 = 11234123441292997999,
        };

        // when

        var jsonStr = JsonUtility.ToJson(sut);
        var encrypted = aesGcm.Encrypt(jsonStr);
        var decodedStruct = JsonUtility.FromJson<TestScores>(aesGcm.Decrypt(encrypted.Value, encrypted.Key));

        // then
        Assert.AreEqual("{\"value1\":62222,\"value2\":1948987999,\"value3\":11234123441292997999}", jsonStr);
        Assert.AreEqual(sut.value1, decodedStruct.value1);
        Assert.AreEqual(sut.value2, decodedStruct.value2);
        Assert.AreEqual(sut.value3, decodedStruct.value3);
    }

    [Test]
    /// <summary>
    /// string SHA256 hash testing
    /// </summary>
    public void _003_MySHA256HashTest()
    {
        // setup
        var key = Encoding.UTF8.GetBytes(@"<5s=-%s'2faHjUWs>?sd#aoY#f1GUfEa");
        var key2 = Encoding.UTF8.GetBytes(@"s>?sd#aoY#f1GUfE");
        var sha256Hash = new SHA256Hash();
        var text = "test text for SHA256 customized class.";
        var byteText = Encoding.UTF8.GetBytes(text);

        // when
        var hashByteText = sha256Hash.String(byteText);
        var hashWithKey = sha256Hash.String(text, key);

        // then
        Assert.AreEqual(hashByteText, sha256Hash.String(text));
        Assert.AreEqual(hashWithKey, sha256Hash.String(byteText, key));

        Assert.True(sha256Hash.Check(hashWithKey, text, key));
        Assert.False(sha256Hash.Check(hashWithKey, text + "a", key));
        Assert.False(sha256Hash.Check(hashWithKey, text, key2));
    }
}
