using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class States
{
	public bool gamePrepDone = false;
	public bool cardSelectionDone = false;
	public bool resultsDone = false;
	public bool arenaGraphicsLoaded = false;

	public int graphicsDone = -1;

	public void Reset ()
	{
		gamePrepDone = false;
		cardSelectionDone = false;
		resultsDone = false;
		arenaGraphicsLoaded = false;
		graphicsDone = -1;
	}

	public States ()
	{}
}

[System.Serializable]
public class UserInfo
{
	public string username;

	public bool isOnline = true;
	public bool isActive = true;
	public bool isWelcomed = false;

	public bool checkPing = false;
	public bool isHost = false;

	public States states = new States ();

	public int kills = 0;
	public int deaths = 0;

	public int roundsWon = 0;
	public int swagPoints = 0;

	public CardContent character;
	public List<CardContent> equipment = new List<CardContent>();
	
	public Deployment deployment = Deployment.Land;
	public List<string> immunities = new List<string>();

	public UserInfo ()
	{
		username = "ERROR";
	}
	
	public UserInfo (string Username)
	{
		username = Username;
	}

	public UserInfo Clone ()
	{
		return SerializerHelper.DeserializeFromString<UserInfo> (SerializerHelper.SerializeToString (this));
	}

	public void CleanClientData ()
	{
		states.Reset();
		character = null;
		equipment = new List<CardContent>();
		immunities = new List<string>();
	}

	public void ResetGameInfo ()
	{
		kills = 0;
		deaths = 0;
		roundsWon = 0;
		swagPoints = 0;

		ResetRoundInfo ();
	}

	public void ResetRoundInfo ()
	{
		if (GameLobbyManager.acces.gameSettings.gamemode != GameMode.TotalScore)
		{
			kills = 0;
			deaths = 0;
		}
		
		states.Reset();
		
		character = null;
		equipment = new List<CardContent>();
		immunities = new List<string>();
	}

	public void CalculateStats ()
	{
		foreach (string immunity in character.immunities)
		{
			if (!immunities.Contains (immunity))
			{
				immunities.Add (immunity);
			}
		}

		bool hasVehicle = false;

		foreach (CardContent item in equipment)
		{
			if (item.type == (int)CardType.Armour)
			{
				foreach (string immunity in item.immunities)
				{
					if (!immunities.Contains (immunity))
					{
						immunities.Add (immunity);
					}
				}
			}

			if (item.type == (int)CardType.Vehicle)
			{
				hasVehicle = true;
			}
		}

		if (hasVehicle)
		{
			immunities = new List<string>();

			foreach (CardContent item in equipment)
			{
				if (item.type == (int)CardType.Vehicle)
				{
					foreach (string immunity in item.immunities)
					{
						if (!immunities.Contains (immunity))
						{
							immunities.Add (immunity);
						}
					}
				}
			}
		}
	}
}

public class UserManager : MonoBehaviour
{
	public static UserManager acces;
	public List<UserInfo> userList = new List<UserInfo>();
	public List<bool> pingList = new List<bool>();

	public int activePlayers
	{
		get
		{
			int i = 0;
			foreach (UserInfo user in userList)
			{
				if (user.isOnline)
				{
					i++;
				}
			}
			return i;
		}
	}

	public UserInfo thisUser
	{
		get
		{
			foreach (UserInfo user in userList)
			{
				if (user.username == ProfileManager.acces.profile.username)
				{
					return user;
				}
			}
			return null;
		}
	}

	public List<InstantGuiPopup> userInfoFields = new List<InstantGuiPopup>();

	void Awake ()
	{
		acces = this;
	}

	void Start ()
	{
		StartCoroutine (CheckForTimeouts ());
	}

	void OnDestroy ()
	{
		if (SwitchBox.isServer && SwitchBox.isServerOn)
		{
			GetComponent<NetworkView>().RPC("KickClient", RPCMode.Others, "", true, "Server Closed");
		}
	}

	public void Reset ()
	{
		userList = new List<UserInfo>();
		pingList = new List<bool>();
	}

