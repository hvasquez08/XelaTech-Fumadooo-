namespace XelaTechSoporte;

// Los 12 reportes minimos. Cada metodo devuelve el reporte como texto;
// la consola lo imprime y el modo grafico lo muestra en la pagina.
// Todo se calcula con los datos que hay en memoria en ese momento.
public class Reportes
{
    private GestorClientes gestorClientes;
    private GestorTecnicos gestorTecnicos;
    private GestorEquipos gestorEquipos;
    private GestorRepuestos gestorRepuestos;
    private GestorSolicitudes gestorSolicitudes;
    private MatrizCarga matriz;

    public Reportes(GestorClientes gc, GestorTecnicos gt, GestorEquipos ge, GestorRepuestos gr,
                    GestorSolicitudes gs, MatrizCarga m)
    {
        gestorClientes = gc;
        gestorTecnicos = gt;
        gestorEquipos = ge;
        gestorRepuestos = gr;
        gestorSolicitudes = gs;
        matriz = m;
    }

    // ===== Ayudas para armar el texto =====

    private string Linea()
    {
        return new string('-', 84) + "\n";
    }

    private string Titulo(string titulo)
    {
        return "REPORTE: " + titulo.ToUpper() + "\n" + Linea();
    }

    private string TextoOrden(bool asc)
    {
        return asc ? "Ascendente" : "Descendente";
    }

    private string EncabezadoSolicitudes()
    {
        return Formato.Col("Código", 10) + Formato.Col("Cliente", 22) + Formato.Col("Equipo", 20) +
               Formato.Col("Estado", 21) + "Total\n";
    }

    private string FilaSolicitud(SolicitudServicio s)
    {
        return Formato.Col(s.Codigo, 10) + Formato.Col(s.Cliente.NombreCompleto, 22) +
               Formato.Col(s.Equipo.Descripcion(), 20) + Formato.Col(Formato.NombreEstado(s.Estado), 21) +
               Formato.Dinero(s.Total) + "\n";
    }

    private string TablaSolicitudes(List<SolicitudServicio> lista)
    {
        if (lista.Count == 0) return "  (no hay solicitudes)\n";

        string texto = EncabezadoSolicitudes();
        foreach (SolicitudServicio s in lista)
        {
            texto += FilaSolicitud(s);
        }
        return texto;
    }

    // ===== Reporte 1 =====
    public string SolicitudesEnEspera()
    {
        string texto = Titulo("Solicitudes en espera por prioridad");
        texto += BloqueCola("URGENTE", gestorSolicitudes.ColaUrgente);
        texto += "\n";
        texto += BloqueCola("NORMAL", gestorSolicitudes.ColaNormal);
        return texto;
    }

    private string BloqueCola(string nombre, Queue<SolicitudServicio> cola)
    {
        string texto = "Cola " + nombre + " (" + cola.Count + " en espera, se atiende de arriba hacia abajo)\n";
        if (cola.Count == 0) return texto + "  (vacía)\n";

        texto += Formato.Col("#", 4) + Formato.Col("Código", 10) + Formato.Col("Cliente", 24) +
                 Formato.Col("Equipo", 22) + "Fecha\n";
        int n = 1;
        // Recorrer la cola con foreach no saca elementos
        foreach (SolicitudServicio s in cola)
        {
            texto += Formato.Col(n.ToString(), 4) + Formato.Col(s.Codigo, 10) + Formato.Col(s.Cliente.NombreCompleto, 24) +
                     Formato.Col(s.Equipo.Descripcion(), 22) + Formato.Fecha(s.FechaRecepcion) + "\n";
            n++;
        }
        return texto;
    }

    // ===== Reporte 2 =====
    public string SolicitudesActivasFinalizadasCanceladas()
    {
        List<SolicitudServicio> activas = new List<SolicitudServicio>();
        List<SolicitudServicio> finalizadas = new List<SolicitudServicio>();
        List<SolicitudServicio> canceladas = new List<SolicitudServicio>();

        foreach (SolicitudServicio s in gestorSolicitudes.Solicitudes)
        {
            if (s.Estado == EstadoSolicitud.Entregada) finalizadas.Add(s);
            else if (s.Estado == EstadoSolicitud.Cancelada) canceladas.Add(s);
            else activas.Add(s);
        }

        string texto = Titulo("Solicitudes activas, finalizadas y canceladas");
        texto += "ACTIVAS (" + activas.Count + ")\n" + TablaSolicitudes(activas) + "\n";
        texto += "FINALIZADAS / ENTREGADAS (" + finalizadas.Count + ")\n" + TablaSolicitudes(finalizadas) + "\n";
        texto += "CANCELADAS (" + canceladas.Count + ")\n" + TablaSolicitudes(canceladas);
        return texto;
    }

