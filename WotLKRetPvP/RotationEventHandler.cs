using System;
using System.Collections.Generic;
using wManager.Wow.Helpers;

namespace CombatRotation.RotationFramework
{
	// Token: 0x02000008 RID: 8
	public class RotationEventHandler
	{
		// Token: 0x06000044 RID: 68 RVA: 0x00003EF7 File Offset: 0x000020F7
		public static void Start()
		{
			RotationEventHandler._luaPlayerGuid = Lua.LuaDoString<string>("return UnitGUID('player');", "");
			EventsLuaWithArgs.OnEventsLuaStringWithArgs += RotationEventHandler.CombatLogEventHandler;
			EventsLuaWithArgs.OnEventsLuaStringWithArgs += RotationSpellVerifier.NotifyForDelegate;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00003F32 File Offset: 0x00002132
		public static void Stop()
		{
			EventsLuaWithArgs.OnEventsLuaStringWithArgs -= RotationSpellVerifier.NotifyForDelegate;
			EventsLuaWithArgs.OnEventsLuaStringWithArgs -= RotationEventHandler.CombatLogEventHandler;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003F5C File Offset: 0x0000215C
		private static void CombatLogEventHandler(string id, List<string> args)
		{
			bool flag = id == "PLAYER_DEAD";
			if (flag)
			{
				RotationSpellVerifier.ForceClearVerification();
			}
			bool flag2 = id == "COMBAT_LOG_EVENT_UNFILTERED";
			if (flag2)
			{
				RotationSpellVerifier.NotifyCombatLog(args);
			}
			bool flag3 = id == "UNIT_SPELLCAST_FAILED" || id == "UNIT_SPELLCAST_INTERRUPTED" || id == "UNIT_SPELLCAST_FAILED_QUIET" || id == "UNIT_SPELLCAST_STOP" || id == "UNIT_SPELLMISS";
			if (flag3)
			{
				string a = args[0];
				string spellName = args[1];
				bool flag4 = a == "player" && RotationSpellVerifier.IsSpellWaitingForVerification(spellName);
				if (flag4)
				{
					RotationSpellVerifier.ForceClearVerification(spellName);
				}
				bool flag5 = a == "player";
				if (flag5)
				{
					RotationFramework.ForceIsCast = false;
				}
			}
			bool flag6 = id == "UNIT_SPELLCAST_SUCCEEDED" || id == "UNIT_SPELLCAST_SENT";
			if (flag6)
			{
				string a2 = args[0];
				string text = args[1];
				bool flag7 = a2 == "player" && RotationSpellVerifier.IsSpellWaitingForVerification(text);
				if (flag7)
				{
					List<string> args2 = new List<string>
					{
						"0",
						"SPELL_CAST_SUCCESS",
						RotationEventHandler._luaPlayerGuid,
						RotationEventHandler._playerName,
						"0x0000000000000000",
						"0x0000000000000000",
						"nil",
						"0x0000000000000000",
						"0",
						text,
						"0x00"
					};
					RotationSpellVerifier.NotifyCombatLog(args2);
				}
				bool flag8 = a2 == "player";
				if (flag8)
				{
					RotationFramework.ForceIsCast = false;
				}
			}
			bool flag9 = id == "UNIT_COMBAT";
			if (flag9)
			{
				List<string> args3 = new List<string>
				{
					"0",
					"NONE",
					RotationEventHandler._luaPlayerGuid,
					RotationEventHandler._playerName,
					"0x0000000000000000",
					"0x0000000000000000",
					"nil",
					"0x0000000000000000"
				};
				RotationSpellVerifier.NotifyCombatLog(args3);
			}
			bool flag10 = id == "COMBAT_TEXT_UPDATE";
			if (flag10)
			{
			}
			bool flag11 = id == "UI_ERROR_MESSAGE" && (args[0] == "Out of range." || args[0] == "You are too far away!");
			if (flag11)
			{
				RotationSpellVerifier.ClearIfOutOfRange();
			}
		}

		// Token: 0x04000016 RID: 22
		private static string _luaPlayerGuid;

		// Token: 0x04000017 RID: 23
		private static string _playerName = RotationFramework.Me.Name;
	}
}
