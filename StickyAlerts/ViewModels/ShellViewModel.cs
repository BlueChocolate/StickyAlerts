using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using StickyAlerts.Models;
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

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private AlertsViewModel _alertsViewModel;

        [ObservableProperty]
        private SettingsViewModel _settingsViewModel;

        [ObservableProperty]
        private bool _isSettingsVisible;

        [ObservableProperty]
        private bool _isNotifyIconVisible;

        public ShellViewModel(IShellService shellService, IAlertService alertService, AlertsViewModel alertsViewModel, SettingsViewModel settingsViewModel)
        {
            _shellService = shellService;

            AlertsViewModel = alertsViewModel;
            SettingsViewModel = settingsViewModel;
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
            WeakReferenceMessenger.Default.Send(new AlertCollectionAddingMessage());
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
