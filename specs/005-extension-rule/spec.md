# Feature 005: Extension Rule Implementation

## 概述

**Feature Name**: Extension规则(扩展名修改)  
**Priority**: P1 (High - 核心文件重命名功能)  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: Feature 001（规则引擎核心）

WPF 对齐说明：规则实现见 `ReNamerWPF/ReNamer/Rules/OtherRules.cs`，配置面板见 `ReNamerWPF/ReNamer/Views/RuleConfigs/ExtensionConfigPanel.xaml(.cs)`。

## 问题陈述

Extension规则用于修改文件扩展名,支持:
1. **手动设置扩展名** - 直接指定新扩展名
2. **二进制签名检测** - 通过文件头魔数自动识别真实文件类型
3. **追加模式** - 添加扩展名而非替换
4. **重复扩展名移除** - 清理如 `file.txt.txt` 的情况
5. **大小写控制** - Case sensitive选项

核心难点: **二进制签名检测** - 需要识别常见文件格式的Magic Bytes。

## 需求（基于 DFM 分析）

以下为 DFM 分析的历史参考；WPF 已实现 Extension 规则，功能以当前实现为准。

### Frame定义 (对照 docs/rules_requirements.md 第235-248行)

```pascal
type
  TFrame_RuleExtension = class(TFrame)
  private
    // 主要功能
    CheckBox_NewExtension: TCheckBox;        // 启用新扩展名
    Edit_Extension: TEdit;                   // 新扩展名输入
    CheckBox_ExtensionAppend: TCheckBox;     // 追加模式
    CheckBox_DetectBinSign: TCheckBox;       // 二进制签名检测
    
    // 选项
    CheckBox_RemoveDuplicateExtensions: TCheckBox; // 移除重复扩展名
    CheckBox_CaseSensitive: TCheckBox;             // 大小写敏感
  end;
```

### 控件属性详解 (第239-248行)

```pascal
// 新扩展名
CheckBox_NewExtension: TCheckBox
  Caption = "New file extension (without the dot):"
  Left = 40
  Top = 24
  TabOrder = 0
  Checked = True  // 默认勾选
  State = cbChecked

Edit_Extension: TEdit
  Left = 56
  Top = 48
  Width = 208
  TabOrder = 1
  Anchors = [akTop, akLeft, akRight]
  Font.Height = 243
  Text = ""  // 空表示移除扩展名

// 追加模式
CheckBox_ExtensionAppend: TCheckBox
  Caption = "Append to the original filename"
  Left = 56
  Top = 80
  TabOrder = 2
  Checked = False

// 二进制签名检测
CheckBox_DetectBinSign: TCheckBox
  Caption = "Detect using \"binary signature\""
  Left = 56
  Top = 104
  TabOrder = 3
  Checked = False
  OnClick = CheckBox_DetectBinSignClick

Label_Hint: TLabel
  Left = 56
  Top = 136
  Width = 344
  Height = 64
  AutoSize = False
  WordWrap = True
  Anchors = [akTop, akLeft, akRight]
  Caption = "Note: Some files may have multiple extensions matching their data type, for example: doc/ppt/xls have the same signature. Unrecognised files will remain unchanged."

// 移除重复扩展名
CheckBox_RemoveDuplicateExtensions: TCheckBox
  Caption = "Remove duplicate extensions"
  Left = 40
  Top = 208
  TabOrder = 4
  Checked = False

// 大小写敏感
CheckBox_CaseSensitive: TCheckBox
  Caption = "Case sensitive"
  Left = 56
  Top = 232
  TabOrder = 5
  Checked = False
```

## Binary Signature Detection (魔数检测)

### 文件签名数据库

