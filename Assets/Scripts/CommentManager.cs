using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommentManager : MonoBehaviour
{
	[System.Serializable]
	public class CommentData
	{
		public string passWord;
		public string hostName;
		public List<string> users;
		public string protocol;
		
		public CommentData (string passWord, string hostName, List<string> users, string protocol)
		{
			this.passWord = passWord;
			this.hostName = hostName;
			this.users = users;
			this.protocol = protocol;
		}
		
		public CommentData ()
		{
			users = new List<string>();
		}
	}

	public static string SerialiseComment (string passWord, string hostName, List<string> users, string protocol)
	{
		CommentData commentData = new CommentData (passWord, hostName, users, protocol);
		return SerializerHelper.SerializeToString (commentData);
	}

	public static CommentData GetCommentData (string comment)
	{
		return SerializerHelper.DeserializeFromString<CommentData> (comment);
	}
}
