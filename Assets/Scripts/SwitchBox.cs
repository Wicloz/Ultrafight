using UnityEngine;
using System.Collections;

public class SwitchBox : MonoBehaviour
{
	public static GameObject Instantiate (GameObject prefab, Vector3 position, Quaternion rotation)
	{
		GameObject instantiation = null;
		
		if (Network.isClient || Network.isServer)
		{
			instantiation = (GameObject) Network.Instantiate (prefab, position, rotation, 0);
		}
		else
		{
			instantiation = (GameObject) Object.Instantiate (prefab, position, rotation);
		}
		
		return instantiation;
	}
	
	public static void Destroy (GameObject instance)
	{
		if (Network.isClient || Network.isServer)
		{
			Network.Destroy (instance);
		}
		else
		{
			Object.Destroy (instance);
		}
	}

	public static bool isHost
	{
		get
		{
			if (!Network.isClient)
			{
				return true;
			}
			else return false;
		}
	}

	public static bool isServer
	{
		get
		{
			return Network.isServer;
		}
	}

	public static bool isClient
	{
		get
		{
			return Network.isClient;
		}
	}

	public static bool isServerOn
	{
		get
		{
			if (Network.isClient || Network.isServer)
			{
				return true;
			}
			else return false;
		}
	}
}
