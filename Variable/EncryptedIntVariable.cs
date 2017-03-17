using System;
using BicDB;

namespace BicDB.Container
{

	public class EncryptedIntVariable :  VariableBase, IVariable{
		#region static
		private static System.Random random = new System.Random();
		#endregion

		#region AsValue
		protected int data;
		public int AsInt{ get{ return data ^ seed; } set{ data = value ^ seed; NotifyChanged();} }
		public string AsString{ get{ return AsInt.ToString (); } set{ AsInt = int.Parse (value);} }
		public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;} }
		public DataType Type { get { return DataType.Int; }}
		#endregion

		#region member
		private int seed = 0;
		#endregion

		#region LifeCycle
		public EncryptedIntVariable() : base(){
			seed = random.Next(int.MaxValue);
		}

		public EncryptedIntVariable(int _value) : base(){
			seed = random.Next(int.MaxValue);
			AsInt = _value;
		}
		#endregion

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildNumberVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}
	}
}
