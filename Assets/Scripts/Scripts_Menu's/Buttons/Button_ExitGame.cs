using UnityEngine;
using System.Collections;

public class Button_ExitGame : MonoBehaviour
{
	void OnMouseUp ()
	{
		if (!MenuManager.acces.exitConfirmation)
		{
			MenuManager.acces.ExitGame ();
		}
	}

	void OnMouseOver ()
	{
		MainMenuFader.acces.mouseLocation = MenuBackground.Exit;
	}

	void OnMouseEnter ()
	{
		MainMenuFader.acces.FadeBackgroundSmart ();
	}
}
