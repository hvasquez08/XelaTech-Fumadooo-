namespace XelaTechSoporte;

// Genera datos de prueba con Random.
// Todo se crea con los mismos constructores y metodos de los gestores,
// asi los datos pasan por las mismas validaciones que los que escribe el usuario.
public class GeneradorDatos
{
    private Random random = new Random();

    private GestorClientes gestorClientes;
    private GestorTecnicos gestorTecnicos;
    private GestorEquipos gestorEquipos;
    private GestorRepuestos gestorRepuestos;
    private GestorSolicitudes gestorSolicitudes;
    private MatrizCarga matriz;

    // Datos fijos para armar los registros al azar
    private string[] nombres = { "José", "María", "Carlos", "Ana", "Luis", "Sofía", "Pedro", "Lucía",
                                 "Jorge", "Andrea", "Diego", "Gabriela", "Mario", "Fernanda", "Kevin", "Paola" };
    private string[] apellidos = { "López", "García", "Pérez", "Xicará", "Cux", "Tzul", "Ajanel", "Racancoj",
                                   "Méndez", "Vásquez", "Coyoy", "Sac", "Ramírez", "Chojolán", "Zapeta", "Orozco" };
    private string[] dominios = { "gmail.com", "hotmail.com", "outlook.com", "url.edu.gt" };

    // Marca y modelo van en la misma posicion de cada arreglo
    private string[] marcasLaptop = { "HP", "Dell", "Lenovo", "Asus", "Acer" };
    private string[] modelosLaptop = { "Pavilion 15", "Inspiron 3520", "IdeaPad 3", "VivoBook 15", "Aspire 5" };
    private string[] marcasEscritorio = { "Dell", "HP", "Lenovo", "Ensamblada" };
    private string[] modelosEscritorio = { "OptiPlex 3080", "ProDesk 400", "ThinkCentre M70", "Ryzen 5 Gamer" };
    private string[] marcasImpresora = { "Epson", "Canon", "HP", "Brother" };
    private string[] modelosImpresora = { "L3250", "PIXMA G3110", "LaserJet M111w", "DCP-T520W" };
    private string[] estadosFisicos = { "Bueno", "Regular", "Con golpes", "Rayones leves", "Pantalla rayada" };
    private string[] accesorios = { "Cargador", "Ninguno", "Cargador y funda", "Cable de poder", "Cable USB" };

    private string[] problemasComputadora = { "No enciende", "Se calienta mucho", "Pantalla azul al iniciar",
                                              "Está muy lenta", "No carga la batería", "Hace ruido el ventilador" };
    private string[] problemasImpresora = { "No jala el papel", "Imprime manchado", "Atasco de papel",
                                            "No la reconoce la computadora", "Error de cartucho" };
    private string[] diagnosticosComputadora = { "Limpieza interna y cambio de pasta térmica", "Falla en el disco, se reemplaza",
                                                 "Se reinstala el sistema operativo", "Cambio de pieza dañada",
                                                 "Mantenimiento preventivo completo" };
    private string[] diagnosticosImpresora = { "Limpieza de cabezal y rodillos", "Cambio de rodillo de arrastre",
                                               "Cartucho dañado, se reemplaza", "Mantenimiento preventivo completo" };

    public GeneradorDatos(GestorClientes gc, GestorTecnicos gt, GestorEquipos ge, GestorRepuestos gr,
                          GestorSolicitudes gs, MatrizCarga m)
    {
        gestorClientes = gc;
        gestorTecnicos = gt;
        gestorEquipos = ge;
        gestorRepuestos = gr;
        gestorSolicitudes = gs;
        matriz = m;
    }

