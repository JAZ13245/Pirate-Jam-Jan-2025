using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.SceneManagement;

public class NarrativeHandler : MonoBehaviour
{
    [SerializeField] public DialogueRunner dialogueRunner; // Yarn Spinner Dialogue Runner
    [SerializeField] public LineView lineView;
    [SerializeField] public bool inDialogue;

    private static NarrativeHandler instance; // Singleton for the Narrative Handler

    public static NarrativeHandler Instance
    {
        get { return instance; }
    }

    private Coroutine autoContinueCoroutine;
    private float autoContinueDelay = 2f; // Default delay for auto-continue
    private float textSpeed = 120f; 

    [SerializeField] private AudioSource audioSource; // AudioSource to play voice lines
    [SerializeField] private List<AudioClip> voiceLineDatabase; // Database of voice lines

    private void Awake()
    {
        // Make NarrativeHandler a Singleton
        if (instance != null)
        {
            Debug.LogError("Found more than one NarrativeHandler in the scene!");
        }
        else
        {
            instance = this;
        }

        AddYarnCommands();
    }

    private void Start() {
        // Run Cutscene based off the loaded level
        StartDialogue(PlayerPrefs.GetString("selectedSceneName", "Test"));
    }

    public void StartDialogue(string node)
    {
        dialogueRunner.StartDialogue(node);
    }

    public void DialogueStarted()
    {
        inDialogue = true;
        MusicManager.Instance.QuietMusicVolume();
    }

    public void DialogueCompleted()
    {
        inDialogue = false;
        MusicManager.Instance.NormalMusicVolume();
    }

    // Converts Functions to Yarn Commands
    private void AddYarnCommands()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.AddCommandHandler("YarnTest", () => YarnTest());

            dialogueRunner.AddCommandHandler<float>(
                "StartAutoContinue",
                StartAutoContinue
            );


            dialogueRunner.AddCommandHandler(
                "StopAutoContinue",
                StopAutoContinue
            );

            dialogueRunner.AddCommandHandler<float>(
                "SetTextSpeed",
                SetTextSpeed
            );

            dialogueRunner.AddCommandHandler<int>(
                "PlayVoiceLine",
                PlayVoiceLine
            );

            dialogueRunner.AddCommandHandler<string>(
                "LoadSceneByName",
                LoadSceneByName
            );

            dialogueRunner.AddCommandHandler(
                "LoadSelectedLevelScene",
                LoadSelectedLevelScene
            );

            dialogueRunner.AddCommandHandler<int>(
                "ChangeTrack",
                ChangeTrack
            );
        }
    }

    private void YarnTest()
    {
        Debug.Log("Test");
    }

    public static class AdderFunction
    {
        public static NarrativeHandler narrativeHandler = NarrativeHandler.Instance;
    }

    public bool IsDialogueActive()
    {
        return dialogueRunner.IsDialogueRunning;
    }

    private void StartAutoContinue(float delay)
    {
        if (autoContinueCoroutine != null)
        {
            StopCoroutine(autoContinueCoroutine); // Stop existing auto-continue
        }

        autoContinueDelay = delay;
        autoContinueCoroutine = StartCoroutine(AutoContinue());
    }

    private void StopAutoContinue()
    {
        if (autoContinueCoroutine != null)
        {
            StopCoroutine(autoContinueCoroutine);
            autoContinueCoroutine = null;
        }
    }

    private IEnumerator AutoContinue()
    {
        while (inDialogue)
        {
            yield return new WaitForSeconds(autoContinueDelay);

            if (dialogueRunner.IsDialogueRunning)
            {
                dialogueRunner.OnViewRequestedInterrupt(); // Advance to the next line
            }
        }
    }

    // Sets the text speed for the dialogue
    private void SetTextSpeed(float speed)
    {
        textSpeed = speed;

        // Update the text speed in the LineView (if available)
        if (lineView != null)
        {
            lineView.typewriterEffectSpeed = textSpeed;
        }
        else
        {
            Debug.LogWarning("No LineView found in DialogueRunner. Text speed cannot be updated.");
        }
    }

    private void PlayVoiceLine(int voiceClipIndex)
    {
        audioSource.Stop(); // Stop any currently playing voice line
        audioSource.clip = voiceLineDatabase[voiceClipIndex];
        audioSource.Play();

        // Calculate the auto-continue delay based on the voice clip length
        float voiceLineDuration = audioSource.clip.length;
        float additionalDelay = 0.1f; // Small delay after the voice line finishes
        autoContinueDelay = voiceLineDuration + additionalDelay;

        // Restart the auto-continue coroutine with the new delay
        if (autoContinueCoroutine != null)
        {
            StopCoroutine(autoContinueCoroutine);
        }
        autoContinueCoroutine = StartCoroutine(AutoContinue());
    }

    private void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name is null or empty!");
        }
    }

    private void LoadSelectedLevelScene()
    {
        LoadSceneByName(PlayerPrefs.GetString("selectedSceneName", "MainMenu"));
    }

    private void ChangeTrack(int trackIndex)
    {
        MusicManager.Instance.PlayTrack(trackIndex);
    }
}
