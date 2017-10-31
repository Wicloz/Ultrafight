using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerGameController : MonoBehaviour
{
	public static ServerGameController acces;
	public bool gameRunning = false;
	private int thisRound = 0;

	private Dictionary<MinorGameState, bool> handledStates = new Dictionary<MinorGameState, bool> ();
	public BattleManager battleManager;

	private bool serverLogic
	{
		get
		{
			if (SwitchBox.isServerOn && gameRunning && SwitchBox.isServer)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
	
	public bool resolvingDone = false;

	void Awake ()
	{
		acces = this;
	}
	
	public void Reset ()
	{
		gameRunning = false;
		battleManager = new BattleManager ();
		thisRound = 0;
		SetStates ();
	}

	public void StartGame ()
	{
		if (SwitchBox.isServerOn)
		{
			Reset ();

			// Handle card synchronisation
			if (GameLobbyManager.acces.gameSettings.cardmode == CardMode.Shared)
			{
				GetComponent<NetworkView>().RPC("SendCardsToHost", RPCMode.Others);
				UserManager.acces.GetUser (ProfileManager.acces.profile.username).states.gamePrepDone = true;
			}
			else
			{
				foreach (UserInfo user in UserManager.acces.userList)
				{
					user.states.gamePrepDone = true;
				}
			}

			GameStates.acces.minorGameState = MinorGameState.GamePrep;
			gameRunning = true;
		}
	}

	private void EndGame ()
	{
		gameRunning = false;
		Reset ();
	}

	private void SetStates ()
	{
		handledStates = new Dictionary<MinorGameState, bool> ();

		handledStates.Add (MinorGameState.CardSelection, false);
		handledStates.Add (MinorGameState.Battle, false);
		handledStates.Add (MinorGameState.Results, false);
	}

	void Update ()
	{
		if (serverLogic && GameStates.acces.majorGameState == MajorGameState.Running)
		{
			switch (GameStates.acces.minorGameState)
			{
			case MinorGameState.GamePrep:
				
				// Check for all active players GamePrepDone
				bool allPlayersSent = true;
				foreach (UserInfo user in UserManager.acces.userList)
				{
					if (user.isActive && !user.states.gamePrepDone)
					{
						allPlayersSent = false;
					}
				}
				
				// Check for start game
				if (allPlayersSent)
				{
					NextRound ();
				}
				
				break;
			case MinorGameState.CardSelection:

				// Check for all active (online) players CardSelectionDone
				bool allPlayersPicked = true;
				foreach (UserInfo user in UserManager.acces.userList)
				{
					if (user.isActive && !user.states.cardSelectionDone)
					{
						allPlayersPicked = false;
					}
				}

				// Check for BattlePrepStart
				if (allPlayersPicked)
				{
					HostBattlePrepStart ();
				}

				break;
			case MinorGameState.BattlePrep:

				// Check for ResolvingDone
				if (resolvingDone)
				{
					resolvingDone = false;
					HostGraphicController.acces.CreateGraphics (battleManager.roundFightData);
					HostPrepGraphicData ();
				}

				// Check for all (online) players ArenaGraphicsLoaded
				bool allPlayersLoaded = true;
				foreach (UserInfo user in UserManager.acces.userList)
				{
					if (!user.states.arenaGraphicsLoaded)
					{
						allPlayersLoaded = false;
					}
				}
				
				// Check for BattleStart
				if (allPlayersLoaded && !handledStates[MinorGameState.Battle])
				{
					HostBattleStart ();
				}
				
				break;
			case MinorGameState.Battle:

				HostGraphicController.acces.GameUpdate ();

				break;
			case MinorGameState.Results:

				// Check for all (online) players ResultsDone
				bool allResultsDone = true;
				foreach (UserInfo user in UserManager.acces.userList)
				{
					if (!user.states.resultsDone)
					{
						allResultsDone = false;
					}
				}

				// Check for next round
				if (allResultsDone)
				{
					if (thisRound == GameLobbyManager.acces.gameSettings.rounds)
					{
						// End game and show results
						EndGame ();
						GameStates.acces.majorGameState = MajorGameState.Results;
					}
					else
					{
						NextRound ();
					}
				}

				break;
			}
		}
	}

	public void AllocatePoints (string attacker, string defender)
	{
		foreach (FightData roundResolve in battleManager.roundFightData)
		{
			if (roundResolve.attackerName == attacker && roundResolve.defenderName == defender)
			{
				if (roundResolve.win)
				{
					UserManager.acces.GetUser (attacker).kills ++;
					UserManager.acces.GetUser (defender).deaths ++;
				}
			}
		}
	}

	// State changes
	private void NextRound ()
	{
		if (thisRound < GameLobbyManager.acces.gameSettings.rounds)
		{
			UserManager.acces.ResetPlayerRoundInfo ();
			SetStates ();
			HostGraphicController.acces.Reset ();
			battleManager = new BattleManager ();
			
			thisRound ++;
			HostCardSelectionStart ();
		}
		else
		{
			EndGame ();
			GameLobbyManager.acces.SendGameResults ();
		}
	}

	private void HostCardSelectionStart ()
	{
		// Set state
		handledStates [MinorGameState.CardSelection] = true;
		GameStates.acces.minorGameState = MinorGameState.CardSelection;

		// Notify clients
		foreach (UserInfo user in UserManager.acces.userList)
		{
			SendCardSelectionStart (user.username);
		}
	}

	private void HostBattlePrepStart ()
	{
		// Set state
		GameStates.acces.minorGameState = MinorGameState.BattlePrep;
		
		// Perform resolve calculations
		StartCoroutine ("ResolveBattles");

		// Notify clients
		foreach (UserInfo user in UserManager.acces.userList)
		{
			SendBattlePrepStart (user.username);
		}
	}

	public void HostPrepGraphicData ()
	{
		// Send Graphic Data
		foreach (UserInfo user in UserManager.acces.userList)
		{
			SendPrepGraphicData (user.username);
		}
	}
	
	private void HostBattleStart ()
	{
		// Set state
		handledStates [MinorGameState.Battle] = true;
		GameStates.acces.minorGameState = MinorGameState.Battle;

		// Notify clients
		foreach (UserInfo user in UserManager.acces.userList)
		{
			SendBattleStart (user.username);
		}
	}
	
	public void HostResultsStart ()
	{
		// Set state
		handledStates [MinorGameState.Results] = true;
		GameStates.acces.minorGameState = MinorGameState.Results;

		// Calculate results
		if (GameLobbyManager.acces.gameSettings.gamemode == GameMode.RoundWins)
		{
			int highScore = -9999;
			foreach (UserInfo user in UserManager.acces.userList)
			{
				int score = user.kills - user.deaths;
				
				if (score >= highScore)
				{
					highScore = score;
				}
			}
			
			foreach (UserInfo user in UserManager.acces.userList)
			{
				int score = user.kills - user.deaths;
				
				if (score == highScore)
				{
					user.roundsWon ++;
				}
			}
		}

		// Notify clients
		foreach (UserInfo user in UserManager.acces.userList)
		{
			SendResultsStart (user.username);
		}
	}

	// State change announcements
	public void SendCardSelectionStart (string user)
	{
		// Notify client
		GetComponent<NetworkView>().RPC("CardSelectionStart", RPCMode.All, user, GameLobbyManager.acces.gameSettings.picksAmount);

		// Add random cards to client carousel
		if (GameLobbyManager.acces.gameSettings.cardmode != CardMode.ClientOnly)
		{
			SendHostCards (GameLobbyManager.acces.gameSettings.picksAmount, user);
		}
	}

	// CardSelectionDone is handled in the function that recieves a client's selected cards.

	public void SendBattlePrepStart (string user)
	{
		// Notify client
		GetComponent<NetworkView>().RPC("BattlePrepStart", RPCMode.All, user);
	}

	public void SendPrepGraphicData (string user)
	{
		// Send Graphic Data
		HostGraphicController.acces.SendArenaGraphics (user);
	}

	[RPC] public void RecieveGraphicsLoaded (string username)
	{
		foreach (UserInfo user in UserManager.acces.userList)
		{
			if (user.username == username)
			{
				user.states.arenaGraphicsLoaded = true;
			}
		}
	}

	public void SendBattleStart (string user)
	{
		// Notify client
		GetComponent<NetworkView>().RPC("BattleStart", RPCMode.All, user);
	}

	public void SendResultsStart (string user)
	{
		// Notify client
		GetComponent<NetworkView>().RPC("ResultsStart", RPCMode.All, user);
	}

	[RPC] public void RecieveResultsDone (string username)
	{
		foreach (UserInfo user in UserManager.acces.userList)
		{
			if (user.username == username)
			{
				user.states.resultsDone = true;
			}
		}
	}

	// Card synchronising
	[RPC] public void RecieveCard (string cardString, int list)
	{
		CardContent card = SerializerHelper.DeserializeFromString<CardContent> (cardString);
		Debug.Log ("Recieved card: " + card.name);

		switch (list)
		{
		case 0:
			ConfigManager.acces.AddClientCharacterCard (card);
			break;
		case 1:
			ConfigManager.acces.AddClientEquipmentCard (card);
			break;
		}
	}

	[RPC] public void RecieveGamePrepDone (string username)
	{
		foreach (UserInfo user in UserManager.acces.userList)
		{
			if (user.username == username)
			{
				user.states.gamePrepDone = true;
			}
		}
	}

	// Card selection
	private void SendHostCards (int amount, string username)
	{
		foreach (UserInfo user in UserManager.acces.userList)
		{
			if (user.username == username && user.isActive)
			{
				foreach (CardContent card in ConfigManager.acces.GetCharacterCards (amount))
				{
					string cardString = SerializerHelper.SerializeToString (card);
					GetComponent<NetworkView>().RPC("AddCarouselCard", RPCMode.All, user.username, cardString, 1);
				}
				
				foreach (CardContent card in ConfigManager.acces.GetEquipmentCards (amount))
				{
					string cardString = SerializerHelper.SerializeToString (card);
					GetComponent<NetworkView>().RPC("AddCarouselCard", RPCMode.All, user.username, cardString, 2);
				}
			}
		}
	}
	
	[RPC] public void RecieveCardSelection (string username, string characterString_1, string characterString_2, string equipmentString_1, string equipmentString_2, string equipmentString_3)
	{
		string characterString = characterString_1 + characterString_2;
		string equipmentString = equipmentString_1 + equipmentString_2 + equipmentString_3;

		CardContent character = SerializerHelper.DeserializeFromString<CardContent> (characterString);
		List<CardContent> equipment = SerializerHelper.DeserializeFromString<List<CardContent>> (equipmentString);
		
		foreach (UserInfo user in UserManager.acces.userList)
		{
			if (user.username == username)
			{
				user.character = character;
				user.equipment = equipment;
				user.CalculateStats ();
				user.states.cardSelectionDone = true;
			}
		}
	}

	private void SendCard (string username, CardContent card, int carousel)
	{
		string cardString = SerializerHelper.SerializeToString (card);
		GetComponent<NetworkView>().RPC("AddCarouselCard", RPCMode.All, username, cardString, carousel);
	}

	// Battle resolver
	private IEnumerator ResolveBattles ()
	{
		foreach (UserInfo attacker in UserManager.acces.userList)
		{
			foreach (UserInfo defender in UserManager.acces.userList)
			{
				if (attacker.username != defender.username)
				{
					battleManager.SimulateAttack (attacker, defender);
				}
			}
			yield return null;
		}
		resolvingDone = true;
	}
}
