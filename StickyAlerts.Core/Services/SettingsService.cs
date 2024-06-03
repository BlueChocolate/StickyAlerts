using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StickyAlerts.Core.Services
{
    public interface ISettingsService
    {
        public interface ISettingsService<TModel> where TModel : class
        {
            public TModel Current { get; }
            public void Save();
            public void Apply(string name);
            public void ApplyAll();
            public void OnFileChanged(Action<object?, object?, string> callback);
        }
    }
}
