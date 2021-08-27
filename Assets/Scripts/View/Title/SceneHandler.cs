using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneHandler : MonoBehaviour
{
    [SerializeField] Button nextScene = default;

    private AsyncOperation asyncLoad;

    void Start()
    {
        nextScene.onClick.AddListener(SceneTransition); // ボタンを押すとシーン遷移開始
        DisplayLoading();                               // ロード画面の表示
        StartCoroutine(LoadScene());                    // 非同期ロードの開始
    }

    private IEnumerator LoadScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync(1);     // 次のシーンの非同期ロード開始
        asyncLoad.allowSceneActivation = false;         // シーン遷移無効化

        while (asyncLoad.progress < 0.9f)               // ロードが完了するまで 3 秒待機を繰り返す
        {
            yield return new WaitForSeconds(3);
        }

        DisplayNext();                                  // ロード完了したら次の画面を表示
    }

    private void SceneTransition()
    {
        asyncLoad.allowSceneActivation = true;          // シーン遷移有効化
    }

    private void DisplayLoading()
    {
        // ロード画面の描画
    }

    private void DisplayNext()
    {
        // ロード完了後画面の描画
    }
}