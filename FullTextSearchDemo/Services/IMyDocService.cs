using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FullTextSearchDemo.Services;

public interface IMyDocService
{
    // Movies methods
    Results<Ok<object>, ProblemHttpResult> GetMovies(GetMoviesQuery query);
    Results<Ok<object>, ProblemHttpResult> SearchMovies(SearchMovieQuery query);
    Results<Ok<object>, ProblemHttpResult> FullTextSearchMovies(SearchMovieQuery query);
    
    // Products methods
    Results<Ok<object>, ProblemHttpResult> GetProducts(GetProductsQuery query);
    Results<Ok<object>, ProblemHttpResult> SearchProducts(ProductsSearchQuery query);
    Results<Ok<object>, ProblemHttpResult> FullTextSearchProducts(ProductsSearchQuery query);
    Results<Ok<string>, ProblemHttpResult> AddProduct(Product product);
    Results<Ok<string>, ProblemHttpResult> UpdateProduct(int id, Product product);
    Results<Ok<string>, ProblemHttpResult> DeleteProduct(int id);
}
