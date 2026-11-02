// Pagina de atencion de colas: usa las dos Queue de GestorSolicitudes y la MatrizCarga

dibujarBarra(false);
cargarColas();
cargarMatriz();

async function cargarColas() {
    const respuesta = await llamarApi("/api/colas/ver");
    if (!respuesta.ok) {
        mostrarAviso(respuesta.mensaje, true);
        return;
    }
    document.getElementById("numUrgente").textContent = respuesta.urgente.filas.length;
    document.getElementById("numNormal").textContent = respuesta.normal.filas.length;
    dibujarTabla("tablaUrgente", respuesta.urgente.columnas, respuesta.urgente.filas);
    dibujarTabla("tablaNormal", respuesta.normal.columnas, respuesta.normal.filas);
}

async function atenderSiguiente() {
    const respuesta = await llamarApi("/api/colas/atender");
    revisarRespuesta(respuesta);
    cargarColas();
    cargarMatriz();
}

// Dibuja la matriz int[,] como tabla. Mientras mas carga, mas fuerte el color de la celda.
async function cargarMatriz() {
    const respuesta = await llamarApi("/api/colas/matriz");
    if (!respuesta.ok) return;

    // Valor mas alto para calcular la intensidad del color
    let maximo = 1;
    for (const fila of respuesta.filas) {
        for (const v of fila.valores) {
            if (v > maximo) maximo = v;
        }
    }

    let html = "<thead><tr><th>Técnico</th>";
    for (const dia of respuesta.dias) {
        html += "<th>" + dia + "</th>";
    }
    html += "<th>Total</th></tr></thead><tbody>";

    for (const fila of respuesta.filas) {
        html += "<tr><td>" + escaparHtml(fila.tecnico) + "</td>";
        for (const v of fila.valores) {
            const intensidad = v === 0 ? 0 : 0.15 + (v / maximo) * 0.6;
            html += '<td class="celda" style="background: rgba(244, 183, 47, ' + intensidad + ')">' + v + "</td>";
        }
        html += '<td class="celda">' + fila.total + "</td></tr>";
    }
    html += "</tbody><tfoot><tr><td>Total por día</td>";
    for (const t of respuesta.totalesDia) {
        html += "<td>" + t + "</td>";
    }
    html += "<td></td></tr></tfoot>";

    document.getElementById("matriz").innerHTML = html;
    document.getElementById("mayorCarga").textContent = respuesta.mayor;
}

// Evita que un nombre con simbolos raros rompa la tabla
function escaparHtml(texto) {
    const div = document.createElement("div");
    div.textContent = texto;
    return div.innerHTML;
}
