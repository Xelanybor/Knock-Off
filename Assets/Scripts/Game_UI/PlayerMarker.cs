using UnityEngine;

public class PlayerMarker : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 0, 0);
    private Transform playerTransform;
    private Quaternion fixedRotation;
    private MarbleController player;
    public Color borderColour;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponentInParent<MarbleController>();  // get the player for this UI object
        playerTransform = player.transform;
        fixedRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        transform.rotation = fixedRotation;
        transform.position = playerTransform.position + worldOffset;
    }
}
