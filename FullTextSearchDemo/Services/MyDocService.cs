using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FullTextSearchDemo.Services;

public class MyDocService : IMyDocService
{
    private readonly IMyDataService _movieService;
    private readonly ISearchCacheService _cacheService;

    public MyDocService(IMyDataService movieService, ISearchCacheService cacheService)
    {
        _movieService = movieService;
        _cacheService = cacheService;
    }

    // Movies methods
    public Results<Ok<object>, ProblemHttpResult> Get(GetDataQuery query)
    {
        try
        {
            var result = _movieService.Get(query);
            return TypedResults.Ok<object>(result);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error retrieving movies");
        }
    }

    public Results<Ok<object>, ProblemHttpResult> Search(SearchDataQuery query)
    {
        try
        {
            var result = _movieService.Search(query);
            return TypedResults.Ok<object>(result);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error searching movies");
        }
    }

    public Results<Ok<object>, ProblemHttpResult> FullTextSearch(SearchDataQuery query)
    {
        try
        {
            // Create cache key using HashCode.Combine for better performance
            var hashCode = HashCode.Combine(
                "movies_fulltext",
                query.Term,
                query.PageNumber,
                query.PageSize,
                query.NameFacets != null ? string.Join("_", query.NameFacets) : ""
            );

            var cacheKey = $"movies_{hashCode}";

            var result = _cacheService.GetOrCreate(cacheKey, () => _movieService.FullTextSearch(query));
            return TypedResults.Ok<object>(result!);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error performing full-text search on movies");
        }
    }

 
}
