//v1
using System;
using System.Windows.Input;

namespace Mi.Common
{

    /// <summary>
    /// Command class useful in implementing MVVM in wpf
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MICommand<T> : ICommand
    {
        public Action<T> CommandAction { get; set; }
        public Func<T, bool> CanExecuteFunc { get; set; }

        public MICommand(Action<T> _CommandAction)
        {
            CommandAction = _CommandAction;
            CanExecuteFunc = null;

        }

        public MICommand(Action<T> _CommandAction, Func<T, bool> _CanExecuteFunc)
        {
            CommandAction = _CommandAction;
            CanExecuteFunc = _CanExecuteFunc;
        }

        public void Execute(object parameter)
        {
            CommandAction((T)parameter);
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }


    public class MICommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public MICommand(Action _CommandAction)
        {
            CommandAction = _CommandAction;
            CanExecuteFunc = null;

        }

        public MICommand(Action _CommandAction, Func<bool> _CanExecuteFunc)
        {
            CommandAction = _CommandAction;
            CanExecuteFunc = _CanExecuteFunc;
        }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        internal void RaiseCanExecuteChanged()
        {
            
        }
    }

}