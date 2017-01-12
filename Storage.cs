using System;

namespace BigjamLibrary.BicDB
{
	public interface IStorage
	{
		void Save(Action<bool> _callback, IConvertString _table);
		void Load<T>(Action<bool> _callback, IConvertString _table)  where T : IModel, new();
	}

	public interface IConvertString{
		string Name{ get; set;}
		int GetRowSize();
		IModel GetRow(int _rowIndex);
	}

}

