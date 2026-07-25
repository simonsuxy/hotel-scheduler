using DAL.Entities;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace hotel_scheduler.Views
{
    public partial class CalendarView : ContentView
    {
        private readonly CalendarViewModel _viewModel;
        private CancellationTokenSource? _buildGridDebounce;

        private const double RoomColumnWidth = 80;
        private const double RowHeight = 60;
        private const double HeaderHeight = 48;
        private const double SubHeaderHeight = 28;

        public static readonly List<string> ReservationColors =
        [
            "#4C9BE8", "#56C596", "#F4A261", "#E76F51",
            "#A78BFA", "#F472B6", "#38BDF8", "#FBBF24"
        ];

        public CalendarView(CalendarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CalendarViewModel.Reservations) ||
                    e.PropertyName == nameof(CalendarViewModel.VisibleDates) ||
                    e.PropertyName == nameof(CalendarViewModel.StartDate) ||
                    e.PropertyName == nameof(CalendarViewModel.EndDate))
                    DebouncedBuildGrid();
            };

            Loaded += async (s, e) =>
            {
                await _viewModel.InitializeAsync();
                BuildGrid();
            };
        }

        // Debounce BuildGrid to avoid multiple calls within a short time
        private void DebouncedBuildGrid()
        {
            _buildGridDebounce?.Cancel();
            _buildGridDebounce = new CancellationTokenSource();
            var token = _buildGridDebounce.Token;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Task.Delay(50, token); // Wait 50ms for more PropertyChanged events
                    if (!token.IsCancellationRequested)
                    {
                        BuildGrid();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Debounce was cancelled, another call came in
                }
            });
        }

        public void BuildGrid()
        {
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();

            var dates = _viewModel.VisibleDates;
            var rooms = _viewModel.Rooms.ToList();
            var reservations = _viewModel.Reservations.ToList();

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

            for (int i = 0; i < dates.Count; i++)
            {
                var date = dates[i];
                var isToday = date.Date == DateTime.Today;
                var dateColumn = 1 + (i * 2);

                var dateLabel = new Label
                {
                    Text = $"{date:ddd} {date.Day}",
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 12,
                    TextColor = isToday
                        ? Color.FromArgb("#4C4EE7")
                        : Color.FromArgb("#888888"),
                    FontAttributes = isToday
                        ? FontAttributes.Bold
                        : FontAttributes.None
                };
                CalendarGrid.SetRow(dateLabel, 0);
                CalendarGrid.SetColumn(dateLabel, dateColumn);
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
                CalendarGrid.SetColumn(amLabel, dateColumn);
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
                CalendarGrid.SetColumn(pmLabel, dateColumn + 1);
                CalendarGrid.Children.Add(pmLabel);

                var separator = new BoxView
                {
                    BackgroundColor = Color.FromArgb("#E0E0E0"),
                    WidthRequest = 1,
                    HorizontalOptions = LayoutOptions.Start
                };
                CalendarGrid.SetRow(separator, 0);
                CalendarGrid.SetColumn(separator, dateColumn);
                CalendarGrid.SetRowSpan(separator, 2 + rooms.Count);
                CalendarGrid.Children.Add(separator);
            }

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
                    var date = dates[i];
                    var capturedRoom = room;

                    var amDate = date.AddHours(8);
                    var amCell = MakeCell(capturedRoom, amDate);
                    CalendarGrid.SetRow(amCell, gridRow);
                    CalendarGrid.SetColumn(amCell, 1 + (i * 2));
                    CalendarGrid.Children.Add(amCell);

                    var pmDate = date.AddHours(14);
                    var pmCell = MakeCell(capturedRoom, pmDate);
                    CalendarGrid.SetRow(pmCell, gridRow);
                    CalendarGrid.SetColumn(pmCell, 1 + (i * 2) + 1);
                    CalendarGrid.Children.Add(pmCell);
                }

                var roomReservations = reservations.Where(r => r.RoomId == room.Id);

                foreach (var res in roomReservations)
                {
                    // Skip if reservation is completely outside the visible date window
                    if (res.CheckOut.Date < dates[0].Date || res.CheckIn.Date > dates[^1].Date)
                        continue;

                    // Determine target start/end dates clamped to visible boundaries
                    var clampedStart = res.CheckIn.Date < dates[0].Date ? dates[0].Date : res.CheckIn.Date;
                    var clampedEnd = res.CheckOut.Date > dates[^1].Date ? dates[^1].Date : res.CheckOut.Date;

                    var startDayIndex = dates.FindIndex(d => d.Date == clampedStart.Date);
                    var endDayIndex = dates.FindIndex(d => d.Date == clampedEnd.Date);

                    if (startDayIndex == -1 || endDayIndex == -1) continue;

                    // Calculate start slot index (0-based relative to date columns)
                    int startHalfCol = startDayIndex * 2;
                    if (res.CheckIn.Date >= dates[0].Date && res.CheckIn.Hour >= 12)
                    {
                        startHalfCol += 1; // Starts in PM
                    }

                    // Calculate end slot index
                    int endHalfCol = endDayIndex * 2;
                    if (res.CheckOut.Date <= dates[^1].Date)
                    {
                        // Standard checkouts happen in AM (< 12:00). 
                        // If checkout is in PM (>= 12:00), expand to PM slot.
                        if (res.CheckOut.Hour >= 12)
                        {
                            endHalfCol += 1;
                        }
                    }
                    else
                    {
                        endHalfCol = (dates.Count * 2) - 1; // Clamp to last visible PM slot
                    }

                    // Calculate span: inclusive count of half-day cells
                    var halfColSpan = Math.Max(1, (endHalfCol - startHalfCol) + 1);

                    var bar = new Border
                    {
                        BackgroundColor = Color.FromArgb(res.Color),
                        StrokeShape = new RoundRectangle { CornerRadius = 8 },
                        // Adjust padding: Top/Bottom set to 0, Left set to 14 to shift text inward
                        Padding = new Thickness(14, 0, 10, 0),
                        // Reduced vertical margin from 6 to 2 so the bar fills almost the full row height (60px)
                        Margin = new Thickness(1, 2),
                        VerticalOptions = LayoutOptions.Fill, // Stretches bar vertically across the row
                        Content = new Label
                        {
                            Text = res.GuestName,
                            TextColor = Colors.White,
                            FontSize = 12,
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            // Margin shifts the text position inside the frame further right if needed
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
            var cell = new ContentView
            {
                BackgroundColor = Colors.Transparent
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
                _viewModel.OpenNewReservationFromCellCommand
                    .Execute((room, dateTime));
            cell.GestureRecognizers.Add(tap);
            return cell;
        }
    }
}