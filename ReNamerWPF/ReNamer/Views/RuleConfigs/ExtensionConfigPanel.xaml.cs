using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class ExtensionConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly ExtensionRule _rule;

    public ExtensionConfigPanel(ExtensionRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        chkNewExtension.IsChecked = _rule.NewExtensionEnabled;
        txtExtension.Text = _rule.NewExtension;
        chkAppendToOriginal.IsChecked = _rule.AppendToOriginal;
        chkDetectBinSign.IsChecked = _rule.DetectBinarySignature;
        chkRemoveDuplicate.IsChecked = _rule.RemoveDuplicateExtensions;
        chkCaseSensitive.IsChecked = _rule.CaseSensitive;
    }

    public void ApplyConfig()
    {
        _rule.NewExtensionEnabled = chkNewExtension.IsChecked == true;
        _rule.NewExtension = txtExtension.Text;
        _rule.AppendToOriginal = chkAppendToOriginal.IsChecked == true;
        _rule.DetectBinarySignature = chkDetectBinSign.IsChecked == true;
        _rule.RemoveDuplicateExtensions = chkRemoveDuplicate.IsChecked == true;
        _rule.CaseSensitive = chkCaseSensitive.IsChecked == true;
    }
}
