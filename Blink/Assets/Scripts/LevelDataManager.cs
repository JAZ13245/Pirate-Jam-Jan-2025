using UnityEngine;

public class LevelDataManager : MonoBehaviour
{
    public LevelData[] levels;

    public static LevelDataManager Instance { get; private set; }
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public LevelData GetLevelByIndex(int levelIndex)
    {
        return levels[levelIndex];
    }

    public void SetSelectedLevelInfo(LevelData level)
    {
        PlayerPrefs.SetString("selectedSceneName", levels[0].sceneName);
        PlayerPrefs.SetString("selectedLevelName", levels[0].levelName);
        PlayerPrefs.SetInt("selectedLevelNumber", levels[0].levelNumber);
    }

    public void SetSelectedLevelInfoByIndex(int levelIndex)
    {
        LevelData level = GetLevelByIndex(levelIndex);

        PlayerPrefs.SetString("selectedSceneName", levels[0].sceneName);
        PlayerPrefs.SetString("selectedLevelName", levels[0].levelName);
        PlayerPrefs.SetInt("selectedLevelNumber", levels[0].levelNumber);
    }

    public void SetSelectedLevelToNextLevel()
    {
        int nextLevelIndex = PlayerPrefs.GetInt("selectedLevelNumber", 1);
        SetSelectedLevelInfoByIndex(nextLevelIndex);
    }

    public LevelData GetNextLevel(){
        int nextLevelIndex = PlayerPrefs.GetInt("selectedLevelNumber", 1); // Don't have to shift it since it's the next level
        return levels[nextLevelIndex];
    }
}
