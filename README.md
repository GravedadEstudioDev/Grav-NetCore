# Grav-NetCore

[![NuGet](https://img.shields.io/nuget/v/Grav-NetCore.svg)](https://www.nuget.org/packages/Grav-NetCore/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Librería genérica para operaciones CRUD con Entity Framework Core, incluyendo paginación, filtros, ordenamiento y mapeo automático entre entidades y DTOs.

## Características

- ✅ **Servicio CRUD genérico**: Reduce código repetitivo en tus controladores
- ✅ **Paginación integrada**: Soporte completo para listados paginados con filtros y ordenamiento
- ✅ **Mapeo automático**: Conversión entre entidades y DTOs sin configuración adicional
- ✅ **Queries personalizadas**: Soporte para joins y queries complejas con LINQ
- ✅ **Multi-targeting**: Compatible con .NET 6.0, 7.0 y 8.0

## Instalación

```bash
dotnet add package Grav-NetCore
```

O agrega la referencia directamente en tu archivo `.csproj`:

```xml
<PackageReference Include="Grav-NetCore" Version="1.0.0" />
```

## Uso Básico

### 1. Define tu entidad y DTO

```csharp
// Entidad de base de datos
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// DTO
public class ProductDTO
{
    public int productId { get; set; }  // Case-insensitive mapping
    public string name { get; set; }
    public decimal price { get; set; }
}
```

### 2. Configura el servicio en Program.cs

```csharp
using GravNetCore.Services;

var builder = WebApplication.CreateBuilder(args);

// Registra el DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registra el servicio genérico
builder.Services.AddScoped<IGenericService<Product, ProductDTO, MyDbContext>,
                            GenericService<Product, ProductDTO, MyDbContext>>();

var app = builder.Build();
```

### 3. Usa el servicio en tu controlador

```csharp
using GravNetCore.Services;
using GravNetCore.Helpers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IGenericService<Product, ProductDTO, MyDbContext> _service;

    public ProductsController(IGenericService<Product, ProductDTO, MyDbContext> service)
    {
        _service = service;
    }

    [HttpGet]
    public List<ProductDTO> GetAll()
    {
        return _service.ListarCompleto();
    }

    [HttpGet("{id}")]
    public ProductDTO? GetById(int id)
    {
        return _service.Recuperar(id, "ProductId", "productId");
    }

    [HttpPost]
    public int Create([FromBody] ProductDTO dto)
    {
        return _service.Guardar(dto, "productId", "ProductId");
    }

    [HttpPut]
    public int Update([FromBody] ProductDTO dto)
    {
        return _service.Guardar(dto, "productId", "ProductId");
    }

    [HttpDelete("{id}")]
    public int Delete(int id)
    {
        return _service.Borrar(id, "ProductId");
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<PaginatedResponse<ProductDTO>>> GetPaginated(
        int pageNumber = 1,
        int pageSize = 10,
        string orderBy = "productId",
        string direction = "asc")
    {
        return await _service.ListarPaginadoConFiltros(
            pageNumber,
            pageSize,
            direction,
            orderBy,
            filtroCustom: p => p.Price > 0  // Filtro opcional
        );
    }

    [HttpGet("select")]
    public IActionResult GetForDropdown()
    {
        // Retorna objetos {value, label} para dropdowns
        return _service.ListarSelect("ProductId", "Name");
    }
}
```

## Uso Avanzado: Queries con Joins

Para consultas que requieren joins o proyecciones complejas:

```csharp
[HttpGet("with-category")]
public async Task<ActionResult<PaginatedResponse<ProductWithCategoryDTO>>> GetWithCategory(
    int pageNumber = 1,
    int pageSize = 10)
{
    return await _service.ListarPaginadoConQuery(
        queryBuilder: context => context.Products
            .Join(context.Categories,
                p => p.CategoryId,
                c => c.CategoryId,
                (p, c) => new ProductWithCategoryDTO
                {
                    productId = p.ProductId,
                    name = p.Name,
                    categoryName = c.Name
                }),
        pageNumber: pageNumber,
        pageSize: pageSize,
        ascOrDesc: "asc",
        orderBy: "name"
    );
}
```

## API Reference

### IGenericService<TEntity, TDTO, TContext>

#### Métodos

- **`List<TDTO> ListarCompleto()`**: Obtiene todos los registros
- **`IActionResult ListarSelect(string idPropertyName, string textPropertyName)`**: Lista para dropdowns
- **`TDTO? Recuperar(int id, string entityIdProperty, string dtoIdProperty)`**: Obtiene un registro por ID
- **`int Borrar(int id, string entityIdProperty)`**: Elimina un registro
- **`int Guardar(TDTO dto, string dtoIdProperty, string entityIdProperty)`**: Inserta o actualiza
- **`Task<ActionResult<PaginatedResponse<TDTO>>> ListarPaginadoConFiltros(...)`**: Paginación con filtros
- **`Task<ActionResult<PaginatedResponse<TDTO>>> ListarPaginadoConQuery(...)`**: Paginación con queries custom

### MappingExtensions

- **`TDestination MapTo<TDestination>(object source)`**: Mapea a nuevo objeto
- **`void MapProperties(object source, object destination)`**: Mapea a objeto existente

### PaginatedResponse<T>

```csharp
public class PaginatedResponse<T>
{
    public int TotalRecords { get; set; }
    public List<T> Data { get; set; }
}
```

## Características del Mapeo

- **Case-insensitive**: `ProductId` se mapea automáticamente a `productId`
- **Nullable-compatible**: Maneja conversiones entre tipos nullable y no-nullable
- **Type-safe**: Valida compatibilidad de tipos antes de mapear
- **Bidireccional**: Funciona de Entity → DTO y DTO → Entity

## Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un issue o pull request en el repositorio.

## Licencia

Este proyecto está licenciado bajo la Licencia MIT. Ver el archivo LICENSE para más detalles.

## Soporte

Para reportar bugs o solicitar features, por favor abre un issue en [GitHub](https://github.com/Invitafy/Grav-NetCore/issues).
