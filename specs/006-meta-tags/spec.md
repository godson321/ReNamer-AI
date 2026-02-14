# Feature 006: Meta Tags Processor

## 概述

**Feature Name**: Meta Tags处理器  
**Priority**: P1 (High - 规则系统的核心增强功能)  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: Feature 001（规则引擎核心）

WPF 对齐说明：解析入口见 `ReNamerWPF/ReNamer/Rules/RuleHelpers.cs`（`ResolveMetaTags`），各规则调用处以当前实现为准。

## 问题陈述

Meta Tags 是ReNamer中的高级功能,允许在规则参数中使用占位符标签,运行时动态替换为文件的实际属性。支持:
1. **文件属性标签** - `:File_Name:`, `:File_Path:`, `:File_Size:`, `:File_Created:`, `:File_Modified:`
2. **EXIF元数据标签** - `:Width:`, `:Height:`, `:ExifDate:`, `:Camera_Model:`
3. **随机数标签** - `:Random_Number:`, `:Random_Alpha:`, `:Random_Hex:`
4. **日期时间标签** - `:Current_Date:`, `:Current_Time:`, `:Year:`, `:Month:`, `:Day:`
5. **计数器标签** - `:Counter:`, `:Unique_ID:`

核心难点: **EXIF元数据读取** - 需要解析JPEG/TIFF/PNG/MP4等文件的元数据。

## 需求（基于 DFM 与 Review Checklist）

以下为 DFM/Checklist 的历史参考；WPF 已实现 Meta Tags 解析与集成，功能以当前实现为准。

### Meta Tags 应用场景

根据 `review_checklist.md` 第86-94行,以下规则需要支持Meta Tags:

```pascal
// Replace规则 - SpeedButton_ReplaceWithMetaTag
Replace: FindText="photo", ReplaceWith=":File_Name:_backup"
// 结果: "photo123.jpg" → "photo123_backup.jpg"

// Insert规则 - BitBtn_InsertMetaTag
Insert: Text=":Width:x:Height:", Position=Prefix
// 结果: "image.jpg" → "1920x1080_image.jpg"

// Rearrange规则 - SB_InsertMetaTag
Rearrange: Pattern=":File_Name: (:File_Size:)"
// 结果: "document.pdf" → "document (1024 KB).pdf"
```

### Meta Tags 完整列表

