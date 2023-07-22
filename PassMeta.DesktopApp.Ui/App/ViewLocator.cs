using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;

namespace PassMeta.DesktopApp.Ui.App;

using Avalonia.Controls.Templates;
using Avalonia.Controls;
using System;

public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        return type == null
            ? new TextBlock { Text = "Not Found: " + name }
            : (Control)Activator.CreateInstance(type)!;
    }

    public bool Match(object data) => data is PageViewModel;
}