using System;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x0200000C RID: 12
	internal class RotationLua : RotationAction
	{
		// Token: 0x06000072 RID: 114 RVA: 0x00004C18 File Offset: 0x00002E18
		public RotationLua(string lua, float range = 30f, bool ignoresGlobal = false)
		{
			this._luaAction = lua;
			this._actionRange = range;
			this.IgnoresGlobal = ignoresGlobal;
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00004C38 File Offset: 0x00002E38
		public bool Execute(WoWUnit target, bool force = false)
		{
			bool flag = force && RotationFramework.Me.IsCasting();
			if (flag)
			{
				Lua.LuaDoString("SpellStopCasting();", false);
			}
			Lua.LuaDoString(this._luaAction, false);
			return true;
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00004C7C File Offset: 0x00002E7C
		public float Range()
		{
			return this._actionRange;
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000075 RID: 117 RVA: 0x00004C94 File Offset: 0x00002E94
		public bool IgnoresGlobal { get; }

		// Token: 0x04000024 RID: 36
		private readonly string _luaAction;

		// Token: 0x04000025 RID: 37
		private readonly float _actionRange;
	}
}
