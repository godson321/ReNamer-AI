using System;
using ReNamer.Models;
using ReNamer.Services;

namespace ReNamer.Rules;

public enum ChineseConvertDirection
{
    TraditionalToSimplified,
    SimplifiedToTraditional
}

public class ChineseConvertRule : RuleBase
{
    public ChineseConvertDirection Direction { get; set; } = ChineseConvertDirection.TraditionalToSimplified;
    public bool SkipExtension { get; set; } = true;

    public override string RuleName => "Chinese Convert";

    public override string Description => Direction switch
    {
        ChineseConvertDirection.SimplifiedToTraditional => "Chinese convert: Simplified to Traditional",
        _ => "Chinese convert: Traditional to Simplified"
    };

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, extension) = SplitFileName(fileName, SkipExtension, file.IsFolder);

        try
        {
            var converted = Direction switch
            {
                ChineseConvertDirection.SimplifiedToTraditional =>
                    ChineseScriptConversionService.ConvertSimplifiedToTraditional(baseName),
                _ =>
                    ChineseScriptConversionService.ConvertTraditionalToSimplified(baseName)
            };

            return converted + extension;
        }
        catch
        {
            return fileName;
        }
    }
}
