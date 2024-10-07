using UnityEngine;

public class MusicManager : MonoBehaviour
{

    public AudioSource menuIntro;
    public AudioSource menuLoop;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuIntro.Play();
        Invoke("StartLooping", menuIntro.clip.length);
    }

    public void StartLooping()
    {
        menuLoop.Play();
    }
}
