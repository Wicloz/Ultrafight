using UnityEngine;
using System.Collections;

public class CarouselSliderScript : MonoBehaviour
{
	public bool mouseDown = false;

	void OnMouseDown ()
	{
		mouseDown = true;
	}
	
	void OnMouseUp ()
	{
		mouseDown = false;
	}
}
