using System;
using System.Windows.Input;

namespace ExamSchedular.Core
{
    // Basit (parametresiz) komut
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged;

        // ViewModel'den çağırın: SaveCommand.RaiseCanExecuteChanged();
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // Parametreli komut
    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null) return true;
            if (!TryGetParam(parameter, out var value)) return false;
            return _canExecute(value);
        }

        public void Execute(object? parameter)
        {
            if (TryGetParam(parameter, out var value))
                _execute(value);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        // null / UnsetValue güvenli dönüşüm
        private static bool TryGetParam(object? parameter, out T? value)
        {
            if (parameter == null || parameter.GetType().FullName == "MS.Internal.NamedObject")
            {
                value = default;
                return true;
            }
            if (parameter is T t)
            {
                value = t;
                return true;
            }
            value = default;
            return false;
        }
    }
}
