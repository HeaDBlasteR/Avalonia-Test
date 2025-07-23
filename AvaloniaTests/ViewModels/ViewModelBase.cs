using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System.ComponentModel;

namespace AvaloniaTests.ViewModels
{
    // Базовый класс для всех ViewModel, обеспечивает поддержку уведомлений об изменении свойств
    public class ViewModelBase : ObservableObject, IReactiveObject
    {
        // Вызыв событие перед изменением свойства
        void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args)
        {
            OnPropertyChanging(args);
        }

        //После
        void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            OnPropertyChanged(args);
        }
    }
}