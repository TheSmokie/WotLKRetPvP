using System;
using System.CodeDom.Compiler;
using System.Net;
using System.Net.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Helpers;

// Token: 0x02000002 RID: 2
public class Authentication
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	public Authentication(string orderId, string productId)
	{
		bool flag = orderId == null;
		if (flag)
		{
			MessageBox.Show("[RetPalaPvPTBC]: You need to enter your order id (from your Rocketr email) into the plugin settings to use this!");
			Products.ProductStop();
		}
		this.orderId = orderId;
		this.productId = productId;
		CompilerResults compilerResults = RunCode.Compile(RunCode.CodeType.CSharp, "\r\n                public class AuthMainClass\r\n                {\r\n                    public static void Main()\r\n                    {   \r\n                        System.Text.RegularExpressions.Match matcher = System.Text.RegularExpressions.Regex.Match(authManager.LoginS﻿erver.GetSubcriptionInfoThre﻿ad(), \"(Key: )(.*)(\\\\.\\\\.\\\\.)(.*)\");\r\n                        if (matcher.Success) \r\n                        {\r\n                            robotManager.Helpful.Var.SetVar(\"wRobotAuthKey\", matcher.Groups[2].Value);\r\n                        } \r\n                        else\r\n                        {\r\n                            robotManager.Helpful.Var.SetVar(\"wRobotAuthKey\", \"TRIAL\");\r\n                        }\r\n                        \r\n                    }\r\n                }\r\n            ", false, true);
		RunCode.InvokeStaticMethod(compilerResults, "AuthMainClass", "Main", false);
		Logging.WriteError(RunCode.ErrorsToString(compilerResults), true);
		this.wRobotAuthKey = Var.GetVar<string>("wRobotAuthKey");
		Authentication.httpClient.BaseAddress = new Uri("http://schaka.me:8080");
		Authentication.httpClient.Timeout = TimeSpan.FromSeconds(30.0);
		Authentication._isRunning = true;
		Authentication._validationThread = new Thread(new ThreadStart(this.CheckValidiation));
		Authentication._validationThread.Start();
	}

	// Token: 0x06000002 RID: 2 RVA: 0x00002120 File Offset: 0x00000320
	public static void Kill()
	{
		bool isRunning = Authentication._isRunning;
		if (isRunning)
		{
			Authentication._isRunning = false;
			try
			{
				Authentication._validationThread.Abort();
			}
			catch (Exception ex)
			{
			}
		}
	}

	// Token: 0x06000003 RID: 3 RVA: 0x00002164 File Offset: 0x00000364
	private void CheckValidiation()
	{
		while (Conditions.ProductIsStarted && Authentication._isRunning)
		{
			HttpResponseMessage httpResponseMessage = null;
			try
			{
				bool flag = false;
				try
				{
					httpResponseMessage = Authentication.httpClient.GetAsync(string.Concat(new string[]
					{
						"/authenticate?orderId=",
						this.orderId.Trim(),
						"&productId=",
						this.productId,
						"&wRobotAuthKey=",
						this.wRobotAuthKey
					})).Result;
				}
				catch (Exception arg)
				{
					Logging.WriteError("[1st try] Error connecting to authentication server " + arg, true);
					this.ToggleAllowUnsafeHeaderParsing(true);
					flag = true;
				}
				bool flag2 = (httpResponseMessage != null && httpResponseMessage.StatusCode != HttpStatusCode.OK) || flag;
				if (flag2)
				{
					this.ContinueAfterWait(3.0);
					try
					{
						httpResponseMessage = Authentication.httpClient.GetAsync(string.Concat(new string[]
						{
							"/authenticate?orderId=",
							this.orderId.Trim(),
							"&productId=",
							this.productId,
							"&wRobotAuthKey=",
							this.wRobotAuthKey
						})).Result;
						flag = false;
					}
					catch (Exception arg2)
					{
						Logging.WriteError("[2nd try] Error connecting to authentication server " + arg2, true);
						flag = true;
					}
				}
				string value = (httpResponseMessage != null) ? httpResponseMessage.Content.ReadAsStringAsync().Result : "false";
				bool flag3 = !bool.Parse(value) || (httpResponseMessage != null && httpResponseMessage.StatusCode != HttpStatusCode.OK) || flag;
				if (flag3)
				{
					this.StopBot();
					return;
				}
				this.ContinueAfterWait(3.0);
			}
			catch (ThreadAbortException ex)
			{
				Logging.Write("[RetPalaPvPTBC]: Force closing authentication thread");
			}
			catch (Exception arg3)
			{
				Products.ProductStop();
				MessageBox.Show("[RetPalaPvPTBC]: Some serious error is happening, hop on Discord and report this.");
				Logging.WriteError(string.Format("Result: {0}", (httpResponseMessage != null) ? httpResponseMessage.Content.ReadAsStringAsync() : null), false);
				Logging.WriteError(string.Concat(arg3) ?? "", false);
			}
		}
		Logging.Write("[RetPalaPvPTBC]: Closing authentication thread");
	}

	// Token: 0x06000004 RID: 4 RVA: 0x000023EC File Offset: 0x000005EC
	private void StopBot()
	{
		Authentication._isRunning = false;
		Products.ProductStop();
		MessageBox.Show("[RetPalaPvPTBC]:\r\n            You are trying to use an order id (" + this.orderId + ") for a different product or another IP is already using this order id.\r\n            \r\n            If you changed your IP (one on wRobot session), wait 15 minutes.\r\n\r\n            Keep in mind: Multiple IPs are ONLY allowed within one wRobot license and ONLY if you purchased the multiple IP product!");
	}

	// Token: 0x06000005 RID: 5 RVA: 0x00002418 File Offset: 0x00000618
	private void ContinueAfterWait(double minutes = 3.0)
	{
		while (DateTime.Now.AddMinutes(minutes) > DateTime.Now)
		{
			bool flag = !Conditions.ProductIsStarted;
			if (flag)
			{
				break;
			}
			Thread.Sleep(100);
		}
	}

	// Token: 0x06000006 RID: 6 RVA: 0x00002460 File Offset: 0x00000660
	public bool ToggleAllowUnsafeHeaderParsing(bool enable)
	{
		Assembly assembly = Assembly.GetAssembly(typeof(SettingsSection));
		bool flag = assembly != null;
		if (flag)
		{
			Type type = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
			bool flag2 = type != null;
			if (flag2)
			{
				object obj = type.InvokeMember("Section", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty, null, null, new object[0]);
				bool flag3 = obj != null;
				if (flag3)
				{
					FieldInfo field = type.GetField("useUnsafeHeaderParsing", BindingFlags.Instance | BindingFlags.NonPublic);
					bool flag4 = field != null;
					if (flag4)
					{
						field.SetValue(obj, enable);
						return true;
					}
				}
			}
		}
		return false;
	}

	// Token: 0x04000001 RID: 1
	private readonly string orderId;

	// Token: 0x04000002 RID: 2
	private readonly string productId;

	// Token: 0x04000003 RID: 3
	private readonly string wRobotAuthKey;

	// Token: 0x04000004 RID: 4
	private static readonly HttpClient httpClient = new HttpClient();

	// Token: 0x04000005 RID: 5
	private static Thread _validationThread;

	// Token: 0x04000006 RID: 6
	private static bool _isRunning = false;
}
