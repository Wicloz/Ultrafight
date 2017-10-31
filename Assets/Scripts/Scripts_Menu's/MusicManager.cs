using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Music {None, MainMenu, Lobby, Game, Results, Loading};

public class MusicManager : MonoBehaviour
{
	public static MusicManager acces;

	public List<AudioClip> mainmenu = new List<AudioClip>();
	public List<AudioClip> lobby = new List<AudioClip>();
	public List<AudioClip> battlescenes = new List<AudioClip>();
	public List<AudioClip> results = new List<AudioClip>();
	public List<AudioClip> loading = new List<AudioClip>();
	
	public AudioSource gameMusicPlayer;
	public AudioSource menuMusicPlayer;
	public AudioSource loadingMusicPlayer;
	
	public Music currentMusicType;
	public Music currentGameMusicType;
	public Music currentMenuMusicType;

	private float gameVolume
	{
		get
		{
			if (currentMusicType == Music.Game || currentMusicType == Music.Lobby || currentMusicType == Music.Results)
			{
				return OptionsManager.acces.musicVolume;
			}
			else
			{
				return 0;
			}
		}
	}
	private float menuVolume
	{
		get
		{
			if (currentMusicType == Music.MainMenu)
			{
				return OptionsManager.acces.musicVolume;
			}
			else
			{
				return 0;
			}
		}
	}
	private float loadingVolume
	{
		get
		{
			if (currentMusicType == Music.Loading)
			{
				return OptionsManager.acces.musicVolume;
			}
			else
			{
				return 0;
			}
		}
	}

	void Awake ()
	{
		acces = this;
	}

	void Start ()
	{
		currentMusicType = Music.None;
		currentGameMusicType = Music.None;
		currentMenuMusicType = Music.None;
	}

	void Update ()
	{
		if (!menuMusicPlayer.isPlaying)
		{
			SetMenuMusic (currentMusicType);
		}

		if (!loadingMusicPlayer.isPlaying)
		{
			SetLoadingMusic ();
		}

		if (currentMusicType != Music.Loading)
		{
			loadingMusicPlayer.Pause();
		}

		if (!gameMusicPlayer.isPlaying && SwitchBox.isServer)
		{
			SetGameMusic (currentMusicType);
		}

		if (!gameMusicPlayer.isPlaying && !SwitchBox.isServer)
		{
			SetLobbyTrack ();
		}

		gameMusicPlayer.volume = Mathf.Lerp (gameMusicPlayer.volume, gameVolume, Time.deltaTime * 2);
		menuMusicPlayer.volume = Mathf.Lerp (menuMusicPlayer.volume, menuVolume, Time.deltaTime * 2);
		loadingMusicPlayer.volume = Mathf.Lerp (loadingMusicPlayer.volume, loadingVolume, Time.deltaTime * 2);
	}

	public void ResetGameMusic ()
	{
		Invoke ("SetLobbyTrack", 2);
	}

	private void SetLobbyTrack ()
	{
		SetGameTrack ("To the Ends");
		currentGameMusicType = Music.MainMenu;
	}

	public void SetMusicState (Music state)
	{
		currentMusicType = state;

		if (currentMusicType == Music.Loading)
		{
			loadingMusicPlayer.UnPause();
		}
		
		else if (currentMusicType == Music.Game || currentMusicType == Music.Lobby || currentMusicType == Music.Results)
		{
			SetGameMusic (currentMusicType);
		}

		else if (currentMusicType == Music.MainMenu)
		{
			SetMenuMusic (currentMusicType);
		}
	}

	public void SetMenuMusic (Music music)
	{
		if (music != currentMenuMusicType || !menuMusicPlayer.isPlaying)
		{
			menuMusicPlayer.Stop();
			int random = 0;
			
			switch (music)
			{
			case Music.MainMenu:
				random = Random.Range(0, mainmenu.Count);
				menuMusicPlayer.clip = mainmenu[random];
				break;
			}
			
			currentMenuMusicType = music;
			menuMusicPlayer.Play();
		}
	}

	public void SetLoadingMusic ()
	{
		loadingMusicPlayer.Stop();
			
		int random = Random.Range(0, loading.Count);
		loadingMusicPlayer.clip = loading[random];
			
		loadingMusicPlayer.Play();
	}

	public void SetGameMusic (Music music)
	{
		if (music != currentGameMusicType || !gameMusicPlayer.isPlaying)
		{
			string clip = "";
			int random = 0;
			
			switch (music)
			{
			case Music.Game:
				random = Random.Range(0, battlescenes.Count);
				clip = battlescenes[random].name;
				break;
			case Music.Lobby:
				random = Random.Range(0, lobby.Count);
				clip = lobby[random].name;
				break;
			case Music.Results:
				random = Random.Range(0, results.Count);
				clip = results[random].name;
				break;
			}

			currentGameMusicType = music;
			GetComponent<NetworkView>().RPC ("SetGameTrack", RPCMode.AllBuffered, clip);
		}
	}

	[RPC] public void SetGameTrack (string clipName)
	{
		gameMusicPlayer.Stop();
		List<AudioClip> musicList = new List<AudioClip> ();

		musicList.AddRange (lobby);
		musicList.AddRange (battlescenes);
		musicList.AddRange (results);

		//bool clipFound = false;
		foreach (AudioClip audio in musicList)
		{
			if (audio.name == clipName)
			{
				gameMusicPlayer.clip = audio;
				//clipFound = true;
				break;
			}
		}

		gameMusicPlayer.Play();
	}
}
