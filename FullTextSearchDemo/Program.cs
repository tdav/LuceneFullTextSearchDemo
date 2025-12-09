using FullTextSearchDemo.Endpoints;
using FullTextSearchDemo.Search;
using FullTextSearchDemo.SearchEngine;
using FullTextSearchDemo.Services;
using Scalar.AspNetCore;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMemoryCache();

        builder.Services.AddOpenApi();

        builder.Services.AddScoped<IMyDataService, MovieService>();
        builder.Services.AddScoped<ISearchCacheService, SearchCacheService>();
        builder.Services.AddScoped<IMyDocService, MyDocService>();
        
        builder.Services.AddSearchEngineServices(new MyDataConfiguration());

        builder.Services.AddHostedService<MyDataImporterService>();

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference("/docs", options =>
        {
            options.WithTitle("My Full Text Search");
        });

        var docsGroup = app.MapGroup("/api/docs")
            .WithOpenApi();

        docsGroup.MapMyDocEndpoints();

        app.Run();
    }
}