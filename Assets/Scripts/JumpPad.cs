using UnityEngine;

// Haengt am Trigger-Objekt des Jump-Pads.
// Beruehrt der Spieler die Zone, wird er auf die eingestellte Hoehe katapultiert.
[RequireComponent(typeof(Collider))]
public class JumpPad : MonoBehaviour
{
    [Tooltip("Sprunghoehe in Metern (J1 = 7, J2 = 9)")]
    public float launchHeight = 7f;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<FirstPersonController>();
        if (player != null)
            player.Launch(launchHeight);
    }
}
