using FullTextSearchDemo.Models;
using FullTextSearchDemo.Parameters;
using FullTextSearchDemo.Services;

namespace FullTextSearchDemo.Endpoints;

public static class MyDocEndpoints
{
    public static RouteGroupBuilder MapMyDocEndpoints(this RouteGroupBuilder group)
    {
        var moviesGroup = group.MapGroup("/movies").WithTags("Movies");
        var productsGroup = group.MapGroup("/products").WithTags("Products");

        // Movies endpoints
        moviesGroup.MapGet("/", (
            [AsParameters] GetMoviesQuery query,
            IMyDocService service) => service.GetMovies(query))
            .WithName("GetMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get movies with filtering";
                operation.Description = "Retrieves movies based on specified query parameters";
                return operation;
            });

        moviesGroup.MapGet("/search", (
            [AsParameters] SearchMovieQuery query,
            IMyDocService service) => service.SearchMovies(query))
            .WithName("SearchMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search movies";
                operation.Description = "Search movies across all fields using prefix matching";
                return operation;
            });

        moviesGroup.MapGet("/fulltextsearch", (
            [AsParameters] SearchMovieQuery query,
            IMyDocService service) => service.FullTextSearchMovies(query))
            .WithName("FullTextSearchMovies")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Full-text search movies";
                operation.Description = "Perform full-text search on movies with caching support";
                return operation;
            });

        // Products endpoints
        productsGroup.MapGet("/", (
            [AsParameters] GetProductsQuery query,
            IMyDocService service) => service.GetProducts(query))
            .WithName("GetProducts")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get products with filtering";
                operation.Description = "Retrieves products based on specified query parameters";
                return operation;
            });

        productsGroup.MapGet("/search", (
            [AsParameters] ProductsSearchQuery query,
            IMyDocService service) => service.SearchProducts(query))
            .WithName("SearchProducts")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search products";
                operation.Description = "Search products across all fields using fuzzy matching";
                return operation;
            });

        productsGroup.MapGet("/fulltextsearch", (
            [AsParameters] ProductsSearchQuery query,
            IMyDocService service) => service.FullTextSearchProducts(query))
            .WithName("FullTextSearchProducts")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Full-text search products";
                operation.Description = "Perform full-text search on products with caching support";
                return operation;
            });

        productsGroup.MapPost("/", (
            Product product,
            IMyDocService service) => service.AddProduct(product))
            .WithName("AddProduct")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Add a new product";
                operation.Description = "Adds a new product to the search index";
                return operation;
            });

        productsGroup.MapPut("/{id:int}", (
            int id,
            Product product,
            IMyDocService service) => service.UpdateProduct(id, product))
            .WithName("UpdateProduct")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update a product";
                operation.Description = "Updates an existing product in the search index";
                return operation;
            });

        productsGroup.MapDelete("/{id:int}", (
            int id,
            IMyDocService service) => service.DeleteProduct(id))
            .WithName("DeleteProduct")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Delete a product";
                operation.Description = "Removes a product from the search index";
                return operation;
            });

        return group;
    }
}
