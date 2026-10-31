namespace XelaTechSoporte;

// Busquedas programadas a mano (no se usa BinarySearch, Find ni Where)
public static class Buscador
{
    // Cuantas comparaciones hizo la ultima busqueda binaria (para mostrarlo en pantalla)
    public static int ComparacionesBinaria = 0;

    // ================= BUSQUEDA SECUENCIAL =================
    // Recorre la lista completa desde el inicio y guarda TODAS las coincidencias.

    // Clientes por nombre parcial (sin importar mayusculas)
    public static List<Cliente> SecuencialClientesPorNombre(List<Cliente> lista, string txt)
    {
        if (lista.Count == 0)
            throw new Exception("No hay clientes registrados para buscar.");
        if (string.IsNullOrWhiteSpace(txt))
            throw new Exception("Debe escribir un texto para buscar.");

        string buscado = txt.Trim().ToLower();
        List<Cliente> encontrados = new List<Cliente>();

        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i].NombreCompleto.ToLower().Contains(buscado))
            {
                encontrados.Add(lista[i]);
            }
        }
        return encontrados;
    }

    // Solicitudes por estado exacto
    public static List<SolicitudServicio> SecuencialSolicitudesPorEstado(List<SolicitudServicio> lista, EstadoSolicitud est)
    {
        if (lista.Count == 0)
            throw new Exception("No hay solicitudes registradas para buscar.");

        List<SolicitudServicio> encontrados = new List<SolicitudServicio>();
        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i].Estado == est)
            {
                encontrados.Add(lista[i]);
            }
        }
        return encontrados;
    }

    // Equipos por marca (sin importar mayusculas)
    public static List<Equipo> SecuencialEquiposPorMarca(List<Equipo> lista, string marca)
    {
        if (lista.Count == 0)
            throw new Exception("No hay equipos registrados para buscar.");
        if (string.IsNullOrWhiteSpace(marca))
            throw new Exception("Debe escribir la marca a buscar.");

        string buscada = marca.Trim().ToLower();
        List<Equipo> encontrados = new List<Equipo>();

        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i].Marca.ToLower() == buscada)
            {
                encontrados.Add(lista[i]);
            }
        }
        return encontrados;
    }

    // ================= BUSQUEDA BINARIA =================

    // Revisa que cada codigo sea menor que el siguiente
    public static bool EstaOrdenadaPorCodigo(List<SolicitudServicio> lista)
    {
        for (int i = 0; i < lista.Count - 1; i++)
        {
            if (string.Compare(lista[i].Codigo, lista[i + 1].Codigo, StringComparison.Ordinal) > 0)
            {
                return false;
            }
        }
        return true;
    }

    // Busca una solicitud por codigo partiendo la lista en mitades.
    // Devuelve null si no la encuentra.
    public static SolicitudServicio BinariaSolicitudPorCodigo(List<SolicitudServicio> lista, string cod)
    {
        ComparacionesBinaria = 0;

        if (lista.Count == 0)
            throw new Exception("No hay solicitudes registradas para buscar.");
        if (string.IsNullOrWhiteSpace(cod))
            throw new Exception("Debe escribir el código de la solicitud.");
        // Validacion 11: la lista debe estar ordenada por el mismo campo que se compara
        if (!EstaOrdenadaPorCodigo(lista))
            throw new Exception("La lista no está ordenada por código; no se puede usar búsqueda binaria.");

        string buscado = cod.Trim().ToUpper();
        int inicio = 0;
        int fin = lista.Count - 1;

        while (inicio <= fin)
        {
            int medio = (inicio + fin) / 2;
            ComparacionesBinaria++;

            int comparacion = string.Compare(lista[medio].Codigo, buscado, StringComparison.Ordinal);

            if (comparacion == 0)
                return lista[medio];          // lo encontro
            else if (comparacion < 0)
                inicio = medio + 1;           // el buscado esta en la mitad de arriba
            else
                fin = medio - 1;              // el buscado esta en la mitad de abajo
        }
        return null;
    }
}
