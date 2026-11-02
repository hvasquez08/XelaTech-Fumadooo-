// Pagina de clientes: usa GestorClientes desde C#

dibujarBarra(false);
cargarClientes();

async function cargarClientes() {
    const respuesta = await llamarApi("/api/clientes/listar");
    if (respuesta.ok) {
        dibujarTabla("tablaClientes", respuesta.columnas, respuesta.filas);
    } else {
        mostrarAviso(respuesta.mensaje, true);
    }
}

async function registrarCliente() {
    const datos = {
        nombre: valor("nombre"),
        telefono: valor("telefono"),
        correo: valor("correo"),
        tipo: valor("tipo")
    };

    const respuesta = await llamarApi("/api/clientes/registrar", datos);
    if (revisarRespuesta(respuesta)) {
        limpiarCampos(["nombre", "telefono", "correo"]);
        cargarClientes();
    }
}

async function consultarCliente() {
    const respuesta = await llamarApi("/api/clientes/consultar", { codigo: valor("codigoConsulta") });
    if (!respuesta.ok) {
        mostrarAviso(respuesta.mensaje, true);
        document.getElementById("ficha").classList.add("oculto");
        document.getElementById("detalleCliente").classList.add("oculto");
        return;
    }

    const c = respuesta.cliente;
    document.getElementById("fCodigo").textContent = c.codigo;
    document.getElementById("fNombre").textContent = c.nombre;
    document.getElementById("fTelefono").textContent = c.telefono;
    document.getElementById("fCorreo").textContent = c.correo;
    document.getElementById("fTipo").textContent = c.tipo;
    document.getElementById("fSolicitudes").textContent = c.solicitudes;
    document.getElementById("ficha").classList.remove("oculto");

    dibujarTabla("tablaEquiposCliente", respuesta.equipos.columnas, respuesta.equipos.filas);
    dibujarTabla("tablaSolicitudesCliente", respuesta.listaSolicitudes.columnas, respuesta.listaSolicitudes.filas);
    document.getElementById("detalleCliente").classList.remove("oculto");
}

async function actualizarCliente() {
    const datos = {
        codigo: document.getElementById("fCodigo").textContent,
        telefono: valor("nuevoTelefono"),
        correo: valor("nuevoCorreo")
    };

    const respuesta = await llamarApi("/api/clientes/actualizar", datos);
    if (revisarRespuesta(respuesta)) {
        limpiarCampos(["nuevoTelefono", "nuevoCorreo"]);
        document.getElementById("codigoConsulta").value = datos.codigo;
        consultarCliente();
        cargarClientes();
    }
}
