using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShiftUC.Solver.Modelacion;
using ShiftUC.Solver;

namespace SolverDemo
{
    /// <summary>
    /// Modela el resultado de un problema de optimización
    /// </summary>
    public class Resultado
    {
        #region Atributos
        /// <summary>
        /// Diccionario con las variables indexadas
        /// </summary>
        private Dictionary<string, VariableIndexada> _variablesIndexadas;
        /// <summary>
        /// Diccionario con las variables escalares
        /// </summary>
        private Dictionary<string, VariableEscalar> _variablesEscalares;
        /// <summary>
        /// Lista con los nombres de las variables indexadas asociadas a este resultado
        /// </summary>
        private List<NombreVariablesIndexadas> _nombreVariablesIndexadas;
        /// <summary>
        /// Lista con los nombres de las variables escalares asociadas a este resultado
        /// </summary>
        private List<NombreVariablesEscalares> _nombreVariablesEscalares;
        /// <summary>
        /// El tiempo que tomó en resolver en segundos
        /// </summary>
        private double _tiempoResolucion; 
        #endregion

        /// <summary>
        /// El tiempo que tomó en resolver en segundos
        /// </summary>
        public double TiempoResolucion
        {
            get { return _tiempoResolucion; }
        }

        #region Métodos
        /// <summary>
        /// Genera un resultado de un problema de optimzación
        /// </summary>
        /// <param name="PS">El objeto ProblemaSolver</param>
        /// <param name="_nombreVariablesIndexadas">La lista con los nombres de las variables indexadas del probema</param>
        /// <param name="_nombreVariablesEscalares">La lista con los nombres de las variables escalares del probema</param>
        public Resultado(ProblemaSolver PS, double tiempoResolucion_s, List<NombreVariablesIndexadas> nombreVariablesIndexadas, List<NombreVariablesEscalares> nombreVariablesEscalares)
        {
            _tiempoResolucion = tiempoResolucion_s;
            _variablesIndexadas = new Dictionary<string, VariableIndexada>();
            List<NombreVariablesIndexadas> ListaIndex = new List<NombreVariablesIndexadas>();
            foreach (NombreVariablesIndexadas NVI in nombreVariablesIndexadas)
            {
                string nombre = NVI.ToString();
                try
                {
                    VariableIndexada VI = PS.ObtenerVariableIndexada(nombre);
                    _variablesIndexadas.Add(VI.Nombre, VI);
                    ListaIndex.Add(NVI);
                }
                catch (Exception e)
                {
                    //Utiles.EscribeMensajeConsola("La variable " + NVI.ToString() + " no estaba en el problema", CategoriaMensaje.Categoria_4, 0, 0);
                }
            }
            _nombreVariablesIndexadas = ListaIndex;


            _variablesEscalares = new Dictionary<string, VariableEscalar>();
            _nombreVariablesEscalares = new List<NombreVariablesEscalares>(nombreVariablesEscalares);
            foreach (NombreVariablesEscalares NVE in _nombreVariablesEscalares)
            {
                string nombre = NVE.ToString();
                VariableEscalar VE = PS.ObtenerVariableEscalar(nombre);
                _variablesEscalares.Add(VE.Nombre, VE);
            }
            

        }
        /// <summary>
        /// Retorna los valores de la variable indicada en un diccionario
        /// </summary>
        /// <param name="NombreVariable">El nombre de la variable</param>
        /// <param name="Tipo">El tipo: Entero o continuo</param>
        /// <returns></returns>
        public Dictionary<Tupla,double> GetValoresVariableIndexada(NombreVariablesIndexadas NombreVariable, TIPO_VARIABLE Tipo)
        {
            Dictionary<Tupla, double> valores = new Dictionary<Tupla, double>();
            if (_nombreVariablesIndexadas.Contains(NombreVariable) && _variablesIndexadas.ContainsKey(NombreVariable.ToString()))
            {
                VariableIndexada VI = _variablesIndexadas[NombreVariable.ToString()];
                foreach (Tupla T in VI.Claves)
                { 
                    double valor = VI[T].ObtenerValor();
                    valor = Variable.RedondeaVariable(Tipo, valor);
                    valores.Add(T, valor);                    
                }
            }
            else
            {
                return null;
            }
            return valores;
        }
        /// <summary>
        /// Busca el valor de una variable escalar
        /// </summary>
        /// <param name="NombreVariable">El nombre de la variable</param>
        /// <returns>El valor</returns>
        public double GetValorVariableEscalar(NombreVariablesEscalares NombreVariable)
        {
            double valor = 0;
            if(_nombreVariablesEscalares.Contains(NombreVariable) && _variablesEscalares.ContainsKey(NombreVariable.ToString()))
            {
                VariableEscalar VE = _variablesEscalares[NombreVariable.ToString()];
                valor = VE.ObtenerValor();
            }
            else
            {
                throw new Exception("La variable " + NombreVariable.ToString() + " no estaba en el problema");
            }
            return valor;
        }

        
        #endregion
    }
}
