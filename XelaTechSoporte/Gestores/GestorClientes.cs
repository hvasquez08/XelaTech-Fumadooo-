namespace XelaTechSoporte;

// Administra la lista de clientes
public class GestorClientes
{
    private List<Cliente> clientes = new List<Cliente>();

    public List<Cliente> Clientes
    {
        get { return clientes; }
    }

    // Genera el siguiente codigo: CLI-0001, CLI-0002, ...
    public string GenerarCodigo()
    {
        int numero = clientes.Count + 1;
        string codigo = "CLI-" + numero.ToString("D4");
        // Por seguridad se verifica que no exista
        while (BuscarPorCodigo(codigo) != null)
        {
            numero++;
            codigo = "CLI-" + numero.ToString("D4");
        }
        return codigo;
    }

    public void Registrar(Cliente c)
    {
        if (c == null)
            throw new Exception("No se puede registrar un cliente vacío.");
        // Validacion 1: no repetir codigos
        if (BuscarPorCodigo(c.Codigo) != null)
            throw new Exception("Ya existe un cliente con el código " + c.Codigo + ".");

        clientes.Add(c);
    }

    public Cliente BuscarPorCodigo(string cod)
    {
        if (string.IsNullOrWhiteSpace(cod)) return null;
        cod = cod.Trim().ToUpper();

        foreach (Cliente c in clientes)
        {
            if (c.Codigo == cod) return c;
        }
        return null;
    }

    public void Actualizar(string cod, string tel, string correo)
    {
        Cliente c = BuscarPorCodigo(cod);
        if (c == null)
            throw new Exception("No existe el cliente " + cod + ".");
        c.ActualizarDatos(tel, correo);
    }

    // Usa la busqueda secuencial del Buscador
    public List<Cliente> BuscarPorNombreParcial(string txt)
    {
        return Buscador.SecuencialClientesPorNombre(clientes, txt);
    }
}
