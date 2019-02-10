using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RedisUsage.CqrsCore.Extensions;
using System;
using System.Collections.Generic;

namespace RedisUsage.UnitTest.CqrsCoreMockupTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
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
            
            public Queue<DumyTest> Queue { get; set; }

            public DumyTest MySelf { get; set; }

            public SubDumyTest SubTest { get; set; }
        }

        public class SubDumyTest
        {
            public string Name { get; set; }
            public DumyTest MySelf { get; set; }
            
            public SubDumyTest SubMySelf { get; set; }
        }
    }
}
