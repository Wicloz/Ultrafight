using UnityEngine;
using System.Collections;

public class EscapeMenu : MonoBehaviour
{
	public static EscapeMenu acces;

	private bool isEnabled = false;

	void Awake ()
	{
		acces = this;
	}

	void OnGUI ()
	{
		if (isEnabled)
		{
			if (GUI.Button(new Rect((Screen.width / 2) - 50, (Screen.height / 2) - 10, 100, 20), "Leave Game"))
			{
				if (SwitchBox.isServer)
				{
					MenuManager.acces.LeaveGame("Server Closed");
				}
				else if (SwitchBox.isClient)
				{
					MenuManager.acces.LeaveGame("Disconnected");
				}

				isEnabled = false;
			}

			if (GUI.Button(new Rect((Screen.width / 2) - 50, (Screen.height / 2) + 20, 100, 20), "Options"))
			{
				isEnabled = false;
				MenuManager.acces.OptionsScreen();
			}
		}
	}

	public void SwitchStatus ()
	{
		isEnabled = !isEnabled;
	}
}
