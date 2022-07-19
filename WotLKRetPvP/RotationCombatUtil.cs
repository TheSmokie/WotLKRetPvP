using System;
using System.Collections.Generic;
using System.Linq;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x02000007 RID: 7
	public class RotationCombatUtil
	{
		// Token: 0x0600002A RID: 42 RVA: 0x00003704 File Offset: 0x00001904
		public static WoWUnit FindFriend(Func<WoWUnit, bool> predicate)
		{
			bool flag = RotationFramework.Me.HealthPercent < 60.0;
			WoWUnit result;
			if (flag)
			{
				result = RotationFramework.Me;
			}
			else
			{
				result = (from u in RotationFramework.Units
				where u.IsAlive && u.Reaction == Reaction.Friendly && predicate(u) && !TraceLine.TraceLineGo(u.Position)
				orderby u.HealthPercent
				select u).FirstOrDefault<WoWUnit>();
			}
			return result;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003784 File Offset: 0x00001984
		public static WoWUnit FindEnemy(Func<WoWUnit, bool> predicate)
		{
			return RotationCombatUtil.FindEnemy(RotationFramework.Units, predicate);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000037A4 File Offset: 0x000019A4
		public static WoWUnit FindEnemyPlayer(Func<WoWUnit, bool> predicate)
		{
			return RotationCombatUtil.FindEnemy(RotationFramework.Units, (WoWUnit u) => predicate(u) && u.IsPlayer());
		}

		// Token: 0x0600002D RID: 45 RVA: 0x000037DC File Offset: 0x000019DC
		public static WoWUnit FindEnemyCasting(Func<WoWUnit, bool> predicate)
		{
			return RotationCombatUtil.FindEnemyCasting(RotationFramework.Units, predicate);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000037FC File Offset: 0x000019FC
		public static WoWUnit FindPlayerCasting(Func<WoWUnit, bool> predicate)
		{
			return RotationCombatUtil.FindEnemyCasting(RotationFramework.Units, (WoWUnit u) => predicate(u) && u.IsPlayer());
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003834 File Offset: 0x00001A34
		public static WoWUnit FindEnemyCastingOnMe(Func<WoWUnit, bool> predicate)
		{
			return RotationCombatUtil.FindEnemyCastingOnMe(RotationFramework.Units, predicate);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003854 File Offset: 0x00001A54
		public static WoWUnit FindPlayerCastingOnMe(Func<WoWUnit, bool> predicate)
		{
			return RotationCombatUtil.FindEnemyCastingOnMe(RotationFramework.Units, (WoWUnit u) => predicate(u) && u.IsPlayer());
		}

		// Token: 0x06000031 RID: 49 RVA: 0x0000388C File Offset: 0x00001A8C
		private static WoWUnit FindEnemyCasting(IEnumerable<WoWUnit> units, Func<WoWUnit, bool> predicate)
		{
			return RotationCombatUtil.FindEnemy(units, (WoWUnit u) => predicate(u) && u.WowClass != WoWClass.Hunter && u.WowClass != WoWClass.Warrior && u.WowClass != WoWClass.Rogue && u.IsCasting());
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000038C0 File Offset: 0x00001AC0
		private static WoWUnit FindEnemyCastingOnMe(IEnumerable<WoWUnit> units, Func<WoWUnit, bool> predicate)
		{
			return RotationCombatUtil.FindEnemyCasting(units, (WoWUnit u) => predicate(u) && u.Target == RotationFramework.Me.Guid);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000038F4 File Offset: 0x00001AF4
		private static WoWUnit FindEnemy(IEnumerable<WoWUnit> units, Func<WoWUnit, bool> rangePredicate)
		{
			return (from u in units
			where rangePredicate(u) && u.IsAlive && u.Reaction < Reaction.Neutral && !TraceLine.TraceLineGo(RotationFramework.Me.Position, u.Position, CGWorldFrameHitFlags.HitTestWMO)
			orderby u.GetDistance
			select u).FirstOrDefault<WoWUnit>();
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00003950 File Offset: 0x00001B50
		public static WoWUnit BotTarget(Func<WoWUnit, bool> predicate)
		{
			WoWUnit target = RotationFramework.Target;
			return (!TraceLine.TraceLineGo(target.Position) && predicate(target)) ? target : null;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00003984 File Offset: 0x00001B84
		public static WoWUnit FindPet(Func<WoWUnit, bool> predicate)
		{
			WoWUnit pet = RotationFramework.Pet;
			return (!TraceLine.TraceLineGo(pet.Position) && predicate(pet)) ? pet : null;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x000039B8 File Offset: 0x00001BB8
		public static WoWUnit FindMe(Func<WoWUnit, bool> predicate)
		{
			return RotationFramework.Me;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000039D0 File Offset: 0x00001BD0
		public static void CastBuff(RotationSpell buff, WoWUnit target)
		{
			bool flag = buff.Spell.Name == "Power Word: Fortitude" && target.HasBuff("Prayer of Fortitude");
			if (!flag)
			{
				bool flag2 = buff.Spell.Name == "Mark of the Wild" && target.HasBuff("Gift of the Wild");
				if (!flag2)
				{
					bool flag3 = buff.Spell.Name == "Divine Spirit" && target.HasBuff("Prayer of Spirit");
					if (!flag3)
					{
						bool flag4 = buff.Spell.Name == "Blessing of Kings" && target.HasBuff("Greater Blessing of Kings");
						if (!flag4)
						{
							bool flag5 = buff.Spell.Name == "Blessing of Might" && target.HasBuff("Greater Blessing of Might");
							if (!flag5)
							{
								bool flag6 = buff.IsKnown() && buff.CanCast() && !target.HasBuff(buff.Spell.Name);
								if (flag6)
								{
									RotationCombatUtil.CastSpell(buff, target, false);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00003AFD File Offset: 0x00001CFD
		public static void CastBuff(RotationSpell buff)
		{
			RotationCombatUtil.CastBuff(buff, RotationFramework.Me);
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00003B0C File Offset: 0x00001D0C
		public static bool IsAutoRepeating(string name)
		{
			return Lua.LuaDoString<bool>("return IsAutoRepeatSpell(\"" + name + "\")", "");
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003B38 File Offset: 0x00001D38
		public static bool IsAutoAttacking()
		{
			return Lua.LuaDoString<bool>("return IsCurrentSpell('Attack') == 1 or IsCurrentSpell('Attack') == true", "");
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003B5C File Offset: 0x00001D5C
		public static bool CastSpell(RotationSpell spell, WoWUnit unit, bool force = false)
		{
			bool flag = RotationSpellVerifier.IsWaitingForVerification() && !force;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = spell.Spell.Name == "Shoot" && RotationCombatUtil.IsAutoRepeating("Shoot");
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = unit != null && spell.IsKnown() && spell.CanCast();
					if (flag3)
					{
						bool flag4 = spell.Spell.CastTime > 0f;
						if (flag4)
						{
							bool flag5 = spell.Verification != RotationSpell.VerificationType.NONE;
							if (flag5)
							{
								RotationSpellVerifier.QueueVerification(spell.Spell.Name, unit, spell.Verification);
							}
							RotationFramework.ForceIsCast = true;
						}
						bool flag6 = RotationCombatUtil.AreaSpells.Contains(spell.Spell.Name);
						if (flag6)
						{
							SpellManager.CastSpellByIDAndPosition(spell.Spell.Id, unit.Position);
						}
						else
						{
							bool flag7 = unit.Guid != RotationFramework.Me.Guid && spell.NeedsFacing;
							if (flag7)
							{
								MovementManager.Face(unit);
							}
							RotationCombatUtil.ExecuteActionOnUnit<object>(unit, delegate(string luaUnitId)
							{
								RotationLogger.Fight(string.Format("Casting {0} ({1} on {2} with guid {3}", new object[]
								{
									spell.FullName(),
									spell.Spell.Name,
									luaUnitId,
									unit.Guid
								}));
								Lua.LuaDoString(string.Concat(new string[]
								{
									"\r\n\t\t\t\t\t\tif ",
									force.ToString().ToLower(),
									" then SpellStopCasting() end\r\n                        CastSpellByName(\"",
									spell.FullName(),
									"\", \"",
									luaUnitId,
									"\");\r\n\t\t\t\t\t\t--CombatTextSetActiveUnit(\"",
									luaUnitId,
									"\");\r\n\t\t\t\t\t\tFocusUnit(\"",
									luaUnitId,
									"\");\r\n\t\t\t\t\t\t"
								}), false);
								return null;
							});
						}
						result = true;
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003D10 File Offset: 0x00001F10
		public static T ExecuteActionOnUnit<T>(WoWUnit unit, Func<string, T> action)
		{
			return RotationCombatUtil.ExecuteActionOnTarget<T>(unit.Guid, action);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003D30 File Offset: 0x00001F30
		public static T ExecuteActionOnTarget<T>(ulong target, Func<string, T> action)
		{
			bool flag = target == RotationFramework.Me.Guid;
			T result;
			if (flag)
			{
				result = action("player");
			}
			else
			{
				bool flag2 = target == RotationFramework.Target.Guid;
				if (flag2)
				{
					result = action("target");
				}
				else
				{
					object locker = RotationCombatUtil._locker;
					lock (locker)
					{
						RotationCombatUtil.SetMouseoverGuid(target);
						result = action("mouseover");
					}
				}
			}
			return result;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00003DC4 File Offset: 0x00001FC4
		private static void SetMouseoverUnit(WoWUnit unit)
		{
			RotationCombatUtil.SetMouseoverGuid(unit.Guid);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00003DD4 File Offset: 0x00001FD4
		public static T ExecuteActionOnFocus<T>(ulong target, Func<string, T> action)
		{
			object focusLocker = RotationCombatUtil._focusLocker;
			T result;
			lock (focusLocker)
			{
				RotationCombatUtil.SetFocusGuid(target);
				result = action("focus");
			}
			return result;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00003E28 File Offset: 0x00002028
		public static void SetFocusGuid(ulong guid)
		{
			RotationFramework.Me.FocusGuid = guid;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00003E44 File Offset: 0x00002044
		private static void SetMouseoverGuid(ulong guid)
		{
			RotationFramework.Me.MouseOverGuid = guid;
		}

		// Token: 0x04000013 RID: 19
		private static object _locker = new object();

		// Token: 0x04000014 RID: 20
		private static object _focusLocker = new object();

		// Token: 0x04000015 RID: 21
		private static List<string> AreaSpells = new List<string>
		{
			"Mass Dispel",
			"Blizzard",
			"Rain of Fire",
			"Freeze",
			"Volley",
			"Flare",
			"Hurricane",
			"Flamestrike",
			"Distract"
		};
	}
}
