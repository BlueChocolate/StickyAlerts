using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using StickyAlerts.Models;
using StickyAlerts.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StickyAlerts.ViewModels
{
    public partial class AlertsViewModel : ObservableObject, IRecipient<AlertCollectionAddingMessage>, IRecipient<AlertPropertyChangedMessage>
    {
        private IAlertService _alertService;

        public ObservableCollection<AlertViewModel> Alerts { get; }
        public ObservableCollection<AlertViewModel> ActivedAlerts { get; }
        public ObservableCollection<AlertViewModel> UnactivedAlerts { get; }

        public AlertsViewModel(IAlertService alertService)
        {
            _alertService = alertService;

            Alerts = _alertService.Alerts;
            ActivedAlerts = new(Alerts.Where(a => a.IsActive));
            UnactivedAlerts = new(Alerts.Where(a => !a.IsActive));
            Alerts.CollectionChanged += Alerts_CollectionChanged;

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void Alerts_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (AlertViewModel alert in e.NewItems)
                    {
                        if (alert.IsActive) ActivedAlerts.Add(alert);
                        else UnactivedAlerts.Add(alert);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (AlertViewModel alert in e.OldItems)
                    {
                        if (alert.IsActive) ActivedAlerts.Remove(alert);
                        else UnactivedAlerts.Remove(alert);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    AlertService.SortByDeadlineAndActiveState(ActivedAlerts);
                    AlertService.SortByDeadlineAndActiveState(UnactivedAlerts);
                    break;

                default:
                    ActivedAlerts.Clear();
                    UnactivedAlerts.Clear();
                    foreach (var alert in Alerts)
                    {
                        if (alert.IsActive) ActivedAlerts.Add(alert);
                        else UnactivedAlerts.Add(alert);
                    }
                    break;
            }
        }

        [RelayCommand]
        public void Add()
        {
            _alertService.Add();
        }

        [RelayCommand]
        public void Delete(object parameter)
        {
            if (parameter is AlertViewModel alert) _alertService.Delete(alert);
            else if (parameter is Guid id) _alertService.Delete(id);
            else throw new ArgumentException("Invalid parameter type.");
        }

        public void Receive(AlertCollectionAddingMessage message)
        {
            _alertService.Add();
        }

        public void Receive(AlertPropertyChangedMessage message)
        {
            if (message.PropertyName == nameof(AlertViewModel.Deadline))
            {
                var alert = Alerts.FirstOrDefault(a => a.Id == message.Id);
                if (alert != null)
                {
                    if (alert.IsActive)
                    {
                        if (!ActivedAlerts.Contains(alert))
                        {
                            ActivedAlerts.Add(alert);
                        }
                        if (UnactivedAlerts.Contains(alert))
                        {
                            UnactivedAlerts.Remove(alert);
                        }
                    }
                    else
                    {
                        if (ActivedAlerts.Contains(alert))
                        {
                            ActivedAlerts.Remove(alert);
                        }
                        if (!UnactivedAlerts.Contains(alert))
                        {
                            UnactivedAlerts.Add(alert);
                        }
                    }
                }
            }
        }
    }
}
