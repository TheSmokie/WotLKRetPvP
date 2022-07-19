using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x02000009 RID: 9
	public static class Extensions
	{
		// Token: 0x06000049 RID: 73 RVA: 0x00004224 File Offset: 0x00002424
		public static bool HasDebuffType(this WoWUnit unit, string type)
		{
			return RotationCombatUtil.ExecuteActionOnUnit<bool>(unit, delegate(string luaUnitId)
			{
				string command = string.Concat(new string[]
				{
					"\r\n                local hasDebuff = false;\r\n                for i=1,40 do\r\n                    local name, rank, iconTexture, count, debuffType, duration, timeLeft = UnitDebuff(\"",
					luaUnitId,
					"\", i);\r\n                    if debuffType == \"",
					type,
					"\" then\r\n                        hasDebuff = true\r\n                        break;\r\n                    end\r\n                end\r\n                return hasDebuff;"
				});
				return Lua.LuaDoString<bool>(command, "");
			});
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00004258 File Offset: 0x00002458
		public static string AsString(this IEnumerable<string> list)
		{
			return list.Aggregate((string s1, string s2) => s1 + ", " + s2);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00004290 File Offset: 0x00002490
		public static bool IsCasting(this WoWUnit unit)
		{
			return RotationCombatUtil.ExecuteActionOnUnit<bool>(unit, delegate(string luaUnitId)
			{
				string command = string.Concat(new string[]
				{
					"return (UnitCastingInfo(\"",
					luaUnitId,
					"\") ~= nil or UnitChannelInfo(\"",
					luaUnitId,
					"\") ~= nil)"
				});
				return Lua.LuaDoString<bool>(command, "");
			});
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000042C8 File Offset: 0x000024C8
		public static bool IsCreatureType(this WoWUnit unit, string creatureType)
		{
			bool flag = Extensions._creatureTypeCache.ContainsKey(unit.Entry);
			bool result;
			if (flag)
			{
				result = (Extensions._creatureTypeCache[unit.Entry] == creatureType);
			}
			else
			{
				string text = RotationCombatUtil.ExecuteActionOnUnit<string>(unit, delegate(string luaUnitId)
				{
					string command = "return UnitCreatureType(\"" + luaUnitId + "\")";
					return Lua.LuaDoString<string>(command, "");
				});
				Extensions._creatureTypeCache.Add(unit.Entry, text);
				result = (text == creatureType);
			}
			return result;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004348 File Offset: 0x00002548
		public static float CastingTimeLeft(this WoWUnit unit, string name)
		{
			return RotationCombatUtil.ExecuteActionOnUnit<float>(unit, delegate(string luaUnitId)
			{
				string command = string.Concat(new string[]
				{
					"\r\n            local castingTimeLeft = 0;\r\n    \r\n            local name, rank, displayName, icon, startTime, endTime, isTradeSkill = UnitCastingInfo(\"",
					luaUnitId,
					"\")\r\n            if name == \"",
					name,
					"\" then\r\n                castingTimeLeft = endTime - GetTime()\r\n            end\r\n            return castingTimeLeft;"
				});
				return Lua.LuaDoString<float>(command, "");
			});
		}

		// Token: 0x0600004E RID: 78 RVA: 0x0000437C File Offset: 0x0000257C
		public static bool CastingTimeLessThan(this WoWUnit unit, string name, float lessThan)
		{
			float num = unit.CastingTimeLeft(name);
			return num > 0f && num < lessThan;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000043B0 File Offset: 0x000025B0
		public static bool CastingSpell(this WoWUnit unit, params string[] names)
		{
			return RotationCombatUtil.ExecuteActionOnUnit<bool>(unit, delegate(string luaUnitId)
			{
				string command = string.Concat(new string[]
				{
					"\r\n\t            local isCastingSpell = false;\r\n\t    \r\n\t            local name = UnitCastingInfo(\"",
					luaUnitId,
					"\")\r\n\t            if ",
					Extensions.LuaOrCondition(names, "name"),
					" then\r\n\t                isCastingSpell = true\r\n\t            end\r\n\t            return isCastingSpell;"
				});
				return Lua.LuaDoString<bool>(command, "");
			});
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000043E4 File Offset: 0x000025E4
		public static bool HasMana(this WoWUnit unit)
		{
			return RotationCombatUtil.ExecuteActionOnUnit<bool>(unit, delegate(string luaUnitId)
			{
				string command = string.Concat(new string[]
				{
					"return (UnitPowerType(\"",
					luaUnitId,
					"\") == 0 and UnitMana(\"",
					luaUnitId,
					"\") > 1)"
				});
				return Lua.LuaDoString<bool>(command, "");
			});
		}

		// Token: 0x06000051 RID: 81 RVA: 0x0000441C File Offset: 0x0000261C
		public static string GetName(this Aura aura)
		{
			bool flag = Extensions._invisibleAuras.Contains(aura.SpellId);
			string result;
			if (flag)
			{
				result = "-";
			}
			else
			{
				Spell spell = new Spell(aura.SpellId);
				bool flag2 = spell.Name == "-";
				if (flag2)
				{
					Extensions._invisibleAuras.Add(aura.SpellId);
				}
				result = spell.Name;
			}
			return result;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00004484 File Offset: 0x00002684
		public static bool HasBuff(this WoWUnit unit, string name)
		{
			return unit.HaveBuff(name);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000044A0 File Offset: 0x000026A0
		public static bool HasAnyBuff(this WoWUnit unit, params string[] names)
		{
			return names.Any(new Func<string, bool>(unit.HaveBuff));
		}

		// Token: 0x06000054 RID: 84 RVA: 0x000044C4 File Offset: 0x000026C4
		public static bool HaveAllDebuffs(this WoWUnit unit, params string[] names)
		{
			return names.All(new Func<string, bool>(unit.HaveBuff));
		}

		// Token: 0x06000055 RID: 85 RVA: 0x000044E8 File Offset: 0x000026E8
		public static bool HaveAllBuffsKnown(this WoWUnit unit, params string[] names)
		{
			foreach (string text in names)
			{
				bool flag = RotationSpellbook.IsKnown(text, 1U) && !unit.HasBuff(text);
				if (flag)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00004534 File Offset: 0x00002734
		public static bool IsPlayer(this WoWUnit unit)
		{
			return unit.Type == WoWObjectType.AzeriteItem;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00004550 File Offset: 0x00002750
		private static string LuaAndCondition(string[] names, string varname)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string text in names)
			{
				stringBuilder.Append(string.Concat(new string[]
				{
					" and ",
					varname,
					" == \"",
					text,
					"\""
				}));
			}
			return stringBuilder.ToString().Substring(5);
		}

		// Token: 0x06000058 RID: 88 RVA: 0x000045C0 File Offset: 0x000027C0
		private static string LuaOrCondition(string[] names, string varname)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string text in names)
			{
				stringBuilder.Append(string.Concat(new string[]
				{
					" or ",
					varname,
					" == \"",
					text,
					"\""
				}));
			}
			return stringBuilder.ToString().Substring(4);
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00004630 File Offset: 0x00002830
		private static string LuaTable(string[] names)
		{
			string str = "{";
			foreach (string str2 in names)
			{
				str = str + "[\"" + str2 + "\"] = false,";
			}
			return str + "};";
		}

		// Token: 0x04000018 RID: 24
		private static readonly Dictionary<int, string> _creatureTypeCache = new Dictionary<int, string>();

		// Token: 0x04000019 RID: 25
		private static readonly HashSet<uint> _invisibleAuras = new HashSet<uint>();
	}
}
