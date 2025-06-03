using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using TravelManagerWPF.Models;

namespace TravelManagerWPF.Services
{
    public class DataService
    {
        private readonly string _dataDirectory;
        private readonly string _productsFile;
        private readonly string _itinerariesFile;
        private readonly string _reservationsFile;

        public DataService()
        {
            _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TravelManager");
            _productsFile = Path.Combine(_dataDirectory, "products.json");
            _itinerariesFile = Path.Combine(_dataDirectory, "itineraries.json");
            _reservationsFile = Path.Combine(_dataDirectory, "reservations.json");

            if (!Directory.Exists(_dataDirectory))
                Directory.CreateDirectory(_dataDirectory);
        }

        // Travel Products
        public async Task<ObservableCollection<TravelProduct>> LoadProductsAsync()
        {
            try
            {
                if (!File.Exists(_productsFile))
                    return new ObservableCollection<TravelProduct>();

                var json = await File.ReadAllTextAsync(_productsFile);
                var products = JsonConvert.DeserializeObject<List<TravelProduct>>(json) ?? new List<TravelProduct>();
                return new ObservableCollection<TravelProduct>(products);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
                return new ObservableCollection<TravelProduct>();
            }
        }

        public async Task SaveProductsAsync(ObservableCollection<TravelProduct> products)
        {
            try
            {
                var json = JsonConvert.SerializeObject(products, Formatting.Indented);
                await File.WriteAllTextAsync(_productsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving products: {ex.Message}");
            }
        }

        // Itineraries
        public async Task<ObservableCollection<Itinerary>> LoadItinerariesAsync()
        {
            try
            {
                if (!File.Exists(_itinerariesFile))
                    return new ObservableCollection<Itinerary>();

                var json = await File.ReadAllTextAsync(_itinerariesFile);
                var itineraries = JsonConvert.DeserializeObject<List<Itinerary>>(json) ?? new List<Itinerary>();
                return new ObservableCollection<Itinerary>(itineraries);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading itineraries: {ex.Message}");
                return new ObservableCollection<Itinerary>();
            }
        }

        public async Task SaveItinerariesAsync(ObservableCollection<Itinerary> itineraries)
        {
            try
            {
                var json = JsonConvert.SerializeObject(itineraries, Formatting.Indented);
                await File.WriteAllTextAsync(_itinerariesFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving itineraries: {ex.Message}");
            }
        }

        // Reservations
        public async Task<ObservableCollection<Reservation>> LoadReservationsAsync()
        {
            try
            {
                if (!File.Exists(_reservationsFile))
                    return new ObservableCollection<Reservation>();

                var json = await File.ReadAllTextAsync(_reservationsFile);
                var reservations = JsonConvert.DeserializeObject<List<Reservation>>(json) ?? new List<Reservation>();
                return new ObservableCollection<Reservation>(reservations);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reservations: {ex.Message}");
                return new ObservableCollection<Reservation>();
            }
        }

        public async Task SaveReservationsAsync(ObservableCollection<Reservation> reservations)
        {
            try
            {
                var json = JsonConvert.SerializeObject(reservations, Formatting.Indented);
                await File.WriteAllTextAsync(_reservationsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving reservations: {ex.Message}");
            }
        }
    }
} 