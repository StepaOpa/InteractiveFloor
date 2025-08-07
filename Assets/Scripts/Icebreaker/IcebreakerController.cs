using UnityEngine;

public class IcebreakerController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 3f;
    [SerializeField] private float maxRotation = 15f;
    [SerializeField] private float rotationSpeed = 3f;

    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;

    private bool isMoving = false;
    private Quaternion originRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }




    void Movement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontalInput * moveSpeed * Time.deltaTime, Space.World);
        Tilt(horizontalInput);
        // SmallRotation(horizontalInput);
    }

    void Tilt(float horizontalInput)
    {
        float tilt = Mathf.Lerp(0, maxTiltAngle, Mathf.Abs(horizontalInput));
        float rotation = Mathf.Lerp(0, maxRotation, Mathf.Abs(horizontalInput));

        originRotation = Quaternion.Euler(0, rotation * horizontalInput, tilt * horizontalInput);

        transform.rotation = Quaternion.Slerp(transform.rotation, originRotation, tiltSpeed * Time.deltaTime);
    }

    private bool IsShipMoving()
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        return currentRotation.y != 0 || currentRotation.z != 0;
    }

}
