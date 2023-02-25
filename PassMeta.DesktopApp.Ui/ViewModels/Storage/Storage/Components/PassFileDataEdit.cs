namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components;

using Models;
using ReactiveUI;

public class PassFileDataEdit : ReactiveObject
{
    private readonly ViewElements _viewElements;
        
    private bool _editMode;
    public bool Mode
    {
        get => _editMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _editMode, value);
            this.RaisePropertyChanged(nameof(SectionName));
                
            if (_viewElements.SectionNameEditBox is null) return;
            if (value)
            {
                var section = _sectionBtn!.Section;
                if (section.Items.Count == 0)
                {
                    _viewElements.SectionNameEditBox.SelectionStart = 0;
                    _viewElements.SectionNameEditBox.SelectionEnd = section.Name.Length;
                    _viewElements.SectionNameEditBox.Focus();
                }
                else
                {
                    _viewElements.SectionNameEditBox.SelectionStart = section.Name.Length;
                    _viewElements.SectionNameEditBox.SelectionEnd = section.Name.Length;
                }
            }
            else
            {
                _viewElements.SectionNameEditBox.SelectionStart = 0;
                _viewElements.SectionNameEditBox.SelectionEnd = 0;
            }
        }
    }
        
    private PassFileSectionBtn? _sectionBtn;
    public PassFileSectionBtn? SectionBtn
    {
        get => _sectionBtn;
        set
        {
            _sectionBtn = value;
            Mode = false;
        }
    }
        
    public string? SectionName
    {
        get => _sectionBtn?.Name;
        set
        {
            if (_sectionBtn is not null)
                _sectionBtn.Name = value ?? string.Empty;
        }
    }

    public PassFileDataEdit(ViewElements viewElements)
    {
        _viewElements = viewElements;
    }
}