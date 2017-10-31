using UnityEngine;
using System.Collections;

public class CardBehaviour : MonoBehaviour
{
	public bool isClicked = false;
	public bool isSelected = false;

	private Material original;
	public Material selected;

	public bool mouseDown = false;

	public TypogenicText textMesh;
	public CardContent card;
	public string owner;

	public Camera perspCam;

	void Awake ()
	{
		original = this.gameObject.GetComponent<Renderer>().material;
		perspCam = GameObject.Find ("PerspectiveCamera").GetComponent<Camera>();
	}

	void OnMouseDown ()
	{
		isClicked = true;
		mouseDown = true;
	}

	void OnMouseUp ()
	{
		mouseDown = false;
	}

	void Update ()
	{
		if (isSelected)
		{
			this.gameObject.GetComponent<Renderer>().material = selected;
		}
		else
		{
			this.gameObject.GetComponent<Renderer>().material = original;
		}

		float xPos = 0;
		float yPos = 0;

		RaycastHit rayHit = new RaycastHit();
		if(Physics.Raycast(perspCam.ScreenPointToRay(Input.mousePosition), out rayHit))
		{
			if (rayHit.transform == this.transform)
			{
				xPos = rayHit.point.x - rayHit.transform.position.x;
				yPos = rayHit.point.y - rayHit.transform.position.y;
			}
		}

		transform.localRotation = Quaternion.Euler (new Vector3 (yPos * -2, xPos * -4, 0));
	}

	public void InitialiseCard (CardContent card, string owner)
	{
		this.owner = owner;
		this.card = card;
		textMesh.Text = this.card.name;

		for (int i = 0; i < 400; i++)
		{
			textMesh.Text = textMesh.Text + " ";
		}
		textMesh.Text = textMesh.Text + ".";
	}
}
