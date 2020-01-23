using CargoDispatching.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CargoDispatching.Reader
{
    public class CSVReader
    {
        /// <summary>
        /// Cargo list is the list where you store the related information
        /// for each cargo. 
        /// </summary>
        private List<Cargo> _cargoList;

        /// <summary>
        /// Location list is the list where you store location specific data.
        /// </summary>
        private List<Location> _locationList;

        /// <summary>
        /// Total forecast
        /// </summary>
        private Double _totalForecast;

        /// <summary>
        /// Name of the cargo file
        /// </summary>
        private String _cargoFile;

        /// <summary>
        /// Name of the location-cargo file
        /// </summary>
        private String _locationCargoFile;

        public CSVReader(String cargoFile, String locationCargoFile)
        {
            _cargoFile = cargoFile;
            _locationCargoFile = locationCargoFile;
            _cargoList = new List<Cargo>();
            _locationList = new List<Location>();
        }

        /// <summary>
        /// Read the files
        /// </summary>
        public void Read()
        {
            ReadCargoInformation();
            ReadLocationCargoMatrix();
        }

        /// <summary>
        /// Read the global information concerning each cargo.
        /// </summary>
        public void ReadCargoInformation()
        {
            using (var sr = File.OpenText(_cargoFile))
            {
                string s = sr.ReadLine();
                while ((s = sr.ReadLine()) != null)
                {
                    var line = s.Split(',');

                    var id = line[0];
                    var minCapacity = Convert.ToDouble(line[1]);
                    var maxCapacity = Convert.ToDouble(line[2]);
                    var excessCapacity = Convert.ToDouble(line[3]);
                    var regularCost = Convert.ToDouble(line[4]);
                    var excessCost = Convert.ToDouble(line[5]);
                    var demurrageCost = Convert.ToDouble(line[6]);
                    var coverageRate = Convert.ToDouble(line[7]);

                    var cargo = new Cargo(id, minCapacity, maxCapacity, excessCapacity,
                        regularCost, excessCost, demurrageCost, coverageRate);
                    _cargoList.Add(cargo);
                }
            }
        }

        /// <summary>
        /// Read the location-cargo incidence matrix and the forecast for each location
        /// </summary>
        public void ReadLocationCargoMatrix()
        {
            using (var sr = File.OpenText(_locationCargoFile))
            {
                string s = sr.ReadLine();

                while ((s=sr.ReadLine()) != null)
                {
                    var line = s.Split(',');

                    var id = line[0];
                    var forecast = Convert.ToDouble(line[1]);

                    var location = new Location(id, forecast);

                    var workingWithYk = Convert.ToDouble(line[2]);
                    var workingWithMng = Convert.ToDouble(line[3]);
                    var workingWithTex = Convert.ToDouble(line[4]);
                    var workingWithAras = Convert.ToDouble(line[5]);
                    var workingWithUps = Convert.ToDouble(line[6]);
                    var workingWithPtt = Convert.ToDouble(line[7]);


                    if (workingWithYk > 0)
                    {
                        var cargo = _cargoList.Find(c => c.GetId() == "YK");
                        location.AddToCargoList(cargo);
                    }
                    if (workingWithMng > 0)
                    {
                        var cargo = _cargoList.Find(c => c.GetId() == "MNG");
                        location.AddToCargoList(cargo);
                    }
                    if (workingWithTex > 0)
                    {
                        var cargo = _cargoList.Find(c => c.GetId() == "TEX");
                        location.AddToCargoList(cargo);
                    }
                    if (workingWithAras > 0)
                    {
                        var cargo = _cargoList.Find(c => c.GetId() == "ARAS");
                        location.AddToCargoList(cargo);
                    }
                    if (workingWithUps > 0)
                    {
                        var cargo = _cargoList.Find(c => c.GetId() == "UPS");
                        location.AddToCargoList(cargo);
                    }
                    if (workingWithPtt > 0)
                    {
                        var cargo = _cargoList.Find(c => c.GetId() == "PTT");
                        location.AddToCargoList(cargo);
                    }

                    _locationList.Add(location);
                }
            }
        }

        /// <summary>
        /// Get cargo list
        /// </summary>
        /// <returns></returns>
        public List<Cargo> GetCargoList()
        {
            return _cargoList;
        }

        /// <summary>
        /// Get location list
        /// </summary>
        /// <returns></returns>
        public List<Location> GetLocationList()
        {
            return _locationList;
        }
    }
}
