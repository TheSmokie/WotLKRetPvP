using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using wManager;
using wManager.Wow;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x0200000A RID: 10
	public class RotationFramework
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00004696 File Offset: 0x00002896
		// (set) Token: 0x0600005C RID: 92 RVA: 0x000046A0 File Offset: 0x000028A0
		internal static bool IsCast
		{
			get
			{
				return RotationFramework._playerCasting;
			}
			set
			{
				DateTime now = DateTime.Now;
				bool flag = RotationFramework._playerCasting && !value && RotationFramework._lastCastingStateChanged.AddMilliseconds(100.0) < now;
				if (flag)
				{
					RotationFramework._playerCasting = false;
					RotationFramework._lastCastingStateChanged = now;
				}
				else
				{
					RotationFramework._playerCasting = value;
					RotationFramework._lastCastingStateChanged = now;
				}
			}
		}

		// Token: 0x17000008 RID: 8
		// (set) Token: 0x0600005D RID: 93 RVA: 0x000046FB File Offset: 0x000028FB
		internal static bool ForceIsCast
		{
			set
			{
				RotationFramework._playerCasting = value;
				RotationFramework._lastCastingStateChanged = DateTime.Now;
			}
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00004710 File Offset: 0x00002910
		public static void Initialize(bool slowRotation = false, bool framelock = true)
		{
			bool frameIsLocked = Memory.WowMemory.FrameIsLocked;
			if (frameIsLocked)
			{
				Memory.WowMemory.UnlockFrame(false);
			}
			RotationFramework._rotationSpellbook = new RotationSpellbook();
			RotationFramework._slowRotation = slowRotation;
			RotationFramework._framelock = framelock;
			RotationEventHandler.Start();
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00004758 File Offset: 0x00002958
		public static void Dispose()
		{
			bool frameIsLocked = Memory.WowMemory.FrameIsLocked;
			if (frameIsLocked)
			{
				Memory.WowMemory.UnlockFrame(false);
			}
			RotationEventHandler.Stop();
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00004788 File Offset: 0x00002988
		public static void RunRotation(List<RotationStep> rotation)
		{
			float globalCooldown = RotationFramework.GetGlobalCooldown();
			bool flag = globalCooldown != 0f;
			RotationFramework.IsCast = (RotationFramework.Me.IsCast || RotationFramework.Me.IsCasting());
			bool slowRotation = RotationFramework._slowRotation;
			if (slowRotation)
			{
				bool isCast = RotationFramework.IsCast;
				if (isCast)
				{
					int castingTimeLeft = RotationFramework.Me.CastingTimeLeft;
					RotationLogger.Fight(string.Format("Slow rotation - still casting! Wait for {0}", castingTimeLeft + 100));
					Thread.Sleep(castingTimeLeft + 100);
				}
				else
				{
					bool flag2 = flag;
					if (flag2)
					{
						RotationLogger.Fight(string.Format("No spell casted, waiting for {0} for global cooldown to end!", globalCooldown * 1000f + 100f));
						Thread.Sleep((int)(globalCooldown * 1000f + 100f));
					}
				}
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			bool framelock = RotationFramework._framelock;
			if (framelock)
			{
				RotationFramework.RunInFrameLock(rotation, flag);
			}
			else
			{
				RotationFramework.RunInLock(rotation, flag);
			}
			stopwatch.Stop();
			bool flag3 = stopwatch.ElapsedMilliseconds > 150L;
			if (flag3)
			{
				RotationLogger.Fight("Iteration took " + stopwatch.ElapsedMilliseconds + "ms");
			}
		}

		// Token: 0x06000061 RID: 97 RVA: 0x000048B8 File Offset: 0x00002AB8
		private static void RunInLock(List<RotationStep> rotation, bool gcdEnabled)
		{
			object locker = ObjectManager.Locker;
			lock (locker)
			{
				RotationFramework.UpdateUnits();
				foreach (RotationStep rotationStep in rotation)
				{
					bool flag2 = rotationStep.ExecuteStep(gcdEnabled);
					if (flag2)
					{
						break;
					}
				}
			}
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00004948 File Offset: 0x00002B48
		private static void RunInFrameLock(List<RotationStep> rotation, bool gcdEnabled)
		{
			bool framelock = RotationFramework._framelock;
			if (framelock)
			{
				wManagerSetting.CurrentSetting.UseLuaToMove = true;
				Memory.WowMemory.LockFrame();
			}
			RotationFramework.UpdateUnits();
			foreach (RotationStep rotationStep in rotation)
			{
				bool flag = rotationStep.ExecuteStep(gcdEnabled);
				if (flag)
				{
					break;
				}
			}
			bool framelock2 = RotationFramework._framelock;
			if (framelock2)
			{
				Memory.WowMemory.UnlockFrame(false);
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000049E4 File Offset: 0x00002BE4
		private static void UpdateUnits()
		{
			RotationFramework.player = ObjectManager.Me;
			RotationFramework.target = ObjectManager.Target;
			RotationFramework.pet = ObjectManager.Pet;
			List<WoWUnit> list = new List<WoWUnit>();
			list.AddRange(from u in ObjectManager.GetObjectWoWPlayer()
			where u.GetDistance <= 50f
			select u);
			list.AddRange(from u in ObjectManager.GetObjectWoWUnit()
			where u.GetDistance <= 50f
			select u);
			RotationFramework.units.Clear();
			RotationFramework.units.AddRange(list);
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000064 RID: 100 RVA: 0x00004A8D File Offset: 0x00002C8D
		public static WoWLocalPlayer Me
		{
			get
			{
				return RotationFramework.player;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00004A94 File Offset: 0x00002C94
		public static WoWUnit Target
		{
			get
			{
				return RotationFramework.target;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00004A9B File Offset: 0x00002C9B
		public static WoWUnit Pet
		{
			get
			{
				return RotationFramework.pet;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000067 RID: 103 RVA: 0x00004AA2 File Offset: 0x00002CA2
		public static List<WoWUnit> Units
		{
			get
			{
				return RotationFramework.units;
			}
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00004AAC File Offset: 0x00002CAC
		public static float GetGlobalCooldown()
		{
			bool flag = Usefuls.WowVersion > 8606U;
			float result;
			if (flag)
			{
				result = (float)SpellManager.GlobalCooldownTimeLeft() / 1000f;
			}
			else
			{
				string command = "\r\n\t        local lastCd = 0;\r\n\t        local globalCd = 0;\r\n\r\n\t        for i = 1, 20 do\r\n\t            \r\n\t            local spellName, spellRank = GetSpellName(i, BOOKTYPE_SPELL);\r\n\r\n\t            if not spellName then\r\n\t                break;\r\n\t            end\r\n\t            \r\n\t            local start, duration, enabled = GetSpellCooldown(i, BOOKTYPE_SPELL);\r\n\r\n\t            if enabled == 1 and start > 0 and duration > 0 then\r\n\t                lastCd = (start + duration - GetTime()); -- cooldown in seconds\r\n\t            end            \r\n\r\n\t            if lastCd > 0 and lastCd <= 1.5 then\r\n\t                local currentCd = (start + duration - GetTime());\r\n\t                if lastCd - currentCd <= 0.001 then\r\n\t                    globalCd = currentCd;\r\n\t                    break;\r\n\t                end\r\n\t            end\r\n\r\n\t        end\r\n\r\n\t        return globalCd;";
				result = Lua.LuaDoString<float>(command, "");
			}
			return result;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00004AF0 File Offset: 0x00002CF0
		public static float GetItemCooldown(string itemName)
		{
			string command = "\r\n\t        for bag=0,4 do\r\n\t            for slot=1,36 do\r\n\t                local name = GetContainerItemLink(bag,slot);\r\n\t                if (name and name == \"" + itemName + "\") then\r\n\t                    local start, duration, enabled = GetContainerItemCooldown(bag, slot);\r\n\t                    if enabled then\r\n\t                        return (duration - (GetTime() - start)) * 1000;\r\n\t                    end\r\n\t                end;\r\n\t            end;\r\n\t        end\r\n\t        return 0;";
			return Lua.LuaDoString<float>(command, "");
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00004B1E File Offset: 0x00002D1E
		public static void UsePvPTrinket()
		{
			Lua.RunMacroText("/cast Every Man for Himself\n/use Medallion of the Alliance\n/use Medallion of the Horde\n/use Titan-Forged Rune of Audacity\n/use Titan-Forged Rune of Determination\n/use Titan-Forged Rune of Accuracy\n/use Titan-Forged Rune of Cruelty\n/use Titan-Forged Rune of Alacrity");
		}

		// Token: 0x0400001A RID: 26
		private static RotationSpellbook _rotationSpellbook;

		// Token: 0x0400001B RID: 27
		private static bool _slowRotation = false;

		// Token: 0x0400001C RID: 28
		private static bool _framelock = true;

		// Token: 0x0400001D RID: 29
		private static WoWLocalPlayer player = ObjectManager.Me;

		// Token: 0x0400001E RID: 30
		private static WoWUnit pet = ObjectManager.Pet;

		// Token: 0x0400001F RID: 31
		private static WoWUnit target = ObjectManager.Target;

		// Token: 0x04000020 RID: 32
		private static List<WoWUnit> units = ObjectManager.GetObjectWoWUnit();

		// Token: 0x04000021 RID: 33
		private static bool _playerCasting;

		// Token: 0x04000022 RID: 34
		private static DateTime _lastCastingStateChanged = DateTime.MinValue;
	}
}
