﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GuiaCiudad.W8.viewModels
{
    public class ActionCommand : ICommand
    {
        Action<object> action;

        public ActionCommand(Action<object> action)
        {
            // TODO: Complete member initialization
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
