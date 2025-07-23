using System.Collections;
using System.Collections.Generic;
using BicUtil.TableView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BicUtil.TableView
{
    public class TableCellCrossScrollSupporter : MonoBehaviour
    {
        #region DI
        [SerializeField]
        private EventTrigger eventTrigger;
        [SerializeField]
        private EventControlledScrollRect horizontalScrollView;
        [SerializeField]
        private EventControlledScrollRect verticalScrollView;
        [SerializeField]
        private UnityEvent onClickedCellAction;
        #endregion

        #region Control
        Vector2 startDragPosition = Vector2.zero;
        bool isBeginScroll = false;
        bool isCellScroll = false;

        private void Awake(){
            this.addListener(EventTriggerType.PointerDown, this.onBeginDrag);
            this.addListener(EventTriggerType.Drag, this.onDrag);
            this.addListener(EventTriggerType.PointerUp, this.onEndDrag);
        }

        private void addListener (EventTriggerType _eventType, System.Action<PointerEventData> _listener)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = _eventType;
            entry.callback.AddListener(data => _listener.Invoke((PointerEventData)data));
            eventTrigger.triggers.Add(entry);
        }

        private void onBeginDrag(PointerEventData _data){
            startDragPosition = _data.position;
            horizontalScrollView.StopMovement();
            verticalScrollView.StopMovement();
            isBeginScroll = false;
        }

        private void onDrag(PointerEventData _data){
            if(isBeginScroll == false){
                if(Mathf.Abs(_data.position.y - startDragPosition.y) > Mathf.Abs(_data.position.x - startDragPosition.x)){
                    verticalScrollView.OnBeginDrag(_data);
                    isCellScroll = false;
                }else{
                    horizontalScrollView.OnBeginDrag(_data);
                    isCellScroll = true;
                }

                isBeginScroll = true;
            }else{
                if(isCellScroll == false){
                    verticalScrollView.OnDrag(_data);
                }else{
                    horizontalScrollView.OnDrag(_data);
                }
            }
        }


        private void onEndDrag(PointerEventData _data){
            var _distance = Vector2.Distance(_data.position, startDragPosition);

            if(_distance < 15f){
                onClickedCellAction.Invoke();
            }

            if(isBeginScroll == true){
                if(isCellScroll == false){
                    verticalScrollView.OnEndDrag(_data);
                }else{
                    horizontalScrollView.OnEndDrag(_data);
                }
            }
        }
        #endregion

    }
}