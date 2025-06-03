using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TravelManagerWPF.Models
{
    public class TravelProduct : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _destination = string.Empty;
        private decimal _price;
        private DateTime _startDate;
        private int _durationDays;
        private string _description = string.Empty;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Destination
        {
            get => _destination;
            set
            {
                _destination = value;
                OnPropertyChanged();
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public int DurationDays
        {
            get => _durationDays;
            set
            {
                _durationDays = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate => StartDate.AddDays(DurationDays);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}