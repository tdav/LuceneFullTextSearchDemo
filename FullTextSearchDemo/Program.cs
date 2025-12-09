using FullTextSearchDemo.Endpoints;
using FullTextSearchDemo.Search;
using FullTextSearchDemo.SearchEngine;
using FullTextSearchDemo.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add memory cache for search results caching
        builder.Services.AddMemoryCache();
        
        // Add response compression
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IMovieService, MovieService>();
        builder.Services.AddScoped<ISearchCacheService, SearchCacheService>();
        
        builder.Services.AddSearchEngineServices(new ProductConfiguration());
        builder.Services.AddSearchEngineServices(new MoviesConfiguration());

        builder.Services.AddHostedService<MovieImporterService>();

        var app = builder.Build();

        // Use response compression
        app.UseResponseCompression();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        // Map Minimal API endpoints
        var docsGroup = app.MapGroup("/api/docs")
            .WithOpenApi();
        
        docsGroup.MapMyDocEndpoints();

        app.Run();
    }
}