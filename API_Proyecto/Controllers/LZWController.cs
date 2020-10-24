using System;
using System.Collections.Generic;
using System.IO;
using API_Proyecto.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibreriaGenericos;

namespace API_Proyecto.Controllers
{
    [Route("api")]
    [ApiController]
    public class LZWController : ControllerBase
    {
        public static CompresorLZW Compresor = new CompresorLZW();
        public static List<Compressions> ListArchivos = new List<Compressions>();
        public static Dictionary<string, string> DicCompress = new Dictionary<string, string>();
        public static Dictionary<string, string> DicDecompress = new Dictionary<string, string>();
        // GET: api/Huffman
        [HttpGet]
        public IEnumerable<string> Get()
        {
            CargarHistorial();
            return new string[] { "Jose Daniel Giron", "Cristian Josue Barrientos" };
        }
        void CargarHistorial()
        {
            string Historial = Path.GetFullPath("Datos LZW\\" + "Historial.txt");
            FileStream ArchivoHistotial = new FileStream(Historial, FileMode.OpenOrCreate);
            StreamReader reader = new StreamReader(ArchivoHistotial);
            string Texto = reader.ReadToEnd();
            string[] Archivos = Texto.Split('~');
            int Posicion = 0;
            if (Archivos.Length > 1)
            {
                while (Archivos.Length > Posicion + 1)
                {
                    Compressions NuevoArchivo = new Compressions();
                    // 0-> Nombre Original
                    if (Archivos[Posicion].Contains("\r\n"))
                        NuevoArchivo.NombreOriginal = Archivos[Posicion++].Substring(2);
                    else
                        NuevoArchivo.NombreOriginal = Archivos[Posicion++];
                    // 1 -> Peso de Original
                    int SizeOriginal = Convert.ToInt32(Archivos[Posicion++]);
                    // 2 -> Nombre Comprimido
                    NuevoArchivo.NombreComprimido = Archivos[Posicion++];
                    // 3 -> Peso Comprimido
                    int SizeComprimido = Convert.ToInt32(Archivos[Posicion++]);
                    DicCompress.Add(NuevoArchivo.NombreOriginal, NuevoArchivo.NombreComprimido);
                    DicDecompress.Add(NuevoArchivo.NombreComprimido, NuevoArchivo.NombreOriginal);
                    NuevoArchivo.RazonCompresion = (double)SizeComprimido / SizeOriginal;
                    NuevoArchivo.FactorCompresion = (double)SizeOriginal / SizeComprimido;
                    NuevoArchivo.PorcentajeReduccion = (double)NuevoArchivo.FactorCompresion * 100;
                    NuevoArchivo.RutaComprimido = Path.GetFullPath("Archivos Compress\\" + NuevoArchivo.NombreComprimido);

                    ListArchivos.Add(NuevoArchivo);
                }
            }
            reader.Close();
            ArchivoHistotial.Close();
        }
        // POST: api/compress/name
        [HttpPost("compress/{name}")]
        public IActionResult Compress([FromForm] IFormFile file, [FromRoute]string name)
        {
            try
            {
                Compressions NuevoArchivo = new Compressions();
                string RutaOriginal = Path.GetFullPath("Archivos Originales\\" + file.FileName);
                string RutaCompresion = Path.GetFullPath("Archivos Compress\\" + name + ".lzw");
                FileStream ArchivoOriginal = new FileStream(RutaOriginal, FileMode.OpenOrCreate);
                file.CopyTo(ArchivoOriginal);
                ArchivoOriginal.Close();
                Compresor.Comprimir(RutaOriginal, RutaCompresion);
                FileInfo Original = new FileInfo(RutaOriginal);
                FileInfo Comprimido = new FileInfo(RutaCompresion);
                NuevoArchivo.NombreOriginal = file.FileName;
                NuevoArchivo.NombreComprimido = name + ".lzw";
                if (!DicCompress.ContainsKey(NuevoArchivo.NombreOriginal))
                {
                    DicCompress.Add(NuevoArchivo.NombreOriginal, NuevoArchivo.NombreComprimido);
                    DicDecompress.Add(NuevoArchivo.NombreComprimido, NuevoArchivo.NombreOriginal);
                    NuevoArchivo.RazonCompresion = (double)Comprimido.Length / Original.Length;
                    NuevoArchivo.FactorCompresion = (double)Original.Length / Comprimido.Length;
                    NuevoArchivo.PorcentajeReduccion = (double)NuevoArchivo.FactorCompresion * 100;
                    NuevoArchivo.RutaComprimido = Path.GetFullPath("Archivos Compress\\" + NuevoArchivo.NombreComprimido);
                    ListArchivos.Add(NuevoArchivo);
                    string Historial = Path.GetFullPath("Datos LZW\\" + "Historial.txt");
                    StreamWriter writer = new StreamWriter(Historial, true);
                    string Texto = NuevoArchivo.NombreOriginal + "~" + Original.Length + "~" + NuevoArchivo.NombreComprimido + "~" + Comprimido.Length + "~";
                    writer.WriteLine(Texto);
                    writer.Close();
                }
                FileStream ArchivoFinal = new FileStream(RutaCompresion, FileMode.Open);
                FileStreamResult FileFinal = new FileStreamResult(ArchivoFinal, "text/lzw");
                return FileFinal;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/decompress
        [HttpPost("decompress")]
        public IActionResult Decompress([FromForm]  IFormFile file)
        {
            try
            {
                string RutaCompresion = Path.GetFullPath("Archivos Compress\\" + file.FileName);
                FileStream ArchivoCompresion = new FileStream(RutaCompresion, FileMode.OpenOrCreate);
                file.CopyTo(ArchivoCompresion);
                ArchivoCompresion.Close();
                string NombreOriginal = DicDecompress[file.FileName];
                string RutaDescompresion = Path.GetFullPath("Archivos Decompress\\" + NombreOriginal);
                Compresor.Descomprimir(RutaCompresion, RutaDescompresion);
                FileStream ArchivoFinal = new FileStream(RutaDescompresion, FileMode.Open);
                FileStreamResult FileFinal = new FileStreamResult(ArchivoFinal, "text/txt");
                return FileFinal;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // GET: api/compressions
        [HttpGet("compressions")]
        public IEnumerable<Compressions> Compressions()
        {
            return ListArchivos;
        }
    }
}
