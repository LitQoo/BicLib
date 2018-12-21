using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace BicUtil.InfinityNumber
{
	public class InfinityNumberVariableTest {

		[Test]
		public void CreateTest() {
			var _gold = new InfinityNumberVariable(1000, 0);

			Assert.AreEqual(_gold.Quantity, 1000);
			Assert.AreEqual(_gold.Unit, 0);
		}

		[Test]
		public void AdjustmentUnitTest(){
			{
				var _gold = new InfinityNumberVariable(9999, 0);

				Assert.AreEqual(_gold.Quantity, 9999, "test1");
				Assert.AreEqual(_gold.Unit, 0, "test1");
			}

			{
				var _gold = new InfinityNumberVariable(1000000, 0);

				Assert.AreEqual(_gold.Quantity, 1000, "test2");
				Assert.AreEqual(_gold.Unit, 1, "test2");
			}

			{
				var _gold = new InfinityNumberVariable(999, 1);

				Assert.AreEqual(_gold.Quantity, 999000, "test3");
				Assert.AreEqual(_gold.Unit, 0, "test3");
			}
		}

		[Test]
		public void AddTest(){
			int testNo = 1;
			{
				var _gold = new InfinityNumberVariable(1000, 0);

				_gold.Add(1000, 0);

				Assert.AreEqual(_gold.Quantity, 2000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "2K");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(1000, 1);

				_gold.Add(1000, 0);

				Assert.AreEqual(_gold.Quantity, 1001, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 1, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1.001M");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(1000, 1);

				_gold.Add(1000, 2);

				Assert.AreEqual(_gold.Quantity, 1001, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1.001B");
				testNo++;
			}
		}

		[Test]
		public void SubTest(){
			int testNo = 1;
			{
				var _gold = new InfinityNumberVariable(1000, 0);

				_gold.Sub(1000, 0);

				Assert.AreEqual(_gold.Quantity, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "0");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(1000, 1);

				_gold.Sub(1000, 0);

				Assert.AreEqual(_gold.Quantity, 999000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "999K");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(2001, 1);

				_gold.Sub(1001, 2);

				Assert.AreEqual(_gold.Quantity, -999000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 1, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "-999M");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(-2001, 1);

				Assert.AreEqual(_gold.ToString(), "-2.001M");
				testNo++;
			}
		}


		[Test]
		public void MultiplyTest(){
			int testNo = 1;
			{
				var _gold = new InfinityNumberVariable(1000, 0);

				_gold.Multiply(1000, 0);

				Assert.AreEqual(_gold.Quantity, 1000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 1, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1M");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(1000, 1);

				_gold.Multiply(1000, 0);

				Assert.AreEqual(_gold.Quantity, 1000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1B");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(2001, 1);

				_gold.Multiply(1001, 2);

				Assert.AreEqual(_gold.Quantity, 2003, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 4, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "2.003bb");
				testNo++;
			}
		}

		[Test]
		public void MultiplyTest2(){
			int testNo = 1;
			{
				var _gold = new InfinityNumberVariable(1000, 0);

				_gold.Multiply(3.5f);

				Assert.AreEqual(_gold.Quantity, 3500, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "3.500K");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(1000, 1);

				_gold.Multiply(0.01f);

				Assert.AreEqual(_gold.Quantity, 10000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "10K");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(2001, 1);

				_gold.Multiply(2000.001f);

				Assert.AreEqual(_gold.Quantity, 4002, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "4.002B");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(2001, 1);

				_gold.Multiply(9000000000);

				Assert.AreEqual(_gold.Quantity, 18008, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 4, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "18.008bb");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(100000, 0);

				_gold.Multiply(0.0002f);
				
				Assert.AreEqual(_gold.Quantity, 19, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "19");
				testNo++;
			}
		}

		[Test]
		public void SetByStringTest(){
			int testNo = 1;
			{
				var _gold = new InfinityNumberVariable(0, 0);

				_gold.SetByString("3.500 K");

				Assert.AreEqual(_gold.Quantity, 3500, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "3.500K");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(0, 0);

				_gold.SetByString("10 K");

				Assert.AreEqual(_gold.Quantity, 10000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "10K");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(0, 0);

				_gold.SetByString("4.002 B");

				Assert.AreEqual(_gold.Quantity, 4002, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "4.002B");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(0, 0);

				_gold.SetByString("18.008 bb");

				Assert.AreEqual(_gold.Quantity, 18008, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 4, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "18.008bb");
				testNo++;
			}

			{
				var _gold = new InfinityNumberVariable(0, 0);

				_gold.SetByString("19");

				Assert.AreEqual(_gold.Quantity, 19, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "19");
				testNo++;
			}
		}
        
        [Test]
        public void onchangednotifyteset(){
            var _gold = new InfinityNumberVariable(0, 0);
            
            bool _isCalledNotify = false;
            _gold.OnChangedValueActions += _variable=>{
                _isCalledNotify = true;
            };

            _gold.SetByString("19");
            Assert.AreEqual(_isCalledNotify, true);

            _isCalledNotify = false;
            _gold.Add(1, 1);
            Assert.AreEqual(_isCalledNotify, true);

            _isCalledNotify = false;
            _gold.Multiply(1, 1);
            Assert.AreEqual(_isCalledNotify, true);

            _isCalledNotify = false;
            _gold.Sub(1, 1);
            Assert.AreEqual(_isCalledNotify, true);

            _isCalledNotify = false;
            _gold.Set(1, 1);
            Assert.AreEqual(_isCalledNotify, true);
        }

        [Test]
        public void toJsonTest(){
            var _gold = new InfinityNumberVariable(100, 2);

			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
            _gold.BuildFormattedString(_stringBuilder, BicUtil.Json.JsonConvertor.GetInstance());

            Assert.AreEqual("100 M", _stringBuilder.ToString());
        }
        
        [Test]
        public void fromJsonTest(){
            var _gold = new InfinityNumberVariable(100, 2);

            string _json = "\"100.100 B\"";
            int _counter = 0;
            _gold.BuildVariable(ref _json, ref _counter, BicUtil.Json.JsonConvertor.GetInstance());

            Assert.AreEqual("100.100B", _gold.ToString());
        }

	}
}