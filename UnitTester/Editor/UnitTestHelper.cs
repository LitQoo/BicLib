using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using UnityEditor;

public class UnitTester{
    [MenuItem("UnitTest/Test")]
    [UnitTest(IsPass = true)]
    static public void Test()
    {
        #if UNITY_EDITOR
        Debug.Log("================== [Start UnitTest] ==================");
        string result = "";
        int failCount = 0;
        int okCount = 0;
        
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var _classResult = UnitTester.testClass(type);

                result += _classResult.message;
                okCount += _classResult.okCount;
                failCount += _classResult.failCount;
            }
        }

        if(failCount > 0){
            Debug.LogError("Test Result \nOK " + okCount + " / Failed " + failCount + "\n" + result);
        }else{
            Debug.Log("Test Result \nOK " + okCount + " / Failed " + failCount + "\n" + result);
        }
        
        Debug.Log("================== [Finished UnitTest] ==================");
        #else
        Debug.LogWarning("====== [Do not support unit test if not UNITY_EDITOR mode] ======");
        #endif
    }

    #if UNITY_EDITOR
    static bool         __r4((string , int, int) _r){ 
        return _r.ToString() == 
@"([Pass]UnitTestTest.NoTest(()
[OK] UnitTestTest.add(((System.Int32)3,(System.Int32)4))>>> 7==7
[OK] UnitTestTest.sub(((System.Int32)3,(System.Int32)10))>>> -7==-7
[OK] UnitTestTest.sub(((System.Int32)100,(System.Int32)2))>>> 98==98
[OK] UnitTestTest.move(((UnityEngine.Vector2)(4.0, 5.0)))
, 5, 0)";
    }
    [UnitTest(typeof(UnitTestTest), Result="__r4")]
    #endif
    
    private static (string message, int okCount, int failCount) testClass(Type type)
    {
        
        string result = "";
        int failCount = 0;
        int okCount = 0;

        // Iterate through all the methods of the class.
        foreach (MethodInfo mInfo in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static))
        {
            // Iterate through all the Attributes for each method.
            
            Attribute[] attrs = null;

            try{
                attrs = Attribute.GetCustomAttributes(mInfo);
            }catch{
                continue;
            }
            
            foreach (Attribute attr in attrs)
            {
                // Check for UnitTest
                if (attr.GetType() == typeof(UnitTestAttribute)){
                    var _testResult = testMethod(mInfo, attr as UnitTestAttribute);
                    if(_testResult.isOk == true){
                        okCount++;
                    }else{
                        failCount++;
                    }

                    result += _testResult.message+"\n";
                }
            }
        }

        return (result, okCount, failCount);
    }
    
    #if UNITY_EDITOR
    static object[] __p2()=>new object[]{new object[]{"a","b","c"}};
    [UnitTest(Param = "__p2", Result="((System.String)a,(System.String)b,(System.String)c)")]
    #endif

    private static string paramToString(object[] param){
        if(param == null){
            return "(no param)";
        }

        var _result = "";
        for(int i = 0; i < param.Length; i++){
            if(param[i] == null){
                _result+="null";
            }else{
                _result+="("+param[i].GetType().ToString() + ")" + param[i].ToString();
            }

            if(i != param.Length-1){
                _result+=",";
            }
        }

        return "("+_result+")";
    }
    
    #if UNITY_EDITOR
    static object[]     __p3(){
                                    var _method = typeof(UnitTestTest).GetMethod("add", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                    var _attr = new UnitTestAttribute(3, 4){Result = 7};
                                    return new object[]{_method, _attr};
                                }
    static bool         __r3((bool, string) _r){ return (_r.Item1==true && _r.Item2=="[OK] UnitTestTest.add(((System.Int32)3,(System.Int32)4))>>> 7==7");}
    [UnitTest(Param="__p3", Result="__r3")]
    #endif

    private static (bool isOk, string message) testMethod(MethodInfo mInfo, UnitTestAttribute attr)
    {
        if(attr.IsPass == true){
            return (true, "[Pass]" + mInfo.DeclaringType.Name + "." + mInfo.Name + "("+paramToString(attr.Params));
        }

        if(attr.Result == null){
            return (false, "[Need Setup Test]  " + mInfo.DeclaringType.Name + "." + mInfo.Name + "("+paramToString(attr.Params));
        }

        object instance;
        object _actualResult = null; 

        if(mInfo.IsStatic == true){
            instance = null;
        }else if(string.IsNullOrEmpty(attr.Instance) == true){
            instance = (object)Activator.CreateInstance(mInfo.DeclaringType);
        }else{
            instance = mInfo.DeclaringType.GetMethod(attr.Instance, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
        }
        
        if(attr.Param != null){
            attr.Params = (object[])mInfo.DeclaringType.GetMethod(attr.Param, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
        }

        try{
            _actualResult = mInfo.Invoke(instance, attr.Params);
        }catch(Exception _e){
            return (false, "[Failed]" + mInfo.DeclaringType.Name + "." + mInfo.Name + "("+paramToString(attr.Params)+") >>> method.invoke >>>" + _e.Message);
        }

        try{
            var _expectedMethod = mInfo.DeclaringType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault(_info=>_info.Name==(string)attr.Result);
            var _isOk = (bool)_expectedMethod.Invoke(instance, mInfo.ReturnType == typeof(void) ? null : new object[]{_actualResult});

            if (_isOk == true)
            {
                return (true, "[OK] "+mInfo.DeclaringType.Name + "." + mInfo.Name + "("+paramToString(attr.Params)+")");
            }
            else
            {
                return (false, "[Failed]" + mInfo.DeclaringType.Name + "." + mInfo.Name+ "("+paramToString(attr.Params)+")");
            }
        }catch{
            return getResultMessageWithValue(mInfo, _actualResult, attr);
        }
    }

    #if UNITY_EDITOR
    static object[]     __p1(){
                                    var _method = typeof(UnitTestTest).GetMethod("add", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                    var _attr = new UnitTestAttribute(3, 4){Result = 7};
                                    return new object[]{_method, 7, _attr};
                                }
    static bool         __r1((bool, string) _r){ return (_r.Item1==true && _r.Item2=="[OK] UnitTestTest.add(((System.Int32)3,(System.Int32)4))>>> 7==7");}
    [UnitTest(Param="__p1", Result="__r1")]
    #endif

    private static (bool isOk, string message) getResultMessageWithValue(MethodInfo _mInfo, object _actualResult, UnitTestAttribute attr){
        try{
            if (_actualResult.Equals(attr.Result))
            {
                return (true, "[OK] "+_mInfo.DeclaringType.Name + "." + _mInfo.Name + "("+paramToString(attr.Params)+")>>> " + _actualResult.ToString() + "==" + attr.Result.ToString());
            }
            else
            {
                return (false, "[Failed] "+_mInfo.DeclaringType.Name + "." + _mInfo.Name + "("+paramToString(attr.Params)+")>>> " + _actualResult.ToString() + "!=" + attr.Result.ToString());
            }
        }catch(Exception _e){
            return (false, "[Failed] "+_mInfo.DeclaringType.Name + "." + _mInfo.Name + "("+paramToString(attr.Params)+")>>> return check error >>> " + _e.Message);
        }
    }
}