using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaTests.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AvaloniaTests
{
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

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}