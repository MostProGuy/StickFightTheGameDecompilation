using System;
using System.Diagnostics;
using System.Threading;

namespace InControl
{
	public class Logger
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<LogMessage> OnLogMessage__BackingField;

		public static event Action<LogMessage> OnLogMessage
		{
			add
			{
				Action<LogMessage> action = OnLogMessage__BackingField;
				Action<LogMessage> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnLogMessage__BackingField, (Action<LogMessage>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<LogMessage> action = OnLogMessage__BackingField;
				Action<LogMessage> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnLogMessage__BackingField, (Action<LogMessage>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public static void LogInfo(string text)
		{
			if (OnLogMessage__BackingField != null)
			{
				LogMessage obj = new LogMessage
				{
					text = text,
					type = LogMessageType.Info
				};
				OnLogMessage__BackingField(obj);
			}
		}

		public static void LogWarning(string text)
		{
			if (OnLogMessage__BackingField != null)
			{
				LogMessage obj = new LogMessage
				{
					text = text,
					type = LogMessageType.Warning
				};
				OnLogMessage__BackingField(obj);
			}
		}

		public static void LogError(string text)
		{
			if (OnLogMessage__BackingField != null)
			{
				LogMessage obj = new LogMessage
				{
					text = text,
					type = LogMessageType.Error
				};
				OnLogMessage__BackingField(obj);
			}
		}
	}
}
