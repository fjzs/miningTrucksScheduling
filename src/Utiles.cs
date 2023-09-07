using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SolverDemo
{
    class Utiles
    {
        /// <summary>
        /// Escribe un parámetro 
        /// </summary>
        /// <param name="Valores"></param>
        /// <param name="NombreVariable"></param>
        /// <returns></returns>
        public static string EscribeParametro(Array Valores, string NombreVariable)
        {
            int Dimension = Valores.Rank;
            int[] Longitud = new int[Dimension];
            int capacidad = 1;
            for (int i = 0; i < Dimension; i++)
            {
                Longitud[i] = Valores.GetLength(i);
                capacidad *= Valores.GetLength(i);
            }
            StringBuilder salida = new StringBuilder(capacidad * 3);
            salida.Append("\n");
            salida.Append("\n");
            salida.Append("\n");
            salida.Append("param " + NombreVariable + ":");
            if (Dimension == 1)
            {

                salida.Append("=\n");
                for (int i = 0; i < Longitud[0]; i++)
                {
                    salida.Append(string.Format("{0}\t{1}\n", i + 1, Valores.GetValue(i).ToString().Replace(',', '.')));
                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 2)
            {
                salida.Append(string.Format("\n"));
                for (int i = 0; i < Longitud[1]; i++)
                {
                    salida.Append(string.Format("\t{0}", i + 1));

                }
                salida.Append(string.Format(":=\n"));
                for (int i = 0; i < Longitud[0]; i++)
                {
                    salida.Append(string.Format("{0}", i + 1));
                    for (int j = 0; j < Longitud[1]; j++)
                    {
                        salida.Append(string.Format("\t{0}", Valores.GetValue(i, j).ToString().Replace(',', '.')));
                    }
                    salida.Append(string.Format("\n"));
                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 3)
            {
                salida.Append(string.Format("=\n"));
                for (int k = 0; k < Longitud[2]; k++)
                {
                    salida.Append(string.Format("[*,*,{0}]:\t", k + 1));
                    for (int i = 0; i < Longitud[1]; i++)
                    {
                        salida.Append(string.Format("\t{0}", i + 1));
                    }
                    salida.Append(string.Format(":=\n"));
                    for (int i = 0; i < Longitud[0]; i++)
                    {
                        salida.Append(string.Format("{0}", i + 1));
                        for (int j = 0; j < Longitud[1]; j++)
                        {
                            salida.Append(string.Format("\t{0}", Valores.GetValue(i, j, k).ToString().Replace(',', '.')));
                        }
                        salida.Append(string.Format("\n"));
                    }

                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 4)
            {
                salida.Append(string.Format("=\n"));
                for (int z = 0; z < Longitud[3]; z++)
                {
                    for (int k = 0; k < Longitud[2]; k++)
                    {
                        salida.Append(string.Format("[*,*,{0},{1}]:\t", k + 1, z + 1));
                        for (int i = 0; i < Longitud[1]; i++)
                        {
                            salida.Append(string.Format("\t{0}", i + 1));
                        }
                        salida.Append(string.Format(":=\n"));
                        for (int i = 0; i < Longitud[0]; i++)
                        {
                            salida.Append(string.Format("{0}", i + 1));
                            for (int j = 0; j < Longitud[1]; j++)
                            {
                                salida.Append(string.Format("\t{0}", Valores.GetValue(i, j, k, z).ToString().Replace(',', '.')));
                            }
                            salida.Append(string.Format("\n"));
                        }

                    }
                }
                salida.Append(string.Format(";\n"));
            }



            //Se abre el archivo al final.

            return salida.ToString();

        }

        /// <summary>
        /// Método para la creación de archivos
        /// </summary>
        /// <param name="Ruta"></param>
        /// <param name="Contenido"></param>
        /// <returns></returns>
        public static bool CrearArchivoEnEstaCarpeta(string rutaCarpeta, string nombreArchivo, string Contenido)
        {
            try
            {
                if (!Directory.Exists(rutaCarpeta)) //Si no existe la carpeta la creo
                {
                    Directory.CreateDirectory(rutaCarpeta);
                }
                string rutaFinal = rutaCarpeta + "/" + nombreArchivo;
                StreamWriter archivo = new StreamWriter(rutaFinal);
                archivo.WriteLine(Contenido);
                archivo.Close();
                return true;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("No se pudo crear el archivo " + nombreArchivo);
                return false;
            }

        }

        /// <summary>
        /// Escribe un mensaje en la consola
        /// </summary>
        /// <param name="msg">El mensaje</param>
        /// <param name="categoria">La categoría. 1 para importante. 2 medio. 3 bajo</param>
        public static void EscribeMensajeConsola(string msg, CategoriaMensaje CM, int saltosAntes, int saltosDespues)
        {
            ConsoleColor color_actual = Console.ForegroundColor;

            string m = msg;
            if (CM == CategoriaMensaje.Categoria_1)
            {
                m = msg.ToUpper();
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (CM == CategoriaMensaje.Categoria_2)
            {
                m = msg.ToUpper();
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else if (CM == CategoriaMensaje.Categoria_3)
            {
                m = msg.ToUpper();
            }

            for (int i = 0; i < saltosAntes; i++)
            {
                Console.WriteLine();
            }
            Console.WriteLine(m);
            for (int i = 0; i < saltosDespues; i++)
            {
                Console.WriteLine();
            }

            Console.ForegroundColor = color_actual;
        }
        
        /// <summary>
        /// Formatea la hora
        /// </summary>
        /// <param name="instante_minutos"></param>
        /// <returns></returns>
        public static string FormatHora(double instante_minutos)
        {
            double instante_hora = instante_minutos / 60;

            int hora = (int)instante_hora;
            string H = hora.ToString();
            if (hora < 10)
            {
                H = "0" + H;
            }

            int minutos = (int)((instante_hora - hora) * 60);
            string M = minutos.ToString();
            if (minutos < 10)
            {
                M = "0" + M;
            }

            double x = (instante_hora - ((double)minutos / 60) - hora);
            int segundos = (int)(x * 3600);
            string S = segundos.ToString();
            if (segundos < 10)
            {
                S = "0" + S;
            }

            string m = H + ":" + M + ":" + S;
            return m;
        }

        /// <summary>
        /// Formatea un valor a algo mostrable en pesos
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static string FormatDoubleAPesos(double valor)
        {
            string resultado = "";
            valor = Math.Round(valor, 0);
            int numDigits = valor.ToString().Length;

            int i = numDigits-1;
            int pongoPunto = 0;
            while (i >= 0)
            {
                if (pongoPunto == 3)
                {
                    resultado = "." + resultado;
                    pongoPunto = 0;
                }
                pongoPunto++;
                char numero = valor.ToString()[i];
                resultado = numero + resultado;
                i--;
            }
            return resultado;
        }
    }
}
