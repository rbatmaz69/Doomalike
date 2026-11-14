using UnityEngine;
using UnityEngine.InputSystem;

// First-Person-Controller: WASD laufen, Maus umschauen, Leertaste springen,
// Double Jump, Dash, Jump-Pads (Launch) und Wand-Mechanik (halten, klettern, Wandsprung).
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

    [Header("Wand")]
    public float climbSpeed = 4f;          // Klettergeschwindigkeit (W an der Wand)
    public float wallStamina = 1.5f;       // Sekunden Halten/Klettern, bis man abrutscht
    public float wallSlideSpeed = 3f;      // Rutschgeschwindigkeit ohne Stamina
    public float wallJumpHeight = 2.5f;    // Hoehe des Wandsprungs
    public float wallJumpPush = 9f;        // Schub von der Wand weg
    public float pushDecay = 20f;          // wie schnell der Schub abklingt

    [Header("Grappling Hook")]
    public float grappleSpeed = 25f;        // Zuggeschwindigkeit zum Greifpunkt
    public float grappleStopDistance = 1.5f; // ab hier loest sich der Haken
    public float grappleMomentum = 0.4f;    // wie viel Schwung am Ende bleibt

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

    // Wand
    Vector3 wallNormal;
    float wallContactTimer;
    float wallTimeLeft;
    Vector3 push; // waagerechter Schub (Wandsprung)

    // Grappling Hook
    bool grappling;
    Vector3 grappleTarget;

    // fuer das HUD spaeter
    public int DashCharges => dashCharges;
    public bool IsGrappling => grappling;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        dashCharges = maxDashCharges;
        wallTimeLeft = wallStamina;

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

        // Haken: zum Greifpunkt ziehen, bis man nah genug ist
        if (grappling)
        {
            Vector3 toTarget = grappleTarget - transform.position;
            if (toTarget.magnitude <= grappleStopDistance)
            {
                StopGrapple();
            }
            else
            {
                velocity.y = 0f;
                push = Vector3.zero;
                controller.Move(toTarget.normalized * grappleSpeed * Time.deltaTime);
                return;
            }
        }

        if (controller.isGrounded)
        {
            jumpsLeft = maxJumps;
            wallTimeLeft = wallStamina;
            push = Vector3.zero;
            if (velocity.y < 0f) velocity.y = -2f;
        }

        // Wandkontakt kommt aus OnControllerColliderHit und gilt kurz nach
        wallContactTimer -= Time.deltaTime;
        bool onWall = wallContactTimer > 0f && !controller.isGrounded;

        bool jumpPressed = kb.spaceKey.wasPressedThisFrame;

        if (onWall && jumpPressed)
        {
            // Wandsprung: nach oben und von der Wand weg
            velocity.y = Mathf.Sqrt(wallJumpHeight * -2f * gravity);
            push = wallNormal * wallJumpPush;
            jumpsLeft = Mathf.Max(jumpsLeft, 1);
            wallContactTimer = 0f;
            onWall = false;
        }
        else if (onWall)
        {
            if (wallTimeLeft > 0f)
            {
                // Halten bzw. Klettern (W), solange Stamina da ist
                wallTimeLeft -= Time.deltaTime;
                velocity.y = z > 0f ? climbSpeed : 0f;
            }
            else
            {
                // Stamina leer: langsam abrutschen
                velocity.y = Mathf.Max(velocity.y + gravity * Time.deltaTime, -wallSlideSpeed);
            }
        }
        else if (jumpPressed && jumpsLeft > 0)
        {
            // Springen: am Boden der erste Sprung, in der Luft der Double Jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsLeft--;
        }

        if (!onWall) velocity.y += gravity * Time.deltaTime;

        push = Vector3.MoveTowards(push, Vector3.zero, pushDecay * Time.deltaTime);

        Vector3 move = input * moveSpeed + push + Vector3.up * velocity.y;
        // leicht zur Wand druecken, damit der Kontakt beim Halten nicht abreisst
        if (onWall) move -= wallNormal * 1f;
        controller.Move(move * Time.deltaTime);
    }

    // Wird vom CharacterController bei Kollisionen aufgerufen
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag("ClimbWall")) return;
        if (Mathf.Abs(hit.normal.y) > 0.3f) return; // nur senkrechte Flaechen

        wallNormal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
        wallContactTimer = 0.15f;
    }

    // Wird vom GrapplingHook aufgerufen: zieht den Spieler zum Punkt
    public void StartGrapple(Vector3 target)
    {
        grappleTarget = target;
        grappling = true;
        dashTimer = 0f;
        wallContactTimer = 0f;
    }

    // Loest den Haken und behaelt etwas Schwung in Zugrichtung
    public void StopGrapple()
    {
        if (!grappling) return;
        grappling = false;

        Vector3 dir = (grappleTarget - transform.position).normalized;
        push = new Vector3(dir.x, 0f, dir.z) * grappleSpeed * grappleMomentum;
        velocity.y = Mathf.Max(0f, dir.y * grappleSpeed * grappleMomentum);
        jumpsLeft = maxJumps; // nach dem Haken ist wieder ein Sprung moeglich
    }

    // Wird z.B. von Jump-Pads aufgerufen: schleudert den Spieler auf die gegebene Hoehe
    public void Launch(float height)
    {
        grappling = false;
        velocity.y = Mathf.Sqrt(height * -2f * gravity);
        dashTimer = 0f;
        wallContactTimer = 0f;
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
