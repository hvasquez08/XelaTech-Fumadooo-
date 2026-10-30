namespace XelaTechSoporte;

// Laptop hereda de Equipo
public class Laptop : Equipo
{
    private double pulgadas;
    private string estadoBateria;

    public Laptop(string codigoInterno, string numeroSerie, string marca, string modelo,
                  Cliente propietario, string estadoFisico, string accesorios,
                  double pulgadas, string estadoBateria)
        : base(codigoInterno, numeroSerie, marca, modelo, propietario, estadoFisico, accesorios)
    {
        if (pulgadas < 10 || pulgadas > 20)
            throw new Exception("El tamaño de pantalla debe estar entre 10 y 20 pulgadas.");
        if (string.IsNullOrWhiteSpace(estadoBateria))
            throw new Exception("El estado de la batería es obligatorio.");

        this.pulgadas = pulgadas;
        this.estadoBateria = estadoBateria.Trim();
    }

    public double Pulgadas { get { return pulgadas; } }
    public string EstadoBateria { get { return estadoBateria; } }

    // Revision de laptop: Q150
    public override decimal CalcularCostoRevision()
    {
        return 150;
    }

    public override string Diagnosticar()
    {
        return "Revisar batería (" + estadoBateria + "), pantalla de " + pulgadas +
               "\", teclado, bisagras y temperatura del procesador.";
    }

    public override string TipoEquipo()
    {
        return "Laptop";
    }

    public override string DatosPropios()
    {
        return pulgadas + "\" | Batería: " + estadoBateria;
    }

    public override string ToString()
    {
        return base.ToString() + " | " + DatosPropios();
    }
}
