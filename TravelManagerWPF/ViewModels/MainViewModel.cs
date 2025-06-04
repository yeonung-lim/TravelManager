using System.Collections.ObjectModel;
using TravelManagerWPF.Models;
using System;
using TravelManagerWPF.Helpers;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TravelManagerWPF.Services;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Defaults;
using System.IO;
using System.Reflection;

namespace TravelManagerWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private ObservableCollection<TravelProduct> _allProducts = new();
        private ObservableCollection<TravelProduct> _filteredProducts = new();
        private string _searchText = string.Empty;
        private string _selectedDestinationFilter = "전체";
        private int _nextProductId = 1;
        private TravelProduct _newProduct = new();

        // CookieRun 폰트를 위한 SKTypeface
        private static readonly SKTypeface CookieRunTypeface = GetCookieRunTypeface();

        // 여행 일정 관련 프로퍼티들
        private ObservableCollection<Itinerary> _allItineraries = new();
        private ObservableCollection<Itinerary> _filteredItineraries = new();
        private Itinerary? _selectedItinerary;
        private Itinerary? _editingItinerary;
        private Itinerary _newItinerary = new();
        private DaySchedule? _selectedDaySchedule;
        private DaySchedule? _editingDaySchedule;
        private DaySchedule _newDaySchedule = new();
        private Activity? _selectedActivity;
        private Activity? _editingActivity;
        private Activity _newActivity = new();
        private int _nextItineraryId = 1;

        // 예약 관리 관련 프로퍼티들
        private ObservableCollection<Reservation> _allReservations = new();
        private ObservableCollection<Reservation> _filteredReservations = new();
        private Reservation? _selectedReservation;
        private Reservation? _editingReservation;
        private Reservation _newReservation = new();
        private int _nextReservationId = 1;
        private string _reservationSearchText = string.Empty;
        private ReservationStatus _selectedStatusFilter = ReservationStatus.Pending;
        private bool _showAllStatuses = true;

        // 통계 관련 프로퍼티들
        private ISeries[] _monthlyRevenueSeries = Array.Empty<ISeries>();
        private ISeries[] _destinationPopularitySeries = Array.Empty<ISeries>();
        private ISeries[] _reservationStatusSeries = Array.Empty<ISeries>();
        private Axis[] _monthlyRevenueXAxes = Array.Empty<Axis>();
        private Axis[] _monthlyRevenueYAxes = Array.Empty<Axis>();
        private Axis[] _destinationPopularityXAxes = Array.Empty<Axis>();
        private Axis[] _destinationPopularityYAxes = Array.Empty<Axis>();

        // 툴팁 폰트 설정
        public SolidColorPaint TooltipTextPaint { get; } = new SolidColorPaint(SKColors.Black) 
        { 
            SKTypeface = CookieRunTypeface
        };

        public ObservableCollection<TravelProduct> Products
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> DestinationOptions { get; set; } = new() { "전체" };

        // 여행 일정 관련 프로퍼티들
        public ObservableCollection<Itinerary> Itineraries
        {
            get => _filteredItineraries;
            set
            {
                _filteredItineraries = value;
                OnPropertyChanged();
            }
        }

        public Itinerary? SelectedItinerary
        {
            get => _selectedItinerary;
            set
            {
                _selectedItinerary = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsItinerarySelected));
                OnPropertyChanged(nameof(SelectedItineraryDays));
            }
        }

        public Itinerary? EditingItinerary
        {
            get => _editingItinerary;
            set
            {
                _editingItinerary = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditingItinerary));
            }
        }

        public Itinerary NewItinerary
        {
            get => _newItinerary;
            set
            {
                _newItinerary = value;
                OnPropertyChanged();
            }
        }

        public DaySchedule? SelectedDaySchedule
        {
            get => _selectedDaySchedule;
            set
            {
                _selectedDaySchedule = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDayScheduleSelected));
                OnPropertyChanged(nameof(SelectedDayActivities));
            }
        }

        public DaySchedule? EditingDaySchedule
        {
            get => _editingDaySchedule;
            set
            {
                _editingDaySchedule = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditingDaySchedule));
            }
        }

        public DaySchedule NewDaySchedule
        {
            get => _newDaySchedule;
            set
            {
                _newDaySchedule = value;
                OnPropertyChanged();
            }
        }

        public Activity? SelectedActivity
        {
            get => _selectedActivity;
            set
            {
                _selectedActivity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsActivitySelected));
            }
        }

        public Activity? EditingActivity
        {
            get => _editingActivity;
            set
            {
                _editingActivity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditingActivity));
            }
        }

        public Activity NewActivity
        {
            get => _newActivity;
            set
            {
                _newActivity = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DaySchedule> SelectedItineraryDays =>
            SelectedItinerary?.DaySchedules ?? new ObservableCollection<DaySchedule>();

        public ObservableCollection<Activity> SelectedDayActivities =>
            SelectedDaySchedule?.Activities ?? new ObservableCollection<Activity>();

        // Commands
        public ICommand AddProductCommand { get; private set; }
        public ICommand EditProductCommand { get; private set; }
        public ICommand SaveProductCommand { get; private set; }
        public ICommand DeleteProductCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand LoadDataCommand { get; private set; }
        public ICommand SaveDataCommand { get; private set; }

        // 여행 일정 관련 명령어들
        public ICommand AddItineraryCommand { get; private set; }
        public ICommand EditItineraryCommand { get; private set; }
        public ICommand SaveItineraryCommand { get; private set; }
        public ICommand DeleteItineraryCommand { get; private set; }
        public ICommand CancelEditItineraryCommand { get; private set; }
        public ICommand AddDayScheduleCommand { get; private set; }
        public ICommand EditDayScheduleCommand { get; private set; }
        public ICommand SaveDayScheduleCommand { get; private set; }
        public ICommand DeleteDayScheduleCommand { get; private set; }
        public ICommand CancelEditDayScheduleCommand { get; private set; }
        public ICommand AddActivityCommand { get; private set; }
        public ICommand EditActivityCommand { get; private set; }
        public ICommand SaveActivityCommand { get; private set; }
        public ICommand DeleteActivityCommand { get; private set; }
        public ICommand CancelEditActivityCommand { get; private set; }

        // 예약 관리 관련 명령어들
        public ICommand AddReservationCommand { get; private set; }
        public ICommand EditReservationCommand { get; private set; }
        public ICommand SaveReservationCommand { get; private set; }
        public ICommand DeleteReservationCommand { get; private set; }
        public ICommand CancelEditReservationCommand { get; private set; }
        public ICommand ConfirmReservationCommand { get; private set; }
        public ICommand CancelReservationCommand { get; private set; }
        public ICommand CompleteReservationCommand { get; private set; }

        public TravelProduct NewProduct
        {
            get => _newProduct;
            set
            {
                _newProduct = value;
                OnPropertyChanged();
            }
        }
        
        private TravelProduct? _selectedProduct;
        public TravelProduct? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProductSelected));
            }
        }

        private TravelProduct? _editingProduct;
        public TravelProduct? EditingProduct
        {
            get => _editingProduct;
            set
            {
                _editingProduct = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditing));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public string SelectedDestinationFilter
        {
            get => _selectedDestinationFilter;
            set
            {
                _selectedDestinationFilter = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public bool IsProductSelected => SelectedProduct != null;
        public bool IsEditing => EditingProduct != null;
        public bool IsItinerarySelected => SelectedItinerary != null;
        public bool IsEditingItinerary => EditingItinerary != null;
        public bool IsDayScheduleSelected => SelectedDaySchedule != null;
        public bool IsEditingDaySchedule => EditingDaySchedule != null;
        public bool IsActivitySelected => SelectedActivity != null;
        public bool IsEditingActivity => EditingActivity != null;
        public bool IsReservationSelected => SelectedReservation != null;
        public bool IsEditingReservation => EditingReservation != null;

        // 예약 관리 관련 프로퍼티들
        public ObservableCollection<Reservation> Reservations
        {
            get => _filteredReservations;
            set
            {
                _filteredReservations = value;
                OnPropertyChanged();
            }
        }

        public Reservation? SelectedReservation
        {
            get => _selectedReservation;
            set
            {
                _selectedReservation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsReservationSelected));
            }
        }

        public Reservation? EditingReservation
        {
            get => _editingReservation;
            set
            {
                _editingReservation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditingReservation));
            }
        }

        public Reservation NewReservation
        {
            get => _newReservation;
            set
            {
                _newReservation = value;
                OnPropertyChanged();
            }
        }

        public string ReservationSearchText
        {
            get => _reservationSearchText;
            set
            {
                _reservationSearchText = value;
                OnPropertyChanged();
                FilterReservations();
            }
        }

        public ReservationStatus SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged();
                FilterReservations();
            }
        }

        public bool ShowAllStatuses
        {
            get => _showAllStatuses;
            set
            {
                _showAllStatuses = value;
                OnPropertyChanged();
                FilterReservations();
            }
        }

        public ObservableCollection<ReservationStatus> StatusOptions { get; set; } = new()
        {
            ReservationStatus.Pending,
            ReservationStatus.Confirmed,
            ReservationStatus.Cancelled,
            ReservationStatus.Completed
        };

        public ObservableCollection<TravelProduct> ProductsForReservation => _allProducts;

        // 통계 관련 공개 프로퍼티들
        public ISeries[] MonthlyRevenueSeries
        {
            get => _monthlyRevenueSeries;
            set
            {
                _monthlyRevenueSeries = value;
                OnPropertyChanged();
            }
        }

        public ISeries[] DestinationPopularitySeries
        {
            get => _destinationPopularitySeries;
            set
            {
                _destinationPopularitySeries = value;
                OnPropertyChanged();
            }
        }

        public ISeries[] ReservationStatusSeries
        {
            get => _reservationStatusSeries;
            set
            {
                _reservationStatusSeries = value;
                OnPropertyChanged();
            }
        }

        public Axis[] MonthlyRevenueXAxes
        {
            get => _monthlyRevenueXAxes;
            set
            {
                _monthlyRevenueXAxes = value;
                OnPropertyChanged();
            }
        }

        public Axis[] MonthlyRevenueYAxes
        {
            get => _monthlyRevenueYAxes;
            set
            {
                _monthlyRevenueYAxes = value;
                OnPropertyChanged();
            }
        }

        public Axis[] DestinationPopularityXAxes
        {
            get => _destinationPopularityXAxes;
            set
            {
                _destinationPopularityXAxes = value;
                OnPropertyChanged();
            }
        }

        public Axis[] DestinationPopularityYAxes
        {
            get => _destinationPopularityYAxes;
            set
            {
                _destinationPopularityYAxes = value;
                OnPropertyChanged();
            }
        }

        // 통계 요약 정보
        public decimal TotalRevenue => _allReservations
            .Where(r => r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Completed)
            .Sum(r => r.TotalAmount);

        public int TotalReservations => _allReservations.Count;

        public int ConfirmedReservations => _allReservations.Count(r => r.Status == ReservationStatus.Confirmed);

        public int CompletedReservations => _allReservations.Count(r => r.Status == ReservationStatus.Completed);

        public string MostPopularDestination
        {
            get
            {
                var destinationCounts = _allReservations
                    .Where(r => r.Product != null)
                    .GroupBy(r => r.Product!.Destination)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                return destinationCounts?.Key ?? "없음";
            }
        }

        public decimal AverageReservationAmount => TotalReservations > 0 ? TotalRevenue / TotalReservations : 0;

        public MainViewModel()
        {
            _dataService = new DataService();
            
            // _allProducts의 CollectionChanged 이벤트 구독
            _allProducts.CollectionChanged += (sender, e) => 
                OnPropertyChanged(nameof(ProductsForReservation));
                
            InitializeCommands();
            InitializeData();
            LoadDataAsync();
            UpdateStatistics();
        }

        private void InitializeCommands()
        {
            AddProductCommand = new RelayCommand(_ => AddProduct(), _ => !IsEditing);
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => IsProductSelected && !IsEditing);
            SaveProductCommand = new RelayCommand(_ => SaveProduct(), _ => IsEditing);
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => IsProductSelected && !IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            SearchCommand = new RelayCommand(_ => FilterProducts());
            LoadDataCommand = new RelayCommand(_ => LoadDataAsync());
            SaveDataCommand = new RelayCommand(_ => SaveDataAsync());

            // 여행 일정 관련 명령어들 초기화
            AddItineraryCommand = new RelayCommand(_ => AddItinerary(), _ => !IsEditingItinerary);
            EditItineraryCommand = new RelayCommand(_ => EditItinerary(), _ => IsItinerarySelected && !IsEditingItinerary);
            SaveItineraryCommand = new RelayCommand(_ => SaveItinerary(), _ => IsEditingItinerary);
            DeleteItineraryCommand = new RelayCommand(_ => DeleteItinerary(), _ => IsItinerarySelected && !IsEditingItinerary);
            CancelEditItineraryCommand = new RelayCommand(_ => CancelEditItinerary(), _ => IsEditingItinerary);
            
            AddDayScheduleCommand = new RelayCommand(_ => AddDaySchedule(), _ => IsItinerarySelected && !IsEditingDaySchedule);
            EditDayScheduleCommand = new RelayCommand(_ => EditDaySchedule(), _ => IsDayScheduleSelected && !IsEditingDaySchedule);
            SaveDayScheduleCommand = new RelayCommand(_ => SaveDaySchedule(), _ => IsEditingDaySchedule);
            DeleteDayScheduleCommand = new RelayCommand(_ => DeleteDaySchedule(), _ => IsDayScheduleSelected && !IsEditingDaySchedule);
            CancelEditDayScheduleCommand = new RelayCommand(_ => CancelEditDaySchedule(), _ => IsEditingDaySchedule);
            
            AddActivityCommand = new RelayCommand(_ => AddActivity(), _ => IsDayScheduleSelected && !IsEditingActivity);
            EditActivityCommand = new RelayCommand(_ => EditActivity(), _ => IsActivitySelected && !IsEditingActivity);
            SaveActivityCommand = new RelayCommand(_ => SaveActivity(), _ => IsEditingActivity);
            DeleteActivityCommand = new RelayCommand(_ => DeleteActivity(), _ => IsActivitySelected && !IsEditingActivity);
            CancelEditActivityCommand = new RelayCommand(_ => CancelEditActivity(), _ => IsEditingActivity);

            // 예약 관리 관련 명령어들 초기화
            AddReservationCommand = new RelayCommand(_ => AddReservation(), _ => !IsEditingReservation);
            EditReservationCommand = new RelayCommand(_ => EditReservation(), _ => IsReservationSelected && !IsEditingReservation);
            SaveReservationCommand = new RelayCommand(_ => SaveReservation(), _ => IsEditingReservation);
            DeleteReservationCommand = new RelayCommand(_ => DeleteReservation(), _ => IsReservationSelected && !IsEditingReservation);
            CancelEditReservationCommand = new RelayCommand(_ => CancelEditReservation(), _ => IsEditingReservation);
            ConfirmReservationCommand = new RelayCommand(_ => ConfirmReservation(), _ => IsReservationSelected && !IsEditingReservation);
            CancelReservationCommand = new RelayCommand(_ => CancelReservation(), _ => IsReservationSelected && !IsEditingReservation);
            CompleteReservationCommand = new RelayCommand(_ => CompleteReservation(), _ => IsReservationSelected && !IsEditingReservation);
        }

        private void InitializeData()
        {
            NewProduct = new TravelProduct
            {
                StartDate = DateTime.Today
            };

            NewItinerary = new Itinerary();
            NewDaySchedule = new DaySchedule { Date = DateTime.Today };
            NewActivity = new Activity { StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) };
            
            NewReservation = new Reservation
            {
                ReservationDate = DateTime.Today,
                Status = ReservationStatus.Pending,
                NumberOfPeople = 1
            };
        }

        private async void LoadDataAsync()
        {
            try
            {
                var loadedProducts = await _dataService.LoadProductsAsync();
                _allItineraries = await _dataService.LoadItinerariesAsync();
                _allReservations = await _dataService.LoadReservationsAsync();
                
                // 기존 이벤트 구독 해제 후 새로운 컬렉션으로 교체
                _allProducts.CollectionChanged -= (sender, e) => 
                    OnPropertyChanged(nameof(ProductsForReservation));
                    
                _allProducts = loadedProducts;
                
                // 새로운 컬렉션에 이벤트 구독
                _allProducts.CollectionChanged += (sender, e) => 
                    OnPropertyChanged(nameof(ProductsForReservation));
                
                // 예약에 대한 상품 참조 설정
                foreach (var reservation in _allReservations)
                {
                    reservation.Product = _allProducts.FirstOrDefault(p => p.Id == reservation.ProductId);
                }
                
                // ID 자동 증가를 위한 최대값 찾기
                if (_allProducts.Any())
                {
                    _nextProductId = _allProducts.Max(p => p.Id) + 1;
                }

                if (_allItineraries.Any())
                {
                    _nextItineraryId = _allItineraries.Max(i => i.Id) + 1;
                }
                
                if (_allReservations.Any())
                {
                    _nextReservationId = _allReservations.Max(r => r.Id) + 1;
                }

                UpdateDestinationOptions();
                FilterProducts();
                FilterItineraries();
                FilterReservations();
                UpdateStatistics();
                
                // 초기 로드 완료 후 ProductsForReservation 알림
                OnPropertyChanged(nameof(ProductsForReservation));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"데이터 로드 중 오류가 발생했습니다: {ex.Message}", "오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void SaveDataAsync()
        {
            try
            {
                await _dataService.SaveProductsAsync(_allProducts);
                await _dataService.SaveItinerariesAsync(_allItineraries);
                await _dataService.SaveReservationsAsync(_allReservations);
                System.Windows.MessageBox.Show("데이터가 성공적으로 저장되었습니다.", "저장 완료", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"데이터 저장 중 오류가 발생했습니다: {ex.Message}", "오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(NewProduct.Name) || string.IsNullOrWhiteSpace(NewProduct.Destination))
            {
                System.Windows.MessageBox.Show("상품명과 목적지를 입력해주세요.", "입력 오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var product = new TravelProduct
            {
                Id = _nextProductId++,
                Name = NewProduct.Name,
                Destination = NewProduct.Destination,
                Price = NewProduct.Price,
                StartDate = NewProduct.StartDate,
                DurationDays = NewProduct.DurationDays,
                Description = NewProduct.Description
            };

            _allProducts.Add(product);  // CollectionChanged 이벤트가 자동으로 발생
            UpdateDestinationOptions();
            FilterProducts();

            // Reset form
            NewProduct = new TravelProduct { StartDate = DateTime.Today };
            OnPropertyChanged(nameof(NewProduct));
        }

        private void EditProduct()
        {
            if (SelectedProduct == null) return;

            EditingProduct = new TravelProduct
            {
                Id = SelectedProduct.Id,
                Name = SelectedProduct.Name,
                Destination = SelectedProduct.Destination,
                Price = SelectedProduct.Price,
                StartDate = SelectedProduct.StartDate,
                DurationDays = SelectedProduct.DurationDays,
                Description = SelectedProduct.Description
            };
        }

        private void SaveProduct()
        {
            if (EditingProduct == null || SelectedProduct == null) return;

            if (string.IsNullOrWhiteSpace(EditingProduct.Name) || string.IsNullOrWhiteSpace(EditingProduct.Destination))
            {
                System.Windows.MessageBox.Show("상품명과 목적지를 입력해주세요.", "입력 오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // 실제 제품 업데이트 (속성 변경은 CollectionChanged를 발생시키지 않으므로 수동 알림 필요)
            SelectedProduct.Name = EditingProduct.Name;
            SelectedProduct.Destination = EditingProduct.Destination;
            SelectedProduct.Price = EditingProduct.Price;
            SelectedProduct.StartDate = EditingProduct.StartDate;
            SelectedProduct.DurationDays = EditingProduct.DurationDays;
            SelectedProduct.Description = EditingProduct.Description;

            UpdateDestinationOptions();
            FilterProducts();
            CancelEdit();
            
            // 기존 아이템의 속성 변경은 CollectionChanged가 발생하지 않으므로 수동 알림
            OnPropertyChanged(nameof(ProductsForReservation));
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = System.Windows.MessageBox.Show(
                $"'{SelectedProduct.Name}' 상품을 삭제하시겠습니까?", 
                "삭제 확인", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _allProducts.Remove(SelectedProduct);  // CollectionChanged 이벤트가 자동으로 발생
                UpdateDestinationOptions();
                FilterProducts();
                SelectedProduct = null;
            }
        }

        private void CancelEdit()
        {
            EditingProduct = null;
        }

        private void FilterProducts()
        {
            var filtered = _allProducts.AsEnumerable();

            // 검색어 필터
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p => 
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Destination.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // 목적지 필터
            if (SelectedDestinationFilter != "전체")
            {
                filtered = filtered.Where(p => p.Destination == SelectedDestinationFilter);
            }

            Products = new ObservableCollection<TravelProduct>(filtered);
        }

        private void FilterItineraries()
        {
            Itineraries = new ObservableCollection<Itinerary>(_allItineraries);
        }

        private void UpdateDestinationOptions()
        {
            var destinations = _allProducts.Select(p => p.Destination).Distinct().OrderBy(d => d).ToList();
            
            DestinationOptions.Clear();
            DestinationOptions.Add("전체");
            foreach (var destination in destinations)
            {
                if (!string.IsNullOrWhiteSpace(destination))
                    DestinationOptions.Add(destination);
            }
            
            OnPropertyChanged(nameof(DestinationOptions));
        }

        // 여행 일정 관리 메서드들
        private void AddItinerary()
        {
            if (string.IsNullOrWhiteSpace(NewItinerary.Title))
            {
                System.Windows.MessageBox.Show("일정 제목을 입력해주세요.", "입력 오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var itinerary = new Itinerary
            {
                Id = _nextItineraryId++,
                ProductId = NewItinerary.ProductId,
                Title = NewItinerary.Title
            };

            _allItineraries.Add(itinerary);
            FilterItineraries();

            NewItinerary = new Itinerary();
            OnPropertyChanged(nameof(NewItinerary));
        }

        private void EditItinerary()
        {
            if (SelectedItinerary == null) return;

            EditingItinerary = new Itinerary
            {
                Id = SelectedItinerary.Id,
                ProductId = SelectedItinerary.ProductId,
                Title = SelectedItinerary.Title
            };
        }

        private void SaveItinerary()
        {
            if (EditingItinerary == null || SelectedItinerary == null) return;

            if (string.IsNullOrWhiteSpace(EditingItinerary.Title))
            {
                System.Windows.MessageBox.Show("일정 제목을 입력해주세요.", "입력 오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            SelectedItinerary.ProductId = EditingItinerary.ProductId;
            SelectedItinerary.Title = EditingItinerary.Title;

            FilterItineraries();
            CancelEditItinerary();
        }

        private void DeleteItinerary()
        {
            if (SelectedItinerary == null) return;

            var result = System.Windows.MessageBox.Show(
                $"'{SelectedItinerary.Title}' 일정을 삭제하시겠습니까?", 
                "삭제 확인", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _allItineraries.Remove(SelectedItinerary);
                FilterItineraries();
                SelectedItinerary = null;
            }
        }

        private void CancelEditItinerary()
        {
            EditingItinerary = null;
        }

        private void AddDaySchedule()
        {
            if (SelectedItinerary == null) return;

            var daySchedule = new DaySchedule
            {
                DayNumber = NewDaySchedule.DayNumber,
                Date = NewDaySchedule.Date,
                Accommodation = NewDaySchedule.Accommodation,
                Transportation = NewDaySchedule.Transportation
            };

            SelectedItinerary.DaySchedules.Add(daySchedule);
            OnPropertyChanged(nameof(SelectedItineraryDays));

            NewDaySchedule = new DaySchedule { Date = DateTime.Today };
            OnPropertyChanged(nameof(NewDaySchedule));
        }

        private void EditDaySchedule()
        {
            if (SelectedDaySchedule == null) return;

            EditingDaySchedule = new DaySchedule
            {
                DayNumber = SelectedDaySchedule.DayNumber,
                Date = SelectedDaySchedule.Date,
                Accommodation = SelectedDaySchedule.Accommodation,
                Transportation = SelectedDaySchedule.Transportation
            };
        }

        private void SaveDaySchedule()
        {
            if (EditingDaySchedule == null || SelectedDaySchedule == null) return;

            SelectedDaySchedule.DayNumber = EditingDaySchedule.DayNumber;
            SelectedDaySchedule.Date = EditingDaySchedule.Date;
            SelectedDaySchedule.Accommodation = EditingDaySchedule.Accommodation;
            SelectedDaySchedule.Transportation = EditingDaySchedule.Transportation;

            OnPropertyChanged(nameof(SelectedItineraryDays));
            CancelEditDaySchedule();
        }

        private void DeleteDaySchedule()
        {
            if (SelectedDaySchedule == null || SelectedItinerary == null) return;

            var result = System.Windows.MessageBox.Show(
                $"{SelectedDaySchedule.DayNumber}일차 일정을 삭제하시겠습니까?", 
                "삭제 확인", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                SelectedItinerary.DaySchedules.Remove(SelectedDaySchedule);
                OnPropertyChanged(nameof(SelectedItineraryDays));
                SelectedDaySchedule = null;
            }
        }

        private void CancelEditDaySchedule()
        {
            EditingDaySchedule = null;
        }

        private void AddActivity()
        {
            if (SelectedDaySchedule == null) return;

            if (string.IsNullOrWhiteSpace(NewActivity.Name))
            {
                System.Windows.MessageBox.Show("활동명을 입력해주세요.", "입력 오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var activity = new Activity
            {
                Name = NewActivity.Name,
                Description = NewActivity.Description,
                StartTime = NewActivity.StartTime,
                EndTime = NewActivity.EndTime,
                Location = NewActivity.Location
            };

            SelectedDaySchedule.Activities.Add(activity);
            OnPropertyChanged(nameof(SelectedDayActivities));

            NewActivity = new Activity { StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) };
            OnPropertyChanged(nameof(NewActivity));
        }

        private void EditActivity()
        {
            if (SelectedActivity == null) return;

            EditingActivity = new Activity
            {
                Name = SelectedActivity.Name,
                Description = SelectedActivity.Description,
                StartTime = SelectedActivity.StartTime,
                EndTime = SelectedActivity.EndTime,
                Location = SelectedActivity.Location
            };
        }

        private void SaveActivity()
        {
            if (EditingActivity == null || SelectedActivity == null) return;

            if (string.IsNullOrWhiteSpace(EditingActivity.Name))
            {
                System.Windows.MessageBox.Show("활동명을 입력해주세요.", "입력 오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            SelectedActivity.Name = EditingActivity.Name;
            SelectedActivity.Description = EditingActivity.Description;
            SelectedActivity.StartTime = EditingActivity.StartTime;
            SelectedActivity.EndTime = EditingActivity.EndTime;
            SelectedActivity.Location = EditingActivity.Location;

            OnPropertyChanged(nameof(SelectedDayActivities));
            CancelEditActivity();
        }

        private void DeleteActivity()
        {
            if (SelectedActivity == null || SelectedDaySchedule == null) return;

            var result = System.Windows.MessageBox.Show(
                $"'{SelectedActivity.Name}' 활동을 삭제하시겠습니까?", 
                "삭제 확인", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                SelectedDaySchedule.Activities.Remove(SelectedActivity);
                OnPropertyChanged(nameof(SelectedDayActivities));
                SelectedActivity = null;
            }
        }

        private void CancelEditActivity()
        {
            EditingActivity = null;
        }

        // 예약 관리 관련 메서드들
        private void AddReservation()
        {
            if (string.IsNullOrWhiteSpace(NewReservation.CustomerName) || 
                string.IsNullOrWhiteSpace(NewReservation.CustomerEmail) ||
                NewReservation.ProductId <= 0 ||
                NewReservation.NumberOfPeople <= 0)
            {
                System.Windows.MessageBox.Show("필수 정보를 모두 입력해주세요.\n(고객명, 이메일, 상품, 인원)", "입력 오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var selectedProduct = _allProducts.FirstOrDefault(p => p.Id == NewReservation.ProductId);
            if (selectedProduct != null)
            {
                NewReservation.TotalAmount = selectedProduct.Price * NewReservation.NumberOfPeople;
            }

            var reservation = new Reservation
            {
                Id = _nextReservationId++,
                ProductId = NewReservation.ProductId,
                Product = selectedProduct,
                CustomerName = NewReservation.CustomerName,
                CustomerEmail = NewReservation.CustomerEmail,
                CustomerPhone = NewReservation.CustomerPhone,
                ReservationDate = NewReservation.ReservationDate,
                Status = NewReservation.Status,
                TotalAmount = NewReservation.TotalAmount,
                NumberOfPeople = NewReservation.NumberOfPeople,
                SpecialRequests = NewReservation.SpecialRequests
            };

            _allReservations.Add(reservation);
            FilterReservations();
            UpdateStatistics();

            // Reset form
            NewReservation = new Reservation
            {
                ReservationDate = DateTime.Today,
                Status = ReservationStatus.Pending,
                NumberOfPeople = 1
            };
            OnPropertyChanged(nameof(NewReservation));
        }

        private void EditReservation()
        {
            if (SelectedReservation == null) return;

            EditingReservation = new Reservation
            {
                Id = SelectedReservation.Id,
                ProductId = SelectedReservation.ProductId,
                CustomerName = SelectedReservation.CustomerName,
                CustomerEmail = SelectedReservation.CustomerEmail,
                CustomerPhone = SelectedReservation.CustomerPhone,
                ReservationDate = SelectedReservation.ReservationDate,
                Status = SelectedReservation.Status,
                TotalAmount = SelectedReservation.TotalAmount,
                NumberOfPeople = SelectedReservation.NumberOfPeople,
                SpecialRequests = SelectedReservation.SpecialRequests
            };
        }

        private void SaveReservation()
        {
            if (EditingReservation == null || SelectedReservation == null) return;

            if (string.IsNullOrWhiteSpace(EditingReservation.CustomerName) || 
                string.IsNullOrWhiteSpace(EditingReservation.CustomerEmail) ||
                EditingReservation.ProductId <= 0 ||
                EditingReservation.NumberOfPeople <= 0)
            {
                System.Windows.MessageBox.Show("필수 정보를 모두 입력해주세요.\n(고객명, 이메일, 상품, 인원)", "입력 오류", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var selectedProduct = _allProducts.FirstOrDefault(p => p.Id == EditingReservation.ProductId);
            if (selectedProduct != null)
            {
                EditingReservation.TotalAmount = selectedProduct.Price * EditingReservation.NumberOfPeople;
            }

            // 실제 예약 업데이트
            SelectedReservation.ProductId = EditingReservation.ProductId;
            SelectedReservation.Product = selectedProduct;
            SelectedReservation.CustomerName = EditingReservation.CustomerName;
            SelectedReservation.CustomerEmail = EditingReservation.CustomerEmail;
            SelectedReservation.CustomerPhone = EditingReservation.CustomerPhone;
            SelectedReservation.ReservationDate = EditingReservation.ReservationDate;
            SelectedReservation.Status = EditingReservation.Status;
            SelectedReservation.TotalAmount = EditingReservation.TotalAmount;
            SelectedReservation.NumberOfPeople = EditingReservation.NumberOfPeople;
            SelectedReservation.SpecialRequests = EditingReservation.SpecialRequests;

            FilterReservations();
            UpdateStatistics();
            CancelEditReservation();
        }

        private void DeleteReservation()
        {
            if (SelectedReservation == null) return;

            var result = System.Windows.MessageBox.Show(
                $"'{SelectedReservation.CustomerName}'님의 예약을 삭제하시겠습니까?", 
                "삭제 확인", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _allReservations.Remove(SelectedReservation);
                FilterReservations();
                UpdateStatistics();
                SelectedReservation = null;
            }
        }

        private void CancelEditReservation()
        {
            EditingReservation = null;
        }

        private void ConfirmReservation()
        {
            if (SelectedReservation == null) return;
            SelectedReservation.Status = ReservationStatus.Confirmed;
            FilterReservations();
            UpdateStatistics();
        }

        private void CancelReservation()
        {
            if (SelectedReservation == null) return;
            SelectedReservation.Status = ReservationStatus.Cancelled;
            FilterReservations();
            UpdateStatistics();
        }

        private void CompleteReservation()
        {
            if (SelectedReservation == null) return;
            SelectedReservation.Status = ReservationStatus.Completed;
            FilterReservations();
            UpdateStatistics();
        }

        private void FilterReservations()
        {
            var filtered = _allReservations.AsEnumerable();

            // 검색어 필터
            if (!string.IsNullOrWhiteSpace(ReservationSearchText))
            {
                filtered = filtered.Where(r => 
                    r.CustomerName.Contains(ReservationSearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.CustomerEmail.Contains(ReservationSearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.CustomerPhone.Contains(ReservationSearchText, StringComparison.OrdinalIgnoreCase));
            }

            // 상태 필터
            if (!ShowAllStatuses)
            {
                filtered = filtered.Where(r => r.Status == SelectedStatusFilter);
            }

            Reservations = new ObservableCollection<Reservation>(filtered.OrderByDescending(r => r.ReservationDate));
            UpdateStatistics();
        }

        // 통계 관련 메서드들
        private void UpdateStatistics()
        {
            UpdateStatisticsValues();
            UpdateMonthlyRevenueChart();
            UpdateDestinationPopularityChart();
            UpdateReservationStatusChart();
        }

        private void UpdateStatisticsValues()
        {
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(TotalReservations));
            OnPropertyChanged(nameof(ConfirmedReservations));
            OnPropertyChanged(nameof(CompletedReservations));
            OnPropertyChanged(nameof(MostPopularDestination));
            OnPropertyChanged(nameof(AverageReservationAmount));
        }

        private void UpdateMonthlyRevenueChart()
        {
            var monthlyData = _allReservations
                .Where(r => r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Completed)
                .GroupBy(r => new { Year = r.ReservationDate.Year, Month = r.ReservationDate.Month })
                .Select(g => new 
                { 
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(r => r.TotalAmount),
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1)
                })
                .OrderBy(x => x.Date)
                .Take(12) // 최근 12개월
                .ToArray();

            if (monthlyData.Any())
            {
                MonthlyRevenueSeries = new ISeries[]
                {
                    new ColumnSeries<decimal>
                    {
                        Values = monthlyData.Select(x => x.Revenue).ToArray(),
                        Name = "월별 매출",
                        Fill = new SolidColorPaint(SKColors.SkyBlue),
                        Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                        YToolTipLabelFormatter = value => $"₩{value.Model:N0}",
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = CookieRunTypeface }
                    }
                };

                MonthlyRevenueXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = monthlyData.Select(x => x.Period).ToArray(),
                        Name = "월",
                        NamePaint = new SolidColorPaint(SKColors.Black) { SKTypeface = CookieRunTypeface },
                        LabelsPaint = new SolidColorPaint(SKColors.Gray) { SKTypeface = CookieRunTypeface },
                        TextSize = 12
                    }
                };

                MonthlyRevenueYAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "매출 (원)",
                        NamePaint = new SolidColorPaint(SKColors.Black) { SKTypeface = CookieRunTypeface },
                        LabelsPaint = new SolidColorPaint(SKColors.Gray) { SKTypeface = CookieRunTypeface },
                        TextSize = 12,
                        Labeler = value => $"₩{value:N0}"
                    }
                };
            }
        }

        private void UpdateDestinationPopularityChart()
        {
            var destinationData = _allReservations
                .Where(r => r.Product != null)
                .GroupBy(r => r.Product!.Destination)
                .Select(g => new { Destination = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10) // 상위 10개 목적지
                .ToArray();

            if (destinationData.Any())
            {
                DestinationPopularitySeries = new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Values = destinationData.Select(x => x.Count).ToArray(),
                        Name = "예약 건수",
                        Fill = new SolidColorPaint(SKColors.LightGreen),
                        Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 2 },
                        YToolTipLabelFormatter = value => $"{value.Model}건",
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = CookieRunTypeface }
                    }
                };

                DestinationPopularityXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = destinationData.Select(x => x.Destination).ToArray(),
                        Name = "목적지",
                        NamePaint = new SolidColorPaint(SKColors.Black) { SKTypeface = CookieRunTypeface },
                        LabelsPaint = new SolidColorPaint(SKColors.Gray) { SKTypeface = CookieRunTypeface },
                        TextSize = 12
                    }
                };

                DestinationPopularityYAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "예약 건수",
                        NamePaint = new SolidColorPaint(SKColors.Black) { SKTypeface = CookieRunTypeface },
                        LabelsPaint = new SolidColorPaint(SKColors.Gray) { SKTypeface = CookieRunTypeface },
                        TextSize = 12
                    }
                };
            }
        }

        private void UpdateReservationStatusChart()
        {
            var statusData = Enum.GetValues<ReservationStatus>()
                .Select(status => new
                {
                    Status = status.ToString(),
                    Count = _allReservations.Count(r => r.Status == status)
                })
                .Where(x => x.Count > 0)
                .ToArray();

            if (statusData.Any())
            {
                var colors = new SKColor[]
                {
                    SKColors.Orange,    // Pending
                    SKColors.Green,     // Confirmed
                    SKColors.Red,       // Cancelled
                    SKColors.Purple     // Completed
                };

                ReservationStatusSeries = statusData.Select((data, index) => 
                    new PieSeries<int>
                    {
                        Values = new[] { data.Count },
                        Name = GetStatusDisplayName(data.Status),
                        Fill = new SolidColorPaint(colors[index % colors.Length]),
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black) { SKTypeface = CookieRunTypeface }
                    }).ToArray();
            }
        }

        private string GetStatusDisplayName(string status)
        {
            return status switch
            {
                "Pending" => "대기 중",
                "Confirmed" => "확정",
                "Cancelled" => "취소",
                "Completed" => "완료",
                _ => status
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static SKTypeface GetCookieRunTypeface()
        {
            try
            {
                // 여러 가능한 경로들을 시도
                var possiblePaths = new[]
                {
                    // 1. 실행 파일과 같은 폴더의 fonts 디렉토리
                    Path.Combine(Path.GetDirectoryName(Environment.ProcessPath ?? "") ?? "", "fonts", "CookieRun Regular.ttf"),
                    
                    // 2. AppDomain BaseDirectory의 fonts 디렉토리
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts", "CookieRun Regular.ttf"),
                    
                    // 3. 현재 작업 디렉토리의 fonts 디렉토리
                    Path.Combine(Environment.CurrentDirectory, "fonts", "CookieRun Regular.ttf"),
                    
                    // 4. Assembly Location 기반 경로
                    Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", "fonts", "CookieRun Regular.ttf")
                };

                // 디버깅을 위한 경로 정보 출력 (릴리즈에서는 제거 가능)
                System.Diagnostics.Debug.WriteLine($"ProcessPath: {Environment.ProcessPath}");
                System.Diagnostics.Debug.WriteLine($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
                System.Diagnostics.Debug.WriteLine($"CurrentDirectory: {Environment.CurrentDirectory}");
                System.Diagnostics.Debug.WriteLine($"Assembly Location: {System.Reflection.Assembly.GetExecutingAssembly().Location}");

                foreach (var fontPath in possiblePaths)
                {
                    System.Diagnostics.Debug.WriteLine($"Trying font path: {fontPath}");
                    
                    if (!string.IsNullOrEmpty(fontPath) && File.Exists(fontPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"Font found at: {fontPath}");
                        return SKTypeface.FromFile(fontPath);
                    }
                }

                // 폰트를 찾지 못한 경우 추가 디버깅 정보
                System.Diagnostics.Debug.WriteLine("Font file not found in any of the attempted paths");
                
                // fonts 디렉토리가 존재하는지 확인
                var baseDir = Path.GetDirectoryName(Environment.ProcessPath ?? "") ?? "";
                var fontsDir = Path.Combine(baseDir, "fonts");
                if (Directory.Exists(fontsDir))
                {
                    System.Diagnostics.Debug.WriteLine($"Fonts directory exists: {fontsDir}");
                    var files = Directory.GetFiles(fontsDir, "*.ttf");
                    System.Diagnostics.Debug.WriteLine($"TTF files in fonts directory: {string.Join(", ", files)}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Fonts directory does not exist: {fontsDir}");
                }
                
                // 폰트 파일을 찾을 수 없는 경우 기본 폰트 사용
                return SKTypeface.Default;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading font: {ex.Message}");
                return SKTypeface.Default;
            }
        }
    }
}
