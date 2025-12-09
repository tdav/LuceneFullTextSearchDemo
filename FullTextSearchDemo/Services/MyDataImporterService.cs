using FullTextSearchDemo.Models;
using FullTextSearchDemo.SearchEngine;
using FullTextSearchDemo.SearchEngine.Queries;
using FullTextSearchDemo.SearchEngine.Results;
using Lucene.Net.Index;

namespace FullTextSearchDemo.Services;

public class MyDataImporterService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MyDataImporterService(IServiceScopeFactory scopeFactory)
    {
        _serviceScopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var searchEngine = scope.ServiceProvider.GetRequiredService<ISearchEngine<MyData>>();

        var result = new SearchResult<MyData>()
        {
            TotalItems = 0
        };

        try
        {
            result = searchEngine.Search(new AllFieldsSearchQuery { Type = SearchType.ExactMatch });
        }
        catch (IndexNotFoundException ex)
        {
            //Ignore exception when index does not exist
            Console.WriteLine(ex);
        }

        if (result.TotalItems > 0)
        {
            return;
        }

        await ImportMoviesAsync(searchEngine, stoppingToken);
    }

    private static async Task ImportMoviesAsync(ISearchEngine<MyData> searchEngine, CancellationToken stoppingToken)
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "title.basics.tsv");

        var index = 0;

        var startTime = DateTime.Now;
        using var reader = new StreamReader(filePath);
        var batch = new List<MyData>();
        while (await reader.ReadLineAsync(stoppingToken) is { } line)
        {
            //skip headers
            if (index == 0)
            {
                index++;
                continue;
            }

            try
            {
                batch.Add(GetMovie(line));
            }
            catch
            {
                //skip invalid lines
            }

            if (index % 500_000 == 0)
            {
                searchEngine.AddRange(batch);
                batch.Clear();
                var time = DateTime.Now - startTime;
                Console.WriteLine($"Indexed {index} completed in {time.TotalSeconds} seconds.");
            }

            index++;

            if (index > 2_000_000)
            {
                break;
            }
        }

        var endTime = DateTime.Now;
        var duration = endTime - startTime;
        Console.WriteLine($"Indexing completed in {duration.TotalHours} hours.");
        Console.WriteLine($"Indexing completed in {duration.TotalMinutes} minutes.");
        Console.WriteLine($"Indexing completed in {duration.TotalSeconds} seconds.");
        Console.WriteLine($"Indexed {index} movies.");

        //Avoid to keep in memory all the movies
        searchEngine.DisposeResources();
    }

    private static MyData GetMovie(string line)
    {
        var fields = line.Split(';');

        if (fields.Length < 2)
        {
            Console.WriteLine($"Error: Insufficient fields - {line}");
            throw new Exception();
        }

        try
        {
            return new MyData
            {
                ID = fields[0],
                NAME= fields[1],
            };
        }
        catch
        {
            Console.WriteLine($"Error: {line}");
            throw;
        }
    }
  
}