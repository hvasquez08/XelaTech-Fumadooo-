namespace XelaTechSoporte;

// Clase base (abstracta) de todos los equipos.
// No se pueden crear objetos "Equipo" directamente, solo Laptop, Escritorio o Impresora.
public abstract class Equipo
{
    // Atributos protegidos: las clases hijas los pueden usar
    protected string codigoInterno;
    protected string numeroSerie;
    protected string marca;
    protected string modelo;
    protected Cliente propietario;
    protected string estadoFisico;
    protected string accesorios;

    public Equipo(string codigoInterno, string numeroSerie, string marca, string modelo,
                  Cliente propietario, string estadoFisico, string accesorios)
    {
        if (string.IsNullOrWhiteSpace(codigoInterno))
            throw new Exception("El código interno es obligatorio.");
        if (string.IsNullOrWhiteSpace(numeroSerie))
            throw new Exception("El número de serie es obligatorio.");
        if (string.IsNullOrWhiteSpace(marca))
            throw new Exception("La marca es obligatoria.");
        if (string.IsNullOrWhiteSpace(modelo))
            throw new Exception("El modelo es obligatorio.");
        if (propietario == null)
            throw new Exception("El equipo debe tener un propietario registrado.");
        if (string.IsNullOrWhiteSpace(estadoFisico))
            throw new Exception("El estado físico es obligatorio.");

        this.codigoInterno = codigoInterno.Trim();
        this.numeroSerie = numeroSerie.Trim().ToUpper();
        this.marca = marca.Trim();
        this.modelo = modelo.Trim();
        this.propietario = propietario;
        this.estadoFisico = estadoFisico.Trim();

        // Los accesorios no son obligatorios
        if (string.IsNullOrWhiteSpace(accesorios))
            this.accesorios = "Ninguno";
        else
            this.accesorios = accesorios.Trim();
    }

    // ===== Propiedades (solo lectura) =====
    public string CodigoInterno { get { return codigoInterno; } }
    public string NumeroSerie { get { return numeroSerie; } }
    public string Marca { get { return marca; } }
    public string Modelo { get { return modelo; } }
    public Cliente Propietario { get { return propietario; } }
    public string EstadoFisico { get { return estadoFisico; } }
    public string Accesorios { get { return accesorios; } }

    // ===== Metodos abstractos: cada tipo de equipo los programa a su manera =====
    public abstract decimal CalcularCostoRevision();
    public abstract string Diagnosticar();
    public abstract string TipoEquipo();

    // Especialidad que necesita el tecnico. Por defecto es Computadoras;
    // la impresora la sobrescribe.
    public virtual Especialidad EspecialidadRequerida()
    {
        return Especialidad.Computadoras;
    }

    // Datos propios de cada tipo (pulgadas, gabinete, tinta...)
    public virtual string DatosPropios()
    {
        return "";
    }

    // Nombre corto, por ejemplo "Laptop HP"
    public string Descripcion()
    {
        return TipoEquipo() + " " + marca + " " + modelo;
    }

    public override string ToString()
    {
        return codigoInterno + " | " + TipoEquipo() + " " + marca + " " + modelo +
               " | Serie: " + numeroSerie + " | Dueño: " + propietario.NombreCompleto +
               " | Estado: " + estadoFisico + " | Accesorios: " + accesorios;
    }
}
