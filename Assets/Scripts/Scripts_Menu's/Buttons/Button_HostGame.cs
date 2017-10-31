using UnityEngine;
using System.Collections;

public class Button_HostGame : MonoBehaviour
{
	void OnMouseUp ()
	{
		if (!MenuManager.acces.exitConfirmation && ConfigManager.acces.allConfigsLoaded)
		{
			HostGameManager.acces.ButtonClicked ();
			MenuManager.acces.GameLobby ();
		}
	}

	void OnMouseOver ()
	{
		MainMenuFader.acces.mouseLocation = MenuBackground.HostGame;
	}

	void OnMouseEnter ()
	{
		MainMenuFader.acces.FadeBackgroundSmart ();
	}
}
