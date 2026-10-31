namespace XelaTechSoporte;

// Ordenamientos programados a mano (no se usa Sort, OrderBy ni ThenBy).
// Todos ordenan una COPIA de la lista, asi la lista original no cambia
// y sigue ordenada por codigo para la busqueda binaria.
public static class Ordenador
{
    // ================= 1. BUBBLE SORT =================
    // Solicitudes por costo total.
    // Compara cada elemento con el siguiente y los intercambia si estan al reves.
    public static List<SolicitudServicio> BubbleSortPorCosto(List<SolicitudServicio> lista, bool asc)
    {
        if (lista.Count < 2)
            throw new Exception("Se necesitan al menos 2 solicitudes para ordenar.");

        List<SolicitudServicio> copia = new List<SolicitudServicio>(lista);
        int n = copia.Count;

        for (int i = 0; i < n - 1; i++)
        {
            bool huboCambio = false;

            // En cada pasada el valor extremo queda al final, por eso se recorre hasta n - 1 - i
            for (int j = 0; j < n - 1 - i; j++)
            {
                bool intercambiar;
                if (asc)
                    intercambiar = copia[j].Total > copia[j + 1].Total;
                else
                    intercambiar = copia[j].Total < copia[j + 1].Total;

                if (intercambiar)
                {
                    SolicitudServicio temp = copia[j];
                    copia[j] = copia[j + 1];
                    copia[j + 1] = temp;
                    huboCambio = true;
                }
            }

            // Si no hubo ningun intercambio la lista ya esta ordenada
            if (!huboCambio) break;
        }
        return copia;
    }

    // ================= 2. SELECTION SORT =================
    // Tecnicos por cantidad de servicios finalizados.
    // En cada vuelta busca el menor (o mayor) de lo que falta y lo pone en su lugar.
    public static List<Tecnico> SelectionSortPorFinalizados(List<Tecnico> lista, bool asc)
    {
        if (lista.Count < 2)
            throw new Exception("Se necesitan al menos 2 técnicos para ordenar.");

        List<Tecnico> copia = new List<Tecnico>(lista);
        int n = copia.Count;

        for (int i = 0; i < n - 1; i++)
        {
            int posElegido = i;
            for (int j = i + 1; j < n; j++)
            {
                if (asc)
                {
                    if (copia[j].ServiciosFinalizados < copia[posElegido].ServiciosFinalizados) posElegido = j;
                }
                else
                {
                    if (copia[j].ServiciosFinalizados > copia[posElegido].ServiciosFinalizados) posElegido = j;
                }
            }

            // Un solo intercambio por vuelta
            if (posElegido != i)
            {
                Tecnico temp = copia[i];
                copia[i] = copia[posElegido];
                copia[posElegido] = temp;
            }
        }
        return copia;
    }

    // ================= 3. SHELL SORT =================
    // Repuestos por existencia.
    // Es como insercion pero comparando elementos separados por una "brecha"
    // que empieza en n/2 y se va partiendo a la mitad hasta llegar a 1.
    public static List<Repuesto> ShellSortPorExistencia(List<Repuesto> lista, bool asc)
    {
        if (lista.Count < 2)
            throw new Exception("Se necesitan al menos 2 repuestos para ordenar.");

        List<Repuesto> copia = new List<Repuesto>(lista);
        int n = copia.Count;

        for (int brecha = n / 2; brecha > 0; brecha = brecha / 2)
        {
            for (int i = brecha; i < n; i++)
            {
                Repuesto actual = copia[i];
                int j = i;

                // Se mueven hacia adelante los elementos que estan en el orden contrario
                while (j >= brecha && DebeIrAntes(actual, copia[j - brecha], asc))
                {
                    copia[j] = copia[j - brecha];
                    j = j - brecha;
                }
                copia[j] = actual;
            }
        }
        return copia;
    }

    private static bool DebeIrAntes(Repuesto a, Repuesto b, bool asc)
    {
        if (asc)
            return a.Existencia < b.Existencia;
        else
            return a.Existencia > b.Existencia;
    }

    // ================= 4. QUICK SORT =================
    // Solicitudes por fecha de recepcion.
    // Se elige un pivote y se dejan a un lado las que van antes y al otro las que van despues;
    // luego se repite (recursividad) en cada lado.
    public static List<SolicitudServicio> QuickSortPorFecha(List<SolicitudServicio> lista, bool asc)
    {
        if (lista.Count < 2)
            throw new Exception("Se necesitan al menos 2 solicitudes para ordenar.");

        List<SolicitudServicio> copia = new List<SolicitudServicio>(lista);
        QuickSort(copia, 0, copia.Count - 1, asc);
        return copia;
    }

    private static void QuickSort(List<SolicitudServicio> lista, int inicio, int fin, bool asc)
    {
        // Caso base: un elemento o ninguno ya esta ordenado
        if (inicio >= fin) return;

        int posPivote = Particion(lista, inicio, fin, asc);
        QuickSort(lista, inicio, posPivote - 1, asc);
        QuickSort(lista, posPivote + 1, fin, asc);
    }

    // Usa el ultimo elemento como pivote
    private static int Particion(List<SolicitudServicio> lista, int inicio, int fin, bool asc)
    {
        DateTime pivote = lista[fin].FechaRecepcion;
        int i = inicio - 1; // ultima posicion del lado "antes del pivote"

        for (int j = inicio; j < fin; j++)
        {
            bool vaAntes;
            if (asc)
                vaAntes = lista[j].FechaRecepcion <= pivote;
            else
                vaAntes = lista[j].FechaRecepcion >= pivote;

            if (vaAntes)
            {
                i++;
                Intercambiar(lista, i, j);
            }
        }

        // El pivote se coloca en su posicion final
        Intercambiar(lista, i + 1, fin);
        return i + 1;
    }

    private static void Intercambiar(List<SolicitudServicio> lista, int a, int b)
    {
        SolicitudServicio temp = lista[a];
        lista[a] = lista[b];
        lista[b] = temp;
    }
}