```pascal
const
  /// Meta Tags 分类定义
  META_TAG_CATEGORIES: array[0..4] of string = (
    'File Properties',
    'EXIF Metadata',
    'Random Values',
    'Date & Time',
    'Counters'
  );

  /// 所有支持的 Meta Tags
  META_TAGS: array[0..30] of TMetaTagDefinition = (
    // File Properties (文件属性)
    (Tag: ':File_Name:';       Category: 0; Description: 'Original file name without extension'),
    (Tag: ':File_NameExt:';    Category: 0; Description: 'Original file name with extension'),
    (Tag: ':File_Path:';       Category: 0; Description: 'Full file path'),
    (Tag: ':File_Folder:';     Category: 0; Description: 'Folder name'),
    (Tag: ':File_Extension:';  Category: 0; Description: 'File extension with dot'),
    (Tag: ':File_Size:';       Category: 0; Description: 'File size in bytes'),
    (Tag: ':File_SizeKB:';     Category: 0; Description: 'File size in kilobytes'),
    (Tag: ':File_SizeMB:';     Category: 0; Description: 'File size in megabytes'),
    (Tag: ':File_Created:';    Category: 0; Description: 'File creation date and time'),
    (Tag: ':File_Modified:';   Category: 0; Description: 'File modification date and time'),
    (Tag: ':File_Accessed:';   Category: 0; Description: 'File last access date and time'),
    
    // EXIF Metadata (图像/视频元数据)
    (Tag: ':Width:';           Category: 1; Description: 'Image/video width in pixels'),
    (Tag: ':Height:';          Category: 1; Description: 'Image/video height in pixels'),
    (Tag: ':ExifDate:';        Category: 1; Description: 'EXIF date taken'),
    (Tag: ':Camera_Make:';     Category: 1; Description: 'Camera manufacturer'),
    (Tag: ':Camera_Model:';    Category: 1; Description: 'Camera model'),
    (Tag: ':ISO:';             Category: 1; Description: 'ISO speed'),
    (Tag: ':FNumber:';         Category: 1; Description: 'F-number/aperture'),
    (Tag: ':ExposureTime:';    Category: 1; Description: 'Exposure time (shutter speed)'),
    (Tag: ':FocalLength:';     Category: 1; Description: 'Focal length in mm'),
    (Tag: ':GPS_Latitude:';    Category: 1; Description: 'GPS latitude'),
    (Tag: ':GPS_Longitude:';   Category: 1; Description: 'GPS longitude'),
    
    // Random Values (随机值)
    (Tag: ':Random_Number:';   Category: 2; Description: 'Random number (0-99999)'),
    (Tag: ':Random_Alpha:';    Category: 2; Description: 'Random alphabetic string (5 chars)'),
    (Tag: ':Random_AlphaNum:'; Category: 2; Description: 'Random alphanumeric string'),
    (Tag: ':Random_Hex:';      Category: 2; Description: 'Random hexadecimal string'),
    (Tag: ':Random_GUID:';     Category: 2; Description: 'Random GUID/UUID'),
    
    // Date & Time (日期时间)
    (Tag: ':Current_Date:';    Category: 3; Description: 'Current date (YYYY-MM-DD)'),
    (Tag: ':Current_Time:';    Category: 3; Description: 'Current time (HH:MM:SS)'),
    (Tag: ':Year:';            Category: 3; Description: 'Current year (YYYY)'),
    (Tag: ':Month:';           Category: 3; Description: 'Current month (MM)'),
    (Tag: ':Day:';             Category: 3; Description: 'Current day (DD)'),
    
    // Counters (计数器)
    (Tag: ':Counter:';         Category: 4; Description: 'Sequential counter (auto-increment)'),
    (Tag: ':Unique_ID:';       Category: 4; Description: 'Unique identifier based on file hash')
  );
```

## Meta Tag Processor Implementation

### 核心处理器类

```pascal
type
  /// Meta Tag 定义
  TMetaTagDefinition = record
    Tag: string;           // 标签名称 (如 ":File_Name:")
    Category: Integer;     // 分类索引
    Description: string;   // 描述
  end;

  /// Meta Tag 处理器
  TMetaTagProcessor = class
  private
    FCounter: Integer;               // Counter标签计数器
    FExifCache: TDictionary<string, TExifData>; // EXIF缓存
    
    function GetFileProperty(const ATag: string; const AFile: TRenFile): string;
    function GetExifProperty(const ATag: string; const AFile: TRenFile): string;
    function GetRandomValue(const ATag: string): string;
    function GetDateTimeValue(const ATag: string): string;
    function GetCounterValue(const ATag: string): string;
    
    function ReadExifData(const AFilePath: string): TExifData;
  public
    constructor Create;
    destructor Destroy; override;
    
    /// 处理字符串中的所有Meta Tags
    function ProcessTags(const AText: string; const AFile: TRenFile): string;
    
    /// 检查字符串是否包含Meta Tags
    function ContainsTags(const AText: string): Boolean;
    
    /// 获取所有可用的Meta Tags
    class function GetAvailableTags: TArray<TMetaTagDefinition>;
    
    /// 重置计数器
    procedure ResetCounters;
  end;
```

### ProcessTags实现

```pascal
function TMetaTagProcessor.ProcessTags(const AText: string; 
  const AFile: TRenFile): string;
var
  I: Integer;
  Tag: string;
  Value: string;
begin
  Result := AText;
  
  // 遍历所有Meta Tags
  for I := Low(META_TAGS) to High(META_TAGS) do
  begin
    Tag := META_TAGS[I].Tag;
    
    // 检查标签是否存在
    if Pos(Tag, Result) = 0 then Continue;
    
    // 获取标签值
    case META_TAGS[I].Category of
      0: Value := GetFileProperty(Tag, AFile);   // File Properties
      1: Value := GetExifProperty(Tag, AFile);   // EXIF Metadata
      2: Value := GetRandomValue(Tag);           // Random Values
      3: Value := GetDateTimeValue(Tag);         // Date & Time
      4: Value := GetCounterValue(Tag);          // Counters
    else
      Value := '';
    end;
    
    // 替换标签为值
    Result := StringReplace(Result, Tag, Value, [rfReplaceAll, rfIgnoreCase]);
  end;
end;
```

