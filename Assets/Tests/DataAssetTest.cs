using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

public class DataAssetTest
{
    [UnityTest]
    /// <summary>
    /// TextAlignmentOption is set to invalid value by default on DataAsset Editor. </ br>
    /// They must be edited at least once. </ br>
    /// </summary>
    public IEnumerator _001_MessageSourceValidationTest()
    {
        // Load from main Resources folder
        var floorMessagesData = Resources.Load<FloorMessagesData>("DataAssets/Message/FloorMessagesData");

        floorMessagesData.ForEach(floorMessageSource =>
        {
            floorMessageSource.fixedMessages.ForEach(srcArray =>
            {
                srcArray.ForEach(src => AssertMessageSource(src));
            });

            floorMessageSource.randomMessages.ForEach(srcArray =>
            {
                srcArray.ForEach(src => AssertMessageSource(src));
            });
        });

        yield return null;
    }

    private void AssertMessageSource(MessageSource src)
    {
        Debug.Log("Assert message source: " + src.name + ", alignment: " + src.alignment);
        Assert.False(0 == (int)src.alignment);
        Assert.AreNotEqual(0, src.fontSize);
        Assert.AreNotEqual(0, src.literalsPerSec);
        Assert.AreNotEqual(null, src.sentence);
        Assert.AreNotEqual("", src.sentence);
    }
}
