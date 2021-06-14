using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class UITest
{

    [OneTimeSetUp]
    public void InitializeTest()
    {
        SceneManager.LoadSceneAsync("MainScene").completed += _ =>
        {
            Debug.Log("Scene Loaded");
        };
    }
}