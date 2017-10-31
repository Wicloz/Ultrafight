using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BattleManager
{
	public List<FightData> roundFightData = new List<FightData> ();
	public List<FightData> allFightData = new List<FightData> ();

	public void SimulateAttack (UserInfo attacker, UserInfo defender)
	{
		List<FightData> equipmentData =  new List<FightData>();
		List<FightData> characterData = new List<FightData>();
		
		foreach (CardContent equipment in attacker.equipment)
		{
			equipmentData.AddRange (PerformAttackCard (equipment, attacker, defender));
		}

		characterData.AddRange (PerformAttackCard (attacker.character, attacker, defender));
		
		// Decide which outcome
		List<FightData> fightData = new List<FightData> (equipmentData);
		fightData.AddRange (characterData);
		FightData selected = new FightData (attacker, defender);
		selected.weaponName = "No Equipment!";

		bool succes = false;
		int topScore = 0;

		foreach (FightData data in fightData)
		{
			allFightData.Add (data);

			if (data.win)
			{
				succes = true;
				if (data.resolveCount > topScore)
				{
					topScore = data.resolveCount;
				}
			}
		}
		if (!succes)
		{
			foreach (FightData data in fightData)
			{
				if (data.resolveCount > topScore)
				{
					topScore = data.resolveCount;
				}
			}
		}
		foreach (FightData data in fightData)
		{
			if (succes)
			{
				if (data.resolveCount == topScore && data.win)
				{
					selected = data;
					break;
				}
			}
			else if (topScore != 0)
			{
				if (data.resolveCount == topScore)
				{
					selected = data;
					break;
				}
			}
			else
			{
				if (data.weaponName != "No Weapon!")
				{
					selected = data;
					break;
				}
			}
		}

		roundFightData.Add (selected);
	}

	private List<FightData> PerformAttackCard (CardContent equipment, UserInfo attacker, UserInfo defender)
	{
		FightData meleeData = new FightData (attacker, defender);
		FightData rangedData = new FightData (attacker, defender);

		if (equipment.melee.Count != 0)
		{
			SectionMelee melee = equipment.melee [Random.Range (0, equipment.melee.Count - 1)];
			meleeData = PerformAttackMelee (melee, attacker, defender);

			if (attacker.deployment != defender.deployment && melee.meleeEnabled)
			{
				meleeData.weaponName = "No Hit!";
			}
		}

		if (equipment.ranged.Count != 0)
		{
			SectionRanged ranged = equipment.ranged [Random.Range (0, equipment.ranged.Count - 1)];
			rangedData = PerformAttackRanged (ranged, attacker, defender);

			if (!ranged.targetable.Contains((int)defender.deployment) && ranged.rangedEnabled)
			{
				rangedData.weaponName = "No Hit!";
			}
		}

		List<FightData> data = new List<FightData> ();
		data.Add (meleeData);
		data.Add (rangedData);
		
		return data;
	}

	private string ResolveReactionToDamage (string damage, string succes, UserInfo defender)
	{
		if (succes == "true")
		{
			if (!defender.immunities.Contains(damage))
			{
				return "hit";
			}
			else
			{
				return "immune";
			}
		}
		
		else
		{
			return succes;
		}
	}

	private FightData PerformAttackMelee (SectionMelee weapon, UserInfo attacker, UserInfo defender)
	{
		FightData data = new FightData (attacker, defender);

		string directSucces = "true";
		string smallSucces = "true";
		string largeSucces = "true";

		// Melee Attack
		if (weapon.meleeEnabled && attacker.deployment == defender.deployment)
		{
			data.weaponName = weapon.name;
			
			if (weapon.damageDirectEnabled)
			{
				foreach (string damagetype in weapon.damageTypesDirect)
				{
					ResolveData resolve = new ResolveData();
					resolve.damageType = damagetype;
					resolve.reaction = ResolveReactionToDamage (damagetype, directSucces, defender);
					data.resolveDirect.Add(resolve);
				}
			}
			
			if (weapon.damageSmallEnabled)
			{
				foreach (string damagetype in weapon.damageTypesSmall)
				{
					ResolveData resolve = new ResolveData();
					resolve.damageType = damagetype;
					resolve.reaction = ResolveReactionToDamage (damagetype, smallSucces, defender);
					data.resolveSmall.Add(resolve);
				}
			}
			
			if (weapon.damageLargeEnabled)
			{
				foreach (string damagetype in weapon.damageTypesLarge)
				{
					ResolveData resolve = new ResolveData();
					resolve.damageType = damagetype;
					resolve.reaction = ResolveReactionToDamage (damagetype, largeSucces, defender);
					data.resolveLarge.Add(resolve);
				}
			}

			data.CheckForWin();
		}

		return data;
	}

	private FightData PerformAttackRanged (SectionRanged weapon, UserInfo attacker, UserInfo defender)
	{
		FightData data = new FightData (attacker, defender);

		string directSucces = "true";
		string smallSucces = "true";
		string largeSucces = "true";

		// Ranged Attack
		if (weapon.rangedEnabled && weapon.targetable.Contains((int)defender.deployment))
		{
			data.weaponName = weapon.name;

			// Check for shot down
			if (weapon.rangedEnabled)
			{
				foreach (CardContent defensive in defender.equipment)
				{
					foreach (SectionRanged defRan in defensive.ranged)
					{
						if (weapon.speed == (int)ProjectileSpeed.Low && defensive.type == (int)CardType.Weapon && defRan.rangedEnabled && defRan.targetable.Contains((int)Deployment.Air))
						{
							if (defRan.damageDirectEnabled)
							{
								foreach (string damagetype in defRan.damageTypesDirect)
								{
									if (!defRan.projectileImmunities.Contains(damagetype))
									{
										directSucces = "shot";
										smallSucces = "shot";
									}
								}
							}
							
							if (defRan.damageSmallEnabled)
							{
								foreach (string damagetype in defRan.damageTypesSmall)
								{
									if (!defRan.projectileImmunities.Contains(damagetype))
									{
										directSucces = "shot";
										smallSucces = "shot";
									}
								}
							}
							
							if (defRan.damageLargeEnabled)
							{
								foreach (string damagetype in defRan.damageTypesLarge)
								{
									if (!defRan.projectileImmunities.Contains(damagetype))
									{
										directSucces = "shot";
										smallSucces = "shot";
									}
								}
							}
						}
					}
				}
			} // End shot down

			if (weapon.damageDirectEnabled)
			{
				foreach (string damagetype in weapon.damageTypesDirect)
				{
					ResolveData resolve = new ResolveData();
					resolve.damageType = damagetype;
					resolve.reaction = ResolveReactionToDamage (damagetype, directSucces, defender);
					data.resolveDirect.Add(resolve);
				}
			}
			
			if (weapon.damageSmallEnabled)
			{
				foreach (string damagetype in weapon.damageTypesSmall)
				{
					ResolveData resolve = new ResolveData();
					resolve.damageType = damagetype;
					resolve.reaction = ResolveReactionToDamage (damagetype, smallSucces, defender);
					data.resolveSmall.Add(resolve);
				}
			}
			
			if (weapon.damageLargeEnabled)
			{
				foreach (string damagetype in weapon.damageTypesLarge)
				{
					ResolveData resolve = new ResolveData();
					resolve.damageType = damagetype;
					resolve.reaction = ResolveReactionToDamage (damagetype, largeSucces, defender);
					data.resolveLarge.Add(resolve);
				}
			}

			data.CheckForWin();
		}

		return data;
	}
}

