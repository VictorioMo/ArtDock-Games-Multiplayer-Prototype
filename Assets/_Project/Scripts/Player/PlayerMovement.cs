using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.12f;

    [Header("Visuals")]
    [SerializeField] private MeshRenderer playerRenderer;

    private CharacterController _controller;

    private float _verticalVelocity;
    private float _cameraPitch;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        // Only the owning player controls this character.
        _controller.enabled = IsOwner;

        // Only the owning player uses this camera.
        playerCamera.gameObject.SetActive(IsOwner);

        // Hide our own capsule to avoid blocking the camera.
        if (playerRenderer != null)
            playerRenderer.enabled = !IsOwner;

        if (!IsOwner)
            return;

        // Disable the original scene camera.
        Camera sceneCamera = Camera.main;

        if (sceneCamera != null && sceneCamera != playerCamera)
            sceneCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        HandleCursor();

        HandleMovement();
        HandleMouseLook();
    }

    private void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        Vector3 input = Vector3.zero;

        if (keyboard.wKey.isPressed)
            input.z += 1f;

        if (keyboard.sKey.isPressed)
            input.z -= 1f;

        if (keyboard.aKey.isPressed)
            input.x -= 1f;

        if (keyboard.dKey.isPressed)
            input.x += 1f;

        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 movement =
            transform.TransformDirection(input) * moveSpeed;

        // Ensure we dont accumulate negative velocity on ground
        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += gravity * Time.deltaTime;

        movement.y = _verticalVelocity;

        _controller.Move(movement * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        // Horizontal rotation rotates the entire player.
        transform.Rotate(
            Vector3.up,
            mouseDelta.x * mouseSensitivity
        );

        // Vertical rotation affects only the camera.
        _cameraPitch -= mouseDelta.y * mouseSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -85f, 85f);

        cameraPivot.localRotation =
            Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    private void HandleCursor()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}