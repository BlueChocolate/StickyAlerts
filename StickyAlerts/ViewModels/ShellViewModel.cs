using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyAlerts.Services;
using StickyAlerts.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StickyAlerts.ViewModels
{
    public partial class ShellViewModel : ObservableObject
    {
        private readonly IShellService _shellService;
        private readonly IAlertService _alertService;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private AlertsView _alertsView;

        [ObservableProperty]
        private SettingsView _settingsView;

        [ObservableProperty]
        private bool _isSettingsVisible;

        [ObservableProperty]
        private bool _isNotifyIconVisible;

        public ShellViewModel(IShellService shellService, IAlertService alertService, AlertsView alertsView, SettingsView settingsView)
        {
            _shellService = shellService;
            _alertService = alertService;
            _alertsView = alertsView;
            _settingsView = settingsView;
            Title = "Sticky Alerts";
            IsSettingsVisible = false;
        }

        [RelayCommand]
        private void ShowShell()
        {
            _shellService.ShowShell();
        }

        [RelayCommand]
        private void HideShell()
        {
            _shellService.HideShell();
        }

        [RelayCommand]
        private void Exit()
        {
            Environment.Exit(0);
        }

        [RelayCommand]
        private void AddAlert()
        {
            _alertService.Add();
        }

        [RelayCommand]
        private void SwitchToAlertsView()
        {
            IsSettingsVisible = false;
        }

        [RelayCommand]
        private void SwitchToSettingsView()
        {
            IsSettingsVisible = true;
        }
    }
}
