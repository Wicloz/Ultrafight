using UnityEngine;
using System.Collections;

public enum MenuBackground {MainMenu, JoinGame, HostGame, Options, Exit};

public class MainMenuFader : MonoBehaviour
{
	public static MainMenuFader acces;

	public GameObject mainmenu;
	public GameObject joinGame;
	public GameObject hostGame;
	public GameObject options;
	public GameObject exit;

	public MenuBackground mouseLocation = MenuBackground.MainMenu; 

	private GameObject fadeFromBackground;
	private GameObject fadeToBackground;
	public bool fading = false;
	public bool lockFading = false;

	void Awake ()
	{
		acces = this;
		fadeFromBackground = mainmenu;
		fadeToBackground = mainmenu;
	}

	public void StopFading ()
	{
		StopCoroutine ("FadeCurrentBackground");

		fadeFromBackground.transform.localPosition = new Vector3 (fadeFromBackground.transform.localPosition.x, fadeFromBackground.transform.localPosition.y, 1);
		fadeToBackground.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1, 1, 1, 1);

		fading = false;
	}

	public void FadeBackgroundSmart ()
	{
		StartCoroutine ("FadeBuffer");
	}

	private IEnumerator FadeBuffer ()
	{
		yield return null;
		FadeBackgroundForced (mouseLocation);
	}

	public void FadeBackgroundForced (MenuBackground background)
	{
		if (!lockFading)
		{
			GameObject backgroundObject = null;

			switch (background)
			{
			case MenuBackground.MainMenu:
				backgroundObject = mainmenu;
				break;
			case MenuBackground.JoinGame:
				backgroundObject = joinGame;
				break;
			case MenuBackground.HostGame:
				backgroundObject = hostGame;
				break;
			case MenuBackground.Options:
				backgroundObject = options;
				break;
			case MenuBackground.Exit:
				backgroundObject = exit;
				break;
			}

			if (fadeToBackground != backgroundObject)
			{
				StopFading ();
				StartCoroutine ("FadeCurrentBackground", backgroundObject);
			}
		}
	}

	private IEnumerator FadeCurrentBackground (GameObject fadeTo)
	{
		if (!fading)
		{
			fading = true;

			fadeFromBackground = fadeToBackground;
			fadeToBackground = fadeTo;

			fadeFromBackground.transform.localPosition = new Vector3 (fadeFromBackground.transform.localPosition.x, fadeFromBackground.transform.localPosition.y, 0.5f);
			fadeToBackground.transform.localPosition = new Vector3 (fadeToBackground.transform.localPosition.x, fadeToBackground.transform.localPosition.y, 0);
			
			Material mat = fadeToBackground.GetComponent<MeshRenderer>().sharedMaterial;
			
			float alpha = 0;
			mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha);
			while (alpha < 0.95f)
			{
				yield return null;
				alpha = Mathf.Lerp(alpha, 1, Time.deltaTime * 4f);
				
				mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha);
			}
			
			StopFading ();
		}
	}
}
