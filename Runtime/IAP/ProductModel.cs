#if BICUTIL_IAP
using BicDB.Container;
using BicDB.Variable;
using UnityEngine.Purchasing;
using System;

namespace BicUtil.Purchasing
{
    public class ProductModel<PRODUCTENUM> : ProductModelBase where PRODUCTENUM : struct, Enum{
        #region const
        public const string KEY_PRIMARY = "idType";
        #endregion

        #region Field
        public EnumVariable<PRODUCTENUM> IdType = new EnumVariable<PRODUCTENUM>();
        #endregion


        #region LifeCycle
        public ProductModel(){
            init();
        }

        public ProductModel(PRODUCTENUM _idType, string _id, ProductType _productType, int _amount){
            Id.AsString = _id;
            IdType.AsEnum = _idType;
            ProductType.AsEnum = _productType;
            PurchaseCount.AsInt = 0;
            Amount.AsInt = _amount;
            LastRecipt.AsString = string.Empty;

            init();
        }

        private void init(){
            AddManagedColumn (KEY_PRIMARY, IdType);
            AddManagedColumn ("id", Id);
            AddManagedColumn ("value", PurchaseCount);
            AddManagedColumn ("recipt", LastRecipt);
            AddManagedColumn ("reciptList", ReciptList);
            AddManagedColumn ("type", ProductType);
        }

        public void AddRecipt(string _recipt){
            ReciptList.Add(new StringVariable(_recipt));
            LastRecipt.AsString = _recipt;
        }
        #endregion
    }

    public abstract class ProductModelBase : RecordContainer{
        
        #region InstantData
        public StringVariable CurrencyCode = new StringVariable();
        public StringVariable PriceString = new StringVariable();
        public StringVariable Price = new StringVariable();
        public StringVariable Title = new StringVariable();
        public EncryptedIntVariable Amount = new EncryptedIntVariable();
        #endregion

        public StringVariable Id = new StringVariable();
        public EnumVariable<ProductType> ProductType = new EnumVariable<ProductType>();
        public EncryptedIntVariable PurchaseCount = new EncryptedIntVariable();
        public StringVariable LastRecipt = new StringVariable();
        public ListContainer<StringVariable> ReciptList = new ListContainer<StringVariable>();
        public IDs StoreIds{
            get{
                return new IDs(){{ Id.AsString, AppleAppStore.Name },{ Id.AsString,  GooglePlay.Name },};
            }
        }

        [Obsolete]
        public EncryptedIntVariable Value{
            get{
                return PurchaseCount;
            }
        }
    }

}
#endif