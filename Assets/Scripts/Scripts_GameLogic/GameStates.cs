using UnityEngine;
using System.Collections;

public enum MajorGameState {Stopped, Loading, Lobby, Running, Results};
public enum MinorGameState {None, GamePrep, CardSelection, BattlePrep, Battle, Results};

public class GameStates : MonoBehaviour
{
	public static GameStates acces;

	public MajorGameState majorGameState = MajorGameState.Stopped;
	public MinorGameState minorGameState = MinorGameState.None;

	void Awake ()
	{
		acces = this;
	}

	public void Reset ()
	{
		majorGameState = MajorGameState.Stopped;
		minorGameState = MinorGameState.None;
	}
}
