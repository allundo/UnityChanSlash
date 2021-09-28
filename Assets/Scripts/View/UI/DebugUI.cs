using UnityEngine;
using System;
using System.Text;
using UnityEngine.UI;

public class DebugUI : SingletonMonoBehaviour<DebugUI>
{
    private Text text;
    private StringBuilder sb = new StringBuilder();

    protected override void Awake()
    {
        base.Awake();
        text = GetComponent<Text>();
    }

    public static void Log(string mes)
    {
        Instance.sb.Append(DateTime.Now.ToString("[HH:mm:ss] ") + mes + Environment.NewLine);
        Instance.text.text = Instance.sb.ToString();
        Debug.Log(mes);
    }

    void OnGUI()
    {
        Rect rect = new Rect(10, 40, 200, 30);
        if (GUI.Button(rect, "Clear"))
        {
            sb.Clear();
        }
    }
}