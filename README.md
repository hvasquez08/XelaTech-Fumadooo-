# XelaTech Soporte

Sistema de consola en C# para llevar el control de un taller de soporte técnico:
clientes, técnicos, equipos, repuestos y solicitudes de servicio.
Todo se guarda en listas en memoria (no usa base de datos).

## Qué tiene

- Registro de clientes, técnicos, equipos (escritorio, laptop e impresora) y repuestos
- Solicitudes de servicio con prioridad y estado
- Búsqueda secuencial y búsqueda binaria
- Ordenamientos: Bubble Sort, Selection Sort, Shell Sort y Quick Sort
- Matriz de carga semanal de los técnicos
- 12 reportes
- Generador de datos de prueba
- Modo gráfico: un servidor web local con páginas HTML que usan los mismos gestores de la consola

## Cómo ejecutarlo

Se necesita el SDK de .NET 10.

```
dotnet run --project XelaTechSoporte
```

Para abrir directo el modo gráfico:

```
dotnet run --project XelaTechSoporte -- web
```

Luego se abre en el navegador `http://localhost:5000/`.