### GetFileProperty实现

```pascal
function TMetaTagProcessor.GetFileProperty(const ATag: string; 
  const AFile: TRenFile): string;
var
  FileInfo: TSearchRec;
begin
  if ATag = ':File_Name:' then
    Result := ChangeFileExt(ExtractFileName(AFile.FullPath), '')
    
  else if ATag = ':File_NameExt:' then
    Result := ExtractFileName(AFile.FullPath)
    
  else if ATag = ':File_Path:' then
    Result := AFile.FullPath
    
  else if ATag = ':File_Folder:' then
    Result := ExtractFileName(ExcludeTrailingPathDelimiter(ExtractFilePath(AFile.FullPath)))
    
  else if ATag = ':File_Extension:' then
    Result := ExtractFileExt(AFile.FullPath)
    
  else if ATag = ':File_Size:' then
  begin
    if FindFirst(AFile.FullPath, faAnyFile, FileInfo) = 0 then
    begin
      Result := IntToStr(FileInfo.Size);
      FindClose(FileInfo);
    end
    else
      Result := '0';
  end
  
  else if ATag = ':File_SizeKB:' then
  begin
    if FindFirst(AFile.FullPath, faAnyFile, FileInfo) = 0 then
    begin
      Result := Format('%.2f', [FileInfo.Size / 1024]);
      FindClose(FileInfo);
    end
    else
      Result := '0';
  end
  
  else if ATag = ':File_SizeMB:' then
  begin
    if FindFirst(AFile.FullPath, faAnyFile, FileInfo) = 0 then
    begin
      Result := Format('%.2f', [FileInfo.Size / (1024*1024)]);
      FindClose(FileInfo);
    end
    else
      Result := '0';
  end
  
  else if ATag = ':File_Created:' then
  begin
    if FindFirst(AFile.FullPath, faAnyFile, FileInfo) = 0 then
    begin
      Result := DateTimeToStr(FileInfo.TimeStamp);
      FindClose(FileInfo);
    end
    else
      Result := '';
  end
  
  else if ATag = ':File_Modified:' then
  begin
    if FindFirst(AFile.FullPath, faAnyFile, FileInfo) = 0 then
    begin
      Result := DateTimeToStr(FileInfo.TimeStamp);
      FindClose(FileInfo);
    end
    else
      Result := '';
  end
  
  else
    Result := '';
end;
```

### GetExifProperty实现

```pascal
type
  /// EXIF 数据结构
  TExifData = record
    Width: Integer;
    Height: Integer;
    ExifDate: TDateTime;
    CameraMake: string;
    CameraModel: string;
    ISO: Integer;
    FNumber: Double;
    ExposureTime: string;
    FocalLength: Double;
    GPSLatitude: string;
    GPSLongitude: string;
    IsValid: Boolean;
  end;

function TMetaTagProcessor.GetExifProperty(const ATag: string; 
  const AFile: TRenFile): string;
var
  ExifData: TExifData;
begin
  // 检查缓存
  if not FExifCache.TryGetValue(AFile.FullPath, ExifData) then
  begin
    // 读取EXIF数据
    ExifData := ReadExifData(AFile.FullPath);
    
    // 缓存
    FExifCache.Add(AFile.FullPath, ExifData);
  end;
  
  // 提取值
  if not ExifData.IsValid then
  begin
    Result := '';
    Exit;
  end;
  
  if ATag = ':Width:' then
    Result := IntToStr(ExifData.Width)
  else if ATag = ':Height:' then
    Result := IntToStr(ExifData.Height)
  else if ATag = ':ExifDate:' then
    Result := DateTimeToStr(ExifData.ExifDate)
  else if ATag = ':Camera_Make:' then
    Result := ExifData.CameraMake
  else if ATag = ':Camera_Model:' then
    Result := ExifData.CameraModel
  else if ATag = ':ISO:' then
    Result := IntToStr(ExifData.ISO)
  else if ATag = ':FNumber:' then
    Result := Format('f/%.1f', [ExifData.FNumber])
  else if ATag = ':ExposureTime:' then
    Result := ExifData.ExposureTime
  else if ATag = ':FocalLength:' then
    Result := Format('%.1fmm', [ExifData.FocalLength])
  else if ATag = ':GPS_Latitude:' then
    Result := ExifData.GPSLatitude
  else if ATag = ':GPS_Longitude:' then
    Result := ExifData.GPSLongitude
  else
    Result := '';
end;
```

