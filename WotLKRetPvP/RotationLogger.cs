using System;
using robotManager.Helpful;

namespace CombatRotation.RotationFramework
{
	// Token: 0x0200000B RID: 11
	public static class RotationLogger
	{
		// Token: 0x0600006D RID: 109 RVA: 0x00004B6C File Offset: 0x00002D6C
		public static void Trace(string log)
		{
			bool flag = RotationLogger.Level >= RotationLogger.LogLevel.TRACE;
			if (flag)
			{
				Logging.WriteDebug("[RTF]: " + log);
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00004B9C File Offset: 0x00002D9C
		public static void Fight(string log)
		{
			Logging.WriteFight("[RTF] " + log);
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00004BB0 File Offset: 0x00002DB0
		public static void LightDebug(string log)
		{
			bool flag = RotationLogger.Level >= RotationLogger.LogLevel.DEBUG_LIGHT;
			if (flag)
			{
				Logging.WriteFight("[RTF] " + log);
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00004BE0 File Offset: 0x00002DE0
		public static void Debug(string log)
		{
			bool flag = RotationLogger.Level >= RotationLogger.LogLevel.DEBUG;
			if (flag)
			{
				Logging.WriteFight("[RTF] " + log);
			}
		}

		// Token: 0x04000023 RID: 35
		public static RotationLogger.LogLevel Level = RotationLogger.LogLevel.DEBUG;

		// Token: 0x02000026 RID: 38
		public enum LogLevel
		{
			// Token: 0x04000083 RID: 131
			INFO,
			// Token: 0x04000084 RID: 132
			DEBUG_LIGHT,
			// Token: 0x04000085 RID: 133
			DEBUG = 3,
			// Token: 0x04000086 RID: 134
			TRACE
		}
	}
}