```pascal
type
  /// 文件签名定义
  TFileSignature = record
    Extension: string;      // 扩展名(如 "jpg")
    MagicBytes: TBytes;     // 魔数字节序列
    Offset: Integer;        // 偏移量(通常为0)
    Description: string;    // 描述
  end;

const
  /// 常见文件签名数据库(部分)
  FILE_SIGNATURES: array[0..30] of TFileSignature = (
    // 图像格式
    (Extension: 'jpg';  MagicBytes: [$FF, $D8, $FF];                     Offset: 0; Description: 'JPEG Image'),
    (Extension: 'png';  MagicBytes: [$89, $50, $4E, $47, $0D, $0A, $1A, $0A]; Offset: 0; Description: 'PNG Image'),
    (Extension: 'gif';  MagicBytes: [$47, $49, $46, $38];                Offset: 0; Description: 'GIF Image'),
    (Extension: 'bmp';  MagicBytes: [$42, $4D];                          Offset: 0; Description: 'BMP Image'),
    (Extension: 'webp'; MagicBytes: [$52, $49, $46, $46];                Offset: 0; Description: 'WebP Image'),
    
    // 文档格式
    (Extension: 'pdf';  MagicBytes: [$25, $50, $44, $46];                Offset: 0; Description: 'PDF Document'),
    (Extension: 'doc';  MagicBytes: [$D0, $CF, $11, $E0, $A1, $B1, $1A, $E1]; Offset: 0; Description: 'MS Office Document'),
    (Extension: 'docx'; MagicBytes: [$50, $4B, $03, $04];                Offset: 0; Description: 'Office Open XML'),
    (Extension: 'rtf';  MagicBytes: [$7B, $5C, $72, $74, $66];           Offset: 0; Description: 'Rich Text Format'),
    
    // 压缩文件
    (Extension: 'zip';  MagicBytes: [$50, $4B, $03, $04];                Offset: 0; Description: 'ZIP Archive'),
    (Extension: 'rar';  MagicBytes: [$52, $61, $72, $21, $1A, $07];      Offset: 0; Description: 'RAR Archive'),
    (Extension: '7z';   MagicBytes: [$37, $7A, $BC, $AF, $27, $1C];      Offset: 0; Description: '7-Zip Archive'),
    (Extension: 'gz';   MagicBytes: [$1F, $8B];                          Offset: 0; Description: 'GZip Archive'),
    
    // 音频格式
    (Extension: 'mp3';  MagicBytes: [$FF, $FB];                          Offset: 0; Description: 'MP3 Audio'),
    (Extension: 'mp3';  MagicBytes: [$49, $44, $33];                     Offset: 0; Description: 'MP3 Audio (ID3)'),
    (Extension: 'wav';  MagicBytes: [$52, $49, $46, $46];                Offset: 0; Description: 'WAV Audio'),
    (Extension: 'flac'; MagicBytes: [$66, $4C, $61, $43];                Offset: 0; Description: 'FLAC Audio'),
    
    // 视频格式
    (Extension: 'mp4';  MagicBytes: [$66, $74, $79, $70];                Offset: 4; Description: 'MP4 Video'),
    (Extension: 'avi';  MagicBytes: [$52, $49, $46, $46];                Offset: 0; Description: 'AVI Video'),
    (Extension: 'mkv';  MagicBytes: [$1A, $45, $DF, $A3];                Offset: 0; Description: 'Matroska Video'),
    
    // 可执行文件
    (Extension: 'exe';  MagicBytes: [$4D, $5A];                          Offset: 0; Description: 'Windows Executable'),
    (Extension: 'dll';  MagicBytes: [$4D, $5A];                          Offset: 0; Description: 'Windows DLL'),
    
    // 脚本/代码
    (Extension: 'xml';  MagicBytes: [$3C, $3F, $78, $6D, $6C];           Offset: 0; Description: 'XML Document'),
    (Extension: 'html'; MagicBytes: [$3C, $21, $44, $4F, $43, $54, $59, $50, $45]; Offset: 0; Description: 'HTML Document'),
    
    // 数据库
    (Extension: 'sqlite'; MagicBytes: [$53, $51, $4C, $69, $74, $65, $20, $66, $6F, $72, $6D, $61, $74, $20, $33]; Offset: 0; Description: 'SQLite Database'),
    
    // 字体
    (Extension: 'ttf';  MagicBytes: [$00, $01, $00, $00, $00];           Offset: 0; Description: 'TrueType Font'),
    (Extension: 'otf';  MagicBytes: [$4F, $54, $54, $4F];                Offset: 0; Description: 'OpenType Font'),
    
    // 其他
    (Extension: 'iso';  MagicBytes: [$43, $44, $30, $30, $31];           Offset: $8001; Description: 'ISO Image'),
    (Extension: 'ps';   MagicBytes: [$25, $21, $50, $53];                Offset: 0; Description: 'PostScript'),
    (Extension: 'epub'; MagicBytes: [$50, $4B, $03, $04];                Offset: 0; Description: 'EPUB eBook'),
    (Extension: 'swf';  MagicBytes: [$46, $57, $53];                     Offset: 0; Description: 'Flash SWF')
  );
```

