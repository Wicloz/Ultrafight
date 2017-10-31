using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientGameController : MonoBehaviour
{
	public static ClientGameController acces;
	public Transform carouselParent;
	public GameObject carouselPrefab;

	private GameObject carouselCharacter;
	private GameObject carouselEquipment;

	void Awake ()
	{
		acces = this;
	}

	public void Reset ()
	{
		DestroyCarousels ();
	}

	void OnGUI ()
	{
		if (carouselCharacter != null && carouselEquipment != null && carouselCharacter.GetComponent<CarouselBehaviour>().selectedCard != null && carouselEquipment.GetComponent<CarouselBehaviour>().selectedCard != null)
		{
			if (GUI.Button (new Rect(Screen.width - 210, 10, 200, Screen.height - 20 - ChatManager.acces.chatHeight), "Confirm\nSelection"))
			{
				SendCardSelection ();
				DestroyCarousels ();
			}
		}
	}

	// Card synchronising
	[RPC] public void SendCardsToHost ()
	{
		GameStates.acces.minorGameState = MinorGameState.GamePrep;

		if (!SwitchBox.isServer && UserManager.acces.thisUser.isActive)
		{
			foreach (CardContent card in ConfigManager.acces.GetCharacterList())
			{
				string cardString = SerializerHelper.SerializeToString (card);
				GetComponent<NetworkView>().RPC("RecieveCard", RPCMode.Server, cardString, 0);
			}
			
			foreach (CardContent card in ConfigManager.acces.GetEquipmentList())
			{
				string cardString = SerializerHelper.SerializeToString (card);
				GetComponent<NetworkView>().RPC("RecieveCard", RPCMode.Server, cardString, 1);
			}
		}

		Invoke ("SendGamePrepDone", 0.1f);
	}

	private void SendGamePrepDone ()
	{
		GetComponent<NetworkView>().RPC("RecieveGamePrepDone", RPCMode.Server, ProfileManager.acces.profile.username);
	}

	// State notifications
	[RPC] public void CardSelectionStart (string recipient, int amount)
	{
		if (recipient == ProfileManager.acces.profile.username)
		{
			// Set gamestate
			GameStates.acces.minorGameState = MinorGameState.CardSelection;
			Debug.LogWarning ("CardSelectionStart");

			// Create carousels
			carouselCharacter = CreateCarousel (new List<CardContent>(), 3.8f);
			carouselEquipment = CreateCarousel (new List<CardContent>(), 0);

			// Add random cards to own carousel
			if (GameLobbyManager.acces.gameSettings.cardmode == CardMode.ClientOnly && UserManager.acces.thisUser.isActive)
			{
				List<CardContent> cardsCharacter = ConfigManager.acces.GetCharacterCards (amount);
				List<CardContent> cardsEquipment = ConfigManager.acces.GetEquipmentCards (amount);
				carouselCharacter.GetComponent<CarouselBehaviour>().AddData (cardsCharacter);
				carouselEquipment.GetComponent<CarouselBehaviour>().AddData (cardsEquipment);
			}
		}
	}

	[RPC] public void BattlePrepStart (string recipient)
	{
		if (recipient == ProfileManager.acces.profile.username)
		{
			// Set gamestate
			GameStates.acces.minorGameState = MinorGameState.BattlePrep;
			Debug.LogWarning ("BattlePrepStart");

			// Clean up card selection
			DestroyCarousels ();
		}
	}

	[RPC] public void BattleStart (string recipient)
	{
		if (recipient == ProfileManager.acces.profile.username)
		{
			// Set gamestate
			GameStates.acces.minorGameState = MinorGameState.Battle;
			Debug.LogWarning ("BattleStart");
		}
	}

	[RPC] public void ResultsStart (string recipient)
	{
		if (recipient == ProfileManager.acces.profile.username)
		{
			// Set gamestate
			GameStates.acces.minorGameState = MinorGameState.Results;
			Debug.LogWarning ("ResultsStart");

			// Destroy battle graphics
			ClientGraphicController.acces.Reset ();

			// Send done message
			if (SwitchBox.isServer)
			{
				ServerGameController.acces.RecieveResultsDone (ProfileManager.acces.profile.username);
			}
			else
			{
				GetComponent<NetworkView>().RPC("RecieveResultsDone", RPCMode.Server, ProfileManager.acces.profile.username);
			}
		}
	}
	
	// Card selection
	private void SendCardSelection ()
	{
		List<CardContent> equipmentCards = new List<CardContent>();
		equipmentCards.Add (carouselEquipment.GetComponent<CarouselBehaviour>().GetSelectedCard());
		CardContent characterCard = carouselCharacter.GetComponent<CarouselBehaviour> ().GetSelectedCard ();

		List<string> characterStrings = SerializerHelper.SerializeToString (characterCard, 2);
		List<string> equipmentStrings = SerializerHelper.SerializeToString (equipmentCards, 3);

		if (SwitchBox.isServer)
		{
			ServerGameController.acces.RecieveCardSelection (ProfileManager.acces.profile.username, characterStrings[0], characterStrings[1], equipmentStrings[0], equipmentStrings[1], equipmentStrings[2]);
		}
		else
		{
			GetComponent<NetworkView>().RPC("RecieveCardSelection", RPCMode.Server, ProfileManager.acces.profile.username, characterStrings[0], characterStrings[1], equipmentStrings[0], equipmentStrings[1], equipmentStrings[2]);
		}
	}

	private void DestroyCarousels ()
	{
		if (carouselCharacter != null)
		{
			Object.Destroy (carouselCharacter);
		}

		if (carouselEquipment != null)
		{
			Object.Destroy (carouselEquipment);
		}
	}

	[RPC] public void AddCarouselCard (string username, string cardString, int carousel)
	{
		if (username == ProfileManager.acces.profile.username)
		{
			CardContent card = SerializerHelper.DeserializeFromString<CardContent> (cardString);

			switch (carousel)
			{
			case 1:
				carouselCharacter.GetComponent<CarouselBehaviour>().AddData (card);
				break;
			case 2:
				carouselEquipment.GetComponent<CarouselBehaviour>().AddData (card);
				break;
			}
		}
	}

	private GameObject CreateCarousel (List<CardContent> cards, float yPos)
	{
		Vector3 position = new Vector3(0, yPos, 10);

		GameObject carousel = (GameObject) Object.Instantiate (carouselPrefab, position, Quaternion.identity);
		carousel.transform.SetParent (carouselParent, false);
	
		carousel.GetComponent<CarouselBehaviour>().AddData (cards);
		return carousel;
	}
}
