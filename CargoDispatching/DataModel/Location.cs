using System;
using System.Collections.Generic;
using System.Text;

namespace CargoDispatching.DataModel
{
    public class Location
    {
        /// <summary>
        /// Location ID: Either a code that determines the location or its unique
        /// name. For example; Erguvan, Akasya, Defne, Seller_1, .., Seller_N
        /// </summary>
        private String _id;

        /// <summary>
        /// This parameter denotes the cargo list that works with this location.
        /// </summary>
        private List<Cargo> _cargoList;

        /// <summary>
        /// The weekly forecast value for this location
        /// </summary>
        private Double _forecast;

        public Location(String id, Double forecast)
        {
            _id = id;
            _forecast = forecast;
            _cargoList = new List<Cargo>();
        }

        public void AddToCargoList(Cargo cargo)
        {
            if(!_cargoList.Contains(cargo))
                _cargoList.Add(cargo);
        }

        /// <summary>
        /// Get the id of the location
        /// </summary>
        /// <returns></returns>
        public String GetId()
        {
            return _id;
        }

        /// <summary>
        /// Get the forecast of the location
        /// </summary>
        /// <returns></returns>
        public Double GetForecast()
        {
            return _forecast;
        }

        /// <summary>
        /// Get the cargo list
        /// </summary>
        /// <returns></returns>
        public List<Cargo> GetCargoList()
        {
            return _cargoList;
        }

        public Boolean ContainsCargo(Cargo cargo)
        {
            var contains = _cargoList.Contains(cargo);
            return contains;
        }
    }
}
