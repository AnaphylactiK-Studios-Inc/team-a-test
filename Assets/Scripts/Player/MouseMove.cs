using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseMove : MonoBehaviour
{
    public float mouseSense = 200f;
    public Transform playerBody;
    [SerializeField] private GameObject menuScreen;
    [SerializeField] private RectTransform reticle;

    public Camera playerCam;

    [Header("Controller Reticle")]
    [SerializeField] float reticleControllerSpeed = 800f;
    [SerializeField] bool invertStickY = false;
    public bool reticleEnabled = true;
    public Vector2 ReticleScreenPos => _reticlePos;

    InputSystem_Actions _input;
    Vector2 _reticlePos;

    float xRotation = 0f;

    bool menu = false;
    public bool lookLocked = false;
    [SerializeField] private PlayerMovement playerMovement;

    void Awake()
    {
        if (playerBody == null)
            playerBody = transform.root;

        _input = new InputSystem_Actions();
    }

    void OnEnable()  => _input.Enable();
    void OnDisable() => _input.Disable();

    void Start()
    {
        _reticlePos = new Vector2(Screen.width / 2f, Screen.height / 2f);
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


        if (!menu && reticleEnabled)
        {
            Vector2 stick = _input.Game.Look.ReadValue<Vector2>();
            stick.y *= invertStickY ? -1f : 1f;
            _reticlePos += stick * reticleControllerSpeed * Time.unscaledDeltaTime;
            _reticlePos.x = Mathf.Clamp(_reticlePos.x, 0f, Screen.width);
            _reticlePos.y = Mathf.Clamp(_reticlePos.y, 0f, Screen.height);
            reticle.position = _reticlePos;
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
