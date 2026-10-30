namespace XelaTechSoporte;

// Clase asociativa: une una solicitud con un repuesto y guarda cuantos se usaron.
// Asi no se duplica el repuesto del inventario.
public class RepuestoUsado
{
    private Repuesto repuesto;
    private int cantidad;

    public RepuestoUsado(Repuesto repuesto, int cantidad)
    {
        if (repuesto == null)
            throw new Exception("El repuesto no existe.");
        if (cantidad <= 0)
            throw new Exception("La cantidad usada debe ser mayor que cero.");

        this.repuesto = repuesto;
        this.cantidad = cantidad;
    }

    public Repuesto Repuesto { get { return repuesto; } }
    public int Cantidad { get { return cantidad; } }

    public void AgregarCantidad(int cant)
    {
        if (cant <= 0)
            throw new Exception("La cantidad debe ser mayor que cero.");
        cantidad = cantidad + cant;
    }

    // Precio de venta por la cantidad usada
    public decimal Subtotal()
    {
        return repuesto.PrecioVenta * cantidad;
    }
}
