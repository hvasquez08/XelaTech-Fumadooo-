// Pagina de repuestos: usa GestorRepuestos desde C#

dibujarBarra(false);

// Aqui se guardan las filas del inventario para mostrar el detalle del repuesto elegido
let inventario = [];

cargarRepuestos();

async function cargarRepuestos() {
    // Para el select siempre se carga el inventario completo
    const todos = await llamarApi("/api/repuestos/listar");
    if (!todos.ok) {
        mostrarAviso(todos.mensaje, true);
        return;
    }
    inventario = todos.filas;

    const elegido = valor("repuestoElegido");
    llenarSelect("repuestoElegido", inventario, "Seleccione un repuesto", 1);
    document.getElementById("repuestoElegido").value = elegido;
    mostrarRepuesto();

    // La tabla muestra todos o solo los que estan en minimo
    if (opcionElegida("grupoVista") === "minimos") {
        const minimos = await llamarApi("/api/repuestos/minimos");
        dibujarTabla("tablaRepuestos", minimos.columnas, minimos.filas);
    } else {
        dibujarTabla("tablaRepuestos", todos.columnas, todos.filas);
    }
}

// Columnas: 0 Código, 1 Nombre, 2 Categoría, 3 Existencia, 4 Mínimo, 5 Costo, 6 Precio, 7 Usados, 8 Estado
function mostrarRepuesto() {
    const codigo = valor("repuestoElegido");
    const ficha = document.getElementById("ficha");

    let fila = null;
    for (const f of inventario) {
        if (f[0] === codigo) fila = f;
    }

    if (fila === null) {
        ficha.classList.add("oculto");
        return;
    }

    document.getElementById("fNombre").textContent = fila[1];
    document.getElementById("fCategoria").textContent = fila[2];
    document.getElementById("fExistencia").textContent = fila[3];
    document.getElementById("fMinimo").textContent = fila[4];
    document.getElementById("fPrecio").textContent = fila[6];
    document.getElementById("fEstado").innerHTML = '<span class="etiqueta ' + claseEtiqueta(fila[8]) + '">' + fila[8] + '</span>';
    ficha.classList.remove("oculto");
}

async function registrarRepuesto() {
    const datos = {
        nombre: valor("nombre"),
        categoria: valor("categoria"),
        existencia: valor("existencia"),
        minima: valor("minima"),
        costo: valor("costo"),
        precio: valor("precio")
    };

    const respuesta = await llamarApi("/api/repuestos/registrar", datos);
    if (revisarRespuesta(respuesta)) {
        limpiarCampos(["nombre", "categoria", "costo", "precio"]);
        cargarRepuestos();
    }
}

async function reabastecer() {
    const datos = {
        codigo: valor("repuestoElegido"),
        cantidad: valor("cantidad")
    };

    if (datos.codigo === "") {
        mostrarAviso("Seleccione un repuesto.", true);
        return;
    }

    const respuesta = await llamarApi("/api/repuestos/reabastecer", datos);
    if (revisarRespuesta(respuesta)) {
        cargarRepuestos();
    }
}
