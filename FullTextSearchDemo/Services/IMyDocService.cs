using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FullTextSearchDemo.Services;

public interface IMyDocService
{
    // Movies methods
    Results<Ok<object>, ProblemHttpResult> Get(GetDataQuery query);
    Results<Ok<object>, ProblemHttpResult> Search(SearchDataQuery query);
    Results<Ok<object>, ProblemHttpResult> FullTextSearch(SearchDataQuery query);    
}
