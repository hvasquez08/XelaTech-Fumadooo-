using System.Globalization;
using System.Text;

namespace XelaTechSoporte;

class Program
{
    static void Main(string[] args)
    {
        // Para que se vean bien las tildes y la ñ
        Console.OutputEncoding = Encoding.UTF8;

        // Formato de Guatemala para fechas y numeros
        CultureInfo cultura = new CultureInfo("es-GT");
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.CurrentCulture = cultura;

        Menu menu = new Menu();

        // Si se ejecuta con el argumento "web" se abre directo el modo grafico
        // (sirve para trabajar el diseño de la interfaz en local)
        if (args.Length > 0 && args[0].ToLower() == "web")
        {
            menu.ModoGrafico();
        }

        menu.Iniciar();
    }
}
