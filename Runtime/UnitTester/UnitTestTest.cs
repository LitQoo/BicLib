using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using UnityEditor;

public class UnitTestTest : MonoBehaviour{
    public Vector2 Position{get;set;}

    [UnitTest(IsPass=true)]
    private void NoTest(){

    }

    [UnitTest(3, 4, Result=7)]
    private int add(int a, int b){
        return a+b;
    }

    [UnitTest(3, 10, Result=-7)]
    [UnitTest(100, 2, Result=98)]
    static public int sub(int a, int b){
        return a-b;
    }

    #if UNIT_TEST || UNITY_EDITOR
    static UnitTestTest      __i1()=>    new UnitTestTest{Position = new Vector2(2, 3)}; 
    static object[]     __p1()=>    new object[]{new Vector2(4,5)};
    bool                __r1()=>    this.Position == new Vector2(6,8);
    #endif
    [UnitTest(Instance="__i1", Param="__p1", Result="__r1")]
    public void move(Vector2 _pos){
        this.Position += _pos;
    }

    public override string ToString(){
        return "UnitTestTest";
    }
}
