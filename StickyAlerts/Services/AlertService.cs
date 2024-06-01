using HandyControl.Tools.Extension;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyAlerts.Models;
using StickyAlerts.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace StickyAlerts.Services
{
    public interface IAlertService
    {
        public ObservableCollection<Alert> Alerts { get; }
        public void Align();
        public void Load();
        public void Save();
        public void Add();
        public void Add(string title, string note, DateTime deadline, bool isVisible = true, bool showNote = false);
    }

    public class AlertService : IAlertService
    {
        private ConcurrentDictionary<Guid, AlertWindow> _alertWindows;
        private ISettingsService<UserSettings> _userSettings;
        private ILogger<AlertService> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ObservableCollection<Alert> Alerts { get; private set; }

        public AlertService(ISettingsService<UserSettings> userSettings, ILogger<AlertService> logger)
        {
            _alertWindows = new ConcurrentDictionary<Guid, AlertWindow>();
            _userSettings = userSettings;
            _logger = logger;
            _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, };
            Load();
        }

        public void Load()
        {
            // 读取本地文件
            var directoryPath = _userSettings.Current.AlertsPath;
            try
            {
                var filePath = Path.Combine(directoryPath, "Alerts.json");
                if (Directory.Exists(_userSettings.Current.AlertsPath))
                {
                    // 如果指定路径存在，则读取文件

                    var json = File.ReadAllText(filePath);
                    var alerts = JsonSerializer.Deserialize<ObservableCollection<Alert>>(json);
                    if (alerts != null)
                    {
                        Alerts = alerts;
                    }
                    else
                    {
                        Alerts = [];
                        File.WriteAllText(filePath, JsonSerializer.Serialize(Alerts, _jsonSerializerOptions));
                    }
                }
                else
                {
                    // 如果指定路径不存在，则创建目录与文件
                    _logger.LogWarning("Failed to load alerts from {filePath}, directory not exists, try create a empty file", filePath);
                    Directory.CreateDirectory(directoryPath);
                    Alerts = [];
                    File.WriteAllText(filePath, JsonSerializer.Serialize(Alerts, _jsonSerializerOptions));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load alerts from {directoryPath}: {exception}, please check your setting", directoryPath, e.Message);
            }

            // 创建便笺窗体
            foreach (var alert in Alerts)
            {
                AddAlertWindow(alert);
            }
            Align();
            Save();
        }

        private void AddAlertWindow(Alert alert)
        {
            var alertWindow = new AlertWindow { DataContext = alert };
            alertWindow.UpdateLayout();
            // 愚蠢的方法，但确实有用
            alertWindow.Show();
            //alertWindow.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            alert.Width = alertWindow.ActualWidth;
            alert.Height = alertWindow.ActualHeight;
            Save();
            if (!alert.IsVisible) alertWindow.Hide();
            _alertWindows.TryAdd(alert.Id, alertWindow);
        }

        public void Add()
        {
            Add("新的便笺", string.Empty, DateTime.Today.AddDays(1).AddHours(9), true, false);
        }

        public void Add(string title, string note, DateTime deadline, bool isVisible = true, bool showNote = false)
        {
            var alert = new Alert()
            {
                Id = Guid.NewGuid(),
                Title = title,
                Note = note,
                Deadline = deadline,
                LastModified = DateTime.Now,
                IsVisible = isVisible,
                ShowNote = showNote,
            };
            Alerts.Add(alert);
            Sort();
            AddAlertWindow(alert);
            Align();
            Save();
        }

        public void Save()
        {
            var directoryPath = _userSettings.Current.AlertsPath;
            try
            {
                var filePath = Path.Combine(directoryPath, "Alerts.json");
                if (Directory.Exists(directoryPath))
                {
                    File.WriteAllText(filePath, JsonSerializer.Serialize(Alerts, _jsonSerializerOptions));
                }
                else
                {
                    _logger.LogWarning("Failed to save alerts to {filePath}, directory not exists, try create", filePath);
                    Directory.CreateDirectory(directoryPath);
                    File.WriteAllText(filePath, JsonSerializer.Serialize(Alerts, _jsonSerializerOptions));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save alerts to {directoryPath}: {exception}, please check your setting", directoryPath, e.Message);
            }
        }

        public void Align()
        {
            var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            var horizontalSpacing = _userSettings.Current.HorizontalSpacing;
            var verticalSpacing = _userSettings.Current.VerticalSpacing;
            int columns = 1;
            var nextHeight = verticalSpacing;

            Sort();
            foreach (var alert in Alerts)
            {
                if (nextHeight + (int)alert.Height > screenHeight)
                {
                    columns++;
                    nextHeight = verticalSpacing;
                }
                _alertWindows[alert.Id].Left = screenWidth - columns * (horizontalSpacing + (int)alert.Width);
                _alertWindows[alert.Id].Top = nextHeight;
                nextHeight += verticalSpacing + (int)alert.Height;
            }
        }

        private void Sort()
        {
            List<Alert> sortedList = [.. Alerts.OrderBy(a => !a.IsVisible).ThenBy(a => a.Deadline)];
            for (int i = 0; i < sortedList.Count; i++)
            {
                Alerts.Move(Alerts.IndexOf(sortedList[i]), i);
            }
        }
    }
}
