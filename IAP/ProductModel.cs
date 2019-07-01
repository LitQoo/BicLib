#if BICUTIL_IAP
using BicDB.Container;
using BicDB.Variable;
using UnityEngine.Purchasing;
using System;

namespace BicUtil.Purchasing
{
    public class ProductModel<PRODUCTENUM> : RecordContainer where PRODUCTENUM : struct{
        #region const
        public const string KEY_PRIMARY = "idType";
        #endregion

        #region Field
        public StringVariable Id = new StringVariable();
        public EnumVariable<PRODUCTENUM> IdType = new EnumVariable<PRODUCTENUM>();
        public EnumVariable<ProductType> ProductType = new EnumVariable<ProductType>();
        public EncryptedIntVariable PurchaseCount = new EncryptedIntVariable();
        public StringVariable Recipt = new StringVariable();
        public EncryptedIntVariable Amount = new EncryptedIntVariable();
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

        #endregion

        #region InstantData
        public StringVariable CurrencyCode = new StringVariable();
        public StringVariable PriceString = new StringVariable();
        public StringVariable Title = new StringVariable();
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
            Recipt.AsString = string.Empty;

            init();
        }

        private void init(){
            AddManagedColumn (KEY_PRIMARY, IdType);
            AddManagedColumn ("id", Id);
            AddManagedColumn ("value", PurchaseCount);
            AddManagedColumn ("recipt", Recipt);
        }
        #endregion
    }

}
#endif