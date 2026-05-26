using UnityEngine;
using Mirror;
using Steamworks;
using Steamworks.Data;

public class SteamLobby : MonoBehaviour
{
    private NetworkManager networkManager;
    private const string HostAddressKey = "HostAddress";

    private Lobby? currentLobby;

    [Header("UI")]
    public GameObject mainMenuPanel;   // Panel con todos los botones del menú
    public GameObject inviteButton;    // Botón "Invitar amigos"

    private void Start()
    {
        Application.targetFrameRate = 60;

        networkManager = GetComponent<NetworkManager>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;

        // Estado inicial UI
        if (inviteButton != null)
            inviteButton.SetActive(false);
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
    // BOTONES UI
    // =========================

    public async void HostLobby()
    {
        Debug.Log("Creando Lobby...");

        await SteamMatchmaking.CreateLobbyAsync(4);

        // 👇 OCULTAR MENÚ Y MOSTRAR INVITAR
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (inviteButton != null)
            inviteButton.SetActive(true);
    }

    public void InviteFriends()
    {
        if (networkManager.mode == NetworkManagerMode.Host && currentLobby.HasValue)
        {
            SteamFriends.OpenGameInviteOverlay(currentLobby.Value.Id);
        }
    }

    public void OpenSteamFriendsList()
    {
        SteamFriends.OpenOverlay("friends");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendId)
    {
        await lobby.Join();
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (networkManager.mode == NetworkManagerMode.Host)
            return;

        networkManager.StartClient();
    }
}