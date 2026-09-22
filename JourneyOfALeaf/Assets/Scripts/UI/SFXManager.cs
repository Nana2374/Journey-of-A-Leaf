using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("General SFX")]
    public AudioClip collectClip;
    public AudioClip jumpClip;
    public AudioClip walkClip;

    [Header("Home SFX")]
    public AudioClip placeClip;
    public AudioClip storeClip;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayCollect() => audioSource.PlayOneShot(collectClip);
    public void PlayJump() => audioSource.PlayOneShot(jumpClip);
    public void PlayWalk() => audioSource.PlayOneShot(walkClip);
    public void PlayPlace() => audioSource.PlayOneShot(placeClip);
    public void PlayStore() => audioSource.PlayOneShot(storeClip);

    // Generic method if you want to play any clip directly
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}