### ReadExifData实现 (JPEG示例)

```pascal
function TMetaTagProcessor.ReadExifData(const AFilePath: string): TExifData;
var
  FileStream: TFileStream;
  ExifReader: TExifReader;  // 第三方库或自实现
begin
  Result.IsValid := False;
  
  // 仅支持图像文件
  if not IsImageFile(AFilePath) then Exit;
  
  try
    FileStream := TFileStream.Create(AFilePath, fmOpenRead or fmShareDenyNone);
    try
      ExifReader := TExifReader.Create(FileStream);
      try
        // 读取基本信息
        Result.Width := ExifReader.ImageWidth;
        Result.Height := ExifReader.ImageHeight;
        
        // 读取相机信息
        Result.CameraMake := ExifReader.GetTagValue('Make');
        Result.CameraModel := ExifReader.GetTagValue('Model');
        
        // 读取拍摄参数
        Result.ISO := StrToIntDef(ExifReader.GetTagValue('ISOSpeedRatings'), 0);
        Result.FNumber := StrToFloatDef(ExifReader.GetTagValue('FNumber'), 0);
        Result.ExposureTime := ExifReader.GetTagValue('ExposureTime');
        Result.FocalLength := StrToFloatDef(ExifReader.GetTagValue('FocalLength'), 0);
        
        // 读取日期
        Result.ExifDate := ExifReader.GetDateTimeTaken;
        
        // 读取GPS
        Result.GPSLatitude := ExifReader.GetGPSLatitude;
        Result.GPSLongitude := ExifReader.GetGPSLongitude;
        
        Result.IsValid := True;
      finally
        ExifReader.Free;
      end;
    finally
      FileStream.Free;
    end;
  except
    // EXIF读取失败,返回无效数据
    Result.IsValid := False;
  end;
end;
```

**EXIF库选择**:
- **dexif** - Free Pascal原生EXIF库
- **ExifTool.pas** - ExifTool的Pascal包装器
- **TExifReader** - 自实现简化版EXIF读取器

### GetRandomValue实现

```pascal
function TMetaTagProcessor.GetRandomValue(const ATag: string): string;
const
  ALPHA_CHARS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz';
  ALPHANUM_CHARS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  HEX_CHARS = '0123456789ABCDEF';
var
  I: Integer;
  GUID: TGUID;
begin
  if ATag = ':Random_Number:' then
    Result := IntToStr(Random(100000))
    
  else if ATag = ':Random_Alpha:' then
  begin
    Result := '';
    for I := 1 to 5 do
      Result := Result + ALPHA_CHARS[Random(Length(ALPHA_CHARS)) + 1];
  end
  
  else if ATag = ':Random_AlphaNum:' then
  begin
    Result := '';
    for I := 1 to 8 do
      Result := Result + ALPHANUM_CHARS[Random(Length(ALPHANUM_CHARS)) + 1];
  end
  
  else if ATag = ':Random_Hex:' then
  begin
    Result := '';
    for I := 1 to 8 do
      Result := Result + HEX_CHARS[Random(Length(HEX_CHARS)) + 1];
  end
  
  else if ATag = ':Random_GUID:' then
  begin
    CreateGUID(GUID);
    Result := GUIDToString(GUID);
    Result := Copy(Result, 2, Length(Result) - 2); // 移除{}
  end
  
  else
    Result := '';
end;
```

### GetDateTimeValue实现

