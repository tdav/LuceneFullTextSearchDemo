using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using FullTextSearchDemo.Services;

namespace FullTextSearchDemo.Endpoints;

public static class MyDocEndpoints
{
    public static RouteGroupBuilder MapMyDocEndpoints(this RouteGroupBuilder group)
    {
        var myDataGroup = group.MapGroup("/data").WithTags("Movies");
        

        // Movies endpoints
        myDataGroup.MapGet("/", (
            [AsParameters] GetDataQuery query,
            IMyDocService service) => service.Get(query))
            .WithName("GetMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get movies with filtering";
                operation.Description = "Retrieves movies based on specified query parameters";
                return operation;
            });

        myDataGroup.MapGet("/search", (
            [AsParameters] SearchDataQuery query,
            IMyDocService service) => service.Search(query))
            .WithName("SearchMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search movies";
                operation.Description = "Search movies across all fields using prefix matching";
                return operation;
            });

        myDataGroup.MapGet("/fulltextsearch", (
            [AsParameters] SearchDataQuery query,
            IMyDocService service) => service.FullTextSearch(query))
            .WithName("FullTextSearchMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Full-text search movies";
                operation.Description = "Perform full-text search on movies with caching support";
                return operation;
            });

        return group;
    }
}
