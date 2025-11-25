namespace GravNetCore.Helpers
{
    /// <summary>
    /// Respuesta paginada genérica que contiene el total de registros y los datos de la página actual
    /// </summary>
    /// <typeparam name="T">Tipo de datos en la colección</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Total de registros disponibles (sin paginar)
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Datos de la página actual
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();
    }
}
