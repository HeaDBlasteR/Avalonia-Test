using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System.ComponentModel;

namespace AvaloniaTests.ViewModels
{
    public class ViewModelBase : ObservableObject, IReactiveObject
    {
        void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args)
        {
            OnPropertyChanging(args);
        }

        void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            OnPropertyChanged(args);
        }
    }
}