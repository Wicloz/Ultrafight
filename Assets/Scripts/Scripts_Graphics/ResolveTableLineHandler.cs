using UnityEngine;
using System.Collections;

public class ResolveTableLineHandler : MonoBehaviour
{
	public TypogenicText left;
	public TypogenicText right;

	public void Initialise (string left, string right)
	{
		this.left.Text = left;
		this.right.Text = right;
	}
}
