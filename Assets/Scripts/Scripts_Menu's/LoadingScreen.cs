using UnityEngine;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
	public static LoadingScreen acces;

	public GameObject loadingScreen;
	public GameObject guiRoot;

	private string loadingText = "";

	void Awake ()
	{
		acces = this;
	}

	public void StartLoading (string message)
	{
		MusicManager.acces.SetMusicState (Music.Loading);

		loadingText = message;
		loadingScreen.SetActive (true);
		guiRoot.SetActive (false);
	}

	public void StopLoading ()
	{
		loadingText = "";
		loadingScreen.SetActive (false);
		guiRoot.SetActive (true);
	}
}
