using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Settings")]
    private bool canLerp = false, waitToLerp;
    [Header("Level Manager Settings")]
    public TextMeshProUGUI levelName;
    public TextMeshProUGUI levelNumber;
    public Image levelPreview;
    private LevelData selectedLevel;

    private CanvasGroup from, to;

    private void Start() {
        LevelData firstLevel = LevelDataManager.Instance.GetLevelByIndex(0);
        LevelDataManager.Instance.SetSelectedLevelInfoByIndex(firstLevel.levelNumber - 1);
        LoadLevelInfo(0);

        MusicManager.Instance.PlayTrack(0);
    }

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

    public void LoadLevelInfo(int levelIndex)
    {
        LevelData level = LevelDataManager.Instance.GetLevelByIndex(levelIndex);

        selectedLevel = level;
        levelName.text = level.levelName;
        levelPreview.sprite = level.levelPreview;
        levelNumber.text = $"{level.levelNumber}";
    }

    public void LoadCutscene()
    {
        ChangeSceneFromName("Cutscene");
    }

    public void Quit() => Application.Quit();
}