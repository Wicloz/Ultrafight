using UnityEngine;
using System.Collections;

public enum GameMode {RoundWins, TotalScore, Swagpoints};
public enum CardMode {HostOnly, ClientOnly, Shared};

public class GameLobbyManager : MonoBehaviour
{
	public static GameLobbyManager acces;

	public GameSettings gameSettings = new GameSettings();

	// Lobby menu references
	public InstantGuiSlider guiRounds;
	public InstantGuiPopup guiGamemode;
	public InstantGuiPopup guiCardmode;
	public InstantGuiElement guiRoundLabel;
	public InstantGuiButton guiStartGame;

	void Awake ()
	{
		acces = this;
	}

	void Start ()
	{
		StartCoroutine ("SynchroniseSettings");
	}

	public void Reset ()
	{
		gameSettings = new GameSettings ();
	}

	public void Update ()
	{
		if (SwitchBox.isServer)
		{
			guiRounds.disabled = false;
			guiGamemode.disabled = false;
			guiCardmode.disabled = false;
			guiStartGame.disabled = false;

			gameSettings.rounds = (int) guiRounds.value + 1;
			gameSettings.gamemode = (GameMode) guiGamemode.list.selected;
			gameSettings.cardmode = (CardMode) guiCardmode.list.selected;

			if (guiStartGame.pressed)
			{
				SendGameRunning ();
				ServerGameController.acces.StartGame ();
				guiStartGame.pressed = false;
			}

			if (GameStates.acces.majorGameState == MajorGameState.Lobby)
			{
				UserManager.acces.RemoveOfflineUsers ();
				UserManager.acces.SetUsersActive ();
			}
		}

		else
		{
			guiRounds.disabled = true;
			guiGamemode.disabled = true;
			guiCardmode.disabled = true;
			guiStartGame.disabled = true;

			guiRounds.value = gameSettings.rounds - 1;
			guiGamemode.list.selected = (int) gameSettings.gamemode;
			guiCardmode.list.selected = (int) gameSettings.cardmode;
		}

		guiRoundLabel.text = gameSettings.rounds.ToString();
	}

	private IEnumerator SynchroniseSettings ()
	{
		while (true)
		{
			if (!ServerGameController.acces.gameRunning && SwitchBox.isServerOn && SwitchBox.isServer)
			{
				SendSettings ();
			}
			yield return new WaitForSeconds (0.1f);
		}
	}

	private int SafetyInt (int value, int fallback)
	{
		if (SwitchBox.isServer)
		{
			return value;
		}
		else
		{
			return fallback;
		}
	}

	// Joining Game
	public void JoinServerHost ()
	{
		GameStates.acces.majorGameState = MajorGameState.Lobby;
		UserManager.acces.AddUser(ProfileManager.acces.profile.username);

		Debug.LogError ("Server Joined");
		ChatManager.acces.SendServerMessage (ProfileManager.acces.profile.username + " joined the server.");
	}

	public void JoinServerClient ()
	{
		GetComponent<NetworkView>().RPC ("AddUser", RPCMode.Server, ProfileManager.acces.profile.username);
		
		Debug.LogError ("Server Joined");
		ChatManager.acces.SendServerMessage (ProfileManager.acces.profile.username + " joined the server.");
	}

	// Sending GameStates
	public void SendGameLobby ()
	{
		SendSettings ();
		GetComponent<NetworkView>().RPC ("GameLobbyStart", RPCMode.All);
	}
	
	[RPC] public void GameLobbyStart ()
	{
		MenuManager.acces.GameLobby ();
		Debug.LogWarning ("GameLobby");
		GameStates.acces.majorGameState = MajorGameState.Lobby;
		LoadingScreen.acces.StopLoading ();
	}

	public void SendGameRunning ()
	{
		SendSettings ();
		GetComponent<NetworkView>().RPC ("GameRunningStart", RPCMode.All);
	}

	[RPC] public void GameRunningStart ()
	{
		MenuManager.acces.GameScene ();
		Debug.LogWarning ("GameScene");
		GameStates.acces.majorGameState = MajorGameState.Running;
		LoadingScreen.acces.StopLoading ();
	}
	
	public void SendGameResults ()
	{
		GetComponent<NetworkView>().RPC ("GameResultsStart", RPCMode.All);
	}
	
	[RPC] public void GameResultsStart ()
	{
		MenuManager.acces.GameResults ();
		Debug.LogWarning ("GameResults");
		GameStates.acces.majorGameState = MajorGameState.Results;
		LoadingScreen.acces.StopLoading ();
	}

	// Streaming settings
	private void SendSettings ()
	{
		Debug.Log ("Sending Settings ...");
		string settings = SerializerHelper.SerializeToString (gameSettings);
		GetComponent<NetworkView>().RPC ("RecieveSettings", RPCMode.Others, settings);
	}

	[RPC] public void RecieveSettings (string settings)
	{
		gameSettings = SerializerHelper.DeserializeFromString<GameSettings> (settings);
	}
}

[System.Serializable]
public class GameSettings
{
	public int rounds = 3;
	public GameMode gamemode = GameMode.RoundWins;
	public CardMode cardmode = CardMode.HostOnly;

	public int picksAmount = 3;
	
	public GameSettings ()
	{}
}
