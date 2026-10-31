namespace XelaTechSoporte;

// Administra las solicitudes y las dos colas de atencion
public class GestorSolicitudes
{
    // Lista con TODAS las solicitudes (en orden de registro = orden por codigo)
    private List<SolicitudServicio> solicitudes = new List<SolicitudServicio>();

    // Colas FIFO: el primero que llega es el primero que se atiende
    private Queue<SolicitudServicio> colaNormal = new Queue<SolicitudServicio>();
    private Queue<SolicitudServicio> colaUrgente = new Queue<SolicitudServicio>();

    public List<SolicitudServicio> Solicitudes { get { return solicitudes; } }
    public Queue<SolicitudServicio> ColaNormal { get { return colaNormal; } }
    public Queue<SolicitudServicio> ColaUrgente { get { return colaUrgente; } }

    // SOL-0001, SOL-0002, ... (como el numero siempre sube, la lista queda ordenada por codigo)
    public string GenerarCodigo()
    {
        int numero = solicitudes.Count + 1;
        string codigo = "SOL-" + numero.ToString("D4");
        while (BuscarPorCodigo(codigo) != null)
        {
            numero++;
            codigo = "SOL-" + numero.ToString("D4");
        }
        return codigo;
    }

    public SolicitudServicio CrearSolicitud(Cliente cli, Equipo eq, string desc, Prioridad prio)
    {
        return CrearSolicitud(cli, eq, desc, prio, DateTime.Now);
    }

    // Sobrecarga con fecha (la usa el generador de datos)
    public SolicitudServicio CrearSolicitud(Cliente cli, Equipo eq, string desc, Prioridad prio, DateTime fecha)
    {
        // Validacion 4: cliente y equipo deben existir
        if (cli == null)
            throw new Exception("El cliente no existe.");
        if (eq == null)
            throw new Exception("El equipo no existe.");
        if (eq.Propietario != cli)
            throw new Exception("El equipo " + eq.CodigoInterno + " no pertenece al cliente " + cli.Codigo + ".");
        // Validacion 5: una sola solicitud activa por equipo
        if (ExisteActivaParaEquipo(eq))
            throw new Exception("El equipo " + eq.CodigoInterno + " ya tiene una solicitud activa.");

        SolicitudServicio s = new SolicitudServicio(GenerarCodigo(), cli, eq, desc, prio, fecha);
        solicitudes.Add(s);
        cli.IncrementarSolicitudes();

        // Pasa a la cola correspondiente y queda "En espera"
        Encolar(s);
        return s;
    }

    public void Encolar(SolicitudServicio s)
    {
        s.CambiarEstado(EstadoSolicitud.EnEspera); // Recibida -> En espera (se guarda Recibida en la pila)

        if (s.Prioridad == Prioridad.Urgente)
            colaUrgente.Enqueue(s);
        else
            colaNormal.Enqueue(s);
    }

    // Atiende la siguiente solicitud: primero la cola urgente y si esta vacia la normal.
    // "dia" es la columna de la matriz (0 = lunes ... 6 = domingo).
    public SolicitudServicio AtenderSiguiente(GestorTecnicos gestorTecnicos, MatrizCarga matriz, int dia)
    {
        // Validacion adicional 20: no atender si ambas colas estan vacias
        if (colaUrgente.Count == 0 && colaNormal.Count == 0)
            throw new Exception("No hay solicitudes en las colas para atender.");

        // Se mira (Peek) sin sacar, por si no hay tecnico disponible
        Queue<SolicitudServicio> cola;
        if (colaUrgente.Count > 0)
            cola = colaUrgente;
        else
            cola = colaNormal;

        SolicitudServicio s = cola.Peek();
        Especialidad requerida = s.Equipo.EspecialidadRequerida();
        Tecnico t = gestorTecnicos.ObtenerDisponible(requerida);

        if (t == null)
            throw new Exception("No hay técnico disponible de " + requerida + " (o General) con carga menor a " +
                                Tecnico.LIMITE_ACTIVOS + ". La solicitud " + s.Codigo + " sigue en espera.");

        // Ahora si se saca de la cola y se asigna
        cola.Dequeue();
        s.AsignarTecnico(t);
        matriz.RegistrarAsignacion(gestorTecnicos.IndiceDe(t), dia);
        return s;
    }

