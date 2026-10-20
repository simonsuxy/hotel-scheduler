using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Text;
using hotel_scheduler.Views;



namespace hotel_scheduler.ViewModels
{
    public partial class SettingsViewModel(SettingsService settingsService,
        ReservationService reservationService, DateRangePickerViewModel dateRangePickerViewModel) : ObservableObject
    {
        private readonly SettingsService _settingsService = settingsService;
        private readonly ReservationService _reservationService = reservationService;
        private readonly DateRangePickerViewModel _dateRangePickerViewModel = dateRangePickerViewModel;
        [ObservableProperty] public partial SettingsDTO CurrentSettings { get; set; } = new();
        [ObservableProperty] public partial string StatusMessage { get; set; } = "";
        [ObservableProperty] public partial bool ShowStatus { get; set; } = false;
        [ObservableProperty] public partial string StatusColor { get; set; } = "#43A047";

        public async Task InitializeAsync()
        {
            CurrentSettings = await _settingsService.GetSettingsAsync();
            ShowStatus = false;
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            await _settingsService.UpdateSettingsAsync(CurrentSettings);
            ShowMessage("Settings saved!", "#43A047");
        }

        [RelayCommand]
        private async Task ExportCsvAsync()
        {
            _dateRangePickerViewModel.Initialize(DateTime.Today.AddDays(-14), DateTime.Today.AddDays(14));
            _dateRangePickerViewModel.OnRangeSelected = async (start, end) =>
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
            };
            await Application.Current!.MainPage!.ShowPopupAsync(new DateRangePickerPopup(_dateRangePickerViewModel));
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
                    ShowMessage("Database backup saved to desktop.", "#43A047");
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

        private async void ShowMessage(string msg, string colorhex)
        {
            StatusMessage = msg;
            StatusColor = colorhex;
            ShowStatus = true;
            await Task.Delay(4000);
            ShowStatus = false;
        }


    }
}
