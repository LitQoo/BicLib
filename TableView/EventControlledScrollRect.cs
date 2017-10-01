using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using BicDB.Container;
using BicDB;
using System;

namespace BicUtil.TableView
{

	public class EventControlledScrollRect : ScrollRect{

		public class DragEvent : UnityEvent { }

		public DragEvent OnBeginDragActions = new DragEvent();
		public DragEvent OnEndDragActions = new DragEvent();

		public override void OnBeginDrag (UnityEngine.EventSystems.PointerEventData eventData)
		{
			base.OnBeginDrag (eventData);
			if (OnBeginDragActions != null) {
				OnBeginDragActions.Invoke ();
			}
		}

		public override void OnEndDrag (UnityEngine.EventSystems.PointerEventData eventData)
		{
			base.OnEndDrag (eventData);
			if (OnEndDragActions != null) {
				OnEndDragActions.Invoke ();
			}
		}
	}
}
