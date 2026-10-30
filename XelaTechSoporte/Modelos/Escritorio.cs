namespace XelaTechSoporte;

// Computadora de escritorio, hereda de Equipo
public class Escritorio : Equipo
{
    private string tipoGabinete;
    private bool tieneFuenteExterna;

    public Escritorio(string codigoInterno, string numeroSerie, string marca, string modelo,
                      Cliente propietario, string estadoFisico, string accesorios,
                      string tipoGabinete, bool tieneFuenteExterna)
        : base(codigoInterno, numeroSerie, marca, modelo, propietario, estadoFisico, accesorios)
    {
        if (string.IsNullOrWhiteSpace(tipoGabinete))
            throw new Exception("El tipo de gabinete es obligatorio.");

        this.tipoGabinete = tipoGabinete.Trim();
        this.tieneFuenteExterna = tieneFuenteExterna;
    }

    public string TipoGabinete { get { return tipoGabinete; } }
    public bool TieneFuenteExterna { get { return tieneFuenteExterna; } }

    // Revision de escritorio: Q100
    public override decimal CalcularCostoRevision()
    {
        return 100;
    }

    public override string Diagnosticar()
    {
        string fuente = tieneFuenteExterna ? "externa" : "interna";
        return "Revisar fuente de poder " + fuente + ", gabinete " + tipoGabinete +
               ", memoria RAM, disco duro y ventiladores.";
    }

    public override string TipoEquipo()
    {
        return "Escritorio";
    }

    public override string DatosPropios()
    {
        string fuente = tieneFuenteExterna ? "Fuente externa" : "Fuente interna";
        return "Gabinete " + tipoGabinete + " | " + fuente;
    }

    public override string ToString()
    {
        return base.ToString() + " | " + DatosPropios();
    }
}
