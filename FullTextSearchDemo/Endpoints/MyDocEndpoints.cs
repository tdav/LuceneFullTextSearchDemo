using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using FullTextSearchDemo.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FullTextSearchDemo.Endpoints;

public static class MyDocEndpoints
{
    public static RouteGroupBuilder MapMyDocEndpoints(this RouteGroupBuilder group)
    {
        var moviesGroup = group.MapGroup("/movies").WithTags("Movies");
        var productsGroup = group.MapGroup("/products").WithTags("Products");

        // Movies endpoints
        moviesGroup.MapGet("/", GetMovies)
            .WithName("GetMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get movies with filtering";
                operation.Description = "Retrieves movies based on specified query parameters";
                return operation;
            });

        moviesGroup.MapGet("/search", SearchMovies)
            .WithName("SearchMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search movies";
                operation.Description = "Search movies across all fields using prefix matching";
                return operation;
            });

        moviesGroup.MapGet("/fulltextsearch", FullTextSearchMovies)
            .WithName("FullTextSearchMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Full-text search movies";
                operation.Description = "Perform full-text search on movies with caching support";
                return operation;
            });

        // Products endpoints
        productsGroup.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get products with filtering";
                operation.Description = "Retrieves products based on specified query parameters";
                return operation;
            });

        productsGroup.MapGet("/search", SearchProducts)
            .WithName("SearchProducts")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search products";
                operation.Description = "Search products across all fields using fuzzy matching";
                return operation;
            });

        productsGroup.MapGet("/fulltextsearch", FullTextSearchProducts)
            .WithName("FullTextSearchProducts")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Full-text search products";
                operation.Description = "Perform full-text search on products with caching support";
                return operation;
            });

        productsGroup.MapPost("/", PostProduct)
            .WithName("AddProduct")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Add a new product";
                operation.Description = "Adds a new product to the search index";
                return operation;
            });

        productsGroup.MapPut("/{id:int}", PutProduct)
            .WithName("UpdateProduct")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update a product";
                operation.Description = "Updates an existing product in the search index";
                return operation;
            });

        productsGroup.MapDelete("/{id:int}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Delete a product";
                operation.Description = "Removes a product from the search index";
                return operation;
            });

        return group;
    }

    // Movies handlers
    private static Results<Ok<object>, ProblemHttpResult> GetMovies(
        [AsParameters] GetMoviesQuery query,
        IMovieService movieService)
    {
        try
        {
            var result = movieService.GetMovies(query);
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

    private static Results<Ok<object>, ProblemHttpResult> SearchMovies(
        [AsParameters] SearchMovieQuery query,
        IMovieService movieService)
    {
        try
        {
            var result = movieService.SearchMovies(query);
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

    private static Results<Ok<object>, ProblemHttpResult> FullTextSearchMovies(
        [AsParameters] SearchMovieQuery query,
        IMovieService movieService,
        ISearchCacheService cacheService)
    {
        try
        {
            // Create cache key based on query parameters
            var cacheKey = $"movies_fulltext_{query.Term}_{query.PageNumber}_{query.PageSize}";
            
            if (query.FacetGenreFacets != null)
            {
                cacheKey += $"_genres_{string.Join("_", query.FacetGenreFacets)}";
            }
            
            if (query.TitleTypeFacets != null)
            {
                cacheKey += $"_types_{string.Join("_", query.TitleTypeFacets)}";
            }

            var result = cacheService.GetOrCreate(cacheKey, () => movieService.FullTextSearchMovies(query));
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

    // Products handlers
    private static Results<Ok<object>, ProblemHttpResult> GetProducts(
        [AsParameters] GetProductsQuery query,
        IProductService productService)
    {
        try
        {
            var result = productService.GetProducts(query);
            return TypedResults.Ok<object>(result);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error retrieving products");
        }
    }

    private static Results<Ok<object>, ProblemHttpResult> SearchProducts(
        [AsParameters] ProductsSearchQuery query,
        IProductService productService)
    {
        try
        {
            var result = productService.SearchProducts(query);
            return TypedResults.Ok<object>(result);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error searching products");
        }
    }

    private static Results<Ok<object>, ProblemHttpResult> FullTextSearchProducts(
        [AsParameters] ProductsSearchQuery query,
        IProductService productService,
        ISearchCacheService cacheService)
    {
        try
        {
            // Create cache key based on query parameters
            var cacheKey = $"products_fulltext_{query.Search}_{query.PageNumber}_{query.PageSize}";
            
            if (query.Categories != null)
            {
                cacheKey += $"_categories_{string.Join("_", query.Categories)}";
            }
            
            if (query.InSale.HasValue)
            {
                cacheKey += $"_insale_{query.InSale.Value}";
            }

            var result = cacheService.GetOrCreate(cacheKey, () => productService.FullSearchProducts(query));
            return TypedResults.Ok<object>(result!);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error performing full-text search on products");
        }
    }

    private static Results<Ok<string>, ProblemHttpResult> PostProduct(
        Product product,
        IProductService productService,
        ISearchCacheService cacheService)
    {
        try
        {
            productService.Add(product);
            
            // Clear cache for products since we added a new one
            cacheService.Clear();
            
            return TypedResults.Ok("Product added to the search index.");
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error adding product");
        }
    }

    private static Results<Ok<string>, ProblemHttpResult> PutProduct(
        int id,
        Product product,
        IProductService productService,
        ISearchCacheService cacheService)
    {
        try
        {
            productService.Update(id, product);
            
            // Clear cache for products since we updated one
            cacheService.Clear();
            
            return TypedResults.Ok("Product updated to the search index.");
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error updating product");
        }
    }

    private static Results<Ok<string>, ProblemHttpResult> DeleteProduct(
        int id,
        IProductService productService,
        ISearchCacheService cacheService)
    {
        try
        {
            productService.Delete(id);
            
            // Clear cache for products since we deleted one
            cacheService.Clear();
            
            return TypedResults.Ok("Product deleted to the search index.");
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error deleting product");
        }
    }
}
