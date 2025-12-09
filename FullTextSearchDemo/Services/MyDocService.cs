using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FullTextSearchDemo.Services;

public class MyDocService : IMyDocService
{
    private readonly IMovieService _movieService;
    private readonly IProductService _productService;
    private readonly ISearchCacheService _cacheService;

    public MyDocService(
        IMovieService movieService,
        IProductService productService,
        ISearchCacheService cacheService)
    {
        _movieService = movieService;
        _productService = productService;
        _cacheService = cacheService;
    }

    // Movies methods
    public Results<Ok<object>, ProblemHttpResult> GetMovies(GetMoviesQuery query)
    {
        try
        {
            var result = _movieService.GetMovies(query);
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

    public Results<Ok<object>, ProblemHttpResult> SearchMovies(SearchMovieQuery query)
    {
        try
        {
            var result = _movieService.SearchMovies(query);
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

    public Results<Ok<object>, ProblemHttpResult> FullTextSearchMovies(SearchMovieQuery query)
    {
        try
        {
            // Create cache key using HashCode.Combine for better performance
            var hashCode = HashCode.Combine(
                "movies_fulltext",
                query.Term,
                query.PageNumber,
                query.PageSize,
                query.FacetGenreFacets != null ? string.Join("_", query.FacetGenreFacets) : "",
                query.TitleTypeFacets != null ? string.Join("_", query.TitleTypeFacets) : ""
            );

            var cacheKey = $"movies_{hashCode}";

            var result = _cacheService.GetOrCreate(cacheKey, () => _movieService.FullTextSearchMovies(query));
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

    // Products methods
    public Results<Ok<object>, ProblemHttpResult> GetProducts(GetProductsQuery query)
    {
        try
        {
            var result = _productService.GetProducts(query);
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

    public Results<Ok<object>, ProblemHttpResult> SearchProducts(ProductsSearchQuery query)
    {
        try
        {
            var result = _productService.SearchProducts(query);
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

    public Results<Ok<object>, ProblemHttpResult> FullTextSearchProducts(ProductsSearchQuery query)
    {
        try
        {
            // Create cache key using HashCode.Combine for better performance
            var hashCode = HashCode.Combine(
                "products_fulltext",
                query.Search,
                query.PageNumber,
                query.PageSize,
                query.Categories != null ? string.Join("_", query.Categories) : "",
                query.InSale?.ToString() ?? ""
            );

            var cacheKey = $"products_{hashCode}";

            var result = _cacheService.GetOrCreate(cacheKey, () => _productService.FullSearchProducts(query));
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

    public Results<Ok<string>, ProblemHttpResult> AddProduct(Product product)
    {
        try
        {
            _productService.Add(product);
            
            // Clear cache for products since we added a new one
            _cacheService.Clear();
            
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

    public Results<Ok<string>, ProblemHttpResult> UpdateProduct(int id, Product product)
    {
        try
        {
            _productService.Update(id, product);
            
            // Clear cache for products since we updated one
            _cacheService.Clear();
            
            return TypedResults.Ok("Product updated in the search index.");
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error updating product");
        }
    }

    public Results<Ok<string>, ProblemHttpResult> DeleteProduct(int id)
    {
        try
        {
            _productService.Delete(id);
            
            // Clear cache for products since we deleted one
            _cacheService.Clear();
            
            return TypedResults.Ok("Product deleted from the search index.");
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
