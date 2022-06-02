using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

public class Example
{
    // Testアトリビュートを付ける
    [Test]
    public void ExampleTest()
    {
        // 条件式がtrueだったら成功
        Assert.That(true);
    }

    [UnityTest]
    public IEnumerator HelloWorldTest()
    {

        var testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/TestCanvas"));
        var tmHelloWorld = testCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        yield return new WaitForSeconds(3f);

        Assert.AreEqual(tmHelloWorld[0].text, "Hello! World!");
    }
}