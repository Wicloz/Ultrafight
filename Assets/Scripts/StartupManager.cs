using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartupManager : MonoBehaviour
{
	public GameObject instantGuiMaster;
	public List<GameObject> guiPopups;
	public List<GameObject> guiTabs;

	void Start ()
	{
		StartCoroutine ("StartupSequence");
	}

	private IEnumerator StartupSequence ()
	{
		LoadingScreen.acces.StartLoading ("Starting Game ...");

		yield return null;
		instantGuiMaster.SetActive (true);
		ActivateChildren (instantGuiMaster.transform);
		yield return null;
		DisableSelectedGuis ();
		instantGuiMaster.SetActive (false);

		yield return null;
		MenuManager.acces.GotoMainMenu ();
		LoadingScreen.acces.StopLoading ();
	}

	private void ActivateChildren (Transform t)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);

			child.gameObject.SetActive (true);
			ActivateChildren (child);
		}
	}

	private void DisableSelectedGuis ()
	{
		foreach (GameObject go in guiPopups)
		{
			go.SetActive (false);
		}

		foreach (GameObject go in guiTabs)
		{
			go.SetActive (false);
		}
	}
}
