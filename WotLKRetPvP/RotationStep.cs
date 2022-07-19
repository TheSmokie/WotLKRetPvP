using System;
using System.Diagnostics;
using wManager.Wow.ObjectManager;

namespace CombatRotation.RotationFramework
{
	// Token: 0x02000011 RID: 17
	public class RotationStep
	{
		// Token: 0x060000A7 RID: 167 RVA: 0x00005ED4 File Offset: 0x000040D4
		public RotationStep(RotationAction spell, float priority, Func<RotationAction, WoWUnit, bool> predicate, Func<Func<WoWUnit, bool>, WoWUnit> targetFinder, bool forceCast = false, bool checkRange = true)
		{
			this._action = spell;
			this.Priority = priority;
			this._predicate = predicate;
			this._targetFinder = targetFinder;
			this._forceCast = forceCast;
			this._checkRange = checkRange;
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00005F24 File Offset: 0x00004124
		public bool ExecuteStep(bool globalActive)
		{
			bool flag = (globalActive && !this._action.IgnoresGlobal) || (RotationFramework.IsCast && !this._forceCast);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				Func<WoWUnit, bool> func;
				if (!this._checkRange)
				{
					func = ((WoWUnit u) => true);
				}
				else
				{
					func = ((WoWUnit u) => u.GetDistance <= this._action.Range());
				}
				Func<WoWUnit, bool> arg = func;
				Stopwatch stopwatch = Stopwatch.StartNew();
				string text = "<noname>";
				bool flag2 = this._action.GetType() == typeof(RotationSpell);
				if (flag2)
				{
					RotationSpell rotationSpell = (RotationSpell)this._action;
					text = rotationSpell.FullName();
				}
				WoWUnit woWUnit = this._targetFinder(arg);
				stopwatch.Stop();
				RotationLogger.Trace(string.Format("({0}) targetFinder ({1}) - {2}: {3} ms", new object[]
				{
					text,
					this._targetFinder.Method.Name,
					(woWUnit != null) ? woWUnit.Name : null,
					stopwatch.ElapsedMilliseconds
				}));
				stopwatch.Restart();
				bool flag3 = woWUnit != null && this._predicate(this._action, woWUnit);
				if (flag3)
				{
					stopwatch.Stop();
					RotationLogger.Trace(string.Format("({0}) predicate ({1}): on {2} {3} ms", new object[]
					{
						text,
						this._targetFinder.Method.Name,
						woWUnit.Name,
						stopwatch.ElapsedMilliseconds
					}));
					stopwatch.Restart();
					bool flag4 = this._action.Execute(woWUnit, this._forceCast);
					stopwatch.Stop();
					RotationLogger.Trace(string.Format("action ({0}): {1} ms", text, stopwatch.ElapsedMilliseconds));
					result = flag4;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0400003B RID: 59
		public readonly float Priority;

		// Token: 0x0400003C RID: 60
		private readonly RotationAction _action;

		// Token: 0x0400003D RID: 61
		private readonly Func<RotationAction, WoWUnit, bool> _predicate;

		// Token: 0x0400003E RID: 62
		private readonly Func<Func<WoWUnit, bool>, WoWUnit> _targetFinder;

		// Token: 0x0400003F RID: 63
		private readonly bool _forceCast = false;

		// Token: 0x04000040 RID: 64
		private readonly bool _checkRange = true;
	}
}
