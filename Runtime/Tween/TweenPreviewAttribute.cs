[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class TweenPreviewAttribute : System.Attribute
{
	public string Name{
		get;set;
	}

	public object[] Params{
		get;
		set;
	}
	
	public TweenPreviewAttribute(string _name)
	{
		Name = _name;
	}
}