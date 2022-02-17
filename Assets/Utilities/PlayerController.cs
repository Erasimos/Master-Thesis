using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform playerBody;
    public Transform playerCamera;

    private float movementSpeed = 50f;
    private float lookSpeed = 100f;
    private float xRotation = 0f;

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
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerBody.Rotate(transform.up, mouseX);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //playerBody.Rotate(transform.up, xRotation);
        //playerCamera.Rotate(transform.up, xRotation);
        
        //playerBody.Rotate(transform.right, yRotation);
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
