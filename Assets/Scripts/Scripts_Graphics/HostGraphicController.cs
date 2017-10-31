using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HostGraphicController : MonoBehaviour
{
	public static HostGraphicController acces;

	public List<GraphicSet> graphicList = new List<GraphicSet>();
	private int currentGraphic = -1;
	private int maxGraphics = 0;

	public string winMessage = "Succes!";
	public string loseMessage = "Fail!";

	public string hitMessage = "Hit!";
	public string evadeMessage = "Evaded";
	public string immuneMessage = "Immune";
	public string shotMessage = "Shot Down";

	void Awake ()
	{
		acces = this;
	}

	public void Reset ()
	{
		graphicList = new List<GraphicSet>();
		currentGraphic = -1;
		maxGraphics = 0;
	}

	public void CreateGraphics (List<FightData> battle)
	{
		foreach (FightData fight in battle)
		{
			GraphicSet graphics = new GraphicSet();
			graphicList.Add (graphics);

			graphics.attacker = fight.attackerName;
			graphics.defender = fight.defenderName;
			graphics.weaponName = fight.weaponName;

			graphics.attackerChar = UserManager.acces.GetUser (fight.attackerName).character.name;
			graphics.defenderChar = UserManager.acces.GetUser (fight.defenderName).character.name;

			List<ResolveData> fightResolve = new List<ResolveData>();
			fightResolve.AddRange (fight.resolveDirect);
			fightResolve.AddRange (fight.resolveSmall);
			fightResolve.AddRange (fight.resolveLarge);

			foreach (ResolveData resolve in fightResolve)
			{
				ResolveData parsedResolve = new ResolveData();
				graphics.attacks.Add (parsedResolve);

				string firstLetter = resolve.damageType.Remove (1);
				parsedResolve.damageType = firstLetter.ToUpper() + resolve.damageType.Substring(1);

				switch (resolve.reaction)
				{
				case "hit":
					parsedResolve.reaction = hitMessage;
					break;
				case "immune":
					parsedResolve.reaction = immuneMessage;
					break;
				case "evaded":
					parsedResolve.reaction = evadeMessage;
					break;
				case "shot":
					parsedResolve.reaction = shotMessage;
					break;
				}
			}

			if (fight.win)
			{
				graphics.resolve = winMessage;
			}
			else
			{
				graphics.resolve = loseMessage;
			}
		}

		maxGraphics = graphicList.Count - 1;
	}

	public void SendArenaGraphics (string username)
	{
		foreach (UserInfo user in UserManager.acces.userList)
		{
			PlayerGraphics graphics = new PlayerGraphics (user.username);
			graphics.characterCard = SmallCard.CreateCard (user.character);
			foreach (CardContent card in user.equipment)
			{
				graphics.equipmentCards.Add (SmallCard.CreateCard (card));
			}
			
			string graphicString = SerializerHelper.SerializeToString (graphics);
			GetComponent<NetworkView>().RPC("RecievePlayerGraphics", RPCMode.All, username, graphicString);
		}
	}

	// Battle Animatian Stuff
	private void SendGraphics (int index)
	{
		ServerGameController.acces.AllocatePoints (graphicList [index].attacker, graphicList [index].defender);

		// Start client graphics
		string graphicString = SerializerHelper.SerializeToString (graphicList[index]);
		GetComponent<NetworkView>().RPC("RecieveResolveData", RPCMode.All, graphicString, index);
	}

	[RPC] public void RecieveGraphicAnimationDone (string username, int graphicDone)
	{
		UserManager.acces.GetUser (username).states.graphicsDone = graphicDone;
	}

	public void GameUpdate ()
	{
		bool allUsersDone = true;
		foreach (UserInfo user in UserManager.acces.userList)
		{
			if (user.states.graphicsDone < currentGraphic)
			{
				allUsersDone = false;
			}
		}

		if (allUsersDone)
		{
			if (currentGraphic < maxGraphics)
			{
				currentGraphic ++;
				SendGraphics (currentGraphic);
			}
			else
			{
				ServerGameController.acces.HostResultsStart ();
			}
		}
	}
}

[System.Serializable]
public class GraphicSet
{
	public string attacker = "ERROR";
	public string defender = "ERROR";

	public string attackerChar = "ERROR";
	public string defenderChar = "ERROR";

	public string weaponName = "ERROR";
	public List<ResolveData> attacks = new List<ResolveData>();

	public string resolve = "ERROR";

	public GraphicSet ()
	{}
}
