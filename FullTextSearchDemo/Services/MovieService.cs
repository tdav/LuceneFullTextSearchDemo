using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using FullTextSearchDemo.SearchEngine;
using FullTextSearchDemo.SearchEngine.Queries;
using FullTextSearchDemo.SearchEngine.Results;

namespace FullTextSearchDemo.Services;

public class MovieService : IMyDataService
{
    private readonly ISearchEngine<MyData> _searchEngine;

    public MovieService(ISearchEngine<MyData> searchEngine)
    {
        _searchEngine = searchEngine;
    }

    public SearchResult<MyData> Get(GetDataQuery query)
    {
        var searchFields = new Dictionary<string, string?>();
               
        if (query.Name != null)
        {
            searchFields.Add(nameof(query.Name), query.Name);
        }

        var facets = GetFacets(query);

        return _searchEngine.Search(new FieldSpecificSearchQuery
        {
            SearchTerms = searchFields,
            Type = SearchType.PrefixMatch,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            Facets = facets
        });
    }

    public SearchResult<MyData> Search(SearchDataQuery query)
    {
        var facets = GetFacets(query);

        return _searchEngine.Search(new AllFieldsSearchQuery
        {
            SearchTerm = query.Term,
            Type = SearchType.PrefixMatch,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            Facets = facets
        });
    }

    public SearchResult<MyData> FullTextSearch(SearchDataQuery query)
    {
        var facets = GetFacets(query);
        return _searchEngine.Search(new FullTextSearchQuery
        {
            SearchTerm = query.Term,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            Facets = facets
        });
    }

    private static IDictionary<string, IEnumerable<string?>?> GetFacets(MoviesQuery query)
    {
        var facets = new Dictionary<string, IEnumerable<string?>?>();
        
        if (query.NameFacets != null)
        {
            facets.Add(nameof(MyData.NAME), query.NameFacets);
        }

        return facets;
    }
}