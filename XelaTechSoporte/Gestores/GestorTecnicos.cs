namespace XelaTechSoporte;

// Administra la lista de tecnicos
public class GestorTecnicos
{
    private List<Tecnico> tecnicos = new List<Tecnico>();

    public List<Tecnico> Tecnicos
    {
        get { return tecnicos; }
    }

    // TEC-001, TEC-002, ...
    public string GenerarCodigo()
    {
        int numero = tecnicos.Count + 1;
        string codigo = "TEC-" + numero.ToString("D3");
        while (BuscarPorCodigo(codigo) != null)
        {
            numero++;
            codigo = "TEC-" + numero.ToString("D3");
        }
        return codigo;
    }

    public void Registrar(Tecnico t)
    {
        if (t == null)
            throw new Exception("No se puede registrar un técnico vacío.");
        if (BuscarPorCodigo(t.Codigo) != null)
            throw new Exception("Ya existe un técnico con el código " + t.Codigo + ".");

        tecnicos.Add(t);
    }

    public Tecnico BuscarPorCodigo(string cod)
    {
        if (string.IsNullOrWhiteSpace(cod)) return null;
        cod = cod.Trim().ToUpper();

        foreach (Tecnico t in tecnicos)
        {
            if (t.Codigo == cod) return t;
        }
        return null;
    }

    // Busca entre los tecnicos que pueden recibir el trabajo
    // y devuelve el que tiene MENOS trabajos activos (balanceo de carga).
    // Si ninguno cumple devuelve null.
    public Tecnico ObtenerDisponible(Especialidad esp)
    {
        Tecnico elegido = null;
        foreach (Tecnico t in tecnicos)
        {
            if (t.PuedeRecibir(esp))
            {
                if (elegido == null || t.TrabajosActivos < elegido.TrabajosActivos)
                {
                    elegido = t;
                }
            }
        }
        return elegido;
    }

    // Disponibles y con espacio para mas trabajos
    public List<Tecnico> ListarDisponibles()
    {
        List<Tecnico> lista = new List<Tecnico>();
        foreach (Tecnico t in tecnicos)
        {
            if (t.Disponible && t.TrabajosActivos < Tecnico.LIMITE_ACTIVOS)
            {
                lista.Add(t);
            }
        }
        return lista;
    }

    // Posicion del tecnico en la lista = fila que le toca en la matriz de carga
    public int IndiceDe(Tecnico t)
    {
        for (int i = 0; i < tecnicos.Count; i++)
        {
            if (tecnicos[i] == t) return i;
        }
        return -1;
    }
}
