using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace hotel_scheduler.Views
{
    public partial class CalendarView : ContentView
    {
        private readonly CalendarViewModel _viewModel;

        private const double RoomColumnWidth = 80;
        private const double RowHeight = 60;
        private const double HeaderHeight = 48;
        private const double SubHeaderHeight = 28;

        // Track structure changes including start date
        private int _lastDayCount = 0;
        private int _lastRoomCount = 0;
        private DateTime _lastStartDate = DateTime.MinValue;
        private readonly Dictionary<int, List<View>> _roomStaticElements = [];

        private CancellationTokenSource? _rebuildDebounce;

        public CalendarView(CalendarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CalendarViewModel.StartDate) ||
                    e.PropertyName == nameof(CalendarViewModel.VisibleDates))
                    MainThread.BeginInvokeOnMainThread(BuildGrid);

                else if (e.PropertyName == nameof(CalendarViewModel.Reservations))
                    MainThread.BeginInvokeOnMainThread(BuildReservationBars);
            };

            Loaded += async (s, e) =>
            {
                await _viewModel.InitializeAsync();
                BuildGrid();
            };
        }

        private async Task DebounceRebuildAsync()
        {
            _rebuildDebounce?.Cancel();
            _rebuildDebounce = new CancellationTokenSource();
            try
            {
                await Task.Delay(150, _rebuildDebounce.Token);
                BuildGrid();
            }
            catch (TaskCanceledException) { }
        }

        private bool NeedsStructureRebuild(List<DateTime> dates, List<RoomDTO> rooms)
        {
            if (dates.Count == 0) return false;
            return dates.Count != _lastDayCount
                || rooms.Count != _lastRoomCount
                || dates[0].Date != _lastStartDate;
        }

        public void BuildGrid()
        {
            var dates = _viewModel.VisibleDates;
            var rooms = _viewModel.Rooms.ToList();

            if (dates.Count == 0) return;

            CalendarGrid.BatchBegin();
            try
            {
                CalendarGrid.Children.Clear();

                if (NeedsStructureRebuild(dates, rooms))
                {
                    CalendarGrid.ColumnDefinitions.Clear();
                    CalendarGrid.RowDefinitions.Clear();

                    CalendarGrid.ColumnDefinitions.Add(
                        new ColumnDefinition { Width = RoomColumnWidth });

                    for (int i = 0; i < dates.Count * 2; i++)
                        CalendarGrid.ColumnDefinitions.Add(
                            new ColumnDefinition { Width = GridLength.Star });

                    CalendarGrid.RowDefinitions.Add(
                        new RowDefinition { Height = HeaderHeight });
                    CalendarGrid.RowDefinitions.Add(
                        new RowDefinition { Height = SubHeaderHeight });
                    foreach (var _ in rooms)
                        CalendarGrid.RowDefinitions.Add(
                            new RowDefinition { Height = RowHeight });

                    _roomStaticElements.Clear();

                    _lastDayCount = dates.Count;
                    _lastRoomCount = rooms.Count;
                    _lastStartDate = dates[0].Date;
                }

                for (int i = 0; i < dates.Count; i++)
                {
                    var date = dates[i];
                    var isToday = date.Date == DateTime.Today;
                    var dateCol = 1 + (i * 2);

                    var dateLabel = new Label
                    {
                        Text = $"{date:ddd} {date.Day}",
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 12,
                        TextColor = isToday
                            ? Color.FromArgb("#4C4EE7")
                            : Color.FromArgb("#888888"),
                        FontAttributes = isToday ? FontAttributes.Bold : FontAttributes.None
                    };
                    CalendarGrid.SetRow(dateLabel, 0);
                    CalendarGrid.SetColumn(dateLabel, dateCol);
                    CalendarGrid.SetColumnSpan(dateLabel, 2);
                    CalendarGrid.Children.Add(dateLabel);

                    var amLabel = new Label
                    {
                        Text = "AM",
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 10,
                        TextColor = Color.FromArgb("#AAAAAA")
                    };
                    CalendarGrid.SetRow(amLabel, 1);
                    CalendarGrid.SetColumn(amLabel, dateCol);
                    CalendarGrid.Children.Add(amLabel);

                    var pmLabel = new Label
                    {
                        Text = "PM",
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 10,
                        TextColor = Color.FromArgb("#AAAAAA")
                    };
                    CalendarGrid.SetRow(pmLabel, 1);
                    CalendarGrid.SetColumn(pmLabel, dateCol + 1);
                    CalendarGrid.Children.Add(pmLabel);

                    var separator = new BoxView
                    {
                        BackgroundColor = Color.FromArgb("#E0E0E0"),
                        WidthRequest = 1,
                        HorizontalOptions = LayoutOptions.Start
                    };
                    CalendarGrid.SetRow(separator, 0);
                    CalendarGrid.SetColumn(separator, dateCol);
                    CalendarGrid.SetRowSpan(separator, 2 + rooms.Count);
                    CalendarGrid.Children.Add(separator);
                }

                for (int rowIndex = 0; rowIndex < rooms.Count; rowIndex++)
                {
                    var room = rooms[rowIndex];
                    var gridRow = rowIndex + 2;

                    if (!_roomStaticElements.TryGetValue(room.Id, out var cachedElements))
                    {
                        cachedElements = [];

                        var roomLabel = new Label
                        {
                            Text = room.Name,
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.Center,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#1A1A1A")
                        };
                        Grid.SetRow(roomLabel, gridRow);
                        Grid.SetColumn(roomLabel, 0);
                        cachedElements.Add(roomLabel);

                        var rowSep = new BoxView
                        {
                            BackgroundColor = Color.FromArgb("#F0F0F0"),
                            HeightRequest = 1,
                            VerticalOptions = LayoutOptions.End
                        };
                        Grid.SetRow(rowSep, gridRow);
                        Grid.SetColumn(rowSep, 0);
                        Grid.SetColumnSpan(rowSep, 1 + _lastDayCount * 2);
                        cachedElements.Add(rowSep);

                        for (int i = 0; i < _lastDayCount; i++)
                        {
                            var capturedRoom = room;
                            var amDate = _viewModel.VisibleDates[i].AddHours(8);
                            var pmDate = _viewModel.VisibleDates[i].AddHours(14);

                            var amCell = MakeCell(capturedRoom, amDate);
                            Grid.SetRow(amCell, gridRow);
                            Grid.SetColumn(amCell, 1 + (i * 2));
                            cachedElements.Add(amCell);

                            var pmCell = MakeCell(capturedRoom, pmDate);
                            Grid.SetRow(pmCell, gridRow);
                            Grid.SetColumn(pmCell, 1 + (i * 2) + 1);
                            cachedElements.Add(pmCell);
                        }

                        _roomStaticElements[room.Id] = cachedElements;
                    }
                    else
                    {
                        foreach (var el in cachedElements)
                            Grid.SetRow(el, gridRow);
                    }

                    foreach (var el in cachedElements)
                        CalendarGrid.Children.Add(el);
                }
            }
            finally
            {
                CalendarGrid.BatchCommit();
            }
            BuildReservationBars();
        }

        public void BuildReservationBars()
        {
            var dates = _viewModel.VisibleDates;
            var rooms = _viewModel.Rooms.ToList();
            var reservations = _viewModel.Reservations.ToList();

            if (dates.Count == 0) return;

            if (NeedsStructureRebuild(dates, rooms))
            {
                BuildGrid();
                return;
            }

            CalendarGrid.BatchBegin();
            try
            {
                var barsToRemove = CalendarGrid.Children
                    .OfType<Border>()
                    .Where(v => v.ClassId == "reservation-bar")
                    .ToList();

                foreach (var bar in barsToRemove)
                    CalendarGrid.Children.Remove(bar);

                var reservationsByRoom = reservations
                    .GroupBy(r => r.RoomId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                for (int rowIndex = 0; rowIndex < rooms.Count; rowIndex++)
                {
                    var room = rooms[rowIndex];
                    var gridRow = rowIndex + 2;

                    if (!reservationsByRoom.TryGetValue(room.Id, out var roomReservations))
                        continue;

                    foreach (var res in roomReservations)
                    {
                        if (res.CheckOut.Date < dates[0].Date ||
                            res.CheckIn.Date > dates[^1].Date) continue;

                        var clampedStart = res.CheckIn.Date < dates[0].Date
                            ? dates[0].Date : res.CheckIn.Date;
                        var clampedEnd = res.CheckOut.Date > dates[^1].Date
                            ? dates[^1].Date : res.CheckOut.Date;

                        var startDayIndex = dates.FindIndex(d => d.Date == clampedStart.Date);
                        var endDayIndex = dates.FindIndex(d => d.Date == clampedEnd.Date);

                        if (startDayIndex == -1 || endDayIndex == -1) continue;

                        int startHalfCol = startDayIndex * 2;
                        if (res.CheckIn.Date >= dates[0].Date && res.CheckIn.Hour >= 12)
                            startHalfCol++;

                        int endHalfCol = endDayIndex * 2;
                        // Use > 12 so 12:00 (AM CheckOut) stays in the AM half-column
                        if (res.CheckOut.Date <= dates[^1].Date && res.CheckOut.Hour > 12)
                            endHalfCol++;
                        else if (res.CheckOut.Date > dates[^1].Date)
                            endHalfCol = (dates.Count * 2) - 1;

                        var halfColSpan = Math.Max(1, (endHalfCol - startHalfCol) + 1);

                        var bar = new Border
                        {
                            ClassId = "reservation-bar",
                            BackgroundColor = Color.FromArgb(res.Color),
                            StrokeShape = new RoundRectangle { CornerRadius = 8 },
                            Padding = new Thickness(14, 0, 10, 0),
                            Margin = new Thickness(1, 2),
                            VerticalOptions = LayoutOptions.Fill,
                            Content = new Label
                            {
                                Text = res.GuestName,
                                TextColor = Colors.White,
                                FontSize = 12,
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                LineBreakMode = LineBreakMode.TailTruncation,
                                Margin = new Thickness(4, 0, 0, 0)
                            }
                        };

                        var capturedRes = res;
                        var tap = new TapGestureRecognizer();
                        tap.Tapped += (s, e) =>
                            _viewModel.OpenEditReservationCommand.Execute(capturedRes);
                        bar.GestureRecognizers.Add(tap);

                        CalendarGrid.SetRow(bar, gridRow);
                        CalendarGrid.SetColumn(bar, 1 + startHalfCol);
                        CalendarGrid.SetColumnSpan(bar, halfColSpan);
                        CalendarGrid.Children.Add(bar);
                    }
                }
            }
            finally
            {
                CalendarGrid.BatchCommit();
            }
        }

        private ContentView MakeCell(RoomDTO room, DateTime dateTime)
        {
            var cell = new ContentView { BackgroundColor = Colors.Transparent };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
                _viewModel.OpenNewReservationFromCellCommand.Execute((room, dateTime));
            cell.GestureRecognizers.Add(tap);
            return cell;
        }
    }
}