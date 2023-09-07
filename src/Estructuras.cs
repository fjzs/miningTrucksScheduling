using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ShiftUC.Solver.Modelacion;
using ShiftUC.Solver;
using System.Configuration;

namespace SolverDemo
{
    public struct StatusArco
    { 
        public Arco arco;
        /// <summary>
        /// El status: 1= vacío. 2=cargado
        /// </summary>
        public int Status;

        public StatusArco(Arco A, int s)
        {
            arco = A;
            Status = s;
        }
    }

    public struct DoubleDouble
    {
        public double Double1;
        public double Double2;
        public DoubleDouble(double d1, double d2)
        {
            Double1 = d1;
            Double2 = d2;
        }
    }

    public struct InfoSimulaRuteo
    {
        public double DemoraTotal_min;
        public double TiempoOciosoPala_min;
        public double TiempoLLegadaAPala_min;
        public InfoSimulaRuteo(double W, double TO, double TiempoLLegoPala)
        {
            TiempoLLegadaAPala_min = TiempoLLegoPala;
            DemoraTotal_min = W;
            TiempoOciosoPala_min = TO;
        }
    }

    public struct CamionCiclo
    {
        public int IdCamion;
        public int Ciclo;
        public double InstanteInicio;
        public CamionCiclo(int idCamion, int ciclo, double instanteInicio)
        {
            IdCamion = idCamion;
            Ciclo = ciclo;
            InstanteInicio = instanteInicio;
        }
    }

    
}