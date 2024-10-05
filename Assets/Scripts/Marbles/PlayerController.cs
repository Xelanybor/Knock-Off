using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private MarbleController marblePrefab;
    private MarbleController marble;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        marble = Instantiate(marblePrefab, new Vector3(-4, -1, 0), Quaternion.identity);
        marble.name = "Player Marble";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public MarbleController GetMarble()
    {
        return marble;
    }


}
