using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lab4_AIS.Model
{
    [Serializable]
    public class CarAuction : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public string Maker { get; set; }

        public string Model { get; set; }

        public string Color { get; set; }

        public string VIN { get; set; }

        public int? Price { get; set; }

        public int? YearProd { get; set; }

        public int? Mileage { get; set; }

        public bool? IsSold { get; set; }

        public bool? HasAccidents { get; set; }

        public CarAuction(string maker, string model, string color, string vin, int price, int yearProd, int mileage, bool issold, bool hasAccidents)
        {
            Maker = maker;
            Model = model;
            Color = color;
            VIN = vin;
            Price = price;
            YearProd = yearProd;
            Mileage = mileage;
            IsSold = false;
            HasAccidents = hasAccidents;
        }

        public CarAuction(int id, string maker, string model, string color, string vin, int price, int yearProd, int mileage, bool issold, bool hasAccidents)
        {
            Id = id;
            Maker = maker;
            Model = model;
            Color = color;
            VIN = vin;
            Price = price;
            YearProd = yearProd;
            Mileage = mileage;
            IsSold = false;
            HasAccidents = hasAccidents;
        }

        public CarAuction()
        {
            Maker = "";
            Model = "";
            Color = "";
            VIN = "";
            Price = null;
            YearProd = null;
            Mileage = null;
            IsSold = false;
            HasAccidents = null;
        }

        public override string ToString()
        {
            return $"{Id},{Maker},{Model},{Color},{VIN},{Price},{YearProd},{Mileage},{IsSold},{HasAccidents}";
        }

        public static CarAuction Parse(string str)
        {
            string[] parts = str.Split(',');
            if (parts.Length == 9)
                return new CarAuction(parts[0], parts[1], parts[2], parts[3], int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]), bool.Parse(parts[7]), bool.Parse(parts[8]));
            else if (parts.Length == 10)
                return new CarAuction(int.Parse(parts[0]), parts[1], parts[2], parts[3], parts[4], int.Parse(parts[5]), int.Parse(parts[6]), int.Parse(parts[7]), bool.Parse(parts[8]), bool.Parse(parts[9]));
            else
                throw new FormatException("Неверный формат данных.");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
