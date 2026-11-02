// Pagina de tecnicos: usa GestorTecnicos desde C#

dibujarBarra(false);
cargarTecnicos();

async function cargarTecnicos() {
    const respuesta = await llamarApi("/api/tecnicos/listar");
    if (respuesta.ok) {
        dibujarTabla("tablaTecnicos", respuesta.columnas, respuesta.filas);
    } else {
        mostrarAviso(respuesta.mensaje, true);
    }
}

async function registrarTecnico() {
    const datos = {
        nombre: valor("nombre"),
        especialidad: valor("especialidad"),
        disponible: valor("disponible")
    };

    const respuesta = await llamarApi("/api/tecnicos/registrar", datos);
    if (revisarRespuesta(respuesta)) {
        limpiarCampos(["nombre"]);
        cargarTecnicos();
    }
}

async function consultarTecnico() {
    const respuesta = await llamarApi("/api/tecnicos/consultar", { codigo: valor("codigoConsulta") });
    if (!respuesta.ok) {
        mostrarAviso(respuesta.mensaje, true);
        document.getElementById("ficha").classList.add("oculto");
        return;
    }

    const t = respuesta.tecnico;
    document.getElementById("fCodigo").textContent = t.codigo;
    document.getElementById("fNombre").textContent = t.nombre;
    document.getElementById("fEspecialidad").textContent = t.especialidad;
    document.getElementById("fDisponible").textContent = t.disponible ? "Sí" : "No";
    document.getElementById("fActivos").textContent = t.activos + " de " + t.limite;
    document.getElementById("fFinalizados").textContent = t.finalizados;
    document.getElementById("fPromedio").textContent = t.promedio;

    // Barras con el arreglo de calificaciones: posicion 0 = una estrella ... posicion 4 = cinco
    let mayor = 1;
    for (const cantidad of t.calificaciones) {
        if (cantidad > mayor) mayor = cantidad;
    }

    const contenedor = document.getElementById("barrasCalificaciones");
    contenedor.innerHTML = "";
    for (let i = 4; i >= 0; i--) {
        const porcentaje = (t.calificaciones[i] / mayor) * 100;
        contenedor.insertAdjacentHTML("beforeend",
            '<div class="barra-fila">' +
            '  <span>' + (i + 1) + ' estrella' + (i > 0 ? 's' : '') + '</span>' +
            '  <div class="barra-fondo"><div class="barra-llena" style="width:' + porcentaje + '%"></div></div>' +
            '  <strong>' + t.calificaciones[i] + '</strong>' +
            '</div>');
    }

    document.getElementById("ficha").classList.remove("oculto");
}

async function cambiarDisponibilidad() {
    const codigo = document.getElementById("fCodigo").textContent;
    const respuesta = await llamarApi("/api/tecnicos/disponibilidad", { codigo: codigo });
    if (revisarRespuesta(respuesta)) {
        document.getElementById("codigoConsulta").value = codigo;
        consultarTecnico();
        cargarTecnicos();
    }
}

async function registrarCalificacion() {
    const datos = {
        solicitud: valor("solicitud"),
        calificacion: valor("calificacion")
    };

    const respuesta = await llamarApi("/api/tecnicos/calificar", datos);
    if (revisarRespuesta(respuesta)) {
        limpiarCampos(["solicitud"]);
        cargarTecnicos();
    }
}