    // ===== Reporte 3 =====
    public string TecnicosDisponiblesYCargaCompleta()
    {
        List<Tecnico> disponibles = new List<Tecnico>();
        List<Tecnico> completos = new List<Tecnico>();
        List<Tecnico> noDisponibles = new List<Tecnico>();

        foreach (Tecnico t in gestorTecnicos.Tecnicos)
        {
            if (t.TrabajosActivos >= Tecnico.LIMITE_ACTIVOS) completos.Add(t);
            else if (!t.Disponible) noDisponibles.Add(t);
            else disponibles.Add(t);
        }

        string texto = Titulo("Técnicos disponibles y con carga completa");
        texto += "DISPONIBLES (" + disponibles.Count + ")\n" + TablaTecnicos(disponibles) + "\n";
        texto += "CON CARGA COMPLETA (" + completos.Count + ")\n" + TablaTecnicos(completos) + "\n";
        texto += "MARCADOS COMO NO DISPONIBLES (" + noDisponibles.Count + ")\n" + TablaTecnicos(noDisponibles);
        return texto;
    }

    private string TablaTecnicos(List<Tecnico> lista)
    {
        if (lista.Count == 0) return "  (ninguno)\n";

        string texto = Formato.Col("Código", 10) + Formato.Col("Nombre", 26) + Formato.Col("Especialidad", 15) +
                       Formato.Col("Activos", 10) + Formato.Col("Finalizados", 13) + "Promedio\n";
        foreach (Tecnico t in lista)
        {
            texto += Formato.Col(t.Codigo, 10) + Formato.Col(t.NombreCompleto, 26) + Formato.Col(t.Especialidad.ToString(), 15) +
                     Formato.Col(t.TrabajosActivos + "/" + Tecnico.LIMITE_ACTIVOS, 10) +
                     Formato.Col(t.ServiciosFinalizados.ToString(), 13) + t.CalificacionPromedio.ToString("0.00") + "\n";
        }
        return texto;
    }

    // ===== Reporte 4 (usa Selection Sort) =====
    public string TecnicoConMasFinalizados()
    {
        string texto = Titulo("Técnico con más servicios finalizados");
        List<Tecnico> tecnicos = gestorTecnicos.Tecnicos;

        if (tecnicos.Count == 0) return texto + "No hay técnicos registrados.\n";

        Tecnico mejor;
        if (tecnicos.Count == 1)
        {
            mejor = tecnicos[0];
        }
        else
        {
            // Se ordena de mayor a menor y el primero es el que mas finalizo
            List<Tecnico> ordenados = Ordenador.SelectionSortPorFinalizados(tecnicos, false);
            mejor = ordenados[0];
            texto += "Algoritmo: Selection Sort | Criterio: Servicios finalizados | Orden: Descendente\n\n";
        }

        if (mejor.ServiciosFinalizados == 0)
            return texto + "Ningún técnico ha finalizado servicios todavía.\n";

        texto += "Nombre:                " + mejor.NombreCompleto + " (" + mejor.Codigo + ")\n";
        texto += "Especialidad:          " + mejor.Especialidad + "\n";
        texto += "Servicios finalizados: " + mejor.ServiciosFinalizados + "\n";
        texto += "Calificación promedio: " + mejor.CalificacionPromedio.ToString("0.00") +
                 " (" + mejor.TotalCalificaciones + " calificaciones)\n";

        // Distribucion de calificaciones usando el arreglo unidimensional
        texto += "Distribución:          ";
        for (int i = 0; i < 5; i++)
        {
            texto += "[" + (i + 1) + "]: " + mejor.Calificaciones[i] + "   ";
        }
        return texto + "\n";
    }

    // Para el modulo de Ordenamientos (Selection Sort completo)
    public string TecnicosPorFinalizados(bool asc)
    {
        List<Tecnico> ordenados = Ordenador.SelectionSortPorFinalizados(gestorTecnicos.Tecnicos, asc);

        string texto = Titulo("Técnicos ordenados por servicios finalizados");
        texto += "Algoritmo: Selection Sort | Criterio: Servicios finalizados | Orden: " + TextoOrden(asc) + "\n";
        texto += Linea();
        texto += TablaTecnicos(ordenados);
        texto += Linea() + "Total de técnicos: " + ordenados.Count + "\n";
        return texto;
    }

