using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private bool canLerp = false, waitToLerp;

    private CanvasGroup from, to;

    private void Update()
    {
        if (!canLerp) return;

        to.gameObject.SetActive(true); 
        from.alpha -= Time.deltaTime;
        if(from.alpha <= .2f && waitToLerp || !waitToLerp)
        {
            to.alpha += Time.deltaTime;
        }

        if (from.alpha == 0)
        {
            from.gameObject.SetActive(false);
        }
    }

    public void SetFrom(CanvasGroup From) => from = From;

    public void SetTo(CanvasGroup To) => to = To;
    public void ChangeMenu() => canLerp = true;

    public void WaitToLerp(bool boolean) => waitToLerp = boolean;

    public void ChangeSceneFromIndex(int sceneIndex) => SceneManager.LoadScene(sceneIndex); 
    public void ChangeSceneFromName(string sceneName) => SceneManager.LoadScene(sceneName); 
    public void Quit() => Application.Quit();
}