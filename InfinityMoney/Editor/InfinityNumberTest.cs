using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace BicUtil.InfinityNumber
{
	public class InfinityNumberTest {

		[Test]
		public void CreateTest() {
			var _gold = new InfinityNumber(1000, 0);

			Assert.AreEqual(_gold.Quantity, 1000);
			Assert.AreEqual(_gold.Unit, 0);
		}

		[Test]
		public void AdjustmentUnitTest(){
			{
				var _gold = new InfinityNumber(9999, 0);

				Assert.AreEqual(_gold.Quantity, 9999, "test1");
				Assert.AreEqual(_gold.Unit, 0, "test1");
			}

			{
				var _gold = new InfinityNumber(1000000, 0);

				Assert.AreEqual(_gold.Quantity, 1000, "test2");
				Assert.AreEqual(_gold.Unit, 1, "test2");
			}

			{
				var _gold = new InfinityNumber(999, 1);

				Assert.AreEqual(_gold.Quantity, 999000, "test3");
				Assert.AreEqual(_gold.Unit, 0, "test3");
			}
		}

		[Test]
		public void AddTest(){
			int testNo = 1;
			{
				var _gold = new InfinityNumber(1000, 0);

				_gold.Add(1000, 0);

				Assert.AreEqual(_gold.Quantity, 2000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "2K");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(1000, 1);

				_gold.Add(1000, 0);

				Assert.AreEqual(_gold.Quantity, 1001, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 1, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1.001M");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(1000, 1);

				_gold.Add(1000, 2);

				Assert.AreEqual(_gold.Quantity, 1001, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1.001B");
				testNo++;
			}
		}


		[Test]
		public void AddTest2(){
			int testNo = 1;
			{
				var _gold = new InfinityNumber(1000, 0);

				_gold.Add("1000");

				Assert.AreEqual(_gold.Quantity, 2000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "2K");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(1000, 1);

				_gold.Add("1000");

				Assert.AreEqual(_gold.Quantity, 1001, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 1, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1.001M");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(1000, 1);

				_gold.Add("1000 M");

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
				var _gold = new InfinityNumber(1000, 0);

				_gold.Sub(1000, 0);

				Assert.AreEqual(_gold.Quantity, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "0");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(1000, 1);

				_gold.Sub(1000, 0);

				Assert.AreEqual(_gold.Quantity, 999000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "999K");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(2001, 1);

				_gold.Sub(1001, 2);

				Assert.AreEqual(_gold.Quantity, -999000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 1, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "-999M");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(-2001, 1);

				Assert.AreEqual(_gold.ToString(), "-2.001M");
				testNo++;
			}
		}
		[Test]
		public void SubTest2(){
			int testNo = 1;
			{
				var _gold = new InfinityNumber(1000, 0);

				_gold.Sub("1000");

				Assert.AreEqual(_gold.Quantity, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "0");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(1000, 1);

				_gold.Sub("1000");

				Assert.AreEqual(_gold.Quantity, 999000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "999K");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(2001, 1);

				_gold.Sub("1001 M");

				Assert.AreEqual(_gold.Quantity, -999000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 1, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "-999M");
				testNo++;
			}
		}


		[Test]
		public void MultiplyTest(){
			int testNo = 1;
			{
				var _gold = new InfinityNumber(1000, 0);

				_gold.Multiply(1000, 0);

				Assert.AreEqual(_gold.Quantity, 1000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 1, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1M");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(1000, 1);

				_gold.Multiply(1000, 0);

				Assert.AreEqual(_gold.Quantity, 1000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "1B");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(2001, 1);

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
				var _gold = new InfinityNumber(1000, 0);

				_gold.Multiply(3.5f);

				Assert.AreEqual(_gold.Quantity, 3500, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "3.500K");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(1000, 1);

				_gold.Multiply(0.01f);

				Assert.AreEqual(_gold.Quantity, 10000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "10K");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(2001, 1);

				_gold.Multiply(2000.001f);

				Assert.AreEqual(_gold.Quantity, 4002, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "4.002B");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(2001, 1);

				_gold.Multiply(9000000000);

				Assert.AreEqual(_gold.Quantity, 18008, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 4, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "18.008bb");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(100000, 0);

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
				var _gold = new InfinityNumber(0, 0);

				_gold.SetByString("3.500 K");

				Assert.AreEqual(_gold.Quantity, 3500, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "3.500K");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(0, 0);

				_gold.SetByString("10 K");

				Assert.AreEqual(_gold.Quantity, 10000, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 0, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "10K");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(0, 0);

				_gold.SetByString("4.002 B");

				Assert.AreEqual(_gold.Quantity, 4002, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "4.002B");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(0, 0);

				_gold.SetByString("18.008 bb");

				Assert.AreEqual(_gold.Quantity, 18008, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 4, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "18.008bb");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(0, 0);

				_gold.SetByString("100.100 B");

				Assert.AreEqual(_gold.Quantity, 100100, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "100.100B");
				testNo++;
			}

			{
				var _gold = new InfinityNumber(0, 0);

				_gold.SetByString("-100.100 B");

				Assert.AreEqual(_gold.Quantity, -100100, "test" + testNo.ToString());
				Assert.AreEqual(_gold.Unit, 2, "test" + testNo.ToString());
				Assert.AreEqual(_gold.ToString(), "-100.100B");
				testNo++;
			}
		}


		[Test]
		public void IsGreaterTest(){
			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(2000, 1);
				
				Assert.AreEqual(_c2 > _c1, true);
			}

			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(1000, 1);

				Assert.AreEqual(_c2 > _c1, false);
			}

			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(100, 2);

				Assert.AreEqual(_c2 > _c1, true);
			}

			{
				var _c1 = new InfinityNumber(10, 2);
				var _c2 = new InfinityNumber(1000, 1);

				Assert.AreEqual(_c2 > _c1, false);
			}

			{
				var _c1 = new InfinityNumber("1000 M");
				var _c2 = new InfinityNumber("2 B");

				Assert.AreEqual(_c2 > _c1, true);
			}

			{
				var _c1 = new InfinityNumber("1000 M");
				var _c2 = new InfinityNumber("1 B");

				Assert.AreEqual(_c2 > _c1, false);
				Assert.AreEqual(_c1 > _c2, false);
			}
			
		}

		[Test]
		public void IsLowerTest(){
			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(2000, 1);

				Assert.AreEqual(_c2 < _c1, false);
			}

			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(1000, 1);

				Assert.AreEqual(_c2 < _c1, false);
			}

			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(100, 2);

				Assert.AreEqual(_c2 < _c1, false);
			}

			{
				var _c1 = new InfinityNumber(10, 2);
				var _c2 = new InfinityNumber(1000, 1);

				Assert.AreEqual(_c2 < _c1, true);
			}

			{
				var _c1 = new InfinityNumber("1000 M");
				var _c2 = new InfinityNumber("2 B");

				Assert.AreEqual(_c2 < _c1, false);
			}

			{
				var _c1 = new InfinityNumber("1000 M");
				var _c2 = new InfinityNumber("1 B");

				Assert.AreEqual(_c2 < _c1, false);
				Assert.AreEqual(_c1 < _c2, false);
			}
		}


		[Test]
		public void IsSameTest(){
			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(2000, 1);
				Assert.AreEqual(_c1 >= _c2, false);
				Assert.AreEqual(_c1 <= _c2, true);
			}

			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(1000, 1);
				Assert.AreEqual(_c1 >= _c2, true);
				Assert.AreEqual(_c1 <= _c2, true);
			}

			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(100, 2);
				Assert.AreEqual(_c1 >= _c2, false);
				Assert.AreEqual(_c1 <= _c2, true);
			}

			{
				var _c1 = new InfinityNumber("1001 M");
				var _c2 = new InfinityNumber("1 B");
				Assert.AreEqual(_c1 >= _c2, true);
				Assert.AreEqual(_c1 <= _c2, false);
			}

			{
				var _c1 = new InfinityNumber("1000 M");
				var _c2 = new InfinityNumber("1 B");
				Assert.AreEqual(_c1 >= _c2, true);
				Assert.AreEqual(_c1 <= _c2, true);
			}
		}

		[Test]
		public void divisionTest(){
			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(2000, 1);

				Assert.AreEqual(_c2/_c1, 2f);
				Assert.AreEqual(_c1/_c2, 0.5f);
			}

			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(2000, 0);

				Assert.AreEqual(_c2/_c1, 0.002f);
				Assert.AreEqual(_c1/_c2, 500f);
			}

			{
				var _c1 = new InfinityNumber(1000, 1);
				var _c2 = new InfinityNumber(2500, 3);

				Assert.AreEqual(_c2/_c1, 2500000f);
			}
		}

	}
}