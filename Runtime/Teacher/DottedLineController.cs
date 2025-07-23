using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.Teacher{
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    public class DottedLineController : MonoBehaviour
    {
        #region DI
        [SerializeField]
        private LineRenderer lineRenderer;
        [SerializeField]
        private SpriteRenderer head;
        [SerializeField]
        private Shader shader;

        private bool isInit = false;
        #endregion

        private void Awake(){
            init();
        }

        private void init(){
            if(isInit == true){
                return;
            }

            this.lineRenderer.material = new Material(this.shader);
            isInit = true;
        }

        public void SetPosition(Vector3[] _positions, float _startOffset = 0f, float _endOffset = 0f, bool _reload = true){
            init();

            lineRenderer.positionCount = _positions.Length;
            
            if(_startOffset > 0){
                var _distance = Vector3.Distance(_positions[1], _positions[0]);
                var _offsetPosition = Vector3.Lerp(_positions[1], _positions[0], (_distance - _startOffset) / _distance);
                _positions[0] = _offsetPosition;
            }

            if(_endOffset > 0){
                var _count = _positions.Length;
                var _distance = Vector3.Distance(_positions[_count - 2], _positions[_count - 1]);
                var _offsetPosition = Vector3.Lerp(_positions[_count - 2], _positions[_count - 1], (_distance - _endOffset) / _distance);
                _positions[_count - 1] = _offsetPosition;
            }

            lineRenderer.SetPositions(_positions);

            if(_reload == true){
                reloadDot();
            }
        }

        public void SetSpacing(float _space, bool _reload = true){
            init();

            lineRenderer.material.SetFloat("_Spacing", _space);
            if(_reload == true){
                reloadDot();
            }
        }

        public void SetSpeed(float _speed){
            init();

            lineRenderer.material.SetFloat("_Speed", _speed);
        }

        public void SetDotSize(float _size, bool _reload = true){
            init();

            lineRenderer.endWidth = _size;
            lineRenderer.startWidth = _size;
            if(_reload == true){
                reloadDot();
            }
        }

        public void SetColor(Color _color){
            init();
            
            lineRenderer.endColor = _color;
            lineRenderer.startColor = _color;

            if(head != null){
                head.color = _color;
            }
        }

        private void reloadDot()
        {
            var _dotR = lineRenderer.startWidth;
            var _distance = 0f;
            var _positionCount = lineRenderer.positionCount;
            for(int i = 1; i < _positionCount; i++){
                _distance += Vector2.Distance(lineRenderer.GetPosition(i), lineRenderer.GetPosition(i - 1));
            }

            var _space = lineRenderer.material.GetFloat("_Spacing");
            var _repeatCount = _distance / (_dotR + _dotR * _space);  
            lineRenderer.material.SetFloat("_RepeatCount", _repeatCount);

            if(head != null){
                head.transform.localPosition = lineRenderer.GetPosition(_positionCount - 1);
                Vector2 direction = lineRenderer.GetPosition(_positionCount - 1) - lineRenderer.GetPosition(_positionCount - 2);
                float _angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                head.transform.rotation = Quaternion.Euler(0, 0, _angle);
            }
        }

        // #if UNITY_EDITOR
        // private void Update(){
        //    reloadDot();
        // }
        // #endif
    }
}