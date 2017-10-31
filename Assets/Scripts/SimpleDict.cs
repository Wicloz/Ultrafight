using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Wicloz
{
	namespace Collections
	{
		public class SimpleDict<TKey, TValue>
		{
			List <TKey> keyList = new List <TKey> ();
			Dictionary <TKey, TValue> valueDict = new Dictionary <TKey, TValue> ();

			public void Add (TKey key, TValue value)
			{
				valueDict.Add (key, value);
				keyList.Add (key);
			}

			public void Remove (TKey key)
			{
				valueDict.Remove (key);
				keyList.Remove (key);
			}

			public void Edit (TKey key, TValue value)
			{
				valueDict [key] = value;
			}

			public void Edit (int index, TValue value)
			{
				valueDict [keyList [index]] = value;
			}

			public TValue Acces (TKey key)
			{
				return valueDict [key];
			}

			public TValue Acces (int index)
			{
				return valueDict [keyList [index]];
			}

			public TKey GetKey (int index)
			{
				return keyList [index];
			}

			public int Count
			{
				get
				{
					return keyList.Count;
				}
			}

			public bool ContainsKey (TKey key)
			{
				return valueDict.ContainsKey (key);
			}
		}
	}
}
