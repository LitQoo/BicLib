// using System;
// using System.Collections;
// using System.Collections.Generic;
// using BicDB.Core;
// using BicDB.Variable;
// using BicUtil.ClassInitializer;
// using BicUtil.Translate;
// using BicUtil.Tween;
// using BicUtil.UI;
// using BicUtil.UIFlow;
// using UnityEngine;
// using UnityEngine.UI;

// namespace BicUtil.UIFlow.Review{
//     public class ReviewPopup : ReviewPopupBase, IClassInitializerObject, IUIFlowObject
//     {
       

//         #region ClassInitialiszer
//         public void Deinitialize()
//         {
//             UIFlow.Instance.UnregisterUI(this);	
//             this.OnClose -= backUIFlow;
//         }

//         public void Initialize()
//         {
//             UIFlow.Instance.RegisterUI(this);
//             this.Init();
//             this.OnClose += backUIFlow;
//         }
//         #endregion

//         #region IUIFlowObject
//         public OnCloseUIResult OnClosedUI(IUIFlowObject _fromUI, Action _finishCallback, object _parameter)
//         {
//             return OnCloseUIResult.DoNotWait;
//         }

//         public void OnOpenedUI(IUIFlowObject _fromUI, object _paramter)
//         {
//             this.Open();
//         }
//         #endregion

//         #region Event
//         private void backUIFlow(){
//             if(UIFlow.Instance.CurrentUI == this as IUIFlowObject){
//                 UIFlow.Instance.Back(CloseMode.Disable);
//             }
//         }
//         #endregion
//     }
// }