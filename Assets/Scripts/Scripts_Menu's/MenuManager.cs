using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MenuState {MainMenu, ServerBrowser, HostScreen, OptionsScreen, GameLobby, GameScene, GameResults};

public class MenuManager : MonoBehaviour
{
	public static MenuManager acces;

	public MenuState previousState;
	public MenuState state;

	public List<GameObject> mainMenuObjects;

	public List<GameObject> serverBrowserObjects;
	public List<GameObject> hostScreenObjects;

	public List<GameObject> lobbyObjects;
	public List<GameObject> resultsObjects;
	public List<GameObject> gameSceneObjects;

	public List<GameObject> userListObjects;
	
	public List<GameObject> optionObjects;

	public List<GameObject> leftGameObjects;
	public List<GameObject> quitConfirmObjects;
	
	public bool exitConfirmation = false;
	public InstantGuiElement quitReason;

	void Awake ()
	{
		acces = this;
	}

	void OnGUI ()
	{
		if (exitConfirmation)
		{
			GUI.Box(new Rect((Screen.width / 2) - 150, (Screen.height / 2) - 50, 300, 80), "Are you sure you want to exit the game?");

			if (GUI.Button(new Rect((Screen.width / 2) - 110, (Screen.height / 2) - 20, 90, 40), "Exit Game"))
			{
				Application.Quit();
			}

			if (GUI.Button(new Rect((Screen.width / 2) + 20, (Screen.height / 2) - 20, 90, 40), "Nevermind"))
			{
				exitConfirmation = false;
				MainMenuFader.acces.lockFading = false;
				MainMenuFader.acces.FadeBackgroundSmart ();
			}
		}
	}
		
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape))
		{
			if (state == MenuState.ServerBrowser || state == MenuState.OptionsScreen)
			{
				GotoLastState ();
			}
			else if (state == MenuState.MainMenu)
			{
				if (!exitConfirmation)
				{
					MainMenuFader.acces.FadeBackgroundForced (MenuBackground.Exit);
					ExitGame();
				}
				else
				{
					exitConfirmation = false;
					MainMenuFader.acces.lockFading = false;
					MainMenuFader.acces.FadeBackgroundSmart ();
				}
			}
			else
			{
				EscapeMenu.acces.SwitchStatus();
			}
		}
	}

	// Functional
	public void ExitGame ()
	{
		exitConfirmation = true;
		MainMenuFader.acces.lockFading = true;
	}

	public void LeaveGame (string reason)
	{
		if (SwitchBox.isClient)
		{
			Network.Disconnect();
		}
		else if (SwitchBox.isServer)
		{
			GetComponent<NetworkView>().RPC("KickClient", RPCMode.Others, "", true, "Server Closed");

			MasterServer.UnregisterHost();
			Network.Disconnect();
		}

		UserManager.acces.Reset ();
		ServerGameController.acces.Reset ();
		ClientGameController.acces.Reset ();
		GameLobbyManager.acces.Reset ();
		GameStates.acces.Reset ();
		MusicManager.acces.ResetGameMusic ();
		ClientGraphicController.acces.Reset ();

		GotoMainMenu ();

		SetObjectsState (leftGameObjects, true);
		quitReason.text = reason;
	}

	[RPC] public void KickClient (string username, bool allUsers, string reason)
	{
		if (username == ProfileManager.acces.profile.username || allUsers)
		{
			LeaveGame (reason);
		}
	}

	public void GotoLastState ()
	{
		switch (previousState)
		{
		case MenuState.GameLobby:
			GameLobby ();
			break;
		case MenuState.GameScene:
			GameScene ();
			break;
		case MenuState.MainMenu:
			GotoMainMenu ();
			break;
		case MenuState.OptionsScreen:
			OptionsScreen ();
			break;
		case MenuState.ServerBrowser:
			ServerBrowser ();
			break;
		}
	}

	private void SetObjectsState (List<GameObject> objects, bool state)
	{
		foreach (GameObject go in objects)
		{
			go.SetActive (state);
		}
	}

	// Menu's
	public void GotoMainMenu ()
	{
		previousState = state;
		state = MenuState.MainMenu;
		MusicManager.acces.SetMusicState (Music.MainMenu);
		MainMenuFader.acces.FadeBackgroundSmart ();

		SetObjectsState (mainMenuObjects, true);
		SetObjectsState (serverBrowserObjects, false);
		SetObjectsState (hostScreenObjects, false);
		SetObjectsState (lobbyObjects, false);
		SetObjectsState (resultsObjects, false);
		SetObjectsState (gameSceneObjects, false);
		SetObjectsState (userListObjects, false);
		SetObjectsState (optionObjects, false);
		SetObjectsState (leftGameObjects, false);
		SetObjectsState (quitConfirmObjects, false);

		JoinGameMenu.acces.isEnabled = false;
		ChatManager.acces.Disable ();
	}

	public void ServerBrowser ()
	{
		previousState = state;
		state = MenuState.ServerBrowser;
		MusicManager.acces.SetMusicState (Music.MainMenu);
		MainMenuFader.acces.StopFading ();
		
		SetObjectsState (mainMenuObjects, false);
		SetObjectsState (serverBrowserObjects, true);
		SetObjectsState (hostScreenObjects, false);
		SetObjectsState (lobbyObjects, false);
		SetObjectsState (resultsObjects, false);
		SetObjectsState (gameSceneObjects, false);
		SetObjectsState (userListObjects, false);
		SetObjectsState (optionObjects, false);
		SetObjectsState (leftGameObjects, false);
		SetObjectsState (quitConfirmObjects, false);

		JoinGameMenu.acces.isEnabled = true;
		ChatManager.acces.Disable ();

		JoinGameMenu.acces.ButtonClicked ();
	}

	public void OptionsScreen ()
	{
		previousState = state;
		state = MenuState.OptionsScreen;
		MusicManager.acces.SetMusicState (Music.MainMenu);
		MainMenuFader.acces.StopFading ();

		SetObjectsState (mainMenuObjects, false);
		SetObjectsState (serverBrowserObjects, false);
		SetObjectsState (hostScreenObjects, false);
		SetObjectsState (resultsObjects, false);
		SetObjectsState (userListObjects, false);
		SetObjectsState (optionObjects, true);
		SetObjectsState (leftGameObjects, false);
		SetObjectsState (quitConfirmObjects, false);
		
		JoinGameMenu.acces.isEnabled = false;
		ChatManager.acces.Disable ();
	}

	// Game scenes
	public void GameLobby ()
	{
		previousState = state;
		state = MenuState.GameLobby;
		MusicManager.acces.SetMusicState (Music.Lobby);
		MainMenuFader.acces.StopFading ();
		
		SetObjectsState (mainMenuObjects, false);
		SetObjectsState (serverBrowserObjects, false);
		SetObjectsState (hostScreenObjects, false);
		SetObjectsState (lobbyObjects, true);
		SetObjectsState (resultsObjects, false);
		SetObjectsState (gameSceneObjects, false);
		SetObjectsState (userListObjects, true);
		SetObjectsState (optionObjects, false);
		SetObjectsState (leftGameObjects, false);
		SetObjectsState (quitConfirmObjects, false);

		JoinGameMenu.acces.isEnabled = false;
		ChatManager.acces.Enable ();
	}

	public void GameScene ()
	{
		previousState = state;
		state = MenuState.GameScene;
		MusicManager.acces.SetMusicState (Music.Game);
		MainMenuFader.acces.StopFading ();
		
		SetObjectsState (mainMenuObjects, false);
		SetObjectsState (serverBrowserObjects, false);
		SetObjectsState (hostScreenObjects, false);
		SetObjectsState (lobbyObjects, false);
		SetObjectsState (resultsObjects, false);
		SetObjectsState (gameSceneObjects, true);
		SetObjectsState (userListObjects, true);
		SetObjectsState (optionObjects, false);
		SetObjectsState (leftGameObjects, false);
		SetObjectsState (quitConfirmObjects, false);

		JoinGameMenu.acces.isEnabled = false;
		ChatManager.acces.Enable ();
	}

	public void GameResults ()
	{
		previousState = state;
		state = MenuState.GameResults;
		MusicManager.acces.SetMusicState (Music.Results);
		MainMenuFader.acces.StopFading ();
		
		SetObjectsState (mainMenuObjects, false);
		SetObjectsState (serverBrowserObjects, false);
		SetObjectsState (hostScreenObjects, false);
		SetObjectsState (lobbyObjects, false);
		SetObjectsState (resultsObjects, false);
		SetObjectsState (gameSceneObjects, false);
		SetObjectsState (userListObjects, true);
		SetObjectsState (optionObjects, false);
		SetObjectsState (leftGameObjects, false);
		SetObjectsState (quitConfirmObjects, false);
		
		JoinGameMenu.acces.isEnabled = false;
		ChatManager.acces.Enable ();
	}
}
