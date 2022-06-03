using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;

public class CSharpSpecTest
{
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
        Object.Destroy(go);

        yield return null;

        Assert.True(tests[1].check);
    }
}