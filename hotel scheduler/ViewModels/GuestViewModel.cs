using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hotel_scheduler.Models.Entities;
using hotel_scheduler.Models.Services.DatabaseServices;
using System.Collections.ObjectModel;

namespace hotel_scheduler.ViewModels
{
    public partial class GuestViewModel(GuestService guestService) : ObservableObject
    {
        private readonly GuestService _guestService = guestService;
    }
}
       