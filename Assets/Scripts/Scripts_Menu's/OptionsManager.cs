using UnityEngine;
using System.Collections;
using System.IO;

public enum OptionsTab {None, Video, Audio, Cards, Profile};

public class OptionsManager : MonoBehaviour
{
	public static OptionsManager acces;

	private string settingsFile;

	public InstantGuiPopup resolutionsBox;
	public InstantGuiPopup screenmodeBox;
	public InstantGuiToggle borderlessToggle;
	public InstantGuiPopup qualityBox;

	public InstantGuiSlider musicSlider;
	public InstantGuiSlider effectSlider;

	public InstantGuiElement profileTab;

	public string[] lastVideoSettings = new string[3];

	public string resolution
	{
		get
		{
			return resolutionsBox.list.labels[resolutionsBox.list.selected];
		}
		set
		{
			for (int i = 0; i < resolutionsBox.list.labels.Length; i++)
			{
				string label = resolutionsBox.list.labels[i];

				if (label == value)
				{
					resolutionsBox.list.selected = i;
					break;
				}
			}
		}
	}
	public string screenMode
	{
		get
		{
			return screenmodeBox.list.labels[screenmodeBox.list.selected];
		}
		set
		{
			for (int i = 0; i < screenmodeBox.list.labels.Length; i++)
			{
				string label = screenmodeBox.list.labels[i];
				
				if (label == value)
				{
					screenmodeBox.list.selected = i;
					break;
				}
			}
		}
	}
	public bool borderless
	{
		get
		{
			return borderlessToggle.check;
		}
		set
		{
			borderlessToggle.check = value;
		}
	}
	public string quality
	{
		get
		{
			return qualityBox.list.labels[qualityBox.list.selected];
		}
		set
		{
			for (int i = 0; i < qualityBox.list.labels.Length; i++)
			{
				string label = qualityBox.list.labels[i];
				
				if (label == value)
				{
					qualityBox.list.selected = i;
					break;
				}
			}
		}
	}

	public float musicVolume
	{
		get
		{
			return musicSlider.value;
		}
		set
		{
			musicSlider.value = value;
		}
	}
	public float effectsVolume
	{
		get
		{
			return effectSlider.value;
		}
		set
		{
			effectSlider.value = value;
		}
	}

	void Awake ()
	{
		acces = this;
		settingsFile = Application.dataPath + "\\..\\options.ini";
	}

	void Start ()
	{
		LoadOptions ();
		SetVideoSettings ();
		WriteLastSettings ();
	}

	void OnDestroy ()
	{
		SaveOptions ();
	}

	void Update ()
	{
		if (GameStates.acces.majorGameState == MajorGameState.Stopped)
		{
			profileTab.disabled = false;
		}
		else
		{
			profileTab.disabled = true;
		}

		if (lastVideoSettings [0] != this.resolution || lastVideoSettings [1] != this.screenMode || lastVideoSettings [2] != this.quality)
		{
			Debug.Log ("Video Settings Changed, setting new data ...");
			SetVideoSettings ();
			ClientGraphicController.acces.ReloadGraphicPositions ();
		}

		WriteLastSettings ();
	}

	private void WriteLastSettings ()
	{
		lastVideoSettings [0] = this.resolution;
		lastVideoSettings [1] = this.screenMode;
		lastVideoSettings [2] = this.quality;
	}

	public void SetVideoSettings ()
	{
		char[] delim = new char[] {'x'};

		int resWidth = System.Convert.ToInt32 (this.resolution.Split(delim)[0].Trim());
		int resHeight = System.Convert.ToInt32 (this.resolution.Split(delim)[1].Trim());
		bool fullscreen = false;
		
		switch (this.quality)
		{
		case "Simple":
			QualitySettings.SetQualityLevel (3);
			break;
		case "Good":
			QualitySettings.SetQualityLevel (4);
			break;
		case "Fantastic":
			QualitySettings.SetQualityLevel (5);
			break;
		}

		if (this.screenMode == "Fullscreen")
		{
			fullscreen = true;
		}
		//if (borderless)
		//{
		//	fullscreen = false;
		//}

		Screen.SetResolution (resWidth, resHeight, fullscreen);
	}

	public void SaveOptions ()
	{
		Settings set = new Settings ();

		set.resolution = this.resolution;
		set.screenMode = this.screenMode;
		set.borderless = this.borderless;
		set.quality = this.quality;

		set.musicVolume = this.musicVolume;
		set.effectsVolume = this.effectsVolume;

		SerializerHelper.SaveFileXml (set, settingsFile);

		string shortcut = Application.dataPath + "\\..\\ultrafight.bat";
		string content = "@ECHO OFF\nstart Ultrafight.exe\nexit";
		if (borderless)
		{
			content = "@ECHO OFF\nstart Ultrafight.exe -popupwindow\nexit";
		}
		File.WriteAllText (shortcut, content);
	}

	public void LoadOptions ()
	{
		if (File.Exists (settingsFile))
		{
			Settings set = SerializerHelper.LoadFileXml<Settings> (settingsFile);

			this.resolution = set.resolution;
			this.screenMode = set.screenMode;
			this.borderless = set.borderless;
			this.quality = set.quality;

			this.musicVolume = set.musicVolume;
			this.effectsVolume = set.effectsVolume;
		}

		else
		{
			SaveOptions ();
		}
	}
}

[System.Serializable]
public class Settings
{
	// Video
	public string resolution;
	public string screenMode;
	public bool borderless;
	public string quality;

	// Audio
	public float musicVolume;
	public float effectsVolume;

	public Settings ()
	{}
}