    // Genera "cantidadClientes" clientes, con sus equipos y solicitudes.
    // Devuelve un resumen de lo que se creo.
    public string GenerarDatosPrueba(int cantidadClientes)
    {
        if (cantidadClientes < 1 || cantidadClientes > 50)
            throw new Exception("La cantidad de clientes debe estar entre 1 y 50.");

        int repuestosNuevos = CrearRepuestos();
        int tecnicosNuevos = CrearTecnicos();

        int equiposNuevos = 0;
        List<SolicitudServicio> nuevas = new List<SolicitudServicio>();

        // Las fechas van subiendo para que el orden de llegada sea realista
        DateTime fecha = DateTime.Now.AddDays(-14);

        for (int i = 0; i < cantidadClientes; i++)
        {
            Cliente c = CrearCliente();
            gestorClientes.Registrar(c);

            // Cada cliente trae 1 o 2 equipos
            int cantEquipos = random.Next(1, 3);
            for (int k = 0; k < cantEquipos; k++)
            {
                Equipo e = CrearEquipo(c);
                gestorEquipos.Registrar(e);
                equiposNuevos++;

                // El 85 % de los equipos llega con una solicitud
                if (random.Next(100) < 85)
                {
                    Prioridad prio = random.Next(100) < 30 ? Prioridad.Urgente : Prioridad.Normal;
                    string problema;
                    if (e is Impresora)
                        problema = problemasImpresora[random.Next(problemasImpresora.Length)];
                    else
                        problema = problemasComputadora[random.Next(problemasComputadora.Length)];

                    fecha = fecha.AddMinutes(random.Next(60, 600));
                    SolicitudServicio s = gestorSolicitudes.CrearSolicitud(c, e, problema, prio, fecha);
                    nuevas.Add(s);
                }
            }
        }

        // Se atienden las colas y cada solicitud avanza al azar
        int atendidas = 0;
        int vueltas = 0;
        while (vueltas < 500)
        {
            vueltas++;
            SolicitudServicio s;
            try
            {
                s = gestorSolicitudes.AtenderSiguiente(gestorTecnicos, matriz, random.Next(0, 7));
            }
            catch (Exception)
            {
                break; // colas vacias o no hay tecnico disponible: el resto queda en espera
            }
            atendidas++;
            AvanzarAlAzar(s);
        }

        // Conteo de como quedaron las solicitudes nuevas
        int entregadas = 0, canceladas = 0, enEspera = 0, enProceso = 0;
        foreach (SolicitudServicio s in nuevas)
        {
            if (s.Estado == EstadoSolicitud.Entregada) entregadas++;
            else if (s.Estado == EstadoSolicitud.Cancelada) canceladas++;
            else if (s.Estado == EstadoSolicitud.EnEspera) enEspera++;
            else enProceso++;
        }

        string texto = "Datos de prueba generados:\n";
        texto += "  Repuestos nuevos:  " + repuestosNuevos + "\n";
        texto += "  Técnicos nuevos:   " + tecnicosNuevos + "\n";
        texto += "  Clientes nuevos:   " + cantidadClientes + "\n";
        texto += "  Equipos nuevos:    " + equiposNuevos + "\n";
        texto += "  Solicitudes nuevas: " + nuevas.Count + " (entregadas: " + entregadas + ", canceladas: " + canceladas +
                 ", en proceso: " + enProceso + ", en espera: " + enEspera + ")\n";
        return texto;
    }

    // ===== Repuestos =====
    private int CrearRepuestos()
    {
        // Solo se crean si el inventario esta vacio (para no repetirlos)
        if (gestorRepuestos.Repuestos.Count > 0) return 0;

        string[] nombresRep = { "Disco SSD 480 GB", "Memoria RAM 8 GB", "Batería para laptop", "Teclado para laptop",
                                "Pantalla 15.6 pulgadas", "Fuente de poder 500 W", "Pasta térmica", "Ventilador de CPU",
                                "Cabezal de impresión", "Rodillo de arrastre", "Cartucho de tinta negra", "Tóner láser" };
        string[] categorias = { "Almacenamiento", "Memoria", "Energía", "Periféricos",
                                "Pantallas", "Energía", "Mantenimiento", "Enfriamiento",
                                "Impresoras", "Impresoras", "Consumibles", "Consumibles" };
        decimal[] costos = { 250, 180, 300, 150, 450, 220, 25, 60, 280, 75, 90, 350 };

        for (int i = 0; i < nombresRep.Length; i++)
        {
            int existencia = random.Next(0, 16);
            int minimo = random.Next(2, 6);
            // Precio de venta = costo + 30 % (asi nunca es menor que el costo)
            decimal precio = Math.Round(costos[i] * 1.30m, 2);

            Repuesto r = new Repuesto(gestorRepuestos.GenerarCodigo(), nombresRep[i], categorias[i],
                                      existencia, minimo, costos[i], precio);
            gestorRepuestos.Registrar(r);
        }
        return nombresRep.Length;
    }

    // ===== Tecnicos =====
    private int CrearTecnicos()
    {
        // Se asegura que existan al menos 6 tecnicos de distintas especialidades
        Especialidad[] especialidades = { Especialidad.Computadoras, Especialidad.Computadoras, Especialidad.Impresoras,
                                          Especialidad.General, Especialidad.Computadoras, Especialidad.Impresoras };
        int creados = 0;
        while (gestorTecnicos.Tecnicos.Count < 6)
        {
            int pos = gestorTecnicos.Tecnicos.Count;
            // El sexto tecnico queda "no disponible" para probar esa validacion
            bool disponible = pos != 5;
            string nombre = NombreAlAzar();
            Tecnico t = new Tecnico(gestorTecnicos.GenerarCodigo(), nombre, especialidades[pos], disponible);
            gestorTecnicos.Registrar(t);
            creados++;
        }
        return creados;
    }

