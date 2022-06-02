using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

public class HelloWorld
{
    // A Test behaves as an ordinary method
    [Test]
    public void HelloWorldSimplePasses()
    {
        Debug.Log("Hello! World!: Test");
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator HelloWorldWithEnumeratorPasses()
    {
        var testCanvas = Object.Instantiate(Resources.Load<GameObject>("Prefabs/TestCanvas"));
        var tmHelloWorld = testCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        yield return null;

        Assert.AreEqual(tmHelloWorld[0].text, "Hello! World!");
    }
}
