using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera; // Reference to the camera
    [SerializeField] private float minZoom = 7f; // Minimum orthographic size
    [SerializeField] private float maxZoom = 9f; // Maximum orthographic size
    [SerializeField] private float zoomSpeed = 2f; // Speed at which the zoom adjusts
    [SerializeField] private float zoomBuffer = 1.5f; // Extra space to add around the marbles
       
    private Transform[] MarbleTransforms = new Transform[4]; // Array of marble transforms, max 4.

    public void setMarbleTransforms(Transform[] marbleTransforms)
    {
        this.MarbleTransforms = marbleTransforms;
    }

    private void Update()
    {
        AdjustCameraZoom();
    }

    private void AdjustCameraZoom()
    {
        if (MarbleTransforms.Length == 0)
            return;

        // Get the bounds that encapsulate all the marbles
        Bounds marbleBounds = GetMarbleBounds();

        // Calculate the required orthographic size based on the size of the bounds
        float requiredZoom = Mathf.Max(marbleBounds.size.x, marbleBounds.size.y) / 2f * zoomBuffer;
        requiredZoom = Mathf.Clamp(requiredZoom, minZoom, maxZoom);

        // Smoothly interpolate the camera's orthographic size to the required zoom
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, requiredZoom, Time.deltaTime * zoomSpeed);
    }

    private Bounds GetMarbleBounds()
    {
        if (MarbleTransforms.Length == 1)
            return new Bounds(MarbleTransforms[0].position, Vector3.zero);

        // Create an initial bounds set around the first marble
        Bounds bounds = new Bounds(MarbleTransforms[0].position, Vector3.zero);

        // Expand the bounds to include all marbles
        for (int i = 1; i < MarbleTransforms.Length; i++)
        {
            bounds.Encapsulate(MarbleTransforms[i].position);
        }

        return bounds;
    }
}
