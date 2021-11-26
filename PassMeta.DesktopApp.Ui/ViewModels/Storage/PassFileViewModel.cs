namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Avalonia.Media;
    using Common;
    using Common.Models.Entities;
    using DynamicData;
    using ReactiveUI;
    
    public partial class PassFileViewModel : ReactiveObject
    {
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        
        public int SelectedColorIndex { get; set; }

        public static Tuple<string, ISolidColorBrush?>[] Colors => new[]
        {
            new Tuple<string, ISolidColorBrush?>("-", null),
            new Tuple<string, ISolidColorBrush?>(Resources.PASSFILE_COLOR__RED, Brushes.Red),
            new Tuple<string, ISolidColorBrush?>(Resources.PASSFILE_COLOR__BLUE, Brushes.Blue),
            new Tuple<string, ISolidColorBrush?>(Resources.PASSFILE_COLOR__GREY, Brushes.Gray),
            new Tuple<string, ISolidColorBrush?>(Resources.PASSFILE_COLOR__PINK, Brushes.Pink),
            new Tuple<string, ISolidColorBrush?>(Resources.PASSFILE_COLOR__GREEN, Brushes.Green),
            new Tuple<string, ISolidColorBrush?>(Resources.PASSFILE_COLOR__ORANGE, Brushes.Orange),
            new Tuple<string, ISolidColorBrush?>(Resources.PASSFILE_COLOR__PURPLE, Brushes.Purple),
            new Tuple<string, ISolidColorBrush?>(Resources.PASSFILE_COLOR__YELLOW, Brushes.Yellow),
        }.OrderBy(c => c.Item1).ToArray();

        private string _createdOn;
        public string CreatedOn
        {
            get => _createdOn;
            set => this.RaiseAndSetIfChanged(ref _createdOn, value);
        }
        
        private string _changedOn;
        public string ChangedOn
        {
            get => _changedOn;
            set => this.RaiseAndSetIfChanged(ref _changedOn, value);
        }

        private string? _localChangedOn;
        public string? LocalChangedOn
        {
            get => _localChangedOn;
            set => this.RaiseAndSetIfChanged(ref _localChangedOn, value);
        }

        public PassFileViewModel()
        {
            
        }

        public PassFileViewModel(PassFile passFile)
        {
            Title = string.Format(Resources.STORAGE_PASSFILE__TITLE, passFile.Name);

            Name = passFile.Name;

            SelectedColorIndex = passFile.Color is null ? 0
                : Colors.IndexOf(Colors.FirstOrDefault(c => c.Item2?.Color == Color.Parse(passFile.Color)));

            CreatedOn = passFile.CreatedOn.ToString(CultureInfo.CurrentCulture);
            ChangedOn = passFile.IsArchived.ToString(CultureInfo.CurrentCulture);
            LocalChangedOn = passFile.ChangedLocalOn?.ToString(CultureInfo.CurrentCulture);
        }
    }
}