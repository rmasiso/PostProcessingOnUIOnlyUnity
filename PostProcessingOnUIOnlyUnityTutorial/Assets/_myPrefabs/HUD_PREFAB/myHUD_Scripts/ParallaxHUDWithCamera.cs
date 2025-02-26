using UnityEngine;

public class ParallaxHUDWithCamera : MonoBehaviour
{
    public Transform mainCamera;          // assign the main camera in the inspector
    public float parallaxSensitivity = 0.1f; // how sensitive the parallax effect is
    public float smoothTime = 0.2f;         // how smoothly the parallax effect reacts
    public float returnSpeed = 2f;          // how quickly the hud returns to its original position

    private Vector3 _velocity;               // for smoothing the movement (smoothdamp)
    private Vector3 _initialPosition;        // the hud's starting position
    private Quaternion _previousCameraRotation;   // stores the camera's rotation from the last frame
    private Vector3 _targetPosition;          // the position we want the hud to move towards

    void Start()
    {
        if (mainCamera == null)
        {
            Debug.LogError("main camera not assigned to " + gameObject.name);
            enabled = false; // disable the script if there's no camera
            return;
        }
        _initialPosition = transform.localPosition;  // remember the initial position
        _previousCameraRotation = mainCamera.rotation; // remember the starting camera rotation
        _targetPosition = transform.localPosition;     // start the target position at the initial position
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // 1. figure out how much the camera rotated since last frame (using quaternions!)
            Quaternion rotationDifference = mainCamera.rotation * Quaternion.Inverse(_previousCameraRotation);

            // 2. convert that rotation difference into euler angles for easier calculations
            Vector3 eulerAngles = rotationDifference.eulerAngles;

            // 3. make sure the angles are between -180 and 180 (helps with smooth looping)
            eulerAngles.x = eulerAngles.x > 180 ? eulerAngles.x - 360 : eulerAngles.x;
            eulerAngles.y = eulerAngles.y > 180 ? eulerAngles.y - 360 : eulerAngles.y;

            // 4. calculate the parallax offset based on those rotations and our sensitivity setting
            Vector2 offset = new Vector2(eulerAngles.y, eulerAngles.x) * parallaxSensitivity;

            // 5. update the target position by adding that offset
            _targetPosition += (Vector3)offset;

            // 6. smoothly move the hud towards the target position (damping!)
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, _targetPosition, ref _velocity, smoothTime);

            // 7. slowly return the target position back towards the original position
            _targetPosition = Vector3.Lerp(_targetPosition, _initialPosition, Time.deltaTime * returnSpeed);

            // 8. remember this frame's camera rotation for the next frame's calculations
            _previousCameraRotation = mainCamera.rotation;
        }
    }
}