#if BICUTIL_IAP
using UnityEngine.Purchasing;
using BicDB.Container;
using BicDB.Variable;
using System;
using BicDB.Storage;
using BicDB.Core;

namespace BicUtil.Purchasing{
    public enum PurchasingResult{
        Complete,
        Refunded,
        NotInitialized,
        NotAvailable,
        Failed,
        Unknown
    }

    public interface IPurchasingManager<PRODUCTTYPE> where PRODUCTTYPE : struct, Enum
    {
        EnumVariable<SubscriptionStateType> SubscriptionState{get;}
        PRODUCTTYPE SubscriptionActiveID {get;set;}
        ProductModel<PRODUCTTYPE> GetProduct(PRODUCTTYPE _idType);

        void AddProduct(PRODUCTTYPE _idType, string _id, ProductType _productType, int _amount, string _defaultCurrentCode, string _defaultPriceString, float _defaultPrice, string _title, Action<IVariableReadOnly> _callback);
        void Initialize();
        void BuyProduct(PRODUCTTYPE _idType, Action<PurchasingResult> _callback);
        void RestorePurchases(Action<bool> _callback);
        void Save(Action<BicDB.Result> _callback = null, object _parameter = null);
        TableLoadData GetTableLoadData();
    }
}
#endif