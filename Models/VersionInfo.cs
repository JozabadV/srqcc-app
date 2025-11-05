namespace SRQCC.Models
{
    public class VersionInfo
    {
        public string version { get; set; }
        public int version_code { get; set; }
        public bool es_obligatoria { get; set; }
        public string descripcion { get; set; }
        public string url_descarga { get; set; }
        public DateTime fecha_publicacion { get; set; }
    }
}