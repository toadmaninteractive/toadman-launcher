using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Toadman.GameLauncher.Core
{
    public interface ICommand<in T> : ICommand
    {
        bool CanExecute(T parameter);
        void Execute(T parameter);
    }

    public class RelayCommand<T> : ICommand<T>
    {
        readonly Action<T> execute;
        readonly Predicate<T> canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            try
            {
                return canExecute == null || canExecute((T)parameter);
            }
            catch (Exception)
            {
                //TODO logging
                throw;
            }
        }

        public bool CanExecute(T parameter)
        {
            try
            {
                return canExecute == null || canExecute(parameter);
            }
            catch (Exception)
            {
                //TODO logging
                throw;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute((T)parameter);
        }

        public void Execute(T parameter)
        {
            execute(parameter);
        }
    }

    public class RelayCommand : ICommand
    {
        readonly Action execute;
        readonly Func<bool> canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            try
            {
                return canExecute == null || canExecute();
            }
            catch (Exception)
            {
                //TODO logging
                throw;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute();
        }
    }

    public class AsyncCommand<T> : ICommand<T>
    {
        readonly Func<T, Task> execute;
        readonly Predicate<T> canExecute;

        public AsyncCommand(Func<T, Task> execute, Predicate<T> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute((T)parameter);
        }

        public bool CanExecute(T parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            var task = execute((T)parameter);
            task.Track();

            if (task.Exception != null)
                throw task.Exception;
        }

        public void Execute(T parameter)
        {
            var task = execute(parameter);
            task.Track();

            if (task.Exception != null)
                throw task.Exception;
        }
    }

    public class AsyncCommand : ICommand
    {
        readonly Func<Task> execute;
        readonly Func<bool> canExecute;

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            var task = execute();
            task.Track();

            if (task.Exception != null)
                throw task.Exception;
        }
    }
}