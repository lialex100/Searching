using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Searching;
using SearchingUI.Helper;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        public static WinSearch search;
        public static string Text;

        public readonly string _Line = "--------------------------------------------------";

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            search = new WinSearch(limitResult:20000);
            watch.Stop();
            Text = watch.Elapsed.TotalSeconds.ToString();
        }

        private Stopwatch watch;

        [TestInitialize]
        public void init()
        {
            watch = new Stopwatch();
            watch.Start();
        }

        [TestCleanup()]
        public void Cleanup()
        {
            watch.Stop();
            Trace.WriteLine(_Line);
            Trace.WriteLine($"Method running time : {watch.Elapsed.TotalSeconds}");
        }


        [TestMethod]
        public void PrintInitTime()
        {
            Trace.WriteLine($"init time for the index : {Text}");
            Trace.WriteLine(_Line);
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
        public void SpeedTest()
        {
            Trace.WriteLine(_Line);
            var filter = search.Output2.Where(x => x.Path.Contains("aa", StringComparison.OrdinalIgnoreCase)).ToList();
            filter.Count();
            Trace.WriteLine(_Line);

        }

        [TestMethod]
        public void LoadConfig()
        {
            var conf = new Config();
        

            var temp = conf.Load();

        }

        [TestMethod]
        public void SaveConfig ()
        {
            var conf = new Config();
            conf.Records = search.Output2;

         var temp =    conf.Save();

        }

        [TestMethod]
        public void LoadSaveConfig()
        {
            var conf = new Config();
            conf.Records = search.Output2.Take(10).ToList();
            conf.Save().s;
            


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

        [TestMethod]
        public void SearchAndFilter()
        {
            Stopwatch watch = new Stopwatch();
        

        ///    var search = new WinSearch();
            var _results = search.Output2;
            Trace.WriteLine($"Orginal Count {_results.Count}");
            watch.Start();
            

            var num = _results.Where(x => x.Path.Contains("Z", StringComparison.OrdinalIgnoreCase)).Select(x => x.Number);

           
           
            Trace.WriteLine($"Filtered count {num.ToList().Count}");
            watch.Stop();


            Trace.WriteLine($"linq time for the index : {watch.Elapsed.TotalSeconds}");
        }
    }
}
