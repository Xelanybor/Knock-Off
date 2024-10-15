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
        Vector3 requiredPosition = new Vector3(marbleBounds.center.x, marbleBounds.center.y, -10);
        requiredPosition.x = Mathf.Clamp(requiredPosition.x, -5f, 5f);
        requiredPosition.y = Mathf.Clamp(requiredPosition.y, -2.5f, 2.5f);

        // Smoothly interpolate the camera's orthographic size to the required zoom
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, requiredZoom, Time.deltaTime * zoomSpeed);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, requiredPosition, Time.deltaTime * zoomSpeed);
    }

    private Bounds GetMarbleBounds()
    {
        if (MarbleTransforms == null || MarbleTransforms.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        // Find the first valid transform
        Transform firstValidTransform = null;
        foreach (var marbleTransform in MarbleTransforms)
        {
            if (marbleTransform != null)
            {
                firstValidTransform = marbleTransform;
                break;
            }
        }

        // If no valid transforms were found, return empty bounds
        if (firstValidTransform == null)
            return new Bounds(Vector3.zero, Vector3.zero);

        // Create an initial bounds set around the first valid marble
        Bounds bounds = new Bounds(firstValidTransform.position, Vector3.zero);

        // Expand the bounds to include all valid marbles
        foreach (var marbleTransform in MarbleTransforms)
        {
            if (marbleTransform != null)
            {
                bounds.Encapsulate(marbleTransform.position);
            }
        }

        return bounds;
    }

}
