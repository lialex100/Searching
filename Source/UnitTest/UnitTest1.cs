using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Searching;
using Helper.Helper;

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
            search = new WinSearch(limitResult: 10);
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
        public void SaveConfig()
        {
            var conf = new Config();
            conf.Records = search.Output2;

            var temp = conf.Save();
        }

        [TestMethod]
        public void LoadSaveConfig()
        {
            int numberOfRecord = 20;

            var conf = new Config();
            conf.Records = search.Output2.Take(numberOfRecord).ToList();
            conf.Save().Should().BeTrue();

            conf.Records = null;
            conf.Records.Should().BeNull();

            conf.Load().Should().BeTrue();
            conf.Records.Count.Should().Be(numberOfRecord);
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
            var conf = new Config();

            var temp = conf.Load();

            int no = 50;
            Stopwatch watch = new Stopwatch();
         
            ///    var search = new WinSearch();
     //       var _results = search.Output2;
            var _results = conf.Records;


            Trace.WriteLine($"Orginal Count {_results.Count}");
            watch.Start();
            Regex regex = new Regex(@"a", RegexOptions.Compiled);

            for (int i = 0; i < no; i++)
            {
                filter(_results, regex);
            }

            watch.Stop();

            Trace.WriteLine($"filter logic running time : {watch.Elapsed.TotalSeconds / no:R}");
        }

        public void filter(List<RecodResult> record , Regex regex)
        {
            //   Regex regex = new Regex(@"a", RegexOptions.IgnoreCase);
            //   
            //var dd = record.Where(x =>
            //{
            //    return regex.Match(x.Path).Success;
            //}).ToList();

            var dd = record.AsParallel().Where(x => x.Path.Contains("a", StringComparison.OrdinalIgnoreCase)).ToList();
          //  Trace.WriteLine($"Filtered count {dd.ToList().Count}");
        }
    }
}