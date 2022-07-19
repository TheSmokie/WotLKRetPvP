using System;
using System.Collections.Generic;
using System.Linq;
using robotManager.Helpful;
using wManager.Wow.Helpers;

namespace CombatRotation.RotationFramework
{
	// Token: 0x0200000F RID: 15
	public class RotationSpellbook
	{
		// Token: 0x06000089 RID: 137 RVA: 0x00005020 File Offset: 0x00003220
		public RotationSpellbook()
		{
			EventsLuaWithArgs.OnEventsLuaStringWithArgs += this.LuaEventHandler;
			RotationSpellbook._playerSpells.AddRange(this.GetSpellsFromLua("BOOKTYPE_SPELL"));
			RotationSpellbook._playerSpells.AddRange(this.GetSpellsFromLua("BOOKTYPE_PET"));
			foreach (RotationSpellbook.PlayerSpell playerSpell in RotationSpellbook._playerSpells)
			{
				RotationLogger.Debug(string.Format("Fightclass framework found in spellbook: {0} Rank {1}", playerSpell.Name, playerSpell.Rank));
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000050D8 File Offset: 0x000032D8
		~RotationSpellbook()
		{
			EventsLuaWithArgs.OnEventsLuaStringWithArgs -= this.LuaEventHandler;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00005114 File Offset: 0x00003314
		private void LuaEventHandler(string id, List<string> args)
		{
			bool flag = id == "LEARNED_SPELL_IN_TAB";
			if (flag)
			{
				RotationLogger.Debug("Updating known spells because of " + id);
				this.SpellUpdateHandler();
			}
			bool flag2 = id == "PET_BAR_UPDATE" && RotationSpellbook._lastUpdate.AddSeconds(1.0) < DateTime.Now;
			if (flag2)
			{
				RotationSpellbook._lastUpdate = DateTime.Now;
				RotationLogger.Debug("Updating known spells because of " + id);
				this.SpellUpdateHandler();
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600008C RID: 140 RVA: 0x000051A0 File Offset: 0x000033A0
		public static string RankString
		{
			get
			{
				return RotationSpellbook.LocaleToRank[RotationSpellbook.Locale];
			}
		}

		// Token: 0x0600008D RID: 141 RVA: 0x000051B4 File Offset: 0x000033B4
		public static bool IsKnown(string spellName, uint rank = 1U)
		{
			return RotationSpellbook._playerSpells.Any((RotationSpellbook.PlayerSpell spell) => spell.Name == spellName && spell.Rank >= rank);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x000051F0 File Offset: 0x000033F0
		public static RotationSpellbook.PlayerSpell Get(string spellName, uint rank = 0U)
		{
			bool flag = rank > 0U;
			RotationSpellbook.PlayerSpell result;
			if (flag)
			{
				result = RotationSpellbook._playerSpells.FirstOrDefault((RotationSpellbook.PlayerSpell spell) => spell.Name == spellName && spell.Rank == rank);
			}
			else
			{
				result = (from spell in RotationSpellbook._playerSpells
				where spell.Name == spellName
				select spell into p
				orderby p.Rank
				select p).LastOrDefault<RotationSpellbook.PlayerSpell>();
			}
			return result;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x0000527C File Offset: 0x0000347C
		private void SpellUpdateHandler()
		{
			RotationSpellbook._playerSpells.Clear();
			RotationSpellbook._playerSpells.AddRange(this.GetSpellsFromLua("BOOKTYPE_SPELL"));
			RotationSpellbook._playerSpells.AddRange(this.GetSpellsFromLua("BOOKTYPE_PET"));
		}

		// Token: 0x06000090 RID: 144 RVA: 0x000052B8 File Offset: 0x000034B8
		private List<RotationSpellbook.PlayerSpell> GetSpellsFromLua(string bookType = "BOOKTYPE_SPELL")
		{
			string command = (Usefuls.WowVersion >= 13164U) ? this.GetRankStringCata(bookType) : this.GetRankStringTbc(bookType);
			List<string> source = Lua.LuaDoString<string>(command, "").Split(new char[]
			{
				';'
			}).ToList<string>();
			return (from s in source
			where !string.IsNullOrWhiteSpace(s)
			select s).Select(delegate(string spellString)
			{
				uint rank = RotationSpellbook.convertSafely(spellString, 1, 1U);
				uint castTime = RotationSpellbook.convertSafely(spellString, 2, 0U);
				uint minRange = RotationSpellbook.convertSafely(spellString, 3, 0U);
				uint maxRange = RotationSpellbook.convertSafely(spellString, 4, 5U);
				return new RotationSpellbook.PlayerSpell
				{
					Name = spellString.Split(new char[]
					{
						'+'
					})[0],
					Rank = rank,
					CastTime = castTime,
					MinRange = minRange,
					MaxRange = maxRange
				};
			}).ToList<RotationSpellbook.PlayerSpell>();
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00005358 File Offset: 0x00003558
		private static uint convertSafely(string spellString, int index, uint fallback)
		{
			uint result = fallback;
			try
			{
				result = Convert.ToUInt32(spellString.Split(new char[]
				{
					'+'
				})[index]);
			}
			catch (Exception ex)
			{
				Logging.WriteError("Error converting " + spellString, true);
			}
			return result;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x000053B4 File Offset: 0x000035B4
		private string GetRankStringTbc(string bookType = "BOOKTYPE_SPELL")
		{
			return "\r\n\t        local knownSpells = \"\"\r\n\t\t\tlocal function round(n)\r\n\t\t\t    return n % 1 >= 0.5 and math.ceil(n) or math.floor(n)\r\n\t\t\tend\r\n\r\n\t        \r\n\t        local i = 1;\r\n\t        while true do\r\n\t            local spellName, spellRank = GetSpellName(i, " + bookType + ");\r\n\t            \r\n\t            if not spellName then\r\n\t                break;\r\n\t            end\r\n\r\n\t            local _, _, currentRankString = string.find(spellRank, \" (%d+)$\");\r\n\t            local currentRank = tonumber(currentRankString);\r\n\t            local castTime, minRange, maxRange, spellId = 0, 0, 0, 0;\r\n\r\n\t            if (string.find(spellRank, \" (%d+)$\")) then\r\n\t\t\t\t\t-- name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange\r\n\t                _, _, _, _, _, _, castTime, minRange, maxRange = GetSpellInfo(spellName .. \"(\" .. spellRank .. \")\");\r\n\t            end\r\n\t            \r\n\t            knownSpells = knownSpells .. spellName .. \"+\" .. (currentRank and currentRank or '1') .. \"+\" .. castTime .. \"+\" .. round(minRange) .. \"+\" .. round(maxRange) .. \";\"            \r\n\r\n\t            i = i + 1;\r\n\t        end\r\n\t        return knownSpells;";
		}

		// Token: 0x06000093 RID: 147 RVA: 0x000053D8 File Offset: 0x000035D8
		private string GetRankStringCata(string bookType = "BOOKTYPE_SPELL")
		{
			return string.Concat(new string[]
			{
				"\r\n\t        local knownSpells = \"\"\r\n\t\t\tlocal function round(n)\r\n\t\t\t    return n % 1 >= 0.5 and math.ceil(n) or math.floor(n)\r\n\t\t\tend\r\n\r\n\t        \r\n\t        local i = 1;\r\n\t        while true do\r\n\t            local spellName, spellRank = GetSpellBookItemName(i, ",
				bookType,
				");\r\n\t            local skillType, special = GetSpellBookItemInfo(i, ",
				bookType,
				");\r\n\t            \r\n\t            if not spellName then\r\n\t                break;\r\n\t            end\r\n\r\n\t            local _, _, currentRankString = string.find(spellRank, \" (%d+)$\");\r\n\t            local currentRank = tonumber(currentRankString);\r\n\t            local castTime, minRange, maxRange, spellId = 0, 0, 0, 0;\r\n\r\n\t            if (skillType == \"SPELL\") then\r\n\t                _, _, _, castTime, minRange, maxRange = GetSpellInfo(special);\r\n\t\t\t\t\tspellId = special;\r\n\t            end\r\n\t            \r\n\t            knownSpells = knownSpells .. spellName .. \"+\" .. (currentRank and currentRank or '1') .. \"+\" .. castTime .. \"+\" .. round(minRange) .. \"+\" .. round(maxRange) .. \";\"            \r\n\r\n\t            i = i + 1;\r\n\t        end\r\n\t        return knownSpells;"
			});
		}

		// Token: 0x04000030 RID: 48
		private static List<RotationSpellbook.PlayerSpell> _playerSpells = new List<RotationSpellbook.PlayerSpell>();

		// Token: 0x04000031 RID: 49
		private static DateTime _lastUpdate = DateTime.MinValue;

		// Token: 0x04000032 RID: 50
		private static readonly string Locale = Lua.LuaDoString<string>("return GetLocale()", "");

		// Token: 0x04000033 RID: 51
		private static readonly Dictionary<string, string> LocaleToRank = new Dictionary<string, string>
		{
			{
				"enGB",
				"Rank"
			},
			{
				"enUS",
				"Rank"
			},
			{
				"deDE",
				"Rang"
			},
			{
				"frFr",
				"Rang"
			},
			{
				"ruRU",
				"Уровень"
			}
		};

		// Token: 0x02000028 RID: 40
		public class PlayerSpell
		{
			// Token: 0x0400008C RID: 140
			public string Name;

			// Token: 0x0400008D RID: 141
			public uint CastTime;

			// Token: 0x0400008E RID: 142
			public uint MinRange;

			// Token: 0x0400008F RID: 143
			public uint MaxRange;

			// Token: 0x04000090 RID: 144
			public uint Rank = 1U;
		}
	}
}
