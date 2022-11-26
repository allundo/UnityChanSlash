using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenCaptureModule : MonoBehaviour
{
#if UNITY_EDITOR

    private const int MAX_SHOTS = 100;
    private int index = 0;
    private int coolTime = 0;
    private string date;
    private string folderPath;
    private bool isKeyDown = false;

    void Start()
    {
        date = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        folderPath = Application.dataPath + "/../DevResources/ScreenShots/" + date;
        Debug.Log(date);
    }

    void Update()
    {
        coolTime--;

        if (Keyboard.current.spaceKey.isPressed)
        {
            if (isKeyDown) return;

            isKeyDown = true;

            if (coolTime > 0)
            {
                Debug.Log($"{(float)coolTime / 60f}秒お待ち下さい");
                return;
            }

            if (index >= MAX_SHOTS) throw new Exception($"限界枚数({MAX_SHOTS})超えました");

            // スクリーンショットを保存
            Directory.CreateDirectory(folderPath);

            string fileName = $"ScreenShot{index++:00}.png";
            ScreenCapture.CaptureScreenshot(folderPath + "/" + fileName);
            Debug.Log($"スクリーンショットを保存: " + fileName);
            coolTime = 60;
        }
        else
        {
            isKeyDown = false;
        }
    }

#endif
}
