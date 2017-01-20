using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Searching;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        public static WinSearch search;
        public static string Text;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            search = new WinSearch();
            watch.Stop();
            Text = watch.Elapsed.TotalSeconds.ToString();
        }
        [TestMethod]
        public void PrintInitTime()
        {
            Trace.WriteLine($"init time for the index : {Text}");
        }

        [TestMethod]
        public void PrintTop10()
        {
            foreach (var VARIABLE in search.Output.Take(10))
            {
                Trace.WriteLine(VARIABLE);
            }
        }

        [TestMethod]
        public void ShowCount()
        {

            Trace.WriteLine($"Count {search.Output.Count}");

        }
        [TestMethod]

        public void Search()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var search = new WinSearch("Test");
            watch.Stop();
            var Text = watch.Elapsed.TotalSeconds.ToString();

            Trace.WriteLine($"init time for the index : {Text}");
            Trace.WriteLine($"Count {search.Output.Count}");
        }
    }
}
