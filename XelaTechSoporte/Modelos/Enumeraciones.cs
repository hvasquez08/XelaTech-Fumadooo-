namespace XelaTechSoporte;

// Prioridad con la que se atiende una solicitud
public enum Prioridad
{
    Normal,
    Urgente
}

// Estados por los que pasa una solicitud (en este orden)
public enum EstadoSolicitud
{
    Recibida,
    EnEspera,
    Asignada,
    EnDiagnostico,
    EnReparacion,
    ListaParaEntregar,
    Entregada,
    Cancelada
}

// Tipos de cliente que atiende XelaTech
public enum TipoCliente
{
    Estudiante,
    Profesional,
    PequenoNegocio,
    Institucion
}

// Especialidad de cada tecnico
public enum Especialidad
{
    Computadoras,
    Impresoras,
    General
}
