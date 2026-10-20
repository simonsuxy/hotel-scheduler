using System.ComponentModel;
using System.Diagnostics;
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
        private static readonly Dictionary<string, Color> ColorCache = [];
        private static readonly Color AccentColor = Color.FromArgb("#4C4EE7");
        private static readonly Color MutedColor = Color.FromArgb("#888888");
        private static readonly Color FaintColor = Color.FromArgb("#AAAAAA");
        private List<DateTime> _shownDates = [];
        private int _lastDayCount;
        private string _roomSignature = "";
        private readonly List<Label> _dateLabels = [];
        private readonly List<View> _barElements = [];

        // Newer refresh invalidates an older one that is still computing
        private int _buildTicket;

        private sealed record CellInfo(RoomDTO Room, int HalfCol);
        private sealed record BarPosition(int Row, int Column, int Span, string Hex, ReservationDTO Reservation);

        public CalendarView(CalendarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // The VM raises its notification when loading finishes, which triggers the first build.
            Loaded += async (s, e) => await _viewModel.InitializeAsync();
        }

        // Listens ONLY to the two "data ready" signals. StartDate/EndDate changes are ignored.
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(CalendarViewModel.VisibleDates)
                               or nameof(CalendarViewModel.Reservations))
            {
                MainThread.BeginInvokeOnMainThread(() => _ = RefreshGridAsync());
            }
        }

        private async Task RefreshGridAsync()
        {
            try
            {
                var ticket = ++_buildTicket;

                // Snapshot on the UI thread (the VM replaces these lists, never mutates them)
                var dates = _viewModel.VisibleDates;
                var rooms = _viewModel.Rooms;
                var reservations = _viewModel.Reservations;
                if (dates.Count == 0) return;

                // Pure math over the snapshot, no UI objects touched, so it can run off-thread
                var bars = await Task.Run(() => ComputeBarPositions(dates, rooms, reservations));

                if (ticket != _buildTicket) return;   // a newer refresh started meanwhile

                // Creating/adding UI elements has to stay on the UI thread
                CalendarGrid.BatchBegin();            // one layout pass for all changes below
                try
                {
                    _shownDates = dates;

                    // only when day count or rooms change
                    if (NeedsStructureRebuild(dates, rooms))
                        RebuildStructure(dates, rooms);

                    UpdateDateLabels(dates);          // edits existing labels in place
                    ApplyBars(bars);
                }
                finally
                {
                    CalendarGrid.BatchCommit();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Calendar refresh failed: {ex}");
            }
        }

        private bool NeedsStructureRebuild(List<DateTime> dates, List<RoomDTO> rooms)
            => dates.Count != _lastDayCount || RoomSignature(rooms) != _roomSignature;

        // Changes when rooms are added/removed/renamed/reordered
        private static string RoomSignature(List<RoomDTO> rooms)
            => string.Join('|', rooms.Select(r => $"{r.Id}:{r.Name}"));

        // ---------- Pure computation (runs on a background thread) ----------

        private static List<BarPosition> ComputeBarPositions(
            List<DateTime> dates, List<RoomDTO> rooms, List<ReservationDTO> reservations)
        {
            var result = new List<BarPosition>(reservations.Count);
            if (dates.Count == 0) return result;

            var first = dates[0].Date;
            var last = dates[^1].Date;
            var lastHalfCol = dates.Count * 2 - 1;

            // One pass over reservations + O(1) room lookup (rooms start at grid row 2)
            var rowByRoom = new Dictionary<int, int>(rooms.Count);
            for (int i = 0; i < rooms.Count; i++)
                rowByRoom[rooms[i].Id] = i + 2;

            foreach (var res in reservations)
            {
                if (!rowByRoom.TryGetValue(res.RoomId, out var row)) continue;
                if (res.CheckOut.Date < first || res.CheckIn.Date > last) continue;

                // Days are consecutive, so the index is simply "days since the first day".
                // No FindIndex scan and no dictionary needed. Math.Max/Min clamp to the visible range.
                int startDay = Math.Max(0, (res.CheckIn.Date - first).Days);
                int endDay = Math.Min(dates.Count - 1, (res.CheckOut.Date - first).Days);

                // Each day has 2 columns: even = AM, odd = PM
                int startCol = startDay * 2;
                if (res.CheckIn.Date >= first && res.CheckIn.Hour >= 12)
                    startCol++;

                int endCol = endDay * 2;
                if (res.CheckOut.Date <= last && res.CheckOut.Hour > 12)
                    endCol++;
                else if (res.CheckOut.Date > last)
                    endCol = lastHalfCol;

                var span = Math.Max(1, endCol - startCol + 1);
                var hex = string.IsNullOrWhiteSpace(res.Color) ? "#4C4EE7" : res.Color;

                result.Add(new BarPosition(row, 1 + startCol, span, hex, res));
            }

            return result;
        }

        // ---------- UI building (UI thread only) ----------

        private void RebuildStructure(List<DateTime> dates, List<RoomDTO> rooms)
        {
            CalendarGrid.Children.Clear();
            CalendarGrid.ColumnDefinitions.Clear();
            CalendarGrid.RowDefinitions.Clear();
            _dateLabels.Clear();
            _barElements.Clear();

            int days = dates.Count;

            CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = RoomColumnWidth });
            for (int i = 0; i < days * 2; i++)
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = HeaderHeight });
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = SubHeaderHeight });
            foreach (var _ in rooms)
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = RowHeight });

            // Header elements: created once here. Only the date label text changes later.
            for (int i = 0; i < days; i++)
            {
                var col = 1 + i * 2;

                var dateLabel = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 12
                };
                Grid.SetRow(dateLabel, 0);
                Grid.SetColumn(dateLabel, col);
                Grid.SetColumnSpan(dateLabel, 2);
                CalendarGrid.Children.Add(dateLabel);
                _dateLabels.Add(dateLabel);

                AddSubHeader("AM", col);
                AddSubHeader("PM", col + 1);

                var separator = new BoxView
                {
                    BackgroundColor = GetColor("#E0E0E0"),
                    WidthRequest = 1,
                    HorizontalOptions = LayoutOptions.Start
                };
                Grid.SetRow(separator, 0);
                Grid.SetColumn(separator, col);
                Grid.SetRowSpan(separator, 2 + rooms.Count);
                CalendarGrid.Children.Add(separator);
            }

            // Room rows: label, separator line, and one tappable cell per half-day
            for (int r = 0; r < rooms.Count; r++)
            {
                var room = rooms[r];
                var row = r + 2;

                var roomLabel = new Label
                {
                    Text = room.Name,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = GetColor("#1A1A1A")
                };
                Grid.SetRow(roomLabel, row);
                Grid.SetColumn(roomLabel, 0);
                CalendarGrid.Children.Add(roomLabel);

                var rowSep = new BoxView
                {
                    BackgroundColor = GetColor("#F0F0F0"),
                    HeightRequest = 1,
                    VerticalOptions = LayoutOptions.End
                };
                Grid.SetRow(rowSep, row);
                Grid.SetColumn(rowSep, 0);
                Grid.SetColumnSpan(rowSep, 1 + days * 2);
                CalendarGrid.Children.Add(rowSep);

                for (int h = 0; h < days * 2; h++)
                {
                    var cell = MakeCell(room, h);
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, 1 + h);
                    CalendarGrid.Children.Add(cell);
                }
            }

            _lastDayCount = days;
            _roomSignature = RoomSignature(rooms);
        }

        private void AddSubHeader(string text, int col)
        {
            var label = new Label
            {
                Text = text,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 10,
                TextColor = FaintColor
            };
            Grid.SetRow(label, 1);
            Grid.SetColumn(label, col);
            CalendarGrid.Children.Add(label);
        }

        // Shifting the range only needs new text on existing labels, not new elements
        private void UpdateDateLabels(List<DateTime> dates)
        {
            for (int i = 0; i < dates.Count && i < _dateLabels.Count; i++)
            {
                var date = dates[i];
                var isToday = date.Date == DateTime.Today;
                var label = _dateLabels[i];

                label.Text = $"{date:ddd} {date.Day}";
                label.TextColor = isToday ? AccentColor : MutedColor;
                label.FontAttributes = isToday ? FontAttributes.Bold : FontAttributes.None;
            }
        }

        private void ApplyBars(List<BarPosition> bars)
        {
            // Remove the old bars via our own list (no scan over Children)
            foreach (var old in _barElements)
                CalendarGrid.Children.Remove(old);
            _barElements.Clear();

            foreach (var b in bars)
            {
                var bar = new Border
                {
                    BackgroundColor = GetColor(b.Hex),
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    Padding = new Thickness(14, 0, 10, 0),
                    Margin = new Thickness(1, 2),
                    VerticalOptions = LayoutOptions.Fill,
                    Content = new Label
                    {
                        Text = b.Reservation.GuestName,
                        TextColor = Colors.White,
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center,
                        LineBreakMode = LineBreakMode.TailTruncation,
                        Margin = new Thickness(4, 0, 0, 0)
                    }
                };

                // One shared handler method; the reservation travels in CommandParameter,
                // so there is no per-bar closure
                var tap = new TapGestureRecognizer { CommandParameter = b.Reservation };
                tap.Tapped += OnBarTapped;
                bar.GestureRecognizers.Add(tap);

                Grid.SetRow(bar, b.Row);
                Grid.SetColumn(bar, b.Column);
                Grid.SetColumnSpan(bar, b.Span);
                CalendarGrid.Children.Add(bar);   // added last, so bars sit above the cells
                _barElements.Add(bar);
            }
        }

        private ContentView MakeCell(RoomDTO room, int halfCol)
        {
            var cell = new ContentView { BackgroundColor = Colors.Transparent };
            var tap = new TapGestureRecognizer { CommandParameter = new CellInfo(room, halfCol) };
            tap.Tapped += OnCellTapped;
            cell.GestureRecognizers.Add(tap);
            return cell;
        }

        private void OnBarTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is ReservationDTO reservation)
                _viewModel.OpenEditReservationCommand.Execute(reservation);
        }

        private void OnCellTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not CellInfo cell) return;
            var dayIndex = cell.HalfCol / 2;
            if (dayIndex >= _shownDates.Count) return;

            // The date is resolved at TAP time, not baked into the cell. That is why cells
            // survive a range shift without being recreated (before, every shift rebuilt them all).
            var dateTime = _shownDates[dayIndex].Date.AddHours(cell.HalfCol % 2 == 0 ? 8 : 14);
            _viewModel.OpenNewReservationFromCellCommand.Execute((cell.Room, dateTime));
        }

        private static Color GetColor(string hex)
        {
            if (!ColorCache.TryGetValue(hex, out var color))
            {
                color = Color.FromArgb(hex);
                ColorCache[hex] = color;
            }
            return color;
        }
    }
}