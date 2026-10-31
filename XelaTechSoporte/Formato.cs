namespace XelaTechSoporte;

// Clase de apoyo para mostrar los datos con un texto mas amigable.
// Se usa en la consola, en los reportes y en el modo grafico.
public static class Formato
{
    public static string Dinero(decimal valor)
    {
        return "Q " + valor.ToString("0.00");
    }

    public static string Fecha(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy HH:mm");
    }

    public static string NombreEstado(EstadoSolicitud estado)
    {
        switch (estado)
        {
            case EstadoSolicitud.Recibida: return "Recibida";
            case EstadoSolicitud.EnEspera: return "En espera";
            case EstadoSolicitud.Asignada: return "Asignada";
            case EstadoSolicitud.EnDiagnostico: return "En diagnóstico";
            case EstadoSolicitud.EnReparacion: return "En reparación";
            case EstadoSolicitud.ListaParaEntregar: return "Lista para entregar";
            case EstadoSolicitud.Entregada: return "Entregada";
            case EstadoSolicitud.Cancelada: return "Cancelada";
            default: return estado.ToString();
        }
    }

    public static string NombreTipoCliente(TipoCliente tipo)
    {
        switch (tipo)
        {
            case TipoCliente.Estudiante: return "Estudiante";
            case TipoCliente.Profesional: return "Profesional";
            case TipoCliente.PequenoNegocio: return "Pequeño negocio";
            case TipoCliente.Institucion: return "Institución";
            default: return tipo.ToString();
        }
    }

    // Corta un texto largo para que las columnas de las tablas no se descuadren
    public static string Recortar(string texto, int maximo)
    {
        if (texto == null) return "";
        if (texto.Length <= maximo) return texto;
        return texto.Substring(0, maximo - 3) + "...";
    }

    // Arma una columna de ancho fijo para las tablas de consola
    public static string Col(string texto, int ancho)
    {
        return Recortar(texto, ancho - 1).PadRight(ancho);
    }
}
