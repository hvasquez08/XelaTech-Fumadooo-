namespace XelaTechSoporte;

public class Repuesto
{
    private string codigo;
    private string nombre;
    private string categoria;
    private int existencia;
    private int existenciaMinima;
    private decimal costoUnitario;
    private decimal precioVenta;
    private int cantidadUtilizada;

    public Repuesto(string codigo, string nombre, string categoria, int existencia,
                    int existenciaMinima, decimal costoUnitario, decimal precioVenta)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new Exception("El código del repuesto es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre))
            throw new Exception("El nombre del repuesto es obligatorio.");
        if (string.IsNullOrWhiteSpace(categoria))
            throw new Exception("La categoría del repuesto es obligatoria.");
        if (existencia < 0)
            throw new Exception("La existencia no puede ser negativa.");
        if (existenciaMinima < 0)
            throw new Exception("La existencia mínima no puede ser negativa.");
        if (costoUnitario <= 0)
            throw new Exception("El costo unitario debe ser mayor que cero.");
        // Validacion adicional: no vender con perdida
        if (precioVenta < costoUnitario)
            throw new Exception("El precio de venta no puede ser menor que el costo unitario.");

        this.codigo = codigo.Trim();
        this.nombre = nombre.Trim();
        this.categoria = categoria.Trim();
        this.existencia = existencia;
        this.existenciaMinima = existenciaMinima;
        this.costoUnitario = costoUnitario;
        this.precioVenta = precioVenta;
        cantidadUtilizada = 0;
    }

    public string Codigo { get { return codigo; } }
    public string Nombre { get { return nombre; } }
    public string Categoria { get { return categoria; } }
    public int Existencia { get { return existencia; } }
    public int ExistenciaMinima { get { return existenciaMinima; } }
    public decimal CostoUnitario { get { return costoUnitario; } }
    public decimal PrecioVenta { get { return precioVenta; } }
    public int CantidadUtilizada { get { return cantidadUtilizada; } }

    // Se usa cuando una solicitud ocupa este repuesto
    public void Descontar(int cant)
    {
        if (cant <= 0)
            throw new Exception("La cantidad debe ser mayor que cero.");
        if (!HayExistencia(cant))
            throw new Exception("No hay existencia suficiente de " + nombre + ". Disponible: " + existencia);

        existencia = existencia - cant;
        cantidadUtilizada = cantidadUtilizada + cant;
    }

    public void Reabastecer(int cant)
    {
        if (cant <= 0)
            throw new Exception("La cantidad a reabastecer debe ser mayor que cero.");
        existencia = existencia + cant;
    }

    // Se usa cuando se cancela una solicitud: el repuesto regresa al inventario
    public void Devolver(int cant)
    {
        if (cant <= 0) return;
        existencia = existencia + cant;
        cantidadUtilizada = cantidadUtilizada - cant;
        if (cantidadUtilizada < 0) cantidadUtilizada = 0;
    }

    public bool EstaAgotado()
    {
        return existencia == 0;
    }

    // Alcanzo o quedo por debajo de su existencia minima
    public bool EstaEnMinimo()
    {
        return existencia <= existenciaMinima;
    }

    public bool HayExistencia(int cant)
    {
        return cant <= existencia;
    }

    public string EstadoInventario()
    {
        if (EstaAgotado()) return "AGOTADO";
        if (EstaEnMinimo()) return "EN MÍNIMO";
        return "Normal";
    }

    public override string ToString()
    {
        return codigo + " | " + nombre + " | " + categoria + " | Existencia: " + existencia +
               " (mín. " + existenciaMinima + ") | Costo: " + Formato.Dinero(costoUnitario) +
               " | Venta: " + Formato.Dinero(precioVenta) + " | Usados: " + cantidadUtilizada;
    }
}
