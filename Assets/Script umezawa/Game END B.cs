using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // ゲームを終了する関数
    
    public void Quit()
    {

#if UNITY_EDITOR
        
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // アプリ終了
        Application.Quit();
#endif
    }
}