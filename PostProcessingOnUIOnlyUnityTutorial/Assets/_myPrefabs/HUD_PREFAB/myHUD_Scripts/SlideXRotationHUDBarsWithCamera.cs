using UnityEngine;

public class SlideXRotationHUDBarsWithCAmera : MonoBehaviour
{
    public Transform mainCamera;           // assign the main camera here in the inspector
    public float rotationSensitivity = 10f; // how much the camera's x rotation affects the bar movement
    public float barMoveRange = 2f;         // the maximum distance the bars can move
    public float smoothingSpeed = 5f;       // controls how quickly the bars reach their target position
    public float initialYOffset = 0f;       // offsets the starting position of the bars

    private float _initialY;                  // stores the initial y position of the bars
    private float _targetYOffset;            // the y offset we're smoothly moving towards

    void Start()
    {
        if (mainCamera == null)
        {
            Debug.LogError("main camera not assigned to " + gameObject.name);
            enabled = false; // disable the script if the camera isn't assigned
            return;
        }
        _initialY = transform.localPosition.y; // remember the initial y position
        _targetYOffset = 0f;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // 1. get the camera's x-axis rotation
            float cameraXRotation = mainCamera.rotation.eulerAngles.x;

            // 2. normalize the rotation to be between -180 and 180 degrees
            float normalizedRotation = cameraXRotation > 180 ? cameraXRotation - 360 : cameraXRotation;

            // 3. calculate the target y position based on the normalized rotation
            _targetYOffset = normalizedRotation * rotationSensitivity * barMoveRange / 180f; // scale rotation to the movement range

            // 4. smoothly move the bar towards the target y position
            float currentY = transform.localPosition.y - _initialY;
            float newY = Mathf.Lerp(currentY, _targetYOffset, Time.deltaTime * smoothingSpeed);
            transform.localPosition = new Vector3(transform.localPosition.x, _initialY + newY, transform.localPosition.z);
        }
    }
}