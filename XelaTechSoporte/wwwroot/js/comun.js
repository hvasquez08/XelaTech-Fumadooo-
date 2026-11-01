// =========================================================
//  Funciones que usan todas las paginas del modo grafico
// =========================================================

// Envia datos al programa de C# y devuelve la respuesta (JSON).
// El servidor espera que todos los valores vengan como texto.
async function llamarApi(ruta, datos) {
    const datosTexto = {};
    if (datos) {
        for (const clave in datos) {
            datosTexto[clave] = String(datos[clave]);
        }
    }

    try {
        const respuesta = await fetch(ruta, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(datosTexto)
        });
        return await respuesta.json();
    } catch (error) {
        return {
            ok: false,
            mensaje: "No hay conexión con el programa de C#. Abra el modo gráfico desde la consola (opción 12)."
        };
    }
}

// Lee el valor de un campo por su id
function valor(id) {
    return document.getElementById(id).value.trim();
}

// Deja vacios los campos indicados
function limpiarCampos(ids) {
    for (const id of ids) {
        document.getElementById(id).value = "";
    }
}

// ---------- Aviso flotante (verde = bien, rojo = error) ----------
let temporizadorAviso = null;

function mostrarAviso(texto, esError) {
    const aviso = document.getElementById("aviso");
    aviso.textContent = texto;
    aviso.className = "aviso visible " + (esError ? "error" : "ok");

    clearTimeout(temporizadorAviso);
    temporizadorAviso = setTimeout(function () {
        aviso.className = "aviso";
    }, esError ? 7000 : 4500);
}

// Muestra el mensaje de la respuesta y devuelve true si todo salio bien
function revisarRespuesta(respuesta) {
    if (respuesta.mensaje) {
        mostrarAviso(respuesta.mensaje, !respuesta.ok);
    }
    return respuesta.ok;
}

// ---------- Tablas ----------

// Clase de color segun el texto del estado
function claseEtiqueta(texto) {
    switch (texto) {
        case "Recibida":
        case "En espera":
        case "EN MÍNIMO":
            return "e-espera";
        case "Asignada":
            return "e-asignada";
        case "En diagnóstico":
            return "e-diagnostico";
        case "En reparación":
            return "e-reparacion";
        case "Lista para entregar":
        case "Normal":
        case "Sí":
            return "e-lista";
        case "Entregada":
            return "e-entregada";
        case "Cancelada":
        case "Urgente":
        case "AGOTADO":
        case "No":
            return "e-cancelada";
        default:
            return "e-normal";
    }
}

// Dibuja una tabla dentro del contenedor.
// columnas = ["Código", "Nombre", ...]   filas = [["CLI-0001", "Ana"], ...]
function dibujarTabla(idContenedor, columnas, filas) {
    const contenedor = document.getElementById(idContenedor);
    contenedor.innerHTML = "";

    if (!filas || filas.length === 0) {
        const p = document.createElement("p");
        p.className = "vacio";
        p.textContent = "No hay datos para mostrar.";
        contenedor.appendChild(p);
        return;
    }

    // Estas columnas se muestran como etiqueta de color
    const columnasConEtiqueta = ["Estado", "Prioridad", "Disponible"];

    const tabla = document.createElement("table");
    const thead = document.createElement("thead");
    const filaTitulos = document.createElement("tr");
    for (const titulo of columnas) {
        const th = document.createElement("th");
        th.textContent = titulo;
        filaTitulos.appendChild(th);
    }
    thead.appendChild(filaTitulos);
    tabla.appendChild(thead);

    const tbody = document.createElement("tbody");
    for (const fila of filas) {
        const tr = document.createElement("tr");
        for (let i = 0; i < fila.length; i++) {
            const td = document.createElement("td");
            if (columnasConEtiqueta.includes(columnas[i])) {
                const span = document.createElement("span");
                span.className = "etiqueta " + claseEtiqueta(fila[i]);
                span.textContent = fila[i];
                td.appendChild(span);
            } else {
                td.textContent = fila[i];
            }
            tr.appendChild(td);
        }
        tbody.appendChild(tr);
    }
    tabla.appendChild(tbody);

    const envoltura = document.createElement("div");
    envoltura.className = "tabla-contenedor";
    envoltura.appendChild(tabla);
    contenedor.appendChild(envoltura);
}

// Llena un <select> con las filas de una tabla.
// El valor es la primera columna (el codigo) y el texto "codigo - columna indicada".
function llenarSelect(idSelect, filas, textoInicial, columnaTexto) {
    const select = document.getElementById(idSelect);
    select.innerHTML = "";

    const primera = document.createElement("option");
    primera.value = "";
    primera.textContent = textoInicial;
    select.appendChild(primera);

    for (const fila of filas) {
        const opcion = document.createElement("option");
        opcion.value = fila[0];
        opcion.textContent = fila[0] + " - " + fila[columnaTexto];
        select.appendChild(opcion);
    }
}

