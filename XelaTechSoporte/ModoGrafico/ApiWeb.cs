using System.Globalization;

namespace XelaTechSoporte;

// Recibe lo que pide la pagina web y llama a los MISMOS gestores, algoritmos y reportes
// que usa el menu de consola. Aqui no hay logica nueva: solo se leen los datos de la
// pagina, se llama al metodo que corresponde y se arma la respuesta.
public class ApiWeb
{
    private GestorClientes gestorClientes;
    private GestorTecnicos gestorTecnicos;
    private GestorEquipos gestorEquipos;
    private GestorRepuestos gestorRepuestos;
    private GestorSolicitudes gestorSolicitudes;
    private MatrizCarga matriz;
    private Reportes reportes;
    private GeneradorDatos generador;

    public ApiWeb(GestorClientes gc, GestorTecnicos gt, GestorEquipos ge, GestorRepuestos gr,
                  GestorSolicitudes gs, MatrizCarga m, Reportes rep, GeneradorDatos gen)
    {
        gestorClientes = gc;
        gestorTecnicos = gt;
        gestorEquipos = ge;
        gestorRepuestos = gr;
        gestorSolicitudes = gs;
        matriz = m;
        reportes = rep;
        generador = gen;
    }

    // Decide que hacer segun la ruta. Si algo falla se devuelve el mensaje de la excepcion,
    // asi la pagina muestra el mismo error que mostraria la consola.
    public object Procesar(string ruta, Dictionary<string, string> datos)
    {
        try
        {
            switch (ruta)
            {
                // ----- Clientes -----
                case "/api/clientes/listar": return ListarClientes();
                case "/api/clientes/registrar": return RegistrarCliente(datos);
                case "/api/clientes/consultar": return ConsultarCliente(datos);
                case "/api/clientes/actualizar": return ActualizarCliente(datos);

                // ----- Tecnicos -----
                case "/api/tecnicos/listar": return ListarTecnicos();
                case "/api/tecnicos/registrar": return RegistrarTecnico(datos);
                case "/api/tecnicos/consultar": return ConsultarTecnico(datos);
                case "/api/tecnicos/disponibilidad": return CambiarDisponibilidad(datos);
                case "/api/tecnicos/calificar": return Calificar(datos);

                // ----- Equipos -----
                case "/api/equipos/listar": return ListarEquipos(gestorEquipos.Equipos);
                case "/api/equipos/registrar": return RegistrarEquipo(datos);
                case "/api/equipos/delcliente": return EquiposDelCliente(datos);

                // ----- Solicitudes -----
                case "/api/solicitudes/listar": return ListarSolicitudes(gestorSolicitudes.Solicitudes);
                case "/api/solicitudes/crear": return CrearSolicitud(datos);
                case "/api/solicitudes/consultar": return ConsultarSolicitud(datos);
                case "/api/solicitudes/diagnostico": return RegistrarDiagnostico(datos);
                case "/api/solicitudes/repuesto": return AgregarRepuesto(datos);
                case "/api/solicitudes/estado": return CambiarEstado(datos);
                case "/api/solicitudes/deshacer": return Deshacer(datos);
                case "/api/solicitudes/entregar": return Entregar(datos);
                case "/api/solicitudes/cancelar": return Cancelar(datos);

                // ----- Repuestos -----
                case "/api/repuestos/listar": return ListarRepuestos(gestorRepuestos.Repuestos);
                case "/api/repuestos/registrar": return RegistrarRepuesto(datos);
                case "/api/repuestos/reabastecer": return Reabastecer(datos);
                case "/api/repuestos/minimos": return ListarRepuestos(gestorRepuestos.ListarEnMinimoOAgotados());

                // ----- Atencion de colas -----
                case "/api/colas/ver": return VerColas();
                case "/api/colas/atender": return AtenderSiguiente();
                case "/api/colas/matriz": return VerMatriz();

                // ----- Ordenamientos, busquedas y reportes -----
                case "/api/ordenar": return Ordenar(datos);
                case "/api/buscar": return Buscar(datos);
                case "/api/reporte": return Reporte(datos);

                // ----- Barra superior -----
                case "/api/datosprueba": return DatosPrueba(datos);
                case "/api/salir": return Ok("Modo gráfico cerrado. Puede volver a la consola.");

                // ----- Portal del cliente -----
                case "/api/portal/estado": return EstadoParaCliente(datos);

                default: return Error("La ruta " + ruta + " no existe.");
            }
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    // ======================================================
    //  RESPUESTAS
    // ======================================================

    private object Ok(string mensaje)
    {
        return new { ok = true, mensaje = mensaje };
    }

    private object Error(string mensaje)
    {
        return new { ok = false, mensaje = mensaje };
    }

    // Una tabla: lista de nombres de columnas y lista de filas (cada fila es un arreglo de textos)
    private object Tabla(string[] columnas, List<string[]> filas)
    {
        return new { ok = true, columnas = columnas, filas = filas };
    }

    // ======================================================
    //  LECTURA DE DATOS DE LA PAGINA
    // ======================================================

    private string Dato(Dictionary<string, string> datos, string clave)
    {
        if (datos.ContainsKey(clave) && datos[clave] != null)
            return datos[clave].Trim();
        return "";
    }

    private int LeerEntero(Dictionary<string, string> datos, string clave, string campo)
    {
        int numero;
        if (!int.TryParse(Dato(datos, clave), out numero))
            throw new Exception("El campo '" + campo + "' debe ser un número entero.");
        return numero;
    }

    private decimal LeerDecimal(Dictionary<string, string> datos, string clave, string campo)
    {
        decimal numero;
        string texto = Dato(datos, clave).Replace(',', '.');
        if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out numero))
            throw new Exception("El campo '" + campo + "' debe ser un número.");
        return numero;
    }

