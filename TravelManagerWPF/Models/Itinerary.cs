using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TravelManagerWPF.Models
{
    public class Itinerary : INotifyPropertyChanged
    {
        private int _id;
        private int _productId;
        private string _title = string.Empty;
        private ObservableCollection<DaySchedule> _daySchedules = new();

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

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DaySchedule> DaySchedules
        {
            get => _daySchedules;
            set
            {
                _daySchedules = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class DaySchedule : INotifyPropertyChanged
    {
        private int _dayNumber;
        private DateTime _date;
        private ObservableCollection<Activity> _activities = new();
        private string _accommodation = string.Empty;
        private string _transportation = string.Empty;

        public int DayNumber
        {
            get => _dayNumber;
            set
            {
                _dayNumber = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Activity> Activities
        {
            get => _activities;
            set
            {
                _activities = value;
                OnPropertyChanged();
            }
        }

        public string Accommodation
        {
            get => _accommodation;
            set
            {
                _accommodation = value;
                OnPropertyChanged();
            }
        }

        public string Transportation
        {
            get => _transportation;
            set
            {
                _transportation = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class Activity : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private string _location = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
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

        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                _location = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
} 