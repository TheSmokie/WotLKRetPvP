using System;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x0200000D RID: 13
	internal class RotationRawAction : RotationAction
	{
		// Token: 0x06000076 RID: 118 RVA: 0x00004C9C File Offset: 0x00002E9C
		public RotationRawAction(Action rotationAction, float actionRange, bool ignoresGlobal = false)
		{
			this._rotationAction = rotationAction;
			this._actionRange = actionRange;
			this.IgnoresGlobal = ignoresGlobal;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00004CBC File Offset: 0x00002EBC
		public bool Execute(WoWUnit target, bool force = false)
		{
			this._rotationAction();
			return true;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00004CDC File Offset: 0x00002EDC
		public float Range()
		{
			return this._actionRange;
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000079 RID: 121 RVA: 0x00004CF4 File Offset: 0x00002EF4
		public bool IgnoresGlobal { get; }

		// Token: 0x04000027 RID: 39
		private readonly Action _rotationAction;

		// Token: 0x04000028 RID: 40
		private readonly float _actionRange;
	}
}
