using UnityEngine;
using UnityEngine.InputSystem;

// Grappling Hook: E zielt auf einen Greifpunkt (Layer "Grapple") und zieht den Spieler hin.
// E oder Leertaste waehrend des Zugs loest den Haken wieder.
[RequireComponent(typeof(FirstPersonController))]
public class GrapplingHook : MonoBehaviour
{
    public float range = 20f;
    public LayerMask grappleMask;
    public Transform cameraTransform;
    public Color ropeColor = new Color(1f, 0.55f, 0.1f);
    public float ropeWidth = 0.05f;

    FirstPersonController player;
    LineRenderer rope;
    Vector3 hookPoint;

    void Awake()
    {
        player = GetComponent<FirstPersonController>();

        if (grappleMask.value == 0)
            grappleMask = LayerMask.GetMask("Grapple");

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Seil als einfache Linie
        rope = gameObject.AddComponent<LineRenderer>();
        rope.positionCount = 2;
        rope.startWidth = ropeWidth;
        rope.endWidth = ropeWidth;
        rope.material = new Material(Shader.Find("Sprites/Default"));
        rope.startColor = ropeColor;
        rope.endColor = ropeColor;
        rope.enabled = false;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame)
        {
            if (player.IsGrappling) player.StopGrapple();
            else TryGrapple();
        }

        if (player.IsGrappling && kb.spaceKey.wasPressedThisFrame)
            player.StopGrapple();

        rope.enabled = player.IsGrappling;
    }

    void LateUpdate()
    {
        if (!player.IsGrappling) return;

        // Seil startet leicht rechts unter der Kamera
        Vector3 start = cameraTransform.position
                      + cameraTransform.right * 0.3f
                      - cameraTransform.up * 0.3f
                      + cameraTransform.forward * 0.5f;
        rope.SetPosition(0, start);
        rope.SetPosition(1, hookPoint);
    }

    void TryGrapple()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward,
                out RaycastHit hit, range, grappleMask, QueryTriggerInteraction.Collide))
        {
            hookPoint = hit.collider.transform.position;
            player.StartGrapple(hookPoint);
        }
    }
}
