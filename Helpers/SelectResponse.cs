namespace GravNetCore.Helpers
{
    /// <summary>
    /// Respuesta para dropdowns/selects que contiene un identificador y su texto descriptivo
    /// </summary>
    public class SelectResponse
    {
        /// <summary>
        /// Identificador del elemento (ID)
        /// </summary>
        public int value { get; set; }

        /// <summary>
        /// Texto descriptivo del elemento que se muestra en el dropdown
        /// </summary>
        public string label { get; set; } = string.Empty;
    }
}
