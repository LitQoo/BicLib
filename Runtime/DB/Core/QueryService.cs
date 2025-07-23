using BicDB.Container;
using BicDB.Variable;

namespace BicDB.Core
{
    public class QueryModel : RecordContainer{
        #region Field
        public StringVariable Id = new StringVariable("");
        public IntVariable ExecCount = new IntVariable(0);
        #endregion

        #region LifeCycle
        public QueryModel(){
            AddManagedColumn("id", Id);
            AddManagedColumn("execCount", ExecCount);
        }
        #endregion
    }

    public class QueryException : System.Exception{
        public QueryExceptionType ExceptionType{get;set;}

        public QueryException(string _message, QueryExceptionType _exceptionType) : base(_message){
            this.ExceptionType = _exceptionType;
        }
    }

    public enum QueryExceptionType
    {
        NotImplemented = 0,
        SetupParse = 1,
        NotFoundQueryId = 2,
        CanNotRepeat = 3,
        Expired = 4,
        NotTargetUser = 5,
        NotFoundTable = 6,
        WhereParse = 7,
        UpdateParse = 8
    }
}