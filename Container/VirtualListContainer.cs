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
		private IList<T> _data;
		override protected IList<T> data {
			get { 
				_data = Getter ();
				return _data;
			}

			set { 
				_data = value;
				Setter (_data);
			}
		}
			
		private Func<IList<T>> Getter;
		private Action<IList<T>> Setter;

		public VirtualListContainer(Func<IList<T>> _getter = null, Action<IList<T>> _setter = null) : base(){
			Getter = _getter;
			Setter = _setter;
		}
	}
}