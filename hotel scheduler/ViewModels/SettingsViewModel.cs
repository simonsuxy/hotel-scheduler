using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using hotel_scheduler.Views;
using System.Text;

namespace hotel_scheduler.ViewModels
{
    public partial class SettingsViewModel(SettingsService settingsService, ReservationService reservationService,
        DateRangePickerViewModel dateRangePickerViewModel) : ObservableObject
    {
        private readonly SettingsService _settingsService = settingsService;
        private readonly ReservationService _reservationService = reservationService;
        private readonly DateRangePickerViewModel _dateRangePickerViewModel = dateRangePickerViewModel;

        [ObservableProperty] public partial string HotelName { get; set; } = "Hotel";
        [ObservableProperty] public partial string Email { get; set; } = "";
        [ObservableProperty] public partial string Address { get; set; } = "";
        [ObservableProperty] public partial string Phone { get; set; } = "";
        [ObservableProperty] public partial string Currency { get; set; } = "EUR";
        [ObservableProperty] public partial int DefaultCalendarDays { get; set; } = 14;
        [ObservableProperty] public partial TimeSpan DefaultCheckInTime { get; set; } = new TimeSpan(14, 0, 0);
        [ObservableProperty] public partial TimeSpan DefaultCheckOutTime { get; set; } = new TimeSpan(10, 0, 0);
        [ObservableProperty] public partial string StatusMessage { get; set; } = "";
        [ObservableProperty] public partial bool ShowStatus { get; set; } = false;
        [ObservableProperty] public partial string StatusColor { get; set; } = "#43A047";

        public async Task InitializeAsync()
        {
            try
            {
                var settings = await _settingsService.GetSettingsAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    HotelName = settings.HotelName ?? "Hotel";
                    Email = settings.Email ?? "";
                    Address = settings.Address ?? "";
                    Phone = settings.Phone ?? "";
                    Currency = string.IsNullOrWhiteSpace(settings.Currency) ? "EUR" : settings.Currency;
                    DefaultCalendarDays = settings.DefaultCalendarDays < 1 ? 14 : settings.DefaultCalendarDays;
                    DefaultCheckInTime = settings.DefaultCheckInTime;
                    DefaultCheckOutTime = settings.DefaultCheckOutTime;

                    ShowStatus = false;
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Error loading settings: {ex.Message}", "#E53935");
            }
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                var dto = new SettingsDTO
                {
                    HotelName = HotelName.Trim(),
                    Email = Email.Trim(),
                    Address = Address.Trim(),
                    Phone = Phone.Trim(),
                    Currency = string.IsNullOrWhiteSpace(Currency) ? "EUR" : Currency.Trim(),
                    DefaultCalendarDays = DefaultCalendarDays < 1 ? 14 : DefaultCalendarDays,
                    DefaultCheckInTime = DefaultCheckInTime,
                    DefaultCheckOutTime = DefaultCheckOutTime
                };

                await _settingsService.UpdateSettingsAsync(dto);
                ShowMessage("Settings saved successfully!", "#43A047");
            }
            catch (Exception ex)
            {
                ShowMessage($"Error saving settings: {ex.Message}", "#E53935");
            }
        }

        [RelayCommand]
        private async Task ExportCsvAsync()
        {
            try
            {
                _dateRangePickerViewModel.Initialize(DateTime.Today.AddDays(-14), DateTime.Today.AddDays(14));
                _dateRangePickerViewModel.OnRangeSelected = async (start, end) =>
                {
                    try
                    {
                        var reservations = await _reservationService.GetReservationsByDateAsync(start, end.AddDays(1));
                        var csvBuilder = new StringBuilder();
                        csvBuilder.AppendLine("Reservation ID,Room,Guest,Check-In,Check-Out,Nights,Total Price,Payment Status,Notes");

                        foreach (var r in reservations)
                        {
                            csvBuilder.AppendLine($"{r.Id},\"{r.RoomName}\",\"{r.GuestName}\",{r.CheckIn:yyyy-MM-dd},{r.CheckOut:yyyy-MM-dd},{r.Nights},{r.ComputedTotalPrice},{r.PaymentStatus},\"{r.Notes?.Replace("\"", "\"\"")}\"");
                        }

                        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvBuilder.ToString()));
                        await FileSaver.Default.SaveAsync($"Reservations_{start:yyyyMMdd}-{end:yyyyMMdd}.csv", stream);
                        ShowMessage("Export downloaded successfully.", "#43A047");
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"Export failed: {ex.Message}", "#E53935");
                    }
                };

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.ShowPopupAsync(new DateRangePickerPopup(_dateRangePickerViewModel));
                    }
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Export failed: {ex.Message}", "#E53935");
            }
        }

        [RelayCommand]
        private async Task BackupDatabaseAsync()
        {
            try
            {
                string dbPath = Path.GetFullPath("data.sqlite");
                if (File.Exists(dbPath))
                {
                    using var stream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    await FileSaver.Default.SaveAsync($"Backup_HotelData_{DateTime.Now:yyyyMMdd}.sqlite", stream);
                    ShowMessage("Database backup saved successfully.", "#43A047");
                }
                else
                {
                    ShowMessage("Database file 'data.sqlite' not found.", "#E53935");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Backup failed: {ex.Message}", "#E53935");
            }
        }

        [RelayCommand]
        private async Task RestoreDatabaseAsync()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    string dbPath = Path.GetFullPath("data.sqlite");
                    using var readStream = await result.OpenReadAsync();
                    using var writeStream = new FileStream(dbPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await readStream.CopyToAsync(writeStream);

                    ShowMessage("Database restored! Please restart the application.", "#F57F17");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Restore failed: {ex.Message} (Ensure no reservations are actively open)", "#E53935");
            }
        }

        private void ShowMessage(string msg, string colorHex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                StatusMessage = msg;
                StatusColor = colorHex;
                ShowStatus = true;
                await Task.Delay(4000);
                ShowStatus = false;
            });
        }
    }
}