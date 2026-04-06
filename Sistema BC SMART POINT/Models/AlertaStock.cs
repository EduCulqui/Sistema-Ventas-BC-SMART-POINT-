namespace Sistema_BC_SMART_POINT.Models
{
    public class AlertaStock
    {
        public int IdAlertaStock { get; set; }

        public DateTime FechaAlerta { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaResolucion { get; set; }

        public bool Estado { get; set; }
    }
}
