using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Utility;

namespace BicDB.Container
{

	public class VirtualListContainer<T> : ListContainer<T> where T : IDataBase, new()
	{
		override protected IList<T> data {
			get { 
				return getter ();
			}

			set { 
				setter (value);
			}
		}

		private Func<IList<T>> getter;
		private Action<IList<T>> setter;

		public VirtualListContainer(Func<IList<T>> _getter, Action<IList<T>> _setter = null) : base(){
			getter = _getter;
			setter = _setter;
		}
	}
}