using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaTests.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AvaloniaTests
{
    //Реализует IDataTemplate для поиска и создания соответствующего View по ViewModel
    //Использует соглашение об именовании: заменяет "ViewModel" на "View" в полном имени типа
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)ActivatorUtilities.CreateInstance(ServiceProvider.Instance, type);
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        // Определяет, подходит ли данный шаблон для переданных данных
        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}