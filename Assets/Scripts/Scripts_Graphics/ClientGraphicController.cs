using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientGraphicController : MonoBehaviour
{
	public static ClientGraphicController acces;
	public List<PlayerGraphics> playerGraphics = new List<PlayerGraphics>();
	public List<GameObject> graphicObjects = new List<GameObject>();

	public GameObject cardPrefab;
	public Transform cardParent;
	public GameObject resolvePrefab;

	private int currentPlayingGraphics = -1;
	private GraphicSet currentGraphics;
	private GameObject resolveObject;

	// Screen border values
	float topBorder = 5;
	float bottomBorder
	{
		get
		{
			return (202f / (float)Screen.height) * (topBorder * 2) - topBorder;
		}
	}
	float rightBorder
	{
		get
		{
			return topBorder * ((float)Screen.currentResolution.width / (float)Screen.currentResolution.height);
		}
	}
	float leftBorder
	{
		get
		{
			return (200f / (float)Screen.width) * (rightBorder * 2) - rightBorder;
		}
	}

	void Awake ()
	{
		acces = this;
	}

	public void Reset ()
	{
		foreach (GameObject card in graphicObjects)
		{
			Object.Destroy (card);
		}
		Object.Destroy (resolveObject);

		playerGraphics = new List<PlayerGraphics>();
		graphicObjects = new List<GameObject>();
		
		currentPlayingGraphics = -1;
		currentGraphics = null;
	}

	// Resolve graphics
	public Vector3 GetVectorCentre (Vector3 a, Vector3 b, float z)
	{
		Vector3 c = new Vector3 ((a.x+b.x) / 2, (a.y+b.y) / 2, z);
		return c;
	}

	[RPC] public void RecieveResolveData (string graphicString, int currentGraphic)
	{
		Debug.LogWarning ("PlayingGraphic " + currentGraphic);
		currentGraphics = SerializerHelper.DeserializeFromString<GraphicSet> (graphicString);

		Vector3 attackCard = new Vector3(0,0,0);
		Vector3 defendCard = new Vector3(0,0,0);

		foreach (GameObject card in graphicObjects)
		{
			if (card.GetComponent<CardBehaviour>().owner == currentGraphics.attacker)
			{
				attackCard = new Vector3(card.transform.position.x * -1, card.transform.position.y, 0);
			}
			if (card.GetComponent<CardBehaviour>().owner == currentGraphics.defender)
			{
				defendCard = new Vector3(card.transform.position.x * -1, card.transform.position.y, 0);
			}
		}

		resolveObject = (GameObject)Object.Instantiate (resolvePrefab, GetVectorCentre (attackCard, defendCard, 22), Quaternion.identity);
		resolveObject.GetComponent<ResolveTableHandler>().Initialise (currentGraphics);

		currentPlayingGraphics = currentGraphic;
		StartCoroutine ("ResolveAnimationSequence");
	}

	private IEnumerator ResolveAnimationSequence ()
	{
		float startTime = Time.time;
		float maxTime = 10;
		float passedTime = 0;

		while (passedTime < maxTime)
		{
			yield return null;
			passedTime = Time.time - startTime;
			resolveObject.GetComponent<ResolveTableHandler>().SetTime (maxTime - passedTime);
		}

		Object.Destroy (resolveObject);

		if (SwitchBox.isServer)
		{
			HostGraphicController.acces.RecieveGraphicAnimationDone (ProfileManager.acces.profile.username, currentPlayingGraphics);
		}
		else
		{
			GetComponent<NetworkView>().RPC("RecieveGraphicAnimationDone", RPCMode.Server, ProfileManager.acces.profile.username, currentPlayingGraphics);
		}
	}

	// Game arena graphics
	public Vector3 GetGraphicPos (float angle, float z)
	{
		float newAngle = (angle / 180) * Mathf.PI;
		float xCentre = ((leftBorder + rightBorder) / 2);
		float yCentre = ((topBorder + bottomBorder) / 2);

		float xPos = xCentre + (Mathf.Sin (newAngle) * (rightBorder - xCentre));
		float yPos = yCentre + (Mathf.Cos (newAngle) * (topBorder - yCentre));

		xPos = xPos - (1 * Mathf.Sin (newAngle));
		yPos = yPos - (1.1f * Mathf.Cos (newAngle));

		return new Vector3 (xPos, yPos, z);
	}

	[RPC] public void RecievePlayerGraphics (string username, string graphics)
	{
		if (username == ProfileManager.acces.profile.username)
		{
			PlayerGraphics graphicsObject = SerializerHelper.DeserializeFromString<PlayerGraphics> (graphics);
			playerGraphics.Add (graphicsObject);
			
			if (playerGraphics.Count == UserManager.acces.activePlayers)
			{
				PrepareArena ();
			}
		}
	}

	private void PrepareArena ()
	{
		float angleDifference = 360 / playerGraphics.Count;
		float currentAngle = 0;

		foreach (PlayerGraphics player in playerGraphics)
		{
			GameObject card = (GameObject) Object.Instantiate (cardPrefab, GetGraphicPos (currentAngle, 10), Quaternion.identity);
			card.transform.SetParent (cardParent, false);
			card.GetComponent<CardBehaviour>().InitialiseCard (new CardContent (player.characterCard.name, player.characterCard.description), player.username);

			graphicObjects.Add (card);
			currentAngle += angleDifference;
		}

		if (SwitchBox.isServer)
		{
			ServerGameController.acces.RecieveGraphicsLoaded (ProfileManager.acces.profile.username);
		}
		else
		{
			GetComponent<NetworkView>().RPC("RecieveGraphicsLoaded", RPCMode.Server, ProfileManager.acces.profile.username);
		}
	}

	public void ReloadGraphicPositions ()
	{
		if (playerGraphics.Count != 0)
		{
			float angleDifference = 360 / playerGraphics.Count;
			float currentAngle = 0;
			
			foreach (GameObject graphic in graphicObjects)
			{
				graphic.transform.position = new Vector3 (GetGraphicPos (currentAngle, graphic.transform.position.z).x * -1, GetGraphicPos (currentAngle, graphic.transform.position.z).y, graphic.transform.position.z);
				currentAngle += angleDifference;
			}
		}
	}
}

[System.Serializable]
public class PlayerGraphics
{
	public string username = "ERROR";
	public SmallCard characterCard = new SmallCard ();
	public List<SmallCard> equipmentCards = new List<SmallCard>();

	public PlayerGraphics ()
	{}

	public PlayerGraphics (string username)
	{
		this.username = username;
	}
}

[System.Serializable]
public class SmallCard
{
	public string name = "ERROR";
	public string description = "ERROR";

	public SmallCard ()
	{}

	public static SmallCard CreateCard (CardContent card)
	{
		SmallCard smallCard = new SmallCard ();
		smallCard.name = card.name;
		smallCard.description = card.description;
		return smallCard;
	}
}
