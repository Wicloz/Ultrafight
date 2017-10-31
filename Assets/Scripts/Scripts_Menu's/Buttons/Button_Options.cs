using UnityEngine;
using System.Collections;

public class Button_Options : MonoBehaviour
{
	void OnMouseUp ()
	{
		if (!MenuManager.acces.exitConfirmation)
		{
			MenuManager.acces.OptionsScreen ();
		}
	}

	void OnMouseOver ()
	{
		MainMenuFader.acces.mouseLocation = MenuBackground.Options;
	}

	void OnMouseEnter ()
	{
		MainMenuFader.acces.FadeBackgroundSmart ();
	}
}
