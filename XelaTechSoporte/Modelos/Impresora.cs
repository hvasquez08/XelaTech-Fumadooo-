namespace XelaTechSoporte;

// Impresora, hereda de Equipo
public class Impresora : Equipo
{
    private string tecnologia;
    private int nivelTinta; // porcentaje de 0 a 100

    public Impresora(string codigoInterno, string numeroSerie, string marca, string modelo,
                     Cliente propietario, string estadoFisico, string accesorios,
                     string tecnologia, int nivelTinta)
        : base(codigoInterno, numeroSerie, marca, modelo, propietario, estadoFisico, accesorios)
    {
        if (string.IsNullOrWhiteSpace(tecnologia))
            throw new Exception("La tecnología de la impresora es obligatoria.");
        if (nivelTinta < 0 || nivelTinta > 100)
            throw new Exception("El nivel de tinta debe estar entre 0 y 100.");

        this.tecnologia = tecnologia.Trim();
        this.nivelTinta = nivelTinta;
    }

    public string Tecnologia { get { return tecnologia; } }
    public int NivelTinta { get { return nivelTinta; } }

    // Revision de impresora: Q80
    public override decimal CalcularCostoRevision()
    {
        return 80;
    }

    public override string Diagnosticar()
    {
        string texto = "Revisar cabezal, rodillos y alimentación de papel (" + tecnologia +
                       "). Nivel de tinta/tóner: " + nivelTinta + "%.";
        if (nivelTinta < 15)
            texto = texto + " Se recomienda recargar.";
        return texto;
    }

    public override string TipoEquipo()
    {
        return "Impresora";
    }

    // Las impresoras las atiende un tecnico de impresoras (o uno General)
    public override Especialidad EspecialidadRequerida()
    {
        return Especialidad.Impresoras;
    }

    public override string DatosPropios()
    {
        return tecnologia + " | Tinta: " + nivelTinta + "%";
    }

    public override string ToString()
    {
        return base.ToString() + " | " + DatosPropios();
    }
}
