using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTeleportSkill : MonoBehaviourWithData<PlayerTeleportSkill.TeleportData>
{
    [Serializable]
    public class TeleportData
    {
        public float Radius = 25f;
        public GameObject TeleportPoint;
    }

    public ContactFilter2D contactFilter;
    private InputActions inputActions;

    private bool StartLightFiltr;
    private bool IsTeleported;
    
    private Vector2? TargetPosition;
    private Vector2 aimPosition;
    private Camera mainCamera;

    private void Awake()
    {
        inputActions = new InputActions();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.TeleportAim.performed += OnTeleportAim;
        inputActions.Player.TeleportAim.canceled += OnTeleportAim;
        inputActions.Player.TeleportExecute.performed += OnTeleportExecute;
    }

    private void OnDisable()
    {
        inputActions.Player.TeleportAim.performed -= OnTeleportAim;
        inputActions.Player.TeleportAim.canceled -= OnTeleportAim;
        inputActions.Player.TeleportExecute.performed -= OnTeleportExecute;
        inputActions.Player.Disable();
    }

    private void Start()
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = true;
    }

    private void OnDestroy()
    {
        Data.TeleportPoint.SetActive(false);
    }

    private void OnTeleportAim(InputAction.CallbackContext context)
    {
        aimPosition = context.ReadValue<Vector2>();
    }

    private void OnTeleportExecute(InputAction.CallbackContext context)
    {
        Teleportation();
    }

    private void Update()
    {
        UpdateTeleportTarget();
    }

    private void UpdateTeleportTarget()
    {
        // Read the current aim position every frame
        aimPosition = inputActions.Player.TeleportAim.ReadValue<Vector2>();

        // Get the center of the screen in world coordinates
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        // Calculate the direction vector based on input device
        Vector2 direction;

        // Check if using gamepad (right stick will have non-zero values when moved)
        if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
        {
            // Gamepad: use right stick direction directly
            direction = Gamepad.current.rightStick.ReadValue().normalized;
        }
        else
        {
            // Mouse/Touch: calculate direction from screen center to aim position
            direction = (aimPosition - (Vector2)screenCenter).normalized;
        }

        // Visualize the direction
        Debug.DrawRay(transform.position, direction * Data.Radius, Color.green);

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, Data.Radius);
        foreach (RaycastHit2D hit in hits)
        {
            if (StartLightFiltr && hit.collider?.tag == "Teleport")
            {
                TargetPosition = hit.point;

                Data.TeleportPoint.SetActive(true);
                Data.TeleportPoint.transform.position = TargetPosition.Value;

                IsTeleported = true;
            }

            if (hit.collider?.tag == "Teleport")
                StartLightFiltr = true;
        }

        if (IsTeleported == false)
        {
            TargetPosition = null;

            Data.TeleportPoint.SetActive(false);
        }
        else IsTeleported = false;
        
        StartLightFiltr = false;
    }

    private void Teleportation()
    {
        if (TargetPosition == null) return;

        transform.position = TargetPosition.Value;

        Data.TeleportPoint.SetActive(false);
        TargetPosition = null;
    }
}