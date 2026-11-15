using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

// Einfaches Menue: Host starten (zeigt einen Join-Code) oder per Join-Code beitreten.
// Die Verbindung laeuft ueber Unity Relay, also auch zwischen verschiedenen Netzwerken.
public class ConnectionMenu : MonoBehaviour
{
    public int maxPlayers = 4;

    string joinCode = "";
    string hostCode = "";
    string status = "";
    bool busy;

    async Task SignIn()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            // Eigenes Profil pro Start, damit mehrere Editor-Instanzen sich nicht in die Quere kommen
            var options = new InitializationOptions()
                .SetProfile("p" + Guid.NewGuid().ToString("N").Substring(0, 12));
            await UnityServices.InitializeAsync(options);
        }

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    async void StartHost()
    {
        busy = true;
        status = "Starte Host ...";
        try
        {
            await SignIn();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            hostCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
            status = "";
        }
        catch (Exception e)
        {
            status = "Fehler: " + e.Message;
            Debug.LogException(e);
        }
        busy = false;
    }

    async void StartClient()
    {
        busy = true;
        status = "Verbinde ...";
        try
        {
            await SignIn();

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode.Trim().ToUpper());

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
            status = "";
        }
        catch (Exception e)
        {
            status = "Fehler: " + e.Message;
            Debug.LogException(e);
        }
        busy = false;
    }

    // Zustand einmal pro Frame merken, sonst wechselt das Menue mitten im OnGUI-Aufruf
    bool listening;

    void Update()
    {
        listening = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    }

    void OnGUI()
    {
        // Etwas groessere Schrift, damit es auf dem Retina-Display lesbar ist
        GUI.skin.label.fontSize = 20;
        GUI.skin.button.fontSize = 20;
        GUI.skin.textField.fontSize = 20;

        var nm = NetworkManager.Singleton;
        GUILayout.BeginArea(new Rect(20, 20, 360, 400));

        if (nm == null)
        {
            GUILayout.Label("Kein NetworkManager in der Szene");
        }
        else if (listening)
        {
            GUILayout.Label(nm.IsHost ? "Host  |  Join-Code: " + hostCode : "Client verbunden");
        }
        else if (!busy)
        {
            if (GUILayout.Button("Host starten", GUILayout.Height(44)))
                StartHost();

            GUILayout.Space(12);
            GUILayout.Label("Join-Code:");
            joinCode = GUILayout.TextField(joinCode, 6, GUILayout.Height(36));

            if (GUILayout.Button("Beitreten", GUILayout.Height(44)))
                StartClient();
        }

        if (!string.IsNullOrEmpty(status))
            GUILayout.Label(status);

        GUILayout.EndArea();
    }
}
