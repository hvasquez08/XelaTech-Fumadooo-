namespace XelaTechSoporte;

// Administra el inventario de repuestos
public class GestorRepuestos
{
    private List<Repuesto> repuestos = new List<Repuesto>();

    public List<Repuesto> Repuestos
    {
        get { return repuestos; }
    }

    // REP-001, REP-002, ...
    public string GenerarCodigo()
    {
        int numero = repuestos.Count + 1;
        string codigo = "REP-" + numero.ToString("D3");
        while (BuscarPorCodigo(codigo) != null)
        {
            numero++;
            codigo = "REP-" + numero.ToString("D3");
        }
        return codigo;
    }

    public void Registrar(Repuesto r)
    {
        if (r == null)
            throw new Exception("No se puede registrar un repuesto vacío.");
        if (BuscarPorCodigo(r.Codigo) != null)
            throw new Exception("Ya existe un repuesto con el código " + r.Codigo + ".");

        repuestos.Add(r);
    }

    public Repuesto BuscarPorCodigo(string cod)
    {
        if (string.IsNullOrWhiteSpace(cod)) return null;
        cod = cod.Trim().ToUpper();

        foreach (Repuesto r in repuestos)
        {
            if (r.Codigo == cod) return r;
        }
        return null;
    }

    public void Reabastecer(string cod, int cantidad)
    {
        Repuesto r = BuscarPorCodigo(cod);
        if (r == null)
            throw new Exception("No existe el repuesto " + cod + ".");
        r.Reabastecer(cantidad);
    }

    public List<Repuesto> ListarEnMinimoOAgotados()
    {
        List<Repuesto> lista = new List<Repuesto>();
        foreach (Repuesto r in repuestos)
        {
            if (r.EstaEnMinimo()) lista.Add(r); // EstaEnMinimo tambien incluye a los agotados
        }
        return lista;
    }
}
