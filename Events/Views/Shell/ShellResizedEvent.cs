﻿using AOEMatchDataProvider.Models.Views.Shell;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Events.Views.Shell
{
    public class ShellResizedEvent : PubSubEvent<Rectangle> { }
}
