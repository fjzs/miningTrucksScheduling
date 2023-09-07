using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ShiftUC.Solver.Modelacion;
using ShiftUC.Solver;

namespace SolverDemo
{
    /// <summary>
    /// Es la clase que modela las variables del modelo en CPLEX
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Lista de variables indexadas que se usan en un método de solución
        /// </summary>
        private static Dictionary<MetodoSolucion, List<NombreVariablesIndexadas>> _nombresVariablesIndexadas_SegunMS;
        /// <summary>
        /// Lista de variables escalares que se usan en un método de solución
        /// </summary>
        private static Dictionary<MetodoSolucion, List<NombreVariablesEscalares>> _nombresVariablesEscalares_SegunMS;
        /// <summary>
        /// Lista de variables indexadas que se usan en un método de solución
        /// </summary>
        public static Dictionary<MetodoSolucion,List<NombreVariablesIndexadas>> NombresVariablesIndexadas_SegunMS
        {
            get { return _nombresVariablesIndexadas_SegunMS; }
        }
        /// <summary>
        /// Lista de variables escalares que se usan en un método de solución
        /// </summary>
        public static Dictionary<MetodoSolucion,List<NombreVariablesEscalares>> NombresVariablesEscalares_SegunMS
        {
            get { return _nombresVariablesEscalares_SegunMS; }
        }
        #region Métodos
        
