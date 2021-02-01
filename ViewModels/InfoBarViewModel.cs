using AOEMatchDataProvider.Events.Views.InfoBar;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.ViewModels
{
    public class InfoBarViewModel : BindableBase
    {
        string message;
        public string Message
        {
            get
            {
                return message;
            }

            set
            {
                SetProperty(ref message, value);
            }
        }

        public InfoBarViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<InfoBarMessageChanged>().Subscribe(HandleMessageChanged);
        }

        void HandleMessageChanged(string message)
        {
            this.Message = message; //update message
        }
    }
}
