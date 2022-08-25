using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

[Ignore("Only for spec confirmation.")]
public class CSharpSpecTest
{
    [Test]
    /// <summary>
    /// string compress testing
    /// </summary>
    public void GZipCompressTest()
    {
        // setup
        var hexValueStr = "F0458";

        // when
        var base64String = CompressGZIP(hexValueStr);
        var extractedStr = ExtractGZIP(base64String);

        // then
        Assert.AreEqual("hkrLwob6TTcMN7qZSoT/rJrIWyEjE4uKjEw7P64kGQ8=", base64String);
        Assert.AreEqual(hexValueStr, extractedStr);
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
    public void JsonConvertTest()
    {
        // setup
        var sut = new TestScores
        {
            value1 = 62222,
            value2 = 1948987999,
            value3 = 11234123441292997999,
        };

        // when

        var jsonStr = JsonUtility.ToJson(sut);
        var encryptedJson = CompressGZIP(jsonStr);
        var decodedStruct = JsonUtility.FromJson<TestScores>(ExtractGZIP(encryptedJson));

        // then
        Assert.AreEqual("{\"value1\":62222,\"value2\":1948987999,\"value3\":11234123441292997999}", jsonStr);
        Assert.AreEqual("7FRt3OMwdTy8jK6Z80yFKxlDy0EBMNzSktvwQyxwpA54YahWrxa9ub/HMO81pmiDLxSOHWriUKrj7h880MWjPiZx+tpXP1PsrTUsdcjMjAI=", encryptedJson);
        Assert.AreEqual(sut.value1, decodedStruct.value1);
        Assert.AreEqual(sut.value2, decodedStruct.value2);
        Assert.AreEqual(sut.value3, decodedStruct.value3);
    }

    /// <summary>
    /// Compress and encrypt string
    /// </summary>
    public string CompressGZIP(string str)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var inflateStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                using (var writer = new StreamWriter(inflateStream))
                {
                    writer.Write(str);
                }
            }
            return Convert.ToBase64String(Encrypt(memoryStream.ToArray()));
        }
    }

    /// <summary>
    /// Decrypt and extract compressed string
    /// </summary>
    public string ExtractGZIP(string base64String)
    {
        using (var memoryStream = new MemoryStream(Decrypt(Convert.FromBase64String(base64String))))
        {
            using (var deflateStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                using (var reader = new StreamReader(deflateStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }

    private byte[] Encrypt(byte[] src)
    {
        AesManaged aes = new AesManaged()
        {
            KeySize = 256,
            BlockSize = 128,
            Mode = CipherMode.CBC,
            IV = Encoding.UTF8.GetBytes(@"EaP/p[:c;gj;o,cs"),
            Key = Encoding.UTF8.GetBytes(@"<5s=-%s'2faHjUWs>?sd#aoY#f1GUfEa"),
            Padding = PaddingMode.PKCS7,
        };

        return aes.CreateEncryptor().TransformFinalBlock(src, 0, src.Length);
    }

    private byte[] Decrypt(byte[] src)
    {
        AesManaged aes = new AesManaged()
        {
            KeySize = 256,
            BlockSize = 128,
            Mode = CipherMode.CBC,
            IV = Encoding.UTF8.GetBytes(@"EaP/p[:c;gj;o,cs"),
            Key = Encoding.UTF8.GetBytes(@"<5s=-%s'2faHjUWs>?sd#aoY#f1GUfEa"),
            Padding = PaddingMode.PKCS7,
        };

        return aes.CreateDecryptor().TransformFinalBlock(src, 0, src.Length);
    }

    [Test]
    /// <summary>
    /// Array is handled as reference in method and the fields are editable
    /// </summary>
    public void ArrayStructReferenceTest()
    {
        TestStruct[] tests = new TestStruct[] { new TestStruct(), new TestStruct(), new TestStruct() };

        ChangeSecond(tests);

        Assert.True(tests[1].check);
    }

    private struct TestStruct
    {
        public bool check;
        public TestStruct(bool check = false)
        {
            this.check = check;
        }
    }

    private void ChangeSecond(TestStruct[] tests)
    {
        tests[1].check = true;
    }

    [UnityTest]
    /// <summary>
    /// Array is handled as reference in a notification message and the fields can be edited by the receivers;
    /// </summary>
    public IEnumerator UniRxArrayStructReferenceTest()
    {
        var go = new GameObject("UniRx");
        var subject = new Subject<TestStruct[]>();

        TestStruct[] tests = new TestStruct[] { new TestStruct(), new TestStruct(), new TestStruct() };

        subject.Subscribe(tests => tests[1].check = true).AddTo(go);

        Assert.False(tests[1].check);

        yield return null;

        subject.OnNext(tests);
        subject.OnCompleted();
        UnityEngine.Object.Destroy(go);

        yield return null;

        Assert.True(tests[1].check);
    }
}