    // ===== Reporte 5 =====
    public string EquiposPorTipo()
    {
        // Arreglos de contadores: posicion 0 = Laptop, 1 = Escritorio, 2 = Impresora
        string[] tipos = { "Laptop", "Escritorio", "Impresora" };
        int[] registrados = new int[3];
        int[] atendidos = new int[3];

        foreach (Equipo e in gestorEquipos.Equipos)
        {
            int pos = 0;
            if (e is Escritorio) pos = 1;
            else if (e is Impresora) pos = 2;

            registrados[pos]++;
            if (TieneSolicitudes(e)) atendidos[pos]++;
        }

        string texto = Titulo("Equipos atendidos por tipo");
        texto += Formato.Col("Tipo", 16) + Formato.Col("Registrados", 14) + "Con solicitud de servicio\n";
        int totalReg = 0, totalAte = 0;
        for (int i = 0; i < 3; i++)
        {
            texto += Formato.Col(tipos[i], 16) + Formato.Col(registrados[i].ToString(), 14) + atendidos[i] + "\n";
            totalReg += registrados[i];
            totalAte += atendidos[i];
        }
        texto += Linea();
        texto += Formato.Col("TOTAL", 16) + Formato.Col(totalReg.ToString(), 14) + totalAte + "\n";
        return texto;
    }

    private bool TieneSolicitudes(Equipo e)
    {
        foreach (SolicitudServicio s in gestorSolicitudes.Solicitudes)
        {
            if (s.Equipo == e) return true;
        }
        return false;
    }

    // ===== Reporte 6 (Bubble Sort) =====
    public string SolicitudesPorCosto(bool asc)
    {
        List<SolicitudServicio> ordenadas = Ordenador.BubbleSortPorCosto(gestorSolicitudes.Solicitudes, asc);

        string texto = Titulo("Solicitudes ordenadas por costo total");
        texto += "Algoritmo: Bubble Sort | Criterio: Total | Orden: " + TextoOrden(asc) + "\n";
        texto += Linea();
        texto += TablaSolicitudes(ordenadas);
        texto += Linea() + "Total de solicitudes: " + ordenadas.Count + "\n";
        return texto;
    }

    // ===== Reporte 7 (Quick Sort) =====
    public string SolicitudesPorFecha(bool asc)
    {
        List<SolicitudServicio> ordenadas = Ordenador.QuickSortPorFecha(gestorSolicitudes.Solicitudes, asc);

        string texto = Titulo("Solicitudes ordenadas por fecha de recepción");
        texto += "Algoritmo: Quick Sort | Criterio: Fecha de recepción | Orden: " + TextoOrden(asc) + "\n";
        texto += Linea();
        texto += Formato.Col("Código", 10) + Formato.Col("Fecha", 18) + Formato.Col("Cliente", 22) +
                 Formato.Col("Tiempo est.", 13) + "Estado\n";
        foreach (SolicitudServicio s in ordenadas)
        {
            string tiempo = s.TiempoEstimadoHoras > 0 ? s.TiempoEstimadoHoras + " h" : "Pendiente";
            texto += Formato.Col(s.Codigo, 10) + Formato.Col(Formato.Fecha(s.FechaRecepcion), 18) +
                     Formato.Col(s.Cliente.NombreCompleto, 22) + Formato.Col(tiempo, 13) +
                     Formato.NombreEstado(s.Estado) + "\n";
        }
        texto += Linea() + "Total de solicitudes: " + ordenadas.Count + "\n";
        return texto;
    }

    // ===== Reporte 8 (Shell Sort) =====
    public string RepuestosPorExistencia(bool asc)
    {
        List<Repuesto> ordenados = Ordenador.ShellSortPorExistencia(gestorRepuestos.Repuestos, asc);

        string texto = Titulo("Repuestos ordenados por existencia");
        texto += "Algoritmo: Shell Sort | Criterio: Existencia | Orden: " + TextoOrden(asc) + "\n";
        texto += Linea();
        texto += TablaRepuestos(ordenados);
        texto += Linea() + "Total de repuestos: " + ordenados.Count + "\n";
        return texto;
    }

    private string TablaRepuestos(List<Repuesto> lista)
    {
        if (lista.Count == 0) return "  (ninguno)\n";

        string texto = Formato.Col("Código", 10) + Formato.Col("Nombre", 28) + Formato.Col("Existencia", 12) +
                       Formato.Col("Mínimo", 9) + "Estado\n";
        foreach (Repuesto r in lista)
        {
            texto += Formato.Col(r.Codigo, 10) + Formato.Col(r.Nombre, 28) + Formato.Col(r.Existencia.ToString(), 12) +
                     Formato.Col(r.ExistenciaMinima.ToString(), 9) + r.EstadoInventario() + "\n";
        }
        return texto;
    }

    // ===== Reporte 9 =====
    public string RepuestosEnMinimo()
    {
        List<Repuesto> lista = gestorRepuestos.ListarEnMinimoOAgotados();
        string texto = Titulo("Repuestos en existencia mínima o agotados");
        texto += TablaRepuestos(lista);
        texto += Linea() + "Repuestos que necesitan reabastecerse: " + lista.Count + "\n";
        return texto;
    }

