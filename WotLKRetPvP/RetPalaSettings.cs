using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using robotManager.Helpful;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

// Token: 0x02000004 RID: 4
[Serializable]
public class RetPalaSettings : Settings
{
	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000015 RID: 21 RVA: 0x0000344E File Offset: 0x0000164E
	// (set) Token: 0x06000016 RID: 22 RVA: 0x00003456 File Offset: 0x00001656
	[Setting]
	[Category("__IMPORTANT__")]
	[DisplayName("Rocketr Order ID")]
	[Description("This is your tracking number for when you purchased this product, it is required to use this consistently")]
	public string TransactionId { get; set; }

	// Token: 0x17000003 RID: 3
	// (get) Token: 0x06000017 RID: 23 RVA: 0x0000345F File Offset: 0x0000165F
	// (set) Token: 0x06000018 RID: 24 RVA: 0x00003467 File Offset: 0x00001667
	[Setting]
	[DefaultValue(true)]
	[Category("General")]
	[DisplayName("Framelock")]
	[Description("Lock frames before each combat rotation (can help if it skips spells)")]
	public bool FrameLock { get; set; }

	// Token: 0x17000004 RID: 4
	// (get) Token: 0x06000019 RID: 25 RVA: 0x00003470 File Offset: 0x00001670
	// (set) Token: 0x0600001A RID: 26 RVA: 0x00003478 File Offset: 0x00001678
	[Setting]
	[DefaultValue(false)]
	[Category("General")]
	[DisplayName("Slow rotation for performance issues")]
	[Description("If you have performance issues with wRobot and the fightclass, activate this. It will try to sleep until the next spell can be executed. This can and will cause some spells to skip.")]
	public bool SlowRotation { get; set; }

	// Token: 0x0600001B RID: 27 RVA: 0x00003481 File Offset: 0x00001681
	public RetPalaSettings()
	{
		this.TransactionId = null;
		this.SlowRotation = false;
		this.FrameLock = true;
	}

	// Token: 0x17000005 RID: 5
	// (get) Token: 0x0600001C RID: 28 RVA: 0x000034A3 File Offset: 0x000016A3
	// (set) Token: 0x0600001D RID: 29 RVA: 0x000034AA File Offset: 0x000016AA
	public static RetPalaSettings CurrentSetting { get; set; }

	// Token: 0x0600001E RID: 30 RVA: 0x000034B4 File Offset: 0x000016B4
	public bool Save()
	{
		bool result;
		try
		{
			result = base.Save(Settings.AdviserFilePathAndName("CustomClass-RetPalaPvP", ObjectManager.Me.Name + "." + Usefuls.RealmName));
		}
		catch (Exception arg)
		{
			Logging.WriteError("RetPalaSettings > Save(): " + arg, true);
			result = false;
		}
		return result;
	}

	// Token: 0x0600001F RID: 31 RVA: 0x00003518 File Offset: 0x00001718
	public static bool Load()
	{
		try
		{
			bool flag = File.Exists(Settings.AdviserFilePathAndName("CustomClass-RetPalaPvP", ObjectManager.Me.Name + "." + Usefuls.RealmName));
			if (flag)
			{
				RetPalaSettings.CurrentSetting = Settings.Load<RetPalaSettings>(Settings.AdviserFilePathAndName("CustomClass-RetPalaPvP", ObjectManager.Me.Name + "." + Usefuls.RealmName));
				return true;
			}
			RetPalaSettings.CurrentSetting = new RetPalaSettings();
		}
		catch (Exception arg)
		{
			Logging.WriteError("RetPalaSettings > Load(): " + arg, true);
		}
		return false;
	}
}
