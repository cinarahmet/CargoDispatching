using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using ILOG.CPLEX;
using ILOG.Concert;
using CargoDispatching.DataModel;

namespace CargoDispatching.Algorithm
{
    public class Model
    {
        /// <summary>
        /// Solver instance
        /// </summary>
        private readonly Cplex _solver;

        /// <summary>
        /// Objective function
        /// </summary>
        private ILinearNumExpr _objective;

        /// <summary>
        /// x[i][j] € N denotes the amount of units assigned to cargo i from location j.
        /// </summary>
        private List<List<INumVar>> x;

        /// <summary>
        /// e[i] € N denotes the amount of excess units assigned to cargo i from location j.
        /// </summary>
        private List<List<INumVar>> e;

        /// <summary>
        /// m[i] € N is the number of units stayed under minimum capacity.
        /// </summary>
        private List<INumVar> m;

        /// <summary>
        /// yR[i] € N denotes the regular units assigned to cargo i.
        /// </summary>
        private List<INumVar> yR;

        /// <summary>
        /// yE[i] € N denotes the excess units assigned to cargo i.
        /// </summary>
        private List<INumVar> yE;

        /// <summary>
        /// List of cargo
        /// </summary>
        private readonly List<Cargo> _cargoList;

        /// <summary>
        /// List of locations
        /// </summary>
        private readonly List<Location> _locationList;

        /// <summary>
        /// Solution status: Optimal, feasible..
        /// </summary>
        private Cplex.Status _status;

        /// <summary>
        /// Time limit is given in seconds.
        /// </summary>
        private readonly long _timeLimit = 30;

        /// <summary>
        /// How many seconds the solver worked..
        /// </summary>
        private Double _solutionTime;

        /// <summary>
        /// A sufficiently small number
        /// </summary>
        private readonly Double _epsilon = 0.00001;

        /// <summary>
        /// The weekly total forecast
        /// </summary>
        private readonly Double Q;

        /// <summary>
        /// Number of cargo
        /// </summary>
        private Int32 N;

        /// <summary>
        /// Number of locations
        /// </summary>
        private Int32 M;

        public Model(List<Cargo> cargoList, List<Location> locationList)
        {
            _solver = new Cplex();
            _solver.SetParam(Cplex.Param.TimeLimit, _timeLimit);
            _solver.SetOut(null);

            x = new List<List<INumVar>>();
            e = new List<List<INumVar>>();
            yR = new List<INumVar>();
            yE = new List<INumVar>();
            m = new List<INumVar>();

            _cargoList = cargoList;
            _locationList = locationList;

            N = _cargoList.Count;
            M = _locationList.Count;

        }

        /// <summary>
        /// Run method where the running engine is triggered.
        /// </summary>
        public void Run()
        {
            BuildModel();
            Solve();
            Print();
            ClearModel();
        }

        /// <summary>
        /// Build the model:
        /// 1. Create decision variables
        /// 2. Create objective function
        /// 3. Create constraints
        /// </summary>
        private void BuildModel()
        {
            Console.WriteLine("Model construction starts at {0}", DateTime.Now);
            CreateDecisionVariables();
            CreateObjective();
            CreateConstraints();
            Console.WriteLine("Model construction ends at {0}", DateTime.Now);
        }

        /// <summary>
        /// Solve the mathematical model
        /// </summary>
        private void Solve()
        {
            Console.WriteLine("Algorithm starts running at {0}", DateTime.Now);
            var startTime = DateTime.Now;
            _solver.Solve();
            _solutionTime = (DateTime.Now - startTime).Seconds;
            _status = _solver.GetStatus();
            Console.WriteLine("Algorithm stops running at {0}", DateTime.Now);
        }

