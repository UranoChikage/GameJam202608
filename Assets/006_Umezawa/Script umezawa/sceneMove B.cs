using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// レガシーUI (Button) をクリックしたときに指定したシーンへ遷移するスクリプト。
/// GameObject にアタッチし、Inspector からシーン名を設定してください。
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("移動シーン名")]
    [SerializeField] private string sceneName;

    [Header("待機時間")]
    [SerializeField] private float delay = 0f;

    // Inspector でボタンを直接アサインする場合はここに入れる
    [Header("対象ボタン")]
    [SerializeField] private Button targetButton;

    void Start()
    {
        // ボタンが未アサインなら、同じ GameObject の Button を取得
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (targetButton != null)
            targetButton.onClick.AddListener(OnButtonClicked);
        else
            Debug.LogWarning("[SceneLoader] Button コンポーネントが見つかりません。", this);
    }

    void OnDestroy()
    {
        // リスナーの解除 (メモリリーク防止)
        if (targetButton != null)
            targetButton.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] シーン名が設定されていません。", this);
            return;
        }

        if (delay > 0f)
            StartCoroutine(LoadWithDelay());
        else
            LoadScene();
    }

    private System.Collections.IEnumerator LoadWithDelay()
    {
        yield return new WaitForSeconds(delay);
        LoadScene();
    }

    private void LoadScene()
    {
        // Build Settings にシーンが登録されているか確認
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogError($"[SceneLoader] シーン '{sceneName}' が Build Settings に見つかりません。", this);
    }
}
