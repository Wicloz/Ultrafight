using UnityEngine;
using System.Collections;

public class GlobalRPC : MonoBehaviour
{
	public delegate void SendMessage(string name, RPCMode mode, params object[] args);
	public static SendMessage SendRPC;

	void Awake ()
	{
		SendRPC = SendMessageRPC;
	}

	public void SendMessageRPC (string name, RPCMode mode, params object[] args)
	{
		GetComponent<NetworkView>().RPC (name, mode, args);
	}
}
