using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GravNetCore.Helpers;

namespace GravNetCore.Services
{
    /// <summary>
    /// Interfaz para servicio genérico de operaciones CRUD con Entity Framework
    /// </summary>
    /// <typeparam name="TEntity">Tipo de la entidad en la base de datos</typeparam>
    /// <typeparam name="TDTO">Tipo del DTO (Data Transfer Object)</typeparam>
    /// <typeparam name="TContext">Tipo del DbContext</typeparam>
    public interface IGenericService<TEntity, TDTO, TContext>
        where TEntity : class, new()
        where TDTO : class, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Lista todos los registros sin filtro
        /// </summary>
        /// <returns>Lista completa de DTOs</returns>
        List<TDTO> ListarCompleto();

        /// <summary>
        /// Lista registros para dropdown (id + texto)
        /// </summary>
        /// <param name="idPropertyName">Nombre de la propiedad ID en la entidad</param>
        /// <param name="textPropertyName">Nombre de la propiedad de texto en la entidad</param>
        /// <returns>ActionResult con lista de objetos anónimos {value, label}</returns>
        IActionResult ListarSelect(string idPropertyName, string textPropertyName);

        /// <summary>
        /// Recupera un registro por ID
        /// </summary>
        /// <param name="id">ID del registro a buscar</param>
        /// <param name="entityIdProperty">Nombre de la propiedad ID en la Entity (ej: "NoMesA")</param>
        /// <param name="dtoIdProperty">Nombre de la propiedad ID en el DTO (ej: "noMesA")</param>
        /// <returns>DTO del registro o null si no existe</returns>
        TDTO? Recuperar(int id, string entityIdProperty, string dtoIdProperty);

        /// <summary>
        /// Elimina un registro por ID
        /// </summary>
        /// <param name="id">ID del registro a eliminar</param>
        /// <param name="entityIdProperty">Nombre de la propiedad ID en la Entity (ej: "NoMesA")</param>
        /// <returns>1 si se eliminó correctamente, 0 si hubo error o no se encontró</returns>
        int Borrar(int id, string entityIdProperty);

        /// <summary>
        /// Inserta o actualiza un registro
        /// </summary>
        /// <param name="dto">DTO con los datos a guardar</param>
        /// <param name="dtoIdProperty">Nombre de la propiedad ID en el DTO (ej: "noMesA")</param>
        /// <param name="entityIdProperty">Nombre de la propiedad ID en la Entity (ej: "NoMesA")</param>
        /// <returns>1 si se guardó correctamente, 0 si hubo error</returns>
        int Guardar(TDTO dto, string dtoIdProperty, string entityIdProperty);

        /// <summary>
        /// Lista con paginación, filtros y ordenamiento
        /// </summary>
        /// <param name="pageNumber">Número de página (base 1)</param>
        /// <param name="pageSize">Cantidad de registros por página</param>
        /// <param name="ascOrDesc">Dirección del ordenamiento: "asc" o "desc"</param>
        /// <param name="orderBy">Nombre de la propiedad del DTO por la cual ordenar</param>
        /// <param name="filtroCustom">Función opcional para filtrar las entidades</param>
        /// <returns>ActionResult con respuesta paginada</returns>
        Task<ActionResult<PaginatedResponse<TDTO>>> ListarPaginadoConFiltros(
            int pageNumber,
            int pageSize,
            string ascOrDesc,
            string orderBy,
            Func<TEntity, bool>? filtroCustom = null
        );

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
        Task<ActionResult<PaginatedResponse<TDTO>>> ListarPaginadoConQuery(
            Func<TContext, IQueryable<TDTO>> queryBuilder,
            int pageNumber,
            int pageSize,
            string ascOrDesc,
            string orderBy,
            Func<TDTO, bool>? filtroCustom = null
        );
    }
}
