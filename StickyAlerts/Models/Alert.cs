using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StickyAlerts.Models
{
    public partial class Alert : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string? _title;

        [ObservableProperty]
        private string? _note;

        [ObservableProperty]
        private DateTime _deadline;

        [ObservableProperty]
        private DateTime _lastModified;

        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private bool _showNote;

        [ObservableProperty]
        private double _width;

        [ObservableProperty]
        private double _height;

        [ObservableProperty]
        private double _left;

        [ObservableProperty]
        private double _top;

        [ObservableProperty]
        private bool _topmost;

        [JsonIgnore]
        public TimeSpan Remaining => IsActive ? Deadline - DateTime.Now : TimeSpan.Zero;

        [JsonIgnore]
        public bool IsActive => Deadline >= DateTime.Now;
    }
}
