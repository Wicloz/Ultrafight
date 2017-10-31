using UnityEngine;
using System.Collections;

public class Button_JoinGame : MonoBehaviour
{
	void OnMouseUp ()
	{
		if (!MenuManager.acces.exitConfirmation)
		{
			MenuManager.acces.ServerBrowser ();
		}
	}

	void OnMouseOver ()
	{
		MainMenuFader.acces.mouseLocation = MenuBackground.JoinGame;
	}

	void OnMouseEnter ()
	{
		MainMenuFader.acces.FadeBackgroundSmart ();
	}
}