        /// <summary>
        /// Create decision variables for the optimization model
        /// </summary>
        private void CreateDecisionVariables()
        {
            // Create x[i][j] - variables
            for (int i = 0; i < N; i++)
            {
                var x_i = new List<INumVar>();
                for (int j = 0; j < M; j++)
                {
                    var name = $"x[{(i + 1)}][{(j + 1)}]";
                    var x_ij = _solver.NumVar(0, Int32.MaxValue, NumVarType.Int, name);
                    x_i.Add(x_ij);
                }
                x.Add(x_i);
            }

            // Create e[i][j] - variables
            for (int i = 0; i < N; i++)
            {
                var e_i = new List<INumVar>();
                for (int j = 0; j < M; j++)
                {
                    var name = $"e[{(i + 1)}][{(j + 1)}]";
                    var e_ij = _solver.NumVar(0, Int32.MaxValue, NumVarType.Int, name);
                    e_i.Add(e_ij);
                }
                e.Add(e_i);
            }

            // Create yR[i] - variables
            for (int i = 0; i < N; i++)
            {
                var name = $"yR[{(i + 1)}]";

                var yR_i = _solver.NumVar(0, Int32.MaxValue, NumVarType.Int, name);
                yR.Add(yR_i);
            }

            // Create yE[i] - variables
            for (int i = 0; i < N; i++)
            {
                var name = $"yE[{(i + 1)}]";

                var yE_i = _solver.NumVar(0, Int32.MaxValue, NumVarType.Int, name);
                yE.Add(yE_i);
            }

            // Create m[i] - variables
            for (int i = 0; i < N; i++)
            {
                var name = $"m[{(i + 1)}]";

                var m_i = _solver.NumVar(0, Int32.MaxValue, NumVarType.Int, name);
                m.Add(m_i);
            }
        }

        /// <summary>
        /// Create objective function for the optimization model
        /// </summary>
        private void CreateObjective()
        {
            _objective = _solver.LinearNumExpr();

            for (int i = 0; i < N; i++)
            {
                var cargo = _cargoList[i];

                var regularCost = cargo.GetRegularCost();
                var excessCost = cargo.GetExcessCost();
                var demurrageCost = cargo.GetDemurrageCost();

                _objective.AddTerm(yR[i], regularCost);
                _objective.AddTerm(yE[i], excessCost);
                _objective.AddTerm(m[i], demurrageCost);
            }

            _solver.AddMinimize(_objective);

        }

        /// <summary>
        /// Create constraints for the optimization model
        /// </summary>
        private void CreateConstraints()
        {
            SatisfyForecast();
            RegularDistribution();
            ExcessDistribution();
            Coverage();
            Capacity();
            Nonnegativity();
        }

        /// <summary>
        /// Satisfy demand/forecast for each location
        /// </summary>
        private void SatisfyForecast()
        {
            for (int j = 0; j < M; j++)
            {
                var location = _locationList[j];
                var constraint = _solver.LinearNumExpr();

                for (int i = 0; i < N; i++)
                {
                    var cargo = _cargoList[i];
                    if (location.ContainsCargo(cargo))
                    {
                        constraint.AddTerm(x[i][j], 1);
                        constraint.AddTerm(e[i][j], 1);
                    }
                }

                var forecast = location.GetForecast();
                _solver.AddEq(constraint, forecast);
            }
        }

        /// <summary>
        /// Determines the regular distribution for each cargo
        /// </summary>
        private void RegularDistribution()
        {
            for (int i = 0; i < N; i++)
            {
                var cargo = _cargoList[i];
                var constraint = _solver.LinearNumExpr();

                for (int j = 0; j < M; j++)
                {
                    var location = _locationList[j];
                    if(location.ContainsCargo(cargo))
                        constraint.AddTerm(x[i][j], 1);
                }

                _solver.AddEq(constraint, yR[i]);
            }
        }

        /// <summary>
        /// Determines the excess distribution for each cargo
        /// </summary>
        private void ExcessDistribution()
        {
            for (int i = 0; i < N; i++)
            {
                var cargo = _cargoList[i];
                var constraint = _solver.LinearNumExpr();

                for (int j = 0; j < M; j++)
                {
                    var location = _locationList[j];
                    if (location.ContainsCargo(cargo))
                        constraint.AddTerm(e[i][j], 1);
                }

                _solver.AddEq(constraint, yE[i]);
            }
        }

