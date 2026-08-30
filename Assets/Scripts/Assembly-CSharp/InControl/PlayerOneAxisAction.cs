using System;
using System.Diagnostics;
using System.Threading;

namespace InControl
{
	public class PlayerOneAxisAction : OneAxisInputControl
	{
		private PlayerAction negativeAction;

		private PlayerAction positiveAction;

		public BindingSourceType LastInputType;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Action<BindingSourceType> OnLastInputTypeChanged__BackingField;

		public object UserData { get; set; }

		[Obsolete("Please set this property on device controls directly. It does nothing here.")]
		public new float LowerDeadZone
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		[Obsolete("Please set this property on device controls directly. It does nothing here.")]
		public new float UpperDeadZone
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public event Action<BindingSourceType> OnLastInputTypeChanged
		{
			add
			{
				Action<BindingSourceType> action = OnLastInputTypeChanged__BackingField;
				Action<BindingSourceType> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnLastInputTypeChanged__BackingField, (Action<BindingSourceType>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<BindingSourceType> action = OnLastInputTypeChanged__BackingField;
				Action<BindingSourceType> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnLastInputTypeChanged__BackingField, (Action<BindingSourceType>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		internal PlayerOneAxisAction(PlayerAction negativeAction, PlayerAction positiveAction)
		{
			this.negativeAction = negativeAction;
			this.positiveAction = positiveAction;
			Raw = true;
		}

		internal void Update(ulong updateTick, float deltaTime)
		{
			ProcessActionUpdate(negativeAction);
			ProcessActionUpdate(positiveAction);
			float value = Utility.ValueFromSides(negativeAction, positiveAction);
			CommitWithValue(value, updateTick, deltaTime);
		}

		private void ProcessActionUpdate(PlayerAction action)
		{
			BindingSourceType lastInputType = LastInputType;
			if (action.UpdateTick > base.UpdateTick)
			{
				base.UpdateTick = action.UpdateTick;
				lastInputType = action.LastInputType;
			}
			if (LastInputType != lastInputType)
			{
				LastInputType = lastInputType;
				if (OnLastInputTypeChanged__BackingField != null)
				{
					OnLastInputTypeChanged__BackingField(lastInputType);
				}
			}
		}
	}
}
