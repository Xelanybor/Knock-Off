using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;


    public AudioSource menuIntro;
    public AudioSource menuLoop;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnsureSingleton();
        menuIntro.Play();
        menuLoop.PlayDelayed(menuIntro.clip.length);
    }


    private void EnsureSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Bad Instantiation, destroying.");
            Destroy(gameObject);
        }
    }
}
