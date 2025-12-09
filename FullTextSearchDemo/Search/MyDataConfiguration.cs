using FullTextSearchDemo.Models;
using FullTextSearchDemo.SearchEngine.Configuration;

namespace FullTextSearchDemo.Search;

public class MyDataConfiguration : IIndexConfiguration<MyData>
{
    public string IndexName => "index";

    public FacetConfiguration<MyData>? FacetConfiguration => new()
    {
        IndexName = "index-facets",
        MultiValuedFields = new[] { nameof(MyData.NAME) }
    };
}