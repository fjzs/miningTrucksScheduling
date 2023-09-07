using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShiftUC.Solver.Modelacion;
using ShiftUC.Solver;

namespace SolverDemo
{
    public class SolucionAnterior
    {
        public Dictionary<Tupla, double> AsignaCamionPalaDestino;
        public Dictionary<Tupla, double> InstanteCamionStatusNodo;
        public Dictionary<Tupla, double> CamionCargaAntesEnPala;
        public Dictionary<Tupla, double> CamionDescargaAntesEnDestino;
        public Dictionary<Tupla, double> AsignaCamionAntesArco;

        public SolucionAnterior(
            Dictionary<Tupla, double> AsignaCamionPalaDestino,
            Dictionary<Tupla, double> InstanteCamionStatusNodo,
            Dictionary<Tupla, double> CamionCargaAntesEnPala,
            Dictionary<Tupla, double> CamionDescargaAntesEnDestino,
            Dictionary<Tupla, double> AsignaCamionAntesArco)
        {
            this.AsignaCamionPalaDestino = AsignaCamionPalaDestino;
            this.InstanteCamionStatusNodo = InstanteCamionStatusNodo;
            this.CamionCargaAntesEnPala = CamionCargaAntesEnPala;
            this.CamionDescargaAntesEnDestino = CamionDescargaAntesEnDestino;
            this.AsignaCamionAntesArco = AsignaCamionAntesArco;

        }
            
    }
}
