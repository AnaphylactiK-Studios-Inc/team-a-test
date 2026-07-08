using System;
using System.Collections;
using UnityEngine;

public class MouseMove : MonoBehaviour
{
    public float mouseSense = 200f;
    public Transform playerBody;
    [SerializeField] private GameObject menuScreen;
    [SerializeField] private RectTransform reticle;

    public Camera playerCam;

    float xRotation = 0f;

    bool menu = false;
    public bool lookLocked = false;
    [SerializeField] private PlayerMovement playerMovement;

    void Awake()
    {
        // Fallback to the root object so yaw still works if playerBody is not set in the inspector.
        if (playerBody == null)
        {
            playerBody = transform.root;
        }
    }

    void Start()
    {
        LockCursor(true);

        if (menuScreen != null)
        {
            menuScreen.SetActive(menu);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
            if (menuScreen != null)
            {
                menuScreen.SetActive(menu);
            }
        }

        if (playerBody == null)
        {
            return;
        }

        if (lookLocked) return;


        if (!menu)
        {
            reticle.position = Input.mousePosition;
        }

        // xRotation -= mousey * mouseSense;
        // xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // playerBody.Rotate(Vector3.up * mousex * mouseSense);
    }

    public void SetSensitivity(float value) => mouseSense = value;

    public void FaceDirection(Quaternion targetYaw)
    {
        xRotation = 0f;
        transform.localRotation = Quaternion.identity;
        if (playerBody != null)
            playerBody.rotation = Quaternion.Euler(0f, targetYaw.eulerAngles.y, 0f);
    }

    public void LockCursor(bool locked)
    {
        menu = !locked;
        if (locked)
        {
            // Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Resume game
            Time.timeScale = 1f;

            // enable movement
            playerMovement.moveLocked = false;
        }
        else
        {
            Cursor.visible = true;

            // Pause game
            Time.timeScale = 0f;

            // disable movement
            playerMovement.moveLocked = true;
        }
    }

    private void ToggleMenu()
    {
        LockCursor(menu);
    }
}