        /// <summary>
        /// Método que escribe los datos de un arreglo en un string.
        /// </summary>
        /// <param name="arreglo">Arreglo que se escribirá en el .dat</param>
        /// <param name="nombre">Nombre de la variable</param>
        /// <returns></returns>
        public static string EscribeArregloEnString(Array arreglo, string nombre)
        {
            int Dimension = arreglo.Rank;
            int[] Longitud = new int[Dimension];
            int capacidad = 1;
            for (int i = 0; i < Dimension; i++)
            {
                Longitud[i] = arreglo.GetLength(i);
                capacidad *= arreglo.GetLength(i);
            }
            StringBuilder salida = new StringBuilder(capacidad * 3);
            salida.Append("\n");
            salida.Append("\n");
            salida.Append("\n");
            salida.Append("param " + nombre + ":");
            if (Dimension == 1)
            {

                salida.Append("=\n");
                for (int i = 0; i < Longitud[0]; i++)
                {
                    salida.Append(string.Format("{0}\t{1}\n", i + 1, arreglo.GetValue(i).ToString().Replace(',', '.')));
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
                        salida.Append(string.Format("\t{0}", arreglo.GetValue(i, j).ToString().Replace(',', '.')));
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
                            salida.Append(string.Format("\t{0}", arreglo.GetValue(i, j, k).ToString().Replace(',', '.')));
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
                                salida.Append(string.Format("\t{0}", arreglo.GetValue(i, j, k, z).ToString().Replace(',', '.')));
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
        /// Método que escribe los datos de un arreglo en un string.
        /// </summary>
        /// <param name="arreglo">Arreglo que se escribirá en el .dat</param>
        /// <param name="nombre">Nombre de la variable</param>
        /// <param name="inicio_indices">Indices de inicio para imprimir. Es para indexar valores desde números mayores que 1</param>
        /// <returns></returns>
        public static string EscribeArregloEnString(Array arreglo, string nombre, List<int> inicio_indices)
        {
            int Dimension = arreglo.Rank;
            int[] Longitud = new int[Dimension];
            int capacidad = 1;
            for (int i = 0; i < Dimension; i++)
            {
                Longitud[i] = arreglo.GetLength(i);
                capacidad *= arreglo.GetLength(i);
            }
            StringBuilder salida = new StringBuilder(capacidad * 3);
            salida.Append("\n");
            salida.Append("\n");
            salida.Append("\n");
            salida.Append("param " + nombre + ":");
            if (Dimension == 1)
            {
                salida.Append("=\n");
                for (int i = 0; i < Longitud[0]; i++)
                {
                    salida.Append(string.Format("{0}\t{1}\n", i + inicio_indices[0] + 1, arreglo.GetValue(i).ToString().Replace(',', '.')));
                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 2)
            {
                salida.Append(string.Format("\n"));
                for (int i = 0; i < Longitud[1]; i++)
                {
                    salida.Append(string.Format("\t{0}", i + 1 + inicio_indices[1]));

                }
                salida.Append(string.Format(":=\n"));
                for (int i = 0; i < Longitud[0]; i++)
                {
                    salida.Append(string.Format("{0}", i + 1 + inicio_indices[0]));
                    for (int j = 0; j < Longitud[1]; j++)
                    {
                        salida.Append(string.Format("\t{0}", arreglo.GetValue(i, j).ToString().Replace(',', '.')));
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
                    salida.Append(string.Format("[*,*,{0}]:\t", k + 1 + inicio_indices[2]));
                    for (int i = 0; i < Longitud[1]; i++)
                    {
                        salida.Append(string.Format("\t{0}", i + 1 + inicio_indices[1]));
                    }
                    salida.Append(string.Format(":=\n"));
                    for (int i = 0; i < Longitud[0]; i++)
                    {
                        salida.Append(string.Format("{0}", i + 1 + inicio_indices[0]));
                        for (int j = 0; j < Longitud[1]; j++)
                        {
                            salida.Append(string.Format("\t{0}", arreglo.GetValue(i, j, k).ToString().Replace(',', '.')));
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
                        salida.Append(string.Format("[*,*,{0},{1}]:\t", k + 1 + inicio_indices[2], z + 1 + inicio_indices[3]));
                        for (int i = 0; i < Longitud[1]; i++)
                        {
                            salida.Append(string.Format("\t{0}", i + 1 + inicio_indices[1]));
                        }
                        salida.Append(string.Format(":=\n"));
                        for (int i = 0; i < Longitud[0]; i++)
                        {
                            salida.Append(string.Format("{0}", i + 1 + inicio_indices[0]));
                            for (int j = 0; j < Longitud[1]; j++)
                            {
                                salida.Append(string.Format("\t{0}", arreglo.GetValue(i, j, k, z).ToString().Replace(',', '.')));
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
        /// Método que escribe los datos de un arreglo en un string.
        /// </summary>
        /// <param name="variable">Arreglo que se escribirá en el .dat</param>
        /// <param name="nombre">Nombre de la variable</param>
        /// <returns></returns>
        public static string EscribeVariableEnString(VariableIndexada variable, string nombre, TIPO_VARIABLE tipo)
        {
            if (variable == null)
            {
                return "param " + nombre + ":=;";
            }
            string vacio = ".";
            int Dimension = variable.Rango;
            int[] Longitud = new int[Dimension];
            int capacidad = 1;
            for (int i = 0; i < Dimension; i++)
            {
                //HARDCODE
                //Longitud[i] = variable.ProyectarTuplas(i).Count;
                //capacidad *= variable.ProyectarTuplas(i).Count;
            }
            Dictionary<int, List<int>> keys = new Dictionary<int, List<int>>();
            for (int i = 0; i < Longitud.Length; i++)
            {
                keys.Add(i, new List<int>());
            }

            foreach (Tupla t in variable.Claves) //HARDCODE. ANTES ERA variable.Tuplas
            {
                for (int i = 0; i < Longitud.Length; i++)
                {
                    if (!keys[i].Contains(Convert.ToInt32(t[i].ToString())))
                    {
                        keys[i].Add(Convert.ToInt32(t[i].ToString()));
                    }
                }
            }
            foreach (int index in keys.Keys)
            {
                keys[index].Sort();
            }
            StringBuilder salida = new StringBuilder(capacidad * 3);
            salida.Append("\n");
            salida.Append("\n");
            salida.Append("\n");
            salida.Append("param " + nombre + ":");
            if (Dimension == 1)
            {
                salida.Append("=\n");
                foreach (int val1 in keys[0])
                {
                    string valor = vacio;
                    /*
                    if (variable.ExisteTupla(val1))
                    {
                        valor = RedondeaVariable(tipo, variable[val1].ObtenerValor()).ToString().Replace(',', '.');
                    }*/
                    salida.Append(string.Format("{0}\t{1}\n", val1, valor));
                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 2)
            {
                salida.Append(string.Format("\n"));
                foreach (int val2 in keys[1])
                {
                    salida.Append(string.Format("\t{0}", val2));
                }
                salida.Append(string.Format(":=\n"));
                foreach (int val1 in keys[0])
                {
                    salida.Append(string.Format("{0}", val1));
                    foreach (int val2 in keys[1])
                    {
                        string valor = vacio;
                        /*
                        if (variable.ExisteTupla(val1, val2))
                        {
                            valor = RedondeaVariable(tipo, variable[val1, val2].ObtenerValor()).ToString().Replace(',', '.');
                        }*/
                        salida.Append(string.Format("\t{0}", valor));
                    }
                    salida.Append(string.Format("\n"));
                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 3)
            {
                salida.Append(string.Format("=\n"));
                foreach (int val3 in keys[2])
                {
                    salida.Append(string.Format("[*,*,{0}]:\t", val3));
                    foreach (int val2 in keys[1])
                    {
                        salida.Append(string.Format("\t{0}", val2));
                    }
                    salida.Append(string.Format(":=\n"));
                    foreach (int val1 in keys[0])
                    {
                        salida.Append(string.Format("{0}", val1));
                        foreach (int val2 in keys[1])
                        {
                            string valor = vacio;/*
                            if (variable.ExisteTupla(val1, val2, val3))
                            {
                                valor = RedondeaVariable(tipo, variable[val1, val2, val3].ObtenerValor()).ToString().Replace(',', '.');
                            }*/
                            salida.Append(string.Format("\t{0}", valor));
                        }
                        salida.Append(string.Format("\n"));
                    }
                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 4)
            {
                salida.Append(string.Format("=\n"));
                foreach (int val4 in keys[3])
                {
                    foreach (int val3 in keys[2])
                    {
                        salida.Append(string.Format("[*,*,{0},{1}]:\t", val3, val4));
                        foreach (int val2 in keys[1])
                        {
                            salida.Append(string.Format("\t{0}", val2));
                        }
                        salida.Append(string.Format(":=\n"));
                        foreach (int val1 in keys[0])
                        {
                            salida.Append(string.Format("{0}", val1));
                            foreach (int val2 in keys[1])
                            {
                                string valor = vacio;/*
                                if (variable.ExisteTupla(val1, val2, val3, val4))
                                {
                                    valor = RedondeaVariable(tipo, variable[val1, val2, val3, val4].ObtenerValor()).ToString().Replace(',', '.');
                                }*/
                                salida.Append(string.Format("\t{0}", valor));
                            }
                            salida.Append(string.Format("\n"));
                        }
                    }
                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 5)
            {
                salida.Append(string.Format("=\n"));
                foreach (int val5 in keys[4])
                {
                    foreach (int val4 in keys[3])
                    {
                        foreach (int val3 in keys[2])
                        {
                            salida.Append(string.Format("[*,*,{0},{1},{2}]:\t", val3, val4, val5));
                            foreach (int val2 in keys[1])
                            {
                                salida.Append(string.Format("\t{0}", val2));
                            }
                            salida.Append(string.Format(":=\n"));
                            foreach (int val1 in keys[0])
                            {
                                salida.Append(string.Format("{0}", val1));
                                foreach (int val2 in keys[1])
                                {
                                    string valor = vacio;/*
                                    if (variable.ExisteTupla(val1, val2, val3, val4, val5))
                                    {
                                        valor = RedondeaVariable(tipo, variable[val1, val2, val3, val4, val5].ObtenerValor()).ToString().Replace(',', '.');
                                    }*/
                                    salida.Append(string.Format("\t{0}", valor));
                                }
                                salida.Append(string.Format("\n"));
                            }
                        }
                    }
                }
                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 6)
            {
                salida.Append(string.Format("=\n"));
                foreach (int val6 in keys[5])
                {
                    foreach (int val5 in keys[4])
                    {
                        foreach (int val4 in keys[3])
                        {
                            foreach (int val3 in keys[2])
                            {
                                salida.Append(string.Format("[*,*,{0},{1},{2},{3}]:\t", val3, val4, val5, val6));
                                foreach (int val2 in keys[1])
                                {
                                    salida.Append(string.Format("\t{0}", val2));
                                }
                                salida.Append(string.Format(":=\n"));
                                foreach (int val1 in keys[0])
                                {
                                    salida.Append(string.Format("{0}", val1));
                                    foreach (int val2 in keys[1])
                                    {
                                        string valor = vacio;/*
                                        if (variable.ExisteTupla(val1, val2, val3, val4, val5, val6))
                                        {
                                            valor = RedondeaVariable(tipo, variable[val1, val2, val3, val4, val5, val6].ObtenerValor()).ToString().Replace(',', '.');
                                        }*/
                                        salida.Append(string.Format("\t{0}", valor));
                                    }
                                    salida.Append(string.Format("\n"));
                                }
                            }
                        }
                    }
                }

                salida.Append(string.Format(";\n"));
            }
            else if (Dimension == 7)
            {
                salida.Append(string.Format("=\n"));
                foreach (int val7 in keys[6])
                {
                    foreach (int val6 in keys[5])
                    {
                        foreach (int val5 in keys[4])
                        {
                            foreach (int val4 in keys[3])
                            {
                                foreach (int val3 in keys[2])
                                {
                                    salida.Append(string.Format("[*,*,{0},{1},{2},{3},{4}]:\t", val3, val4, val5, val6, val7));
                                    foreach (int val2 in keys[1])
                                    {
                                        salida.Append(string.Format("\t{0}", val2));
                                    }
                                    salida.Append(string.Format(":=\n"));
                                    foreach (int val1 in keys[0])
                                    {
                                        salida.Append(string.Format("{0}", val1));
                                        foreach (int val2 in keys[1])
                                        {
                                            string valor = vacio;/*
                                            if (variable.ExisteTupla(val1, val2, val3, val4, val5, val6, val7))
                                            {
                                                valor = RedondeaVariable(tipo, variable[val1, val2, val3, val4, val5, val6, val7].ObtenerValor()).ToString().Replace(',', '.');
                                            }*/
                                            salida.Append(string.Format("\t{0}", valor));
                                        }
                                        salida.Append(string.Format("\n"));
                                    }
                                }
                            }
                        }
                    }
                }
                salida.Append(string.Format(";\n"));
            }
            return salida.ToString();

        }
        /// <summary>
        /// Redondea las variables segun su tipo y tolerancia
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static double RedondeaVariable(TIPO_VARIABLE tipo, double valor)
        {
            /* Código Original
            double tolerancia_entera = 0.1;
            switch (tipo)
            {
                case TIPO_VARIABLE.ENTERA:
                    if (Math.Abs(valor - Math.Round(valor)) < tolerancia_entera)
                    {
                        return Math.Round(valor);
                    }
                    return valor;
                default:
                    return valor;
            }
            */

            double tolerancia_entera = 0.1;
            int deciles_redondeo_continua = 4;
            switch (tipo)
            {
                case TIPO_VARIABLE.ENTERA:
                    if (Math.Abs(valor - Math.Round(valor)) < tolerancia_entera)
                    {
                        return Math.Round(valor);
                    }
                    return valor;
                default:
                    double valor_redondeado = Math.Round(valor, deciles_redondeo_continua);
                    return valor_redondeado;
            }
        }
        /// <summary>
        /// Inicializa los nombres de las variables
        /// </summary>
        /// <param name="MS">El método de solución</param>
        public static void InicializaVariablesPorMetodo(List<MetodoSolucion> Metodos)
        {
            _nombresVariablesIndexadas_SegunMS = new Dictionary<MetodoSolucion, List<NombreVariablesIndexadas>>();
            _nombresVariablesEscalares_SegunMS = new Dictionary<MetodoSolucion, List<NombreVariablesEscalares>>();

            List<NombreVariablesIndexadas> VarIndexComunes = new List<NombreVariablesIndexadas>();
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionPala);
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionPalaDestino);
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionDestino);
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionStatusArco);
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionTWPala);
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionTWDestino);
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionAntesTwArco);
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionDespuesTwArco);
            VarIndexComunes.Add(NombreVariablesIndexadas.InstanteCamionStatusNodo);
            VarIndexComunes.Add(NombreVariablesIndexadas.DemoraEnArco_Minutos);
            VarIndexComunes.Add(NombreVariablesIndexadas.InstanteSalidaDestino);
            VarIndexComunes.Add(NombreVariablesIndexadas.EsperaEnColaPala_Minutos);
            VarIndexComunes.Add(NombreVariablesIndexadas.EsperaEnColaDestino_Minutos);           
            VarIndexComunes.Add(NombreVariablesIndexadas.TiempoDeCiclo);
            
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionStatusRuta);
            VarIndexComunes.Add(NombreVariablesIndexadas.CamionCargaAntesEnPala); 
            VarIndexComunes.Add(NombreVariablesIndexadas.CamionDescargaAntesEnDestino); 
            VarIndexComunes.Add(NombreVariablesIndexadas.AsignaCamionAntesArco); 

            List<NombreVariablesEscalares> VarEscalaresComunes = new List<NombreVariablesEscalares>();
            VarEscalaresComunes.Add(NombreVariablesEscalares.FO_CostosPorTiempoDeCiclo);
            VarEscalaresComunes.Add(NombreVariablesEscalares.FO_CostosDeTransporte);
            VarEscalaresComunes.Add(NombreVariablesEscalares.FO_CostosPorDemora);
            VarEscalaresComunes.Add(NombreVariablesEscalares.FO_CostosPorToneladasFaltantes);
            

            foreach (MetodoSolucion MS in Metodos)
            {
                _nombresVariablesIndexadas_SegunMS.Add(MS, new List<NombreVariablesIndexadas>(VarIndexComunes));
                _nombresVariablesEscalares_SegunMS.Add(MS, new List<NombreVariablesEscalares>(VarEscalaresComunes));
            }
        }

        #endregion
    }
}
