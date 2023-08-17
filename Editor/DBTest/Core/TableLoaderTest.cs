using Cysharp.Threading.Tasks;
using BicDB.Storage;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using System.Collections;

namespace BicDB.Core
{
    [TestFixture]
    public class TableLoaderTest
    {
        [Test]
        public async void LoadAsyncSuccessTest(){
            var _loader = new TableLoader();
            var _storage = Substitute.For<ITableStorage>();
            var _table1 = Substitute.For<ITableStorageSuppoter>();
            var _table2 = Substitute.For<ITableStorageSuppoter>();
            var _table3 = Substitute.For<ITableStorageSuppoter>();
            UnityEngine.Debug.Log("LoadAsyncTest");
            int _retry1 = 0;
            int _retry2 = 0;
            _table1.LoadAsync(Arg.Any<object>()).Returns(_callinfo=>{
                UnityEngine.Debug.Log("make table loadasync");
                return UniTask.RunOnThreadPool<Result>(()=>{
                    if(_retry1 == 0){
                        _retry1++;
                        UnityEngine.Debug.Log("table result 1"); 
                        return new Result(1);
                    }else{
                        UnityEngine.Debug.Log("table result 0");
                        return new Result(0);
                    }
                });
            });
            _table2.LoadAsync(Arg.Any<object>()).Returns(_callinfo=>{
                UnityEngine.Debug.Log("make table loadasync");
                return UniTask.RunOnThreadPool<Result>(()=>{
                    if(_retry2 <= 1){
                        _retry2++;
                        UnityEngine.Debug.Log("table result 1"); 
                        return new Result(1);
                    }else{
                        UnityEngine.Debug.Log("table result 0");
                        return new Result(0);
                    }
                });
            });
            _table3.LoadAsync(Arg.Any<object>()).Returns(_callinfo=>{
                UnityEngine.Debug.Log("make table loadasync");
                return UniTask.RunOnThreadPool<Result>(()=>{
                    UnityEngine.Debug.Log("table result 0");
                    return new Result(0);
                });
            });

            Debug.Log("addTable");
            _loader.AddTable(_table1, _storage, null, null);
            _loader.AddTable(_table2, _storage, null, null);
            _loader.AddTable(_table3, _storage, null, null);
            Debug.Log("finish addtable");

            var _result = await _loader.LoadAsync(3);
            Debug.Log("finish test");
            Assert.AreEqual(_result.IsSuccess, true);
        }

        [Test]
        public async void LoadAsyncFailTest(){
            var _loader = new TableLoader();
            var _storage = Substitute.For<ITableStorage>();
            var _table = Substitute.For<ITableStorageSuppoter>();
            UnityEngine.Debug.Log("LoadAsyncTest");
            _table.LoadAsync(Arg.Any<object>()).Returns(_callinfo=>{
                UnityEngine.Debug.Log("make table loadasync");
                return UniTask.RunOnThreadPool<Result>(()=>{
                        UnityEngine.Debug.Log("table result 1"); 
                        return new Result(1);
                });
            });

            _loader.AddTable(_table, _storage, null, null);
            var _result = await _loader.LoadAsync(3);
            Assert.AreEqual(_result.IsSuccess, false);
        }
    }
}