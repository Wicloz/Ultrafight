using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HostGameManager : MonoBehaviour
{
	public static HostGameManager acces;

	public string passWord = "";

	public int maxUsers = 9;
	public const string gameName = "UltraFight";
	private string roomName = "null";
	public string protocolVersion = "A-1.2";

	void Awake ()
	{
		acces = this;
	}

	public void ButtonClicked ()
	{
		StartServer ();
	}

	// Hosting a server
	private void StartServer()
	{
		UserManager.acces.Reset ();
		Network.InitializeServer(maxUsers, 25000, !Network.HavePublicAddress());
		RegisterHost();
	}
	
	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		// On Server Started !!
		if (msEvent == MasterServerEvent.RegistrationSucceeded)
		{
			if (GameStates.acces.majorGameState == MajorGameState.Stopped)
			{
				Debug.LogError  ("Server Started");
				ChatManager.acces.SendServerMessage ("Server Started!");

				GameLobbyManager.acces.JoinServerHost ();
			}
		}

		else if (msEvent == MasterServerEvent.RegistrationFailedNoServer)
		{
			MenuManager.acces.LeaveGame ("Failed to start server");
		}
	}

	public void RegisterHost()
	{
		roomName = ProfileManager.acces.profile.username + "'s Room";

		MasterServer.UnregisterHost ();
		MasterServer.RegisterHost (gameName, roomName, CommentManager.SerialiseComment(passWord, ProfileManager.acces.profile.username, UserManager.acces.GetSimpleUserList(), protocolVersion));
		Debug.LogError ("Server Registered");
	}
}
