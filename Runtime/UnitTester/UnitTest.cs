using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using UnityEditor;
using System.Runtime.CompilerServices;

[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class UnitTestAttribute : System.Attribute
{   
    #if UNITY_EDITOR
    public string Instance{get;set;} = null;
    public string Param{get;set;} = null;
    public object Result{get;set;} = null;
    public object[] Params{get;set;} = null;
    public bool IsPass{get;set;} = false;
    public UnitTestAttribute(params object[] param)
    {
        this.Params = param;
    }
    #else
    public string Instance{get=>null;set{}}
    public object Result{get=>null;set{}}
    public string Param{get=>null;set{}}
    public object[] Params{get=>null;set{}}
    public bool IsPass{get=>true;set{}}
    public UnitTestAttribute(params object[] param)
    {
    }
    #endif
}