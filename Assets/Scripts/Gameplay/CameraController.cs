using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple camera controller to move around world, floating style
public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8.0f;
    [SerializeField] private float shiftMultiplier = 2.0f;

    [SerializeField] Vector3 moveInput;

    [SerializeField] private float mouseSensitivity = 10.0f;

    [SerializeField] Vector2 currentRotation;
    [SerializeField] Vector2 mouseInput;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Mouse movement
        mouseInput.x = -Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseInput.y = Input.GetAxis("Mouse X") * mouseSensitivity;

        currentRotation += mouseInput;
        currentRotation.x = Mathf.Clamp(currentRotation.x, -90f, 90f);
        transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);


        //WASD Movement
        moveInput.x = Input.GetAxisRaw("Horizontal") * moveSpeed;
        moveInput.z = Input.GetAxisRaw("Vertical") * moveSpeed;

        //Check shift key
        if(Input.GetKey(KeyCode.LeftShift))
        {
            moveInput *= shiftMultiplier;
        }
        transform.position += (moveInput.x * transform.right + moveInput.z * transform.forward) * Time.deltaTime;


        
    }
}
