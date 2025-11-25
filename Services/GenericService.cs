using GravNetCore.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GravNetCore.Services
{
    /// <summary>
    /// Servicio genérico para operaciones CRUD con Entity Framework Core
    /// </summary>
    /// <typeparam name="TEntity">Tipo de la entidad en la base de datos</typeparam>
    /// <typeparam name="TDTO">Tipo del DTO (Data Transfer Object)</typeparam>
    /// <typeparam name="TContext">Tipo del DbContext</typeparam>
    public class GenericService<TEntity, TDTO, TContext> : IGenericService<TEntity, TDTO, TContext>
        where TEntity : class, new()
        where TDTO : class, new()
        where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly DbSet<TEntity> _dbSet;

        /// <summary>
        /// Constructor del servicio genérico
        /// </summary>
        /// <param name="context">Instancia del DbContext</param>
        public GenericService(TContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        /// <summary>
        /// Lista todos los registros sin filtro
        /// </summary>
        /// <returns>Lista completa de DTOs</returns>
        public List<TDTO> ListarCompleto()
        {
            try
            {
                var entities = _dbSet.ToList();
                return entities.Select(e => MappingExtensions.MapTo<TDTO>(e)).ToList();
            }
            catch (Exception)
            {
                return new List<TDTO>();
            }
        }

        /// <summary>
        /// Lista registros para dropdown (id + texto)
        /// </summary>
        /// <param name="idPropertyName">Nombre de la propiedad ID en la entidad</param>
        /// <param name="textPropertyName">Nombre de la propiedad de texto en la entidad</param>
        /// <returns>ActionResult con lista de objetos anónimos {value, label}</returns>
        public IActionResult ListarSelect(string idPropertyName, string textPropertyName)
        {
            try
            {
                var entities = _dbSet.ToList();
                var result = entities.Select(e => new
                {
                    value = e.GetType().GetProperty(idPropertyName)?.GetValue(e),
                    label = e.GetType().GetProperty(textPropertyName)?.GetValue(e)
                }).ToList();

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Recupera un registro por ID
        /// </summary>
        /// <param name="id">ID del registro a buscar</param>
        /// <param name="entityIdProperty">Nombre de la propiedad ID en la Entity</param>
        /// <param name="dtoIdProperty">Nombre de la propiedad ID en el DTO</param>
        /// <returns>DTO del registro o null si no existe</returns>
        public TDTO? Recuperar(int id, string entityIdProperty, string dtoIdProperty)
        {
            try
            {
                var entity = _dbSet.ToList().FirstOrDefault(e =>
                {
                    var idValue = e.GetType().GetProperty(entityIdProperty)?.GetValue(e);
                    return idValue != null && (int)idValue == id;
                });

                return entity != null ? MappingExtensions.MapTo<TDTO>(entity) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Elimina un registro por ID
        /// </summary>
        /// <param name="id">ID del registro a eliminar</param>
        /// <param name="entityIdProperty">Nombre de la propiedad ID en la Entity</param>
        /// <returns>1 si se eliminó correctamente, 0 si hubo error o no se encontró</returns>
        public int Borrar(int id, string entityIdProperty)
        {
            try
            {
                var entity = _dbSet.ToList().FirstOrDefault(e =>
                {
                    var idValue = e.GetType().GetProperty(entityIdProperty)?.GetValue(e);
                    return idValue != null && (int)idValue == id;
                });

                if (entity == null) return 0;

                _dbSet.Remove(entity);
                _context.SaveChanges();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Inserta o actualiza un registro
        /// </summary>
        /// <param name="dto">DTO con los datos a guardar</param>
        /// <param name="dtoIdProperty">Nombre de la propiedad ID en el DTO</param>
        /// <param name="entityIdProperty">Nombre de la propiedad ID en la Entity</param>
        /// <returns>1 si se guardó correctamente, 0 si hubo error</returns>
        public int Guardar(TDTO dto, string dtoIdProperty, string entityIdProperty)
        {
            try
            {
                // Leer el ID desde el DTO usando dtoIdProperty
                var dtoIdPropertyInfo = typeof(TDTO).GetProperty(dtoIdProperty);
                if (dtoIdPropertyInfo == null) return 0;

                var idValue = dtoIdPropertyInfo.GetValue(dto);
                int id = idValue != null ? (int)idValue : 0;

                // INSERTAR
                if (id == 0)
                {
                    var newEntity = MappingExtensions.MapTo<TEntity>(dto);
                    _dbSet.Add(newEntity);
                    _context.SaveChanges();
                    return 1;
                }
                // ACTUALIZAR
                else
                {
                    // Buscar la entidad existente usando entityIdProperty
                    var existingEntity = _dbSet.ToList().FirstOrDefault(e =>
                    {
                        var entityIdValue = e.GetType().GetProperty(entityIdProperty)?.GetValue(e);
                        return entityIdValue != null && (int)entityIdValue == id;
                    });

                    if (existingEntity == null) return 0;

                    MappingExtensions.MapProperties(dto, existingEntity);
                    _context.SaveChanges();
                    return 1;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Lista con paginación, filtros y ordenamiento
        /// </summary>
        /// <param name="pageNumber">Número de página (base 1)</param>
        /// <param name="pageSize">Cantidad de registros por página</param>
        /// <param name="ascOrDesc">Dirección del ordenamiento: "asc" o "desc"</param>
        /// <param name="orderBy">Nombre de la propiedad del DTO por la cual ordenar</param>
        /// <param name="filtroCustom">Función opcional para filtrar las entidades</param>
        /// <returns>ActionResult con respuesta paginada</returns>
        public async Task<ActionResult<PaginatedResponse<TDTO>>> ListarPaginadoConFiltros(
            int pageNumber,
            int pageSize,
            string ascOrDesc,
            string orderBy,
            Func<TEntity, bool>? filtroCustom = null)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                var listaFiltrada = query.ToList();

                // Aplicar filtro custom si existe
                if (filtroCustom != null)
                {
                    listaFiltrada = listaFiltrada.Where(filtroCustom).ToList();
                }

                // Mapeo a DTO
                var listaDTOs = listaFiltrada.Select(e => MappingExtensions.MapTo<TDTO>(e)).ToList();

                // Orden dinámico
                var propertyInfo = typeof(TDTO).GetProperty(orderBy);
                if (propertyInfo == null)
                {
                    // Si no existe la propiedad, intentar ordenar por la primera propiedad
                    var firstProperty = typeof(TDTO).GetProperties().FirstOrDefault();
                    if (firstProperty != null)
                    {
                        listaDTOs = ascOrDesc == "desc"
                            ? listaDTOs.OrderByDescending(p => firstProperty.GetValue(p)).ToList()
                            : listaDTOs.OrderBy(p => firstProperty.GetValue(p)).ToList();
                    }
                }
                else
                {
                    listaDTOs = ascOrDesc == "desc"
                        ? listaDTOs.OrderByDescending(p => propertyInfo.GetValue(p)).ToList()
                        : listaDTOs.OrderBy(p => propertyInfo.GetValue(p)).ToList();
                }

                // Paginación
                int totalRecords = listaDTOs.Count;
                var listaPaginada = listaDTOs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var response = new PaginatedResponse<TDTO>
                {
                    TotalRecords = totalRecords,
                    Data = listaPaginada
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Lista con paginación, filtros y ordenamiento permitiendo queries personalizadas con joins
        /// </summary>
        /// <param name="queryBuilder">Función que construye la query con joins usando el contexto</param>
        /// <param name="pageNumber">Número de página (base 1)</param>
        /// <param name="pageSize">Cantidad de registros por página</param>
        /// <param name="ascOrDesc">Dirección del ordenamiento: "asc" o "desc"</param>
        /// <param name="orderBy">Nombre de la propiedad del DTO por la cual ordenar</param>
        /// <param name="filtroCustom">Función opcional para filtrar los DTOs después de la query</param>
        /// <returns>ActionResult con respuesta paginada</returns>
        public async Task<ActionResult<PaginatedResponse<TDTO>>> ListarPaginadoConQuery(
            Func<TContext, IQueryable<TDTO>> queryBuilder,
            int pageNumber,
            int pageSize,
            string ascOrDesc,
            string orderBy,
            Func<TDTO, bool>? filtroCustom = null)
        {
            try
            {
                // Ejecutar el query builder para obtener los DTOs con joins
                var query = queryBuilder(_context);
                var listaDTOs = query.ToList();

                // Aplicar filtro custom si existe
                if (filtroCustom != null)
                {
                    listaDTOs = listaDTOs.Where(filtroCustom).ToList();
                }

                // Orden dinámico
                var propertyInfo = typeof(TDTO).GetProperty(orderBy);
                if (propertyInfo == null)
                {
                    // Si no existe la propiedad, intentar ordenar por la primera propiedad
                    var firstProperty = typeof(TDTO).GetProperties().FirstOrDefault();
                    if (firstProperty != null)
                    {
                        listaDTOs = ascOrDesc == "desc"
                            ? listaDTOs.OrderByDescending(p => firstProperty.GetValue(p)).ToList()
                            : listaDTOs.OrderBy(p => firstProperty.GetValue(p)).ToList();
                    }
                }
                else
                {
                    listaDTOs = ascOrDesc == "desc"
                        ? listaDTOs.OrderByDescending(p => propertyInfo.GetValue(p)).ToList()
                        : listaDTOs.OrderBy(p => propertyInfo.GetValue(p)).ToList();
                }

                // Paginación
                int totalRecords = listaDTOs.Count;
                var listaPaginada = listaDTOs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var response = new PaginatedResponse<TDTO>
                {
                    TotalRecords = totalRecords,
                    Data = listaPaginada
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
