using System;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x02000006 RID: 6
	public interface RotationAction
	{
		// Token: 0x06000027 RID: 39
		bool Execute(WoWUnit target, bool force = false);

		// Token: 0x06000028 RID: 40
		float Range();

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000029 RID: 41
		bool IgnoresGlobal { get; }
	}
}
