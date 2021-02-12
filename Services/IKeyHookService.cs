using AOEMatchDataProvider.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AOEMatchDataProvider.Services
{
    public interface IKeyHookService : ICriticalDisposable
    {
        //void Add(object owner, Keys keys, Action action);
        //void Remove(object owner, Keys keys);

        string Add(Keys keys, Action action);
        void Remove(string token);
    }
}
