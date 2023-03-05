using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;

namespace PassMeta.DesktopApp.Ui.App;

using Avalonia.Controls.Templates;
using Avalonia.Controls;
using System;

public class ViewLocator : IDataTemplate
{
    public bool SupportsRecycling => false;

    public IControl Build(object data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        else
        {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object data)
    {
        return data is PageViewModel;
    }
}