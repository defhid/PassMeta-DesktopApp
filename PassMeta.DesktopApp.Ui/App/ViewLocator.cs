using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;

namespace PassMeta.DesktopApp.Ui.App;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "Not Found: null view" };
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        return type is not null
            ? (Control)Activator.CreateInstance(type)!
            : new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is PageViewModel;
}