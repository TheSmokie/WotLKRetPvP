using System;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x0200000E RID: 14
	public class RotationSpell : RotationAction
	{
		// Token: 0x0600007A RID: 122 RVA: 0x00004CFC File Offset: 0x00002EFC
		public RotationSpell(string name, uint? rank = null, bool ignoresGlobal = false, bool needsFacing = false, RotationSpell.VerificationType type = RotationSpell.VerificationType.CAST_RESULT)
		{
			this.Spell = new Spell(name);
			this._name = this.Spell.NameInGame;
			this._rank = rank;
			this.IgnoresGlobal = ignoresGlobal;
			this.NeedsFacing = 0;
			this.Verification = type;
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00004D4C File Offset: 0x00002F4C
		public bool NotEnoughMana()
		{
			return Lua.LuaDoString<bool>("return select(2, IsUsableSpell(\"" + this.FullName() + "\"))", "");
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00004D80 File Offset: 0x00002F80
		public bool IsUsable()
		{
			return Lua.LuaDoString<bool>("return IsUsableSpell(\"" + this.FullName() + "\")", "");
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00004DB4 File Offset: 0x00002FB4
		public bool CanCast()
		{
			return Lua.LuaDoString<bool>(string.Concat(new string[]
			{
				"\r\n            local spellCooldown = 0;\r\n            local start, duration, enabled = GetSpellCooldown(\"",
				this._name,
				"\");\r\n            if enabled == 1 and start > 0 and duration > 0 then\r\n                spellCooldown = duration - (GetTime() - start)\r\n            elseif enabled == 0 then\r\n                spellCooldown = 1000000.0;\r\n            end\r\n\r\n            return (IsUsableSpell(\"",
				this.FullName(),
				"\") and spellCooldown == 0)"
			}), "");
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00004E08 File Offset: 0x00003008
		public float GetCooldown()
		{
			string command = "\r\n            local start, duration, enabled = GetSpellCooldown(\"" + this._name + "\");\r\n            if enabled == 1 and start > 0 and duration > 0 then\r\n                return duration - (GetTime() - start)\r\n            elseif enabled == 0 then\r\n                return 10000000.0;\r\n            end\r\n            return 0;";
			return Lua.LuaDoString<float>(command, "");
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00004E3C File Offset: 0x0000303C
		public string FullName()
		{
			return (this._rank != null) ? string.Format("{0}({1} {2})", this._name, RotationSpellbook.RankString, this._rank) : (this._name + "()");
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00004E90 File Offset: 0x00003090
		public bool IsKnown()
		{
			return RotationSpellbook.IsKnown(this._name, this._rank ?? 1U);
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000081 RID: 129 RVA: 0x00004EC7 File Offset: 0x000030C7
		public RotationSpell.VerificationType Verification { get; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000082 RID: 130 RVA: 0x00004ECF File Offset: 0x000030CF
		public bool NeedsFacing { get; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000083 RID: 131 RVA: 0x00004ED7 File Offset: 0x000030D7
		public bool IgnoresGlobal { get; }

		// Token: 0x06000084 RID: 132 RVA: 0x00004EE0 File Offset: 0x000030E0
		public override int GetHashCode()
		{
			return this._name.GetHashCode() + this._rank.GetHashCode();
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00004F14 File Offset: 0x00003114
		public override bool Equals(object obj)
		{
			RotationSpell rotationSpell = (RotationSpell)obj;
			bool result;
			if (this._name.Equals((rotationSpell != null) ? rotationSpell._name : null))
			{
				uint? rank = this._rank;
				uint? num = (rotationSpell != null) ? rotationSpell._rank : null;
				result = (rank.GetValueOrDefault() == num.GetValueOrDefault() & rank != null == (num != null));
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00004F88 File Offset: 0x00003188
		public float CastTime()
		{
			RotationSpellbook.PlayerSpell playerSpell = RotationSpellbook.Get(this._name, this._rank.GetValueOrDefault());
			uint? num = (playerSpell != null) ? new uint?(playerSpell.CastTime) : null;
			return (num != null) ? num.GetValueOrDefault() : 0f;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00004FE4 File Offset: 0x000031E4
		public bool Execute(WoWUnit target, bool force = false)
		{
			return RotationCombatUtil.CastSpell(this, target, force);
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00005000 File Offset: 0x00003200
		public float Range()
		{
			return this.Spell.MaxRange;
		}

		// Token: 0x0400002A RID: 42
		public Spell Spell;

		// Token: 0x0400002B RID: 43
		private readonly string _name;

		// Token: 0x0400002C RID: 44
		private readonly uint? _rank;

		// Token: 0x02000027 RID: 39
		public enum VerificationType
		{
			// Token: 0x04000088 RID: 136
			CAST_RESULT,
			// Token: 0x04000089 RID: 137
			CAST_SUCCESS,
			// Token: 0x0400008A RID: 138
			AURA,
			// Token: 0x0400008B RID: 139
			NONE
		}
	}
}
