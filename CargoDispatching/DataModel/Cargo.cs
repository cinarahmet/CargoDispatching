using System;
using System.Collections.Generic;
using System.Text;

namespace CargoDispatching.DataModel
{
    public class Cargo
    {
        /// <summary>
        /// Cargo ID: YK, MNG, TEX.. which differentiates a cargo
        /// from other cargo.This should be uniquely defined.
        /// </summary>
        private String _id;

        /// <summary>
        /// Minimum capacity for the cargo. This capacity stands for the
        /// regular minimum capacity.
        /// </summary>
        private Double _minCapacity;

        /// <summary>
        /// Maximum capacity for the cargo. This capacity stands for the
        /// regular maximum capacity.
        /// </summary>
        private Double _maxCapacity;

        /// <summary>
        /// Excess capacity is introduced if the regular maximum capacities
        /// are exceeded and you need extra capacity.
        /// </summary>
        private Double _excessCapacity;

        /// <summary>
        /// This parameter stands for the regular cost per unit for the cargo.
        /// The cargo applies this cost until a predetermined threshold level.
        /// For example, a cargo might say it costs $3 per drop until 50000
        /// deliveries. Each cargo exceeding this costs $5.
        /// </summary>
        private Double _regularCost;

        /// <summary>
        /// This parameter stands for the additional cost for those that exceed
        /// the regular maximum capacity.
        /// </summary>
        private Double _excessCost;

        /// <summary>
        /// This parameter stands for the cost of staying under the minimum capacity.
        /// </summary>
        private Double _demurrageCost;

        /// <summary>
        /// This is the rate denoting how much capacity can be reserved for the cargo
        /// </summary>
        private Double _coverageRate;

        public Cargo(String id, Double minCapacity, Double maxCapacity, 
            Double excessCapacity, Double regularCost, Double excessCost,
            Double demurrageCost, Double coverageRate)
        {
            _id = id;
            _minCapacity = minCapacity;
            _maxCapacity = maxCapacity;
            _excessCapacity = excessCapacity;
            _regularCost = regularCost;
            _excessCost = excessCost;
            _demurrageCost = demurrageCost;
            _coverageRate = coverageRate;
        }

        /// <summary>
        /// Get the id of the cargo.
        /// </summary>
        /// <returns></returns>
        public String GetId()
        {
            return _id;
        }


        /// <summary>
        /// Get the minimum capacity of the cargo
        /// </summary>
        /// <returns></returns>
        public Double GetMinCapacity()
        {
            return _minCapacity;
        }

        /// <summary>
        /// Get the maximum capacity of the cargo
        /// </summary>
        /// <returns></returns>
        public Double GetMaxCapacity()
        {
            return _maxCapacity;
        }

        /// <summary>
        /// Get the excess capacity of the cargo
        /// </summary>
        /// <returns></returns>
        public Double GetExcessCapacity()
        {
            return _excessCapacity;
        }

        /// <summary>
        /// Get the regular cost of the cargo
        /// </summary>
        /// <returns></returns>
        public Double GetRegularCost()
        {
            return _regularCost;
        }

        /// <summary>
        /// Get the excess cost of the cargo
        /// </summary>
        /// <returns></returns>
        public Double GetExcessCost()
        {
            return _excessCost;
        }
        
        /// <summary>
        /// Get the demurrage cost of the cargo
        /// </summary>
        /// <returns></returns>
        public Double GetDemurrageCost()
        {
            return _demurrageCost;
        }

        /// <summary>
        /// Get the coverage rate
        /// </summary>
        /// <returns></returns>
        public Double GetCoverageRate()
        {
            return _coverageRate;
        }

    }
}