### 魔数检测实现

```pascal
function DetectFileExtensionBySignature(const AFilePath: string): string;
var
  FileStream: TFileStream;
  Buffer: array[0..255] of Byte;
  BytesRead: Integer;
  I: Integer;
  Match: Boolean;
  J: Integer;
begin
  Result := '';
  
  if not FileExists(AFilePath) then Exit;
  
  try
    FileStream := TFileStream.Create(AFilePath, fmOpenRead or fmShareDenyNone);
    try
      // 读取文件头256字节
      BytesRead := FileStream.Read(Buffer, SizeOf(Buffer));
      if BytesRead = 0 then Exit;
      
      // 遍历签名数据库
      for I := Low(FILE_SIGNATURES) to High(FILE_SIGNATURES) do
      begin
        // 检查偏移量是否在读取范围内
        if FILE_SIGNATURES[I].Offset + Length(FILE_SIGNATURES[I].MagicBytes) > BytesRead then
          Continue;
          
        // 比较魔数
        Match := True;
        for J := 0 to High(FILE_SIGNATURES[I].MagicBytes) do
        begin
          if Buffer[FILE_SIGNATURES[I].Offset + J] <> FILE_SIGNATURES[I].MagicBytes[J] then
          begin
            Match := False;
            Break;
          end;
        end;
        
        // 找到匹配
        if Match then
        begin
          Result := FILE_SIGNATURES[I].Extension;
          Exit;
        end;
      end;
    finally
      FileStream.Free;
    end;
  except
    // 文件读取错误,返回空
    Result := '';
  end;
end;
```

### 歧义处理

```pascal
// 问题: doc/xls/ppt 使用相同的 OLE 签名 (D0 CF 11 E0 A1 B1 1A E1)
// 解决: 读取OLE流内部结构区分

function DetectOfficeFormat(const AFilePath: string): string;
var
  FileStream: TFileStream;
  Buffer: array[0..511] of Byte;
  BytesRead: Integer;
begin
  Result := 'doc'; // 默认为doc
  
  FileStream := TFileStream.Create(AFilePath, fmOpenRead or fmShareDenyNone);
  try
    BytesRead := FileStream.Read(Buffer, SizeOf(Buffer));
    if BytesRead < 512 then Exit;
    
    // 检查OLE流中的特征字符串
    // Word: "WordDocument"
    // Excel: "Workbook"
    // PowerPoint: "Current User" / "PowerPoint Document"
    
    if PosInBuffer('WordDocument', Buffer, BytesRead) > 0 then
      Result := 'doc'
    else if PosInBuffer('Workbook', Buffer, BytesRead) > 0 then
      Result := 'xls'
    else if (PosInBuffer('Current User', Buffer, BytesRead) > 0) or
            (PosInBuffer('PowerPoint Document', Buffer, BytesRead) > 0) then
      Result := 'ppt';
  finally
    FileStream.Free;
  end;
end;
```

## Rule Interface Implementation

```pascal
type
  /// Extension规则参数
  TExtensionParams = record
    NewExtension: string;           // 新扩展名(不含点)
    UseNewExtension: Boolean;       // 启用新扩展名
    AppendMode: Boolean;            // 追加模式
    DetectBinarySignature: Boolean; // 二进制签名检测
    RemoveDuplicates: Boolean;      // 移除重复扩展名
    CaseSensitive: Boolean;         // 大小写敏感
  end;

  /// Extension规则实现
  TExtensionRule = class(TInterfacedObject, IRule)
  private
    FParams: TExtensionParams;
    
    function GetCurrentExtension(const AFileName: string): string;
    function RemoveDuplicateExtensions(const AFileName: string): string;
  public
    constructor Create(const AParams: TExtensionParams);
    
    // IRule 接口
    function GetRuleType: TRuleType;
    function GetDescription: string;
    procedure Execute(var AFileName: string; const AFile: TRenFile);
    procedure Reset;
    function Serialize: TJSONObject;
    class function Deserialize(AJson: TJSONObject): IRule;
  end;
```

