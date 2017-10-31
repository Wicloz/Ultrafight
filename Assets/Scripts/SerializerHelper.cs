using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class SerializerHelper : MonoBehaviour
{
	public static string SerializeToString<T> (T data)
	{
		using (MemoryStream ms = new MemoryStream ())
		{
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (ms, data);
			byte[] b = ms.ToArray();
			return System.Convert.ToBase64String(b);
		}
	}
	
	public static T DeserializeFromString<T> (string input)
	{
		if (input == null)
		{
			input = "";
		}
		byte[] b = System.Convert.FromBase64String(input);

		using (MemoryStream ms = new MemoryStream (b))
		{
			BinaryFormatter bf = new BinaryFormatter ();
			T data = (T) bf.Deserialize (ms);
			return data;
		}
	}

	public static List<string> SerializeToString<T> (T data, int size)
	{
		string s = SerializeToString (data);
		return SplitString (s, size);
	}

	public static List<string> SplitString (string input, int size)
	{
		List<string> sections = new List<string>();

		int startChar = 0;
		for (int i = 0; i < input.ToCharArray().Length - 4000; i += 4000)
		{
			sections.Add (input.Substring (i, i + 4000));
			startChar = i + 4000;
		}
		sections.Add (input.Substring (startChar));
		
		while (sections.Count < size)
		{
			sections.Add ("");
		}

		return sections;
	}

	public static void SaveFileXml<T>(T data, string file)
	{
		bool succeed = false;
		
		while (!succeed)
		{
			try
			{
				using (FileStream fs = File.Create(file))
				{
					XmlSerializer xs = new XmlSerializer(typeof(T));
					xs.Serialize(fs, data);
				}
				
				succeed = true;
			}
			catch
			{ }
		}
	}
	
	public static T LoadFileXml<T>(string file)
	{
		T returnValue;
		
		try
		{
			using (FileStream fs = File.Open(file, FileMode.Open))
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				returnValue = (T)xs.Deserialize(fs);
			}
		}
		catch
		{
			returnValue = default(T);
		}
		
		return returnValue;
	}
	
	public static void SaveFileBf<T>(T data, string file)
	{
		bool succeed = false;
		
		while (!succeed)
		{
			try
			{
				using (FileStream fs = File.Create(file))
				{
					BinaryFormatter bf = new BinaryFormatter();
					bf.Serialize(fs, data);
				}
				
				succeed = true;
			}
			catch
			{ }
		}
	}
	
	public static T LoadFileBf<T>(string file)
	{
		T returnValue;
		
		try
		{
			using (FileStream fs = File.Open(file, FileMode.Open))
			{
				BinaryFormatter bf = new BinaryFormatter();
				returnValue = (T)bf.Deserialize(fs);
			}
		}
		catch
		{
			returnValue = default(T);
		}
		
		return returnValue;
	}
}
