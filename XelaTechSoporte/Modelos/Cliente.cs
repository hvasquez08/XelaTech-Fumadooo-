namespace XelaTechSoporte;

public class Cliente
{
    // ===== Atributos (encapsulados) =====
    private string codigo;
    private string nombreCompleto;
    private string telefono;
    private string correo;
    private TipoCliente tipo;
    private int cantidadSolicitudes;

    // ===== Constructores =====

    // Constructor completo
    public Cliente(string codigo, string nombre, string tel, string correo, TipoCliente tipo)
    {
        Codigo = codigo;
        NombreCompleto = nombre;
        Telefono = tel;
        Correo = correo;
        this.tipo = tipo;
        cantidadSolicitudes = 0;
    }

    // Sobrecarga del constructor: registro rapido solo con codigo y nombre.
    // El telefono y el correo quedan pendientes y se completan con ActualizarDatos.
    public Cliente(string codigo, string nombre)
    {
        Codigo = codigo;
        NombreCompleto = nombre;
        telefono = "Pendiente";
        correo = "Pendiente";
        tipo = TipoCliente.Estudiante;
        cantidadSolicitudes = 0;
    }

    // ===== Propiedades =====

    public string Codigo
    {
        get { return codigo; }
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("El código del cliente no puede estar vacío.");
            codigo = value.Trim();
        }
    }

    public string NombreCompleto
    {
        get { return nombreCompleto; }
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("El nombre del cliente es obligatorio.");
            nombreCompleto = value.Trim();
        }
    }

    public string Telefono
    {
        get { return telefono; }
        private set
        {
            // Validacion adicional: 8 digitos (formato de Guatemala)
            if (!EsTelefonoValido(value))
                throw new Exception("El teléfono debe tener 8 dígitos numéricos.");
            telefono = value.Trim();
        }
    }

    public string Correo
    {
        get { return correo; }
        private set
        {
            // Validacion adicional: debe tener @ y un punto despues
            if (!EsCorreoValido(value))
                throw new Exception("El correo no tiene un formato válido (ejemplo: nombre@correo.com).");
            correo = value.Trim();
        }
    }

    public TipoCliente Tipo
    {
        get { return tipo; }
    }

    public int CantidadSolicitudes
    {
        get { return cantidadSolicitudes; }
    }

    // ===== Metodos =====

    public void ActualizarDatos(string tel, string nuevoCorreo)
    {
        Telefono = tel;
        Correo = nuevoCorreo;
    }

    public void IncrementarSolicitudes()
    {
        cantidadSolicitudes++;
    }

    // Validaciones publicas para que el menu pueda volver a pedir el dato si esta mal
    public static bool EsTelefonoValido(string tel)
    {
        if (string.IsNullOrWhiteSpace(tel)) return false;
        tel = tel.Trim();
        if (tel.Length != 8) return false;
        for (int i = 0; i < tel.Length; i++)
        {
            if (!char.IsDigit(tel[i])) return false;
        }
        return true;
    }

    public static bool EsCorreoValido(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;
        texto = texto.Trim();
        if (texto.Contains(" ")) return false;

        int posArroba = texto.IndexOf('@');
        if (posArroba <= 0) return false;

        // Debe haber un punto despues del @ (y no pegado ni al final)
        int posPunto = texto.IndexOf('.', posArroba);
        return posPunto > posArroba + 1 && posPunto < texto.Length - 1;
    }

    public override string ToString()
    {
        return codigo + " | " + nombreCompleto + " | Tel: " + telefono + " | " + correo +
               " | " + Formato.NombreTipoCliente(tipo) + " | Solicitudes: " + cantidadSolicitudes;
    }
}
