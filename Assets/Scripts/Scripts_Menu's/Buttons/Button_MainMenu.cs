using UnityEngine;
using System.Collections;

public class Button_MainMenu : MonoBehaviour
{
	void OnMouseOver ()
	{
		MainMenuFader.acces.mouseLocation = MenuBackground.MainMenu;
	}

	void OnMouseEnter ()
	{
		MainMenuFader.acces.FadeBackgroundSmart ();
	}
}
