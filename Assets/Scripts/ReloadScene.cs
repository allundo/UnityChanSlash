using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour
{
    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(Reload);
    }

    void Reload()
    {
        SceneManager.LoadScene("MainScene");
    }
}
