using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Checkpoints, KillZones und Ziel: Trigger mit Tag "Checkpoint" speichern den Respawn-Punkt,
// Trigger mit Tag "KillZone" setzen den Spieler dorthin zurueck,
// Trigger mit Tag "Goal" beendet das Level (Zeit wird angezeigt, R startet neu).
// Liegt am Player. Ohne Checkpoint geht es zurueck an den Startpunkt (Position beim Spielstart).
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(FirstPersonController))]
public class Respawn : MonoBehaviour
{
    CharacterController controller;
    FirstPersonController player;

    Vector3 respawnPosition;
    Quaternion respawnRotation;

    float startTime;
    float finishTime;

    // fuer das HUD spaeter
    public int CheckpointsReached { get; private set; }
    public bool GoalReached { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        player = GetComponent<FirstPersonController>();

        // Startpunkt = dort, wo der Spieler beim Start steht
        respawnPosition = transform.position;
        respawnRotation = transform.rotation;

        startTime = Time.time;
    }

    void Update()
    {
        // Nach dem Ziel: R startet das Level neu
        if (GoalReached && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KillZone"))
        {
            DoRespawn();
        }
        else if (other.CompareTag("Goal"))
        {
            if (!GoalReached)
            {
                GoalReached = true;
                finishTime = Time.time - startTime;
            }
        }
        else if (other.CompareTag("Checkpoint"))
        {
            // Neuer Punkt? Dann merken (gleicher Checkpoint zaehlt nicht doppelt)
            if (respawnPosition != other.transform.position)
            {
                respawnPosition = other.transform.position;
                respawnRotation = Quaternion.Euler(0f, other.transform.eulerAngles.y, 0f);
                CheckpointsReached++;
            }
        }
    }

    // Einfache Anzeige, bis das richtige HUD kommt
    void OnGUI()
    {
        if (!GoalReached) return;

        int minutes = (int)(finishTime / 60f);
        float seconds = finishTime - minutes * 60f;

        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 40,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 60f), "ZIEL ERREICHT", style);
        style.fontSize = 24;
        GUI.Label(new Rect(0, Screen.height * 0.35f + 60f, Screen.width, 40f),
            $"Zeit {minutes}:{seconds:00.0}   |   R = Neustart", style);
    }

    void DoRespawn()
    {
        // CharacterController muss fuers Teleportieren kurz aus sein
        controller.enabled = false;
        transform.SetPositionAndRotation(respawnPosition, respawnRotation);
        controller.enabled = true;

        player.ResetMotion();
    }
}
