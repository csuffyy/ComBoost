﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System.ComponentModel
{
    public class WpfViewButton : IEntityViewButton
    {
        public ICommand Command { get; set; }
    }
}
