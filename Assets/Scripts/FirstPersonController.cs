using UnityEngine;
using UnityEngine.InputSystem;

// First-Person-Controller: WASD laufen, Maus umschauen, Leertaste springen.
// Double Jump, Dash usw. kommen in den naechsten Issues dazu.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Bewegung")]
    public float moveSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -25f;
    public int maxJumps = 2; // 2 = Double Jump

    [Header("Dash")]
    public float dashDistance = 8f;        // Strecke eines Dashs in Metern
    public float dashDuration = 0.18f;     // Dauer in Sekunden
    public int maxDashCharges = 2;         // wie viele Dashes gespeichert werden
    public float dashRechargeTime = 2f;    // Sekunden, bis eine Ladung zurueckkommt

    [Header("Maus")]
    public float mouseSensitivity = 0.1f;
    public Transform cameraTransform;

    CharacterController controller;
    Vector3 velocity;
    float pitch;
    int jumpsLeft;

    int dashCharges;
    float dashRecharge;
    float dashTimer;
    Vector3 dashDirection;

    // fuer das HUD spaeter
    public int DashCharges => dashCharges;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        dashCharges = maxDashCharges;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        LockCursor(true);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || Mouse.current == null) return;

        // Esc gibt die Maus frei, Klick fangt sie wieder ein
        if (kb.escapeKey.wasPressedThisFrame) LockCursor(false);
        if (Mouse.current.leftButton.wasPressedThisFrame) LockCursor(true);

        if (Cursor.lockState == CursorLockMode.Locked) Look();
        Move(kb);
    }

    void Look()
    {
        Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        transform.Rotate(Vector3.up * delta.x);

        pitch = Mathf.Clamp(pitch - delta.y, -89f, 89f);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void Move(Keyboard kb)
    {
        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 input = transform.right * x + transform.forward * z;
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Dash: Shift druecken, in Laufrichtung (ohne Eingabe nach vorne)
        if (kb.leftShiftKey.wasPressedThisFrame && dashCharges > 0 && dashTimer <= 0f)
        {
            dashDirection = input.sqrMagnitude > 0.01f ? input.normalized : transform.forward;
            dashTimer = dashDuration;
            dashCharges--;
        }

        RechargeDash();

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            velocity.y = 0f; // waehrend des Dashs schwebt man waagerecht
            controller.Move(dashDirection * (dashDistance / dashDuration) * Time.deltaTime);
            return;
        }

        if (controller.isGrounded)
        {
            jumpsLeft = maxJumps;
            if (velocity.y < 0f) velocity.y = -2f;
        }

        // Springen: am Boden der erste Sprung, in der Luft der Double Jump
        if (kb.spaceKey.wasPressedThisFrame && jumpsLeft > 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsLeft--;
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 move = input * moveSpeed + Vector3.up * velocity.y;
        controller.Move(move * Time.deltaTime);
    }

    // Wird z.B. von Jump-Pads aufgerufen: schleudert den Spieler auf die gegebene Hoehe
    public void Launch(float height)
    {
        velocity.y = Mathf.Sqrt(height * -2f * gravity);
        dashTimer = 0f;
        jumpsLeft = maxJumps; // nach dem Pad ist ein Double Jump moeglich
    }

    void RechargeDash()
    {
        if (dashCharges >= maxDashCharges)
        {
            dashRecharge = 0f;
            return;
        }

        dashRecharge += Time.deltaTime;
        if (dashRecharge >= dashRechargeTime)
        {
            dashCharges++;
            dashRecharge = 0f;
        }
    }

    static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
