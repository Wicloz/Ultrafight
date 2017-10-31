using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JoinGameMenu : MonoBehaviour
{
	public static JoinGameMenu acces;

	public bool isEnabled = false;

	private HostData[] hostList;
	private HostData[] filteredList;
	private HostData selectedGame;
	private int listLength = 0;

	CommentManager.CommentData commentData = new CommentManager.CommentData ();

	private string passWord_required = "";
	private string passWord_entered = "";

	private string filter_name = "";
	private bool filter_password = true;
	private bool filter_full = false;

	public float modifier1;
	public float modifier2;
	
	private float boxWidth;
	private float halfBox;
	private float width;
	private float start;
	
	private float boxSpacing;
	private float itemSpacing;
	
	private float boxStartY;
	private float boxEndY;
	private float startY;
	private float endY;
	
	private float boxStartX1;
	private float boxStartX2;
	private float boxStartX3;
	
	private float startX1;
	private float startX2;
	private float startX3;
	
	void Update ()
	{
		if (enabled)
		{
			boxSpacing = Screen.height / modifier1;
			itemSpacing = Screen.height / modifier2;
			
			boxWidth = (Screen.width - boxSpacing * 4) / 3;
			halfBox = boxWidth / 2;
			width = boxWidth - itemSpacing * 2;
			start = boxSpacing;
			
			boxStartY = start;
			boxEndY = Screen.height - start;
			
			boxStartX1 = boxSpacing;
			boxStartX2 = boxStartX1 + boxWidth + boxSpacing;
			boxStartX3 = boxStartX2 + boxWidth + boxSpacing;
			
			startX1 = boxStartX1 + itemSpacing;
			startX2 = boxStartX2 + itemSpacing;
			startX3 = boxStartX3 + itemSpacing;
			
			startY = boxStartY + itemSpacing + 20;
			endY = boxEndY - itemSpacing;
		}
	}

	void Awake ()
	{
		acces = this;
	}

	public void ButtonClicked ()
	{
		RefreshHostList ();
	}

	void OnEnable ()
	{
		RefreshHostList ();
	}

	private void RefreshHostList()
	{
		selectedGame = null;
		MasterServer.RequestHostList(HostGameManager.gameName);
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
		{
			hostList = MasterServer.PollHostList();
			FilterList ();
		}
	}

	private void FilterList ()
	{
		listLength = 0;

		for (int i = 0; i < hostList.Length; i++)
		{
			if (FilterCheck (hostList[i]))
			{
				listLength ++;
			}
		}

		filteredList = new HostData[listLength];
		int number = 0;

		for (int i = 0; i < hostList.Length; i++)
		{
			Debug.Log(hostList[i].gameName + " found");

			if (FilterCheck (hostList[i]))
			{
				Debug.Log(hostList[i].gameName + " is good");

				filteredList[number] = hostList[i];
				number ++;
			}
		}
	}

	private bool FilterCheck (HostData hostData)
	{
		bool filter = true;
		CommentManager.CommentData localCommentData = CommentManager.GetCommentData (hostData.comment);

		if (filter && !filter_password && localCommentData.passWord != "")
		{
			filter = false;
		}

		if (filter && !filter_full && hostData.connectedPlayers == hostData.playerLimit)
		{
			filter = false;
		}

		if (filter && filter_name != "")
		{
			filter = hostData.gameName.Contains (filter_name);
		}
		
		return filter;
	}

	private Vector2 scrollPosition = Vector2.zero;

	void OnGUI()
	{
		if (isEnabled)
		{
			// Boxes
			GUI.Box(new Rect(boxStartX1, boxStartY, boxWidth, boxEndY - boxStartY), "Filters");
			GUI.Box(new Rect(boxStartX2, boxStartY, boxWidth, boxEndY - boxStartY), "Room List");
			GUI.Box(new Rect(boxStartX3, boxStartY, boxWidth, boxEndY - boxStartY), "Room Details");

			// First Section
			if (GUI.Button(new Rect(boxStartX1 + halfBox - 100, endY - 40, 200, 40), "Refresh"))
			{
				RefreshHostList ();
			}

			filter_name = GUI.TextField(new Rect(startX1, startY, width, 22), filter_name);

			filter_password = GUI.Toggle(new Rect(startX1, startY + 100, 200, 22), filter_password, "Show Password Protected");
			filter_full = GUI.Toggle(new Rect(startX1, startY + 125, 200, 22), filter_full, "Show Full Games");
			
			// Second Section
			scrollPosition = GUI.BeginScrollView(new Rect(startX2, startY, width, boxEndY - startY - itemSpacing), scrollPosition, new Rect(0, 0, width - itemSpacing, listLength * 30));

			if (filteredList != null)
			{
				for (int i = 0; i < filteredList.Length; i++)
				{
					if (GUI.Button(new Rect(0, (30 * i), width - itemSpacing, 22), filteredList[i].gameName))
					{
						GetCommentData (filteredList[i]);
						selectedGame = filteredList[i];
					}
				}
			}

			GUI.EndScrollView();

			// Third Section
			if (selectedGame != null)
			{
				GUI.Label (new Rect(startX3, startY, 200, 22), "Host Name: " + commentData.hostName);

				GUI.Label (new Rect(startX3, startY + 44, 200, 22), "Players In Game:");
				for (int i = 0; i < commentData.users.Count; i++)
				{
					GUI.Label (new Rect(startX3, startY + 66 + i * 22, 200, 22), "- " + commentData.users[i]);
				}

				GUI.Label (new Rect(startX3, endY - 200, 200, 22), "Protocol Version: " + commentData.protocol);

				if (passWord_required != "")
				{
					GUI.Label (new Rect(startX3, endY - 102, 200, 22), "Password:");
					passWord_entered = GUI.TextField (new Rect(startX3, endY - 80, width, 22), passWord_entered);
				}
				
				if (GUI.Button(new Rect(boxStartX3 + halfBox - 100, endY - 40, 200, 40), "Join Game"))
				{
					if (passWord_entered == passWord_required)
					{
						JoinServer (selectedGame);
					}
					else
					{
						passWord_entered = "Invalid Password";
					}
				}
			}
		}
	}

	private void GetCommentData (HostData hostData)
	{
		commentData = CommentManager.GetCommentData(hostData.comment);
		passWord_required = commentData.passWord;
	}
	
	// Joining a server
	private void JoinServer (HostData hostData)
	{
		string error = "";

		if (commentData.users.Contains(ProfileManager.acces.profile.username))
		{
			error = "User already in game!";
		}
		if (commentData.protocol != HostGameManager.acces.protocolVersion)
		{
			error = "Mismatching connection protocol versions!";
		}

		if (error == "")
		{
			LoadingScreen.acces.StartLoading ("Joining Game");
			GameStates.acces.majorGameState = MajorGameState.Loading;
			Network.Connect (hostData);
		}
		else
		{
			Debug.LogError (error);
		}
	}

	// On Server Joined
	void OnConnectedToServer()
	{
		GameLobbyManager.acces.JoinServerClient ();
	}
}
