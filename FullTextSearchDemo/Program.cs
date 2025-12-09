using FullTextSearchDemo.Search;
using FullTextSearchDemo.SearchEngine;
using FullTextSearchDemo.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IMovieService, MovieService>();
        builder.Services.AddSearchEngineServices(new ProductConfiguration());
        builder.Services.AddSearchEngineServices(new MoviesConfiguration());

        builder.Services.AddHostedService<MovieImporterService>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}