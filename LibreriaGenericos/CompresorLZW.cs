using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace LibreriaGenericos
{
    public class CompresorLZW
    {
        FileStream ArchivoDestino;
        FileStream ArchivoOriginal;
        public void Comprimir(string RutaOriginal, string RutaDestino)
        {
            string Binario = "";
            string TextoComprimido = "";
            int Posicion = 0;
            int PosicionMayor = 0;
            string CadenaActual = "";
            string CadenaAnterior = "";
            string NuevaCadena = "";
            int PosicionByte;
            ArchivoDestino = new FileStream(RutaDestino, FileMode.OpenOrCreate);
            ArchivoOriginal = new FileStream(RutaOriginal, FileMode.OpenOrCreate);
            using var reader = new BinaryReader(ArchivoOriginal);
            var buffer = new byte[500];
            Dictionary<string, int> DicByteDatoMayor = new Dictionary<string, int>();
            Dictionary<string, int> DicByte = new Dictionary<string, int>();
            Dictionary<int, string> DicValores = new Dictionary<int, string>();
            List<byte> ListaByte = new List<byte>();
            while (ArchivoOriginal.Position < ArchivoOriginal.Length)
            {
                buffer = reader.ReadBytes(500);
                foreach (var Item in buffer)
                {
                    Binario = Convert.ToString(Item, 2).PadLeft(8, '0');
                    if (!DicByte.ContainsKey(Binario))
                    {
                        DicByte.Add(Binario, ++Posicion);
                        DicByteDatoMayor.Add(Binario, Posicion);
                        DicValores.Add(Posicion, Binario);
                        ListaByte.Add(Convert.ToByte(Binario, 2));
                    }
                }
            }
            ArchivoOriginal.Seek(0, SeekOrigin.Begin);
            MetaData(ArchivoDestino, ListaByte);
            int PosInicial = Posicion;
            int NumeroByte = 0;
            //Obtener el Dato Mayor
            while (ArchivoOriginal.Position < ArchivoOriginal.Length)
            {
                buffer = reader.ReadBytes(500);
                foreach (var Item in buffer)
                {
                    CadenaActual = Convert.ToString(Item, 2).PadLeft(8, '0');
                    NuevaCadena = CadenaAnterior + CadenaActual;
                    if (!DicByteDatoMayor.ContainsKey(NuevaCadena))
                    {
                        DicByteDatoMayor.Add(NuevaCadena, ++PosInicial);
                        NumeroByte = DicByteDatoMayor[CadenaAnterior];
                        if (NumeroByte > PosicionMayor)
                            PosicionMayor = NumeroByte;
                        CadenaAnterior = CadenaActual;
                    }
                    else
                        CadenaAnterior = NuevaCadena;
                }
            }
            ArchivoOriginal.Seek(0, SeekOrigin.Begin);
            int AutoCompletar = Convert.ToString(PosicionMayor, 2).Length;
            byte[] BufferW = new byte[800];
            int PosVector = 0;
            TextoComprimido = "";
            CadenaActual = "";
            CadenaAnterior = "";
            NuevaCadena = "";
            PosicionByte = 0;
            while (ArchivoOriginal.Position < ArchivoOriginal.Length)
            {
                buffer = reader.ReadBytes(500);
                foreach (var Item in buffer)
                {
                    CadenaActual = Convert.ToString(Item, 2).PadLeft(8, '0');
                    NuevaCadena = CadenaAnterior + CadenaActual;
                    if (!DicByte.ContainsKey(NuevaCadena))
                    {
                        DicByte.Add(NuevaCadena, ++Posicion);
                        DicValores.Add(Posicion, NuevaCadena);
                        //Imprimir Cadena Anterior
                        PosicionByte = DicByte[CadenaAnterior];
                        TextoComprimido += Convert.ToString(PosicionByte, 2).PadLeft(AutoCompletar, '0');
                        while (TextoComprimido.Length >= 8)
                        {
                            BufferW[PosVector++] = Convert.ToByte(TextoComprimido.Substring(0, 8), 2);
                            TextoComprimido = TextoComprimido.Substring(8);
                        }
                        ArchivoDestino.Write(BufferW, 0, PosVector);
                        PosVector = 0;
                        CadenaAnterior = CadenaActual;
                    }
                    else
                        CadenaAnterior = NuevaCadena;
                }
            }
            if (DicByte.ContainsKey(CadenaAnterior))
            {
                PosicionByte = DicByte[CadenaAnterior];
                TextoComprimido += Convert.ToString(PosicionByte, 2).PadLeft(AutoCompletar, '0');
            }
            while (TextoComprimido != "")
            {
                if (TextoComprimido.Length >= 8)
                {
                    BufferW[PosVector++] = Convert.ToByte(TextoComprimido.Substring(0, 8), 2);
                    TextoComprimido = TextoComprimido.Substring(8);
                }
                else
                {
                    BufferW[PosVector++] = Convert.ToByte(TextoComprimido.PadRight(8, '0'), 2);
                    TextoComprimido = "";
                }
            }
            ArchivoDestino.Write(BufferW, 0, PosVector);
            byte[] ByteMayor = BitConverter.GetBytes(AutoCompletar);
            ArchivoDestino.Seek(4, SeekOrigin.Begin);
            ArchivoDestino.Write(ByteMayor, 0, 4);
            ArchivoDestino.Flush();
            reader.Close();
            ArchivoOriginal.Close();
            ArchivoDestino.Close();
        }

        private void MetaData(FileStream archivoDestino, List<byte> dicByte)
        {
            byte[] Metadatos = new byte[dicByte.Count + 8];
            byte[] ByteMeta = BitConverter.GetBytes(Metadatos.Length - 8);
            for (int i = 0; i < ByteMeta.Length; i++)
                Metadatos[i] = ByteMeta[i];
            for (int i = 0; i < dicByte.Count; i++)
                Metadatos[i + 8] = dicByte[i];
            archivoDestino.Write(Metadatos);
            archivoDestino.Flush();
        }
    }
}
