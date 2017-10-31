using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarouselBehaviour : MonoBehaviour
{
	public GameObject cardWhitePrefab;
	public GameObject cardBlackPrefab;
	
	public List<GameObject> cardList = new List<GameObject>();
	public CardBehaviour selectedCard;

	public CarouselSliderScript slider;
	private bool tracking = false;

	private float lastMousepos;
	private float currentMousepos;

	private Vector3 sliderPos;

	void Start ()
	{
		sliderPos = slider.transform.position;
	}

	void Update ()
	{
		// Parse cards
		bool cardDown = false;
		foreach (GameObject card in cardList)
		{
			CardBehaviour script = card.GetComponent<CardBehaviour> ();

			if (script.isClicked)
			{
				DeselectAllCards ();
				script.isClicked = false;
				script.isSelected = true;
				selectedCard = script;
			}

			if (script.mouseDown)
			{
				cardDown = true;
			}
		}

		if (cardDown || slider.mouseDown)
		{
			tracking = true;
		}
		else
		{
			tracking = false;
		}

		// raycast
		lastMousepos = currentMousepos;
		RaycastHit rayHit = new RaycastHit();
		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
		{
			currentMousepos = rayHit.point.x;
		}
		
		// Movement
		if (tracking)
		{
			this.gameObject.transform.position = new Vector3 (this.gameObject.transform.position.x - (currentMousepos - lastMousepos) * 1.3f, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
			slider.transform.position = sliderPos;
		}
	}

	public void AddData (CardContent card)
	{
		GameObject go;
		
		if (card.type == (int)CardType.Character)
		{
			go = (GameObject) Object.Instantiate (cardWhitePrefab, new Vector3(0,0,0), Quaternion.identity);
		}
		else
		{
			go = (GameObject) Object.Instantiate (cardBlackPrefab, new Vector3(0,0,0), Quaternion.identity);
		}
		
		go.transform.SetParent (this.gameObject.transform, false);
		
		go.GetComponent<CardBehaviour>().InitialiseCard (card, "");
		cardList.Add (go);

		SpaceCards ();
	}
	
	public void AddData (List<CardContent> cards)
	{
		foreach (CardContent card in cards)
		{
			AddData (card);
		}
	}

	public void SpaceCards ()
	{
		float difference = 0;
		int itteration = 0;

		foreach (GameObject card in cardList)
		{
			itteration ++;
			card.transform.position = new Vector3 (difference, card.transform.position.y, card.transform.position.z);

			if (difference > 0)
			{
				difference = difference + (2.5f * itteration * -1);
			}
			else
			{
				difference = difference + (2.5f * itteration);
			}
		}
	}

	public void DeselectAllCards ()
	{
		foreach (GameObject card in cardList)
		{
			CardBehaviour script = card.GetComponent<CardBehaviour> ();
			script.isSelected = false;
		}
	}

	public CardContent GetSelectedCard ()
	{
		return selectedCard.card;
	}
}
