using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using FullTextSearchDemo.SearchEngine.Results;

namespace FullTextSearchDemo.Services;

public interface IMyDataService
{
    SearchResult<MyData> Get(GetDataQuery query);
    
    SearchResult<MyData> Search(SearchDataQuery query);
    
    SearchResult<MyData> FullTextSearch(SearchDataQuery query);
}