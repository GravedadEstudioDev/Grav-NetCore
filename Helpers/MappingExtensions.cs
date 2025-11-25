using System.Reflection;

namespace GravNetCore.Helpers
{
    /// <summary>
    /// Extensiones para mapeo automático entre objetos (similar a AutoMapper)
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Mapea propiedades de un objeto origen a un nuevo objeto destino
        /// </summary>
        /// <typeparam name="TDestination">Tipo del objeto destino</typeparam>
        /// <param name="source">Objeto origen</param>
        /// <returns>Nueva instancia del tipo destino con propiedades mapeadas</returns>
        public static TDestination MapTo<TDestination>(object source) where TDestination : class, new()
        {
            var destination = new TDestination();
            MapProperties(source, destination);
            return destination;
        }

        /// <summary>
        /// Mapea propiedades de un objeto origen a un objeto destino existente.
        /// Compara nombres de propiedades ignorando mayúsculas/minúsculas
        /// </summary>
        /// <param name="source">Objeto origen</param>
        /// <param name="destination">Objeto destino</param>
        public static void MapProperties(object source, object destination)
        {
            if (source == null || destination == null) return;

            var sourceType = source.GetType();
            var destinationType = destination.GetType();

            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var sourceProp in sourceProperties)
            {
                if (!sourceProp.CanRead) continue;

                // Buscar propiedad destino con nombre similar (case-insensitive)
                var destProp = destinationProperties.FirstOrDefault(p =>
                    p.Name.Equals(sourceProp.Name, StringComparison.OrdinalIgnoreCase) &&
                    p.CanWrite
                );

                if (destProp == null) continue;

                try
                {
                    var sourceValue = sourceProp.GetValue(source);

                    // Validar compatibilidad de tipos
                    if (IsCompatibleType(sourceProp.PropertyType, destProp.PropertyType))
                    {
                        destProp.SetValue(destination, sourceValue);
                    }
                }
                catch
                {
                    // Ignorar propiedades que no se puedan mapear
                    continue;
                }
            }
        }

        /// <summary>
        /// Verifica si dos tipos son compatibles para mapeo
        /// </summary>
        private static bool IsCompatibleType(Type sourceType, Type destType)
        {
            // Tipos exactamente iguales
            if (sourceType == destType) return true;

            // Nullable a no-nullable del mismo tipo base
            var sourceUnderlyingType = Nullable.GetUnderlyingType(sourceType);
            var destUnderlyingType = Nullable.GetUnderlyingType(destType);

            if (sourceUnderlyingType != null && sourceUnderlyingType == destType) return true;
            if (destUnderlyingType != null && destUnderlyingType == sourceType) return true;
            if (sourceUnderlyingType != null && destUnderlyingType != null && sourceUnderlyingType == destUnderlyingType) return true;

            // Tipos asignables (herencia, interfaces)
            if (destType.IsAssignableFrom(sourceType)) return true;

            return false;
        }
    }
}
