namespace XelaTechSoporte;

// Administra la lista de equipos (Laptop, Escritorio e Impresora juntos)
public class GestorEquipos
{
    // La lista es de tipo Equipo, pero guarda objetos de las clases hijas (polimorfismo)
    private List<Equipo> equipos = new List<Equipo>();

    public List<Equipo> Equipos
    {
        get { return equipos; }
    }

    // EQ-0001, EQ-0002, ...
    public string GenerarCodigo()
    {
        int numero = equipos.Count + 1;
        string codigo = "EQ-" + numero.ToString("D4");
        while (BuscarPorCodigo(codigo) != null)
        {
            numero++;
            codigo = "EQ-" + numero.ToString("D4");
        }
        return codigo;
    }

    public void Registrar(Equipo e)
    {
        if (e == null)
            throw new Exception("No se puede registrar un equipo vacío.");
        if (BuscarPorCodigo(e.CodigoInterno) != null)
            throw new Exception("Ya existe un equipo con el código " + e.CodigoInterno + ".");
        // Validacion 1: no repetir numeros de serie
        if (ExisteSerie(e.NumeroSerie))
            throw new Exception("Ya existe un equipo con el número de serie " + e.NumeroSerie + ".");

        equipos.Add(e);
    }

    public bool ExisteSerie(string serie)
    {
        if (string.IsNullOrWhiteSpace(serie)) return false;
        serie = serie.Trim().ToUpper();

        foreach (Equipo e in equipos)
        {
            if (e.NumeroSerie == serie) return true;
        }
        return false;
    }

    public Equipo BuscarPorCodigo(string cod)
    {
        if (string.IsNullOrWhiteSpace(cod)) return null;
        cod = cod.Trim().ToUpper();

        foreach (Equipo e in equipos)
        {
            if (e.CodigoInterno == cod) return e;
        }
        return null;
    }

    // Usa la busqueda secuencial del Buscador
    public List<Equipo> BuscarPorMarca(string marca)
    {
        return Buscador.SecuencialEquiposPorMarca(equipos, marca);
    }

    public List<Equipo> EquiposDeCliente(Cliente c)
    {
        List<Equipo> lista = new List<Equipo>();
        foreach (Equipo e in equipos)
        {
            if (e.Propietario == c) lista.Add(e);
        }
        return lista;
    }
}
