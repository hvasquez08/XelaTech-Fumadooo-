// Portal del cliente: registrarse y consultar el estado de su solicitud

dibujarBarra(true);

// Icono y color para cada estado (el nombre es el del enum EstadoSolicitud de C#)
const estilosEstado = {
    Recibida: { icono: "reloj.svg", color: "#8a9bb0" },
    EnEspera: { icono: "reloj.svg", color: "#e2a516" },
    Asignada: { icono: "tecnicos.svg", color: "#2e8ee8" },
    EnDiagnostico: { icono: "busquedas.svg", color: "#7b5cc4" },
    EnReparacion: { icono: "repuestos.svg", color: "#f7932f" },
    ListaParaEntregar: { icono: "check.svg", color: "#1f8a52" },
    Entregada: { icono: "check.svg", color: "#3f4d5e" },
    Cancelada: { icono: "cancelado.svg", color: "#d64545" }
};

// Orden del proceso para dibujar los pasos
const pasos = ["Recibida", "EnEspera", "Asignada", "EnDiagnostico", "EnReparacion", "ListaParaEntregar", "Entregada"];

async function registrar() {
    const datos = {
        nombre: valor("nombre"),
        telefono: valor("telefono"),
        correo: valor("correo"),
        tipo: valor("tipo")
    };

    const respuesta = await llamarApi("/api/clientes/registrar", datos);
    const caja = document.getElementById("mensajeRegistro");

    if (respuesta.ok) {
        caja.textContent = "¡Registro exitoso! Su código de cliente es " + respuesta.codigo +
                           ". Preséntelo en el mostrador al dejar su equipo.";
        caja.classList.remove("oculto");
        limpiarCampos(["nombre", "telefono", "correo"]);
    } else {
        caja.classList.add("oculto");
        mostrarAviso(respuesta.mensaje, true);
    }
}

async function consultarEstado() {
    const respuesta = await llamarApi("/api/portal/estado", { codigo: valor("codigo") });
    const resultado = document.getElementById("resultado");
    resultado.innerHTML = "";

    if (!respuesta.ok) {
        const p = document.createElement("p");
        p.style.color = "var(--rojo)";
        p.textContent = respuesta.mensaje;
        resultado.appendChild(p);
        return;
    }

    const estilo = estilosEstado[respuesta.estado];

    // Icono
    const icono = document.createElement("div");
    icono.className = "icono-estado";
    icono.style.background = estilo.color;
    icono.innerHTML = '<img src="img/' + estilo.icono + '" alt="">';
    resultado.appendChild(icono);

    // Estado
    const titulo = document.createElement("h3");
    titulo.textContent = "Estado: " + respuesta.estadoTexto;
    resultado.appendChild(titulo);

    // Datos (solo los de esta solicitud)
    agregarLinea(resultado, "Código: " + respuesta.codigo);
    agregarLinea(resultado, "Equipo: " + respuesta.equipo);
    agregarLinea(resultado, "Técnico: " + respuesta.tecnico);
    agregarLinea(resultado, "Recibido: " + respuesta.fecha);
    if (respuesta.total !== "") {
        agregarLinea(resultado, "Total a pagar: " + respuesta.total);
    }

    // Pasos del proceso
    if (respuesta.estado !== "Cancelada") {
        const actual = pasos.indexOf(respuesta.estado);
        const barraPasos = document.createElement("div");
        barraPasos.className = "pasos";
        for (let i = 0; i < pasos.length; i++) {
            const punto = document.createElement("div");
            punto.className = i <= actual ? "paso hecho" : "paso";
            punto.title = pasos[i];
            barraPasos.appendChild(punto);
        }
        resultado.appendChild(barraPasos);
    }

    const nota = document.createElement("div");
    nota.className = "nota-binaria";
    nota.textContent = "Encontrada con búsqueda binaria en " + respuesta.comparaciones + " comparación(es).";
    resultado.appendChild(nota);
}

function agregarLinea(contenedor, texto) {
    const p = document.createElement("p");
    p.textContent = texto;
    contenedor.appendChild(p);
}
