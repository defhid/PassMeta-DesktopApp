using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Models;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class FileDialogService : IFileDialogService
{
    /// <inheritdoc />
    public async Task<IResult<string>> AskForReadingAsync(IEnumerable<(string Name, List<string> Extensions)>? filters)
    {
        var fileDialog = new OpenFileDialog
        {
            AllowMultiple = false,
            Filters = filters?
                .Select(x => new FileDialogFilter 
                {
                    Name = x.Name,
                    Extensions = x.Extensions
                })
                .ToList()
        };

        var result = await fileDialog.ShowAsync(App.App.MainWindow!);
        var resultFirst = result?.FirstOrDefault();

        return Result.From(resultFirst is not null, resultFirst!);
    }
}