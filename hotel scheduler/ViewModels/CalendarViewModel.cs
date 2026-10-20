using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using hotel_scheduler.Views;

namespace hotel_scheduler.ViewModels
{
    public partial class CalendarViewModel(ReservationService reservationService, RoomService roomService,
            ReservationViewModel reservationViewModel, IServiceProvider serviceProvider) : ObservableObject
    {
        private readonly ReservationService _reservationService = reservationService;
        private readonly RoomService _roomService = roomService;
        private readonly ReservationViewModel _reservationViewModel = reservationViewModel;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        // These lists are always REPLACED, never modified in place. That makes it safe
        // for the view to read them from a background thread while computing bar positions.
        public List<RoomDTO> Rooms { get; private set; } = [];
        public List<ReservationDTO> Reservations { get; private set; } = [];

        // Built once per range change instead of on every property access.
        private List<DateTime> _visibleDates = [];
        public List<DateTime> VisibleDates => _visibleDates;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DateRangeLabel))]
        [NotifyPropertyChangedFor(nameof(VisibleDates))]
        public partial DateTime StartDate { get; set; } = DateTime.Today;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DateRangeLabel))]
        [NotifyPropertyChangedFor(nameof(VisibleDates))]
        public partial DateTime EndDate { get; set; } = DateTime.Today.AddDays(6);
        [ObservableProperty]
        public partial int DayCount { get; set; } = 7;

        public string DateRangeLabel => $"{StartDate:MMM d} | {EndDate:MMM d}";

        // Only one DB query at a time: a DbContext must never run two queries in parallel.
        private readonly SemaphoreSlim _dbLock = new(1, 1);

        // Bumped on every refresh request. If a newer request exists by the time we get the lock
        // (or finish querying), the older one is skipped/discarded, so rapid arrow clicks
        // collapse into ~1-2 queries instead of one per click.
        private int _loadVersion;

        // True when the date range changed and the grid still needs a FULL rebuild.
        // Stays true if the request that set it gets superseded, so the change is never lost.
        private bool _rangeDirty;

        // Runs the query on a background thread, serialized by the lock.
        private async Task<T> RunDbAsync<T>(Func<Task<T>> query)
        {
            await _dbLock.WaitAsync();
            try 
            { 
                return await Task.Run(query); 
            }
            finally 
            { 
                _dbLock.Release(); 
            }
        }

        public async Task InitializeAsync()
        {
            EndDate = StartDate.AddDays(DayCount - 1);
            Rooms = (await RunDbAsync(() => _roomService.GetAllActiveRoomAsync())).ToList();
            _rangeDirty = true;
            await RefreshAsync();
        }


        // using atomic version keeps track in which order the request came newest
        // if newer request comes, the running one gets discarded
        private async Task RefreshAsync()
        {
            var version = Interlocked.Increment(ref _loadVersion);
            await _dbLock.WaitAsync();  // waits for a lock
            try
            {
                if (version != _loadVersion) return;       // newer request already queued, so discard

                var start = StartDate;
                var end = EndDate;
                var loaded = await Task.Run(() =>
                    _reservationService.GetReservationsByDateAsync(start, end.AddDays(1)));

                if (version != _loadVersion) return;       // newer request already queued, so discard

                Reservations = loaded;
                if (_rangeDirty)
                {
                    // dates changed, data stays the same
                    _rangeDirty = false;
                    _visibleDates = BuildDates(start, end);
                    OnPropertyChanged(nameof(VisibleDates));
                }
                else
                {
                    // dates stay the same, data changed
                    OnPropertyChanged(nameof(Reservations));
                }
            }
            finally { _dbLock.Release(); }
        }

        private static List<DateTime> BuildDates(DateTime start, DateTime end)
        {
            var days = (end.Date - start.Date).Days + 1;
            var list = new List<DateTime>(days);
            for (var i = 0; i < days; i++)
                list.Add(start.Date.AddDays(i));
            return list;
        }


        // Single entry point for every range change (arrows, +/- days, date picker).
        private async Task SetRangeAsync(DateTime start, int dayCount)
        {
            DayCount = dayCount;
            StartDate = start;
            EndDate = start.AddDays(dayCount - 1);
            OnPropertyChanged(nameof(DateRangeLabel));   
            _rangeDirty = true;
            await RefreshAsync();
        }

        [RelayCommand]
        private Task ShiftForwardAsync() => SetRangeAsync(StartDate.AddDays(1), DayCount);

        [RelayCommand]
        private Task ShiftBackwardAsync() => SetRangeAsync(StartDate.AddDays(-1), DayCount);

        [RelayCommand]
        private Task IncreaseDaysAsync()
            => DayCount >= 28 ? Task.CompletedTask : SetRangeAsync(StartDate, DayCount + 1);

        [RelayCommand]
        private Task DecreaseDaysAsync()
            => DayCount <= 5 ? Task.CompletedTask : SetRangeAsync(StartDate, DayCount - 1);

        [RelayCommand]
        private async Task SetDateRangeAsync(object param)
        {
            if (param is not (DateTime start, DateTime end)) return;
            await SetRangeAsync(start.Date, (end.Date - start.Date).Days + 1);
        }

        [RelayCommand]
        private async Task OpenDatePickerAsync()
        {
            var pickerViewModel = _serviceProvider.GetRequiredService<DateRangePickerViewModel>();
            pickerViewModel.Initialize(StartDate, EndDate);
            pickerViewModel.OnRangeSelected = async (start, end) =>
                await SetRangeAsync(start.Date, (end.Date - start.Date).Days + 1);

            var popup = new DateRangePickerPopup(pickerViewModel);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }


        [RelayCommand]
        private async Task OpenNewReservationAsync()
        {
            _reservationViewModel.SetRooms(Rooms);
            _reservationViewModel.InitializeNewReservationPopup();
            _reservationViewModel.OnSaved = async () => await RefreshAsync();
            await ShowPopupAsync();
        }

        [RelayCommand]
        private async Task OpenNewReservationFromCellAsync(object param)
        {
            if (param is not ValueTuple<RoomDTO, DateTime> tuple) return;
            var (room, dateTime) = tuple;

            _reservationViewModel.SetRooms(Rooms);
            _reservationViewModel.InitializeNewReservationPopup(room, dateTime);
            _reservationViewModel.OnSaved = async () => await RefreshAsync();
            await ShowPopupAsync();
        }

        [RelayCommand]
        private async Task OpenEditReservationAsync(ReservationDTO reservation)
        {
            _reservationViewModel.SetRooms(Rooms);
            await _reservationViewModel.InitializeEditReservationPopup(reservation);
            _reservationViewModel.OnSaved = async () => await RefreshAsync();
            await ShowPopupAsync();
        }

        private async Task ShowPopupAsync()
        {
            var guestPopupViewModel = _serviceProvider.GetRequiredService<GuestPopupViewModel>();
            var popup = new ReservationPopup(_reservationViewModel, guestPopupViewModel, _serviceProvider);
            await Application.Current!.MainPage!.ShowPopupAsync(popup);
        }
    }
}