        /// <summary>
        /// Constraints for cargo - location coverage considerations. For example, TEX
        /// can have at most %55 of the cargo from a location
        /// </summary>
        private void Coverage()
        {
            for (int i = 0; i < N; i++)
            {
                var cargo = _cargoList[i];
                for (int j = 0; j < M; j++)
                {
                    var location = _locationList[j];
                    if (location.ContainsCargo(cargo))
                    {
                        var rate = cargo.GetCoverageRate();
                        var forecast = location.GetForecast();

                        var constraint = _solver.LinearNumExpr();
                        constraint.AddTerm(x[i][j], 1);
                        constraint.AddTerm(e[i][j], 1);
                        var rhs = forecast * rate;

                        _solver.AddLe(constraint, rhs);

                    }
                }
            }
        }

        /// <summary>
        /// Capacity constraints for each cargo
        /// </summary>
        private void Capacity()
        {
            for (int i = 0; i < N; i++)
            {
                var cargo = _cargoList[i];

                var ub = cargo.GetMaxCapacity();
                _solver.AddLe(yR[i], ub);

                var lb = cargo.GetMinCapacity();
                var constraint = _solver.LinearNumExpr();
                constraint.AddTerm(yR[i], 1);
                constraint.AddTerm(yE[i], 1);
                constraint.AddTerm(m[i], 1);
                _solver.AddGe(constraint, lb);

                var excessCap = cargo.GetExcessCapacity();
                _solver.AddLe(yE[i], excessCap);
            }
        }

        /// <summary>
        /// Non-negativity constraints
        /// </summary>
        private void Nonnegativity()
        {
            for (int i = 0; i < N; i++)
            {
                _solver.AddGe(yR[i], 0);
                _solver.AddGe(yE[i], 0);
                _solver.AddGe(m[i], 0);

                for (int j = 0; j < M; j++)
                {
                    _solver.AddGe(x[i][j], 0);
                    _solver.AddGe(e[i][j], 0);
                }
            }
        }

        /// <summary>
        /// Print some solution details
        /// </summary>
        private void Print()
        {
            if (!(_status == Cplex.Status.Optimal || _status == Cplex.Status.Feasible))
            {
                Console.WriteLine("No feasible solution exists!");
                return;
            }
             
            var objVal = _solver.GetObjValue();
            Console.WriteLine("\nTotal cost: {0} TL", objVal);

            var totalForecast = _locationList.Sum(location => location.GetForecast());
            Console.WriteLine("Total weekly forecast: {0}\n", totalForecast);
            for (int j = 0; j < M; j++)
            {
                var location = _locationList[j];
                var forecast = location.GetForecast();
                Console.WriteLine("Distribution for {0} with forecast: {1}", location.GetId(), forecast);
                for (int i = 0; i < N; i++)
                {
                    var cargo = _cargoList[i];
                    var x_ij = 0.0;
                    var e_ij = 0.0;

                    if (location.ContainsCargo(cargo))
                    {
                        x_ij = Math.Round(_solver.GetValue(x[i][j]), 0);
                        e_ij = Math.Round(_solver.GetValue(e[i][j]), 0);
                    }

                    Console.WriteLine("{0}\tx:{1}\te:{2}\t", cargo.GetId(), x_ij, e_ij);

                }
                Console.WriteLine();
            }

            Console.WriteLine("\nUnder capacity quantities for each cargo:");
            for (int i = 0; i < N; i++)
            {
                var m_i = Math.Round(_solver.GetValue(m[i]), 0);
                var cargo = _cargoList[i];

                Console.WriteLine("{0}\t:{1}", cargo.GetId(), m_i);
            }


            Console.WriteLine("\nTotal amount assigned for each cargo:");
            for (int i = 0; i < N; i++)
            {
                var yR_i = Math.Round(_solver.GetValue(yR[i]), 0);
                var yE_i = Math.Round(_solver.GetValue(yE[i]), 0);
                var cargo = _cargoList[i];

                Console.WriteLine("{0}\t:{1}", cargo.GetId(), yR_i + yE_i);
            }
        }

        /// <summary>
        /// Clear Cplex
        /// </summary>
        public void ClearModel()
        {
            _solver.ClearModel();
            _solver.Dispose();
        }

    }
}
