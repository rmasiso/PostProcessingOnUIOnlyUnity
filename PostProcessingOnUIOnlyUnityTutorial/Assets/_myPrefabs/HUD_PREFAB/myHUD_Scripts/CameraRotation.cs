using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public float rotationSpeed = 5.0f; 
    public float maxVerticalAngle = 80.0f;  // how far up or down camera can shift (can it do a full 360 -- no, if set to lower)

    private float verticalRotation = 0.0f;
    private float horizontalRotation = 0.0f;

    void Start()
    {
        // grab rotation
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = transform.localEulerAngles.y;

        // potentially lock the cursor
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        // left/right
        float horizontalInput = Input.GetAxis("Horizontal"); // left/right arrows or a/d
        horizontalRotation += horizontalInput * rotationSpeed;

        // up/down
        float verticalInput = Input.GetAxis("Vertical");   // up/sown arrows or w/s
        verticalRotation -= verticalInput * rotationSpeed;

        // clamp the vertical rotation to prevent flipping
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        // apply the rotation to the camera's transform
        transform.localEulerAngles = new Vector3(verticalRotation, horizontalRotation, 0);
    }
}