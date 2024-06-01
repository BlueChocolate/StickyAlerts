using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyAlerts.Models;
using StickyAlerts.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StickyAlerts.ViewModels
{
    public partial class AlertsViewModel : ObservableObject
    {
        public ObservableCollection<Alert> Alerts { get; }
        public IAlertService AlertService { get; }
        public AlertsViewModel(IAlertService alertService)
        {
            AlertService = alertService;
            //Alerts = _alertService.Alerts;
        }
    }
}