```pascal
function TMetaTagProcessor.GetDateTimeValue(const ATag: string): string;
var
  Now: TDateTime;
begin
  Now := SysUtils.Now;
  
  if ATag = ':Current_Date:' then
    Result := FormatDateTime('yyyy-mm-dd', Now)
    
  else if ATag = ':Current_Time:' then
    Result := FormatDateTime('hh:nn:ss', Now)
    
  else if ATag = ':Year:' then
    Result := FormatDateTime('yyyy', Now)
    
  else if ATag = ':Month:' then
    Result := FormatDateTime('mm', Now)
    
  else if ATag = ':Day:' then
    Result := FormatDateTime('dd', Now)
    
  else
    Result := '';
end;
```

### GetCounterValue实现

```pascal
function TMetaTagProcessor.GetCounterValue(const ATag: string): string;
begin
  if ATag = ':Counter:' then
  begin
    Inc(FCounter);
    Result := IntToStr(FCounter);
  end
  
  else if ATag = ':Unique_ID:' then
  begin
    // 基于时间戳+计数器生成唯一ID
    Result := Format('%s_%d', [FormatDateTime('yyyymmddhhnnss', Now), FCounter]);
    Inc(FCounter);
  end
  
  else
    Result := '';
end;
```

## UI Integration

### Meta Tag选择对话框

```pascal
type
  TFormMetaTagSelector = class(TForm)
  private
    ListBoxCategories: TListBox;   // 左侧分类列表
    ListBoxTags: TListBox;         // 右侧标签列表
    MemoDescription: TMemo;        // 底部描述
    ButtonInsert: TButton;         // 插入按钮
    
    procedure LoadTags;
    procedure ListBoxCategoriesClick(Sender: TObject);
    procedure ListBoxTagsDblClick(Sender: TObject);
  public
    SelectedTag: string;
  end;

procedure TFormMetaTagSelector.LoadTags;
var
  I: Integer;
begin
  // 加载分类
  ListBoxCategories.Clear;
  for I := Low(META_TAG_CATEGORIES) to High(META_TAG_CATEGORIES) do
    ListBoxCategories.Items.Add(META_TAG_CATEGORIES[I]);
    
  ListBoxCategories.ItemIndex := 0;
  ListBoxCategoriesClick(nil);
end;

procedure TFormMetaTagSelector.ListBoxCategoriesClick(Sender: TObject);
var
  CategoryIndex: Integer;
  I: Integer;
begin
  CategoryIndex := ListBoxCategories.ItemIndex;
  if CategoryIndex < 0 then Exit;
  
  // 加载该分类的标签
  ListBoxTags.Clear;
  for I := Low(META_TAGS) to High(META_TAGS) do
  begin
    if META_TAGS[I].Category = CategoryIndex then
      ListBoxTags.Items.AddObject(META_TAGS[I].Tag, TObject(I));
  end;
end;

procedure TFormMetaTagSelector.ListBoxTagsDblClick(Sender: TObject);
var
  TagIndex: Integer;
begin
  if ListBoxTags.ItemIndex < 0 then Exit;
  
  TagIndex := Integer(ListBoxTags.Items.Objects[ListBoxTags.ItemIndex]);
  SelectedTag := META_TAGS[TagIndex].Tag;
  MemoDescription.Text := META_TAGS[TagIndex].Description;
  
  ModalResult := mrOK;
end;
```

### Replace规则集成

```pascal
// TFrame_RuleReplace
SpeedButton_ReplaceWithMetaTag: TSpeedButton
  Left = 378
  Top = 56
  Width = 23
  Height = 25
  Flat = True
  Hint = "Insert Meta Tag"
  OnClick = SpeedButton_ReplaceWithMetaTagClick

procedure TFrame_RuleReplace.SpeedButton_ReplaceWithMetaTagClick(Sender: TObject);
var
  Selector: TFormMetaTagSelector;
begin
  Selector := TFormMetaTagSelector.Create(Self);
  try
    if Selector.ShowModal = mrOK then
    begin
      // 插入到光标位置
      Edit_ReplaceWith.SelText := Selector.SelectedTag;
      Edit_ReplaceWith.SetFocus;
    end;
  finally
    Selector.Free;
  end;
end;
```

## Testing Requirements

### 单元测试

