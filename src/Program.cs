using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ShiftUC.Solver;

namespace SolverDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Formulario F = new Formulario();
            MinaCieloAbierto MCA = new MinaCieloAbierto();
        }

        static void Original()
        {
            // Leer datos y generar archivo .DAT
            int m = 3;
            int n = 3;
            double[,] A = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    A[i, j] = i + j + 1;
                }
            }
            double[] b = new double[m];
            for (int i = 0; i < m; i++)
            {
                b[i] = i + 1;
            }
            double[] c = new double[n];
            for (int j = 0; j < n; j++)
            {
                c[j] = j + 1;
            }


            StringBuilder output = new StringBuilder();
            output.AppendLine("param m :=" + m + ";");
            output.AppendLine("param n :=" + n + ";");
            output.AppendLine(Utiles.EscribeParametro(A, "A_Actual"));
            output.AppendLine(Utiles.EscribeParametro(b, "b"));
            output.AppendLine(Utiles.EscribeParametro(c, "c"));

            using (StreamWriter sw = new StreamWriter("Datos.dat", false, Encoding.GetEncoding("iso-8859-1")))
            {
                sw.WriteLine(output.ToString());
            }

            // ResolverDespacho
            ProblemaSolver problema = new ProblemaSolver("Modelo.mod", "Datos.dat");
            using (SolverFrontend solver = new SolverFrontend("SolverBackend_Cplex.dll"))
            {
                solver.ResolverProblema(problema);
            }

            Console.WriteLine("*****************************");

            if (problema.Resuelto)
            {
                Console.WriteLine("SOLUCION");
                VariableIndexada x = problema.ObtenerVariableIndexada("x");
                foreach (Tupla t in x.Claves)
                {
                    Console.WriteLine("x{0} = {1}", t, x[t].ObtenerValor());
                }
            }
            else
            {
                Console.WriteLine("El problema no pudo resolverse.");
            }

            Console.ReadKey();
        }
    }
}
