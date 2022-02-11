using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform playerBody;
    public Transform playerCamera;

    private float movementSpeed = 50f;
    private float lookSpeed = 100f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        HandleLooking();
        HandleMovement();
    }

    void HandleLooking()
    {
        float xRotation = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float yRotation = Mathf.Clamp(Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime, -90, 90);

        playerBody.Rotate(transform.up, xRotation);
        //playerCamera.Rotate(-playerBody.right, yRotation);
    }

    void HandleMovement()
    {
        Vector3 deltaMovement = new Vector3(0, 0, 0);

        // Forward
        if (Input.GetKey(KeyCode.W)) deltaMovement += movementSpeed * playerBody.forward * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S)) deltaMovement -= movementSpeed * playerBody.forward * Time.deltaTime;

        // Backwards
        if (Input.GetKey(KeyCode.D)) deltaMovement += movementSpeed * playerBody.right * Time.deltaTime;
        else if (Input.GetKey(KeyCode.A)) deltaMovement -= movementSpeed * playerBody.right * Time.deltaTime;

        // Apply movement
        playerBody.position += deltaMovement;

    }
}
