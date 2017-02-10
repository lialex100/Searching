using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Searching.LuceneSearch
{
    public static class LuceneSearch
    {
        private static string _lucenePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LuceneIndex");
        private static FSDirectory _fsDirectory;
        private static FSDirectory FsDirectory 
        {
            get
            {
                if (_fsDirectory == null) _fsDirectory = FSDirectory.Open(new DirectoryInfo(_lucenePath));
                if (IndexWriter.IsLocked(_fsDirectory)) IndexWriter.Unlock(_fsDirectory);
                var lockFilePath = Path.Combine(_lucenePath, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _fsDirectory;
            }
        }

        private static void AddToLuceneIndex(RecodResult sampleData, IndexWriter writer)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term("Number", sampleData.Number.ToString()));
            writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();

            // add lucene fields mapped to db fields
            doc.Add(new Field("Number", sampleData.Number.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("FileName", sampleData.FileName?? string.Empty, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Path", sampleData.Path, Field.Store.YES, Field.Index.ANALYZED));

            // add entry to index
            writer.AddDocument(doc);
        }

        public static void AddUpdateLuceneIndex(RecodResult sampleData)
        {
            AddUpdateLuceneIndex(new[] {sampleData});
        }

        public static void AddUpdateLuceneIndex(IEnumerable<RecodResult> sampleDatas)
        {
            // init lucene
          //  var analyzer = new KeywordAnalyzer(Version.LUCENE_30);
            var analyzer = new WhitespaceAnalyzer();
            using (var writer = new IndexWriter(FsDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entry if any)
                foreach (var sampleData in sampleDatas) AddToLuceneIndex(sampleData, writer);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        public static void ClearLuceneIndexRecord(int record_id)
        {
            // init lucene
            //var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var analyzer = new WhitespaceAnalyzer();
            using (var writer = new IndexWriter(FsDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term("Number", record_id.ToString()));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(FsDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }

        private static RecodResult _mapLuceneDocumentToData(Document doc)
        {
            return new RecodResult
            {
                Number = Convert.ToInt32(doc.Get("Number")),
                FileName = doc.Get("FileName"),
                  Path = doc.Get("Path")
            };
        }

        private static IEnumerable<RecodResult> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData).ToList();
        }

        private static IEnumerable<RecodResult> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits,
            IndexSearcher searcher)
        {
            return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.Doc))).ToList();
        }

        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        private static IEnumerable<RecodResult> _search(string searchQuery, string searchField = "")
        {
            // validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<RecodResult>();

            // set up lucene searcher
            using (var searcher = new IndexSearcher(FsDirectory, false))
            {
                var hits_limit = 1000;
               // var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                var analyzer = new WhitespaceAnalyzer();
                // search by single field
                if (!string.IsNullOrEmpty(searchField))
                {
                    var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, hits_limit).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    return results;
                }
                // search by multiple fields (ordered by RELEVANCE)
                else
                {
                    var parser = new MultiFieldQueryParser
                        (Version.LUCENE_30, new[] {"Number", "FileName", "Path" }, analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search
                        (query, null, hits_limit, Sort.RELEVANCE).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    return results;
                }
            }
        }

        public static IEnumerable<RecodResult> Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input)) return new List<RecodResult>();

            var terms = input.Trim().Replace("-", " ").Split(' ')
                .Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }

        public static IEnumerable<RecodResult> SearchDefault(string input, string fieldName = "")
        {
            return string.IsNullOrEmpty(input) ? new List<RecodResult>() : _search(input, fieldName);
        }

        public static IEnumerable<RecodResult> GetAllIndexRecords()
        {
            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_lucenePath).Any()) return new List<RecodResult>();

            // set up lucene searcher
            var searcher = new IndexSearcher(FsDirectory, false);
            var reader = IndexReader.Open(FsDirectory, false);
            var docs = new List<Document>();
            var term = reader.TermDocs();
            while (term.Next()) docs.Add(searcher.Doc(term.Doc));
            reader.Dispose();
            searcher.Dispose();
            return _mapLuceneToDataList(docs);
        }
    }
}