    private double LeerDouble(Dictionary<string, string> datos, string clave, string campo)
    {
        double numero;
        string texto = Dato(datos, clave).Replace(',', '.');
        if (!double.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out numero))
            throw new Exception("El campo '" + campo + "' debe ser un número.");
        return numero;
    }

    // ======================================================
    //  CLIENTES
    // ======================================================

    private object ListarClientes()
    {
        string[] columnas = { "Código", "Nombre", "Teléfono", "Correo", "Tipo", "Solicitudes" };
        List<string[]> filas = new List<string[]>();
        foreach (Cliente c in gestorClientes.Clientes)
        {
            filas.Add(FilaCliente(c));
        }
        return Tabla(columnas, filas);
    }

    private string[] FilaCliente(Cliente c)
    {
        return new string[] { c.Codigo, c.NombreCompleto, c.Telefono, c.Correo,
                              Formato.NombreTipoCliente(c.Tipo), c.CantidadSolicitudes.ToString() };
    }

    private object RegistrarCliente(Dictionary<string, string> datos)
    {
        int tipo = LeerEntero(datos, "tipo", "Tipo de cliente");
        if (tipo < 0 || tipo > 3)
            throw new Exception("Seleccione un tipo de cliente válido.");

        // Se usan las mismas validaciones del constructor de Cliente
        Cliente c = new Cliente(gestorClientes.GenerarCodigo(), Dato(datos, "nombre"), Dato(datos, "telefono"),
                                Dato(datos, "correo"), (TipoCliente)tipo);
        gestorClientes.Registrar(c);
        return new { ok = true, mensaje = "Cliente registrado con el código " + c.Codigo + ".", codigo = c.Codigo };
    }

    private object ConsultarCliente(Dictionary<string, string> datos)
    {
        Cliente c = gestorClientes.BuscarPorCodigo(Dato(datos, "codigo"));
        if (c == null)
            throw new Exception("No existe el cliente " + Dato(datos, "codigo") + ".");

        return new
        {
            ok = true,
            cliente = new
            {
                codigo = c.Codigo,
                nombre = c.NombreCompleto,
                telefono = c.Telefono,
                correo = c.Correo,
                tipo = Formato.NombreTipoCliente(c.Tipo),
                solicitudes = c.CantidadSolicitudes
            },
            equipos = ListarEquipos(gestorEquipos.EquiposDeCliente(c)),
            listaSolicitudes = ListarSolicitudes(gestorSolicitudes.SolicitudesDeCliente(c))
        };
    }

    private object ActualizarCliente(Dictionary<string, string> datos)
    {
        Cliente c = gestorClientes.BuscarPorCodigo(Dato(datos, "codigo"));
        if (c == null)
            throw new Exception("No existe el cliente " + Dato(datos, "codigo") + ".");

        // Si un campo viene vacio se conserva el dato actual
        string tel = Dato(datos, "telefono");
        string correo = Dato(datos, "correo");
        if (tel == "") tel = c.Telefono;
        if (correo == "") correo = c.Correo;

        gestorClientes.Actualizar(c.Codigo, tel, correo);
        return Ok("Datos de " + c.NombreCompleto + " actualizados.");
    }

    // ======================================================
    //  TECNICOS
    // ======================================================

    private object ListarTecnicos()
    {
        string[] columnas = { "Código", "Nombre", "Especialidad", "Disponible", "Activos", "Finalizados", "Promedio" };
        List<string[]> filas = new List<string[]>();
        foreach (Tecnico t in gestorTecnicos.Tecnicos)
        {
            filas.Add(FilaTecnico(t));
        }
        return Tabla(columnas, filas);
    }

    private string[] FilaTecnico(Tecnico t)
    {
        return new string[] { t.Codigo, t.NombreCompleto, t.Especialidad.ToString(), t.Disponible ? "Sí" : "No",
                              t.TrabajosActivos + "/" + Tecnico.LIMITE_ACTIVOS, t.ServiciosFinalizados.ToString(),
                              t.CalificacionPromedio.ToString("0.00") };
    }

    private object RegistrarTecnico(Dictionary<string, string> datos)
    {
        int esp = LeerEntero(datos, "especialidad", "Especialidad");
        if (esp < 0 || esp > 2)
            throw new Exception("Seleccione una especialidad válida.");
        bool disponible = Dato(datos, "disponible") == "true";

        Tecnico t = new Tecnico(gestorTecnicos.GenerarCodigo(), Dato(datos, "nombre"), (Especialidad)esp, disponible);
        gestorTecnicos.Registrar(t);
        return Ok("Técnico registrado con el código " + t.Codigo + ".");
    }

    private object ConsultarTecnico(Dictionary<string, string> datos)
    {
        Tecnico t = gestorTecnicos.BuscarPorCodigo(Dato(datos, "codigo"));
        if (t == null)
            throw new Exception("No existe el técnico " + Dato(datos, "codigo") + ".");

        return new
        {
            ok = true,
            tecnico = new
            {
                codigo = t.Codigo,
                nombre = t.NombreCompleto,
                especialidad = t.Especialidad.ToString(),
                disponible = t.Disponible,
                activos = t.TrabajosActivos,
                limite = Tecnico.LIMITE_ACTIVOS,
                finalizados = t.ServiciosFinalizados,
                promedio = t.CalificacionPromedio.ToString("0.00"),
                calificaciones = t.Calificaciones // el arreglo de 5 posiciones
            }
        };
    }

    private object CambiarDisponibilidad(Dictionary<string, string> datos)
    {
        Tecnico t = gestorTecnicos.BuscarPorCodigo(Dato(datos, "codigo"));
        if (t == null)
            throw new Exception("No existe el técnico " + Dato(datos, "codigo") + ".");

        t.Disponible = !t.Disponible;
        return Ok(t.NombreCompleto + " ahora está " + (t.Disponible ? "disponible." : "no disponible."));
    }

    private object Calificar(Dictionary<string, string> datos)
    {
        SolicitudServicio s = gestorSolicitudes.BuscarPorCodigo(Dato(datos, "solicitud"));
        if (s == null)
            throw new Exception("No existe la solicitud " + Dato(datos, "solicitud") + ".");

        s.Calificar(LeerEntero(datos, "calificacion", "Calificación"));
        return Ok("Calificación registrada para " + s.Tecnico.NombreCompleto + ". Nuevo promedio: " +
                  s.Tecnico.CalificacionPromedio.ToString("0.00"));
    }

    // ======================================================
    //  EQUIPOS
    // ======================================================

    private object ListarEquipos(List<Equipo> lista)
    {
        string[] columnas = { "Código", "Tipo", "Marca", "Modelo", "Serie", "Propietario", "Detalles", "Revisión" };
        List<string[]> filas = new List<string[]>();
        foreach (Equipo e in lista)
        {
            filas.Add(new string[] { e.CodigoInterno, e.TipoEquipo(), e.Marca, e.Modelo, e.NumeroSerie,
                                     e.Propietario.NombreCompleto, e.DatosPropios(),
                                     Formato.Dinero(e.CalcularCostoRevision()) });
        }
        return Tabla(columnas, filas);
    }

    private object EquiposDelCliente(Dictionary<string, string> datos)
    {
        Cliente c = gestorClientes.BuscarPorCodigo(Dato(datos, "cliente"));
        if (c == null)
            throw new Exception("No existe el cliente " + Dato(datos, "cliente") + ".");
        return ListarEquipos(gestorEquipos.EquiposDeCliente(c));
    }

    private object RegistrarEquipo(Dictionary<string, string> datos)
    {
        Cliente dueno = gestorClientes.BuscarPorCodigo(Dato(datos, "propietario"));
        if (dueno == null)
            throw new Exception("No existe el cliente " + Dato(datos, "propietario") + ".");

        string serie = Dato(datos, "serie");
        if (gestorEquipos.ExisteSerie(serie))
            throw new Exception("Ya existe un equipo con el número de serie " + serie.ToUpper() + ".");

        string codigo = gestorEquipos.GenerarCodigo();
        string tipo = Dato(datos, "tipo");
        Equipo equipo;

        if (tipo == "laptop")
        {
            equipo = new Laptop(codigo, serie, Dato(datos, "marca"), Dato(datos, "modelo"), dueno,
                                Dato(datos, "estadoFisico"), Dato(datos, "accesorios"),
                                LeerDouble(datos, "pulgadas", "Pulgadas"), Dato(datos, "bateria"));
        }
        else if (tipo == "escritorio")
        {
            equipo = new Escritorio(codigo, serie, Dato(datos, "marca"), Dato(datos, "modelo"), dueno,
                                    Dato(datos, "estadoFisico"), Dato(datos, "accesorios"),
                                    Dato(datos, "gabinete"), Dato(datos, "fuenteExterna") == "true");
        }
        else if (tipo == "impresora")
        {
            equipo = new Impresora(codigo, serie, Dato(datos, "marca"), Dato(datos, "modelo"), dueno,
                                   Dato(datos, "estadoFisico"), Dato(datos, "accesorios"),
                                   Dato(datos, "tecnologia"), LeerEntero(datos, "tinta", "Nivel de tinta"));
        }
        else
        {
            throw new Exception("Seleccione un tipo de equipo.");
        }

        gestorEquipos.Registrar(equipo);
        return Ok("Equipo (" + equipo.TipoEquipo() + ") registrado con el código " + equipo.CodigoInterno +
                  ". Costo de revisión: " + Formato.Dinero(equipo.CalcularCostoRevision()) + ".");
    }

    // ======================================================
    //  SOLICITUDES
    // ======================================================

    private object ListarSolicitudes(List<SolicitudServicio> lista)
    {
        string[] columnas = { "Código", "Fecha", "Cliente", "Equipo", "Prioridad", "Estado", "Técnico", "Total" };
        List<string[]> filas = new List<string[]>();
        foreach (SolicitudServicio s in lista)
        {
            filas.Add(FilaSolicitud(s));
        }
        return Tabla(columnas, filas);
    }

    private string[] FilaSolicitud(SolicitudServicio s)
    {
        string tecnico = s.Tecnico == null ? "-" : s.Tecnico.NombreCompleto;
        return new string[] { s.Codigo, Formato.Fecha(s.FechaRecepcion), s.Cliente.NombreCompleto, s.Equipo.Descripcion(),
                              s.Prioridad.ToString(), Formato.NombreEstado(s.Estado), tecnico, Formato.Dinero(s.Total) };
    }

    private SolicitudServicio ObtenerSolicitud(Dictionary<string, string> datos)
    {
        SolicitudServicio s = gestorSolicitudes.BuscarPorCodigo(Dato(datos, "codigo"));
        if (s == null)
            throw new Exception("No existe la solicitud " + Dato(datos, "codigo") + ".");
        return s;
    }

    private object CrearSolicitud(Dictionary<string, string> datos)
    {
        Cliente c = gestorClientes.BuscarPorCodigo(Dato(datos, "cliente"));
        Equipo e = gestorEquipos.BuscarPorCodigo(Dato(datos, "equipo"));
        if (c == null)
            throw new Exception("Seleccione un cliente que exista.");
        if (e == null)
            throw new Exception("Seleccione un equipo que exista.");

        Prioridad prio = Dato(datos, "prioridad") == "Urgente" ? Prioridad.Urgente : Prioridad.Normal;
        SolicitudServicio s = gestorSolicitudes.CrearSolicitud(c, e, Dato(datos, "descripcion"), prio);
        return Ok("Solicitud " + s.Codigo + " creada y enviada a la cola " + prio.ToString().ToLower() + ".");
    }

    private object ConsultarSolicitud(Dictionary<string, string> datos)
    {
        SolicitudServicio s = ObtenerSolicitud(datos);

        // Repuestos usados como tabla
        List<string[]> filasRep = new List<string[]>();
        foreach (RepuestoUsado ru in s.RepuestosUsados)
        {
            filasRep.Add(new string[] { ru.Repuesto.Codigo, ru.Repuesto.Nombre, ru.Cantidad.ToString(),
                                        Formato.Dinero(ru.Repuesto.PrecioVenta), Formato.Dinero(ru.Subtotal()) });
        }

        // Historial (pila) como lista de textos, del mas reciente al mas antiguo
        List<string> historial = new List<string>();
        foreach (EstadoSolicitud e in s.HistorialComoArreglo())
        {
            historial.Add(Formato.NombreEstado(e));
        }

        return new
        {
            ok = true,
            solicitud = new
            {
                codigo = s.Codigo,
                cliente = s.Cliente.NombreCompleto + " (" + s.Cliente.Codigo + ")",
                equipo = s.Equipo.Descripcion() + " - serie " + s.Equipo.NumeroSerie,
                problema = s.DescripcionProblema,
                fecha = Formato.Fecha(s.FechaRecepcion),
                prioridad = s.Prioridad.ToString(),
                estado = Formato.NombreEstado(s.Estado),
                tecnico = s.Tecnico == null ? "Sin asignar" : s.Tecnico.NombreCompleto,
                diagnostico = s.Diagnostico == "" ? "Pendiente" : s.Diagnostico,
                sugerencia = s.Equipo.Diagnosticar(),
                horas = s.TiempoEstimadoHoras,
                manoObra = Formato.Dinero(s.CostoManoObra),
                costoRepuestos = Formato.Dinero(s.CostoRepuestos),
                total = Formato.Dinero(s.Total),
                permitidos = s.EstadosPermitidosTexto(),
                calificacion = s.Calificacion
            },
            repuestos = Tabla(new string[] { "Código", "Repuesto", "Cantidad", "Precio", "Subtotal" }, filasRep),
            historial = historial
        };
    }

    private object RegistrarDiagnostico(Dictionary<string, string> datos)
    {
        SolicitudServicio s = ObtenerSolicitud(datos);
        s.RegistrarDiagnostico(Dato(datos, "texto"), LeerEntero(datos, "horas", "Tiempo estimado"));
        return Ok("Diagnóstico registrado. Mano de obra: " + Formato.Dinero(s.CostoManoObra) +
                  ". Estado: " + Formato.NombreEstado(s.Estado) + ".");
    }

    private object AgregarRepuesto(Dictionary<string, string> datos)
    {
        SolicitudServicio s = ObtenerSolicitud(datos);
        Repuesto r = gestorRepuestos.BuscarPorCodigo(Dato(datos, "repuesto"));
        if (r == null)
            throw new Exception("Seleccione un repuesto que exista.");

        int cant = LeerEntero(datos, "cantidad", "Cantidad");
        s.AgregarRepuesto(r, cant);

        string mensaje = "Se agregó " + r.Nombre + " x" + cant + ". Total actual: " + Formato.Dinero(s.Total) + ".";
        // Alerta de inventario
        if (r.EstaAgotado())
            mensaje += " ALERTA: el repuesto se agotó.";
        else if (r.EstaEnMinimo())
            mensaje += " ALERTA: el repuesto llegó a su existencia mínima (" + r.Existencia + ").";
        return Ok(mensaje);
    }

    private object CambiarEstado(Dictionary<string, string> datos)
    {
        SolicitudServicio s = ObtenerSolicitud(datos);
        int estado = LeerEntero(datos, "estado", "Estado");
        if (estado < 0 || estado > 7)
            throw new Exception("Seleccione un estado válido.");

        gestorSolicitudes.CambiarEstado(s.Codigo, (EstadoSolicitud)estado);
        return Ok("Estado de " + s.Codigo + " actualizado a: " + Formato.NombreEstado(s.Estado) + ".");
    }

    private object Deshacer(Dictionary<string, string> datos)
    {
        SolicitudServicio s = ObtenerSolicitud(datos);
        string antes = Formato.NombreEstado(s.Estado);
        gestorSolicitudes.DeshacerUltimoCambio(s.Codigo);
        return Ok("Se deshizo el cambio: " + antes + " → " + Formato.NombreEstado(s.Estado) + ".");
    }

    private object Entregar(Dictionary<string, string> datos)
    {
        SolicitudServicio s = ObtenerSolicitud(datos);
        gestorSolicitudes.Entregar(s.Codigo, LeerEntero(datos, "calificacion", "Calificación"));
        return Ok("Equipo entregado. Total cobrado: " + Formato.Dinero(s.Total) + ".");
    }

    private object Cancelar(Dictionary<string, string> datos)
    {
        SolicitudServicio s = ObtenerSolicitud(datos);
        int devueltos = gestorSolicitudes.Cancelar(s.Codigo);
        return Ok("Solicitud " + s.Codigo + " cancelada. Repuestos devueltos al inventario: " + devueltos + ".");
    }

    // ======================================================
    //  REPUESTOS
    // ======================================================

    private object ListarRepuestos(List<Repuesto> lista)
    {
        string[] columnas = { "Código", "Nombre", "Categoría", "Existencia", "Mínimo", "Costo", "Precio", "Usados", "Estado" };
        List<string[]> filas = new List<string[]>();
        foreach (Repuesto r in lista)
        {
            filas.Add(FilaRepuesto(r));
        }
        return Tabla(columnas, filas);
    }

    private string[] FilaRepuesto(Repuesto r)
    {
        return new string[] { r.Codigo, r.Nombre, r.Categoria, r.Existencia.ToString(), r.ExistenciaMinima.ToString(),
                              Formato.Dinero(r.CostoUnitario), Formato.Dinero(r.PrecioVenta),
                              r.CantidadUtilizada.ToString(), r.EstadoInventario() };
    }

    private object RegistrarRepuesto(Dictionary<string, string> datos)
    {
        Repuesto r = new Repuesto(gestorRepuestos.GenerarCodigo(), Dato(datos, "nombre"), Dato(datos, "categoria"),
                                  LeerEntero(datos, "existencia", "Existencia"), LeerEntero(datos, "minima", "Existencia mínima"),
                                  LeerDecimal(datos, "costo", "Costo unitario"), LeerDecimal(datos, "precio", "Precio de venta"));
        gestorRepuestos.Registrar(r);
        return Ok("Repuesto registrado con el código " + r.Codigo + ".");
    }

    private object Reabastecer(Dictionary<string, string> datos)
    {
        Repuesto r = gestorRepuestos.BuscarPorCodigo(Dato(datos, "codigo"));
        if (r == null)
            throw new Exception("No existe el repuesto " + Dato(datos, "codigo") + ".");

        gestorRepuestos.Reabastecer(r.Codigo, LeerEntero(datos, "cantidad", "Cantidad"));
        return Ok("Nueva existencia de " + r.Nombre + ": " + r.Existencia + ".");
    }

    // ======================================================
    //  ATENCION DE COLAS
    // ======================================================

    private object VerColas()
    {
        return new
        {
            ok = true,
            urgente = TablaCola(gestorSolicitudes.ColaUrgente),
            normal = TablaCola(gestorSolicitudes.ColaNormal)
        };
    }

    private object TablaCola(Queue<SolicitudServicio> cola)
    {
        string[] columnas = { "#", "Código", "Cliente", "Equipo", "Fecha" };
        List<string[]> filas = new List<string[]>();
        int n = 1;
        foreach (SolicitudServicio s in cola)
        {
            filas.Add(new string[] { n.ToString(), s.Codigo, s.Cliente.NombreCompleto, s.Equipo.Descripcion(),
                                     Formato.Fecha(s.FechaRecepcion) });
            n++;
        }
        return Tabla(columnas, filas);
    }

    private object AtenderSiguiente()
    {
        int hoy = MatrizCarga.IndiceDia(DateTime.Now);
        SolicitudServicio s = gestorSolicitudes.AtenderSiguiente(gestorTecnicos, matriz, hoy);
        return Ok("Solicitud " + s.Codigo + " (" + s.Prioridad + ") asignada a " + s.Tecnico.NombreCompleto +
                  " el " + matriz.Dias[hoy] + ". Estado: " + Formato.NombreEstado(s.Estado) + ".");
    }

    private object VerMatriz()
    {
        List<Tecnico> tecnicos = gestorTecnicos.Tecnicos;
        List<object> filas = new List<object>();

        for (int i = 0; i < tecnicos.Count; i++)
        {
            int[] valores = new int[7];
            for (int j = 0; j < 7; j++)
            {
                valores[j] = matriz.ObtenerCarga(i, j);
            }
            filas.Add(new { tecnico = tecnicos[i].NombreCompleto, valores = valores, total = matriz.TotalTecnico(i) });
        }

        int[] totalesDia = new int[7];
        for (int j = 0; j < 7; j++)
        {
            totalesDia[j] = matriz.TotalDia(j, tecnicos.Count);
        }

        return new
        {
            ok = true,
            dias = matriz.Dias,
            filas = filas,
            totalesDia = totalesDia,
            mayor = matriz.TecnicoYDiaMayorCarga(tecnicos)
        };
    }

    // ======================================================
    //  ORDENAMIENTOS
    // ======================================================

    private object Ordenar(Dictionary<string, string> datos)
    {
        string algoritmo = Dato(datos, "algoritmo");
        bool asc = Dato(datos, "orden") != "desc";
        string textoOrden = asc ? "Ascendente" : "Descendente";

        string[] columnas;
        List<string[]> filas = new List<string[]>();
        string nombre;
        string criterio;

        if (algoritmo == "bubble")
        {
            nombre = "Bubble Sort";
            criterio = "Costo total de la solicitud";
            columnas = new string[] { "Código", "Cliente", "Equipo", "Estado", "Total" };
            foreach (SolicitudServicio s in Ordenador.BubbleSortPorCosto(gestorSolicitudes.Solicitudes, asc))
            {
                filas.Add(new string[] { s.Codigo, s.Cliente.NombreCompleto, s.Equipo.Descripcion(),
                                         Formato.NombreEstado(s.Estado), Formato.Dinero(s.Total) });
            }
        }
        else if (algoritmo == "quick")
        {
            nombre = "Quick Sort";
            criterio = "Fecha de recepción";
            columnas = new string[] { "Código", "Fecha", "Cliente", "Tiempo est.", "Estado" };
            foreach (SolicitudServicio s in Ordenador.QuickSortPorFecha(gestorSolicitudes.Solicitudes, asc))
            {
                string tiempo = s.TiempoEstimadoHoras > 0 ? s.TiempoEstimadoHoras + " h" : "Pendiente";
                filas.Add(new string[] { s.Codigo, Formato.Fecha(s.FechaRecepcion), s.Cliente.NombreCompleto,
                                         tiempo, Formato.NombreEstado(s.Estado) });
            }
        }
        else if (algoritmo == "selection")
        {
            nombre = "Selection Sort";
            criterio = "Servicios finalizados del técnico";
            columnas = new string[] { "Código", "Nombre", "Especialidad", "Finalizados", "Promedio" };
            foreach (Tecnico t in Ordenador.SelectionSortPorFinalizados(gestorTecnicos.Tecnicos, asc))
            {
                filas.Add(new string[] { t.Codigo, t.NombreCompleto, t.Especialidad.ToString(),
                                         t.ServiciosFinalizados.ToString(), t.CalificacionPromedio.ToString("0.00") });
            }
        }
        else if (algoritmo == "shell")
        {
            nombre = "Shell Sort";
            criterio = "Existencia del repuesto";
            columnas = new string[] { "Código", "Nombre", "Existencia", "Mínimo", "Estado" };
            foreach (Repuesto r in Ordenador.ShellSortPorExistencia(gestorRepuestos.Repuestos, asc))
            {
                filas.Add(new string[] { r.Codigo, r.Nombre, r.Existencia.ToString(),
                                         r.ExistenciaMinima.ToString(), r.EstadoInventario() });
            }
        }
        else
        {
            throw new Exception("Seleccione un algoritmo.");
        }

        return new { ok = true, algoritmo = nombre, criterio = criterio, orden = textoOrden, columnas = columnas, filas = filas };
    }

    // ======================================================
    //  BUSQUEDAS
    // ======================================================

    private object Buscar(Dictionary<string, string> datos)
    {
        string tipo = Dato(datos, "tipo");
        string texto = Dato(datos, "texto");

        if (tipo == "nombre")
        {
            List<Cliente> lista = Buscador.SecuencialClientesPorNombre(gestorClientes.Clientes, texto);
            List<string[]> filas = new List<string[]>();
            foreach (Cliente c in lista) filas.Add(FilaCliente(c));
            return new
            {
                ok = true,
                mensaje = "Búsqueda secuencial: " + lista.Count + " coincidencia(s) de " + gestorClientes.Clientes.Count + " clientes.",
                columnas = new string[] { "Código", "Nombre", "Teléfono", "Correo", "Tipo", "Solicitudes" },
                filas = filas
            };
        }
        else if (tipo == "estado")
        {
            int estado = LeerEntero(datos, "texto", "Estado");
            if (estado < 0 || estado > 7)
                throw new Exception("Seleccione un estado válido.");

            List<SolicitudServicio> lista = Buscador.SecuencialSolicitudesPorEstado(gestorSolicitudes.Solicitudes, (EstadoSolicitud)estado);
            List<string[]> filas = new List<string[]>();
            foreach (SolicitudServicio s in lista) filas.Add(FilaSolicitud(s));
            return new
            {
                ok = true,
                mensaje = "Búsqueda secuencial: " + lista.Count + " solicitud(es) en estado " + Formato.NombreEstado((EstadoSolicitud)estado) + ".",
                columnas = new string[] { "Código", "Fecha", "Cliente", "Equipo", "Prioridad", "Estado", "Técnico", "Total" },
                filas = filas
            };
        }
        else if (tipo == "marca")
        {
            List<Equipo> lista = Buscador.SecuencialEquiposPorMarca(gestorEquipos.Equipos, texto);
            List<string[]> filas = new List<string[]>();
            foreach (Equipo e in lista)
            {
                filas.Add(new string[] { e.CodigoInterno, e.TipoEquipo(), e.Marca, e.Modelo, e.NumeroSerie, e.Propietario.NombreCompleto });
            }
            return new
            {
                ok = true,
                mensaje = "Búsqueda secuencial: " + lista.Count + " equipo(s) de la marca " + texto + ".",
                columnas = new string[] { "Código", "Tipo", "Marca", "Modelo", "Serie", "Propietario" },
                filas = filas
            };
        }
        else if (tipo == "codigo")
        {
            SolicitudServicio s = Buscador.BinariaSolicitudPorCodigo(gestorSolicitudes.Solicitudes, texto);
            List<string[]> filas = new List<string[]>();
            if (s != null) filas.Add(FilaSolicitud(s));

            string mensaje = s == null ? "Búsqueda binaria: no se encontró " + texto.ToUpper() + "." : "Búsqueda binaria: solicitud encontrada.";
            mensaje += " Comparaciones: " + Buscador.ComparacionesBinaria + " de " + gestorSolicitudes.Solicitudes.Count + " solicitudes.";
            return new
            {
                ok = true,
                mensaje = mensaje,
                columnas = new string[] { "Código", "Fecha", "Cliente", "Equipo", "Prioridad", "Estado", "Técnico", "Total" },
                filas = filas
            };
        }

        throw new Exception("Seleccione un tipo de búsqueda.");
    }

    // ======================================================
    //  REPORTES (el mismo texto que en consola)
    // ======================================================

    private object Reporte(Dictionary<string, string> datos)
    {
        int numero = LeerEntero(datos, "numero", "Reporte");
        bool asc = Dato(datos, "orden") != "desc";
        string texto;

        switch (numero)
        {
            case 1: texto = reportes.SolicitudesEnEspera(); break;
            case 2: texto = reportes.SolicitudesActivasFinalizadasCanceladas(); break;
            case 3: texto = reportes.TecnicosDisponiblesYCargaCompleta(); break;
            case 4: texto = reportes.TecnicoConMasFinalizados(); break;
            case 5: texto = reportes.EquiposPorTipo(); break;
            case 6: texto = reportes.SolicitudesPorCosto(asc); break;
            case 7: texto = reportes.SolicitudesPorFecha(asc); break;
            case 8: texto = reportes.RepuestosPorExistencia(asc); break;
            case 9: texto = reportes.RepuestosEnMinimo(); break;
            case 10: texto = reportes.IngresosTotales(); break;
            case 11:
                int estado = LeerEntero(datos, "estado", "Estado");
                if (estado < 0 || estado > 7) estado = 0;
                texto = reportes.ResultadosBusquedas(Dato(datos, "nombre"), (EstadoSolicitud)estado,
                                                     Dato(datos, "marca"), Dato(datos, "codigo"));
                break;
            case 12: texto = reportes.MatrizSemanal(); break;
            default: throw new Exception("El reporte debe estar entre 1 y 12.");
        }
        return new { ok = true, texto = texto };
    }

    // ======================================================
    //  DATOS DE PRUEBA
    // ======================================================

    private object DatosPrueba(Dictionary<string, string> datos)
    {
        int cantidad = LeerEntero(datos, "cantidad", "Cantidad de clientes");
        string resumen = generador.GenerarDatosPrueba(cantidad);
        return Ok(resumen);
    }

    // ======================================================
    //  PORTAL DEL CLIENTE
    // ======================================================

    // El cliente solo ve el estado, el equipo y el tecnico de la solicitud que consulta.
    // Se usa la BUSQUEDA BINARIA por codigo.
    private object EstadoParaCliente(Dictionary<string, string> datos)
    {
        string codigo = Dato(datos, "codigo");
        SolicitudServicio s = Buscador.BinariaSolicitudPorCodigo(gestorSolicitudes.Solicitudes, codigo);
        if (s == null)
            throw new Exception("No se encontró la solicitud " + codigo.ToUpper() + ". Revise el código.");

        return new
        {
            ok = true,
            codigo = s.Codigo,
            estado = s.Estado.ToString(),
            estadoTexto = Formato.NombreEstado(s.Estado),
            equipo = s.Equipo.TipoEquipo() + " " + s.Equipo.Marca,
            tecnico = s.Tecnico == null ? "Pendiente de asignar" : s.Tecnico.NombreCompleto,
            fecha = Formato.Fecha(s.FechaRecepcion),
            total = s.Estado == EstadoSolicitud.ListaParaEntregar || s.Estado == EstadoSolicitud.Entregada ? Formato.Dinero(s.Total) : "",
            comparaciones = Buscador.ComparacionesBinaria
        };
    }
}