### Execute实现

```pascal
procedure TExtensionRule.Execute(var AFileName: string; const AFile: TRenFile);
var
  BaseName, OldExt, NewExt: string;
begin
  // 移除重复扩展名
  if FParams.RemoveDuplicates then
    AFileName := RemoveDuplicateExtensions(AFileName);
    
  // 分离文件名和扩展名
  BaseName := ChangeFileExt(AFileName, '');
  OldExt := GetCurrentExtension(AFileName);
  
  // 确定新扩展名
  if FParams.DetectBinarySignature then
  begin
    // 通过魔数检测
    NewExt := DetectFileExtensionBySignature(AFile.FullPath);
    
    // 如果检测失败,保持不变
    if NewExt = '' then
    begin
      AFileName := BaseName + OldExt;
      Exit;
    end;
  end
  else if FParams.UseNewExtension then
  begin
    // 使用用户指定的扩展名
    NewExt := Trim(FParams.NewExtension);
  end
  else
  begin
    // 都未启用,保持不变
    Exit;
  end;
  
  // 应用新扩展名
  if FParams.AppendMode then
  begin
    // 追加模式: 保留原扩展名
    if NewExt <> '' then
      AFileName := BaseName + OldExt + '.' + NewExt
    else
      AFileName := BaseName + OldExt;
  end
  else
  begin
    // 替换模式: 替换扩展名
    if NewExt <> '' then
      AFileName := BaseName + '.' + NewExt
    else
      AFileName := BaseName;  // 空扩展名=移除扩展名
  end;
end;
```

### GetCurrentExtension实现

```pascal
function TExtensionRule.GetCurrentExtension(const AFileName: string): string;
var
  DotPos: Integer;
begin
  DotPos := LastDelimiter('.', AFileName);
  
  if DotPos = 0 then
    Result := ''
  else
  begin
    Result := Copy(AFileName, DotPos, Length(AFileName) - DotPos + 1);
    
    // 大小写处理
    if not FParams.CaseSensitive then
      Result := LowerCase(Result);
  end;
end;
```

### RemoveDuplicateExtensions实现

```pascal
function TExtensionRule.RemoveDuplicateExtensions(const AFileName: string): string;
var
  BaseName, Ext: string;
  PrevExt: string;
  CompareFunc: function(const S1, S2: string): Integer;
begin
  Result := AFileName;
  
  // 确定比较函数
  if FParams.CaseSensitive then
    CompareFunc := @CompareStr
  else
    CompareFunc := @CompareText;
    
  // 循环移除重复扩展名
  repeat
    BaseName := ChangeFileExt(Result, '');
    Ext := ExtractFileExt(Result);
    
    if Ext = '' then Break;
    
    PrevExt := ExtractFileExt(BaseName);
    
    // 检查上一个扩展名是否与当前扩展名相同
    if (PrevExt <> '') and (CompareFunc(PrevExt, Ext) = 0) then
      Result := BaseName  // 移除重复的扩展名
    else
      Break;  // 不重复,退出
  until False;
end;
```

**示例**:
```pascal
// 输入: "file.txt.txt.txt"
// 输出: "file.txt"

// 输入: "document.PDF.pdf" (Case insensitive)
// 输出: "document.pdf"

// 输入: "archive.tar.gz" (不同扩展名,不移除)
// 输出: "archive.tar.gz"
```

### GetDescription实现

```pascal
function TExtensionRule.GetDescription: string;
begin
  if FParams.DetectBinarySignature then
    Result := 'Detect extension by binary signature'
  else if FParams.UseNewExtension then
  begin
    if FParams.AppendMode then
      Result := Format('Append extension: .%s', [FParams.NewExtension])
    else if FParams.NewExtension = '' then
      Result := 'Remove extension'
    else
      Result := Format('Set extension: .%s', [FParams.NewExtension]);
  end
  else
    Result := 'Extension (no changes)';
    
  if FParams.RemoveDuplicates then
    Result := Result + ' | Remove duplicates';
end;
```

