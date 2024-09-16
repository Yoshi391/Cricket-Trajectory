using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public float movementSpeed = 10.0f;   
    public float mouseSensitivity = 100.0f;  
    public float maxLookAngle = 80.0f;     

    private float yaw = 0.0f;    
    private float pitch = 0.0f;  
    private bool isFlyingEnabled = false; 

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isFlyingEnabled = !isFlyingEnabled;
            Cursor.lockState = isFlyingEnabled ? CursorLockMode.Locked : CursorLockMode.None; 
            Cursor.visible = !isFlyingEnabled;
        }

        
        if (isFlyingEnabled)
        {
            HandleCameraMovement();
            HandleCameraRotation();
        }
    }

    
    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    private void HandleCameraMovement()
    {
        float moveForward = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        float moveRight = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        float moveUp = 0.0f;

        if (Input.GetKey(KeyCode.Space))
            moveUp = movementSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.LeftShift))
            moveUp = -movementSpeed * Time.deltaTime;

        transform.Translate(new Vector3(moveRight, moveUp, moveForward));
    }
}
