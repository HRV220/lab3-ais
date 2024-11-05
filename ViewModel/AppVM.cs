using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GalaSoft.MvvmLight.Command;
using Lab4_AIS.Model;

namespace Lab4_AIS.ViewModel
{
    public class AppVM : INotifyPropertyChanged
    {

        IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
        private static UdpClient udpClient;
        private ObservableCollection<CarAuction> _cars = new ObservableCollection<CarAuction>();
        private CarAuction _car = new CarAuction();
        private string _validationMessage;
        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
        public CarAuction Car
        {
            get { return _car; }
            set
            {
                _car = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<CarAuction> Cars
        {
            get => _cars;
            set
            {
                _cars = value;
                OnPropertyChanged();  // Уведомление об изменении свойства
            }
        }
        public AppVM()
        {
            Cars = new ObservableCollection<CarAuction>(); // Инициализация коллекции
            this.DeleteDataCommand = new AsyncRelayCommand(this.DeleteData);
            this.AddDataCommand = new AsyncRelayCommand(this.AddData);

            // Вызов асинхронного метода после инициализации
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await GetAllData(); // Асинхронно загружаем данные из базы
        }



        public IAsyncRelayCommand DeleteDataCommand { get; private set; }
        public IAsyncRelayCommand AddDataCommand { get; private set; }

        private string ValidateCar(CarAuction car)
        {
            var errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(car.Maker))
                errorMessages.Add("Maker is required.");
            if (string.IsNullOrWhiteSpace(car.Model))
                errorMessages.Add("Model is required.");
            if (string.IsNullOrWhiteSpace(car.VIN))
                errorMessages.Add("VIN is required.");
            else if (car.VIN.Length != 17)
                errorMessages.Add("VIN must be 17 characters long.");
            if (car.YearProd < 1886 || car.YearProd > DateTime.Now.Year)
                errorMessages.Add("Invalid production year.");
            if (car.Price <= 0)
                errorMessages.Add("Price must be greater than zero.");
            if (car.Mileage < 0)
                errorMessages.Add("Mileage cannot be negative.");

            return string.Join("\n", errorMessages);
        }


        public async Task GetAllData()
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    byte[] requestBytes = Encoding.UTF8.GetBytes("get_all_data");
                    await udpClient.SendAsync(requestBytes, requestBytes.Length, localIP);

                    var receivedResult = await udpClient.ReceiveAsync();
                    string response = Encoding.UTF8.GetString(receivedResult.Buffer);
                    string[] strings = response.Split('\n');

                    Cars.Clear();

                    for (int i = 0; i < strings.Length - 1; i++)
                    {
                        CarAuction car = CarAuction.Parse(strings[i]);
                        Cars.Add(car);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                // Обработка ошибок, например, вывести сообщение пользователю
                Console.WriteLine($"Ошибка получения данных: {ex.Message}");
            }
        }

        public async Task DeleteData()
        {
            try
            {
                if (Car != null && Car.Id > 0)
                {
                    using (var udpClient = new UdpClient())
                    {
                        byte[] requestBytes = Encoding.UTF8.GetBytes($"del_str:{Car.Id}");
                        await udpClient.SendAsync(requestBytes, requestBytes.Length, localIP);

                        // Асинхронно получаем ответ
                        var receivedResult = await udpClient.ReceiveAsync();
                        string response = Encoding.UTF8.GetString(receivedResult.Buffer);

                        // Обновляем список после удаления
                        await GetAllData();

                        ErrorMessage = response;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления данных: {ex.Message}");
            }
        }


        public async Task AddData()
        {
            try
            {
                // Проверяем валидность данных перед отправкой
                var validationErrors = ValidateCar(Car);
                if (!string.IsNullOrEmpty(validationErrors))
                {
                    // Если есть ошибки валидации, выводим их в Label
                    ValidationMessage = validationErrors;
                    return;
                }

                // Если ошибок нет, очищаем ValidationMessage
                ValidationMessage = string.Empty;

                using (var udpClient = new UdpClient())
                {
                    string carData = $"add_str:{Car.Maker},{Car.Model},{Car.Color},{Car.VIN},{Car.Price},{Car.YearProd},{Car.Mileage},{Car.IsSold},{Car.HasAccidents}";
                    byte[] requestBytes = Encoding.UTF8.GetBytes(carData);
                    await udpClient.SendAsync(requestBytes, requestBytes.Length, localIP);

                    // Асинхронно получаем ответ
                    var receivedResult = await udpClient.ReceiveAsync();
                    string response = Encoding.UTF8.GetString(receivedResult.Buffer);
                    await GetAllData();

                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Ошибка добавления данных: {ex.Message}";
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
