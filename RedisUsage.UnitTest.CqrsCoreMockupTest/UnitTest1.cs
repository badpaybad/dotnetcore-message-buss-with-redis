using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RedisUsage.CqrsCore.Extensions;
using System;
using System.Collections.Generic;

namespace RedisUsage.UnitTest.CqrsCoreMockupTest
{


    public class TestWrap<T>
    {
        public T wraped { get; set; }

        public string Id { get; set; }

        public TestWrap(T wrap)
        {
            wraped = wrap;
        }

        public static implicit operator TestWrap<T>(T wrap)
        {
            return new TestWrap<T>(wrap);
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            TestWrap<DumyTest> test = new TestWrap<DumyTest>(new DumyTest());

            WrapMe(test);

            WrapMe(new DumyTest());
        }

        public void WrapMe(TestWrap<DumyTest> test)
        {

        }

        [TestMethod]
        public void TestGenerateObjWithDumyData()
        {
            var obj = GenerateObjectWithDumyDataExtensions.GenerateData<DumyTest>();

            Console.WriteLine(JsonConvert.SerializeObject(obj));
        }

        public class DumyTest
        {
            public string Name { get; set; }

            public List<DumyTest> List { get; set; }

            public DumyTest MySelf { get; set; }

            public SubDumyTest SubTest { get; set; }

            public int Age { get; set; }
            public DateTime Dob { get; set; }
            public decimal Cash { get; set; }
            public double Timeout { get; set; }
            public long Year { get; set; }
            public Guid Cmnd { get; set; }
            public bool IsMale { get; set; }
            public ConsoleColor Enums { get; set; }

            public SubDumyTest[] ArrayTest { get; set; }
        }

        public class SubDumyTest
        {
            public string Name { get; set; }
            public DumyTest MySelf { get; set; }

            public SubDumyTest SubMySelf { get; set; }
        }
    }
}