### Serialization实现

```pascal
function TExtensionRule.Serialize: TJSONObject;
begin
  Result := TJSONObject.Create;
  
  Result.Add('RuleType', 'Extension');
  Result.Add('NewExtension', FParams.NewExtension);
  Result.Add('UseNewExtension', FParams.UseNewExtension);
  Result.Add('AppendMode', FParams.AppendMode);
  Result.Add('DetectBinarySignature', FParams.DetectBinarySignature);
  Result.Add('RemoveDuplicates', FParams.RemoveDuplicates);
  Result.Add('CaseSensitive', FParams.CaseSensitive);
end;

class function TExtensionRule.Deserialize(AJson: TJSONObject): IRule;
var
  Params: TExtensionParams;
begin
  Params.NewExtension := AJson.Get('NewExtension', '');
  Params.UseNewExtension := AJson.Get('UseNewExtension', True);
  Params.AppendMode := AJson.Get('AppendMode', False);
  Params.DetectBinarySignature := AJson.Get('DetectBinarySignature', False);
  Params.RemoveDuplicates := AJson.Get('RemoveDuplicates', False);
  Params.CaseSensitive := AJson.Get('CaseSensitive', False);
  
  Result := TExtensionRule.Create(Params);
end;
```

## Edge Cases & Validation

### 扩展名验证

```pascal
function IsValidExtension(const AExtension: string): Boolean;
const
  INVALID_CHARS = ['\', '/', ':', '*', '?', '"', '<', '>', '|', '.'];
var
  C: Char;
begin
  Result := True;
  
  // 空扩展名有效(表示移除扩展名)
  if AExtension = '' then Exit;
  
  // 检查非法字符
  for C in AExtension do
  begin
    if C in INVALID_CHARS then
    begin
      Result := False;
      Exit;
    end;
  end;
  
  // 检查长度(Windows扩展名通常<16字符)
  if Length(AExtension) > 255 then
    Result := False;
end;
```

### 二进制检测失败处理

```pascal
// 场景1: 文件无法读取(权限不足)
// 行为: 保持文件名不变,不抛出异常

// 场景2: 文件签名未识别
// 行为: 保持文件名不变(按Label_Hint提示)

// 场景3: 歧义格式(doc/xls/ppt)
// 行为: 返回最可能的格式,或默认为第一个匹配
```

### 追加模式与替换模式

```pascal
// 场景: 文件名="report.txt", 新扩展名="pdf"

// 替换模式 (AppendMode=False):
// 结果: "report.pdf"

// 追加模式 (AppendMode=True):
// 结果: "report.txt.pdf"

// 场景: 无扩展名文件="README", 新扩展名="md"

// 替换模式:
// 结果: "README.md"

// 追加模式:
// 结果: "README.md" (无扩展名时,追加等同于替换)
```

### 重复扩展名移除

```pascal
// 大小写敏感 (CaseSensitive=True):
// "file.TXT.txt" → "file.TXT.txt" (不移除,因为大小写不同)

// 大小写不敏感 (CaseSensitive=False):
// "file.TXT.txt" → "file.txt" (移除重复)

// 不同扩展名:
// "archive.tar.gz" → "archive.tar.gz" (保持不变)

// 三重重复:
// "doc.pdf.pdf.pdf" → "doc.pdf"
```

## Testing Requirements

### 单元测试

