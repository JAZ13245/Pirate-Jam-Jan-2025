using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndScreenManager : MonoBehaviour
{
    public int currentEndState = 0; // 0 = Pause, 1 = Death, 2 = Win
    public GameObject[] endScreens; // Each End Screen
    public CanvasGroup[] endScreenCanvasGroups; // Each End Screen
    public TextMeshProUGUI pauseScreenLevelInfo;
    public TextMeshProUGUI deathScreenLevelInfo;
    public TextMeshProUGUI winScreenLevelInfo;

    private string selectedLevelName;
    private string selectedLevelNumber;

    private CanvasGroup from, to;
    private bool canLerp = false, waitToLerp;

    private void Start() {
        LoadLevelInfo();
    }

    public void LoadLevelInfo()
    {
        selectedLevelName = PlayerPrefs.GetString("selectedLevelName", "NULL");
        selectedLevelNumber = $"{PlayerPrefs.GetInt("selectedLevelNumber", 0)}";

        pauseScreenLevelInfo.text = $"{selectedLevelNumber}. {selectedLevelName}";
        deathScreenLevelInfo.text = $"{selectedLevelNumber}. {selectedLevelName}";
        winScreenLevelInfo.text = $"{selectedLevelNumber}. {selectedLevelName}";
    }

    private void Update()
    {
        if (!canLerp) return;

        to.gameObject.SetActive(true); 
        from.alpha -= Time.unscaledDeltaTime;
        if(from.alpha <= .2f && waitToLerp || !waitToLerp)
        {
            to.alpha += Time.unscaledDeltaTime;
        }

        if (from.alpha == 0)
        {
            from.gameObject.SetActive(false);
        }
    }

    public void SetFrom(CanvasGroup From) => from = From;
    public void SetFromAsEndMenu(){
        from = endScreenCanvasGroups[currentEndState].GetComponent<CanvasGroup>();
    }

    public void SetTo(CanvasGroup To) => to = To;
    public void SetToAsEndMenu(){
        to = endScreenCanvasGroups[currentEndState].GetComponent<CanvasGroup>();
    }

    public void showEndScreen(int endScreenIndex)
    {
        endScreens[endScreenIndex].SetActive(true);
        endScreenCanvasGroups[endScreenIndex].alpha = 1;
    }

    public void HideAllScreens()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ChangeMenu() => canLerp = true;

    public void RestartScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // Unpause
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ChangeSceneFromIndex(currentSceneIndex);
    }

    public void NextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Unpause
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ChangeSceneFromIndex(nextSceneIndex);
    }

    public void ChangeSceneFromIndex(int sceneIndex) => SceneManager.LoadScene(sceneIndex); 
    public void ChangeSceneFromName(string sceneName)
    {
        if(sceneName != "MainMenu"){

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Unpause
        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneName); 
    }
}