	void Update ()
	{
		Debug.Log (Time.deltaTime);
		
		for (int i = 0; i < userInfoFields.Count; i++)
		{
			if (userInfoFields[i].list.labels[userInfoFields[i].list.selected] == "Kick Player" && SwitchBox.isServer)
			{
				KickUser (userInfoFields[i].list.labels[0]);
			}

			userInfoFields[i].list.selected = 0;
			userInfoFields[i].gameObject.SetActive (false);
		}

		int index = 1;
		foreach (UserInfo user in userList)
		{
			if (user.isHost)
			{
				SetUserInfofield (userInfoFields[0], user);
			}

			else
			{
				SetUserInfofield (userInfoFields[index], user);
				index ++;
			}
		}
	}

	public UserInfo GetUser (string username)
	{
		foreach (UserInfo user in userList)
		{
			if (user.username == username)
			{
				return user;
			}
		}

		return null;
	}

	private void SetUserInfofield (InstantGuiPopup field, UserInfo user)
	{
		field.gameObject.SetActive (true);
		field.check = user.isOnline;

		int index = 0;
		field.list.labels[index] = user.username;
		index ++;

		if (user.isActive)
		{
			field.list.labels[index] = "Kills: " + user.kills;
			index ++;
			field.list.labels[index] = "Deaths: " + user.deaths;
			index ++;
			
			if (GameLobbyManager.acces.gameSettings.gamemode != GameMode.TotalScore)
			{
				field.list.labels[index] = "Wins: " + user.roundsWon;
				index ++;
			}
			
			field.list.labels[index] = "Swagpoints: " + user.swagPoints;
			index ++;
		}
		else
		{
			field.list.labels[index] = "Spectator";
			index ++;
		}
		
		if (SwitchBox.isServer && user.isOnline && user.username != ProfileManager.acces.profile.username)
		{
			field.list.labels[index] = "Kick Player";
		}
		else
		{
			field.list.labels[index] = "";
		}
	}

	// Manipulate user data
	public void ResetPlayerGameInfo ()
	{
		for (int i = 0; i < userList.Count; i++)
		{
			userList[i].ResetGameInfo ();
			SendUser (userList[i].username);
		}
	}
	
	public void ResetPlayerRoundInfo ()
	{
		for (int i = 0; i < userList.Count; i++)
		{
			userList[i].ResetRoundInfo ();
			SendUser (userList[i].username);
		}
	}

	// Client synchronisation
	private void SendUser (string username)
	{
		foreach (UserInfo user in userList)
		{
			if (user.username == username)
			{
				UserInfo parsedUser = user.Clone ();
				parsedUser.CleanClientData ();

				List<string> stringSections = SerializerHelper.SerializeToString (parsedUser, 4);
				GetComponent<NetworkView>().RPC("RecieveUser", RPCMode.Others, stringSections[0], stringSections[1], stringSections[2], stringSections[3]);
				break;
			}
		}
	}
	
	[RPC] public void RecieveUser (string string_1, string string_2, string string_3, string string_4)
	{
		if (!SwitchBox.isServer)
		{
			string userString = string_1 + string_2 + string_3 + string_4;

			UserInfo user = SerializerHelper.DeserializeFromString<UserInfo> (userString);
			bool userExists = false;

			for (int i = 0; i < userList.Count; i++)
			{
				if (userList[i].username == user.username)
				{
					userList[i] = user;
					userExists = true;
					break;
				}
			}
			
			if (!userExists)
			{
				userList.Add (user);
			}
		}
	}

	// Manage users
	public void KickUser (string username)
	{
		GetComponent<NetworkView>().RPC("KickClient", RPCMode.Others, username, false, "Kicked by Host");
		DisconnectUser (username, " was kicked.");
	}

	public void RemoveUser (string username)
	{
		for (int i = 0; i < userList.Count; i++)
		{
			if (userList[i].username == username)
			{
				userList.RemoveAt(i);
				break;
			}
		}
	}

	public void RemoveOfflineUsers ()
	{
		for (int i = 0; i < userList.Count; i++)
		{
			if (!userList[i].isOnline)
			{
				userList.RemoveAt(i);
				i--;
			}
		}
	}

	public void SetUsersActive ()
	{
		foreach (UserInfo user in userList)
		{
			user.isActive = true;
		}
	}

	// Retrieve information
	public List<string> GetSimpleUserList ()
	{
		List<string> returnList = new List<string> ();

		for (int i = 0; i < userList.Count; i++)
		{
			if (userList[i].isOnline)
			{
				returnList.Add(userList[i].username);
			}
		}

		return returnList;
	}

