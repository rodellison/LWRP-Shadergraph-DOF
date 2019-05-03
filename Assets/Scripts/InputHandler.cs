using UnityEngine;


public class InputHandler : MonoBehaviour
{
    void Update()
    {
        //x keystroke to exit
        if (Input.GetKeyDown("escape") || Input.GetKeyDown("x"))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
			Application.OpenURL(webplayerQuitURL);
#else
			Application.Quit();
#endif
        }
        
        if (Input.GetKeyDown("p"))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = !UnityEditor.EditorApplication.isPaused;
#endif
        }
    }
}