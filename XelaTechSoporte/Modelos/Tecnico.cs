namespace XelaTechSoporte;

public class Tecnico
{
    // Limite de trabajos activos que puede tener un tecnico al mismo tiempo
    public const int LIMITE_ACTIVOS = 3;

    // ===== Atributos =====
    private string codigo;
    private string nombreCompleto;
    private Especialidad especialidad;
    private bool disponible;
    private int trabajosActivos;
    private int serviciosFinalizados;

    // Arreglo unidimensional: la posicion i guarda cuantas calificaciones de valor (i + 1) recibio.
    // calificaciones[0] = cantidad de "1", calificaciones[4] = cantidad de "5"
    private int[] calificaciones = new int[5];

    public Tecnico(string codigo, string nombre, Especialidad especialidad, bool disponible)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new Exception("El código del técnico no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(nombre))
            throw new Exception("El nombre del técnico es obligatorio.");

        this.codigo = codigo.Trim();
        this.nombreCompleto = nombre.Trim();
        this.especialidad = especialidad;
        this.disponible = disponible;
        trabajosActivos = 0;
        serviciosFinalizados = 0;
    }

    // ===== Propiedades =====
    public string Codigo { get { return codigo; } }
    public string NombreCompleto { get { return nombreCompleto; } }
    public Especialidad Especialidad { get { return especialidad; } }
    public int TrabajosActivos { get { return trabajosActivos; } }
    public int ServiciosFinalizados { get { return serviciosFinalizados; } }
    public int[] Calificaciones { get { return calificaciones; } }

    public bool Disponible
    {
        get { return disponible; }
        set { disponible = value; }
    }

    // Promedio calculado a partir del arreglo de calificaciones
    public double CalificacionPromedio
    {
        get
        {
            int cantidad = 0;
            int suma = 0;
            for (int i = 0; i < calificaciones.Length; i++)
            {
                cantidad += calificaciones[i];
                suma += calificaciones[i] * (i + 1);
            }
            if (cantidad == 0) return 0;
            return (double)suma / cantidad;
        }
    }

    public int TotalCalificaciones
    {
        get
        {
            int total = 0;
            for (int i = 0; i < calificaciones.Length; i++)
            {
                total += calificaciones[i];
            }
            return total;
        }
    }

    // ===== Metodos =====

    // Revisa si el tecnico puede recibir un trabajo de cierta especialidad
    public bool PuedeRecibir(Especialidad esp)
    {
        if (!disponible) return false;                         // validacion 18
        if (trabajosActivos >= LIMITE_ACTIVOS) return false;   // validacion 7
        if (especialidad == Especialidad.General) return true; // General atiende todo
        return especialidad == esp;                            // validacion 6
    }

    public void AsignarTrabajo()
    {
        if (trabajosActivos >= LIMITE_ACTIVOS)
            throw new Exception("El técnico " + nombreCompleto + " ya llegó al límite de " + LIMITE_ACTIVOS + " trabajos activos.");
        trabajosActivos++;
    }

    // Se llama al entregar: libera el trabajo y suma un servicio finalizado
    public void FinalizarTrabajo()
    {
        if (trabajosActivos > 0) trabajosActivos--;
        serviciosFinalizados++;
    }

    // Se llama al cancelar: solo libera el trabajo (no cuenta como finalizado)
    public void LiberarTrabajo()
    {
        if (trabajosActivos > 0) trabajosActivos--;
    }

    public void RegistrarCalificacion(int c)
    {
        if (c < 1 || c > 5)
            throw new Exception("La calificación debe estar entre 1 y 5.");
        calificaciones[c - 1]++;
    }

    public override string ToString()
    {
        string estado = disponible ? "Disponible" : "No disponible";
        return codigo + " | " + nombreCompleto + " | " + especialidad + " | " + estado +
               " | Activos: " + trabajosActivos + "/" + LIMITE_ACTIVOS +
               " | Finalizados: " + serviciosFinalizados +
               " | Promedio: " + CalificacionPromedio.ToString("0.00");
    }
}
