namespace XelaTechSoporte;

// Matriz de carga semanal: filas = tecnicos, columnas = dias.
// Cada celda guarda cuantas solicitudes se le asignaron a ese tecnico ese dia.
public class MatrizCarga
{
    private int[,] carga;
    private string[] dias = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

    public MatrizCarga()
    {
        // Empieza sin filas; crece cuando se asigna trabajo a un tecnico nuevo
        carga = new int[0, 7];
    }

    public string[] Dias
    {
        get { return dias; }
    }

    public int CantidadFilas
    {
        get { return carga.GetLength(0); }
    }

    public void RegistrarAsignacion(int idxTec, int dia)
    {
        if (idxTec < 0)
            throw new Exception("El técnico no existe en la matriz.");
        if (dia < 0 || dia > 6)
            throw new Exception("El día debe estar entre 0 (lunes) y 6 (domingo).");

        // Si el tecnico todavia no tiene fila, se agranda la matriz
        if (idxTec >= carga.GetLength(0))
        {
            AgrandarMatriz(idxTec + 1);
        }
        carga[idxTec, dia]++;
    }

    // Crea una matriz mas grande y copia los valores anteriores
    private void AgrandarMatriz(int nuevasFilas)
    {
        int[,] nueva = new int[nuevasFilas, 7];
        for (int i = 0; i < carga.GetLength(0); i++)
        {
            for (int j = 0; j < 7; j++)
            {
                nueva[i, j] = carga[i, j];
            }
        }
        carga = nueva;
    }

    public int ObtenerCarga(int idxTec, int dia)
    {
        if (idxTec < 0 || idxTec >= carga.GetLength(0)) return 0;
        return carga[idxTec, dia];
    }

    public int TotalTecnico(int idxTec)
    {
        int total = 0;
        for (int j = 0; j < 7; j++)
        {
            total += ObtenerCarga(idxTec, j);
        }
        return total;
    }

    public int TotalDia(int dia, int cantidadTecnicos)
    {
        int total = 0;
        for (int i = 0; i < cantidadTecnicos; i++)
        {
            total += ObtenerCarga(i, dia);
        }
        return total;
    }

    // Convierte una fecha al numero de columna (lunes = 0 ... domingo = 6)
    public static int IndiceDia(DateTime fecha)
    {
        // En C# DayOfWeek empieza en domingo = 0, por eso se corre
        return ((int)fecha.DayOfWeek + 6) % 7;
    }

    // Texto con el tecnico y el dia de mayor carga
    public string TecnicoYDiaMayorCarga(List<Tecnico> tecnicos)
    {
        if (tecnicos.Count == 0)
            return "No hay técnicos registrados.";

        // Tecnico con mas asignaciones en la semana (suma de su fila)
        int mejorTec = 0;
        for (int i = 1; i < tecnicos.Count; i++)
        {
            if (TotalTecnico(i) > TotalTecnico(mejorTec)) mejorTec = i;
        }

        // Dia con mas asignaciones (suma de la columna)
        int mejorDia = 0;
        for (int j = 1; j < 7; j++)
        {
            if (TotalDia(j, tecnicos.Count) > TotalDia(mejorDia, tecnicos.Count)) mejorDia = j;
        }

        if (TotalTecnico(mejorTec) == 0)
            return "Todavía no hay asignaciones registradas en la matriz.";

        // Celda con el valor mas alto
        int filaMax = 0, colMax = 0;
        for (int i = 0; i < tecnicos.Count; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (ObtenerCarga(i, j) > ObtenerCarga(filaMax, colMax))
                {
                    filaMax = i;
                    colMax = j;
                }
            }
        }

        return "Técnico con mayor carga: " + tecnicos[mejorTec].NombreCompleto + " (" + TotalTecnico(mejorTec) + " asignaciones)\n" +
               "Día con mayor carga: " + dias[mejorDia] + " (" + TotalDia(mejorDia, tecnicos.Count) + " asignaciones)\n" +
               "Celda más alta: " + tecnicos[filaMax].NombreCompleto + " el " + dias[colMax] + " (" + ObtenerCarga(filaMax, colMax) + ")";
    }

    // Arma la tabla como texto
    public string Mostrar(List<Tecnico> tecnicos)
    {
        string texto = "";
        texto += Formato.Col("Técnico", 22);
        for (int j = 0; j < 7; j++)
        {
            texto += Formato.Col(dias[j].Substring(0, 3), 6);
        }
        texto += "Total\n";
        texto += new string('-', 22 + 6 * 7 + 5) + "\n";

        for (int i = 0; i < tecnicos.Count; i++)
        {
            texto += Formato.Col(tecnicos[i].NombreCompleto, 22);
            for (int j = 0; j < 7; j++)
            {
                texto += Formato.Col(ObtenerCarga(i, j).ToString(), 6);
            }
            texto += TotalTecnico(i) + "\n";
        }

        texto += new string('-', 22 + 6 * 7 + 5) + "\n";
        texto += Formato.Col("Total por día", 22);
        for (int j = 0; j < 7; j++)
        {
            texto += Formato.Col(TotalDia(j, tecnicos.Count).ToString(), 6);
        }
        texto += "\n\n" + TecnicoYDiaMayorCarga(tecnicos) + "\n";
        return texto;
    }
}
