using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualEnumVariable<T> : DataBase, IVariable where  T : struct
	{
		#region AsValue
		private Func<T> data;
		public int AsInt{ get{ return (int)Enum.ToObject(typeof(T), data()); } set{ throwSetException(); } }
		public string AsString{ get{ return data().ToString (); } set{ throwSetException(); } }
		public float AsFloat{ get{ return (float)AsInt; } set{ throwSetException(); } }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ throwSetException(); } }
		public DataType Type { get { return DataType.Int; }}
		#endregion

		public VirtualEnumVariable() : base(){
		}

		public VirtualEnumVariable(Func<T> _func) : base(){
			data = _func;
		}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			throwSetException ();
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

		private void throwSetException(){
			throw new SystemException ("this variable not support to write");
		}
	}
}