namespace Core.Entities
{    
    public record Cliente 
    {
        public string Nombre { get; private set; }
        public string Codigo { get; private set; }
        public string Etiqueta { get; private set; }
        public bool Activo { get; private set; }
        public string RutaOrigenSftp { get; private set; }
        public string RutaOrigenEmail { get; private set; }
        public string RutaOrigenSftpGuane { get; private set; }
        public string RutaOrigenEmailGuane { get; private set; }        
        public string RutaOrigenMasivos { get; private set; }
        public string RutaIntegracionMasivos { get; private set; }
        public string RutaProcesadosGuaneSftp { get; private set; }
        public string RutaProcesadosGuaneEmail { get; private set; }
        public string RutaError { get; private set; }
        public string RutaAlmacenada { get; init; }

        // Métodos de dominio
        public string ObtenerRutaOrigen(TipoOrigen origen)
        {
            return origen switch
            {
                TipoOrigen.Sftp => RutaOrigenSftp,
                TipoOrigen.Email => RutaOrigenEmail,
                TipoOrigen.Masivos => RutaOrigenMasivos,
                _ => throw new ArgumentOutOfRangeException(nameof(origen), origen, null)
            };
        }

        public string ObtenerRutaDestino(TipoOrigen origen)
        {
            return origen switch
            {
                TipoOrigen.Sftp => RutaOrigenSftpGuane,
                TipoOrigen.Email => RutaOrigenEmailGuane,
                TipoOrigen.Masivos => RutaIntegracionMasivos,
                _ => throw new ArgumentOutOfRangeException(nameof(origen), origen, null)
            };
        }
        public string ObtenerRutaDestinoGuane(TipoOrigen origen)
        {
            return origen switch
            {
                TipoOrigen.Sftp => RutaProcesadosGuaneSftp,
                TipoOrigen.Email => RutaProcesadosGuaneEmail,                
                _ => throw new ArgumentOutOfRangeException(nameof(origen), origen, null)
            };
        }
    }

    public enum TipoOrigen
    {
        Sftp = 1,
        Email = 2,
        Masivos = 3
    }
}
