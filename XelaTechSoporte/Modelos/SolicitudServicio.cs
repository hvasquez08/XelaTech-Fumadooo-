namespace XelaTechSoporte;

public class SolicitudServicio
{
    // Valores para calcular la mano de obra
    public const decimal TARIFA_POR_HORA = 40;
    public const decimal RECARGO_URGENTE = 0.20m; // 20 %

    // ===== Atributos =====
    private string codigo;
    private Cliente cliente;
    private Equipo equipo;
    private string descripcionProblema;
    private DateTime fechaRecepcion;
    private Prioridad prioridad;
    private EstadoSolicitud estado;
    private Tecnico tecnico;          // queda en null mientras esta en espera
    private string diagnostico;
    private int tiempoEstimadoHoras;
    private decimal costoManoObra;
    private int calificacion;         // 0 = todavia no la califican

    // Lista de repuestos que se usaron en esta solicitud
    private List<RepuestoUsado> repuestosUsados = new List<RepuestoUsado>();

    // Pila con los estados anteriores (LIFO: el ultimo cambio es el primero que se deshace)
    private Stack<EstadoSolicitud> historial = new Stack<EstadoSolicitud>();

    // Constructor normal: la fecha es la del momento en que se crea
    public SolicitudServicio(string codigo, Cliente cliente, Equipo equipo, string descripcion, Prioridad prioridad)
        : this(codigo, cliente, equipo, descripcion, prioridad, DateTime.Now)
    {
    }

    // Sobrecarga del constructor: permite indicar la fecha (la usa el generador de datos de prueba)
    public SolicitudServicio(string codigo, Cliente cliente, Equipo equipo, string descripcion, Prioridad prioridad, DateTime fecha)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new Exception("El código de la solicitud es obligatorio.");
        if (cliente == null)
            throw new Exception("El cliente no existe.");
        if (equipo == null)
            throw new Exception("El equipo no existe.");
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new Exception("La descripción del problema es obligatoria.");

