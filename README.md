# Lucene.NET powered search library for ASP.NET Core

FullTextSearchDemo is a project that provides search engine services for full-text search in documents. It utilizes Lucene.NET for indexing and searching documents efficiently.

## Architecture

This project uses **ASP.NET Core Minimal API** for lightweight, high-performance endpoints with built-in performance optimizations including:
- **In-memory caching** for full-text search results (5-minute cache duration)
- **Response compression** for reduced bandwidth usage
- **Grouped endpoints** for better organization and OpenAPI documentation

## API Endpoints

### Movies Endpoints

- **GET** `/api/docs/movies` - Get movies with filtering
- **GET** `/api/docs/movies/search` - Search movies across all fields
- **GET** `/api/docs/movies/fulltextsearch` - Full-text search movies (with caching)

### Products Endpoints

- **GET** `/api/docs/products` - Get products with filtering
- **POST** `/api/docs/products` - Add a new product
- **GET** `/api/docs/products/search` - Search products across all fields
- **GET** `/api/docs/products/fulltextsearch` - Full-text search products (with caching)
- **PUT** `/api/docs/products/{id}` - Update a product
- **DELETE** `/api/docs/products/{id}` - Delete a product

### Example Usage

```bash
# Full-text search movies
curl "http://localhost:5000/api/docs/movies/fulltextsearch?term=action&pageNumber=1&pageSize=10"

# Full-text search products
curl "http://localhost:5000/api/docs/products/fulltextsearch?search=laptop&pageNumber=1&pageSize=10"

# Add a new product
curl -X POST "http://localhost:5000/api/docs/products" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99,
    "category": "Electronics",
    "inSale": true
  }'

# Update a product
curl -X PUT "http://localhost:5000/api/docs/products/1" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Gaming Laptop",
    "description": "High-performance gaming laptop",
    "price": 1299.99,
    "category": "Electronics",
    "inSale": true
  }'

# Delete a product
curl -X DELETE "http://localhost:5000/api/docs/products/1"
```

## Performance Optimizations

### Caching

Full-text search results are cached in memory for **5 minutes** to improve response times for repeated queries. The cache key includes:
- Search term
- Page number and size
- Facet filters (categories, genres, etc.)

Cache is automatically cleared when products are added, updated, or deleted.

### Response Compression

Response compression is enabled for all endpoints, including HTTPS requests, to reduce bandwidth usage and improve performance for clients with limited network capacity.

### Configuration

To adjust cache duration, modify the `SearchCacheService.cs` file:

```csharp
private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
```

To configure compression options, modify `Program.cs`:

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    // Add additional compression providers if needed
});
```

## Getting Started

### Installation

TBD

### Configuration

#### Defining the type of `document` 

Start by configuring the model you want to search by implementing the IDocument interface in your class.

```csharp
public class Product : IDocument
{
    public string UniqueKey => Id.ToString();
    
    public int Id { get; set; }
    ...
}
```

Please note that the UniqueKey is used for updating and deleting documents. It is crucial to ensure that each document has a unique UniqueKey, as shared UniqueKey values among documents can lead to unintended consequences during update and delete operations.

#### Index directory

Create a class implementing the `IIndexConfiguration<T>` interface, where `T` is the type of documents you want to search.

```csharp
public class ProductConfiguration : IIndexConfiguration<Product>
{
    public string IndexName => "product-index";
}
```

The index name is the folder where Lucene.NET will store the indexed data and related files.

#### Service registration

Register the search engine services using the provided extension method on IServiceCollection.

```csharp
...
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISearchCacheService, SearchCacheService>();
builder.Services.AddSearchEngineServices(new ProductConfiguration());

var app = builder.Build();

app.UseResponseCompression();
...
```

### Usage

#### Adding documents

To add a single document to the search engine, use the Add method.

```csharp
var newProduct = new Product
{
    Id = 1,
    Name = "Sample Product",
    Description = "This is a sample product description."
};

_searchEngine.Add(newProduct);
```

The AddRange method is designed to add a collection of documents:

```csharp
var products = new List<Product>();

// Add items to the list

_searchEngine.AddRange(products);
```

#### Updating documents

To update an existing document in the search engine, use the Update method providing the updated document.

```csharp
var updatedProduct = new Product
{
    Id = 1,
    Name = "Updated Product",
    Description = "This is an updated product description."
};

_searchEngine.Update(updatedProduct);
```

#### Deleting a Document

To remove a single document from the search engine, use the Remove method providing the document.

```csharp
var product = new Product
{
    Id = 1,
    Name = "Updated Product",
};

_searchEngine.Remove(product);
```

If you desire to remove all documents from the index you can use the Clear method.

```csharp
_searchEngine.RemoveAll();
```

#### Free resources

Since the IndexWriter is kept as Singleton everytime that a document is added it is kept in memory.
To free this memory allocations you can use the method:

```csharp
_searchEngine.DisposeResources();
```

#### Searching by Specific Fields

You can perform searches based on specific fields within your documents using the FieldSpecificSearchQuery. Here's an example of how to search for products by their name and description.

```csharp
var searchTerm = new Dictionary<string, string?>();

searchTerm.Add(nameof(Product.Name), "MyProduct");
searchTerm.Add(nameof(Product.Description), "Its description");

var searchQuery = new FieldSpecificSearchQuery
{
    SearchTerms = searchTerm,
    PageNumber = 1,
    PageSize = 10,
    Type = SearchType.ExactMatch
};

var result = _searchEngine.Search(searchQuery);
```

#### Full-Text Searching

To perform a full-text search across all fields of your documents, use the AllFieldsSearchQuery. This allows you to find documents that match a search term regardless of the field.

```csharp
var fullTextQuery = new AllFieldsSearchQuery
{
    SearchTerm = "Sample Product",
    PageNumber = 1,
    PageSize = 10,
    Type = SearchType.FuzzyMatch
};

var fullTextResult = searchEngine.Search(fullTextQuery);
```

#### Facets

##### Configuration

To enable a property to act as facet, it is necessary to annotate it with the `FacetProperty` attribute.

```csharp
public class Movie : IDocument
{
    ...
    [FacetProperty]
    public string TitleType { get; set; }
    ...
}
```

When a property is of type string[], in addition to adding the FacetProperty attribute, it is necessary to configure the field as a multivalued field.

```csharp
public class Movie : IDocument
{
    ...
    [FacetProperty]
    public string[] Genres { get; set; }
    ...
}
```

The MoviesConfiguration shows how to configure the multivalued field correctly:

```csharp
public class MoviesConfiguration : IIndexConfiguration<Movie>
{
    public string IndexName => "movies-index";

    public FacetConfiguration<Movie>? FacetConfiguration => new()
    {
        MultiValuedFields = new[] { nameof(Movie.Genres) }
    };
}
```

If the facet feature is not necessary, you set the `FacetConfiguration` to null.

```csharp
public class PostTestConfiguration : IIndexConfiguration<Post>
{
    public string IndexName => "post-test-index";

    public FacetConfiguration<Post>? FacetConfiguration => null;
}
```

##### Filtering

All public queries expose the Facets dictionary, where the key represents the property name and the value represents the facet search value.

```csharp
var facets = new Dictionary<string, IEnumerable<string?>?>();
facets.Add(nameof(Movie.Genres), new string[]{"Comedy", "Drama", "Action"});

_searchEngine.Search(new AllFieldsSearchQuery { Facets = facets });
```

## LICENSE

Copyright 2023 José Rojas Jimenez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