```pascal
type
  TExtensionRuleTest = class(TTestCase)
  published
    procedure TestSetExtension;
    procedure TestRemoveExtension;
    procedure TestAppendExtension;
    procedure TestBinarySignature_JPEG;
    procedure TestBinarySignature_PNG;
    procedure TestBinarySignature_PDF;
    procedure TestBinarySignature_Unknown;
    procedure TestRemoveDuplicates;
    procedure TestCaseSensitive;
    procedure TestInvalidExtension;
  end;

procedure TExtensionRuleTest.TestSetExtension;
var
  Rule: IRule;
  Params: TExtensionParams;
  FileName: string;
  File: TRenFile;
begin
  Params.NewExtension := 'pdf';
  Params.UseNewExtension := True;
  Params.AppendMode := False;
  
  Rule := TExtensionRule.Create(Params);
  
  FileName := 'document.txt';
  Rule.Execute(FileName, File);
  AssertEquals('document.pdf', FileName);
end;

procedure TExtensionRuleTest.TestBinarySignature_JPEG;
var
  Rule: IRule;
  Params: TExtensionParams;
  FileName: string;
  File: TRenFile;
  TempFile: string;
begin
  // 创建测试JPEG文件
  TempFile := CreateTestJPEG;
  try
    Params.DetectBinarySignature := True;
    
    Rule := TExtensionRule.Create(Params);
    
    File.FullPath := TempFile;
    FileName := 'image.bin';
    Rule.Execute(FileName, File);
    
    AssertEquals('image.jpg', FileName);
  finally
    DeleteFile(TempFile);
  end;
end;

procedure TExtensionRuleTest.TestRemoveDuplicates;
var
  Rule: IRule;
  Params: TExtensionParams;
  FileName: string;
  File: TRenFile;
begin
  Params.RemoveDuplicates := True;
  Params.CaseSensitive := False;
  
  Rule := TExtensionRule.Create(Params);
  
  FileName := 'file.txt.TXT.txt';
  Rule.Execute(FileName, File);
  AssertEquals('file.txt', FileName);
end;
```

### 集成测试

- 100个不同格式文件的魔数检测正确率>95%
- 重复扩展名移除覆盖所有边界情况
- 追加模式与其他规则组合测试
- 大文件(>100MB)魔数检测性能<100ms

## Performance Requirements

- 魔数检测仅读取文件头256-512字节,速度<10ms/文件
- 重复扩展名移除 O(n) 时间复杂度,n为扩展名数量
- 无文件I/O的操作<1μs

## 验收标准

WPF 现状：Extension 规则已实现；以下清单用于历史对照。

### Phase 1: 基本扩展名操作
- [ ] 设置新扩展名正确
- [ ] 移除扩展名正确(空输入)
- [ ] 追加模式工作
- [ ] 大小写敏感/不敏感正确

### Phase 2: 重复扩展名移除
- [ ] 单个重复移除
- [ ] 多个连续重复移除
- [ ] 大小写组合处理正确
- [ ] 不同扩展名不误删

### Phase 3: 二进制签名检测
- [ ] JPEG/PNG/GIF/BMP识别正确
- [ ] PDF/DOC/DOCX识别正确
- [ ] ZIP/RAR/7Z识别正确
- [ ] MP3/MP4/AVI识别正确
- [ ] 未知格式保持不变
- [ ] 文件读取失败不崩溃

### Phase 4: 歧义处理
- [ ] DOC/XLS/PPT通过内部结构区分
- [ ] ZIP/DOCX/EPUB通过内容区分
- [ ] 多个可能扩展名返回最合理选项

### Phase 5: 序列化
- [ ] JSON序列化/反序列化
- [ ] 所有参数保存/恢复
- [ ] 预设兼容

## Dependencies

### 系统单元
- `SysUtils` - 文件操作
- `Classes` - TFileStream

### 自定义单元
- `RuleEngine.Interfaces` - IRule接口
- `FileSignatures.pas` - 魔数数据库(新建)

## Next Steps

1. **创建魔数数据库**
   - 定义 `FileSignatures.pas` 单元
   - 收集100+常见文件格式签名
   - 实现检测函数

2. **实现TExtensionRule类**
   - Execute核心逻辑
   - RemoveDuplicateExtensions
   - GetCurrentExtension

3. **实现Frame界面**
   - 创建TFrame_RuleExtension
   - 绑定控件事件
   - CheckBox_DetectBinSignClick互斥逻辑

4. **单元测试**
   - 创建测试文件生成器
   - 所有魔数检测测试
   - 边界条件测试

5. **集成到规则引擎**
   - 注册到TRuleRegistry
   - 添加到规则选择对话框

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 001 (Rule Engine Core), Feature 004 (Serialize Rule)
