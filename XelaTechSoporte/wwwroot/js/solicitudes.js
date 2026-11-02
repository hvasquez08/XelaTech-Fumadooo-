// Pagina de solicitudes: usa GestorSolicitudes y SolicitudServicio desde C#

dibujarBarra(false);

// Codigo de la solicitud que esta abierta en el detalle
let solicitudAbierta = "";

cargarClientes();
cargarRepuestos();
cargarSolicitudes();

async function cargarClientes() {
    const respuesta = await llamarApi("/api/clientes/listar");
    if (respuesta.ok) {
        llenarSelect("cliente", respuesta.filas, "Seleccione un cliente", 1);
    }
}

async function cargarEquiposDelCliente() {
    const cliente = valor("cliente");
    if (cliente === "") {
        llenarSelect("equipo", [], "Primero seleccione un cliente", 1);
        return;
    }

    const respuesta = await llamarApi("/api/equipos/delcliente", { cliente: cliente });
    if (respuesta.ok) {
        // Columnas: 0 Código, 1 Tipo, 2 Marca, 3 Modelo ...  Se arma un texto "Laptop HP Pavilion"
        const filas = [];
        for (const f of respuesta.filas) {
            filas.push([f[0], f[1] + " " + f[2] + " " + f[3]]);
        }
        const texto = filas.length === 0 ? "El cliente no tiene equipos (regístrelo en Equipos)" : "Seleccione un equipo";
        llenarSelect("equipo", filas, texto, 1);
    }
}

async function cargarRepuestos() {
    const respuesta = await llamarApi("/api/repuestos/listar");
    if (respuesta.ok) {
        // Se muestra el nombre y la existencia
        const filas = [];
        for (const f of respuesta.filas) {
            filas.push([f[0], f[1] + " (hay " + f[3] + ", " + f[6] + ")"]);
        }
        llenarSelect("repuesto", filas, "Seleccione un repuesto", 1);
    }
}

async function cargarSolicitudes() {
    const respuesta = await llamarApi("/api/solicitudes/listar");
    if (respuesta.ok) {
        dibujarTabla("tablaSolicitudes", respuesta.columnas, respuesta.filas);
    } else {
        mostrarAviso(respuesta.mensaje, true);
    }
}

async function crearSolicitud() {
    const datos = {
        cliente: valor("cliente"),
        equipo: valor("equipo"),
        descripcion: valor("descripcion"),
        prioridad: opcionElegida("grupoPrioridad")
    };

    const respuesta = await llamarApi("/api/solicitudes/crear", datos);
    if (revisarRespuesta(respuesta)) {
        limpiarCampos(["descripcion"]);
        cargarSolicitudes();
    }
}

// ---------- Detalle de una solicitud ----------

async function abrirSolicitud() {
    solicitudAbierta = valor("codigo").toUpperCase();
    await mostrarDetalle(true);
}

async function mostrarDetalle(avisarError) {
    const respuesta = await llamarApi("/api/solicitudes/consultar", { codigo: solicitudAbierta });
    if (!respuesta.ok) {
        if (avisarError) mostrarAviso(respuesta.mensaje, true);
        document.getElementById("detalle").classList.add("oculto");
        return;
    }

    const s = respuesta.solicitud;
    document.getElementById("tituloDetalle").textContent = "Solicitud " + s.codigo;
    document.getElementById("dEstado").innerHTML = "";
    document.getElementById("dEstado").appendChild(crearEtiqueta(s.estado));
    document.getElementById("dPrioridad").innerHTML = "";
    document.getElementById("dPrioridad").appendChild(crearEtiqueta(s.prioridad));
    document.getElementById("dCliente").textContent = s.cliente;
    document.getElementById("dEquipo").textContent = s.equipo;
    document.getElementById("dProblema").textContent = s.problema;
    document.getElementById("dFecha").textContent = s.fecha;
    document.getElementById("dTecnico").textContent = s.tecnico;
    document.getElementById("dDiagnostico").textContent = s.diagnostico;
    document.getElementById("dHoras").textContent = s.horas > 0 ? s.horas + " h" : "Pendiente";
    document.getElementById("dManoObra").textContent = s.manoObra;
    document.getElementById("dRepuestos").textContent = s.costoRepuestos;
    document.getElementById("dTotal").textContent = s.total;
    document.getElementById("sugerencia").textContent = "Revisión sugerida: " + s.sugerencia;
    document.getElementById("permitidos").textContent = "Siguiente estado permitido: " + s.permitidos + ".";

    dibujarTabla("tablaRepuestosUsados", respuesta.repuestos.columnas, respuesta.repuestos.filas);
    dibujarPila(s.estado, respuesta.historial);

    document.getElementById("detalle").classList.remove("oculto");
}

function crearEtiqueta(texto) {
    const span = document.createElement("span");
    span.className = "etiqueta " + claseEtiqueta(texto);
    span.textContent = texto;
    return span;
}

// Dibuja la pila: arriba el estado actual y debajo los estados guardados (el primero es el tope)
function dibujarPila(estadoActual, historial) {
    const pila = document.getElementById("pila");
    pila.innerHTML = "";

    const actual = document.createElement("div");
    actual.className = "caja tope";
    actual.textContent = "Actual: " + estadoActual;
    pila.appendChild(actual);

    if (historial.length === 0) {
        const vacia = document.createElement("div");
        vacia.className = "caja";
        vacia.textContent = "(pila vacía)";
        pila.appendChild(vacia);
    }

    for (let i = 0; i < historial.length; i++) {
        const caja = document.createElement("div");
        caja.className = "caja";
        caja.textContent = (i === 0 ? "Tope → " : "") + historial[i];
        pila.appendChild(caja);
    }
}

// Despues de cada accion se refresca el detalle, la lista y los repuestos
async function refrescar() {
    await mostrarDetalle(false);
    cargarSolicitudes();
    cargarRepuestos();
}

async function registrarDiagnostico() {
    const respuesta = await llamarApi("/api/solicitudes/diagnostico", {
        codigo: solicitudAbierta,
        texto: valor("textoDiagnostico"),
        horas: valor("horas")
    });
    if (revisarRespuesta(respuesta)) {
        limpiarCampos(["textoDiagnostico"]);
        refrescar();
    }
}

async function agregarRepuesto() {
    const respuesta = await llamarApi("/api/solicitudes/repuesto", {
        codigo: solicitudAbierta,
        repuesto: valor("repuesto"),
        cantidad: valor("cantidadRepuesto")
    });
    if (revisarRespuesta(respuesta)) {
        refrescar();
    }
}

async function cambiarEstado() {
    const respuesta = await llamarApi("/api/solicitudes/estado", {
        codigo: solicitudAbierta,
        estado: valor("nuevoEstado")
    });
    if (revisarRespuesta(respuesta)) {
        refrescar();
    }
}

async function deshacer() {
    const respuesta = await llamarApi("/api/solicitudes/deshacer", { codigo: solicitudAbierta });
    if (revisarRespuesta(respuesta)) {
        refrescar();
    }
}

async function entregar() {
    const respuesta = await llamarApi("/api/solicitudes/entregar", {
        codigo: solicitudAbierta,
        calificacion: valor("calificacionEntrega")
    });
    if (revisarRespuesta(respuesta)) {
        refrescar();
    }
}

async function cancelar() {
    if (!confirm("¿Seguro que desea cancelar la solicitud " + solicitudAbierta + "?")) return;

    const respuesta = await llamarApi("/api/solicitudes/cancelar", { codigo: solicitudAbierta });
    if (revisarRespuesta(respuesta)) {
        refrescar();
    }
}
