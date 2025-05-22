namespace Core.Entities
{
    public record IQEtiqueta(
        string Codigo,
        string Etiqueta,
        int Longitud,
        string Condicion,
        long Consecutivo,
        string? ConsecutivoEtiqueta,
        int SeparacionCliente
    )
    {
        public string GenerarEtiqueta()
        {
            var consecutivoStr = Consecutivo.ToString().PadLeft(Longitud - Etiqueta.Length - Condicion.Length, '0');
            return $"{Etiqueta}{Condicion}{consecutivoStr}";
        }
    }

}
