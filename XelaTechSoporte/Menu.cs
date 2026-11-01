using System.Globalization;

namespace XelaTechSoporte;

// Menu de consola: muestra las opciones, lee los datos y llama a los gestores.
public class Menu
{
    // Todos los datos del sistema viven aqui (en memoria, sin base de datos)
    private GestorClientes gestorClientes = new GestorClientes();
    private GestorTecnicos gestorTecnicos = new GestorTecnicos();
    private GestorEquipos gestorEquipos = new GestorEquipos();
    private GestorRepuestos gestorRepuestos = new GestorRepuestos();
    private GestorSolicitudes gestorSolicitudes = new GestorSolicitudes();
    private MatrizCarga matriz = new MatrizCarga();
    private Reportes reportes;
    private GeneradorDatos generador;

    public Menu()
    {
        reportes = new Reportes(gestorClientes, gestorTecnicos, gestorEquipos, gestorRepuestos, gestorSolicitudes, matriz);
        generador = new GeneradorDatos(gestorClientes, gestorTecnicos, gestorEquipos, gestorRepuestos, gestorSolicitudes, matriz);
    }

    // ======================================================
    //  MENU PRINCIPAL
    // ======================================================
    public void Iniciar()
    {
        int opcion = 0;
        while (opcion != 11)
        {
            LimpiarPantalla();
            Console.WriteLine("============================================");
            Console.WriteLine("              XELATECH SOPORTE");
            Console.WriteLine("============================================");
            Console.WriteLine("  1. Gestión de clientes");
            Console.WriteLine("  2. Gestión de técnicos");
            Console.WriteLine("  3. Gestión de equipos");
            Console.WriteLine("  4. Gestión de solicitudes");
            Console.WriteLine("  5. Gestión de repuestos");
            Console.WriteLine("  6. Atención de colas");
            Console.WriteLine("  7. Ordenamientos");
            Console.WriteLine("  8. Búsquedas");
            Console.WriteLine("  9. Reportes");
            Console.WriteLine(" 10. Generar datos de prueba");
            Console.WriteLine(" 11. Salir");
            Console.WriteLine(" 12. Modo gráfico (HTML)");
            Console.WriteLine("============================================");

            // Validacion adicional 22: la opcion debe ser numerica y estar en rango
            opcion = LeerEntero("  Seleccione una opción: ", 1, 12);

            switch (opcion)
            {
                case 1: SubmenuClientes(); break;
                case 2: SubmenuTecnicos(); break;
                case 3: SubmenuEquipos(); break;
                case 4: SubmenuSolicitudes(); break;
                case 5: SubmenuRepuestos(); break;
                case 6: SubmenuColas(); break;
                case 7: SubmenuOrdenamientos(); break;
                case 8: SubmenuBusquedas(); break;
                case 9: SubmenuReportes(); break;
                case 10: OpcionDatosPrueba(); break;
                case 12: ModoGrafico(); break;
            }
        }
        Console.WriteLine();
        Console.WriteLine("  Gracias por usar XelaTech Soporte.");
    }

