using ReNamer.Models;
using ReNamer.Services;

namespace ReNamer.Rules;

public class ChineseNumberRule : RuleBase
{
    public bool AllowLooseForms { get; set; } = false;
    public bool SkipExtension { get; set; } = true;

    public override string RuleName => "Chinese Number";

    public override string Description => AllowLooseForms
        ? "Chinese numbers -> Arabic numerals (loose mode)"
        : "Chinese numbers -> Arabic numerals";

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, extension) = SplitFileName(fileName, SkipExtension, file.IsFolder);

        try
        {
            var converted = ChineseNumberConversionService.ConvertChineseNumbersToArabic(baseName, AllowLooseForms);
            return converted + extension;
        }
        catch
        {
            return fileName;
        }
    }
}
