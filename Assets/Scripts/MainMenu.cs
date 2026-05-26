using UnityEngine;
using UnityEngine.SceneManagement;
using Steamworks;
using Steamworks.Data;

public class MainMenu : MonoBehaviour
{
    public SteamLobby steamLobby;

    public async void HostLobby()
    {
        Debug.Log("Creando Lobby...");

        try
        {
            await SteamMatchmaking.CreateLobbyAsync(4);

            // IMPORTANTE: cambiar de escena después de crear el lobby
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creando lobby: {e.Message}");
        }
    }

    public void ReturnToMainMenu()
    {
        if (steamLobby != null && steamLobby.HasLobby())
        {
            steamLobby.LeaveLobby();
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void InviteFriends()
    {
        if (steamLobby == null)
        {
            Debug.LogWarning("SteamLobby no asignado.");
            return;
        }

        if (steamLobby.IsHost() && steamLobby.HasLobby())
        {
            SteamFriends.OpenGameInviteOverlay(steamLobby.GetLobbyId());
        }
        else
        {
            Debug.LogWarning("No eres host o no hay lobby activo.");
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
}