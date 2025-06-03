using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TravelManagerWPF.Models
{
    public class Reservation : INotifyPropertyChanged
    {
        private int _id;
        private int _productId;
        private string _customerName = string.Empty;
        private string _customerEmail = string.Empty;
        private string _customerPhone = string.Empty;
        private DateTime _reservationDate;
        private ReservationStatus _status;
        private decimal _totalAmount;
        private int _numberOfPeople;
        private string _specialRequests = string.Empty;
        private TravelProduct? _product;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public int ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                OnPropertyChanged();
            }
        }

        public TravelProduct? Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged();
            }
        }

        public string CustomerName
        {
            get => _customerName;
            set
            {
                _customerName = value;
                OnPropertyChanged();
            }
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            set
            {
                _customerEmail = value;
                OnPropertyChanged();
            }
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set
            {
                _customerPhone = value;
                OnPropertyChanged();
            }
        }

        public DateTime ReservationDate
        {
            get => _reservationDate;
            set
            {
                _reservationDate = value;
                OnPropertyChanged();
            }
        }

        public ReservationStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfPeople
        {
            get => _numberOfPeople;
            set
            {
                _numberOfPeople = value;
                OnPropertyChanged();
            }
        }

        public string SpecialRequests
        {
            get => _specialRequests;
            set
            {
                _specialRequests = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }
} 