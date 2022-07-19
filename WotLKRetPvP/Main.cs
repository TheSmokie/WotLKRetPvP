using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CombatRotation.RotationFramework;
using robotManager.Helpful;
using wManager;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

// Token: 0x02000003 RID: 3
public class Main : ICustomClass
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000008 RID: 8 RVA: 0x00002517 File Offset: 0x00000717
	public float Range
	{
		get
		{
			return 4f;
		}
	}

	// Token: 0x06000009 RID: 9 RVA: 0x00002520 File Offset: 0x00000720
	public void Initialize()
	{
		RetPalaSettings.Load();
		Authentication authentication = new Authentication(RetPalaSettings.CurrentSetting.TransactionId, "34d8c361761f");
		wManagerSetting.CurrentSetting.UseLuaToMove = true;
		RotationFramework.Initialize(RetPalaSettings.CurrentSetting.SlowRotation, RetPalaSettings.CurrentSetting.FrameLock);
		this._isLaunched = true;
		this.RotationSteps.Sort((RotationStep a, RotationStep b) => a.Priority.CompareTo(b.Priority));
		this.Rotation();
	}

	// Token: 0x0600000A RID: 10 RVA: 0x000025A8 File Offset: 0x000007A8
	public void Rotation()
	{
		while (this._isLaunched)
		{
			try
			{
				bool flag = Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && !Main.Me.IsDead;
				if (flag)
				{
					this.UseBuffs();
					bool inFight = Fight.InFight;
					if (inFight)
					{
						bool flag2 = Main.Me.HasAnyBuff(new string[]
						{
							"Polymorph",
							"Psychic Scream",
							"Fear",
							"Intimidating Shout",
							"Cyclone",
							"Blind",
							"Seduction"
						});
						if (flag2)
						{
							RotationFramework.UsePvPTrinket();
						}
						RotationFramework.RunRotation(this.RotationSteps);
					}
				}
			}
			catch (Exception arg)
			{
				Logging.WriteError("RetPala ERROR:" + arg, true);
			}
			Thread.Sleep(25);
		}
	}

	// Token: 0x0600000B RID: 11 RVA: 0x00002690 File Offset: 0x00000890
	private void UseBuffs()
	{
		bool flag = Main.Me.IsMounted && !this.CrusaderAura.Spell.HaveBuff;
		if (flag)
		{
			this.CrusaderAura.Spell.Launch();
		}
		else
		{
			bool flag2 = Main.Me.IsMounted || Main.Me.InCombatFlagOnly || Fight.InFight || Main.Me.HasAnyBuff(new string[]
			{
				"Food",
				"Drink"
			});
			if (!flag2)
			{
				bool flag3 = Main.Me.HealthPercent < 60.0;
				if (flag3)
				{
					RotationCombatUtil.CastSpell(this.HolyLight, Main.Me, false);
				}
				this.Buffs.ForEach(new Action<RotationSpell>(RotationCombatUtil.CastBuff));
				bool flag4 = Main.Me.HaveBuff("Preparation");
				if (flag4)
				{
					RotationFramework.Units.ForEach(delegate(WoWUnit o)
					{
						bool flag5 = o.Guid != Main.Me.Guid && o.IsPlayer();
						if (flag5)
						{
							this.PartyBuffs.ForEach(delegate(RotationSpell b)
							{
								RotationCombatUtil.CastBuff(b, o);
							});
						}
					});
				}
				Thread.Sleep(Usefuls.Latency);
			}
		}
	}

	// Token: 0x0600000C RID: 12 RVA: 0x000027A0 File Offset: 0x000009A0
	private static WoWUnit FindDispelTarget(Func<WoWUnit, bool> predicate)
	{
		return RotationFramework.Units.FirstOrDefault((WoWUnit o) => o.Reaction == Reaction.Friendly && o.IsPlayer() && o.IsAlive && o.Guid != Main.Me.Guid && predicate(o) && o.HasAnyBuff(new string[]
		{
			"Fear",
			"Psychic Scream",
			"Polymorph",
			"Hammer of Justice",
			"Entangling Roots"
		}));
	}

	// Token: 0x0600000D RID: 13 RVA: 0x000027D8 File Offset: 0x000009D8
	private static WoWUnit FindBoPTarget(Func<WoWUnit, bool> predicate)
	{
		return RotationFramework.Units.FirstOrDefault((WoWUnit o) => o.Reaction == Reaction.Friendly && o.IsPlayer() && o.IsAlive && o.Guid != Main.Me.Guid && predicate(o) && o.HasAnyBuff(new string[]
		{
			"Kidney Shot",
			"Blind",
			"Intimidating Shout"
		}) && o.WowClass != WoWClass.Warrior && o.WowClass != WoWClass.Hunter && o.WowClass != WoWClass.Rogue);
	}

	// Token: 0x0600000E RID: 14 RVA: 0x00002810 File Offset: 0x00000A10
	private static WoWUnit FindFearTarget(Func<WoWUnit, bool> predicate)
	{
		return RotationFramework.Units.FirstOrDefault((WoWUnit o) => o.Reaction == Reaction.Hostile && !o.IsPlayer() && o.IsAlive && predicate(o) && o.Name == "Ebon Gargoyle");
	}

	// Token: 0x0600000F RID: 15 RVA: 0x00002848 File Offset: 0x00000A48
	private static bool OnlyTargetAround()
	{
		return RotationFramework.Units.FirstOrDefault((WoWUnit o) => o.IsPlayer() && o.IsAlive && o.Reaction == Reaction.Hostile && o.Guid != RotationFramework.Target.Guid && o.GetDistance < 10f) == null;
	}

	// Token: 0x06000010 RID: 16 RVA: 0x00002886 File Offset: 0x00000A86
	public void Dispose()
	{
		Authentication.Kill();
		this._isLaunched = false;
		RotationFramework.Dispose();
	}

	// Token: 0x06000011 RID: 17 RVA: 0x0000289C File Offset: 0x00000A9C
	public void ShowConfiguration()
	{
		RetPalaSettings.Load();
		RetPalaSettings.CurrentSetting.ToForm();
		RetPalaSettings.CurrentSetting.Save();
	}

	// Token: 0x06000012 RID: 18 RVA: 0x000028BC File Offset: 0x00000ABC
	public Main()
	{
		List<RotationStep> list = new List<RotationStep>();
		list.Add(new RotationStep(new RotationSpell("Lay on Hands", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 0.9f, (RotationAction s, WoWUnit t) => t.HealthPercent < 10.0 && t.ManaPercentage < 5U, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), true, true));
		list.Add(new RotationStep(new RotationSpell("Retribution Aura", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 1f, (RotationAction s, WoWUnit t) => !t.HaveBuff("Retribution Aura"), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Holy Light", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 1.1f, (RotationAction s, WoWUnit t) => t.HealthPercent <= 35.0 && RotationFramework.Target.HasAnyBuff(new string[]
		{
			"Repentance",
			"Hammer of Justice"
		}) && Main.OnlyTargetAround(), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Holy Light", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 1.1f, (RotationAction s, WoWUnit t) => t.HealthPercent <= 35.0 && t.HaveBuff("Divine Shield") && RotationFramework.Target.HealthPercent > 25.0, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(Main.DivineShield, 1.2f, (RotationAction s, WoWUnit t) => t.HealthPercent < 20.0, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), true, false));
		list.Add(new RotationStep(new RotationSpell("Divine Protection", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 1.2f, (RotationAction s, WoWUnit t) => !Main.Me.HasBuff("Forebereance") && Main.DivineShield.GetCooldown() > 0f && t.HealthPercent < 20.0, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), true, false));
		list.Add(new RotationStep(new RotationSpell("Hammer of Wrath", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 1.4f, (RotationAction s, WoWUnit t) => t.HealthPercent < 20.0, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), false, true));
		list.Add(new RotationStep(new RotationSpell("Consecration", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 2f, (RotationAction s, WoWUnit t) => t.HaveBuff("Vanish") && t.GetDistance < 7f, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), true, false));
		list.Add(new RotationStep(new RotationSpell("Hammer of Justice", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 2f, (RotationAction s, WoWUnit t) => t.HealthPercent > Main.Me.HealthPercent && Main.Me.HealthPercent < 40.0, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), true, true));
		list.Add(new RotationStep(new RotationSpell("Hammer of Justice", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 2.1f, (RotationAction s, WoWUnit t) => t.IsCasting(), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), true, true));
		list.Add(new RotationStep(new RotationSpell("Arcane Torrent", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 2.1f, (RotationAction s, WoWUnit t) => t.IsCasting() && t.GetDistance <= 8f, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), true, false));
		list.Add(new RotationStep(new RotationSpell("Seal of Righteousness", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 3.2f, (RotationAction s, WoWUnit t) => !Main.Me.HaveBuff("Seal of Righteousness"), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Judgement of Justice", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 3.3f, (RotationAction s, WoWUnit t) => true, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), false, true));
		list.Add(new RotationStep(new RotationSpell("Divine Storm", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 4f, (RotationAction s, WoWUnit t) => Main.Me.HasTarget && t.GetDistance <= 8f, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), false, false));
		list.Add(new RotationStep(new RotationSpell("Crusader Strike", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 4.1f, (RotationAction s, WoWUnit t) => true, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), false, true));
		list.Add(new RotationStep(new RotationSpell("Exorcism", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 4.2f, (RotationAction s, WoWUnit t) => Main.Me.HasBuff("The Art of War"), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), true, true));
		list.Add(new RotationStep(new RotationSpell("Avenging Wrath", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 4.3f, (RotationAction s, WoWUnit t) => t.HealthPercent > 80.0 && !t.HasBuff("Forbearance") && RotationFramework.Target.HealthPercent > 90.0 && RotationFramework.Target.GetDistance < 7f, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Hand of Freedom", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 5f, (RotationAction s, WoWUnit t) => t.HasAnyBuff(new string[]
		{
			"Shadowfury",
			"Kidney Shot",
			"Hammer of Justice",
			"Deep Freeze",
			"Frost Nova",
			"Entangling Roots",
			"Hamstring",
			"Crippling Poison"
		}) && RotationFramework.Target.GetDistance > 8f, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Cleanse", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 6f, (RotationAction s, WoWUnit t) => (t.HealthPercent > 70.0 || (RotationFramework.Target.GetDistance >= 10f && Main.OnlyTargetAround())) && (t.HasDebuffType("Magic") || t.HasBuff("Crippling Poison")), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Mana Tap", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 7f, (RotationAction s, WoWUnit t) => t.HasMana(), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.BotTarget), false, true));
		list.Add(new RotationStep(new RotationSpell("Sacred Shield", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 7.1f, (RotationAction s, WoWUnit t) => !t.HasBuff("Sacred Shield"), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Flash of Light", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 8f, (RotationAction s, WoWUnit t) => t.HasBuff("The Art of War"), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Cleanse", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 8f, (RotationAction s, WoWUnit t) => t.HasAnyBuff(new string[]
		{
			"Viper Sting",
			"Slow",
			"Frost Nova",
			"Freeze",
			"Cone of Cold",
			"Frostbolt",
			"Entangling Roots",
			"Crippling Poison"
		}), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		list.Add(new RotationStep(new RotationSpell("Repentance", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 10f, (RotationAction s, WoWUnit t) => true, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindPlayerCastingOnMe), true, true));
		list.Add(new RotationStep(new RotationSpell("Cleanse", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 10.1f, (RotationAction s, WoWUnit t) => true, new Func<Func<WoWUnit, bool>, WoWUnit>(Main.FindDispelTarget), false, true));
		list.Add(new RotationStep(new RotationSpell("Hand of Sacrifice", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 10.2f, delegate(RotationAction s, WoWUnit t)
		{
			bool result;
			if (t != Main.Me)
			{
				result = RotationFramework.Units.Any((WoWUnit o) => o.IsPlayer() && o.Reaction == Reaction.Hostile && o.CastingSpell(new string[]
				{
					"Polymorph"
				}) && o.Target == Main.Me.Guid);
			}
			else
			{
				result = false;
			}
			return result;
		}, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindFriend), false, true));
		list.Add(new RotationStep(new RotationSpell("Divine Sacrifice", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 10.2f, delegate(RotationAction s, WoWUnit t)
		{
			bool result;
			if (t != Main.Me)
			{
				result = RotationFramework.Units.Any((WoWUnit o) => o.IsPlayer() && o.Reaction == Reaction.Hostile && o.CastingSpell(new string[]
				{
					"Polymorph"
				}) && o.Target == Main.Me.Guid);
			}
			else
			{
				result = false;
			}
			return result;
		}, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindFriend), false, true));
		list.Add(new RotationStep(new RotationSpell("Hand of Protection", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 10.3f, (RotationAction s, WoWUnit t) => t != Main.Me && t.HealthPercent < 50.0 && t.WowClass != WoWClass.Warrior && t.WowClass != WoWClass.Rogue && t.WowClass != WoWClass.Hunter && RotationFramework.Units.Any((WoWUnit o) => o.Reaction == Reaction.Hostile && o.IsAlive && o.Position.DistanceTo(t.Position) <= 8f), new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindFriend), false, true));
		list.Add(new RotationStep(new RotationSpell("Hand of Protection", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 10.4f, (RotationAction s, WoWUnit t) => true, new Func<Func<WoWUnit, bool>, WoWUnit>(Main.FindBoPTarget), false, true));
		list.Add(new RotationStep(new RotationSpell("Turn Evil", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 10.4f, (RotationAction s, WoWUnit t) => true, new Func<Func<WoWUnit, bool>, WoWUnit>(Main.FindFearTarget), false, true));
		list.Add(new RotationStep(new RotationSpell("Divine Plea", null, false, false, RotationSpell.VerificationType.CAST_RESULT), 10.5f, (RotationAction s, WoWUnit t) => t.ManaPercentage <= 20U, new Func<Func<WoWUnit, bool>, WoWUnit>(RotationCombatUtil.FindMe), false, true));
		this.RotationSteps = list;
		base..ctor();
	}

	// Token: 0x04000007 RID: 7
	private bool _isLaunched;

	// Token: 0x04000008 RID: 8
	private static WoWLocalPlayer Me = ObjectManager.Me;

	// Token: 0x04000009 RID: 9
	private List<RotationSpell> PartyBuffs = new List<RotationSpell>
	{
		new RotationSpell("Blessing of Kings", null, false, false, RotationSpell.VerificationType.CAST_RESULT)
	};

	// Token: 0x0400000A RID: 10
	private List<RotationSpell> Buffs = new List<RotationSpell>
	{
		new RotationSpell("Blessing of Might", null, false, false, RotationSpell.VerificationType.CAST_RESULT),
		new RotationSpell("Righteous Fury", null, false, false, RotationSpell.VerificationType.CAST_RESULT),
		new RotationSpell("Seal of Righteousness", null, false, false, RotationSpell.VerificationType.CAST_RESULT)
	};

	// Token: 0x0400000B RID: 11
	private RotationSpell CrusaderAura = new RotationSpell("Crusader Aura", null, false, false, RotationSpell.VerificationType.CAST_RESULT);

	// Token: 0x0400000C RID: 12
	private RotationSpell HolyLight = new RotationSpell("Holy Light", null, false, false, RotationSpell.VerificationType.CAST_RESULT);

	// Token: 0x0400000D RID: 13
	private static RotationSpell DivineShield = new RotationSpell("Divine Shield", null, false, false, RotationSpell.VerificationType.CAST_RESULT);

	// Token: 0x0400000E RID: 14
	private List<RotationStep> RotationSteps;
}
