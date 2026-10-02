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

        // Track whether structure needs full rebuild
        private int _lastDayCount = 0;
        private int _lastRoomCount = 0;

        public CalendarView(CalendarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            _viewModel.PropertyChanged += (s, e) =>
            {
                // Full rebuild when dates or rooms change
                if (e.PropertyName == nameof(CalendarViewModel.StartDate) ||
                    e.PropertyName == nameof(CalendarViewModel.VisibleDates))
                    MainThread.BeginInvokeOnMainThread(BuildGrid);

                // Bars only when reservations change
                else if (e.PropertyName == nameof(CalendarViewModel.Reservations))
                    MainThread.BeginInvokeOnMainThread(BuildReservationBars);
            };

            Loaded += async (s, e) =>
            {
                await _viewModel.InitializeAsync();
                BuildGrid();
            };
        }

        // Full rebuild — structure + bars
        public void BuildGrid()
        {
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();

            var dates = _viewModel.VisibleDates;
            var rooms = _viewModel.Rooms.ToList();

            _lastDayCount = dates.Count;
            _lastRoomCount = rooms.Count;

            // Column definitions
            CalendarGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = RoomColumnWidth });

            for (int i = 0; i < dates.Count * 2; i++)
                CalendarGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = GridLength.Star });

            // Row definitions
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = HeaderHeight });
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = SubHeaderHeight });
            foreach (var _ in rooms)
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = RowHeight });

            // Date headers
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

            // Room rows — label, separator, cells
            for (int rowIndex = 0; rowIndex < rooms.Count; rowIndex++)
            {
                var room = rooms[rowIndex];
                var gridRow = rowIndex + 2;

                var roomLabel = new Label
                {
                    Text = room.Name,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#1A1A1A")
                };
                CalendarGrid.SetRow(roomLabel, gridRow);
                CalendarGrid.SetColumn(roomLabel, 0);
                CalendarGrid.Children.Add(roomLabel);

                var rowSep = new BoxView
                {
                    BackgroundColor = Color.FromArgb("#F0F0F0"),
                    HeightRequest = 1,
                    VerticalOptions = LayoutOptions.End
                };
                CalendarGrid.SetRow(rowSep, gridRow);
                CalendarGrid.SetColumn(rowSep, 0);
                CalendarGrid.SetColumnSpan(rowSep, 1 + dates.Count * 2);
                CalendarGrid.Children.Add(rowSep);

                for (int i = 0; i < dates.Count; i++)
                {
                    var capturedRoom = room;
                    var amDate = dates[i].AddHours(8);
                    var pmDate = dates[i].AddHours(14);

                    var amCell = MakeCell(capturedRoom, amDate);
                    CalendarGrid.SetRow(amCell, gridRow);
                    CalendarGrid.SetColumn(amCell, 1 + (i * 2));
                    CalendarGrid.Children.Add(amCell);

                    var pmCell = MakeCell(capturedRoom, pmDate);
                    CalendarGrid.SetRow(pmCell, gridRow);
                    CalendarGrid.SetColumn(pmCell, 1 + (i * 2) + 1);
                    CalendarGrid.Children.Add(pmCell);
                }
            }

            // Draw bars after structure is built
            BuildReservationBars();
        }

        // Only redraws bars — removes old bars and redraws new ones
        // without touching the grid structure (columns, rows, headers, cells)
        public void BuildReservationBars()
        {
            var dates = _viewModel.VisibleDates;
            var rooms = _viewModel.Rooms.ToList();
            var reservations = _viewModel.Reservations.ToList();

            // If structure changed since last build, do a full rebuild instead
            if (dates.Count != _lastDayCount || rooms.Count != _lastRoomCount)
            {
                BuildGrid();
                return;
            }

            // Remove only existing bar elements — identified by tag
            var barsToRemove = CalendarGrid.Children
                .OfType<Border>()
                .Where(v => v.ClassId == "reservation-bar")
                .ToList();

            foreach (var bar in barsToRemove)
                CalendarGrid.Children.Remove(bar);

            // Redraw bars
            for (int rowIndex = 0; rowIndex < rooms.Count; rowIndex++)
            {
                var room = rooms[rowIndex];
                var gridRow = rowIndex + 2;

                var roomReservations = reservations.Where(r => r.RoomId == room.Id);

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
                    if (res.CheckOut.Date <= dates[^1].Date && res.CheckOut.Hour >= 12)
                        endHalfCol++;
                    else if (res.CheckOut.Date > dates[^1].Date)
                        endHalfCol = (dates.Count * 2) - 1;

                    var halfColSpan = Math.Max(1, (endHalfCol - startHalfCol) + 1);

                    var bar = new Border
                    {
                        // Tag so we can find and remove bars without touching other elements
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