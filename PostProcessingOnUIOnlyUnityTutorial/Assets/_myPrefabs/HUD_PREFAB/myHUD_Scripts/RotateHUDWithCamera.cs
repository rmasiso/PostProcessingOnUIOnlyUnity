using UnityEngine;

public class RotateHUDWithCAmera : MonoBehaviour
{
    public Transform mainCamera;     // inspector: assign the main camera here
    public float rotationSpeed = 4f;   // adjust the rotation speed as needed
    public Vector3 rotationAxis = Vector3.forward; // the axis to rotate around (z axis by default)
    public float smoothTime = 0.2f;  // control the smoothness of the rotation

    private Quaternion _targetRotation;  // stores the desired rotation
    private float _currentVelocity; // used for rotation smoothing

    private void Start()
    {
        // ensure the camera is assigned
        if (mainCamera == null)
        {
            Debug.LogError("main camera not set. please assign a main camera in the inspector.");
            enabled = false; // disable the script if there's no camera.
        }

        // initialize the target rotation
        _targetRotation = transform.rotation;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            // 1. get the horizontal rotation from the main camera
            float horizontalRotation = mainCamera.rotation.eulerAngles.y;

            // 2. calculate the target rotation based on camera rotation
            _targetRotation = Quaternion.AngleAxis(horizontalRotation * rotationSpeed, rotationAxis);

            // 3. smoothly rotate the hud towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, smoothTime * Time.deltaTime);
        }
    }
}