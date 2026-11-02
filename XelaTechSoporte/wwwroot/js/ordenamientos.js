// Pagina de ordenamientos: usa la clase Ordenador de C#

dibujarBarra(false);

// Explicacion corta de cada algoritmo (la misma idea del documento de analisis)
const explicaciones = {
    bubble: "Bubble Sort recorre la lista comparando cada solicitud con la siguiente y las intercambia si están en el orden " +
            "contrario. En cada pasada el valor extremo queda al final. Si en una pasada no hubo intercambios, se detiene.",
    quick: "Quick Sort toma una solicitud como pivote (la última) y deja a un lado las que llegaron antes y al otro las que " +
           "llegaron después. Luego repite lo mismo en cada lado (recursividad) hasta que cada parte tiene 0 o 1 elementos.",
    selection: "Selection Sort busca, entre los técnicos que faltan por ubicar, el que tiene menos (o más) servicios " +
               "finalizados y lo coloca en la siguiente posición. Hace un solo intercambio por vuelta.",
    shell: "Shell Sort es como la inserción, pero compara repuestos separados por una brecha. La brecha empieza en la mitad " +
           "de la lista y se va partiendo a la mitad hasta llegar a 1."
};

mostrarExplicacion();

function mostrarExplicacion() {
    document.getElementById("explicacion").textContent = explicaciones[valor("algoritmo")];
}

async function ordenar() {
    const respuesta = await llamarApi("/api/ordenar", {
        algoritmo: valor("algoritmo"),
        orden: opcionElegida("grupoOrden")
    });

    if (!respuesta.ok) {
        mostrarAviso(respuesta.mensaje, true);
        return;
    }

    // Cada reporte indica el algoritmo, el criterio y el orden
    const info = document.getElementById("infoOrden");
    info.innerHTML = "";
    const datos = ["Algoritmo: " + respuesta.algoritmo, "Criterio: " + respuesta.criterio, "Orden: " + respuesta.orden,
                   "Registros: " + respuesta.filas.length];
    for (const texto of datos) {
        const span = document.createElement("span");
        span.textContent = texto;
        info.appendChild(span);
    }

    dibujarTabla("tablaResultado", respuesta.columnas, respuesta.filas);
}
