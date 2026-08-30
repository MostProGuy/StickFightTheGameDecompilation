using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Poly2Tri
{
	public struct FixedBitArray3 : IEnumerable<bool>, IEnumerable
	{
		[CompilerGenerated]
		private sealed class _003CEnumerate_003Ec__Iterator0 : IEnumerable, IEnumerable<bool>, IEnumerator, IDisposable, IEnumerator<bool>
		{
			internal int _003Ci_003E__1;

			internal FixedBitArray3 _0024this;

			internal bool _0024current;

			internal bool _0024disposing;

			internal int _0024PC;

			bool IEnumerator<bool>.Current
			{
				[DebuggerHidden]
				get
				{
					return System_002ECollections_002EGeneric_002EIEnumerator_003Cbool_003E_002Eget_Current();
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return _0024current;
				}
			}

			[DebuggerHidden]
			public _003CEnumerate_003Ec__Iterator0()
			{
			}

			public bool MoveNext()
			{
				uint num = (uint)_0024PC;
				_0024PC = -1;
				switch (num)
				{
				case 0u:
					_003Ci_003E__1 = 0;
					goto IL_0066;
				case 1u:
					_003Ci_003E__1++;
					goto IL_0066;
				default:
					{
						return false;
					}
					IL_0066:
					if (_003Ci_003E__1 < 3)
					{
						_0024current = _0024this[_003Ci_003E__1];
						if (!_0024disposing)
						{
							_0024PC = 1;
						}
						break;
					}
					_0024PC = -1;
					goto default;
				}
				return true;
			}

			[DebuggerHidden]
			private bool System_002ECollections_002EGeneric_002EIEnumerator_003Cbool_003E_002Eget_Current()
			{
				return _0024current;
			}

			[DebuggerHidden]
			public void Dispose()
			{
				_0024disposing = true;
				_0024PC = -1;
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<bool>)this).GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref _0024PC, 0, -2) == -2)
				{
					return this;
				}
				return new _003CEnumerate_003Ec__Iterator0
				{
					_0024this = _0024this
				};
			}
		}

		public bool _0;

		public bool _1;

		public bool _2;

		public bool this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return _0;
				case 1:
					return _1;
				case 2:
					return _2;
				default:
					throw new IndexOutOfRangeException();
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					_0 = value;
					break;
				case 1:
					_1 = value;
					break;
				case 2:
					_2 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public bool Contains(bool value)
		{
			for (int i = 0; i < 3; i++)
			{
				if (this[i] == value)
				{
					return true;
				}
			}
			return false;
		}

		public int IndexOf(bool value)
		{
			for (int i = 0; i < 3; i++)
			{
				if (this[i] == value)
				{
					return i;
				}
			}
			return -1;
		}

		public void Clear()
		{
			_0 = (_1 = (_2 = false));
		}

		public void Clear(bool value)
		{
			for (int i = 0; i < 3; i++)
			{
				if (this[i] == value)
				{
					this[i] = false;
				}
			}
		}

		[DebuggerHidden]
		private IEnumerable<bool> Enumerate()
		{
			//yield-return decompiler failed: Could not find currentField
			_003CEnumerate_003Ec__Iterator0 obj = new _003CEnumerate_003Ec__Iterator0();
			obj._0024this = this;
			obj._0024PC = -2;
			return obj;
		}

		public IEnumerator<bool> GetEnumerator()
		{
			return Enumerate().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
