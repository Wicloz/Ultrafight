using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class ConfigManager : MonoBehaviour
{
	public static ConfigManager acces;
	public ConfigFile temp;
	public ConfigWriter configWriter = new ConfigWriter ();

	// Folders
	private string folderRoot;
	private string folderCharacters;
	private string folderWeapons;
	private string folderArmour;
	private string folderVehicles;

	// Development GUI
	public GameObject cardTab;

	private string field = "test";
	public int top = 100;
	public int left = 100;

	// Temporary loading data
	private string lastError = "";
	private Queue<string> loadQueue = new Queue<string>();

	// Single config load data
	private bool lastConfigLoaded = false;
	private string lastMessage = "";
	private ConfigFile lastParse;

	// List and info
	public bool allConfigsLoaded = false;

	public int totalConfigs = 0;
	public int loadSucces = 0;
	public int loadFail = 0;

	private List<CardContent> characterList = new List<CardContent>();
	private List<CardContent> equipmentList = new List<CardContent>();

	public List<CardContent> gameCharacterList = new List<CardContent>();
	public List<CardContent> gameEquipmentList = new List<CardContent>();

	void Awake ()
	{
		acces = this;

		folderRoot = Application.dataPath + "\\..\\GameData";
		folderCharacters = folderRoot + "\\Characters";
		folderWeapons = folderRoot + "\\Weapons";
		folderArmour = folderRoot + "\\Armour";
		folderVehicles = folderRoot + "\\Vehicles";
	}

	void Start ()
	{
		Directory.CreateDirectory (folderCharacters);
		Directory.CreateDirectory (folderWeapons);
		Directory.CreateDirectory (folderArmour);
		Directory.CreateDirectory (folderVehicles);

		StartCoroutine("LoadAllConfigs");
	}

	private void CopyCardLists ()
	{
		gameCharacterList = GetCharacterList ();
		gameEquipmentList = GetEquipmentList ();
	}

	public List<CardContent> GetCharacterList ()
	{
		return SerializerHelper.DeserializeFromString<List<CardContent>> (SerializerHelper.SerializeToString (characterList));
	}

	public List<CardContent> GetEquipmentList ()
	{
		return SerializerHelper.DeserializeFromString<List<CardContent>> (SerializerHelper.SerializeToString (equipmentList));
	}

	public void AddClientCharacterCard (CardContent card)
	{
		AddClientCard (card, gameCharacterList);
	}

	public void AddClientEquipmentCard (CardContent card)
	{
		AddClientCard (card, gameEquipmentList);
	}

	public void AddClientCard (CardContent card, List<CardContent> list)
	{
		bool exists = false;

		foreach (CardContent cc in list)
		{
			if (cc.name.ToLower() == card.name.ToLower() && cc.type == card.type)
			{
				exists = true;
			}
		}

		if (!exists)
		{
			Debug.Log ("Adding card " + card.name);
			list.Add (card);
		}
	}

	public List<CardContent> GetCharacterCards (int amount)
	{
		return GetCards (amount, "character");
	}

	public List<CardContent> GetEquipmentCards (int amount)
	{
		return GetCards (amount, "equipment");
	}

	private List<CardContent> GetCards (int amount, string type)
	{
		List<CardContent> sourcelist = new List<CardContent>();

		switch (type)
		{
		case "character":
			foreach (CardContent card in gameCharacterList)
			{
				if (card.name != "template")
				{
					sourcelist.Add (card);
				}
			}
			break;
		case "equipment":
			foreach (CardContent card in gameEquipmentList)
			{
				if (card.type != (int)CardType.Vehicle && card.name != "template")
				{
					sourcelist.Add (card);
				}
			}
			break;
		}

		List<int> picks = new List<int> ();
		List<CardContent> cards = new List<CardContent> ();
		
		for (int i = 0; i < amount; i++)
		{
			picks.Add (Random.Range (0, sourcelist.Count - 1));
		}
		
		foreach (int i in picks)
		{
			cards.Add (sourcelist[i]);
		}
		
		return cards;
	}

	void OnGUI ()
	{
		if (cardTab.activeSelf)
		{
			field = GUI.TextField(new Rect(left, top, 200, 22), field);

			if (GUI.Button(new Rect(left, top + 40, 200, 40), "Reload Configurations"))
			{
				StartCoroutine("LoadAllConfigs");
			}

			if (GUI.Button(new Rect(left, top + 40 + 50, 200, 40), "Create Character"))
			{
				configWriter.CreateConfig (folderCharacters + "\\" + field.ToLower() + ".cfg", "character", field);
			}

			if (GUI.Button(new Rect(left, top + 40 + 100, 200, 40), "Create Weapon"))
			{
				configWriter.CreateConfig (folderWeapons + "\\" + field.ToLower() + ".cfg", "weapon", field);
			}

			if (GUI.Button(new Rect(left, top + 40 + 150, 200, 40), "Create Armour"))
			{
				configWriter.CreateConfig (folderArmour + "\\" + field.ToLower() + ".cfg", "armour", field);
			}

			if (GUI.Button(new Rect(left, top + 40 + 200, 200, 40), "Create Vehicle"))
			{
				configWriter.CreateConfig (folderVehicles + "\\" + field.ToLower() + ".cfg", "vehicle", field);
			}

			if (GUI.Button(new Rect(left, top + 40 + 250, 200, 40), "Create a bunch of configs"))
			{
				for (int i = 0; i < 100; i++)
				{
					string name = Random.Range(0, 10000).ToString();

					configWriter.CreateConfig (folderCharacters + "\\" + name.ToLower() + ".cfg", "character", name);
					configWriter.CreateConfig (folderWeapons + "\\" + name.ToLower() + ".cfg", "weapon", name);
					configWriter.CreateConfig (folderArmour + "\\" + name.ToLower() + ".cfg", "armour", name);
					configWriter.CreateConfig (folderVehicles + "\\" + name.ToLower() + ".cfg", "vehicle", name);
				}
			}
		}
	}

	private IEnumerator LoadAllConfigs ()
	{
		allConfigsLoaded = false;
		characterList = new List<CardContent> ();
		equipmentList = new List<CardContent> ();
		loadSucces = 0;
		loadFail = 0;
	
		foreach (string file in Directory.GetFiles(folderRoot, "*.cfg", SearchOption.AllDirectories))
		{
			loadQueue.Enqueue(file);
		}

		totalConfigs = loadQueue.Count;
		yield return null;

		for (int i = 0; i < totalConfigs; i++)
		{
			lastMessage = "";
			lastError = "";
			lastConfigLoaded = false;
			lastParse = null;

			StartCoroutine (LoadConfig(loadQueue.Dequeue()));

			while (!lastConfigLoaded)
			{
				yield return null;
			}

			string message = lastMessage;
			Debug.LogError(message);
			
			if (message.Contains ("Loading of '") && message.Contains ("' Succesfull"))
			{
				loadSucces ++;
			}
			else
			{
				loadFail ++;
			}

			yield return null;
		}

		CopyCardLists ();
		allConfigsLoaded = true;
	}

	private IEnumerator LoadConfig (string path)
	{
		CardContent card = new CardContent ();
		string mode = "none";

		StartCoroutine (ParseBlocks (path));

		while (lastParse == null)
		{
			yield return null;
		}

		ConfigFile cf = lastParse;
		temp = cf;

		if (!cf.succes)
		{
			lastMessage = "Invalid config structure";
			lastConfigLoaded = true;
			return false;
		}

		Debug.Log("Starting loading procedure");

		// Create main information
		switch (LoadString(cf.main.content, "type"))
		{
		case "character":
			mode = "character";
			card.type = (int)CardType.Character;
			break;
		case "weapon":
			mode = "equipment";
			card.type = (int)CardType.Weapon;
			break;
		case "armour":
			mode = "equipment";
			card.type = (int)CardType.Armour;
			break;
		case "vehicle":
			mode = "equipment";
			card.type = (int)CardType.Vehicle;
			break;
		default:
			lastMessage = "Incorrect value for TYPE";
			lastConfigLoaded = true;
			return false;
		}
		
		card.name = LoadString (cf.main.content, "name");
		card.usage = LoadString (cf.main.content, "usageVerb");
		card.description = LoadString (cf.main.content, "description");
		card.strength = LoadFromEnum (ConfigContainers.strength, cf.main.content, "strength");
		card.prioritiseRanged = LoadBool (cf.main.content, "prioritiseRangedAttack");
		
		if (LoadBool(cf.main.content, "deployment.submerged"))
		{
			card.deployment.Add((int)Deployment.Submerged);
		}
		if (LoadBool(cf.main.content, "deployment.water"))
		{
			card.deployment.Add((int)Deployment.Water);
		}
		if (LoadBool(cf.main.content, "deployment.land"))
		{
			card.deployment.Add((int)Deployment.Land);
		}
		if (LoadBool(cf.main.content, "deployment.air"))
		{
			card.deployment.Add((int)Deployment.Air);
		}
		
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			if (LoadBool(cf.main.content, "immunity." + damage.codeName))
			{
				card.immunities.Add (damage.codeName);
			}
		}

		yield return null;

		// Create module information
		foreach (ModuleInfo module in cf.modules)
		{
			if (module.type == "melee")
			{
				SectionMelee melee = new SectionMelee ();
				card.melee.Add (melee);

				melee.meleeEnabled = LoadBool (module.content, "enabled");
				melee.name = LoadString (module.content, "name");

				melee.fixedStrength = LoadBool (module.content, "fixedStrength");
				melee.strengthModifier = LoadInt (module.content, "strengthModifier");

				melee.damageDirectEnabled = LoadBool (module.content, "enableDirectDamage");
				melee.damageSmallEnabled = LoadBool (module.content, "enableSmallblastDamage");
				melee.damageLargeEnabled = LoadBool (module.content, "enableLargeblastDamage");

				foreach (DamageType damage in DamageTypes.damageTypes)
				{
					if (LoadBool(module.content, "directDamage." + damage.codeName))
					{
						melee.damageTypesDirect.Add (damage.codeName);
					}
				}
				foreach (DamageType damage in DamageTypes.damageTypes)
				{
					if (LoadBool(module.content, "smallDamage." + damage.codeName))
					{
						melee.damageTypesSmall.Add (damage.codeName);
					}
				}
				foreach (DamageType damage in DamageTypes.damageTypes)
				{
					if (LoadBool(module.content, "largeDamage." + damage.codeName))
					{
						melee.damageTypesLarge.Add (damage.codeName);
					}
				}
			}

			else if (module.type == "ranged")
			{
				SectionRanged ranged = new SectionRanged ();
				card.ranged.Add (ranged);
				
				ranged.rangedEnabled = LoadBool (module.content, "enabled");
				ranged.name = LoadString (module.content, "name");

				ranged.fixedStrength = LoadBool (module.content, "fixedStrength");
				ranged.strengthModifier = LoadInt (module.content, "strengthModifier");

				ranged.speed = LoadFromEnum (ConfigContainers.projectileSpeed, module.content, "projectileSpeed");
				ranged.projectileAmount = LoadInt (module.content, "projectileAmount");

				if (LoadBool(module.content, "targetable.submerged"))
				{
					ranged.targetable.Add((int)Deployment.Submerged);
				}
				if (LoadBool(module.content, "targetable.water"))
				{
					ranged.targetable.Add((int)Deployment.Water);
				}
				if (LoadBool(module.content, "targetable.land"))
				{
					ranged.targetable.Add((int)Deployment.Land);
				}
				if (LoadBool(module.content, "targetable.air"))
				{
					ranged.targetable.Add((int)Deployment.Air);
				}

				foreach (DamageType damage in DamageTypes.damageTypes)
				{
					if (LoadBool(module.content, "immunity." + damage.codeName))
					{
						ranged.projectileImmunities.Add (damage.codeName);
					}
				}
				
				ranged.damageDirectEnabled = LoadBool (module.content, "enableDirectDamage");
				ranged.damageSmallEnabled = LoadBool (module.content, "enableSmallblastDamage");
				ranged.damageLargeEnabled = LoadBool (module.content, "enableLargeblastDamage");

				foreach (DamageType damage in DamageTypes.damageTypes)
				{
					if (LoadBool(module.content, "directDamage." + damage.codeName))
					{
						ranged.damageTypesDirect.Add (damage.codeName);
					}
				}
				foreach (DamageType damage in DamageTypes.damageTypes)
				{
					if (LoadBool(module.content, "smallDamage." + damage.codeName))
					{
						ranged.damageTypesSmall.Add (damage.codeName);
					}
				}
				foreach (DamageType damage in DamageTypes.damageTypes)
				{
					if (LoadBool(module.content, "largeDamage." + damage.codeName))
					{
						ranged.damageTypesLarge.Add (damage.codeName);
					}
				}
			}

			else
			{
				Debug.Log("Unrecognised module type, skipping load ...");
			}

			yield return null;
		}

		// Check and finish
		if (lastError != "")
		{
			lastMessage = lastError;
			lastConfigLoaded = true;
			return true;
		}

		switch (mode)
		{
		case "character":
			characterList.Add(card);
			break;
		case "equipment":
			equipmentList.Add(card);
			break;
		}

		if (true && card.name != "template")
		{
			configWriter.ReWriteConfigFile (cf, path);
		}

		lastMessage = "Loading of '" + card.name + "' Succesful";
		lastConfigLoaded = true;
	}

	private int LoadFromEnum (Dictionary<string, int> table, List<ConfigField> content, string identifier)
	{
		string key = LoadString (content, identifier);

		if (key == "")
		{
			return 0;
		}

		if (table.ContainsKey(key))
		{
			return table[key];
		}
		else if (lastError == "")
		{
			lastError = "Incorrect value for " + identifier.ToUpper() + " on " + LoadString (content, "name");
		}

		return 0;
	}

	private string LoadString (List<ConfigField> content, string identifier)
	{
		return GetValue (content, identifier);
	}

	private int LoadInt (List<ConfigField> content, string identifier)
	{
		if (GetValue (content, identifier) != "")
		{
			try
			{
				int value = System.Convert.ToInt32 (GetValue (content, identifier));
				return value;
			}
			catch
			{
				if (lastError == "")
				{
					lastError = "Incorrect value for " + identifier.ToUpper() + " on " + LoadString (content, "name");
				}
				return 0;
			}
		}
		else
		{
			return 0;
		}
	}

	private bool LoadBool (List<ConfigField> content, string identifier)
	{
		if (GetValue (content, identifier) != "")
		{
			switch (GetValue (content, identifier))
			{
			case "true":
				return true;
			case "false":
				return false;
			default:
				if (lastError == "")
				{
					lastError = "Incorrect value for " + identifier.ToUpper() + " on " + LoadString (content, "name");
				}
				return false;
			}
		}
		else
		{
			return false;
		}
	}

	private bool ToBool (string input)
	{
		switch (input)
		{
		case "true":
			return true;
		case "false":
			return false;
		default:
			return false;
		}
	}

	private string GetValue (List<ConfigField> fields, string key)
	{
		string s = "";

		foreach (ConfigField field in fields)
		{
			if (field.key == key)
			{
				s = field.value;
			}
		}

		return s;
	}

	private IEnumerator ParseBlocks (string path)
	{
		ConfigFile cf = new ConfigFile ();
		
		string currentline = "";
		int moduleNumber = -1;
		string identifierCurrent = "none";
		string identifierNext = "";
		
		int waitLine = 5;
		bool done = false;

		using (StreamReader file = File.OpenText (path))
		{
			while (!done)
			{
				currentline = file.ReadLine();
				waitLine --;
				Debug.Log("Reading Config Line");
				
				if (!currentline.Trim().StartsWith("//"))
				{
					if (currentline.Contains("CARD"))
					{
						if (identifierCurrent == "none")
						{
							identifierNext = "card";
						}
						else
						{
							cf.succes = false;
							lastParse = cf;
						}
					} // end card
					
					else if (currentline.Contains("MODULE"))
					{
						if (identifierCurrent == "card")
						{
							identifierNext = currentline.Trim().ToLower();
							moduleNumber++;
						}
						else
						{
							cf.succes = false;
							lastParse = cf;
						}
					} // end module
					
					else if (currentline.Contains("{"))
					{
						if (identifierNext != "")
						{
							identifierCurrent = identifierNext;
							identifierNext = "";
						}
						else
						{
							cf.succes = false;
							lastParse = cf;
						}
					} // end {
					
					else if (currentline.Contains("}"))
					{
						if (identifierCurrent == "card" && identifierNext == "")
						{
							done = true;
						}
						else if (identifierCurrent.Contains("module") && identifierNext == "")
						{
							identifierCurrent = "card";
						}
						else
						{
							cf.succes = false;
							lastParse = cf;
						}
					} // end }
					
					if (currentline.Contains("=") && currentline.Replace(" ", "").Replace("\t", "").Replace("}", "").Replace("{", "") != "")
					{
						if (identifierCurrent == "card")
						{
							cf.main.content.Add (LineToField (currentline));
						}
						
						else if (identifierCurrent == "module_melee")
						{
							if (cf.modules.Count <= moduleNumber)
							{
								cf.modules.Add(new ModuleInfo("melee"));
							}
							
							cf.modules[moduleNumber].content.Add (LineToField (currentline));
						}
						
						else if (identifierCurrent == "module_ranged")
						{
							if (cf.modules.Count <= moduleNumber)
							{
								cf.modules.Add(new ModuleInfo("ranged"));
							}
							
							cf.modules[moduleNumber].content.Add (LineToField (currentline));
						}
						
						else if (identifierCurrent.Contains("module"))
						{
							if (cf.modules.Count <= moduleNumber)
							{
								cf.modules.Add(new ModuleInfo("unknown"));
							}
							
							cf.modules[moduleNumber].content.Add (LineToField (currentline));
						}
					}
				}
				
				if (waitLine == 0)
				{
					waitLine = 5;
					yield return null;
				}
			}
		}
	
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			// Add main card immunities
			bool damageMainPresent = false;
			bool immunityMainPresent = false;
			int lastMainDamage = 1;
			for (int i = 0; i < cf.main.content.Count; i++)
			{
				ConfigField field = cf.main.content[i];

				if (field.key.Contains ("immunity."))
				{
					lastMainDamage = i;
					immunityMainPresent = true;
				}
				if (field.key == "immunity." + damage.codeName)
				{
					damageMainPresent = true;
					break;
				}
			}
			if (!damageMainPresent && immunityMainPresent)
			{
				cf.main.content.Insert (lastMainDamage + 1, new ConfigField ("immunity." + damage.codeName, "false"));
			}

			// Add ranged module immunities
			bool damageRangedPresent = false;
			bool immunityRangedPresent = false;
			int lastRangedDamage = 1;
			ModuleInfo ranged = new ModuleInfo ("ranged");

			foreach (ModuleInfo module in cf.modules)
			{
				if (module.type == "ranged")
				{
					ranged = module;
					break;
				}
			}

			for (int i = 0; i < ranged.content.Count; i++)
			{
				ConfigField field = ranged.content[i];

				if (field.key.Contains ("immunity."))
				{
					lastRangedDamage = i;
					immunityRangedPresent = true;
				}
				if (field.key == "immunity." + damage.codeName)
				{
					damageRangedPresent = true;
					break;
				}
			}
			if (!damageRangedPresent && immunityRangedPresent)
			{
				ranged.content.Insert (lastRangedDamage + 1, new ConfigField ("immunity." + damage.codeName, "false"));
			}
		}

		yield return null;
		lastParse = cf;
	}

	private ConfigField LineToField (string line)
	{
		char[] delim = new char[] {'='};
		string[] parts = line.Split(delim);

		ConfigField field = new ConfigField (parts[0].Trim(), parts[1].Trim());
		return field;
	}
}