[System.Serializable]
public class FightData
{
	public string attackerName;
	public string defenderName;
	
	public string weaponName = "No Weapon!";
	
	public List<ResolveData> resolveDirect = new List<ResolveData>();
	public List<ResolveData> resolveSmall = new List<ResolveData>();
	public List<ResolveData> resolveLarge = new List<ResolveData>();
	
	public bool win = false;

	public int resolveCount
	{
		get
		{
			return resolveDirect.Count + resolveSmall.Count + resolveLarge.Count;
		}
	}

	// Functions
	public FightData ()
	{}
	
	public FightData (UserInfo attacker, UserInfo defender)
	{
		this.defenderName = defender.username;
		this.attackerName = attacker.username;
	}

	public void CheckForWin ()
	{
		List<ResolveData> resolveAll = new List<ResolveData> ();
		resolveAll.AddRange (resolveDirect);
		resolveAll.AddRange (resolveSmall);
		resolveAll.AddRange (resolveLarge);

		foreach (ResolveData data in resolveAll)
		{
			if (data.reaction.ToLower() == "hit")
			{
				win = true;
				break;
			}
		}
	}
}

[System.Serializable]
public class ResolveData
{
	public string damageType;
	public string reaction;
	
	public ResolveData ()
	{}
	
	public ResolveData (string damageType, string reaction)
	{
		this.damageType = damageType;
		this.reaction = reaction;
	}
}