    public bool ExisteActivaParaEquipo(Equipo eq)
    {
        foreach (SolicitudServicio s in solicitudes)
        {
            if (s.Equipo == eq && s.EsActiva()) return true;
        }
        return false;
    }

    public SolicitudServicio BuscarPorCodigo(string cod)
    {
        if (string.IsNullOrWhiteSpace(cod)) return null;
        cod = cod.Trim().ToUpper();

        foreach (SolicitudServicio s in solicitudes)
        {
            if (s.Codigo == cod) return s;
        }
        return null;
    }

    // Usa la busqueda secuencial del Buscador
    public List<SolicitudServicio> BuscarPorEstado(EstadoSolicitud est)
    {
        return Buscador.SecuencialSolicitudesPorEstado(solicitudes, est);
    }

    private SolicitudServicio ObtenerOError(string cod)
    {
        SolicitudServicio s = BuscarPorCodigo(cod);
        if (s == null)
            throw new Exception("No existe la solicitud " + cod + ".");
        return s;
    }

    // Cambio de estado manual (opcion "Actualizar estado")
    public void CambiarEstado(string cod, EstadoSolicitud nuevo)
    {
        SolicitudServicio s = ObtenerOError(cod);

        // Estos estados tienen su propia opcion porque hacen algo mas que cambiar el estado
        if (nuevo == EstadoSolicitud.Asignada)
            throw new Exception("La asignación de técnico se hace desde Atención de colas.");
        if (nuevo == EstadoSolicitud.Entregada)
            throw new Exception("Para entregar use la opción 'Entregar' (calcula el total y actualiza al técnico).");
        if (nuevo == EstadoSolicitud.Cancelada)
            throw new Exception("Para cancelar use la opción 'Cancelar' (libera al técnico y devuelve repuestos).");
        if (nuevo == EstadoSolicitud.EnDiagnostico && s.Estado == EstadoSolicitud.Asignada)
            throw new Exception("Para pasar a En diagnóstico use 'Registrar diagnóstico y repuestos'.");

        s.CambiarEstado(nuevo);
    }

    public void DeshacerUltimoCambio(string cod)
    {
        ObtenerOError(cod).DeshacerUltimoCambio();
    }

    public void Entregar(string cod, int calificacion)
    {
        ObtenerOError(cod).Entregar(calificacion);
    }

    public int Cancelar(string cod)
    {
        SolicitudServicio s = ObtenerOError(cod);
        bool estabaEnCola = s.Estado == EstadoSolicitud.Recibida || s.Estado == EstadoSolicitud.EnEspera;

        int devueltos = s.Cancelar();

        // Si todavia estaba en una cola hay que sacarla
        if (estabaEnCola)
        {
            QuitarDeCola(colaUrgente, s);
            QuitarDeCola(colaNormal, s);
        }
        return devueltos;
    }

    // Una Queue no permite quitar un elemento del medio, por eso se arma de nuevo
    // pasando todos los elementos menos el cancelado (se respeta el orden FIFO).
    private void QuitarDeCola(Queue<SolicitudServicio> cola, SolicitudServicio quitar)
    {
        int cantidad = cola.Count;
        for (int i = 0; i < cantidad; i++)
        {
            SolicitudServicio s = cola.Dequeue();
            if (s != quitar)
            {
                cola.Enqueue(s);
            }
        }
    }

    public List<SolicitudServicio> SolicitudesDeCliente(Cliente c)
    {
        List<SolicitudServicio> lista = new List<SolicitudServicio>();
        foreach (SolicitudServicio s in solicitudes)
        {
            if (s.Cliente == c) lista.Add(s);
        }
        return lista;
    }
}
