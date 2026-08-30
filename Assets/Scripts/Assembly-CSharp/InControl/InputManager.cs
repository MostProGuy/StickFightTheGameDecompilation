using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace InControl
{
	public class InputManager
	{
		public static readonly VersionInfo Version = VersionInfo.InControlVersion();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action OnSetup__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<ulong, float> OnUpdate__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action OnReset__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<InputDevice> OnDeviceAttached__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<InputDevice> OnDeviceDetached__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<InputDevice> OnActiveDeviceChanged__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<ulong, float> OnUpdateDevices__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static Action<ulong, float> OnCommitDevices__BackingField;

		private static List<InputDeviceManager> deviceManagers = new List<InputDeviceManager>();

		private static Dictionary<Type, InputDeviceManager> deviceManagerTable = new Dictionary<Type, InputDeviceManager>();

		private static InputDevice activeDevice = InputDevice.Null;

		private static List<InputDevice> devices = new List<InputDevice>();

		private static List<PlayerActionSet> playerActionSets = new List<PlayerActionSet>();

		public static ReadOnlyCollection<InputDevice> Devices;

		private static bool applicationIsFocused;

		private static float initialTime;

		private static float currentTime;

		private static float lastUpdateTime;

		private static ulong currentTick;

		private static VersionInfo? unityVersion;

		private static bool enabled;

		public static bool CommandWasPressed { get; private set; }

		public static bool InvertYAxis { get; set; }

		public static bool IsSetup { get; private set; }

		internal static string Platform { get; private set; }

		[Obsolete("Use InputManager.CommandWasPressed instead.")]
		public static bool MenuWasPressed
		{
			get
			{
				return CommandWasPressed;
			}
		}

		public static bool AnyKeyIsPressed
		{
			get
			{
				return KeyCombo.Detect(true).IncludeCount > 0;
			}
		}

		public static InputDevice ActiveDevice
		{
			get
			{
				return (activeDevice != null) ? activeDevice : InputDevice.Null;
			}
			private set
			{
				activeDevice = ((value != null) ? value : InputDevice.Null);
			}
		}

		public static bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (enabled != value)
				{
					if (value)
					{
						SetZeroTickOnAllControls();
						UpdateInternal();
					}
					else
					{
						ClearInputState();
						SetZeroTickOnAllControls();
					}
					enabled = value;
				}
			}
		}

		public static bool SuspendInBackground { get; internal set; }

		public static bool EnableNativeInput { get; internal set; }

		public static bool EnableXInput { get; internal set; }

		public static uint XInputUpdateRate { get; internal set; }

		public static uint XInputBufferSize { get; internal set; }

		public static bool NativeInputEnableXInput { get; internal set; }

		public static bool NativeInputPreventSleep { get; internal set; }

		public static uint NativeInputUpdateRate { get; internal set; }

		public static bool EnableICade { get; internal set; }

		internal static VersionInfo UnityVersion
		{
			get
			{
				if (!unityVersion.HasValue)
				{
					unityVersion = VersionInfo.UnityVersion();
				}
				return unityVersion.Value;
			}
		}

		internal static ulong CurrentTick
		{
			get
			{
				return currentTick;
			}
		}

		public static event Action OnSetup
		{
			add
			{
				Action action = OnSetup__BackingField;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnSetup__BackingField, (Action)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action action = OnSetup__BackingField;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnSetup__BackingField, (Action)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public static event Action<ulong, float> OnUpdate
		{
			add
			{
				Action<ulong, float> action = OnUpdate__BackingField;
				Action<ulong, float> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnUpdate__BackingField, (Action<ulong, float>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<ulong, float> action = OnUpdate__BackingField;
				Action<ulong, float> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnUpdate__BackingField, (Action<ulong, float>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public static event Action OnReset
		{
			add
			{
				Action action = OnReset__BackingField;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnReset__BackingField, (Action)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action action = OnReset__BackingField;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnReset__BackingField, (Action)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public static event Action<InputDevice> OnDeviceAttached
		{
			add
			{
				Action<InputDevice> action = OnDeviceAttached__BackingField;
				Action<InputDevice> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnDeviceAttached__BackingField, (Action<InputDevice>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<InputDevice> action = OnDeviceAttached__BackingField;
				Action<InputDevice> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnDeviceAttached__BackingField, (Action<InputDevice>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public static event Action<InputDevice> OnDeviceDetached
		{
			add
			{
				Action<InputDevice> action = OnDeviceDetached__BackingField;
				Action<InputDevice> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnDeviceDetached__BackingField, (Action<InputDevice>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<InputDevice> action = OnDeviceDetached__BackingField;
				Action<InputDevice> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnDeviceDetached__BackingField, (Action<InputDevice>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		public static event Action<InputDevice> OnActiveDeviceChanged
		{
			add
			{
				Action<InputDevice> action = OnActiveDeviceChanged__BackingField;
				Action<InputDevice> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnActiveDeviceChanged__BackingField, (Action<InputDevice>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<InputDevice> action = OnActiveDeviceChanged__BackingField;
				Action<InputDevice> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnActiveDeviceChanged__BackingField, (Action<InputDevice>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		internal static event Action<ulong, float> OnUpdateDevices
		{
			add
			{
				Action<ulong, float> action = OnUpdateDevices__BackingField;
				Action<ulong, float> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnUpdateDevices__BackingField, (Action<ulong, float>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<ulong, float> action = OnUpdateDevices__BackingField;
				Action<ulong, float> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnUpdateDevices__BackingField, (Action<ulong, float>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		internal static event Action<ulong, float> OnCommitDevices
		{
			add
			{
				Action<ulong, float> action = OnCommitDevices__BackingField;
				Action<ulong, float> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnCommitDevices__BackingField, (Action<ulong, float>)Delegate.Combine(action2, value), action);
				}
				while ((object)action != action2);
			}
			remove
			{
				Action<ulong, float> action = OnCommitDevices__BackingField;
				Action<ulong, float> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange(ref OnCommitDevices__BackingField, (Action<ulong, float>)Delegate.Remove(action2, value), action);
				}
				while ((object)action != action2);
			}
		}

		[Obsolete("Calling InputManager.Setup() directly is no longer supported. Use the InControlManager component to manage the lifecycle of the input manager instead.", true)]
		public static void Setup()
		{
			SetupInternal();
		}

		internal static bool SetupInternal()
		{
			if (IsSetup)
			{
				return false;
			}
			Platform = Utility.GetWindowsVersion().ToUpper();
			enabled = true;
			initialTime = 0f;
			currentTime = 0f;
			lastUpdateTime = 0f;
			currentTick = 0uL;
			applicationIsFocused = true;
			deviceManagers.Clear();
			deviceManagerTable.Clear();
			devices.Clear();
			Devices = new ReadOnlyCollection<InputDevice>(devices);
			activeDevice = InputDevice.Null;
			playerActionSets.Clear();
			IsSetup = true;
			bool flag = true;
			if (EnableNativeInput && NativeInputDeviceManager.Enable())
			{
				flag = false;
			}
			if (EnableXInput && flag)
			{
				XInputDeviceManager.Enable();
			}
			if (OnSetup__BackingField != null)
			{
				OnSetup__BackingField();
				OnSetup__BackingField = null;
			}
			if (flag)
			{
				AddDeviceManager<UnityInputDeviceManager>();
			}
			return true;
		}

		[Obsolete("Calling InputManager.Reset() method directly is no longer supported. Use the InControlManager component to manage the lifecycle of the input manager instead.", true)]
		public static void Reset()
		{
			ResetInternal();
		}

		internal static void ResetInternal()
		{
			if (OnReset__BackingField != null)
			{
				OnReset__BackingField();
			}
			OnSetup__BackingField = null;
			OnUpdate__BackingField = null;
			OnReset__BackingField = null;
			OnActiveDeviceChanged__BackingField = null;
			OnDeviceAttached__BackingField = null;
			OnDeviceDetached__BackingField = null;
			OnUpdateDevices__BackingField = null;
			OnCommitDevices__BackingField = null;
			DestroyDeviceManagers();
			DestroyDevices();
			playerActionSets.Clear();
			IsSetup = false;
		}

		[Obsolete("Calling InputManager.Update() directly is no longer supported. Use the InControlManager component to manage the lifecycle of the input manager instead.", true)]
		public static void Update()
		{
			UpdateInternal();
		}

		internal static void UpdateInternal()
		{
			AssertIsSetup();
			if (OnSetup__BackingField != null)
			{
				OnSetup__BackingField();
				OnSetup__BackingField = null;
			}
			if (enabled && (!SuspendInBackground || applicationIsFocused))
			{
				currentTick++;
				UpdateCurrentTime();
				float num = currentTime - lastUpdateTime;
				UpdateDeviceManagers(num);
				CommandWasPressed = false;
				UpdateDevices(num);
				CommitDevices(num);
				UpdateActiveDevice();
				UpdatePlayerActionSets(num);
				if (OnUpdate__BackingField != null)
				{
					OnUpdate__BackingField(currentTick, num);
				}
				lastUpdateTime = currentTime;
			}
		}

		public static void Reload()
		{
			ResetInternal();
			SetupInternal();
		}

		private static void AssertIsSetup()
		{
			if (!IsSetup)
			{
				throw new Exception("InputManager is not initialized. Call InputManager.Setup() first.");
			}
		}

		private static void SetZeroTickOnAllControls()
		{
			int count = devices.Count;
			for (int i = 0; i < count; i++)
			{
				ReadOnlyCollection<InputControl> controls = devices[i].Controls;
				int count2 = controls.Count;
				for (int j = 0; j < count2; j++)
				{
					InputControl inputControl = controls[j];
					if (inputControl != null)
					{
						inputControl.SetZeroTick();
					}
				}
			}
		}

		public static void ClearInputState()
		{
			int count = devices.Count;
			for (int i = 0; i < count; i++)
			{
				devices[i].ClearInputState();
			}
			int count2 = playerActionSets.Count;
			for (int j = 0; j < count2; j++)
			{
				playerActionSets[j].ClearInputState();
			}
			activeDevice = InputDevice.Null;
		}

		internal static void OnApplicationFocus(bool focusState)
		{
			if (!focusState)
			{
				if (SuspendInBackground)
				{
					ClearInputState();
				}
				SetZeroTickOnAllControls();
			}
			applicationIsFocused = focusState;
		}

		internal static void OnApplicationPause(bool pauseState)
		{
		}

		internal static void OnApplicationQuit()
		{
			ResetInternal();
		}

		internal static void OnLevelWasLoaded()
		{
			SetZeroTickOnAllControls();
			UpdateInternal();
		}

		public static void AddDeviceManager(InputDeviceManager deviceManager)
		{
			AssertIsSetup();
			Type type = deviceManager.GetType();
			if (deviceManagerTable.ContainsKey(type))
			{
				Logger.LogError("A device manager of type '" + type.Name + "' already exists; cannot add another.");
				return;
			}
			deviceManagers.Add(deviceManager);
			deviceManagerTable.Add(type, deviceManager);
			deviceManager.Update(currentTick, currentTime - lastUpdateTime);
		}

		public static void AddDeviceManager<T>() where T : InputDeviceManager, new()
		{
			AddDeviceManager(new T());
		}

		public static T GetDeviceManager<T>() where T : InputDeviceManager
		{
			InputDeviceManager value;
			if (deviceManagerTable.TryGetValue(typeof(T), out value))
			{
				return value as T;
			}
			return (T)null;
		}

		public static bool HasDeviceManager<T>() where T : InputDeviceManager
		{
			return deviceManagerTable.ContainsKey(typeof(T));
		}

		private static void UpdateCurrentTime()
		{
			if (initialTime < float.Epsilon)
			{
				initialTime = Time.realtimeSinceStartup;
			}
			currentTime = Mathf.Max(0f, Time.realtimeSinceStartup - initialTime);
		}

		private static void UpdateDeviceManagers(float deltaTime)
		{
			int count = deviceManagers.Count;
			for (int i = 0; i < count; i++)
			{
				deviceManagers[i].Update(currentTick, deltaTime);
			}
		}

		private static void DestroyDeviceManagers()
		{
			int count = deviceManagers.Count;
			for (int i = 0; i < count; i++)
			{
				deviceManagers[i].Destroy();
			}
			deviceManagers.Clear();
			deviceManagerTable.Clear();
		}

		private static void DestroyDevices()
		{
			int count = devices.Count;
			for (int i = 0; i < count; i++)
			{
				InputDevice inputDevice = devices[i];
				inputDevice.OnDetached();
			}
			devices.Clear();
			activeDevice = InputDevice.Null;
		}

		private static void UpdateDevices(float deltaTime)
		{
			int count = devices.Count;
			for (int i = 0; i < count; i++)
			{
				InputDevice inputDevice = devices[i];
				inputDevice.Update(currentTick, deltaTime);
			}
			if (OnUpdateDevices__BackingField != null)
			{
				OnUpdateDevices__BackingField(currentTick, deltaTime);
			}
		}

		private static void CommitDevices(float deltaTime)
		{
			int count = devices.Count;
			for (int i = 0; i < count; i++)
			{
				InputDevice inputDevice = devices[i];
				inputDevice.Commit(currentTick, deltaTime);
				if (inputDevice.CommandWasPressed)
				{
					CommandWasPressed = true;
				}
			}
			if (OnCommitDevices__BackingField != null)
			{
				OnCommitDevices__BackingField(currentTick, deltaTime);
			}
		}

		private static void UpdateActiveDevice()
		{
			InputDevice inputDevice = ActiveDevice;
			int count = devices.Count;
			for (int i = 0; i < count; i++)
			{
				InputDevice inputDevice2 = devices[i];
				if (inputDevice2.LastChangedAfter(ActiveDevice))
				{
					ActiveDevice = inputDevice2;
				}
			}
			if (inputDevice != ActiveDevice && OnActiveDeviceChanged__BackingField != null)
			{
				OnActiveDeviceChanged__BackingField(ActiveDevice);
			}
		}

		public static void AttachDevice(InputDevice inputDevice)
		{
			AssertIsSetup();
			if (!inputDevice.IsSupportedOnThisPlatform || inputDevice.IsAttached)
			{
				return;
			}
			if (!devices.Contains(inputDevice))
			{
				devices.Add(inputDevice);
				devices.Sort((InputDevice d1, InputDevice d2) => d1.SortOrder.CompareTo(d2.SortOrder));
			}
			inputDevice.OnAttached();
			if (OnDeviceAttached__BackingField != null)
			{
				OnDeviceAttached__BackingField(inputDevice);
			}
		}

		public static void DetachDevice(InputDevice inputDevice)
		{
			if (IsSetup && inputDevice.IsAttached)
			{
				devices.Remove(inputDevice);
				if (ActiveDevice == inputDevice)
				{
					ActiveDevice = InputDevice.Null;
				}
				inputDevice.OnDetached();
				if (OnDeviceDetached__BackingField != null)
				{
					OnDeviceDetached__BackingField(inputDevice);
				}
			}
		}

		public static void HideDevicesWithProfile(Type type)
		{
			if (type.IsSubclassOf(typeof(UnityInputDeviceProfile)))
			{
				InputDeviceProfile.Hide(type);
			}
		}

		internal static void AttachPlayerActionSet(PlayerActionSet playerActionSet)
		{
			if (!playerActionSets.Contains(playerActionSet))
			{
				playerActionSets.Add(playerActionSet);
			}
		}

		internal static void DetachPlayerActionSet(PlayerActionSet playerActionSet)
		{
			playerActionSets.Remove(playerActionSet);
		}

		internal static void UpdatePlayerActionSets(float deltaTime)
		{
			int count = playerActionSets.Count;
			for (int i = 0; i < count; i++)
			{
				playerActionSets[i].Update(currentTick, deltaTime);
			}
		}
	}
}