        this.codigo = codigo.Trim();
        this.cliente = cliente;
        this.equipo = equipo;
        this.descripcionProblema = descripcion.Trim();
        this.prioridad = prioridad;
        this.fechaRecepcion = fecha;
        this.estado = EstadoSolicitud.Recibida;
        this.tecnico = null;
        this.diagnostico = "";
        this.tiempoEstimadoHoras = 0;
        this.costoManoObra = 0;
        this.calificacion = 0;
    }

    // ===== Propiedades =====
    public string Codigo { get { return codigo; } }
    public Cliente Cliente { get { return cliente; } }
    public Equipo Equipo { get { return equipo; } }
    public string DescripcionProblema { get { return descripcionProblema; } }
    public DateTime FechaRecepcion { get { return fechaRecepcion; } }
    public Prioridad Prioridad { get { return prioridad; } }
    public EstadoSolicitud Estado { get { return estado; } }
    public Tecnico Tecnico { get { return tecnico; } }
    public string Diagnostico { get { return diagnostico; } }
    public int TiempoEstimadoHoras { get { return tiempoEstimadoHoras; } }
    public decimal CostoManoObra { get { return costoManoObra; } }
    public int Calificacion { get { return calificacion; } }
    public List<RepuestoUsado> RepuestosUsados { get { return repuestosUsados; } }

    // Suma de (precio de venta x cantidad) de cada repuesto usado
    public decimal CostoRepuestos
    {
        get
        {
            decimal suma = 0;
            foreach (RepuestoUsado ru in repuestosUsados)
            {
                suma = suma + ru.Subtotal();
            }
            return suma;
        }
    }

    public decimal Total
    {
        get { return CalcularTotal(); }
    }

    // ===== Metodos =====

    public bool EsActiva()
    {
        return estado != EstadoSolicitud.Entregada && estado != EstadoSolicitud.Cancelada;
    }

    public decimal CalcularTotal()
    {
        return costoManoObra + CostoRepuestos;
    }

    // Tabla de transiciones permitidas (seccion 8.2 del analisis)
    public bool EsTransicionPermitida(EstadoSolicitud nuevo)
    {
        // Desde cualquier estado activo se puede cancelar
        if (nuevo == EstadoSolicitud.Cancelada)
            return EsActiva();

        switch (estado)
        {
            case EstadoSolicitud.Recibida: return nuevo == EstadoSolicitud.EnEspera;
            case EstadoSolicitud.EnEspera: return nuevo == EstadoSolicitud.Asignada;
            case EstadoSolicitud.Asignada: return nuevo == EstadoSolicitud.EnDiagnostico;
            case EstadoSolicitud.EnDiagnostico: return nuevo == EstadoSolicitud.EnReparacion;
            case EstadoSolicitud.EnReparacion: return nuevo == EstadoSolicitud.ListaParaEntregar;
            case EstadoSolicitud.ListaParaEntregar: return nuevo == EstadoSolicitud.Entregada;
            default: return false; // Entregada y Cancelada son estados finales
        }
    }

    // Texto con los estados a los que se puede pasar desde el estado actual
    public string EstadosPermitidosTexto()
    {
        string texto = "";
        for (int i = 0; i <= 7; i++)
        {
            EstadoSolicitud e = (EstadoSolicitud)i;
            if (EsTransicionPermitida(e))
            {
                if (texto != "") texto = texto + ", ";
                texto = texto + Formato.NombreEstado(e);
            }
        }
        if (texto == "") texto = "ninguno (estado final)";
        return texto;
    }

    public void CambiarEstado(EstadoSolicitud nuevo)
    {
        // Validacion 12: no modificar una solicitud entregada o cancelada
        if (!EsActiva())
            throw new Exception("La solicitud " + codigo + " ya está " + Formato.NombreEstado(estado) + " y no se puede modificar.");

        // Validacion 9: respetar el orden del proceso
        if (!EsTransicionPermitida(nuevo))
            throw new Exception("No se puede pasar de '" + Formato.NombreEstado(estado) + "' a '" +
                                Formato.NombreEstado(nuevo) + "'. Permitidos: " + EstadosPermitidosTexto() + ".");

        // Reglas extra segun el estado nuevo
        if (nuevo == EstadoSolicitud.Asignada && tecnico == null)
            throw new Exception("No se puede marcar como asignada sin un técnico (se asigna en Atención de colas).");

        if (nuevo == EstadoSolicitud.EnDiagnostico && string.IsNullOrWhiteSpace(diagnostico))
            throw new Exception("Primero debe registrar el diagnóstico.");

        // Validacion adicional 19: no se entrega sin diagnostico ni total
        if (nuevo == EstadoSolicitud.ListaParaEntregar)
        {
            if (string.IsNullOrWhiteSpace(diagnostico) || CalcularTotal() <= 0)
                throw new Exception("La solicitud no tiene diagnóstico ni total calculado.");
        }

        // Se guarda el estado anterior en la pila y se cambia
        historial.Push(estado);
        estado = nuevo;
    }

    public void AsignarTecnico(Tecnico t)
    {
        if (t == null)
            throw new Exception("No hay técnico para asignar.");
        if (estado != EstadoSolicitud.EnEspera)
            throw new Exception("Solo se puede asignar técnico a una solicitud en espera.");
        if (!t.PuedeRecibir(equipo.EspecialidadRequerida()))
            throw new Exception("El técnico " + t.NombreCompleto + " no puede recibir esta solicitud (especialidad, disponibilidad o límite).");

        tecnico = t;
        t.AsignarTrabajo();
        CambiarEstado(EstadoSolicitud.Asignada);
    }

    // Registra (o corrige) el diagnostico y calcula la mano de obra
    public void RegistrarDiagnostico(string texto, int horas)
    {
        if (estado != EstadoSolicitud.Asignada && estado != EstadoSolicitud.EnDiagnostico)
            throw new Exception("El diagnóstico solo se registra cuando la solicitud está Asignada o En diagnóstico.");
        if (string.IsNullOrWhiteSpace(texto))
            throw new Exception("El diagnóstico no puede estar vacío.");
        // Validacion adicional 17
        if (horas <= 0)
            throw new Exception("El tiempo estimado debe ser mayor que cero.");

        diagnostico = texto.Trim();
        tiempoEstimadoHoras = horas;

        // Mano de obra = costo de revision (segun el tipo de equipo) + horas x tarifa
        decimal mano = equipo.CalcularCostoRevision() + horas * TARIFA_POR_HORA;
        if (prioridad == Prioridad.Urgente)
        {
            mano = mano + mano * RECARGO_URGENTE;
        }
        costoManoObra = mano;

        if (estado == EstadoSolicitud.Asignada)
        {
            CambiarEstado(EstadoSolicitud.EnDiagnostico);
        }
    }

    public void AgregarRepuesto(Repuesto r, int cant)
    {
        if (r == null)
            throw new Exception("El repuesto no existe.");
        if (estado != EstadoSolicitud.EnDiagnostico && estado != EstadoSolicitud.EnReparacion)
            throw new Exception("Solo se agregan repuestos cuando la solicitud está En diagnóstico o En reparación.");
        // Validacion adicional 16: la cantidad no puede ser cero ni negativa
        if (cant <= 0)
            throw new Exception("La cantidad debe ser mayor que cero.");
        // Validacion 8: no usar mas de lo que hay
        if (!r.HayExistencia(cant))
            throw new Exception("No hay existencia suficiente de " + r.Nombre + ". Disponible: " + r.Existencia);

        r.Descontar(cant);

        // Si el repuesto ya estaba en la lista solo se suma la cantidad
        foreach (RepuestoUsado ru in repuestosUsados)
        {
            if (ru.Repuesto == r)
            {
                ru.AgregarCantidad(cant);
                return;
            }
        }
        repuestosUsados.Add(new RepuestoUsado(r, cant));
    }

    // Deshace el ultimo cambio de estado usando la pila
    public void DeshacerUltimoCambio()
    {
        if (!EsActiva())
            throw new Exception("No se puede deshacer: la solicitud ya está " + Formato.NombreEstado(estado) + ".");

        // Validacion 10: no desapilar con la pila vacia
        if (historial.Count == 0)
            throw new Exception("El historial está vacío, no hay cambios para deshacer.");

        // La asignacion de tecnico y el paso a "En espera" los hace la cola,
        // por eso no se deshacen (la solicitud ya salio de la cola o ya tiene tecnico).
        EstadoSolicitud anterior = historial.Peek();
        if (anterior == EstadoSolicitud.Recibida || anterior == EstadoSolicitud.EnEspera)
            throw new Exception("No se puede deshacer la asignación hecha por la cola. Si es necesario, cancele la solicitud.");

        estado = historial.Pop();
    }

    // Devuelve el historial como texto (del cambio mas reciente al mas antiguo)
    public string VerHistorial()
    {
        string texto = "Historial de " + codigo + "\n";
        texto = texto + "  Estado actual: " + Formato.NombreEstado(estado) + "\n";
        if (historial.Count == 0)
        {
            texto = texto + "  (sin estados anteriores)\n";
            return texto;
        }

        // Recorrer la pila con foreach no la modifica; empieza por el tope
        int n = 1;
        foreach (EstadoSolicitud e in historial)
        {
            texto = texto + "  " + n + ". " + Formato.NombreEstado(e) + "\n";
            n++;
        }
        return texto;
    }

    // Estados anteriores en arreglo (para el modo grafico). La posicion 0 es el tope.
    public EstadoSolicitud[] HistorialComoArreglo()
    {
        return historial.ToArray();
    }

    // Cancela: libera al tecnico y regresa los repuestos al inventario
    public int Cancelar()
    {
        if (!EsActiva())
            throw new Exception("La solicitud ya está " + Formato.NombreEstado(estado) + " y no se puede cancelar.");

        if (tecnico != null)
        {
            tecnico.LiberarTrabajo();
        }

        int devueltos = 0;
        foreach (RepuestoUsado ru in repuestosUsados)
        {
            ru.Repuesto.Devolver(ru.Cantidad);
            devueltos = devueltos + ru.Cantidad;
        }

        CambiarEstado(EstadoSolicitud.Cancelada);
        return devueltos;
    }

    // Entrega del equipo. Calificacion de 1 a 5 (0 si el cliente no quiere calificar)
    public void Entregar(int calificacionCliente)
    {
        if (estado != EstadoSolicitud.ListaParaEntregar)
            throw new Exception("Solo se puede entregar una solicitud Lista para entregar. Estado actual: " + Formato.NombreEstado(estado) + ".");
        if (calificacionCliente < 0 || calificacionCliente > 5)
            throw new Exception("La calificación debe estar entre 1 y 5 (o 0 para no calificar).");

        CambiarEstado(EstadoSolicitud.Entregada);
        tecnico.FinalizarTrabajo();

        if (calificacionCliente > 0)
        {
            Calificar(calificacionCliente);
        }
    }

    // Validacion adicional 21: solo se califica una solicitud entregada y una sola vez
    public void Calificar(int c)
    {
        if (estado != EstadoSolicitud.Entregada)
            throw new Exception("Solo se pueden calificar solicitudes entregadas.");
        if (calificacion != 0)
            throw new Exception("Esta solicitud ya fue calificada con " + calificacion + ".");

        tecnico.RegistrarCalificacion(c); // aqui se valida que este entre 1 y 5
        calificacion = c;
    }

    public string Comprobante()
    {
        string texto = "";
        texto += "------------------------------------------------\n";
        texto += "  COMPROBANTE DE SERVICIO - XELATECH SOPORTE\n";
        texto += "------------------------------------------------\n";
        texto += "  Solicitud: " + codigo + "   (" + prioridad + ")\n";
        texto += "  Recibida:  " + Formato.Fecha(fechaRecepcion) + "\n";
        texto += "  Cliente:   " + cliente.NombreCompleto + "\n";
        texto += "  Equipo:    " + equipo.Descripcion() + " (serie " + equipo.NumeroSerie + ")\n";
        texto += "  Técnico:   " + (tecnico == null ? "Sin asignar" : tecnico.NombreCompleto) + "\n";
        texto += "  Diagnóstico: " + (diagnostico == "" ? "Pendiente" : diagnostico) + "\n";
        texto += "  Tiempo estimado: " + tiempoEstimadoHoras + " h\n";
        texto += "------------------------------------------------\n";
        texto += "  Mano de obra:            " + Formato.Dinero(costoManoObra) + "\n";
        foreach (RepuestoUsado ru in repuestosUsados)
        {
            texto += "  " + Formato.Col(ru.Repuesto.Nombre + " x" + ru.Cantidad, 25) + Formato.Dinero(ru.Subtotal()) + "\n";
        }
        texto += "  Repuestos:               " + Formato.Dinero(CostoRepuestos) + "\n";
        texto += "  TOTAL:                   " + Formato.Dinero(CalcularTotal()) + "\n";
        texto += "------------------------------------------------\n";
        texto += "  Estado: " + Formato.NombreEstado(estado) + "\n";
        return texto;
    }

    public override string ToString()
    {
        string nombreTecnico = tecnico == null ? "Sin asignar" : tecnico.NombreCompleto;
        return codigo + " | " + cliente.NombreCompleto + " | " + equipo.Descripcion() + " | " + prioridad +
               " | " + Formato.NombreEstado(estado) + " | Técnico: " + nombreTecnico +
               " | Total: " + Formato.Dinero(CalcularTotal());
    }
}
