using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] musicTracks; // Array of music tracks
    [SerializeField] private AudioSource audioSource; // The AudioSource to play the music

    private int currentTrackIndex = 0; // Index to keep track of the current music

    public static MusicManager Instance { get; private set; }
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

    public void PlayTrack(int index)
    {
        if (index >= 0 && index < musicTracks.Length)
        {
            currentTrackIndex = index;
            audioSource.clip = musicTracks[index]; // Set the new clip
            audioSource.Play(); // Play the new track
        }
        else
        {
            Debug.LogWarning("Track index out of range!");
        }
    }

    public void QuietMusicVolume()
    {
        audioSource.volume = 0.08f; 
    }

    public void NormalMusicVolume()
    {
        audioSource.volume = 1f; 
    }
}