	// Managing player connections
	[RPC] public void AddUser (string username)
	{
		bool userExists = false;
		UserInfo thisUser = new UserInfo (username);
		
		for (int i = 0; i < userList.Count; i++)
		{
			if (userList[i].username == username)
			{
				userExists = true;
				userList[i].isOnline = true;
				thisUser = userList[i];
				break;
			}
		}
		
		if (!userExists)
		{
			if (username == ProfileManager.acces.profile.username)
			{
				thisUser.isHost = true;
			}
			
			userList.Add (thisUser);
		}
		
		if (!thisUser.isWelcomed)
		{
			// Code when a player joins the game

			if (GameStates.acces.majorGameState == MajorGameState.Running || GameStates.acces.majorGameState == MajorGameState.Results)
			{
				if (!userExists)
				{
					thisUser.isActive = false;
				}
			}
			
			switch (GameStates.acces.majorGameState)
			{
			case MajorGameState.Lobby:
				GameLobbyManager.acces.SendGameLobby ();
				break;

			case MajorGameState.Running:
				GameLobbyManager.acces.SendGameRunning ();

				switch (GameStates.acces.minorGameState)
				{
				case MinorGameState.CardSelection:
					ServerGameController.acces.SendCardSelectionStart (username);
					break;
				case MinorGameState.BattlePrep:
					ServerGameController.acces.SendBattlePrepStart (username);

					if (ServerGameController.acces.resolvingDone)
					{
						ServerGameController.acces.SendPrepGraphicData (username);
					}

					break;
				case MinorGameState.Battle:
					ServerGameController.acces.SendBattleStart (username);
					break;
				case MinorGameState.Results:
					ServerGameController.acces.SendResultsStart (username);
					break;
				}

				break;

			case MajorGameState.Results:
				GameLobbyManager.acces.SendGameResults ();
				break;
			}
			
			// End join code
			thisUser.isWelcomed = true;
		}

		foreach (UserInfo user in userList)
		{
			SendUser (user.username);
		}
		
		HostGameManager.acces.RegisterHost ();
	}

	[RPC] public void DisconnectUser (string username, string reason)
	{
		foreach (UserInfo user in userList)
		{
			if (user.username == username)
			{
				user.isOnline = false;
				user.isWelcomed = false;
				ChatManager.acces.SendServerMessage(user.username + reason);
				break;
			}
		}
	}
	
	void OnPlayerConnected ()
	{}

	void OnPlayerDisconnected ()
	{
		CheckConnection (" left the game.");
	}

	// Ping system
	private IEnumerator CheckForTimeouts ()
	{
		while (true)
		{
			yield return new WaitForSeconds (10);

			if (SwitchBox.isServerOn && SwitchBox.isServer)
			{
				CheckConnection (" timed out.");
			}

			yield return new WaitForSeconds (10);
		}
	}

	public void CheckConnection (string message)
	{
		pingList = new List<bool>();
		
		foreach (UserInfo user in userList)
		{
			pingList.Add(false);
			
			if (user.username != ProfileManager.acces.profile.username && user.isOnline)
			{
				user.checkPing = true;
				GetComponent<NetworkView>().RPC("Marco", RPCMode.All, user.username);
			}
			else
			{
				user.checkPing = false;
			}
		}

		StopCoroutine ("DelayedConnectionCheck");
		StartCoroutine ("DelayedConnectionCheck", message);
	}

	[RPC] public void Polo (string username)
	{
		for (int i = 0; i < userList.Count; i++)
		{
			if (userList[i].username == username)
			{
				pingList[i] = true;
				break;
			}
		}
	}

	private IEnumerator DelayedConnectionCheck (string message)
	{
		bool done = false;
		float starttime = Time.time;

		while (!done)
		{
			if (Time.time - starttime > 1)
			{
				for (int i = 0; i < userList.Count; i++)
				{
					if (userList[i].checkPing && !pingList[i])
					{
						DisconnectUser (userList[i].username, message);
					}

					SendUser (userList[i].username);
				}

				done = true;
			}

			yield return null;
		}

		HostGameManager.acces.RegisterHost ();
	}
}
