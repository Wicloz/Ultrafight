using UnityEngine;
using System.Collections;
using System.IO;

public class ProfileManager : MonoBehaviour
{
	public static ProfileManager acces;

	public ProfileInfo profile =  new ProfileInfo();
	public string fileLocation;

	public InstantGuiInputText nameField;

	void Awake ()
	{
		acces = this;
		fileLocation = Application.persistentDataPath + "\\profile.dat";
	}

	void Start ()
	{
		LoadProfile();
	}

	void Update ()
	{
		this.profile.username = nameField.text;
	}

	void OnDestroy ()
	{
		SaveProfile();
	}

	private void LoadProfile ()
	{
		if (File.Exists(fileLocation))
		{
			profile = SerializerHelper.LoadFileBf<ProfileInfo> (fileLocation);

			if (profile.username == "")
			{
				profile.username = "USER_" + (int) Random.Range (0, 1000);
				SaveProfile();
			}
			
			nameField.text = profile.username;
		}

		else
		{
			SaveProfile ();
		}
	}

	private void SaveProfile ()
	{
		SerializerHelper.SaveFileBf (profile, fileLocation);
	}
}

[System.Serializable]
public class ProfileInfo
{
	public string username = "";

	public ProfileInfo ()
	{}

	public ProfileInfo (string username)
	{
		this.username = username;
	}
}