    // ===== Clientes =====
    private Cliente CrearCliente()
    {
        string nombre = nombres[random.Next(nombres.Length)];
        string apellido1 = apellidos[random.Next(apellidos.Length)];
        string apellido2 = apellidos[random.Next(apellidos.Length)];
        string nombreCompleto = nombre + " " + apellido1 + " " + apellido2;

        // Telefono de 8 digitos que empieza con 3, 4 o 5
        string telefono = random.Next(3, 6).ToString() + random.Next(1000000, 10000000).ToString();

        string correo = QuitarTildes(nombre.ToLower()) + "." + QuitarTildes(apellido1.ToLower()) +
                        random.Next(10, 100) + "@" + dominios[random.Next(dominios.Length)];

        TipoCliente tipo = (TipoCliente)random.Next(0, 4);
        return new Cliente(gestorClientes.GenerarCodigo(), nombreCompleto, telefono, correo, tipo);
    }

    private string NombreAlAzar()
    {
        return nombres[random.Next(nombres.Length)] + " " + apellidos[random.Next(apellidos.Length)];
    }

    private string QuitarTildes(string texto)
    {
        return texto.Replace("á", "a").Replace("é", "e").Replace("í", "i")
                    .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
    }

    // ===== Equipos =====
    private Equipo CrearEquipo(Cliente dueno)
    {
        string codigo = gestorEquipos.GenerarCodigo();
        string estado = estadosFisicos[random.Next(estadosFisicos.Length)];
        string acc = accesorios[random.Next(accesorios.Length)];
        int tipo = random.Next(0, 3);

        if (tipo == 0)
        {
            double[] tamanos = { 13.3, 14, 15.6, 17.3 };
            string[] baterias = { "Buena", "Regular", "Mala", "No retiene carga" };
            int m = random.Next(marcasLaptop.Length);
            return new Laptop(codigo, GenerarCodigoUnico("LP"), marcasLaptop[m], modelosLaptop[m], dueno, estado, acc,
                              tamanos[random.Next(tamanos.Length)], baterias[random.Next(baterias.Length)]);
        }
        else if (tipo == 1)
        {
            string[] gabinetes = { "Torre", "Mini torre", "Todo en uno" };
            int m = random.Next(marcasEscritorio.Length);
            return new Escritorio(codigo, GenerarCodigoUnico("PC"), marcasEscritorio[m], modelosEscritorio[m], dueno, estado, acc,
                                  gabinetes[random.Next(gabinetes.Length)], random.Next(2) == 0);
        }
        else
        {
            string[] tecnologias = { "Tinta", "Láser", "Matricial" };
            int m = random.Next(marcasImpresora.Length);
            return new Impresora(codigo, GenerarCodigoUnico("IM"), marcasImpresora[m], modelosImpresora[m], dueno, estado, acc,
                                 tecnologias[random.Next(tecnologias.Length)], random.Next(0, 101));
        }
    }

    // Numero de serie al azar que no exista todavia (prefijo + 6 digitos)
    private string GenerarCodigoUnico(string prefijo)
    {
        string serie = prefijo + "-" + random.Next(100000, 1000000);
        while (gestorEquipos.ExisteSerie(serie))
        {
            serie = prefijo + "-" + random.Next(100000, 1000000);
        }
        return serie;
    }

    // ===== Avance de una solicitud =====
    // Despues de asignar el tecnico, la solicitud avanza un numero de pasos al azar.
    private void AvanzarAlAzar(SolicitudServicio s)
    {
        int suerte = random.Next(100);

        if (suerte < 12) return; // se queda Asignada

        // Diagnostico (segun el tipo de equipo)
        bool esImpresora = s.Equipo is Impresora;
        string diagnostico;
        if (esImpresora)
            diagnostico = diagnosticosImpresora[random.Next(diagnosticosImpresora.Length)];
        else
            diagnostico = diagnosticosComputadora[random.Next(diagnosticosComputadora.Length)];
        s.RegistrarDiagnostico(diagnostico, random.Next(1, 9));
        if (suerte < 22) return; // se queda En diagnostico

        // Repuestos (0, 1 o 2 intentos), solo si son del tipo de equipo y hay existencia
        int cantRepuestos = random.Next(0, 3);
        for (int i = 0; i < cantRepuestos; i++)
        {
            Repuesto r = gestorRepuestos.Repuestos[random.Next(gestorRepuestos.Repuestos.Count)];
            bool esDeImpresora = r.Categoria == "Impresoras" || r.Categoria == "Consumibles";
            int cant = random.Next(1, 3);
            if (esDeImpresora == esImpresora && r.HayExistencia(cant))
            {
                s.AgregarRepuesto(r, cant);
            }
        }

        // Un 10 % se cancela a medio proceso
        if (suerte >= 90)
        {
            s.Cancelar();
            return;
        }

        s.CambiarEstado(EstadoSolicitud.EnReparacion);
        if (suerte < 35) return;

        s.CambiarEstado(EstadoSolicitud.ListaParaEntregar);
        if (suerte < 45) return;

        // Se entrega con una calificacion de 3 a 5 (a veces sin calificar)
        int calificacion = random.Next(100) < 85 ? random.Next(3, 6) : 0;
        s.Entregar(calificacion);
    }
}
