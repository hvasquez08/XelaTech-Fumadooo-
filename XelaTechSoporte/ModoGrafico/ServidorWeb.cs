using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace XelaTechSoporte;

// Servidor web local (solo en esta computadora) para el modo grafico.
// - Las paginas HTML, CSS y JS estan en la carpeta wwwroot.
// - Las rutas que empiezan con /api/ llaman a ApiWeb, que usa los mismos gestores de la consola.
// No se usa base de datos: todo sigue en las listas en memoria.
public class ServidorWeb
{
    public const string DIRECCION = "http://localhost:5000/";

    private HttpListener listener;
    private Thread hilo;
    private bool activo;
    private string carpetaWeb;
    private ApiWeb api;

    public ServidorWeb(ApiWeb api)
    {
        this.api = api;
        carpetaWeb = BuscarCarpetaWeb();
    }

    public bool Activo { get { return activo; } }
    public string CarpetaWeb { get { return carpetaWeb; } }

    public void Iniciar()
    {
        if (!Directory.Exists(carpetaWeb))
            throw new Exception("No se encontró la carpeta de la interfaz: " + carpetaWeb);

        listener = new HttpListener();
        listener.Prefixes.Add(DIRECCION);
        listener.Start();
        activo = true;

        // Las peticiones se atienden en otro hilo para que la consola pueda esperar el ENTER
        hilo = new Thread(Escuchar);
        hilo.IsBackground = true;
        hilo.Start();
    }

    public void Detener()
    {
        activo = false;
        try
        {
            listener.Stop();
            listener.Close();
        }
        catch (Exception)
        {
            // Ya estaba detenido
        }
    }

    public void AbrirNavegador()
    {
        try
        {
            ProcessStartInfo info = new ProcessStartInfo(DIRECCION);
            info.UseShellExecute = true;
            Process.Start(info);
        }
        catch (Exception)
        {
            Console.WriteLine("  (No se pudo abrir el navegador automáticamente)");
        }
    }

    // Busca la carpeta wwwroot del proyecto (subiendo desde bin/Debug/net10.0).
    // Asi, si se edita el HTML o el CSS, el cambio se ve con solo recargar la pagina.
    private string BuscarCarpetaWeb()
    {
        DirectoryInfo carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta != null)
        {
            if (File.Exists(Path.Combine(carpeta.FullName, "XelaTechSoporte.csproj")))
            {
                return Path.Combine(carpeta.FullName, "wwwroot");
            }
            carpeta = carpeta.Parent;
        }
        // Si no se encuentra el proyecto (por ejemplo, solo el .exe) se usa la copia junto al ejecutable
        return Path.Combine(AppContext.BaseDirectory, "wwwroot");
    }

    private void Escuchar()
    {
        while (activo)
        {
            try
            {
                HttpListenerContext contexto = listener.GetContext();
                Atender(contexto);
            }
            catch (Exception)
            {
                // Cuando se detiene el servidor GetContext lanza una excepcion y se sale del ciclo
                if (!activo) break;
            }
        }
    }

    private void Atender(HttpListenerContext contexto)
    {
        string ruta = contexto.Request.Url.AbsolutePath;

        if (ruta.StartsWith("/api/"))
        {
            string cuerpo = LeerCuerpo(contexto.Request);
            Dictionary<string, string> datos = ConvertirDatos(cuerpo);

            object respuesta = api.Procesar(ruta, datos);
            string json = JsonSerializer.Serialize(respuesta);
            EnviarTexto(contexto.Response, json, "application/json; charset=utf-8", 200);

            // La pagina pidio cerrar el modo grafico
            if (ruta == "/api/salir")
            {
                Detener();
            }
        }
        else
        {
            EnviarArchivo(contexto.Response, ruta);
        }
    }

    private string LeerCuerpo(HttpListenerRequest peticion)
    {
        if (!peticion.HasEntityBody) return "";
        using (StreamReader lector = new StreamReader(peticion.InputStream, Encoding.UTF8))
        {
            return lector.ReadToEnd();
        }
    }

    // La pagina siempre manda un objeto JSON con valores de texto: { "nombre": "Ana", ... }
    private Dictionary<string, string> ConvertirDatos(string cuerpo)
    {
        if (string.IsNullOrWhiteSpace(cuerpo))
            return new Dictionary<string, string>();

        try
        {
            Dictionary<string, string> datos = JsonSerializer.Deserialize<Dictionary<string, string>>(cuerpo);
            if (datos == null) return new Dictionary<string, string>();
            return datos;
        }
        catch (Exception)
        {
            return new Dictionary<string, string>();
        }
    }

    private void EnviarArchivo(HttpListenerResponse respuesta, string ruta)
    {
        if (ruta == "/") ruta = "/index.html";

        // Ruta completa del archivo dentro de wwwroot
        string relativa = Uri.UnescapeDataString(ruta.TrimStart('/')).Replace('/', Path.DirectorySeparatorChar);
        string archivo = Path.GetFullPath(Path.Combine(carpetaWeb, relativa));

        // Seguridad: no permitir salir de la carpeta wwwroot (por ejemplo con ../)
        string raiz = Path.GetFullPath(carpetaWeb) + Path.DirectorySeparatorChar;
        if (!archivo.StartsWith(raiz) || !File.Exists(archivo))
        {
            EnviarTexto(respuesta, "<h1>404 - Página no encontrada</h1>", "text/html; charset=utf-8", 404);
            return;
        }

        byte[] contenido = File.ReadAllBytes(archivo);
        respuesta.StatusCode = 200;
        respuesta.ContentType = TipoDeArchivo(archivo);
        // Sin cache, para que los cambios al diseño se vean al recargar
        respuesta.Headers.Add("Cache-Control", "no-store");
        respuesta.ContentLength64 = contenido.Length;
        respuesta.OutputStream.Write(contenido, 0, contenido.Length);
        respuesta.OutputStream.Close();
    }

    private void EnviarTexto(HttpListenerResponse respuesta, string texto, string tipo, int codigo)
    {
        byte[] contenido = Encoding.UTF8.GetBytes(texto);
        respuesta.StatusCode = codigo;
        respuesta.ContentType = tipo;
        respuesta.Headers.Add("Cache-Control", "no-store");
        respuesta.ContentLength64 = contenido.Length;
        respuesta.OutputStream.Write(contenido, 0, contenido.Length);
        respuesta.OutputStream.Close();
    }

    private string TipoDeArchivo(string archivo)
    {
        string extension = Path.GetExtension(archivo).ToLower();
        switch (extension)
        {
            case ".html": return "text/html; charset=utf-8";
            case ".css": return "text/css; charset=utf-8";
            case ".js": return "application/javascript; charset=utf-8";
            case ".svg": return "image/svg+xml";
            case ".png": return "image/png";
            case ".jpg": return "image/jpeg";
            case ".ico": return "image/x-icon";
            default: return "application/octet-stream";
        }
    }
}
