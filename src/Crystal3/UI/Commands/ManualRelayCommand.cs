﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystal3.UI.Commands
{
    public class ManualRelayCommand: RelayCommand, ICommand
    {
        private bool canExecute = false;
        public ManualRelayCommand(Action<object> executePredicate): base(executePredicate)
        {

        }

        public override bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public void SetCanExecute(bool value)
        {
            canExecute = value;

            base.RaiseCanExecute();
        }
    }
}
