using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var outputPath = Path.Combine(repoRoot, "ReNamerWPF", "ReNamer", "Resources", "AppIcon.ico");
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

var sizes = new[] { 16, 24, 32, 48, 64, 128, 256 };
var pngImages = sizes.Select(CreatePngIcon).ToList();
WriteIco(outputPath, sizes, pngImages);

Console.WriteLine($"ICON_OK: {outputPath}");

static byte[] CreatePngIcon(int size)
{
    using var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    g.Clear(Color.Transparent);

    var scale = size / 16f;

    // Orange rounded square background (fills most of the icon area to avoid tiny taskbar appearance).
    var bgRect = new RectangleF(0.5f * scale, 0.5f * scale, 15f * scale, 15f * scale);
    using (var bgPath = RoundedRect(bgRect, 2f * scale))
    using (var bgBrush = new SolidBrush(ColorTranslator.FromHtml("#C2410C")))
    {
        g.FillPath(bgBrush, bgPath);
    }

    using var whiteBrush = new SolidBrush(Color.White);
    using var accentBrush = new SolidBrush(ColorTranslator.FromHtml("#FFEDD5"));

    // Top and bottom white bars.
    g.FillRectangle(whiteBrush, 3f * scale, 4f * scale, 10f * scale, 2f * scale);
    g.FillRectangle(whiteBrush, 3f * scale, 10f * scale, 10f * scale, 2f * scale);

    // Right arrow in the center.
    var arrow = new[]
    {
        new PointF(4f * scale, 8f * scale),
        new PointF(10f * scale, 8f * scale),
        new PointF(10f * scale, 6f * scale),
        new PointF(13f * scale, 8f * scale),
        new PointF(10f * scale, 10f * scale),
        new PointF(10f * scale, 8f * scale)
    };
    g.FillPolygon(accentBrush, arrow);

    using var ms = new MemoryStream();
    bmp.Save(ms, ImageFormat.Png);
    return ms.ToArray();
}

static GraphicsPath RoundedRect(RectangleF rect, float radius)
{
    var diameter = radius * 2f;
    var path = new GraphicsPath();
    path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
    path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
    path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
    path.CloseFigure();
    return path;
}

static void WriteIco(string outputPath, IReadOnlyList<int> sizes, IReadOnlyList<byte[]> pngImages)
{
    using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
    using var bw = new BinaryWriter(fs);

    bw.Write((ushort)0); // reserved
    bw.Write((ushort)1); // icon type
    bw.Write((ushort)sizes.Count);

    var offset = 6 + (16 * sizes.Count);
    for (var i = 0; i < sizes.Count; i++)
    {
        var size = sizes[i];
        var png = pngImages[i];

        bw.Write((byte)(size >= 256 ? 0 : size)); // width
        bw.Write((byte)(size >= 256 ? 0 : size)); // height
        bw.Write((byte)0);                        // color count
        bw.Write((byte)0);                        // reserved
        bw.Write((ushort)1);                      // planes
        bw.Write((ushort)32);                     // bits per pixel
        bw.Write(png.Length);                     // image size
        bw.Write(offset);                         // image offset
        offset += png.Length;
    }

    foreach (var png in pngImages)
        bw.Write(png);
}
