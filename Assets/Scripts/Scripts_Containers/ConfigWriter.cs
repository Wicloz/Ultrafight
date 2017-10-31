using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ConfigWriter
{
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

	private string LoadString (List<ConfigField> content, string identifier)
	{
		return GetValue (content, identifier);
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

	private ConfigField CreateField (string key)
	{
		ConfigField field = new ConfigField (key, "");
		return ParseField (field);
	}
	
	private ConfigField CreateField (string key, string value)
	{
		ConfigField field = new ConfigField (key, value);
		return ParseField (field);
	}

	private ConfigField ParseField (ConfigField field)
	{
		switch (field.key)
		{
		case "usageVerb":
			field.description.Add ("\t//The verb that should be put in front of the name when a character is using this card.");
			field.description.Add ("\t//For example: 'character name' using 'a weapon name'.");
			field.description.Add ("\t//For example: Chuck Norris 'driving' the Popemobile.");
			field.description.Add ("\t//Obsolete on character cards.");
			break;
			
		case "type":
			field.description.Add ("\t//Determines as what kind of card the config will be loaded.");
			field.description.Add ("\t//Available types: character, weapon, armour, vehicle.");
			break;
			
		case "strength":
			field.description.Add ("\t//Determines the outcome of battles involving blunt damage.");
			field.description.Add ("\t//This value is only effective on character cards.");
			field.description.Add ("\t//Available values: low, high, extreme.");
			break;
			
		case "deployment.submerged":
			field.description.Add ("\t//On a character or vehicle this determines where a character or vehicle can be deployed.");
			field.description.Add ("\t//On armour this adds to locations where a character can be deployed.");
			field.description.Add ("\t//These values are obsolete on weapon cards.");
			break;
			
		case "prioritiseRangedAttack":
			field.description.Add ("\t//Determines whether the battle resolving will favour using the ranged weapon over the melee weapon.");
			field.description.Add ("\t//Set this to true if the weapon on this card is supposed to be primarily ranged (rocket launchers can be used to smack people),");
			field.description.Add ("\t//and to false if it is supposed to be primarily melee (some melee weapons can be thrown).");
			field.description.Add ("\t//This value is obsolete when there are no weapons present on the card.");
			break;
			
		case "enabled":
			field.description.Add ("\t//If a certain card should not have a module, simply remove the module.");
			field.description.Add ("\t//Alternatively, set this value to false.");
			break;
			
		case "fixedStrength":
			field.description.Add ("\t//Determines whether a weapon copies the strength from the character that wields it or not." );
			break;
			
		case "strengthModifier":
			field.description.Add ("\t//If the strength is not fixed, use this to modify it.");
			field.description.Add ("\t//IMPORTANT: Any character that has its fists or feet as a melee weapon should have a modifier of -1.");
			field.description.Add ("\t//If the strength is fixed, use this to set a value for it.");
			field.description.Add ("\t//Available values: 0 for low, 1 for high, 2 for extreme.");
			break;
			
		case "projectileSpeed":
			field.description.Add ("\t//Determines the speed of the ranged projectiles.");
			field.description.Add ("\t//Low speed projectiles can be shot down by the defender.");
			field.description.Add ("\t//Available values: low, high, beam, teleport.");
			break;
			
		case "projectileAmount":
			field.description.Add ("\t//Determines how many ranged projectiles are shot every volley.");
			break;
			
		case "targetable.submerged":
			field.description.Add ("\t//Determines what kind of areas a ranged weapon can reach. Useful to limit lock-on missile launchers.");
			field.description.Add ("\t//IMPORTANT: Think about whether this ranged weapon can aim at targets underwater, and about whether the projectiles can reach them.");
			break;
			
		case "enableDirectDamage":
			field.description.Add ("\t//Use this to enable or disable the various AOE ranges of the weapon.");
			field.description.Add ("\t//DirectDamage indicates damage that is done when the weapon hits the target.");
			field.description.Add ("\t//SmallblastDamage indicates damage that is done in a small to large AOE around the impact point. (Radius from grenade to ICBM.)");
			field.description.Add ("\t//LargeblastDamage indicates damage that is done in a pretty massive AOE around the impact point. Use sparingly. (Radius like a nuke.)");
			break;
			
		case "[0]":
			field.description.Add ("\t//On a character or vehicle this determines what kind of damage will not cause a defeat.");
			field.description.Add ("\t//On armour this adds to the character's immunities.");
			field.description.Add ("\t//These values are obsolete on weapon cards.");
			break;
			
		case "[1]":
			field.description.Add ("\t//Determines the kinds of damages dealt in their respective ranges.");
			field.description.Add ("\t//Any range that is disabled can be removed.");
			field.description.Add ("\t//Any damage not dealt can also be removed.");
			break;
			
		case "[2]":
			field.description.Add ("\t//Determines what kind of damage the projectile is immune to when shot by the defender.");
			field.description.Add ("\t//Obsolete when the speed isn't low.");
			break;
		}
		
		return field;
	}
	
	public void ReWriteConfigFile (ConfigFile config, string path)
	{
		string type = LoadString(config.main.content, "type");
		bool added0 = false;
		
		for (int i = 0; i < config.main.content.Count; i++)
		{
			ConfigField field = config.main.content[i];
			
			if (type == "character" && field.key == "usageVerb")
			{
				config.main.content.RemoveAt (i);
				i--;
			}
			
			else if (type != "character" && field.key == "strength")
			{
				config.main.content.RemoveAt (i);
				i--;
			}
			
			else if (type == "weapon" && (field.key.Contains ("deployment.") || field.key.Contains ("immunity.")))
			{
				config.main.content.RemoveAt (i);
				i--;
			}
			
			else
			{
				if (!added0 && field.key.Contains ("immunity."))
				{
					config.main.content.Insert (i, CreateField ("[0]"));
					added0 = true;
					i++;
				}
				
				config.main.content[i] = ParseField (field);
			}
		}
		
		foreach (ModuleInfo module in config.modules)
		{
			bool added1 = false;
			bool added2 = false;
			
			for (int i = 0; i < module.content.Count; i++)
			{
				ConfigField field = module.content[i];
				
				if (field.key == "fixedStrength" && (!LoadBool(module.content, "directDamage.blunt") && !LoadBool(module.content, "smallDamage.blunt") && !LoadBool(module.content, "largeDamage.blunt")))
				{
					module.content.RemoveAt (i);
					i--;
				}
				else if (field.key == "strengthModifier" && (!LoadBool(module.content, "directDamage.blunt") && !LoadBool(module.content, "smallDamage.blunt") && !LoadBool(module.content, "largeDamage.blunt")))
				{
					module.content.RemoveAt (i);
					i--;
				}
				
				else if (field.key.Contains ("immunity.") && LoadString(module.content, "projectileSpeed") != "low")
				{
					module.content.RemoveAt (i);
					i--;
				}
				
				else if (field.key.Contains ("directDamage.") && !ToBool(field.value))
				{
					module.content.RemoveAt (i);
					i--;
				}
				
				else if (field.key.Contains ("smallDamage.") && !ToBool(field.value))
				{
					module.content.RemoveAt (i);
					i--;
				}
				
				else if (field.key.Contains ("largeDamage.") && !ToBool(field.value))
				{
					module.content.RemoveAt (i);
					i--;
				}
				
				else
				{
					if (!added2 && field.key.Contains ("immunity."))
					{
						module.content.Insert (i, CreateField ("[2]"));
						added2 = true;
						i++;
					}
					
					if (!added1 && (field.key.Contains ("directDamage.") || field.key.Contains ("smallDamage.") || field.key.Contains ("largeDamage.")))
					{
						module.content.Insert (i, CreateField ("[spacer]"));
						module.content.Insert (i, CreateField ("[1]"));
						added1 = true;
						i += 2;
					}
					
					module.content[i] = ParseField (field);
				}
			}
		}
		
		WriteConfigToFile (config, path);
	}
	
	public void CreateConfig (string path, string type, string name)
	{
		ConfigFile cf = new ConfigFile ();
		
		// Create main body
		cf.main.content.Add (CreateField ("name", name));
		cf.main.content.Add (CreateField ("description", "template"));
		
		if (type != "character")
		{
			cf.main.content.Add (CreateField ("usageVerb"));
		}
		
		cf.main.content.Add (CreateField ("type", type));
		
		if (type == "character")
		{
			cf.main.content.Add (CreateField ("strength", "low"));
		}
		
		if (type != "weapon")
		{
			cf.main.content.Add (CreateField ("deployment.submerged", "false"));
			cf.main.content.Add (CreateField ("deployment.water", "false"));
			cf.main.content.Add (CreateField ("deployment.land", "true"));
			cf.main.content.Add (CreateField ("deployment.air", "false"));
			
			cf.main.content.Add (CreateField ("[0]"));
			foreach (DamageType damage in DamageTypes.damageTypes)
			{
				cf.main.content.Add (CreateField ("immunity." + damage.codeName, "false"));
			}
		}
		
		cf.main.content.Add (CreateField ("prioritiseRangedAttack", "false"));
		
		// Create melee module
		cf.modules.Add (new ModuleInfo ("melee"));
		
		cf.modules[0].content.Add (CreateField ("enabled", "true"));
		cf.modules[0].content.Add (CreateField ("name", "Melee Attack"));
		
		cf.modules[0].content.Add (CreateField ("fixedStrength", "false"));
		cf.modules[0].content.Add (CreateField ("strengthModifier", "0"));
		
		cf.modules[0].content.Add (CreateField ("enableDirectDamage", "true"));
		cf.modules[0].content.Add (CreateField ("enableSmallblastDamage", "false"));
		cf.modules[0].content.Add (CreateField ("enableLargeblastDamage", "false"));
		
		cf.modules[0].content.Add (CreateField ("[1]"));
		cf.modules[0].content.Add (CreateField ("[spacer]"));
		
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			cf.modules[0].content.Add (CreateField ("directDamage." + damage.codeName, "false"));
		}
		
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			cf.modules[0].content.Add (CreateField ("smallDamage." + damage.codeName, "false"));
		}
		
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			cf.modules[0].content.Add (CreateField ("largeDamage." + damage.codeName, "false"));
		}
		
		// Create ranged module
		cf.modules.Add (new ModuleInfo ("ranged"));
		
		cf.modules[1].content.Add (CreateField ("enabled", "true"));
		cf.modules[1].content.Add (CreateField ("name", "Ranged Attack"));
		
		cf.modules[1].content.Add (CreateField ("fixedStrength", "true"));
		cf.modules[1].content.Add (CreateField ("strengthModifier", "0"));
		
		cf.modules[1].content.Add (CreateField ("projectileSpeed", "low"));
		cf.modules[1].content.Add (CreateField ("projectileAmount", "1"));
		
		cf.modules[1].content.Add (CreateField ("targetable.submerged", "false"));
		cf.modules[1].content.Add (CreateField ("targetable.water", "true"));
		cf.modules[1].content.Add (CreateField ("targetable.land", "true"));
		cf.modules[1].content.Add (CreateField ("targetable.air", "true"));
		
		cf.modules[1].content.Add (CreateField ("[2]"));
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			cf.modules[1].content.Add (CreateField ("immunity." + damage.codeName, "false"));
		}
		
		cf.modules[1].content.Add (CreateField ("enableDirectDamage", "true"));
		cf.modules[1].content.Add (CreateField ("enableSmallblastDamage", "false"));
		cf.modules[1].content.Add (CreateField ("enableLargeblastDamage", "false"));
		
		cf.modules[1].content.Add (CreateField ("[1]"));
		cf.modules[1].content.Add (CreateField ("[spacer]"));
		
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			cf.modules[1].content.Add (CreateField ("directDamage." + damage.codeName, "false"));
		}
		
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			cf.modules[1].content.Add (CreateField ("smallDamage." + damage.codeName, "false"));
		}
		
		foreach (DamageType damage in DamageTypes.damageTypes)
		{
			cf.modules[1].content.Add (CreateField ("largeDamage." + damage.codeName, "false"));
		}
		
		// Write to file
		WriteConfigToFile (cf, path);
	}
	
	private void WriteConfigToFile (ConfigFile config, string path)
	{
		File.WriteAllText (path, "CARD" + "\n" + "{" + "\n");
		WriteModule (config.main, path, "\t");
		
		foreach (ModuleInfo module in config.modules)
		{
			File.AppendAllText (path, "\n");
			File.AppendAllText (path, "\tMODULE_" + module.type.ToUpper() + "\n" + "\t{" + "\n");
			
			WriteModule (module, path, "\t\t");
			
			File.AppendAllText (path, "\t}" + "\n");
		}
		
		File.AppendAllText (path, "}" + "\n");
	}
	
	private void WriteModule (ModuleInfo module, string path, string buffer)
	{
		bool firstLine = true;
		
		foreach (ConfigField field in module.content)
		{
			if (!firstLine && field.description.Count > 0)
			{
				File.AppendAllText (path, "\n");
			}
			
			foreach (string desc in field.description)
			{
				File.AppendAllText (path, buffer + desc + "\n");
			}
			
			if (field.key.StartsWith("[") && field.key.EndsWith("]"))
			{
				if (field.key.Contains("spacer"))
				{
					File.AppendAllText (path, "\n");
				}
			}
			else
			{
				File.AppendAllText (path, buffer + field.key + " = " + field.value + "\n");
			}
			
			firstLine = false;
		}
	}
}
