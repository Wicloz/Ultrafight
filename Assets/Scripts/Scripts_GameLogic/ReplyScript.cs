using UnityEngine;
using System.Collections;

public class ReplyScript : MonoBehaviour
{
	[RPC] public void Marco (string username)
	{
		GetComponent<NetworkView>().RPC ("Polo", RPCMode.Server, username);
	}
}
