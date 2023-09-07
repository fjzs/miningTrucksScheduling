using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolverDemo
{
    /// <summary>
    /// Clase que modela la red de transporte
    /// </summary>
    public class Grafo
    {
        #region Atributos
        /// <summary>
        /// La lista de nodos de la red.
        /// </summary>
        private List<Nodo> _nodos;
        /// <summary>
        /// La lista de arcos del grafo
        /// </summary>
        private List<Arco> _arcos;

        //ATRIBUTOS DINÁMICOS
        public List<int> CamionesSimulados;
        public Dictionary<int, double> DemoraTotal;

        #endregion

        #region Constructor
        /// <summary>
        /// El constructor del grafo de la red
        /// </summary>
        /// <param name="_nodos">El diccionario con los nodos de la red. Se identifican mediante su id</param>
        /// <param name="_arcos">La lista de arcos del grafo</param>
        public Grafo(List<Nodo> _nodos, List<Arco> _arcos)
        {
            this._nodos = _nodos;
            this._arcos = _arcos;
        }
        #endregion

        #region Properties
        /// <summary>
        /// La lista de nodos de la red.
        /// </summary>
        public List<Nodo> Nodos
        {
            get { return _nodos; }
        }
        /// <summary>
        /// La lista de arcos del grafo
        /// </summary>
        public List<Arco> Arcos
        {
            get { return _arcos; }
        }
        #endregion

        #region Métodos públicos
        /// <summary>
        /// Reinicia los datos dinámicos de los nodos y los arcos
        /// </summary>
        public void Reset()
        {
            CamionesSimulados = new List<int>();
            DemoraTotal = new Dictionary<int, double>();

            //De los arcos:
            foreach (Nodo N in _nodos)
            {
                N.Reset();
            }
            foreach (Arco A in _arcos)
            {
                A.Reset();
            }
        }
        #endregion

        

    }
}
