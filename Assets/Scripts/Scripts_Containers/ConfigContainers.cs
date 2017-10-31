using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Deployment {Submerged, Water, Land, Air};
public enum ProjectileSpeed {Low, High, Beam, Teleport};
public enum Strength {Low, High, Extreme};
public enum CardType {Character, Weapon, Armour, Vehicle};

public class ConfigContainers : MonoBehaviour
{
	public static Dictionary<string, int> deployment = new Dictionary<string, int>();
	public static Dictionary<string, int> projectileSpeed = new Dictionary<string, int>();
	public static Dictionary<string, int> strength = new Dictionary<string, int>();
	public static Dictionary<string, int> cardType = new Dictionary<string, int>();

	void Awake ()
	{
		DamageTypes.damageTypes.Add (new DamageType ("blunt", "Blunt Impact"));
		DamageTypes.damageTypes.Add (new DamageType ("sharp", "Sharp Impact"));
		DamageTypes.damageTypes.Add (new DamageType ("sharpenergy", "Energised Sharp Impact"));
		DamageTypes.damageTypes.Add (new DamageType ("bullets", "Bullets"));
		DamageTypes.damageTypes.Add (new DamageType ("bulletsAP", "Armour Penetrating Bullets"));
		DamageTypes.damageTypes.Add (new DamageType ("laser", "Laser"));
		DamageTypes.damageTypes.Add (new DamageType ("shockwave", "Shockwave"));
		DamageTypes.damageTypes.Add (new DamageType ("plasma", "Plasma"));
		DamageTypes.damageTypes.Add (new DamageType ("fire", "Fire"));
		DamageTypes.damageTypes.Add (new DamageType ("extremeHeat", "Extreme Heat"));
		DamageTypes.damageTypes.Add (new DamageType ("frost", "Frost"));
		DamageTypes.damageTypes.Add (new DamageType ("elecricity", "Electricity"));
		DamageTypes.damageTypes.Add (new DamageType ("radiation", "Radiation Poisoning"));
		DamageTypes.damageTypes.Add (new DamageType ("infection", "Viral Infection"));
		DamageTypes.damageTypes.Add (new DamageType ("poison", "Poisoning"));
		DamageTypes.damageTypes.Add (new DamageType ("acid", "Acid"));
		DamageTypes.damageTypes.Add (new DamageType ("suffocation", "Suffocation"));
		DamageTypes.damageTypes.Add (new DamageType ("falling", "Falling From a Great Height"));
		DamageTypes.damageTypes.Add (new DamageType ("petrify", "Petrification"));
		DamageTypes.damageTypes.Add (new DamageType ("antimatter", "Antimatter"));
		DamageTypes.damageTypes.Add (new DamageType ("mental", "Mental Discombobulation"));
		DamageTypes.damageTypes.Add (new DamageType ("obliteration", "Total Annihilation"));

		deployment.Add ("submerged", 0);
		deployment.Add ("water", 1);
		deployment.Add ("land", 2);
		deployment.Add ("air", 3);

		projectileSpeed.Add ("low", 0);
		projectileSpeed.Add ("high", 1);
		projectileSpeed.Add ("beam", 2);
		projectileSpeed.Add ("teleport", 3);

		strength.Add ("low", 0);
		strength.Add ("high", 1);
		strength.Add ("extreme", 2);

		cardType.Add ("character", 0);
		cardType.Add ("weapon", 1);
		cardType.Add ("armour", 2);
		cardType.Add ("vehicle", 3);
	}
}

public static class DamageTypes
{
	public static List<DamageType> damageTypes = new List<DamageType>();
}

public class DamageType
{
	public string codeName = "";
	public string neatName = "";
	
	public DamageType (string codeName, string neatName)
	{
		this.codeName = codeName;
		this.neatName = neatName;
	}
}

[System.Serializable]
public class ConfigFile
{
	public ModuleInfo main = new ModuleInfo ("main");
	public List<ModuleInfo> modules = new List<ModuleInfo>();
	public bool succes = true;
	
	public ConfigFile ()
	{}
}

[System.Serializable]
public class ModuleInfo
{
	public string type;
	public List<ConfigField> content = new List<ConfigField>();
	
	public ModuleInfo (string type)
	{
		this.type = type;
	}
}

[System.Serializable]
public class ConfigField
{
	public List<string> description = new List<string>();
	public string key = "";
	public string value = "";

	public ConfigField (string key, string value)
	{
		this.key = key;
		this.value = value;
	}

	public ConfigField ()
	{}
}

[System.Serializable]
public class CardContent
{
	public string name;
	public string usage;

	public string description;
	public int type;
	
	public int strength = -1;
	public List<int> deployment = new List<int>();
	public List<string> immunities = new List<string>();

	public bool prioritiseRanged;
	public List<SectionMelee> melee = new List<SectionMelee>();
	public List<SectionRanged> ranged = new List<SectionRanged>();
	
	public CardContent ()
	{}

	public CardContent (string name, string description)
	{
		this.name = name;
		this.description = description;
	}
}

[System.Serializable]
public class SectionMelee
{
	public string name;
	public bool meleeEnabled = false;

	public bool fixedStrength;
	public int strengthModifier;
	
	public bool damageDirectEnabled;
	public List<string> damageTypesDirect = new List<string>();
	public bool damageSmallEnabled;
	public List<string> damageTypesSmall = new List<string>();
	public bool damageLargeEnabled;
	public List<string> damageTypesLarge = new List<string>();
	
	public SectionMelee()
	{}
}

[System.Serializable]
public class SectionRanged
{
	public string name;
	public bool rangedEnabled = false;

	public bool fixedStrength;
	public int strengthModifier;
	
	public int speed = -1;
	public int projectileAmount = -1;
	public List<int> targetable = new List<int>();
	public List<string> projectileImmunities = new List<string>();
	
	public bool damageDirectEnabled;
	public List<string> damageTypesDirect = new List<string>();
	public bool damageSmallEnabled;
	public List<string> damageTypesSmall = new List<string>();
	public bool damageLargeEnabled;
	public List<string> damageTypesLarge = new List<string>();
	
	public SectionRanged()
	{}
}
