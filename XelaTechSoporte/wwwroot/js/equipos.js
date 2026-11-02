// Pagina de equipos: usa GestorEquipos desde C#

dibujarBarra(false);
cargarClientes();
cargarEquipos();

async function cargarClientes() {
    const respuesta = await llamarApi("/api/clientes/listar");
    if (respuesta.ok) {
        llenarSelect("propietario", respuesta.filas, "Seleccione un cliente", 1);
    }
}

async function cargarEquipos() {
    const respuesta = await llamarApi("/api/equipos/listar");
    if (respuesta.ok) {
        dibujarTabla("tablaEquipos", respuesta.columnas, respuesta.filas);
    } else {
        mostrarAviso(respuesta.mensaje, true);
    }
}

// Muestra solo los campos del tipo elegido
function elegirTipo(boton) {
    elegirOpcion(boton);
    const tipo = boton.dataset.valor;
    document.getElementById("datosLaptop").classList.toggle("oculto", tipo !== "laptop");
    document.getElementById("datosEscritorio").classList.toggle("oculto", tipo !== "escritorio");
    document.getElementById("datosImpresora").classList.toggle("oculto", tipo !== "impresora");
}

async function registrarEquipo() {
    const datos = {
        tipo: opcionElegida("grupoTipo"),
        propietario: valor("propietario"),
        serie: valor("serie"),
        marca: valor("marca"),
        modelo: valor("modelo"),
        estadoFisico: valor("estadoFisico"),
        accesorios: valor("accesorios"),
        pulgadas: valor("pulgadas"),
        bateria: valor("bateria"),
        gabinete: valor("gabinete"),
        fuenteExterna: valor("fuenteExterna"),
        tecnologia: valor("tecnologia"),
        tinta: valor("tinta")
    };

    const respuesta = await llamarApi("/api/equipos/registrar", datos);
    if (revisarRespuesta(respuesta)) {
        limpiarCampos(["serie", "marca", "modelo", "estadoFisico", "accesorios"]);
        cargarEquipos();
    }
}
