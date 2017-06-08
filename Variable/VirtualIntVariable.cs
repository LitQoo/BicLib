using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class VirtualIntVariable : VariableBase, IVariable
	{
		#region AsValue
		private Func<int> data;
		public int AsInt{ get{ return data(); } set{throwSetException ();} }
		public string AsString{ get{ return AsInt.ToString (); } set{ throwSetException ();} }
		public float AsFloat{ get{ return (float)AsInt; } set{ throwSetException ();} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{throwSetException ();} }
		public DataType Type { get { return DataType.Int; }}
		#endregion

		#region LifeCycle
		public VirtualIntVariable() : base(){
		}

		public VirtualIntVariable(Func<int> _func) : base(){
			data = _func;
		}
		#endregion

		#region IDataBase
		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			throwSetException ();
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

		public IVariable AsVariable{ 
			get{ 
				return this;	
			} 
		}

		public D As<D>() where D : class, IDataBase{
			return this as D;
		}
		#endregion

		#region Logic
		private void throwSetException(){
			throw new SystemException ("this variable not support to write");
		}
		#endregion
	}
}