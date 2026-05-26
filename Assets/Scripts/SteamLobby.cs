using UnityEngine;
using Mirror;
using Steamworks;
using Steamworks.Data;

public class SteamLobby : MonoBehaviour
{
    private NetworkManager networkManager;
    private const string HostAddressKey = "HostAddress";

    private Lobby? currentLobby;

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

    // =========================
    // LOBBY HOST
    // =========================

    public async void HostLobby()
    {
        Debug.Log("Creando Lobby...");

        try
        {
            await SteamMatchmaking.CreateLobbyAsync(4);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creando lobby: {e.Message}");
        }
    }

    // =========================
    // STEAM EVENTS
    // =========================

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError("Error creando lobby");
            return;
        }

        networkManager.StartHost();

        lobby.SetFriendsOnly();
        lobby.SetJoinable(true);

        lobby.SetData(HostAddressKey, SteamClient.SteamId.ToString());

        currentLobby = lobby;

        Debug.Log("Lobby creado correctamente.");
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendId)
    {
        Debug.Log($"Invitación recibida de: {friendId}");

        await lobby.Join();
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (networkManager.mode == NetworkManagerMode.Host)
            return;

        Debug.Log("Uniéndose a la partida...");

        networkManager.StartClient();
    }

    // =========================
    // PUBLIC API (para otros scripts)
    // =========================

    public bool IsHost()
    {
        return networkManager != null &&
               networkManager.mode == NetworkManagerMode.Host;
    }

    public bool HasLobby()
    {
        return currentLobby.HasValue;
    }

    public ulong GetLobbyId()
    {
        return currentLobby.Value.Id;
    }

    public void LeaveLobby()
    {
        if (currentLobby.HasValue)
        {
            currentLobby.Value.Leave();
            currentLobby = null;
        }
    }
}