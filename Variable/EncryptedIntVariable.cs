using System;
using BicDB;

namespace BicDB.Variable
{
	public class EncryptedIntVariable : IntVariable{
		#region static
		private static System.Random random = new System.Random();
		#endregion

		#region AsValue
		public new int AsInt{ get{ return data ^ seed; } set{ data = value ^ seed; NotifyChanged ();} }
		#endregion

		#region member
		private int seed = 0;
		#endregion

		#region LifeCycle
		public EncryptedIntVariable(int _value) : base(_value){
			seed = random.Next(int.MaxValue);
			AsInt = _value;
		}
		#endregion
	}
}
