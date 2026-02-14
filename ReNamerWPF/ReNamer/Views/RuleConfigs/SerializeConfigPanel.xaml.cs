using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ReNamer.Rules;
using ReNamer.Services;

namespace ReNamer.Views.RuleConfigs;

public partial class SerializeConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly SerializeRule _rule;

    private sealed class NumberingSystemItem
    {
        public string Value { get; init; } = string.Empty;
        public string Display { get; init; } = string.Empty;
    }

    private static readonly (string Value, string Key)[] NumberingSystemOptions =
    {
        ("Decimal", "Serialize_Numbering_Decimal"),
        ("Hexadecimal", "Serialize_Numbering_Hexadecimal"),
        ("Octal", "Serialize_Numbering_Octal"),
        ("Binary", "Serialize_Numbering_Binary"),
        ("Custom", "Serialize_Numbering_Custom"),
    };

    public SerializeConfigPanel(SerializeRule rule)
    {
        InitializeComponent();
        _rule = rule;

        cmbNumberingSystem.DisplayMemberPath = nameof(NumberingSystemItem.Display);
        cmbNumberingSystem.SelectedValuePath = nameof(NumberingSystemItem.Value);
        RefreshNumberingSystems();
        LanguageService.LanguageChanged += RefreshNumberingSystems;
        LoadConfig();
    }

    private void RefreshNumberingSystems()
    {
        var selected = cmbNumberingSystem.SelectedValue as string ?? _rule.NumberingSystem ?? "Decimal";
        var items = new List<NumberingSystemItem>();
        foreach (var (value, key) in NumberingSystemOptions)
        {
            items.Add(new NumberingSystemItem
            {
                Value = value,
                Display = LanguageService.GetString(key)
            });
        }
        cmbNumberingSystem.ItemsSource = items;
        cmbNumberingSystem.SelectedValue = selected;
    }

    private void LoadConfig()
    {
        nudIndex.Value = _rule.StartNumber;
        nudRepeat.Value = _rule.Repeat;
        nudStep.Value = _rule.Step;
        chkResetEvery.IsChecked = _rule.ResetEvery;
        nudResetEvery.Value = _rule.ResetEveryCount;
        chkResetIfFolder.IsChecked = _rule.ResetIfFolderChanges;
        chkResetIfFileName.IsChecked = _rule.ResetIfFileNameChanges;
        chkPadToLength.IsChecked = _rule.PadToLength;
        nudPadLength.Value = _rule.PadToLengthValue;
        nudPositionValue.Value = _rule.PositionValue;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        txtCustomSymbols.Text = _rule.CustomNumberingSymbols;

        // Select numbering system
        cmbNumberingSystem.SelectedValue = _rule.NumberingSystem ?? "Decimal";

        switch (_rule.InsertWhere)
        {
            case SerializePosition.Prefix: rbPrefix.IsChecked = true; break;
            case SerializePosition.Suffix: rbSuffix.IsChecked = true; break;
            case SerializePosition.Position: rbPosition.IsChecked = true; break;
            case SerializePosition.ReplaceCurrentName: rbReplaceName.IsChecked = true; break;
        }
    }

    public void ApplyConfig()
    {
        _rule.StartNumber = nudIndex.Value;
        _rule.Repeat = nudRepeat.Value;
        _rule.Step = nudStep.Value;
        _rule.ResetEvery = chkResetEvery.IsChecked == true;
        _rule.ResetEveryCount = nudResetEvery.Value;
        _rule.ResetIfFolderChanges = chkResetIfFolder.IsChecked == true;
        _rule.ResetIfFileNameChanges = chkResetIfFileName.IsChecked == true;
        _rule.PadToLength = chkPadToLength.IsChecked == true;
        _rule.PadToLengthValue = nudPadLength.Value;
        _rule.PositionValue = nudPositionValue.Value;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.CustomNumberingSymbols = txtCustomSymbols.Text;

        if (rbPrefix.IsChecked == true) _rule.InsertWhere = SerializePosition.Prefix;
        else if (rbSuffix.IsChecked == true) _rule.InsertWhere = SerializePosition.Suffix;
        else if (rbPosition.IsChecked == true) _rule.InsertWhere = SerializePosition.Position;
        else if (rbReplaceName.IsChecked == true) _rule.InsertWhere = SerializePosition.ReplaceCurrentName;

        _rule.NumberingSystem = cmbNumberingSystem.SelectedValue as string ?? "Decimal";

        _rule.Reset();
    }
}
