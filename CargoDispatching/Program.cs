using System;
using CargoDispatching.Algorithm;
using CargoDispatching.Reader;

namespace CargoDispatching
{
    class Program
    {
        static void Main(string[] args)
        {
            var cargoFile = "CargoTest.csv";
            var locationCargoFile = "LocationCargoTest.csv";
            var reader = new CSVReader(cargoFile, locationCargoFile);
            reader.Read();

            var cargoList = reader.GetCargoList();
            var locationList = reader.GetLocationList();

            var model = new Model(cargoList, locationList);
            model.Run();

            Console.WriteLine("\nPress any key to exit!");
            Console.ReadKey();
        }
    }
}