// Para los grupos de botones de opcion (tipo de equipo, prioridad, orden...)
function elegirOpcion(boton) {
    const grupo = boton.parentElement;
    for (const b of grupo.children) {
        b.classList.remove("activo");
    }
    boton.classList.add("activo");
}

function opcionElegida(idGrupo) {
    const activo = document.querySelector("#" + idGrupo + " .activo");
    return activo ? activo.dataset.valor : "";
}

// =========================================================
//  Barra superior (logo + menu del usuario)
//  Se arma con JavaScript para no repetirla en cada pagina.
// =========================================================
function dibujarBarra(esPortalCliente) {
    const barra = document.getElementById("barra");
    barra.className = "barra";

    let opciones;
    if (esPortalCliente) {
        opciones =
            '<div class="rol">Sesión: Cliente</div>' +
            '<a href="index.html">Panel del administrador</a>';
    } else {
        opciones =
            '<div class="rol">Sesión: Administrador</div>' +
            '<a href="index.html">Panel principal</a>' +
            '<a href="cliente.html">Portal del cliente</a>' +
            '<hr>' +
            '<button onclick="abrirDatosPrueba()">Generar datos de prueba</button>' +
            '<button class="salir" onclick="salirModoGrafico()">Salir del modo gráfico</button>';
    }

    barra.innerHTML =
        '<a class="logo" href="index.html">' +
        '  <img src="img/logo.svg" alt="">' +
        '  <div><span class="nombre">XelaTech</span><span class="sub">Soporte</span></div>' +
        '</a>' +
        '<div class="usuario">' +
        '  <button class="usuario-boton" onclick="alternarMenu(event)">' +
        '    <img src="img/usuario.svg" alt=""> ' + (esPortalCliente ? "Cliente" : "Usuario") +
        '    <span class="flecha">▼</span>' +
        '  </button>' +
        '  <div class="usuario-menu" id="menuUsuario">' + opciones + '</div>' +
        '</div>';

    // Aviso flotante y ventana de datos de prueba (una sola vez por pagina)
    document.body.insertAdjacentHTML("beforeend",
        '<div id="aviso" class="aviso"></div>' +
        '<div class="modal-fondo" id="modalDatos">' +
        '  <div class="modal">' +
        '    <h2>Generar datos de prueba</h2>' +
        '    <p class="ayuda">Se crean clientes con equipos y solicitudes usando Random. ' +
        '       Si no existen, también se crean 6 técnicos y el inventario de repuestos.</p>' +
        '    <label for="cantidadDatos">Cantidad de clientes (1 a 50)</label>' +
        '    <input type="number" id="cantidadDatos" min="1" max="50" value="10">' +
        '    <pre id="resultadoDatos" class="oculto"></pre>' +
        '    <div class="botones">' +
        '      <button class="boton" onclick="generarDatos()">Generar</button>' +
        '      <button class="boton secundario" onclick="cerrarDatosPrueba()">Cerrar</button>' +
        '    </div>' +
        '  </div>' +
        '</div>');
}

function alternarMenu(evento) {
    evento.stopPropagation();
    document.getElementById("menuUsuario").classList.toggle("abierto");
}

// Si se hace clic fuera del menu, se cierra
document.addEventListener("click", function () {
    const menu = document.getElementById("menuUsuario");
    if (menu) menu.classList.remove("abierto");
});

function abrirDatosPrueba() {
    document.getElementById("resultadoDatos").classList.add("oculto");
    document.getElementById("modalDatos").classList.add("abierto");
}

function cerrarDatosPrueba() {
    document.getElementById("modalDatos").classList.remove("abierto");
    // Se recarga la pagina para que se vean los datos nuevos
    if (!document.getElementById("resultadoDatos").classList.contains("oculto")) {
        location.reload();
    }
}

async function generarDatos() {
    const respuesta = await llamarApi("/api/datosprueba", { cantidad: valor("cantidadDatos") });
    if (respuesta.ok) {
        const pre = document.getElementById("resultadoDatos");
        pre.textContent = respuesta.mensaje;
        pre.classList.remove("oculto");
    } else {
        mostrarAviso(respuesta.mensaje, true);
    }
}

async function salirModoGrafico() {
    if (!confirm("¿Cerrar el modo gráfico y volver al menú de la consola?")) return;

    await llamarApi("/api/salir", {});
    document.body.insertAdjacentHTML("beforeend",
        '<div class="cerrado">' +
        '  <h1>Modo gráfico cerrado</h1>' +
        '  <p>Puede cerrar esta pestaña y continuar en la consola.<br>Los datos se conservan mientras el programa siga abierto.</p>' +
        '</div>');
}
