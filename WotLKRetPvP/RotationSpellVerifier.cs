using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x02000010 RID: 16
	public class RotationSpellVerifier
	{
		// Token: 0x06000095 RID: 149 RVA: 0x000054AC File Offset: 0x000036AC
		public static void NotifyCombatLog(List<string> args)
		{
			object verificationLock = RotationSpellVerifier._verificationLock;
			bool flag = false;
			try
			{
				Monitor.Enter(verificationLock, ref flag);
				string text = args[0];
				string eventName = args[1];
				string text2 = args[2];
				string text3 = args[3];
				string text4 = args[4];
				string text5 = args[5];
				string text6 = args[6];
				string text7 = args[7];
				RotationSpell.VerificationType verificationType = RotationSpellVerifier.GetVerificationType();
				bool flag2 = RotationSpellVerifier._successEvents[verificationType].Contains(eventName);
				if (flag2)
				{
					string text8 = args[8];
					string text9 = args[9];
					string text10 = args[10];
					RotationLogger.Trace(string.Concat(new string[]
					{
						eventName,
						" ",
						text2,
						" ",
						text3,
						" ",
						text5,
						" ",
						text6,
						" ",
						text8,
						" ",
						text9,
						" ",
						text10
					}));
					ulong guidforLuaGUID = RotationSpellVerifier.GetGUIDForLuaGUID(text2);
					bool flag3 = guidforLuaGUID == RotationSpellVerifier._playerGuid && RotationSpellVerifier.IsSpellWaitingForVerification(text9);
					if (flag3)
					{
						Tuple<string, string> tuple = RotationSpellVerifier._eventDelegates.FirstOrDefault((Tuple<string, string> e) => e.Item1 == eventName);
						bool flag4 = tuple != null;
						if (flag4)
						{
							string item = tuple.Item2;
							RotationLogger.Debug("Delegating " + eventName + " to " + item);
							RotationSpellVerifier.CreatePassiveEventDelegate(item);
						}
						else
						{
							RotationLogger.Debug("Clearing verification for " + text9);
							RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
						}
					}
					ulong guidforLuaGUID2 = RotationSpellVerifier.GetGUIDForLuaGUID(text5);
					bool flag5 = guidforLuaGUID == 0UL && RotationSpellVerifier.IsWaitingForSpellOnTarget(text9, guidforLuaGUID2);
					if (flag5)
					{
						Tuple<string, string> tuple2 = RotationSpellVerifier._eventDelegates.FirstOrDefault((Tuple<string, string> e) => e.Item1 == eventName);
						bool flag6 = tuple2 != null;
						if (flag6)
						{
							string item2 = tuple2.Item2;
							RotationLogger.Debug("Delegating " + eventName + " to " + item2);
							RotationSpellVerifier.CreatePassiveEventDelegate(item2);
						}
						else
						{
							RotationLogger.Debug("Clearing verification for spell with no source " + text9);
							RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
						}
					}
				}
				bool flag7 = eventName == "SPELL_CAST_FAILED";
				if (flag7)
				{
					string text11 = args[8];
					string text12 = args[9];
					string text13 = args[10];
					string text14 = args[11];
					ulong guidforLuaGUID3 = RotationSpellVerifier.GetGUIDForLuaGUID(text2);
					bool flag8 = guidforLuaGUID3 == RotationSpellVerifier._playerGuid && RotationSpellVerifier.IsSpellWaitingForVerification(text12) && text14 != "Another action is in progress";
					if (flag8)
					{
						RotationLogger.Debug("Clearing verification for " + text12 + " because " + text14);
						RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
					}
				}
				bool flag9 = eventName == "UNIT_DIED";
				if (flag9)
				{
					ulong guidforLuaGUID4 = RotationSpellVerifier.GetGUIDForLuaGUID(text5);
					bool flag10 = RotationSpellVerifier.IsWaitingOnTarget(guidforLuaGUID4);
					if (flag10)
					{
						RotationLogger.Debug("Clearing verification because target died");
						RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
					}
					bool flag11 = guidforLuaGUID4 == RotationSpellVerifier._playerGuid;
					if (flag11)
					{
						RotationLogger.Debug("Clearing verification because we died");
						RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
					}
				}
				RotationSpellVerifier.ClearVerificationOlderThan(10U);
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(verificationLock);
				}
			}
		}

		// Token: 0x06000096 RID: 150 RVA: 0x0000583C File Offset: 0x00003A3C
		public static void QueueVerification(string spellName, WoWUnit target, RotationSpell.VerificationType type)
		{
			object verificationLock = RotationSpellVerifier._verificationLock;
			lock (verificationLock)
			{
				RotationLogger.Debug("Queueing verification for " + spellName + " on " + Thread.CurrentThread.Name);
				RotationSpellVerifier._verification = new Tuple<string, ulong, RotationSpell.VerificationType, DateTime>(spellName, target.Guid, type, DateTime.Now);
				RotationSpellVerifier.RegisterCombatLogClearer();
			}
		}

		// Token: 0x06000097 RID: 151 RVA: 0x000058B8 File Offset: 0x00003AB8
		public static void ForceClearVerification()
		{
			object verificationLock = RotationSpellVerifier._verificationLock;
			lock (verificationLock)
			{
				bool flag2 = RotationSpellVerifier._verification.Item1 != RotationSpellVerifier._emptyVerify.Item1;
				if (flag2)
				{
					RotationLogger.Debug("Force clearing verification with current spell waiting on " + RotationSpellVerifier._verification.Item1);
					RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
				}
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x0000593C File Offset: 0x00003B3C
		public static void ForceClearVerification(string spellName)
		{
			object verificationLock = RotationSpellVerifier._verificationLock;
			lock (verificationLock)
			{
				RotationLogger.Debug("Force clearing verification for " + spellName);
				RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
			}
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00005998 File Offset: 0x00003B98
		public static bool IsWaitingForVerification()
		{
			object verificationLock = RotationSpellVerifier._verificationLock;
			bool result;
			lock (verificationLock)
			{
				result = (RotationSpellVerifier._verification.Item1 != RotationSpellVerifier._emptyVerify.Item1);
			}
			return result;
		}

		// Token: 0x0600009A RID: 154 RVA: 0x000059F0 File Offset: 0x00003BF0
		public static bool IsSpellWaitingForVerification(string spellName)
		{
			object verificationLock = RotationSpellVerifier._verificationLock;
			bool result;
			lock (verificationLock)
			{
				result = (RotationSpellVerifier._verification.Item1 == spellName);
			}
			return result;
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00005A40 File Offset: 0x00003C40
		public static void NotifyForDelegate(string id, List<string> args)
		{
			object verificationLock = RotationSpellVerifier._verificationLock;
			lock (verificationLock)
			{
				bool flag2 = !string.IsNullOrEmpty(RotationSpellVerifier._delegateVerification) && id == RotationSpellVerifier._delegateVerification && args[0] == "focus";
				if (flag2)
				{
					RotationLogger.Debug("Clearing verification for " + RotationSpellVerifier._verification.Item1 + " after delegated event " + id);
					RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
					RotationSpellVerifier._delegateVerification = string.Empty;
				}
			}
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00005AE8 File Offset: 0x00003CE8
		public static void ClearIfOutOfRange()
		{
			object verificationLock = RotationSpellVerifier._verificationLock;
			lock (verificationLock)
			{
				bool flag2 = RotationSpellVerifier._verification.Item1 != RotationSpellVerifier._emptyVerify.Item1;
				if (flag2)
				{
					bool flag3 = !RotationCombatUtil.ExecuteActionOnTarget<bool>(RotationSpellVerifier._verification.Item2, (string luaUnitId) => Lua.LuaDoString<bool>(string.Concat(new string[]
					{
						"\r\n                    local spellInRange = IsSpellInRange(\"",
						RotationSpellVerifier._verification.Item1,
						"\", \"",
						luaUnitId,
						"\") == 1;\r\n                    --DEFAULT_CHAT_FRAME:AddMessage(\"Checking range of ",
						RotationSpellVerifier._verification.Item1,
						" on ",
						luaUnitId,
						" is \" .. (spellInRange and 'true' or 'false'));\r\n                    return spellInRange;"
					}), "")) && !RotationFramework.Me.IsCast;
					if (flag3)
					{
						RotationLogger.Debug(string.Format("Force clearing verification for {0} on {1} because we're out of range", RotationSpellVerifier._verification.Item1, RotationSpellVerifier._verification.Item2));
						RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
					}
				}
			}
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00005BC4 File Offset: 0x00003DC4
		private static bool IsWaitingForSpellOnTarget(string spellName, ulong guid)
		{
			return RotationSpellVerifier._verification.Item1 == spellName && RotationSpellVerifier._verification.Item2 == guid;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00005BF8 File Offset: 0x00003DF8
		private static bool IsWaitingOnTarget(ulong guid)
		{
			return RotationSpellVerifier._verification.Item2 == guid;
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00005C18 File Offset: 0x00003E18
		private static RotationSpell.VerificationType GetVerificationType()
		{
			return RotationSpellVerifier._verification.Item3;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00005C34 File Offset: 0x00003E34
		private static void ClearVerificationOlderThan(uint seconds)
		{
			bool flag = RotationSpellVerifier._verification.Item1 != RotationSpellVerifier._emptyVerify.Item1 && RotationSpellVerifier._verification.Item4.AddSeconds(seconds) < DateTime.Now;
			if (flag)
			{
				RotationLogger.Debug(string.Format("Force clearing verification because spell could not be verified for {0} seconds", seconds));
				RotationSpellVerifier._verification = RotationSpellVerifier._emptyVerify;
			}
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00005CA5 File Offset: 0x00003EA5
		private static void CreatePassiveEventDelegate(string delegatedEvent)
		{
			RotationSpellVerifier._delegateVerification = delegatedEvent;
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00005CAE File Offset: 0x00003EAE
		private static void RegisterCombatLogClearer()
		{
			Lua.LuaDoString("\r\n            if not combatLogClearer then\r\n                combatLogClearer = true;\r\n                local f = CreateFrame(\"Frame\", nil, UIParent); \r\n                f:SetScript(\"OnUpdate\", CombatLogClearEntries);\r\n            end\r\n            ", false);
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00005CC0 File Offset: 0x00003EC0
		public static WoWUnit GetWoWObjectByLuaUnitId(string luaUnitId)
		{
			ulong guid = RotationSpellVerifier.GetGUIDForLuaGUID(Lua.LuaDoString<string>("return UnitGUID('" + luaUnitId + "')", ""));
			bool flag = !string.IsNullOrWhiteSpace(luaUnitId);
			WoWUnit result;
			if (flag)
			{
				result = ObjectManager.GetObjectWoWUnit().FirstOrDefault((WoWUnit o) => o.Guid == guid);
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00005D24 File Offset: 0x00003F24
		public static ulong GetGUIDForLuaGUID(string luaGuid)
		{
			ulong result;
			ulong.TryParse(luaGuid.Replace("x", string.Empty), NumberStyles.HexNumber, null, out result);
			return result;
		}

		// Token: 0x04000034 RID: 52
		private static readonly object _verificationLock = new object();

		// Token: 0x04000035 RID: 53
		private static Tuple<string, ulong, RotationSpell.VerificationType, DateTime> _emptyVerify = new Tuple<string, ulong, RotationSpell.VerificationType, DateTime>("Empty", 0UL, RotationSpell.VerificationType.NONE, DateTime.MinValue);

		// Token: 0x04000036 RID: 54
		private static Tuple<string, ulong, RotationSpell.VerificationType, DateTime> _verification = RotationSpellVerifier._emptyVerify;

		// Token: 0x04000037 RID: 55
		private static string _delegateVerification = string.Empty;

		// Token: 0x04000038 RID: 56
		private static ulong _playerGuid = ObjectManager.Me.Guid;

		// Token: 0x04000039 RID: 57
		private static Dictionary<RotationSpell.VerificationType, HashSet<string>> _successEvents = new Dictionary<RotationSpell.VerificationType, HashSet<string>>
		{
			{
				RotationSpell.VerificationType.CAST_RESULT,
				new HashSet<string>
				{
					"SPELL_DAMAGE",
					"RANGED_DAMAGE",
					"SPELL_MISSED",
					"SPELL_HEAL",
					"SPELL_DRAIN",
					"SPELL_LEECH",
					"SPELL_SUMMON",
					"SPELL_CREATE",
					"SPELL_INSTAKILL"
				}
			},
			{
				RotationSpell.VerificationType.CAST_SUCCESS,
				new HashSet<string>
				{
					"SPELL_CAST_SUCCESS",
					"SPELL_MISSED"
				}
			},
			{
				RotationSpell.VerificationType.AURA,
				new HashSet<string>
				{
					"SPELL_AURA_APPLIED",
					"SPELL_AURA_APPLIED_DOSE",
					"SPELL_AURA_REFRESH",
					"SPELL_MISSED"
				}
			},
			{
				RotationSpell.VerificationType.NONE,
				new HashSet<string>()
			}
		};

		// Token: 0x0400003A RID: 58
		private static HashSet<Tuple<string, string>> _eventDelegates = new HashSet<Tuple<string, string>>
		{
			new Tuple<string, string>("SPELL_HEAL", "UNIT_HEALTH"),
			new Tuple<string, string>("SPELL_AURA_APPLIED", "UNIT_AURA")
		};
	}
}
