using FullTextSearchDemo.SearchEngine.Configuration;
using FullTextSearchDemo.SearchEngine.Helpers;
using FullTextSearchDemo.SearchEngine.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace FullTextSearchDemo.SearchEngine.Services;

internal sealed class DocumentWriter<T> : IDisposable, IDocumentWriter<T> where T : IDocument
{
    private static readonly ConcurrentDictionary<string, RAMDirectory> IndexDirectories = new();
    private static readonly ConcurrentDictionary<string, RAMDirectory> FacetDirectories = new();
    private static readonly ConcurrentDictionary<string, IndexWriter> IndexWriters = new();
    private static readonly ConcurrentDictionary<string, DirectoryTaxonomyWriter> TaxonomyWriters = new();

    private static readonly object IndexLock = new();
    private static readonly object FacetLock = new();

    private readonly FacetsConfig? _facetsConfig;
    private readonly string _indexName;
    private readonly string? _facetIndexName;
    
    private bool _initialized;
    private IndexWriter? _writer;
    private DirectoryTaxonomyWriter? _taxonomyWriter;

    public DocumentWriter(IIndexConfiguration<T> configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.IndexName))
        {
            throw new ArgumentException("Index name must be set before using DocumentWriter.");
        }

        _indexName = configuration.IndexName;
        _facetIndexName = configuration.FacetConfiguration?.IndexName;
        _facetsConfig = configuration.FacetConfiguration?.GetFacetConfig();
    }

    public static RAMDirectory GetOrCreateIndexDirectory(string indexName)
    {
        if (IndexDirectories.TryGetValue(indexName, out var directory))
        {
            return directory;
        }

        lock (IndexLock)
        {
            if (IndexDirectories.TryGetValue(indexName, out directory))
            {
                return directory;
            }

            directory = new RAMDirectory();
            IndexDirectories[indexName] = directory;
            return directory;
        }
    }

    public static RAMDirectory GetOrCreateFacetDirectory(string facetIndexName)
    {
        if (FacetDirectories.TryGetValue(facetIndexName, out var directory))
        {
            return directory;
        }

        lock (FacetLock)
        {
            if (FacetDirectories.TryGetValue(facetIndexName, out directory))
            {
                return directory;
            }

            directory = new RAMDirectory();
            FacetDirectories[facetIndexName] = directory;
            return directory;
        }
    }

    private static IndexWriter GetOrCreateIndexWriter(string indexName, RAMDirectory directory)
    {
        if (IndexWriters.TryGetValue(indexName, out var writer))
        {
            return writer;
        }

        lock (IndexLock)
        {
            if (IndexWriters.TryGetValue(indexName, out writer))
            {
                return writer;
            }

            const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;
            Analyzer standardAnalyzer = new StandardAnalyzer(luceneVersion);
            var indexConfig = new IndexWriterConfig(luceneVersion, standardAnalyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND,
            };

            writer = new IndexWriter(directory, indexConfig);
            IndexWriters[indexName] = writer;
            return writer;
        }
    }

    private static DirectoryTaxonomyWriter GetOrCreateTaxonomyWriter(string facetIndexName, RAMDirectory directory)
    {
        if (TaxonomyWriters.TryGetValue(facetIndexName, out var writer))
        {
            return writer;
        }

        lock (FacetLock)
        {
            if (TaxonomyWriters.TryGetValue(facetIndexName, out writer))
            {
                return writer;
            }

            writer = new DirectoryTaxonomyWriter(directory);
            TaxonomyWriters[facetIndexName] = writer;
            return writer;
        }
    }

    public void Init()
    {
        if (_initialized)
        {
            return;
        }

        // Get or create the RAM directory for the index
        var indexDirectory = GetOrCreateIndexDirectory(_indexName);
        
        // Get or create the shared index writer
        _writer = GetOrCreateIndexWriter(_indexName, indexDirectory);

        if (_facetsConfig != null && !string.IsNullOrWhiteSpace(_facetIndexName))
        {
            var facetDirectory = GetOrCreateFacetDirectory(_facetIndexName);
            _taxonomyWriter = GetOrCreateTaxonomyWriter(_facetIndexName, facetDirectory);
        }

        _initialized = true;
    }

    public void AddDocument([NotNull] T generic)
    {
        var document = generic.ConvertToDocument();
        // IndexWriter is thread-safe and handles concurrent writes internally
        _writer?.AddDocument(GetDocument(document));

        Commit();
    }

    public void Clear()
    {
        // IndexWriter is thread-safe and handles concurrent writes internally
        _writer?.DeleteAll();

        Commit();
    }

    public void AddDocuments(IEnumerable<T> documents)
    {
        // IndexWriter is thread-safe and handles concurrent writes internally
        foreach (var generic in documents)
        {
            _writer?.AddDocument(GetDocument(generic));
        }

        Commit();
    }

    public void UpdateDocument([NotNull] T generic)
    {
        var document = generic.ConvertToDocument();
        // IndexWriter is thread-safe and handles concurrent writes internally
        _writer?.UpdateDocument(new Term(nameof(IDocument.UniqueKey), generic.UniqueKey), GetDocument(document));

        Commit();
    }

    public void RemoveDocument([NotNull] T generic)
    {
        // IndexWriter is thread-safe and handles concurrent writes internally
        _writer?.DeleteDocuments(new Term(nameof(IDocument.UniqueKey), generic.UniqueKey));

        Commit();
    }

    public void Dispose()
    {
        // Don't dispose shared writers and directories
        // They are managed at the static level and shared across instances
        // In a web application, these live for the application lifetime
        _initialized = false;
    }

    private void Commit()
    {
        _writer?.Commit();
        _taxonomyWriter?.Commit();
    }

    private Document GetDocument(T generic)
    {
        var document = generic.ConvertToDocument();
        return GetDocument(document);
    }
    
    /// <summary>
    /// Gets the document with facets applied if configured.
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    private Document GetDocument(Document document)
    {
        return _facetsConfig != null ? _facetsConfig.Build(_taxonomyWriter, document) : document;
    }
}