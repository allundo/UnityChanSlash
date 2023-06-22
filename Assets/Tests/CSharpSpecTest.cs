using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;
using System;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

public class CSharpSpecTest
{
    [Ignore("Only for spec confirmation.")]
    [Test]
    /// <summary>
    /// string encryption testing
    /// </summary>
    public void MyAesGcmEncryptTest()
    {
        // setup
        var aesGcm = new MyAesGcm(Encoding.UTF8.GetBytes(@"<5s=-%s'2faHjUWs>?sd#aoY#f1GUfEa"));
        var hexValueStr = "F0458";

        // when
        var encrypted = aesGcm.Encrypt(hexValueStr);
        var extractedStr = aesGcm.Decrypt(encrypted.Value, encrypted.Key);

        var fabricated = Convert.ToBase64String(Convert.FromBase64String(encrypted.Value).Select(value => (byte)(value / 2)).ToArray());
        var exception = Assert.Throws<CryptographicException>(() => aesGcm.Decrypt(fabricated, encrypted.Key));

        // then
        Assert.AreEqual(hexValueStr, extractedStr);
        StringAssert.StartsWith("Bad PKCS7 padding. Invalid length", exception.Message);
    }

    private struct TestScores
    {
        public UInt16 value1;
        public UInt32 value2;
        public UInt64 value3;
    }

    [Ignore("Only for spec confirmation.")]
    [Test]
    /// <summary>
    /// string compress testing
    /// </summary>
    public void JsonConvertTest()
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

    [Ignore("Only for spec confirmation.")]
    [Test]
    /// <summary>
    /// Struct is assigned into a variable not as a reference but a COPY. <br />
    /// Array is handled as reference in method and the fields are editable
    /// </summary>
    public void ArrayStructReferenceTest()
    {
        TestStruct[] tests = new TestStruct[] { new TestStruct(), new TestStruct(), new TestStruct() };

        var test0 = tests[0]; // Assigns a COPY of tests[0]
        test0.check = true;

        ChangeSecond(tests);

        Assert.False(tests[0].check);
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

    [Ignore("Only for spec confirmation.")]
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

    private event Func<string> sutEvent;

    [Ignore("Only for spec confirmation.")]
    [Test]
    /// <summary>
    /// Multiple delegate events can be assigned to Func/Action but they invoke only the last assigned event.
    /// </summary>
    public void DelegateInvokesOnlyTheLastAssignedEvent()
    {
        sutEvent = null;

        sutEvent += EventTest;
        Assert.AreEqual("EventTestString", sutEvent.Invoke());

        sutEvent += EventTest2;
        Assert.AreEqual("EventTest2String", sutEvent.Invoke());

        sutEvent += EventTest;
        Assert.AreEqual("EventTestString", sutEvent.Invoke());

        sutEvent -= EventTest2;
        Assert.AreEqual("EventTestString", sutEvent.Invoke());

        sutEvent -= EventTest;
        Assert.AreEqual("EventTestString", sutEvent.Invoke());

        // Assigned delegates can be retrieved by GetInvocationList.
        Assert.AreEqual("EventTest", sutEvent.GetInvocationList()[0].GetMethodInfo().Name);

        sutEvent -= EventTest;
        Assert.Throws<NullReferenceException>(() => sutEvent.Invoke(), "Should be a null event invocation");
    }

    private string EventTest()
    {
        return "EventTestString";
    }
    private string EventTest2()
    {
        return "EventTest2String";
    }
}
