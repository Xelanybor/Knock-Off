using UnityEngine;

public class MusicManager : MonoBehaviour
{

    public AudioSource menuIntro;
    public AudioSource menuLoop;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuIntro.Play();
        Debug.Log("Started intro");
        menuLoop.PlayDelayed(menuIntro.clip.length);
    }
}
