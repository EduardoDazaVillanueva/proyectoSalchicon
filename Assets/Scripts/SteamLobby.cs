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

        // Suscribirnos a los eventos del sistema de Lobbies de Steam
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void Update()
    {
        // Facepunch requiere que actualicemos los callbacks frecuentemente
        SteamClient.RunCallbacks();
    }

    private void OnDestroy()
    {
        // Desuscribirnos para evitar errores de memoria al cerrar
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
    }

    // Esta función la conectaremos a tu futuro botón de "Crear Partida" en la UI
    public async void HostLobby()
    {
        // Creamos un lobby con capacidad para 4 jugadores (puedes cambiar este número)
        await SteamMatchmaking.CreateLobbyAsync(4);
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError("No se pudo crear el lobby de Steam.");
            return;
        }

        // Si el lobby se crea bien, iniciamos Mirror como Host (Servidor + Cliente)
        networkManager.StartHost();

        // Guardamos nuestro SteamID como metadata para que los invitados sepan a quién conectarse
        lobby.SetData(HostAddressKey, SteamClient.SteamId.ToString());
        lobby.SetJoinable(true);

        Debug.Log("Lobby creado exitosamente. Ya puedes invitar amigos por Steam.");
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendId)
    {
        // Este evento se dispara cuando estás en el menú de Steam, un amigo te invita y le das a "Jugar"
        Debug.Log("Aceptaste la invitación de: " + friendId);
        await lobby.Join();
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        // Si somos el Host, no hacemos nada porque ya estamos conectados
        if (networkManager.mode == NetworkManagerMode.Host) return;

        // Extraemos el SteamID del Host de la metadata del lobby
        string hostAddress = lobby.GetData(HostAddressKey);
        
        // Le decimos a Mirror que se conecte a ese usuario
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        Debug.Log("Uniéndose a la partida del Host...");
    }
}