    // ======================================================
    //  1. CLIENTES
    // ======================================================
    private void SubmenuClientes()
    {
        int op = 0;
        while (op != 5)
        {
            Titulo("GESTIÓN DE CLIENTES");
            Console.WriteLine("  1. Registrar");
            Console.WriteLine("  2. Consultar");
            Console.WriteLine("  3. Actualizar");
            Console.WriteLine("  4. Listar");
            Console.WriteLine("  5. Volver");
            op = LeerEntero("  Seleccione una opción: ", 1, 5);
            if (op == 5) break;

            try
            {
                switch (op)
                {
                    case 1: RegistrarCliente(); break;
                    case 2: ConsultarCliente(); break;
                    case 3: ActualizarCliente(); break;
                    case 4: ListarClientes(); break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    private Cliente RegistrarCliente()
    {
        Console.WriteLine();
        Console.WriteLine("  --- Registrar cliente ---");
        string codigo = gestorClientes.GenerarCodigo();
        Console.WriteLine("  Código asignado: " + codigo);

        string nombre = LeerTexto("  Nombre completo: ");

        string tel = LeerTexto("  Teléfono (8 dígitos): ");
        while (!Cliente.EsTelefonoValido(tel))
        {
            Console.WriteLine("  El teléfono debe tener 8 dígitos numéricos.");
            tel = LeerTexto("  Teléfono (8 dígitos): ");
        }

        string correo = LeerTexto("  Correo electrónico: ");
        while (!Cliente.EsCorreoValido(correo))
        {
            Console.WriteLine("  El correo debe tener @ y un punto después (ejemplo: ana@gmail.com).");
            correo = LeerTexto("  Correo electrónico: ");
        }

        TipoCliente tipo = LeerTipoCliente();

        Cliente c = new Cliente(codigo, nombre, tel, correo, tipo);
        gestorClientes.Registrar(c);

        Console.WriteLine();
        Console.WriteLine("  Cliente registrado correctamente:");
        Console.WriteLine("  " + c.ToString());
        return c;
    }

    private void ConsultarCliente()
    {
        Cliente c = PedirCliente();
        if (c == null) return;

        Console.WriteLine();
        Console.WriteLine("  " + c.ToString());

        Console.WriteLine();
        Console.WriteLine("  Equipos del cliente:");
        List<Equipo> equipos = gestorEquipos.EquiposDeCliente(c);
        if (equipos.Count == 0) Console.WriteLine("    (ninguno)");
        foreach (Equipo e in equipos)
        {
            Console.WriteLine("    " + e.CodigoInterno + " - " + e.Descripcion() + " - Serie " + e.NumeroSerie);
        }

        Console.WriteLine();
        Console.WriteLine("  Solicitudes del cliente:");
        List<SolicitudServicio> lista = gestorSolicitudes.SolicitudesDeCliente(c);
        if (lista.Count == 0) Console.WriteLine("    (ninguna)");
        foreach (SolicitudServicio s in lista)
        {
            Console.WriteLine("    " + s.Codigo + " - " + s.Equipo.Descripcion() + " - " + Formato.NombreEstado(s.Estado) +
                              " - " + Formato.Dinero(s.Total));
        }
    }

    private void ActualizarCliente()
    {
        Cliente c = PedirCliente();
        if (c == null) return;

        Console.WriteLine("  Datos actuales: " + c.ToString());
        Console.WriteLine("  (Deje en blanco y presione ENTER para conservar el dato actual)");

        string tel = LeerTextoOpcional("  Nuevo teléfono: ");
        if (tel == "") tel = c.Telefono;

        string correo = LeerTextoOpcional("  Nuevo correo: ");
        if (correo == "") correo = c.Correo;

        gestorClientes.Actualizar(c.Codigo, tel, correo);
        Console.WriteLine("  Datos actualizados: " + c.ToString());
    }

    private void ListarClientes()
    {
        List<Cliente> lista = gestorClientes.Clientes;
        Console.WriteLine();
        if (lista.Count == 0)
        {
            Console.WriteLine("  No hay clientes registrados.");
            return;
        }

        Console.WriteLine("  " + Formato.Col("Código", 10) + Formato.Col("Nombre", 28) + Formato.Col("Teléfono", 10) +
                          Formato.Col("Correo", 30) + Formato.Col("Tipo", 17) + "Solic.");
        Console.WriteLine("  " + new string('-', 101));
        foreach (Cliente c in lista)
        {
            Console.WriteLine("  " + Formato.Col(c.Codigo, 10) + Formato.Col(c.NombreCompleto, 28) + Formato.Col(c.Telefono, 10) +
                              Formato.Col(c.Correo, 30) + Formato.Col(Formato.NombreTipoCliente(c.Tipo), 17) + c.CantidadSolicitudes);
        }
        Console.WriteLine("  Total: " + lista.Count);
    }

    // ======================================================
    //  2. TECNICOS
    // ======================================================
    private void SubmenuTecnicos()
    {
        int op = 0;
        while (op != 6)
        {
            Titulo("GESTIÓN DE TÉCNICOS");
            Console.WriteLine("  1. Registrar");
            Console.WriteLine("  2. Consultar");
            Console.WriteLine("  3. Listar con especialidades");
            Console.WriteLine("  4. Registrar calificación");
            Console.WriteLine("  5. Cambiar disponibilidad");
            Console.WriteLine("  6. Volver");
            op = LeerEntero("  Seleccione una opción: ", 1, 6);
            if (op == 6) break;

            try
            {
                switch (op)
                {
                    case 1: RegistrarTecnico(); break;
                    case 2: ConsultarTecnico(); break;
                    case 3: ListarTecnicos(); break;
                    case 4: RegistrarCalificacion(); break;
                    case 5: CambiarDisponibilidad(); break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    private void RegistrarTecnico()
    {
        Console.WriteLine();
        Console.WriteLine("  --- Registrar técnico ---");
        string codigo = gestorTecnicos.GenerarCodigo();
        Console.WriteLine("  Código asignado: " + codigo);

        string nombre = LeerTexto("  Nombre completo: ");

        Console.WriteLine("  Especialidad: 1. Computadoras  2. Impresoras  3. General");
        int esp = LeerEntero("  Opción: ", 1, 3);

        bool disponible = LeerSiNo("  ¿Está disponible? (s/n): ");

        Tecnico t = new Tecnico(codigo, nombre, (Especialidad)(esp - 1), disponible);
        gestorTecnicos.Registrar(t);
        Console.WriteLine("  Técnico registrado: " + t.ToString());
    }

    private void ConsultarTecnico()
    {
        string cod = LeerTexto("  Código del técnico: ");
        Tecnico t = gestorTecnicos.BuscarPorCodigo(cod);
        if (t == null)
        {
            Console.WriteLine("  No existe el técnico " + cod + ".");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("  " + t.ToString());
        Console.WriteLine("  Calificaciones recibidas (arreglo de 5 posiciones):");
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine("    " + (i + 1) + " estrella(s): " + t.Calificaciones[i]);
        }
    }

    private void ListarTecnicos()
    {
        List<Tecnico> lista = gestorTecnicos.Tecnicos;
        Console.WriteLine();
        if (lista.Count == 0)
        {
            Console.WriteLine("  No hay técnicos registrados.");
            return;
        }

        Console.WriteLine("  " + Formato.Col("Código", 9) + Formato.Col("Nombre", 26) + Formato.Col("Especialidad", 14) +
                          Formato.Col("Disponible", 12) + Formato.Col("Activos", 9) + Formato.Col("Finaliz.", 10) + "Promedio");
        Console.WriteLine("  " + new string('-', 88));
        foreach (Tecnico t in lista)
        {
            Console.WriteLine("  " + Formato.Col(t.Codigo, 9) + Formato.Col(t.NombreCompleto, 26) + Formato.Col(t.Especialidad.ToString(), 14) +
                              Formato.Col(t.Disponible ? "Sí" : "No", 12) + Formato.Col(t.TrabajosActivos + "/" + Tecnico.LIMITE_ACTIVOS, 9) +
                              Formato.Col(t.ServiciosFinalizados.ToString(), 10) + t.CalificacionPromedio.ToString("0.00"));
        }
    }

    private void RegistrarCalificacion()
    {
        Console.WriteLine("  La calificación se registra sobre una solicitud ENTREGADA que no se haya calificado.");
        SolicitudServicio s = PedirSolicitud();
        if (s == null) return;

        int c = LeerEntero("  Calificación (1 a 5): ", 1, 5);
        s.Calificar(c);
        Console.WriteLine("  Calificación registrada para " + s.Tecnico.NombreCompleto +
                          ". Nuevo promedio: " + s.Tecnico.CalificacionPromedio.ToString("0.00"));
    }

    private void CambiarDisponibilidad()
    {
        string cod = LeerTexto("  Código del técnico: ");
        Tecnico t = gestorTecnicos.BuscarPorCodigo(cod);
        if (t == null)
        {
            Console.WriteLine("  No existe el técnico " + cod + ".");
            return;
        }

        t.Disponible = !t.Disponible;
        Console.WriteLine("  " + t.NombreCompleto + " ahora está " + (t.Disponible ? "DISPONIBLE" : "NO DISPONIBLE") + ".");
    }

    // ======================================================
    //  3. EQUIPOS
    // ======================================================
    private void SubmenuEquipos()
    {
        int op = 0;
        while (op != 5)
        {
            Titulo("GESTIÓN DE EQUIPOS");
            Console.WriteLine("  1. Registrar laptop");
            Console.WriteLine("  2. Registrar escritorio");
            Console.WriteLine("  3. Registrar impresora");
            Console.WriteLine("  4. Listar");
            Console.WriteLine("  5. Volver");
            op = LeerEntero("  Seleccione una opción: ", 1, 5);
            if (op == 5) break;

            try
            {
                if (op == 4)
                {
                    ListarEquipos();
                }
                else
                {
                    Cliente dueno = PedirCliente();
                    if (dueno != null) RegistrarEquipo(op, dueno);
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    // tipo: 1 = laptop, 2 = escritorio, 3 = impresora
    private Equipo RegistrarEquipo(int tipo, Cliente dueno)
    {
        Console.WriteLine();
        Console.WriteLine("  --- Registrar equipo para " + dueno.NombreCompleto + " ---");
        string codigo = gestorEquipos.GenerarCodigo();
        Console.WriteLine("  Código interno asignado: " + codigo);

        // El numero de serie no se puede repetir
        string serie = LeerTexto("  Número de serie: ");
        while (gestorEquipos.ExisteSerie(serie))
        {
            Console.WriteLine("  Ya existe un equipo con esa serie. Ingrese otra.");
            serie = LeerTexto("  Número de serie: ");
        }

        string marca = LeerTexto("  Marca: ");
        string modelo = LeerTexto("  Modelo: ");
        string estadoFisico = LeerTexto("  Estado físico recibido: ");
        string accesorios = LeerTextoOpcional("  Accesorios entregados (ENTER si ninguno): ");

        Equipo equipo = null;
        if (tipo == 1)
        {
            double pulgadas = LeerDouble("  Tamaño de pantalla en pulgadas (10 a 20): ", 10, 20);
            Console.WriteLine("  Estado de batería: 1. Buena  2. Regular  3. Mala  4. No retiene carga");
            string[] baterias = { "Buena", "Regular", "Mala", "No retiene carga" };
            int b = LeerEntero("  Opción: ", 1, 4);
            equipo = new Laptop(codigo, serie, marca, modelo, dueno, estadoFisico, accesorios, pulgadas, baterias[b - 1]);
        }
        else if (tipo == 2)
        {
            Console.WriteLine("  Tipo de gabinete: 1. Torre  2. Mini torre  3. Todo en uno");
            string[] gabinetes = { "Torre", "Mini torre", "Todo en uno" };
            int g = LeerEntero("  Opción: ", 1, 3);
            bool fuente = LeerSiNo("  ¿Tiene fuente externa? (s/n): ");
            equipo = new Escritorio(codigo, serie, marca, modelo, dueno, estadoFisico, accesorios, gabinetes[g - 1], fuente);
        }
        else
        {
            Console.WriteLine("  Tecnología: 1. Tinta  2. Láser  3. Matricial");
            string[] tecnologias = { "Tinta", "Láser", "Matricial" };
            int t = LeerEntero("  Opción: ", 1, 3);
            int tinta = LeerEntero("  Nivel de tinta/tóner en % (0 a 100): ", 0, 100);
            equipo = new Impresora(codigo, serie, marca, modelo, dueno, estadoFisico, accesorios, tecnologias[t - 1], tinta);
        }

        gestorEquipos.Registrar(equipo);

        // Polimorfismo: cada tipo calcula su propio costo de revision
        Console.WriteLine();
        Console.WriteLine("  Equipo registrado: " + equipo.ToString());
        Console.WriteLine("  Costo de revisión (" + equipo.TipoEquipo() + "): " + Formato.Dinero(equipo.CalcularCostoRevision()));
        return equipo;
    }

    private void ListarEquipos()
    {
        List<Equipo> lista = gestorEquipos.Equipos;
        Console.WriteLine();
        if (lista.Count == 0)
        {
            Console.WriteLine("  No hay equipos registrados.");
            return;
        }

        Console.WriteLine("  " + Formato.Col("Código", 9) + Formato.Col("Tipo", 11) + Formato.Col("Marca / modelo", 22) +
                          Formato.Col("Serie", 12) + Formato.Col("Propietario", 24) + "Revisión");
        Console.WriteLine("  " + new string('-', 88));
        foreach (Equipo e in lista)
        {
            Console.WriteLine("  " + Formato.Col(e.CodigoInterno, 9) + Formato.Col(e.TipoEquipo(), 11) +
                              Formato.Col(e.Marca + " " + e.Modelo, 22) + Formato.Col(e.NumeroSerie, 12) +
                              Formato.Col(e.Propietario.NombreCompleto, 24) + Formato.Dinero(e.CalcularCostoRevision()));
        }
        Console.WriteLine("  Total: " + lista.Count);
    }

    // ======================================================
    //  4. SOLICITUDES
    // ======================================================
    private void SubmenuSolicitudes()
    {
        int op = 0;
        while (op != 10)
        {
            Titulo("GESTIÓN DE SOLICITUDES");
            Console.WriteLine("  1. Crear solicitud normal");
            Console.WriteLine("  2. Crear solicitud urgente");
            Console.WriteLine("  3. Registrar diagnóstico y repuestos");
            Console.WriteLine("  4. Actualizar estado");
            Console.WriteLine("  5. Ver historial");
            Console.WriteLine("  6. Deshacer último cambio");
            Console.WriteLine("  7. Entregar");
            Console.WriteLine("  8. Cancelar");
            Console.WriteLine("  9. Listar solicitudes");
            Console.WriteLine(" 10. Volver");
            op = LeerEntero("  Seleccione una opción: ", 1, 10);
            if (op == 10) break;

            try
            {
                switch (op)
                {
                    case 1: CrearSolicitud(Prioridad.Normal); break;
                    case 2: CrearSolicitud(Prioridad.Urgente); break;
                    case 3: RegistrarDiagnosticoYRepuestos(); break;
                    case 4: ActualizarEstado(); break;
                    case 5: VerHistorial(); break;
                    case 6: DeshacerCambio(); break;
                    case 7: EntregarSolicitud(); break;
                    case 8: CancelarSolicitud(); break;
                    case 9: ListarSolicitudes(); break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    // Sigue el diagrama de flujo: localizar/registrar cliente -> equipo -> solicitud -> cola
    private void CrearSolicitud(Prioridad prioridad)
    {
        Console.WriteLine();
        Console.WriteLine("  --- Nueva solicitud " + prioridad.ToString().ToUpper() + " ---");

        // 1. Registrar o localizar al cliente
        string codCli = LeerTexto("  Código del cliente (o escriba NUEVO para registrarlo): ");
        Cliente cli;
        if (codCli.ToUpper() == "NUEVO")
        {
            cli = RegistrarCliente();
        }
        else
        {
            cli = gestorClientes.BuscarPorCodigo(codCli);
            if (cli == null)
            {
                Console.WriteLine("  No existe el cliente " + codCli + ".");
                return;
            }
        }

        // 2. Elegir o registrar el equipo
        Console.WriteLine();
        Console.WriteLine("  Equipos de " + cli.NombreCompleto + ":");
        List<Equipo> equipos = gestorEquipos.EquiposDeCliente(cli);
        if (equipos.Count == 0) Console.WriteLine("    (no tiene equipos registrados)");
        foreach (Equipo e in equipos)
        {
            string activa = gestorSolicitudes.ExisteActivaParaEquipo(e) ? "  [ya tiene solicitud activa]" : "";
            Console.WriteLine("    " + e.CodigoInterno + " - " + e.Descripcion() + " - Serie " + e.NumeroSerie + activa);
        }

        string codEq = LeerTexto("  Código del equipo (o escriba NUEVO para registrarlo): ");
        Equipo eq;
        if (codEq.ToUpper() == "NUEVO")
        {
            Console.WriteLine("  Tipo de equipo: 1. Laptop  2. Escritorio  3. Impresora");
            int tipo = LeerEntero("  Opción: ", 1, 3);
            eq = RegistrarEquipo(tipo, cli);
        }
        else
        {
            eq = gestorEquipos.BuscarPorCodigo(codEq);
            if (eq == null)
            {
                Console.WriteLine("  No existe el equipo " + codEq + ".");
                return;
            }
        }

        // 3. Crear la solicitud (el gestor valida y la mete en la cola)
        string desc = LeerTexto("  Descripción del problema: ");
        SolicitudServicio s = gestorSolicitudes.CrearSolicitud(cli, eq, desc, prioridad);

        Console.WriteLine();
        Console.WriteLine("  Solicitud creada: " + s.Codigo);
        Console.WriteLine("  Fecha de recepción: " + Formato.Fecha(s.FechaRecepcion));
        Console.WriteLine("  Estado: " + Formato.NombreEstado(s.Estado) + " (cola " + prioridad.ToString().ToLower() + ")");
    }

    private void RegistrarDiagnosticoYRepuestos()
    {
        SolicitudServicio s = PedirSolicitud();
        if (s == null) return;

        Console.WriteLine("  " + s.ToString());

        // Diagnostico (si esta Asignada es obligatorio; si ya esta En diagnostico se puede corregir)
        bool registrar = false;
        if (s.Estado == EstadoSolicitud.Asignada)
            registrar = true;
        else if (s.Estado == EstadoSolicitud.EnDiagnostico)
            registrar = LeerSiNo("  Ya tiene diagnóstico. ¿Desea modificarlo? (s/n): ");
        else if (s.Estado != EstadoSolicitud.EnReparacion)
            throw new Exception("La solicitud debe estar Asignada, En diagnóstico o En reparación. Estado actual: " + Formato.NombreEstado(s.Estado));

        if (registrar)
        {
            // Polimorfismo: cada equipo sugiere su propia revision
            Console.WriteLine("  Revisión sugerida para " + s.Equipo.TipoEquipo() + ": " + s.Equipo.Diagnosticar());
            string texto = LeerTexto("  Diagnóstico: ");
            int horas = LeerEntero("  Tiempo estimado en horas (1 a 100): ", 1, 100);
            s.RegistrarDiagnostico(texto, horas);
            Console.WriteLine("  Diagnóstico registrado. Mano de obra: " + Formato.Dinero(s.CostoManoObra) +
                              (s.Prioridad == Prioridad.Urgente ? " (incluye recargo urgente del 20%)" : ""));
        }

        // Repuestos
        while (LeerSiNo("  ¿Desea agregar un repuesto? (s/n): "))
        {
            MostrarRepuestosCorto();
            string codRep = LeerTexto("  Código del repuesto: ");
            Repuesto r = gestorRepuestos.BuscarPorCodigo(codRep);
            if (r == null)
            {
                Console.WriteLine("  No existe el repuesto " + codRep + ".");
                continue;
            }

            int cant = LeerEntero("  Cantidad: ", 1, 1000);
            try
            {
                s.AgregarRepuesto(r, cant);
                Console.WriteLine("  Agregado: " + r.Nombre + " x" + cant + " = " + Formato.Dinero(r.PrecioVenta * cant));

                // Alerta de inventario
                if (r.EstaAgotado())
                    Console.WriteLine("  *** ALERTA: " + r.Nombre + " se AGOTÓ. ***");
                else if (r.EstaEnMinimo())
                    Console.WriteLine("  *** ALERTA: " + r.Nombre + " llegó a su existencia mínima (" + r.Existencia + " disponibles). ***");
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        Console.WriteLine();
        Console.WriteLine("  Costos parciales -> Mano de obra: " + Formato.Dinero(s.CostoManoObra) +
                          " | Repuestos: " + Formato.Dinero(s.CostoRepuestos) + " | Total: " + Formato.Dinero(s.Total));
        Console.WriteLine("  Estado actual: " + Formato.NombreEstado(s.Estado));
    }

    private void ActualizarEstado()
    {
        SolicitudServicio s = PedirSolicitud();
        if (s == null) return;

        Console.WriteLine("  Estado actual: " + Formato.NombreEstado(s.Estado));
        Console.WriteLine("  Estados permitidos desde aquí: " + s.EstadosPermitidosTexto());
        Console.WriteLine();
        for (int i = 0; i <= 7; i++)
        {
            Console.WriteLine("  " + (i + 1) + ". " + Formato.NombreEstado((EstadoSolicitud)i));
        }
        int op = LeerEntero("  Nuevo estado: ", 1, 8);

        gestorSolicitudes.CambiarEstado(s.Codigo, (EstadoSolicitud)(op - 1));
        Console.WriteLine("  Estado actualizado a: " + Formato.NombreEstado(s.Estado));
        if (s.Estado == EstadoSolicitud.ListaParaEntregar)
            Console.WriteLine("  Total a cobrar: " + Formato.Dinero(s.Total));
    }

    private void VerHistorial()
    {
        SolicitudServicio s = PedirSolicitud();
        if (s == null) return;
        Console.WriteLine();
        Console.WriteLine(s.VerHistorial());
    }

    private void DeshacerCambio()
    {
        SolicitudServicio s = PedirSolicitud();
        if (s == null) return;

        string antes = Formato.NombreEstado(s.Estado);
        gestorSolicitudes.DeshacerUltimoCambio(s.Codigo);
        Console.WriteLine("  Se deshizo el cambio: " + antes + " -> " + Formato.NombreEstado(s.Estado));
    }

    private void EntregarSolicitud()
    {
        SolicitudServicio s = PedirSolicitud();
        if (s == null) return;

        if (s.Estado != EstadoSolicitud.ListaParaEntregar)
            throw new Exception("Solo se entregan solicitudes Lista para entregar. Estado actual: " + Formato.NombreEstado(s.Estado));

        Console.WriteLine();
        Console.WriteLine(s.Comprobante());
        int cal = LeerEntero("  Calificación del cliente para el técnico (1 a 5, 0 = no calificar): ", 0, 5);

        gestorSolicitudes.Entregar(s.Codigo, cal);
        Console.WriteLine("  Equipo entregado. Estado: " + Formato.NombreEstado(s.Estado) + ". Total cobrado: " + Formato.Dinero(s.Total));
    }

    private void CancelarSolicitud()
    {
        SolicitudServicio s = PedirSolicitud();
        if (s == null) return;

        Console.WriteLine("  " + s.ToString());
        if (!LeerSiNo("  ¿Seguro que desea cancelarla? (s/n): ")) return;

        int devueltos = gestorSolicitudes.Cancelar(s.Codigo);
        Console.WriteLine("  Solicitud cancelada. Repuestos devueltos al inventario: " + devueltos);
    }

    private void ListarSolicitudes()
    {
        List<SolicitudServicio> lista = gestorSolicitudes.Solicitudes;
        Console.WriteLine();
        if (lista.Count == 0)
        {
            Console.WriteLine("  No hay solicitudes registradas.");
            return;
        }

        Console.WriteLine("  " + Formato.Col("Código", 10) + Formato.Col("Cliente", 22) + Formato.Col("Equipo", 20) +
                          Formato.Col("Prioridad", 10) + Formato.Col("Estado", 20) + Formato.Col("Técnico", 18) + "Total");
        Console.WriteLine("  " + new string('-', 110));
        foreach (SolicitudServicio s in lista)
        {
            string tec = s.Tecnico == null ? "-" : s.Tecnico.NombreCompleto;
            Console.WriteLine("  " + Formato.Col(s.Codigo, 10) + Formato.Col(s.Cliente.NombreCompleto, 22) +
                              Formato.Col(s.Equipo.Descripcion(), 20) + Formato.Col(s.Prioridad.ToString(), 10) +
                              Formato.Col(Formato.NombreEstado(s.Estado), 20) + Formato.Col(tec, 18) + Formato.Dinero(s.Total));
        }
        Console.WriteLine("  Total: " + lista.Count);
    }

    // ======================================================
    //  5. REPUESTOS
    // ======================================================
    private void SubmenuRepuestos()
    {
        int op = 0;
        while (op != 6)
        {
            Titulo("GESTIÓN DE REPUESTOS");
            Console.WriteLine("  1. Registrar");
            Console.WriteLine("  2. Consultar");
            Console.WriteLine("  3. Reabastecer");
            Console.WriteLine("  4. Ver existencias mínimas");
            Console.WriteLine("  5. Listar inventario");
            Console.WriteLine("  6. Volver");
            op = LeerEntero("  Seleccione una opción: ", 1, 6);
            if (op == 6) break;

            try
            {
                switch (op)
                {
                    case 1: RegistrarRepuesto(); break;
                    case 2: ConsultarRepuesto(); break;
                    case 3: ReabastecerRepuesto(); break;
                    case 4: Console.WriteLine(); Console.WriteLine(reportes.RepuestosEnMinimo()); break;
                    case 5: MostrarRepuestosCorto(); break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    private void RegistrarRepuesto()
    {
        Console.WriteLine();
        Console.WriteLine("  --- Registrar repuesto ---");
        string codigo = gestorRepuestos.GenerarCodigo();
        Console.WriteLine("  Código asignado: " + codigo);

        string nombre = LeerTexto("  Nombre: ");
        string categoria = LeerTexto("  Categoría: ");
        int existencia = LeerEntero("  Existencia inicial: ", 0, 100000);
        int minima = LeerEntero("  Existencia mínima: ", 0, 100000);
        decimal costo = LeerDecimal("  Costo unitario (Q): ", 0.01m);
        decimal precio = LeerDecimal("  Precio de venta (Q): ", 0.01m);
        while (precio < costo)
        {
            Console.WriteLine("  El precio de venta no puede ser menor que el costo (" + Formato.Dinero(costo) + ").");
            precio = LeerDecimal("  Precio de venta (Q): ", 0.01m);
        }

        Repuesto r = new Repuesto(codigo, nombre, categoria, existencia, minima, costo, precio);
        gestorRepuestos.Registrar(r);
        Console.WriteLine("  Repuesto registrado: " + r.ToString());
    }

    private void ConsultarRepuesto()
    {
        string cod = LeerTexto("  Código del repuesto: ");
        Repuesto r = gestorRepuestos.BuscarPorCodigo(cod);
        if (r == null)
        {
            Console.WriteLine("  No existe el repuesto " + cod + ".");
            return;
        }
        Console.WriteLine("  " + r.ToString());
        Console.WriteLine("  Estado del inventario: " + r.EstadoInventario());
    }

    private void ReabastecerRepuesto()
    {
        string cod = LeerTexto("  Código del repuesto: ");
        Repuesto r = gestorRepuestos.BuscarPorCodigo(cod);
        if (r == null)
        {
            Console.WriteLine("  No existe el repuesto " + cod + ".");
            return;
        }
        Console.WriteLine("  Existencia actual: " + r.Existencia);
        int cant = LeerEntero("  Cantidad a ingresar: ", 1, 100000);
        gestorRepuestos.Reabastecer(r.Codigo, cant);
        Console.WriteLine("  Nueva existencia de " + r.Nombre + ": " + r.Existencia);
    }

    private void MostrarRepuestosCorto()
    {
        List<Repuesto> lista = gestorRepuestos.Repuestos;
        Console.WriteLine();
        if (lista.Count == 0)
        {
            Console.WriteLine("  No hay repuestos registrados.");
            return;
        }
        Console.WriteLine("  " + Formato.Col("Código", 9) + Formato.Col("Nombre", 26) + Formato.Col("Categoría", 16) +
                          Formato.Col("Exist.", 8) + Formato.Col("Mín.", 6) + Formato.Col("Precio", 12) + "Estado");
        Console.WriteLine("  " + new string('-', 86));
        foreach (Repuesto r in lista)
        {
            Console.WriteLine("  " + Formato.Col(r.Codigo, 9) + Formato.Col(r.Nombre, 26) + Formato.Col(r.Categoria, 16) +
                              Formato.Col(r.Existencia.ToString(), 8) + Formato.Col(r.ExistenciaMinima.ToString(), 6) +
                              Formato.Col(Formato.Dinero(r.PrecioVenta), 12) + r.EstadoInventario());
        }
    }

    // ======================================================
    //  6. ATENCION DE COLAS
    // ======================================================
    private void SubmenuColas()
    {
        int op = 0;
        while (op != 4)
        {
            Titulo("ATENCIÓN DE COLAS");
            Console.WriteLine("  En cola urgente: " + gestorSolicitudes.ColaUrgente.Count +
                              "   |   En cola normal: " + gestorSolicitudes.ColaNormal.Count);
            Console.WriteLine();
            Console.WriteLine("  1. Ver colas");
            Console.WriteLine("  2. Atender siguiente");
            Console.WriteLine("  3. Ver matriz de carga semanal");
            Console.WriteLine("  4. Volver");
            op = LeerEntero("  Seleccione una opción: ", 1, 4);
            if (op == 4) break;

            try
            {
                Console.WriteLine();
                switch (op)
                {
                    case 1: Console.WriteLine(reportes.SolicitudesEnEspera()); break;
                    case 2: AtenderSiguiente(); break;
                    case 3: Console.WriteLine(reportes.MatrizSemanal()); break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    private void AtenderSiguiente()
    {
        int hoy = MatrizCarga.IndiceDia(DateTime.Now);
        SolicitudServicio s = gestorSolicitudes.AtenderSiguiente(gestorTecnicos, matriz, hoy);

        Console.WriteLine("  Se atendió la solicitud " + s.Codigo + " (" + s.Prioridad + ")");
        Console.WriteLine("  Cliente: " + s.Cliente.NombreCompleto + " | Equipo: " + s.Equipo.Descripcion());
        Console.WriteLine("  Técnico asignado: " + s.Tecnico.NombreCompleto + " (" + s.Tecnico.Especialidad +
                          ", activos " + s.Tecnico.TrabajosActivos + "/" + Tecnico.LIMITE_ACTIVOS + ")");
        Console.WriteLine("  Día registrado en la matriz: " + matriz.Dias[hoy]);
        Console.WriteLine("  Estado: " + Formato.NombreEstado(s.Estado));
    }

    // ======================================================
    //  7. ORDENAMIENTOS
    // ======================================================
    private void SubmenuOrdenamientos()
    {
        int op = 0;
        while (op != 5)
        {
            Titulo("ORDENAMIENTOS");
            Console.WriteLine("  1. Bubble Sort    - solicitudes por costo total");
            Console.WriteLine("  2. Quick Sort     - solicitudes por fecha de recepción");
            Console.WriteLine("  3. Selection Sort - técnicos por servicios finalizados");
            Console.WriteLine("  4. Shell Sort     - repuestos por existencia");
            Console.WriteLine("  5. Volver");
            op = LeerEntero("  Seleccione una opción: ", 1, 5);
            if (op == 5) break;

            try
            {
                bool asc = LeerOrden();
                Console.WriteLine();
                switch (op)
                {
                    case 1: Console.WriteLine(reportes.SolicitudesPorCosto(asc)); break;
                    case 2: Console.WriteLine(reportes.SolicitudesPorFecha(asc)); break;
                    case 3: Console.WriteLine(reportes.TecnicosPorFinalizados(asc)); break;
                    case 4: Console.WriteLine(reportes.RepuestosPorExistencia(asc)); break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    // ======================================================
    //  8. BUSQUEDAS
    // ======================================================
    private void SubmenuBusquedas()
    {
        int op = 0;
        while (op != 5)
        {
            Titulo("BÚSQUEDAS");
            Console.WriteLine("  1. Secuencial: clientes por nombre");
            Console.WriteLine("  2. Secuencial: solicitudes por estado");
            Console.WriteLine("  3. Secuencial: equipos por marca");
            Console.WriteLine("  4. Binaria: solicitud por código");
            Console.WriteLine("  5. Volver");
            op = LeerEntero("  Seleccione una opción: ", 1, 5);
            if (op == 5) break;

            try
            {
                switch (op)
                {
                    case 1: BuscarClientes(); break;
                    case 2: BuscarSolicitudesPorEstado(); break;
                    case 3: BuscarEquiposPorMarca(); break;
                    case 4: BuscarSolicitudBinaria(); break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    private void BuscarClientes()
    {
        string txt = LeerTexto("  Texto a buscar en el nombre: ");
        List<Cliente> lista = gestorClientes.BuscarPorNombreParcial(txt);

        Console.WriteLine();
        if (lista.Count == 0) Console.WriteLine("  No hay coincidencias.");
        foreach (Cliente c in lista)
        {
            Console.WriteLine("  " + c.ToString());
        }
        Console.WriteLine("  Coincidencias: " + lista.Count);
    }

    private void BuscarSolicitudesPorEstado()
    {
        EstadoSolicitud est = LeerEstado();
        List<SolicitudServicio> lista = gestorSolicitudes.BuscarPorEstado(est);

        Console.WriteLine();
        if (lista.Count == 0) Console.WriteLine("  No hay solicitudes en estado " + Formato.NombreEstado(est) + ".");
        foreach (SolicitudServicio s in lista)
        {
            Console.WriteLine("  " + s.ToString());
        }
        Console.WriteLine("  Coincidencias: " + lista.Count);
    }

    private void BuscarEquiposPorMarca()
    {
        string marca = LeerTexto("  Marca: ");
        List<Equipo> lista = gestorEquipos.BuscarPorMarca(marca);

        Console.WriteLine();
        if (lista.Count == 0) Console.WriteLine("  No hay equipos de la marca " + marca + ".");
        foreach (Equipo e in lista)
        {
            Console.WriteLine("  " + e.ToString());
        }
        Console.WriteLine("  Coincidencias: " + lista.Count);
    }

    private void BuscarSolicitudBinaria()
    {
        string cod = LeerTexto("  Código de la solicitud (ej. SOL-0003): ");
        SolicitudServicio s = Buscador.BinariaSolicitudPorCodigo(gestorSolicitudes.Solicitudes, cod);

        Console.WriteLine();
        if (s == null)
            Console.WriteLine("  No se encontró la solicitud " + cod + ".");
        else
            Console.WriteLine(s.Comprobante());
        Console.WriteLine("  Comparaciones realizadas: " + Buscador.ComparacionesBinaria +
                          " de " + gestorSolicitudes.Solicitudes.Count + " solicitudes.");
    }

    // ======================================================
    //  9. REPORTES
    // ======================================================
    private void SubmenuReportes()
    {
        int op = 0;
        while (op != 13)
        {
            Titulo("REPORTES");
            Console.WriteLine("   1. Solicitudes en espera por prioridad");
            Console.WriteLine("   2. Solicitudes activas, finalizadas y canceladas");
            Console.WriteLine("   3. Técnicos disponibles y con carga completa");
            Console.WriteLine("   4. Técnico con más servicios finalizados");
            Console.WriteLine("   5. Equipos atendidos por tipo");
            Console.WriteLine("   6. Solicitudes por costo total (Bubble Sort)");
            Console.WriteLine("   7. Solicitudes por fecha (Quick Sort)");
            Console.WriteLine("   8. Repuestos por existencia (Shell Sort)");
            Console.WriteLine("   9. Repuestos en mínimo o agotados");
            Console.WriteLine("  10. Ingresos totales por servicios finalizados");
            Console.WriteLine("  11. Resultados de las tres formas de búsqueda");
            Console.WriteLine("  12. Matriz semanal de carga de trabajo");
            Console.WriteLine("  13. Volver");
            op = LeerEntero("  Seleccione un reporte: ", 1, 13);
            if (op == 13) break;

            try
            {
                string texto = "";
                switch (op)
                {
                    case 1: texto = reportes.SolicitudesEnEspera(); break;
                    case 2: texto = reportes.SolicitudesActivasFinalizadasCanceladas(); break;
                    case 3: texto = reportes.TecnicosDisponiblesYCargaCompleta(); break;
                    case 4: texto = reportes.TecnicoConMasFinalizados(); break;
                    case 5: texto = reportes.EquiposPorTipo(); break;
                    case 6: texto = reportes.SolicitudesPorCosto(LeerOrden()); break;
                    case 7: texto = reportes.SolicitudesPorFecha(LeerOrden()); break;
                    case 8: texto = reportes.RepuestosPorExistencia(LeerOrden()); break;
                    case 9: texto = reportes.RepuestosEnMinimo(); break;
                    case 10: texto = reportes.IngresosTotales(); break;
                    case 11:
                        string nombre = LeerTexto("  Texto para buscar clientes por nombre: ");
                        EstadoSolicitud est = LeerEstado();
                        string marca = LeerTexto("  Marca para buscar equipos: ");
                        string codigo = LeerTexto("  Código para la búsqueda binaria: ");
                        texto = reportes.ResultadosBusquedas(nombre, est, marca, codigo);
                        break;
                    case 12: texto = reportes.MatrizSemanal(); break;
                }
                Console.WriteLine();
                Console.WriteLine(texto);
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
            Pausa();
        }
    }

    // ======================================================
    //  10. DATOS DE PRUEBA
    // ======================================================
    private void OpcionDatosPrueba()
    {
        Titulo("GENERAR DATOS DE PRUEBA");
        Console.WriteLine("  Se crearán clientes con sus equipos y solicitudes. Si no existen,");
        Console.WriteLine("  también se crean 6 técnicos y el inventario de repuestos.");
        Console.WriteLine();
        int cantidad = LeerEntero("  ¿Cuántos clientes desea generar? (1 a 50): ", 1, 50);

        if (!LeerSiNo("  ¿Confirmar? (s/n): ")) return;

        try
        {
            Console.WriteLine();
            Console.WriteLine(generador.GenerarDatosPrueba(cantidad));
        }
        catch (Exception ex)
        {
            MostrarError(ex.Message);
        }
        Pausa();
    }

    // ======================================================
    //  12. MODO GRAFICO
    // ======================================================
    public void ModoGrafico()
    {
        Titulo("MODO GRÁFICO (HTML)");

        // La pagina usa los MISMOS gestores que la consola
        ApiWeb api = new ApiWeb(gestorClientes, gestorTecnicos, gestorEquipos, gestorRepuestos,
                                gestorSolicitudes, matriz, reportes, generador);
        ServidorWeb servidor = new ServidorWeb(api);

        try
        {
            servidor.Iniciar();
        }
        catch (Exception ex)
        {
            MostrarError("No se pudo iniciar el servidor local: " + ex.Message);
            Pausa();
            return;
        }

        Console.WriteLine("  Servidor local iniciado en: " + ServidorWeb.DIRECCION);
        Console.WriteLine("  Archivos de la interfaz:    " + servidor.CarpetaWeb);
        Console.WriteLine();
        Console.WriteLine("  Se abrirá el navegador. Si no se abre, copie la dirección de arriba.");
        Console.WriteLine("  Presione ENTER aquí (o use 'Salir' en la página) para volver al menú.");

        // Solo se abre el navegador si el programa se usa desde una consola normal
        if (!Console.IsInputRedirected)
        {
            servidor.AbrirNavegador();
        }

        // Se espera hasta que el usuario presione ENTER o la pagina pida salir
        while (servidor.Activo)
        {
            if (SePresionoEnter())
            {
                servidor.Detener();
            }
            Thread.Sleep(200);
        }

        Console.WriteLine();
        Console.WriteLine("  Modo gráfico cerrado. Los datos se conservan.");
        Pausa();
    }

    private bool SePresionoEnter()
    {
        try
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(true);
                return tecla.Key == ConsoleKey.Enter;
            }
        }
        catch (InvalidOperationException)
        {
            // Si la consola no tiene teclado (entrada redirigida) solo se puede salir desde la pagina
        }
        return false;
    }

    // ======================================================
    //  LECTURA SEGURA DE ENTRADAS
    // ======================================================

    private string LeerLinea()
    {
        string linea = Console.ReadLine();
        if (linea == null)
        {
            // Solo pasa si se cierra la entrada estandar
            Console.WriteLine();
            Console.WriteLine("  Se cerró la entrada. Fin del programa.");
            Environment.Exit(0);
        }
        return linea;
    }

    // Repite la pregunta hasta que escriban un entero dentro del rango
    public int LeerEntero(string msg, int min, int max)
    {
        while (true)
        {
            Console.Write(msg);
            string entrada = LeerLinea();
            int numero;
            if (int.TryParse(entrada.Trim(), out numero) && numero >= min && numero <= max)
            {
                return numero;
            }
            Console.WriteLine("  Entrada inválida. Escriba un número entre " + min + " y " + max + ".");
        }
    }

    private decimal LeerDecimal(string msg, decimal min)
    {
        while (true)
        {
            Console.Write(msg);
            // Se acepta punto o coma como separador decimal
            string entrada = LeerLinea().Trim().Replace(',', '.');
            decimal numero;
            if (decimal.TryParse(entrada, NumberStyles.Number, CultureInfo.InvariantCulture, out numero) && numero >= min)
            {
                return numero;
            }
            Console.WriteLine("  Entrada inválida. Escriba un número mayor o igual a " + min + ".");
        }
    }

    private double LeerDouble(string msg, double min, double max)
    {
        while (true)
        {
            Console.Write(msg);
            string entrada = LeerLinea().Trim().Replace(',', '.');
            double numero;
            if (double.TryParse(entrada, NumberStyles.Number, CultureInfo.InvariantCulture, out numero) && numero >= min && numero <= max)
            {
                return numero;
            }
            Console.WriteLine("  Entrada inválida. Escriba un número entre " + min + " y " + max + ".");
        }
    }

    // Validacion 2: no aceptar texto vacio
    private string LeerTexto(string msg)
    {
        while (true)
        {
            Console.Write(msg);
            string entrada = LeerLinea();
            if (!string.IsNullOrWhiteSpace(entrada))
            {
                return entrada.Trim();
            }
            Console.WriteLine("  Este dato es obligatorio.");
        }
    }

    private string LeerTextoOpcional(string msg)
    {
        Console.Write(msg);
        return LeerLinea().Trim();
    }

    private bool LeerSiNo(string msg)
    {
        while (true)
        {
            Console.Write(msg);
            string r = LeerLinea().Trim().ToLower();
            if (r == "s" || r == "si" || r == "sí") return true;
            if (r == "n" || r == "no") return false;
            Console.WriteLine("  Responda s o n.");
        }
    }

    // Devuelve true si es ascendente
    private bool LeerOrden()
    {
        Console.WriteLine("  Orden: 1. Ascendente  2. Descendente");
        return LeerEntero("  Opción: ", 1, 2) == 1;
    }

    private TipoCliente LeerTipoCliente()
    {
        Console.WriteLine("  Tipo de cliente: 1. Estudiante  2. Profesional  3. Pequeño negocio  4. Institución");
        int op = LeerEntero("  Opción: ", 1, 4);
        return (TipoCliente)(op - 1);
    }

    private EstadoSolicitud LeerEstado()
    {
        Console.WriteLine("  Estados:");
        for (int i = 0; i <= 7; i++)
        {
            Console.WriteLine("    " + (i + 1) + ". " + Formato.NombreEstado((EstadoSolicitud)i));
        }
        int op = LeerEntero("  Estado: ", 1, 8);
        return (EstadoSolicitud)(op - 1);
    }

    private Cliente PedirCliente()
    {
        string cod = LeerTexto("  Código del cliente (ej. CLI-0001): ");
        Cliente c = gestorClientes.BuscarPorCodigo(cod);
        if (c == null) Console.WriteLine("  No existe el cliente " + cod + ".");
        return c;
    }

    private SolicitudServicio PedirSolicitud()
    {
        string cod = LeerTexto("  Código de la solicitud (ej. SOL-0001): ");
        SolicitudServicio s = gestorSolicitudes.BuscarPorCodigo(cod);
        if (s == null) Console.WriteLine("  No existe la solicitud " + cod + ".");
        return s;
    }

    // ======================================================
    //  PANTALLA
    // ======================================================

    private void Titulo(string texto)
    {
        LimpiarPantalla();
        Console.WriteLine("============================================");
        Console.WriteLine("  XELATECH SOPORTE - " + texto);
        Console.WriteLine("============================================");
    }

    private void LimpiarPantalla()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Si la salida no es una consola real no se puede limpiar; se ignora
        }
    }

    private void MostrarError(string mensaje)
    {
        Console.WriteLine();
        Console.WriteLine("  ERROR: " + mensaje);
    }

    private void Pausa()
    {
        Console.WriteLine();
        Console.Write("  Presione ENTER para continuar...");
        LeerLinea();
    }
}
