using UnityEngine;
using Mirror;
using Steamworks;
using Steamworks.Data;

public class SteamLobby : MonoBehaviour
{
    private NetworkManager networkManager;
    private const string HostAddressKey = "HostAddress";

    private void Start()
    {
        Application.targetFrameRate = 60;
        networkManager = GetComponent<NetworkManager>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
    }

    // ==========================================
    // ESTE ES EL NUEVO HUD EXCLUSIVO PARA STEAM
    // ==========================================
    private void OnGUI()
    {
        // Si no estamos conectados a nada, mostramos el botón de crear partida
        if (networkManager.mode == NetworkManagerMode.Offline)
        {
            if (GUI.Button(new Rect(10, 10, 220, 30), "Hostear Partida (Steam Lobby)"))
            {
                HostLobby();
            }
        }
        else
        {
            // Si ya estamos conectados, mostramos un texto
            GUI.Label(new Rect(10, 10, 300, 30), "Conectado. Presiona Shift+Tab para invitar.");
        }
    }

    public async void HostLobby()
    {
        Debug.Log("Creando Lobby en Steam...");
        // Creamos un lobby público/amigos para 4 personas
        await SteamMatchmaking.CreateLobbyAsync(4);
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError("No se pudo crear el lobby de Steam.");
            return;
        }

        // Solo cuando Steam confirma el lobby, encendemos Mirror
        networkManager.StartHost();
        lobby.SetData(HostAddressKey, SteamClient.SteamId.ToString());
        lobby.SetJoinable(true);

        Debug.Log("Lobby creado con éxito. Ya puedes invitar amigos.");
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendId)
    {
        Debug.Log("Aceptando la invitación de: " + friendId);
        // Nos unimos al lobby de Steam al aceptar la invitación
        await lobby.Join();
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (networkManager.mode == NetworkManagerMode.Host) return;

        // Sacamos la ID del Host y conectamos Mirror a él
        string hostAddress = lobby.GetData(HostAddressKey);
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        Debug.Log("Uniéndose a la partida del amigo...");
    }
}