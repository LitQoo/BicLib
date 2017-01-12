using System;

namespace BigjamLibrary.BicDB
{
	public interface IStorage
	{
		void Save(Action<bool> _callback, IConvertString _table);
		void Load(Action<bool> _callback, IConvertString _table);
	}

	public interface IConvertString{
		string Name{ get; set;}
		int GetRowSize();
		IRow GetRow(int _rowIndex);
	}

}

