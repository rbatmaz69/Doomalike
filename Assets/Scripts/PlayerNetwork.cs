using Unity.Netcode;
using UnityEngine;

// Liegt am Player-Prefab. Schaltet Steuerung, Kamera und Ansicht je nach Besitzer:
// Der eigene Spieler wird gesteuert und hat Kamera und Ohr, alle anderen sind nur ein sichtbarer Koerper.
// Voraussetzung: FirstPersonController, GrapplingHook und Respawn sind im Prefab ausgeschaltet.
public class PlayerNetwork : NetworkBehaviour
{
    public Camera playerCamera;
    public Renderer[] bodyRenderers; // eigener Koerper wird fuer den Besitzer ausgeblendet

    public override void OnNetworkSpawn()
    {
        bool mine = IsOwner;

        playerCamera.enabled = mine;
        var listener = playerCamera.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = mine;

        foreach (var r in bodyRenderers)
            r.enabled = !mine;

        if (!mine) return;

        // Steuerung nur fuer den eigenen Spieler einschalten
        GetComponent<FirstPersonController>().enabled = true;
        GetComponent<GrapplingHook>().enabled = true;

        var respawn = GetComponent<Respawn>();
        respawn.enabled = true;

        // Startpunkt aus der Szene
        var spawn = GameObject.Find("PlayerSpawn");
        if (spawn != null)
            respawn.SetSpawn(spawn.transform.position, spawn.transform.rotation);
    }
}
