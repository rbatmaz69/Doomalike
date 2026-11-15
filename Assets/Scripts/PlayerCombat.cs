using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// Schiessen, Leben und Respawn im Netzwerk.
// Linksklick schiesst einen Strahl aus der Kamera. Der Server zieht dem Getroffenen Leben ab.
// Bei 0 Leben wird der Getroffene am letzten Checkpoint wiederbelebt, der Schuetze bekommt einen Treffer-Punkt.
public class PlayerCombat : NetworkBehaviour
{
    public Transform cameraTransform;
    public int maxHealth = 100;
    public int damage = 25;
    public float range = 100f;
    public float fireRate = 0.3f;

    [Header("Kugel (nur Optik)")]
    public Material ballMaterial;
    public float ballSpeed = 60f;
    public float ballSize = 0.3f;

    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    public NetworkVariable<int> kills = new NetworkVariable<int>(0);

    float nextShot;
    Respawn respawn;

    public override void OnNetworkSpawn()
    {
        respawn = GetComponent<Respawn>();
        if (IsServer) health.Value = maxHealth;
    }

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        var mouse = Mouse.current;
        if (mouse == null || Cursor.lockState != CursorLockMode.Locked) return;

        if (mouse.leftButton.wasPressedThisFrame && Time.time >= nextShot)
        {
            nextShot = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        Vector3 start = cameraTransform.position
                      + cameraTransform.right * 0.25f
                      - cameraTransform.up * 0.2f
                      + cameraTransform.forward * 0.6f;
        Vector3 end = cameraTransform.position + cameraTransform.forward * range;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward,
                out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;

            var target = hit.collider.GetComponentInParent<PlayerCombat>();
            if (target != null && target != this)
                DamageRpc(target.NetworkObject);
        }

        BallRpc(start, end);
    }

    // Laeuft bei allen Spielern: zeigt die fliegende Kugel
    [Rpc(SendTo.Everyone)]
    void BallRpc(Vector3 start, Vector3 end)
    {
        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(ball.GetComponent<Collider>()); // reine Optik, keine Kollision

        ball.transform.position = start;
        ball.transform.localScale = Vector3.one * ballSize;

        if (ballMaterial != null)
            ball.GetComponent<Renderer>().sharedMaterial = ballMaterial;

        ball.AddComponent<FlyingBall>().Init(end, ballSpeed);
    }

    // Laeuft auf dem Server: zieht Leben ab und prueft, ob der Getroffene ausgeschieden ist
    [Rpc(SendTo.Server)]
    void DamageRpc(NetworkObjectReference targetRef)
    {
        if (!targetRef.TryGet(out NetworkObject targetObject)) return;

        var target = targetObject.GetComponent<PlayerCombat>();
        if (target == null) return;

        target.health.Value -= damage;

        if (target.health.Value <= 0)
        {
            kills.Value++;
            target.health.Value = maxHealth;
            target.RespawnRpc();
        }
    }

    // Laeuft beim Besitzer des Getroffenen
    [Rpc(SendTo.Owner)]
    void RespawnRpc()
    {
        if (respawn != null) respawn.ForceRespawn();
    }

    // Einfache Anzeige fuer den eigenen Spieler
    void OnGUI()
    {
        if (!IsOwner || !IsSpawned) return;

        GUI.skin.label.fontSize = 24;
        GUI.Label(new Rect(20, Screen.height - 90, 400, 40), "Leben: " + health.Value);
        GUI.Label(new Rect(20, Screen.height - 50, 400, 40), "Treffer: " + kills.Value);

        // Tastenbelegung unten rechts
        var legend = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerRight, fontSize = 18 };
        GUI.Label(new Rect(Screen.width - 420f, Screen.height - 230f, 400f, 210f),
            "WASD  Laufen\n" +
            "Space  Springen (2x = Double Jump)\n" +
            "Shift  Dash\n" +
            "E  Enterhaken\n" +
            "Linksklick  Schiessen\n" +
            "An der Wand: W klettern, Space Wandsprung\n" +
            "Esc  Maus freigeben",
            legend);

        // Fadenkreuz in der Bildmitte
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 28 };
        GUI.Label(new Rect(Screen.width / 2f - 20f, Screen.height / 2f - 20f, 40f, 40f), "+", style);
    }
}
