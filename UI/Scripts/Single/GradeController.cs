using System;
using System.Collections;
using System.Collections.Generic;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.UI{
    public class GradeController : MonoBehaviour
    {
        #region DI
        [SerializeField]
        private GameObject[] perfactObjects;
        [SerializeField]
        private GameObject[] gradeObjects;
        [SerializeField]
        private GameObject[] gradeLifeObjects;
        #endregion

        #region Data
        public delegate void Effector(MarkType _markType, GameObject _markObject, bool _isActive, bool _isInitial); 
        public IVariableReadOnly Grade {get=>this.grade;}
        public IVariableReadOnly GradeLife {get=>this.gradeLife;}
        
        public bool IsPerfact{
            get{
                return this.grade.AsInt == this.gradeLifeMax.Length 
                        && this.gradeLife.AsInt == this.gradeLifeMax[this.gradeLifeMax.Length - 1];
            }
        }
        
        private Effector effector = null;
        private EncryptedIntVariable grade = new EncryptedIntVariable(1);
        private EncryptedIntVariable gradeLife = new EncryptedIntVariable(0);  
        private int[] gradeLifeMax;
        private bool isSubscribe = false;

        public enum MarkType{
            Grade,
            Life,
            Perfact
        }
        #endregion

        #region Logic
        public void Setup(int[] _gradeLifeMax, Effector _effector = null){
            this.effector = _effector;
            this.gradeLifeMax = _gradeLifeMax;
            this.grade.AsIntWithoutNotify = this.gradeLifeMax.Length;
            
            if(isSubscribe == false){
                isSubscribe = true;
                grade.Subscribe(updateGrade);
                gradeLife.Subscribe(updateGradeLife);
            }

            setupGrade(this.grade, true);
            setupGradeLife(this.gradeLife, true);
        }


        private void setupGrade(IVariableReadOnly _grade, bool _isInitial)
        {
            for(int i = 0;i < gradeObjects.Length; i++){
                changeGrade(gradeObjects[i], _grade.AsInt > i, _isInitial);
            }

            if(_isInitial == false){
                this.gradeLife.AsInt = this.gradeLifeMax[this.grade.AsInt - 1];
            }else{
                this.gradeLife.AsIntWithoutNotify = this.gradeLifeMax[this.grade.AsInt - 1];
            }
        }

        private void updateGrade(IVariableReadOnly _grade)
        {
            setupGrade(_grade, false);
        }

        private void changeGrade(GameObject _mark, bool _isActive, bool _isInitial){
            if(_mark.activeSelf != _isActive || _isInitial == true){
                if(effector == null){
                    active(_mark, _isActive);
                }else{
                    effector(MarkType.Grade, _mark, _isActive, _isInitial);
                }
            }
        }

        private void active(GameObject _mark, bool _isActive){
            _mark.SetActive(_isActive);
        }

        private void updatePerfact(bool _isInitial)
        {
            for(int i = 0;i < perfactObjects.Length; i++){
                changePerfact(perfactObjects[i], this.IsPerfact, _isInitial);
            }
        }

        private void changePerfact(GameObject _mark, bool _isActive, bool _isInitial){
            if(_mark.activeSelf != _isActive || _isInitial == true){
                if(effector == null){
                    active(_mark, _isActive);
                }else{
                    effector(MarkType.Perfact, _mark, _isActive, _isInitial);
                }
            }
        }

        private void setupGradeLife(IVariableReadOnly _gradeLife, bool _isInitial)
        {
            for(int i = 0;i < gradeLifeObjects.Length; i++){
                changeGradeLife(gradeLifeObjects[i], _gradeLife.AsInt > i, _isInitial);
            }

            updatePerfact(_isInitial);
        }

        private void updateGradeLife(IVariableReadOnly _gradeLife)
        {
            setupGradeLife(_gradeLife, false);
        }


        private void changeGradeLife(GameObject _mark, bool _isActive, bool _isInitial){
            if(_mark.activeSelf != _isActive || _isInitial == true){
                if(effector == null){
                    active(_mark, _isActive);
                }else{
                    effector(MarkType.Life, _mark, _isActive, _isInitial);
                }
            }
        }

        public void DecreaseGradeLife(){
            this.gradeLife.AsInt--;
            if(this.gradeLife.AsInt <= 0 && IsPerfact == false){
                this.grade.AsInt = Mathf.Clamp(this.grade.AsInt - 1, 1, this.gradeLifeMax.Length);
            }
        }

        public void IncreaseGrade(){
            this.grade.AsInt = Mathf.Clamp(this.grade.AsInt + 1, 1, this.gradeLifeMax.Length);
        }
        #endregion
    }
}