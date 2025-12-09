using FullTextSearchDemo.SearchEngine.Facets;
using FullTextSearchDemo.SearchEngine.Models;

namespace FullTextSearchDemo.Models;

public class MyData : IDocument
{
    public string UniqueKey => ID;
    
    public string ID { get; set; }
    
    [FacetProperty]
    public string NAME { get; set; }    
}