    // ===== Reporte 10 =====
    public string IngresosTotales()
    {
        int cantidad = 0;
        decimal manoObra = 0;
        decimal repuestos = 0;

        foreach (SolicitudServicio s in gestorSolicitudes.Solicitudes)
        {
            if (s.Estado == EstadoSolicitud.Entregada)
            {
                cantidad++;
                manoObra += s.CostoManoObra;
                repuestos += s.CostoRepuestos;
            }
        }

        decimal total = manoObra + repuestos;
        string texto = Titulo("Ingresos totales por servicios finalizados");
        texto += "Servicios entregados:      " + cantidad + "\n";
        texto += "Ingresos por mano de obra: " + Formato.Dinero(manoObra) + "\n";
        texto += "Ingresos por repuestos:    " + Formato.Dinero(repuestos) + "\n";
        texto += Linea();
        texto += "INGRESO TOTAL:             " + Formato.Dinero(total) + "\n";
        if (cantidad > 0)
            texto += "Promedio por servicio:     " + Formato.Dinero(total / cantidad) + "\n";
        return texto;
    }

    // ===== Reporte 11 =====
    public string ResultadosBusquedas(string nombre, EstadoSolicitud estado, string marca, string codigo)
    {
        string texto = Titulo("Resultados de las tres formas de búsqueda");

        // 1. Secuencial: clientes por nombre
        texto += "1) Secuencial - clientes cuyo nombre contiene \"" + nombre + "\"\n";
        try
        {
            List<Cliente> clientes = Buscador.SecuencialClientesPorNombre(gestorClientes.Clientes, nombre);
            if (clientes.Count == 0) texto += "   No hay coincidencias.\n";
            foreach (Cliente c in clientes)
            {
                texto += "   " + c.Codigo + " - " + c.NombreCompleto + " (" + c.Telefono + ")\n";
            }
            texto += "   Coincidencias: " + clientes.Count + "\n";
        }
        catch (Exception ex)
        {
            texto += "   " + ex.Message + "\n";
        }

        // 2. Secuencial: solicitudes por estado
        texto += "\n2) Secuencial - solicitudes en estado \"" + Formato.NombreEstado(estado) + "\"\n";
        try
        {
            List<SolicitudServicio> lista = Buscador.SecuencialSolicitudesPorEstado(gestorSolicitudes.Solicitudes, estado);
            if (lista.Count == 0) texto += "   No hay coincidencias.\n";
            foreach (SolicitudServicio s in lista)
            {
                texto += "   " + s.Codigo + " - " + s.Cliente.NombreCompleto + " - " + s.Equipo.Descripcion() + "\n";
            }
            texto += "   Coincidencias: " + lista.Count + "\n";
        }
        catch (Exception ex)
        {
            texto += "   " + ex.Message + "\n";
        }

        // 3. Secuencial: equipos por marca
        texto += "\n3) Secuencial - equipos de la marca \"" + marca + "\"\n";
        try
        {
            List<Equipo> equipos = Buscador.SecuencialEquiposPorMarca(gestorEquipos.Equipos, marca);
            if (equipos.Count == 0) texto += "   No hay coincidencias.\n";
            foreach (Equipo e in equipos)
            {
                texto += "   " + e.CodigoInterno + " - " + e.Descripcion() + " - " + e.Propietario.NombreCompleto + "\n";
            }
            texto += "   Coincidencias: " + equipos.Count + "\n";
        }
        catch (Exception ex)
        {
            texto += "   " + ex.Message + "\n";
        }

        // 4. Binaria: solicitud por codigo
        texto += "\n4) Binaria - solicitud con código \"" + codigo + "\"\n";
        try
        {
            SolicitudServicio s = Buscador.BinariaSolicitudPorCodigo(gestorSolicitudes.Solicitudes, codigo);
            if (s == null)
                texto += "   No se encontró la solicitud.\n";
            else
                texto += "   " + s.ToString() + "\n";
            texto += "   Comparaciones realizadas: " + Buscador.ComparacionesBinaria +
                     " (de " + gestorSolicitudes.Solicitudes.Count + " solicitudes)\n";
        }
        catch (Exception ex)
        {
            texto += "   " + ex.Message + "\n";
        }

        return texto;
    }

    // ===== Reporte 12 =====
    public string MatrizSemanal()
    {
        string texto = Titulo("Matriz semanal de carga de trabajo");
        if (gestorTecnicos.Tecnicos.Count == 0) return texto + "No hay técnicos registrados.\n";
        return texto + matriz.Mostrar(gestorTecnicos.Tecnicos);
    }
}
