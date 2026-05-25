using UnityEngine;
using Mirror;
using Steamworks;
using Steamworks.Data;

public class SteamLobby : MonoBehaviour
{
    private NetworkManager networkManager;
    private const string HostAddressKey = "HostAddress";
    
    // Nueva variable para guardar la sala actual y poder interactuar con ella desde la UI
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

    private void OnGUI()
    {
        if (networkManager.mode == NetworkManagerMode.Offline)
        {
            if (GUI.Button(new Rect(10, 10, 220, 30), "Hostear Partida (Steam Lobby)"))
            {
                HostLobby();
            }
        }
        else
        {
            GUI.Label(new Rect(10, 10, 300, 30), "Conectado. Esperando jugadores...");

            // NUEVA MEJORA DE UX: Botón directo para invitar amigos
            // Solo lo mostramos si somos el anfitrión y la sala ya existe
            if (networkManager.mode == NetworkManagerMode.Host && currentLobby.HasValue)
            {
                if (GUI.Button(new Rect(10, 50, 220, 30), "Invitar Amigos a la Sala"))
                {
                    // Esto fuerza a Steam a abrir el Overlay directamente en la ventana de invitaciones
                    SteamFriends.OpenGameInviteOverlay(currentLobby.Value.Id);
                }
            }
        }
    }

    public async void HostLobby()
    {
        Debug.Log("Creando Lobby en Steam...");
        await SteamMatchmaking.CreateLobbyAsync(4);
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError("No se pudo crear el lobby de Steam.");
            return;
        }

        networkManager.StartHost();
        
        // LA SOLUCIÓN AL PROBLEMA: 
        // Cambiamos la visibilidad para que Steam habilite el botón derecho de "Invitar a jugar"
        lobby.SetFriendsOnly(); // (También podrías usar lobby.SetPublic())
        
        lobby.SetData(HostAddressKey, SteamClient.SteamId.ToString());
        lobby.SetJoinable(true);

        // Guardamos la sala en nuestra variable para que el botón de la UI sepa a dónde invitar
        currentLobby = lobby;

        Debug.Log("Lobby creado. El botón de invitar ya está activo en Steam.");
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendId)
    {
        Debug.Log("Aceptando la invitación de: " + friendId);
        await lobby.Join();
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (networkManager.mode == NetworkManagerMode.Host) return;

        string hostAddress = lobby.GetData(HostAddressKey);
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        Debug.Log("Uniéndose a la partida del amigo...");
    }
}