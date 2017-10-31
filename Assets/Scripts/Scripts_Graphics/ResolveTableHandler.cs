using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResolveTableHandler : MonoBehaviour
{
	private List<GameObject> lines = new List<GameObject>();
	public GameObject linePrefab;

	public TypogenicText weaponText;
	public TypogenicText attackerText;
	public TypogenicText defenderText;
	public TypogenicText timerText;

	public void Initialise (GraphicSet graphics)
	{
		weaponText.Text = graphics.weaponName;
		attackerText.Text = graphics.attackerChar;
		defenderText.Text = graphics.defenderChar;

		for (int i = 0; i < graphics.attacks.Count; i++)
		{
			GameObject line = (GameObject) Object.Instantiate (linePrefab, new Vector3(0, (-1 * i) - 1.5f, 0), Quaternion.identity);
			line.transform.SetParent (this.transform, false);
			line.transform.localScale = new Vector3 (1,1,1);
			line.GetComponent<ResolveTableLineHandler>().Initialise (graphics.attacks[i].damageType, graphics.attacks[i].reaction);
			lines.Add (line);
		}
	}

	public void SetTime (float time)
	{
		float roundTime = ((float)((int) time * 10)) / 10;
		timerText.Text = System.Convert.ToString (roundTime);
	}
}