```pascal
type
  TMetaTagProcessorTest = class(TTestCase)
  published
    procedure TestFileProperties;
    procedure TestExifData;
    procedure TestRandomValues;
    procedure TestDateTime;
    procedure TestCounter;
    procedure TestMultipleTags;
    procedure TestCaching;
  end;

procedure TMetaTagProcessorTest.TestFileProperties;
var
  Processor: TMetaTagProcessor;
  File: TRenFile;
  Result: string;
begin
  Processor := TMetaTagProcessor.Create;
  try
    File.FullPath := 'C:\Test\document.txt';
    
    Result := Processor.ProcessTags(':File_Name:', File);
    AssertEquals('document', Result);
    
    Result := Processor.ProcessTags(':File_NameExt:', File);
    AssertEquals('document.txt', Result);
    
    Result := Processor.ProcessTags(':File_Extension:', File);
    AssertEquals('.txt', Result);
  finally
    Processor.Free;
  end;
end;

procedure TMetaTagProcessorTest.TestMultipleTags;
var
  Processor: TMetaTagProcessor;
  File: TRenFile;
  Result: string;
begin
  Processor := TMetaTagProcessor.Create;
  try
    File.FullPath := 'C:\Photos\IMG_001.jpg';
    
    Result := Processor.ProcessTags(':File_Name:_:Counter:', File);
    AssertEquals('IMG_001_1', Result);
    
    Result := Processor.ProcessTags(':File_Name:_:Counter:', File);
    AssertEquals('IMG_001_2', Result);
  finally
    Processor.Free;
  end;
end;
```

### 集成测试

- 100张JPEG图片的EXIF读取速度<1s
- Meta Tags在Replace/Insert/Rearrange规则中正确工作
- 缓存机制有效(重复读取同一文件不重新解析EXIF)
- Counter标签在批量处理中递增正确

## Performance Requirements

- EXIF读取<10ms/文件(已缓存)
- Meta Tag替换<1ms/标签
- 缓存命中率>90%(批量处理同一文件)

## 验收标准

WPF 现状：Meta Tags 已实现并接入规则体系；以下清单用于历史对照。

### Phase 1: 基本Meta Tags
- [ ] 文件属性标签全部工作
- [ ] 日期时间标签正确
- [ ] 随机值标签生成正确
- [ ] Counter标签递增正确

### Phase 2: EXIF支持
- [ ] JPEG EXIF读取正确
- [ ] PNG元数据读取
- [ ] MP4元数据读取
- [ ] 无EXIF文件不报错

### Phase 3: UI集成
- [ ] Meta Tag选择对话框创建
- [ ] Replace规则SpeedButton工作
- [ ] Insert规则BitBtn工作
- [ ] Rearrange规则SB工作

### Phase 4: 性能优化
- [ ] EXIF缓存实现
- [ ] 批量处理性能达标
- [ ] 内存占用合理

### Phase 5: 高级功能
- [ ] GPS坐标格式化
- [ ] 自定义日期格式
- [ ] 条件标签(if-then-else)

## Dependencies

### 系统单元
- `SysUtils` - 文件/日期处理
- `Classes` - 流处理

### 第三方库
- **dexif** (推荐) - Free Pascal EXIF库
  - 下载: https://github.com/cutec-chris/dexif
  - 或使用 ExifTool命令行包装器

### 自定义单元
- `RuleEngine.Interfaces` - IRule接口
- `FileUtils.pas` - 文件工具函数

## Next Steps

1. **选择EXIF库**
   - 评估 dexif vs ExifTool
   - 编译测试读取性能

2. **实现TMetaTagProcessor类**
   - GetFileProperty
   - GetExifProperty (基本版)
   - GetRandomValue
   - GetDateTimeValue

3. **创建Meta Tag选择对话框**
   - 设计UI布局
   - 实现分类浏览
   - 双击插入功能

4. **集成到规则Frame**
   - Replace添加SpeedButton
   - Insert添加BitBtn
   - Rearrange添加SB

5. **单元测试与优化**
   - EXIF读取测试
   - 缓存性能测试
   - 内存泄漏检测

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 001 (Rule Engine Core), Feature 002 (Main Window UI)
