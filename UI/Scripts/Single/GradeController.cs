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
        private GameObject[] gradeObjects;
        [SerializeField]
        private GameObject[] gradeLifeObjects;
        #endregion

        #region Data
        public IVariableReadOnly Grade {get=>this.grade;}
        public IVariableReadOnly GradeLife {get=>this.gradeLife;}

        private EncryptedIntVariable grade = new EncryptedIntVariable(1);
        private EncryptedIntVariable gradeLife = new EncryptedIntVariable(0);  
        private int[] gradeLifeMax;
        private bool isSubscribe = false;
        #endregion

        #region Logic
        private void updateGrade(IVariableReadOnly _grade)
        {
            for(int i = 0;i < gradeObjects.Length; i++){
                    gradeObjects[i].SetActive(_grade.AsInt > i);
            }

            this.gradeLife.AsInt = this.gradeLifeMax[this.grade.AsInt - 1];
        }

        private void updateGradeLife(IVariableReadOnly _gradeLife)
        {
            for(int i = 0;i < gradeLifeObjects.Length; i++){
                    gradeLifeObjects[i].SetActive(_gradeLife.AsInt > i);
            }
        }

        public void Setup(int[] _gradeLifeMax){
            this.gradeLifeMax = _gradeLifeMax;
            this.grade.AsInt = this.gradeLifeMax.Length;

            if(isSubscribe == false){
                isSubscribe = true;
                grade.Subscribe(updateGrade, true);
                gradeLife.Subscribe(updateGradeLife, true);
            }
        }

        public void DecreaseGradeLife(){
            this.gradeLife.AsInt--;
            if(this.gradeLife.AsInt == 0){
                this.grade.AsInt = Mathf.Clamp(this.grade.AsInt - 1, 1, this.gradeLifeMax.Length);
            }
        }

        public void IncreaseGrade(){
            this.grade.AsInt = Mathf.Clamp(this.grade.AsInt + 1, 1, this.gradeLifeMax.Length);
        }
        #endregion
    }
}