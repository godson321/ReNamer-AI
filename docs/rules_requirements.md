# ReNamer 7.8 功能需求（规则窗口 + 主界面）

基于反编译 DFM 资源的详细功能分析。

# 规则窗口功能需求

## Form_AddRule（添加/编辑规则对话框）

- 类名：`TForm_AddRule`
- 尺寸：Width=617, Height=385（ClientWidth=617, ClientHeight=385）
- 初始位置：Left=426, Top=247（被 Position 覆盖）
- Position：`poMainFormCenter`（瑖主窗口居中）
- Caption："Add Rule"（编辑模式下运行时改为 "Edit Rule"）
- AllowDropFiles：True（支持从资源管理器拖放文件到对话框，触发 `FormDropFiles` 添加到主窗口文件列表）

### 事件
- `OnCreate` → `FormCreate`：初始化规则类型列表、创建各规则 Frame 实例
- `OnDestroy` → `FormDestroy`：释放所有 Frame 实例
- `OnDropFiles` → `FormDropFiles`：接收拖放文件，转发给主窗口处理
- `OnShow` → `FormShow`：显示时恢复上次选中的规则类型、填充编辑数据

### 左侧规则类型列表
- `ListBox_Rules`：TListBox，Left=16, Top=16, Width=129, Height=316
- Anchors=[akTop, akLeft, akBottom]（随窗口高度拉伸）
- Font.Height=243（特定字体大小，非系统默认）
- PopupMenu=`PopupMenu_GlobalRules`：右键菜单含 `MenuItem_RuleDescription`（Caption="Description"，OnClick=`MenuItem_RuleDescriptionClick`）—— 查看规则描述
- `OnClick` → `ListBox_RulesClick`：点击切换右侧 GroupBox_Config 中显示的规则 Frame
- `OnDblClick` → `ListBox_RulesDblClick`：双击确认添加当前选中规则（等同点击 "Add Rule" 按钮）
- 运行时填充 18 种规则类型：Replace, Insert, Delete, Remove, Case, Serialize, RegEx, Extension, Padding, Strip, Clean Up, Transliterate, Rearrange, Reformat Date, Randomize, PascalScript, User Input, Mapping

### 右侧配置区域
- `GroupBox_Config`：TGroupBox，Left=152, Top=11, Width=449, Height=321
- Caption="Configuration:"
- Anchors=[akTop, akLeft, akRight, akBottom]（四方向拉伸）
- 内部动态嵌入当前选中规则类型的 Frame，切换规则时隐藏旧 Frame 显示新 Frame

### 底部按钮
- `BitBtn_Add`：TBitBtn，Left=16, Top=342, Width=409, Height=33
  - Caption="Add Rule"，Anchors=[akLeft, akRight, akBottom]
  - **Default=True**（按 Enter 键触发）
  - **ModalResult=6**（mrYes）
  - 带图标（Glyph 内嵌 BMP，13×13 像素箭头图标）
  - `OnClick` → `BitBtn_AddClick`：读取当前 Frame 的参数，创建规则对象添加到主窗口规则列表
- `BitBtn_Close`：TBitBtn，Left=440, Top=342, Width=161, Height=33
  - Caption="Close"，Anchors=[akRight, akBottom]
  - **Cancel=True**（按 Esc 键触发）
  - **ModalResult=2**（mrCancel）
  - `OnClick` → `BitBtn_CloseClick`：关闭对话框

每个规则以 Frame 形式嵌入 GroupBox_Config 区域。

---

## 1. Replace（替换）

Frame: `TFrame_RuleReplace`，尺寸 400×230

### 输入控件
- `Edit_ReplaceWhat`：TEdit（TabOrder=0），标签 `Label_ReplaceWhat`（"Find:"），Left=80, Top=24, Width=273, Anchors=[akTop,akLeft,akRight]，Font.Height=243
- `Edit_ReplaceWith`：TEdit（TabOrder=1），标签 `Label_ReplaceWith`（"Replace:"），Left=80, Top=56, Width=273, Anchors=[akTop,akLeft,akRight]，Font.Height=243，OnKeyDown=`Edit_ReplaceWithKeyDown`
- `SpeedButton_ReplaceAddSeparator`：TSpeedButton，Left=354, Top=24, Width=23, Height=25，Anchors=[akTop,akRight]，Flat=True，Hint="Separate multiple items"，带内嵌 Glyph，OnClick=`SpeedButton_ReplaceAddSeparatorClick`
- `SpeedButton_ReplaceWithMetaTag`：TSpeedButton，Left=354, Top=56, Width=23, Height=25，Anchors=[akTop,akRight]，Flat=True，Hint="Insert Meta Tag (Ctrl+Ins)"，带内嵌 Glyph (16×16)，OnClick=`SpeedButton_ReplaceWithMetaTagClick`

### 出现次数（RadioButton 三选一）
- 标签 `Label_ReplaceOccurances`（"Occurrences:"），Left=80, Top=91
- `RadioButton_ReplaceAll`（TabOrder=2）："All"，Left=80, Top=111，**默认选中**（Checked=True, TabStop=True）
- `RadioButton_ReplaceFirst`（TabOrder=3）："First"，Left=80, Top=131
- `RadioButton_ReplaceLast`（TabOrder=4）："Last"，Left=80, Top=151

### 选项（CheckBox，独立开关）
- `CheckBox_CaseSensitive`（TabOrder=5）："Case sensitive"，Left=232, Top=104，默认**未勾选**
- `CheckBox_WholeWordsOnly`（TabOrder=6）："Whole words only"，Left=232, Top=128，默认**未勾选**
- `CheckBox_SkipExtension`（TabOrder=7）："Skip extension"，Left=232, Top=152，默认**已勾选**（Checked=True, State=cbChecked）
- `CheckBox_UseWildcards`（TabOrder=8）："Interpret '?', '*', '[', ']' as wildcards and '$n' as backreferences?"，Left=16, Top=192, Width=339，默认**未勾选**，OnClick=`CheckBox_UseWildcardsClick`（勾选后禁用 CaseSensitive/WholeWordsOnly 等不兼容选项）

---

## 2. Insert（插入）

Frame: `TFrame_RuleInsert`，尺寸 400×263

### 输入控件
- `Edit_Insert`：TEdit（TabOrder=0），标签 `Label_InsertWhat`（"Insert:"），Left=80, Top=24, Width=289, Anchors=[akTop,akLeft,akRight]，Font.Height=243，OnKeyDown=`Edit_InsertKeyDown`
- `BitBtn_InsertMetaTag`：TBitBtn（TabOrder=1），Left=80, Top=49, Width=127, Height=26，Caption="Insert Meta Tag"，Hint="Insert Meta Tag (Ctrl+Ins)"，AutoSize=True，带内嵌 Glyph (16×16)，OnClick=`BitBtn_InsertMetaTagClick`

### 插入位置（RadioButton 六选一）
- 标签 `Label_InsertWhere`（"Where:"），Left=16, Top=106
- `RadioButton_InsertPrefix`（TabOrder=2）："Prefix"，Left=80, Top=80，**默认选中**（Checked=True, TabStop=True）—— 插入到文件名前面
- `RadioButton_InsertSuffix`（TabOrder=3）："Suffix"，Left=80, Top=104 —— 插入到文件名后面
- `RadioButton_InsertPosition`（TabOrder=4）："Position:"，Left=80, Top=128 —— 插入到指定字符位置
  - `SpinEdit_InsertPosition`（TabOrder=9）：TSpinEdit，Left=208, Top=126, Width=57，MinValue=1, MaxValue=260, 默认值 1，OnChange=`SpinEdit_InsertPositionChange`，OnKeyDown=`SpinEditKeyDown`
  - `CheckBox_InsertRight`（TabOrder=10）："Right-to-left"，Left=272, Top=128，默认**未勾选**，OnClick=`CheckBox_InsertRightClick`
- `RadioButton_InsertAfterText`（TabOrder=5）："After text:"，Left=80, Top=152 —— 在指定文本之后插入
  - `Edit_InsertAfterText`（TabOrder=11）：TEdit，Left=208, Top=150, Width=137，Font.Height=243，OnChange=`Edit_InsertAfterTextChange`
- `RadioButton_InsertBeforeText`（TabOrder=6）："Before text:"，Left=80, Top=176 —— 在指定文本之前插入
  - `Edit_InsertBeforeText`（TabOrder=12）：TEdit，Left=208, Top=174, Width=137，Font.Height=243，OnChange=`Edit_InsertBeforeTextChange`
- `RadioButton_ReplaceCurrentName`（TabOrder=7）："Replace current name"，Left=80, Top=200 —— 用插入内容完全替换当前文件名

### 选项
- `CheckBox_InsertSkipExtension`（TabOrder=8）："Skip extension"，Left=80, Top=228，默认**已勾选**（Checked=True, State=cbChecked）

---

## 3. Delete（删除）

Frame: `TFrame_RuleDelete`，尺寸 440×230

### From 组（`GroupBox_From`: TGroupBox，Left=16, Top=16, Width=192, Height=93）
- Caption="From:"，ClientWidth=188, ClientHeight=73，TabOrder=0
- `RadioButton_DeleteFromPosition`（GroupBox 内 TabOrder=0）："Position:"，Left=14, Top=10，**默认选中**（Checked=True, TabStop=True）
  - `SpinEdit_DeleteFromPosition`（GroupBox 内 TabOrder=2）：TSpinEdit，Left=120, Top=7, Width=57，MinValue=1, MaxValue=260, 默认值 1，OnChange=`SpinEdit_DeleteFromPositionChange`，OnKeyDown=`SpinEditKeyDown`
- `RadioButton_DeleteFromDelimiter`（GroupBox 内 TabOrder=1）："Delimiter:"，Left=14, Top=42
  - `Edit_DeleteFromDelimiter`（GroupBox 内 TabOrder=3）：TEdit，Left=120, Top=39, Width=57，MaxLength=260，Font.Height=243，OnChange=`Edit_DeleteFromDelimiterChange`

### Until 组（`GroupBox_Until`: TGroupBox，Left=224, Top=16, Width=193, Height=121）
- Caption="Until:"，ClientWidth=189, ClientHeight=101，TabOrder=1
- `RadioButton_DeleteUntilCount`（GroupBox 内 TabOrder=0）："Count:"，Left=14, Top=10，**默认选中**（Checked=True, TabStop=True）
  - `SpinEdit_DeleteUntilCount`（GroupBox 内 TabOrder=3）：TSpinEdit，Left=120, Top=7, Width=57，MinValue=1, MaxValue=260, 默认值 1，OnChange=`SpinEdit_DeleteUntilCountChange`，OnKeyDown=`SpinEditKeyDown`
- `RadioButton_DeleteUntilDelimiter`（GroupBox 内 TabOrder=1）："Delimiter:"，Left=14, Top=42
  - `Edit_DeleteUntilDelimiter`（GroupBox 内 TabOrder=4）：TEdit，Left=120, Top=39, Width=57，MaxLength=260，Font.Height=243，OnChange=`Edit_DeleteUntilDelimiterChange`
- `RadioButton_DeleteUntilEnd`（GroupBox 内 TabOrder=2）："Till the end"，Left=14, Top=74

### 选项
- `CheckBox_DeleteCurrentName`（TabOrder=2）："Delete current name"，Left=32, Top=160，默认**未勾选**，OnChange=`CheckBox_DeleteCurrentNameChange`（勾选后禁用 From/Until 组）
- `CheckBox_DeleteSkipExtension`（TabOrder=3）："Skip extension"，Left=32, Top=184，默认**已勾选**（Checked=True, State=cbChecked）
- `CheckBox_DeleteRightToLeft`（TabOrder=4）："Right-to-left"，Left=240, Top=160，默认**未勾选**
- `CheckBox_LeaveDelimiter`（TabOrder=5）："Do not remove delimiters"，Left=240, Top=184，默认**未勾选**

---

## 4. Remove（移除）

Frame: `TFrame_RuleRemove`，尺寸 400×230

### 输入控件
- `Edit_Remove`：TEdit（TabOrder=0），标签 `Label_RemoveWhat`（"Remove:"），Left=80, Top=24, Width=273, Anchors=[akTop,akLeft,akRight]，Font.Height=243
- `SpeedButton_RemoveAddSeparator`：TSpeedButton，Left=354, Top=24, Width=23, Height=25，Anchors=[akTop,akRight]，Flat=True，Hint="Separate multiple items"，OnClick=`SpeedButton_RemoveAddSeparatorClick`

### 出现次数（RadioButton 三选一）
- 标签 `Label_RemoveOccurances`（"Occurrences:"），Left=80, Top=64
- `RadioButton_RemoveAll`（TabOrder=1）："All"，Left=80, Top=84，**默认选中**（Checked=True, TabStop=True）
- `RadioButton_RemoveFirst`（TabOrder=2）："First"，Left=80, Top=104
- `RadioButton_RemoveLast`（TabOrder=3）："Last"，Left=80, Top=124

### 选项
- `CheckBox_CaseSensitive`（TabOrder=4）："Case sensitive"，Left=232, Top=80，默认**未勾选**
- `CheckBox_WholeWordsOnly`（TabOrder=5）："Whole words only"，Left=232, Top=104，默认**未勾选**
- `CheckBox_SkipExtension`（TabOrder=6）："Skip extension"，Left=232, Top=128，默认**已勾选**（Checked=True, State=cbChecked）
- `CheckBox_UseWildcards`（TabOrder=7）："Interpret symbols '?', '*', '[', ']' as wildcards?"，Left=24, Top=168, Width=245，默认**未勾选**，OnClick=`CheckBox_UseWildcardsClick`（勾选后禁用不兼容选项）

**与 Replace 的区别**：Remove 没有替换输入框，功能等同于 Replace 的替换文本为空。Remove 的通配符模式不支持 `$n` 反向引用。

---

## 5. Case（大小写转换）

Frame: `TFrame_RuleCase`，尺寸 440×265

### 大小写模式（`GroupBox_CaseChange`: TGroupBox，Left=16, Top=8, Width=200, Height=244，ClientWidth=196, ClientHeight=224，TabOrder=0）
- Caption="Case change:"
- `RB_CaseCapitalize`（GroupBox 内 TabOrder=0）："Capitalize Every Word"，Left=14, Top=8，**默认选中**（Checked=True, TabStop=True）
- `RB_CaseAllLower`（GroupBox 内 TabOrder=1）："all lower case"，Left=14, Top=31
- `RB_CaseAllUpper`（GroupBox 内 TabOrder=2）："ALL UPPER CASE"，Left=14, Top=54
- `RB_CaseInvert`（GroupBox 内 TabOrder=3）："iNVERT cASE"，Left=14, Top=77
- `RB_CaseFirstLetterCapital`（GroupBox 内 TabOrder=4）："First letter capital"，Left=14, Top=100
- `RB_CaseSentences`（GroupBox 内 TabOrder=5）："Sentence case"，Left=14, Top=123
- `RB_NoneOfAbove`（GroupBox 内 TabOrder=6）："(none of the above)"，Left=14, Top=146 —— 不做大小写转换

### GroupBox 内的选项
- `CheckBox_CasePreserve`（GroupBox 内 TabOrder=7）："Preserve case"，Left=14, Top=169，默认**未勾选**
- `CheckBox_CaseSkipExtension`（GroupBox 内 TabOrder=8）："Skip extension"，Left=14, Top=192，默认**已勾选**（Checked=True, State=cbChecked）

### GroupBox 外的选项
- `CheckBox_CaseForce`（TabOrder=1）："Force case for fragments:"，Left=232, Top=32，默认**未勾选**
  - `Edit_CaseForce`（TabOrder=2）：TEdit，Left=232, Top=56, Width=193，Font.Height=243，OnChange=`Edit_CaseForceChange`
  - `Label_CaseForceHint`：Left=232, Top=88, Width=193, Height=95，AutoSize=False, WordWrap=True，"Hint: fragments separated with comma will be put in the same case as they are typed in this text box, for example: CD,DVD,DJ"
- `CheckBox_CaseExtensionAlwaysLowerCase`（TabOrder=3）："Extension always lower case"，Left=232, Top=192，默认**未勾选**，OnClick=`CheckBox_CaseExtensionAlwaysLowerCaseClick`（与 UpperCase 互斥）
- `CheckBox_CaseExtensionAlwaysUpperCase`（TabOrder=4）："Extension always upper case"，Left=232, Top=216，默认**未勾选**，OnClick=`CheckBox_CaseExtensionAlwaysUpperCaseClick`（与 LowerCase 互斥）

---

## 6. Serialize（序列化/编号）

Frame: `TFrame_RuleSerialize`，尺寸 440×284

### 编号参数
- `SpinEdit_SerializeIndex`（TabOrder=0）：标签 `Label_IndexStarts`（"Index starts:"），Left=138, Top=24, Width=57，范围 -999999999 ~ 999999999，默认值 1，OnKeyDown=`SpinEditKeyDown`
- `SE_Repeat`（TabOrder=1）：标签 `Label_Repeat`（"Repeat:"），Left=138, Top=48, Width=57，范围 1 ~ 99999999，默认值 1，OnKeyDown=`SpinEditKeyDown` —— 每个编号重复使用的次数
- `SpinEdit_SerializeStep`（TabOrder=2）：标签 `Label_Step`（"Step:"），Left=138, Top=72, Width=57，范围 -99999999 ~ 99999999，默认值 1，OnKeyDown=`SpinEditKeyDown`

### 重置条件
- `CB_ResetEvery`（TabOrder=3）："Reset every:"，Left=24, Top=99，默认**未勾选**
  - `SE_ResetEveryCount`（TabOrder=4）：TSpinEdit，Left=138, Top=97, Width=57，范围 1 ~ 99999999，默认值 1，OnChange=`SE_ResetEveryCountChange`，OnKeyDown=`SpinEditKeyDown`
- `CheckBox_ResetIfFolderChanges`（TabOrder=5）："Reset if folder changes"，Left=24, Top=126，默认**未勾选**
- `CheckBox_ResetIfFileNameChanges`（TabOrder=6）："Reset if file name changes"，Left=24, Top=152，默认**未勾选**

### 零填充
- `CheckBox_SerializePadToLength`（TabOrder=7）："Pad with zeros to length:"，Left=24, Top=186，默认**未勾选**
  - `SpinEdit_SerializePadToLength`（TabOrder=8）：TSpinEdit，Left=224, Top=184, Width=57，范围 1 ~ 260，默认值 1，OnChange=`SpinEdit_SerializePadToLengthChange`，OnKeyDown=`SpinEditKeyDown`

### 编号系统
- `ComboBox_NumberingSystem`（TabOrder=10）：标签 `Label_NumberingSystem`（"Numbering system:"），Left=24, Top=240, Width=176，Style=csDropDownList，OnChange=`ComboBox_NumberingSystemChange`
- `Edit_CustomNumberingSymbols`（TabOrder=11）：标签 `Label_CustomNumberingSymbols`（"Custom numbering symbols:"），Left=224, Top=240, Width=193

### 插入位置（`GB_InsertWhere`: TGroupBox，Left=224, Top=16, Width=193, Height=144，Anchors=[akTop,akRight]）
- Caption="Insert where:"，TabOrder=9
- `RadioButton_SerializePrefix`（GroupBox 内 TabOrder=0）："Prefix"，Left=14, Top=6，**默认选中**（Checked=True, TabStop=True）
- `RadioButton_SerializeSuffix`（GroupBox 内 TabOrder=1）："Suffix"，Left=14, Top=27
- `RadioButton_SerializePosition`（GroupBox 内 TabOrder=2）："Position:"，Left=14, Top=48
  - `SpinEdit_SerializePosition`（GroupBox 内 TabOrder=4）：TSpinEdit，Left=120, Top=46, Width=49，范围 1 ~ 260，默认值 1，OnChange=`SpinEdit_SerializePositionChange`，OnKeyDown=`SpinEditKeyDown`
- `RadioButton_ReplaceCurrentName`（GroupBox 内 TabOrder=3）："Replace current name"，Left=14, Top=69
- `CheckBox_SkipExtension`（GroupBox 内 TabOrder=5）："Skip extension"，Left=14, Top=96，默认**已勾选**（Checked=True, State=cbChecked）

---

## 7. RegEx（正则表达式）

Frame: `TFrame_RuleRegEx`，尺寸 400×200

### 输入控件
- `Edit_Expression`（TabOrder=0）：TEdit，标签 `Label4`（"Expression:"，Left=16, Top=27），Left=88, Top=24, Width=265，Anchors=[akTop,akLeft,akRight]，Font.Height=243，OnKeyDown=`AnyControlKeyDown`
- `SB_InsertExpression`：TSpeedButton，Left=354, Top=24, Width=23, Height=25，Anchors=[akTop,akRight]，Flat=True，Hint="Insert expression (Ctrl+Ins)"，OnClick=`SB_InsertExpressionClick`，点击弹出 `PM_InsertExpression`（TPopupMenu，菜单项在运行时动态填充）
- `Edit_Replace`（TabOrder=1）：TEdit，标签 `Label5`（"Replace:"，Left=16, Top=59），Left=88, Top=56, Width=289，Anchors=[akTop,akLeft,akRight]，Font.Height=243，OnKeyDown=`AnyControlKeyDown`
- `Label1`：Left=88, Top=82，提示文字 "Hint: Use $1..$9 to reference subexpressions."

### 选项
- `CheckBox_CaseSensitive`（TabOrder=2）："Case-sensitive"，Left=88, Top=112，默认**未勾选**（注意此处标签带连字符），OnKeyDown=`AnyControlKeyDown`
- `CheckBox_SkipExtension`（TabOrder=3）："Skip extension"，Left=88, Top=136，默认**已勾选**（Checked=True, State=cbChecked），OnKeyDown=`AnyControlKeyDown`

### 帮助
- `SpeedButton_Help`：TSpeedButton，Left=320, Top=109, Width=57, Height=44，Caption="Help"，Hint="Online Help (F1)"，Flat=True，Layout=blGlyphTop，Spacing=2，OnClick=`SpeedButton_HelpClick`

---

## 8. Extension（扩展名）

Frame: `TFrame_RuleExtension`，尺寸 440×272

### 主要功能
- `CheckBox_NewExtension`（TabOrder=0）："New file extension (without the dot):"，Left=40, Top=24，默认**已勾选**（Checked=True, State=cbChecked）
  - `Edit_Extension`（TabOrder=1）：TEdit，Left=56, Top=48, Width=208，Anchors=[akTop,akLeft,akRight]，Font.Height=243
- `CheckBox_ExtensionAppend`（TabOrder=2）："Append to the original filename"，Left=56, Top=80，默认**未勾选**
- `CheckBox_DetectBinSign`（TabOrder=3）："Detect using \"binary signature\""，Left=56, Top=104，默认**未勾选**，OnClick=`CheckBox_DetectBinSignClick`
  - `Label_Hint`：Left=56, Top=136, Width=344, Height=64，AutoSize=False, WordWrap=True，Anchors=[akTop,akLeft,akRight]，"Note: Some files may have multiple extensions matching their data type, for example: doc/ppt/xls have the same signature. Unrecognised files will remain unchanged."

### 选项
- `CheckBox_RemoveDuplicateExtensions`（TabOrder=4）："Remove duplicate extensions"，Left=40, Top=208，默认**未勾选**
- `CheckBox_CaseSensitive`（TabOrder=5）："Case sensitive"，Left=56, Top=232，默认**未勾选**

---

## 9. Padding（填充）

Frame: `TFrame_RulePadding`，尺寸 409×240

### 数字序列填充（`GroupBox_NumberSequences`: TGroupBox，Left=16, Top=8, Width=368, Height=80，ClientWidth=364, ClientHeight=60）
- Caption="Number sequences"，TabOrder=0
- `CheckBox_AddZeroPadding`（GroupBox 内 TabOrder=0）："Add zero padding to length:"，Left=16, Top=8，默认**未勾选**，OnChange=`CheckBox_AddZeroPaddingChange`（与 RemoveZeroPadding 互斥）
  - `SpinEdit_ZeroPaddingLength`（GroupBox 内 TabOrder=1）：TSpinEdit，Left=288, Top=6, Width=58，范围 1 ~ 260，默认值 1，OnChange=`SpinEdit_ZeroPaddingLengthChange`，OnKeyDown=`SpinEditKeyDown`
- `CheckBox_RemoveZeroPadding`（GroupBox 内 TabOrder=2）："Remove zero padding"，Left=16, Top=32，默认**未勾选**，OnChange=`CheckBox_RemoveZeroPaddingChange`（与 AddZeroPadding 互斥）

### 文本填充（`GroupBox_TextPadding`: TGroupBox，Left=16, Top=96, Width=368, Height=113，ClientWidth=364, ClientHeight=93）
- Caption="Text padding"，TabOrder=1
- `CheckBox_AddTextPadding`（GroupBox 内 TabOrder=0）："Add padding to length:"，Left=16, Top=8，默认**未勾选**
  - `SpinEdit_TextPaddingLength`（GroupBox 内 TabOrder=1）：TSpinEdit，Left=288, Top=6, Width=58，范围 1 ~ 260，默认值 1，OnChange=`SpinEdit_TextPaddingLengthChange`，OnKeyDown=`SpinEditKeyDown`
- `Label_PaddingCharacters`：Left=17, Top=40，"Padding characters:"
  - `Edit_PaddingCharacters`（GroupBox 内 TabOrder=2）：TEdit，Left=216, Top=35, Width=130
- `Label_Position`：Left=17, Top=66，"Position:"
  - `RadioButton_PositionLeft`（GroupBox 内 TabOrder=3）："Left"，Left=216, Top=64，**默认选中**（Checked=True, TabStop=True）
  - `RadioButton_PositionRight`（GroupBox 内 TabOrder=4）："Right"，Left=288, Top=64

### 选项
- `CheckBox_SkipExtension`（TabOrder=2）："Skip extension"，Left=34, Top=216，默认**已勾选**（Checked=True, State=cbChecked）

---

## 10. Strip（剥离字符）

Frame: `TFrame_RuleStrip`，尺寸 410×266

### 字符类型选择（CheckBox + 只读 Edit 显示字符集，每组 CheckBox 在左、Edit 在右）
- `CB_StripEnglishLetters`（TabOrder=0）："English:"，Left=32, Top=18，默认**未勾选**
  - `Edit_StripEnglishLetters`（TabOrder=1）：只读 TEdit，Left=160, Top=16, Width=219，Color=clBtnFace, ReadOnly=True，Anchors=[akTop,akLeft,akRight]，Font.Height=243，Text="abcdefghijklmnopqrstuvwxyz"
- `CheckBox_StripDigits`（TabOrder=2）："Digits:"，Left=32, Top=44，默认**未勾选**
  - `Edit_StripDigits`（TabOrder=3）：只读 TEdit，Left=160, Top=42, Width=219，Color=clBtnFace, ReadOnly=True，Anchors=[akTop,akLeft,akRight]，Font.Height=243，Text="1234567890"
- `CheckBox_StripSymbols`（TabOrder=4）："Symbols:"，Left=32, Top=70，默认**未勾选**
  - `Edit_StripSymbols`（TabOrder=5）：只读 TEdit，Left=160, Top=68, Width=219，Color=clBtnFace, ReadOnly=True，Anchors=[akTop,akLeft,akRight]，Font.Height=243，Text="!\"#$%&'*+,-./:;=?@\\^_`|~"
- `CheckBox_StripBrackets`（TabOrder=6）："Brackets:"，Left=32, Top=96，默认**未勾选**
  - `Edit_StripBrackets`（TabOrder=7）：只读 TEdit，Left=160, Top=94, Width=219，Color=clBtnFace, ReadOnly=True，Anchors=[akTop,akLeft,akRight]，Font.Height=243，Text="(){}[]<>"
- `CheckBox_StripUserDefined`（TabOrder=8）："User defined:"，Left=32, Top=122，默认**未勾选**
  - `Edit_StripUserDefined`（TabOrder=9）：**可编辑** TEdit，Left=160, Top=120, Width=219，Anchors=[akTop,akLeft,akRight]，Font.Height=243，OnChange=`Edit_StripUserDefinedChange`
- `CheckBox_StripUnicodeRange`（TabOrder=10）："Unicode range:"，Left=32, Top=148，默认**未勾选**
  - `Edit_StripUnicodeRange`（TabOrder=11）：可编辑 TEdit，Left=160, Top=146, Width=219，Anchors=[akTop,akLeft,akRight]，Font.Height=243，Text="10000-10FFFF"，OnChange=`Edit_StripUnicodeRangeChange`

### 剥离范围（`Panel_Where`: TPanel，Left=16, Top=176, Width=146, Height=80，BevelOuter=bvNone，TabOrder=12）
容纳 3 个 RadioButton：
- `RB_WhereEverywhere`（Panel 内 TabOrder=0）："Everywhere"，Left=16, Top=8，**默认选中**（Checked=True, TabStop=True）
- `RB_WhereLeading`（Panel 内 TabOrder=1）："Leading"，Left=16, Top=32
- `RB_WhereTrailing`（Panel 内 TabOrder=2）："Trailing"，Left=16, Top=56

### 选项
- `CB_StripAllExceptSelected`（TabOrder=13）："Strip all characters except selected"，Left=176, Top=184，默认**未勾选** —— 反选模式：保留选中类型的字符，剥离其余所有字符
- `CheckBox_CaseSensitive`（TabOrder=14）："Case sensitive"，Left=176, Top=208，默认**未勾选**
- `CheckBox_StripSkipExtension`（TabOrder=15）："Skip extension"，Left=176, Top=232，默认**已勾选**（Checked=True, State=cbChecked）

---

## 11. Clean Up（清理）

Frame: `TFrame_RuleCleanUp`，尺寸 440×298

### 括号内容剥离（`GroupBox_Brackets`: TGroupBox，Left=16, Top=8, Width=408, Height=37，ClientWidth=404, ClientHeight=17，TabOrder=0）
- Caption="Strip out content of brackets:"
- `CheckBox_BracketsRound`（GroupBox 内 TabOrder=0）："(...)"，Left=192, Top=251，默认**未勾选**
- `CheckBox_BracketsSquare`（GroupBox 内 TabOrder=1）："[...]"，Left=264, Top=251，默认**未勾选**
- `CheckBox_BracketsCurvy`（GroupBox 内 TabOrder=2）："{...}"，Left=336, Top=251，默认**未勾选**

### 字符替换为空格（`GroupBox1`: TGroupBox，Left=16, Top=48, Width=408, Height=67，ClientWidth=404, ClientHeight=47，TabOrder=1）
- Caption="Replace these characters with spaces:"
- `CheckBox_SpacesDot`（GroupBox 内 TabOrder=0）：". (dot)"，Left=14, Top=1，默认**未勾选**
- `CheckBox_SpacesComma`（GroupBox 内 TabOrder=1）：", (comma)"，Left=96, Top=1，默认**未勾选**
- `CheckBox_SpacesUnderscore`（GroupBox 内 TabOrder=2）："_"，Left=200, Top=1，默认**未勾选**
- `CheckBox_SpacesPlus`（GroupBox 内 TabOrder=3）："+"，Left=248, Top=1，默认**未勾选**
- `CheckBox_SpacesHyphen`（GroupBox 内 TabOrder=4）："-"，Left=296, Top=1，默认**未勾选**
- `CheckBox_SpacesWeb`（GroupBox 内 TabOrder=5）："%20"，Left=344, Top=1，默认**未勾选**
- `CheckBox_SpacesSkipVersions`（GroupBox 内 TabOrder=6）："Skip number sequences, for example version 1.2.3.4"，Left=14, Top=22，默认**未勾选**

### 空格处理
- `CheckBox_SpacesFix`（TabOrder=2）："Fix spaces: only one space at a time, no spaces on sides of basename"，Left=24, Top=128，默认**已勾选**（Checked=True, State=cbChecked）
- `CB_NormalizeSpaces`（TabOrder=3）："Normalize unicode spaces by replacing them with a standard space"，Left=24, Top=151，默认**已勾选**（Checked=True, State=cbChecked）

### Unicode 处理
- `CheckBox_StripUnicodeEmoji`（TabOrder=4）："Strip unicode emoji"，Left=24, Top=176，默认**未勾选**
- `CheckBox_StripUnicodeMarks`（TabOrder=5）："Strip unicode marks (combining, diacritics, accents)"，Left=24, Top=200，默认**未勾选**

### 其他
- `CB_InsertSpaceBeforeCapitals`（TabOrder=6）："Insert a space in front of capitalized letters (e.g. CamelCase)"，Left=24, Top=223，默认**未勾选**
- `CheckBox_PrepareForSharePoint`（TabOrder=7）："Prepare for SharePoint (always inc. extension)"，Left=24, Top=246，默认**未勾选**
- `CheckBox_SkipExtension`（TabOrder=8）："Skip extension"，Left=24, Top=269，默认**已勾选**（Checked=True, State=cbChecked）

---

## 12. Transliterate（音译转写）

Frame: `TFrame_RuleTranslit`，尺寸 440×260

### 字母表
- `BitBtn_Translits`（TabOrder=0）：TBitBtn，Left=24, Top=15, Width=113, Height=25，Anchors=[akTop,akLeft,akRight]，Caption="Alphabet"，带内嵌 Glyph (16×16)，OnClick=`BitBtn_TranslitsClick`，点击弹出 `PopupMenu_Translits`（OnPopup=`PopupMenu_TranslitsPopup`，动态填充预设字母表列表）
- `Memo_Alphabet`（TabOrder=1）：TMemo，Left=24, Top=40, Width=113, Height=202，Anchors=[akTop,akLeft,akRight,akBottom]，Font.Height=243，ScrollBars=ssAutoBoth，WordWrap=False —— 显示和编辑字母表内容（每行一个 "源字符=目标字符" 对）

### 选项面板（`Panel_Options`: TPanel，Left=142, Top=0, Width=298, Height=260，Align=alRight，BevelOuter=bvNone，TabOrder=2）

#### 方向（RadioButton 二选一）
- `Label2`：Left=16, Top=16，"Direction:"
- `RadioButton_DirectionForward`（Panel 内 TabOrder=0）："Forward"，Left=24, Top=40，**默认选中**（Checked=True, TabStop=True）
- `RadioButton_DirectionBackward`（Panel 内 TabOrder=1）："Backward"，Left=24, Top=64

#### 提示
- `Label3`：Left=16, Top=96，"Hint:"
- `Label_HintText`：Left=16, Top=112, Width=264, Height=80，AutoSize=False, WordWrap=True，"Alphabet is a set of couples represented by letters and separated with an equal sign; they stand for translation of non-english characters to their english representation."

#### 选项
- `CheckBox_AdjustCase`（Panel 内 TabOrder=2）："Auto case adjustment"，Left=16, Top=200，默认**已勾选**（Checked=True, State=cbChecked）
- `CheckBox_SkipExtension`（Panel 内 TabOrder=3）："Skip extension"，Left=16, Top=223，默认**已勾选**（Checked=True, State=cbChecked）

---

## 13. Rearrange（重排）

Frame: `TFrame_RuleRearrange`，尺寸 400×233

### 分割模式（RadioButton 三选一）
- 标签 `Label1`（"Split using:"），Left=24, Top=27
- `RB_UsingDelimiters`（TabOrder=0）："Delimiters"，Left=112, Top=8，**默认选中**（Checked=True, TabStop=True）
- `RB_UsingPositions`（TabOrder=1）："Positions"，Left=112, Top=26
- `RB_UsingExactPatternOfDelimiters`（TabOrder=2）："Exact pattern of delimiters"，Left=112, Top=44

### 输入控件
- `Edit_Delimiters`（TabOrder=3）：TEdit，Left=24, Top=72, Width=329, Anchors=[akTop,akLeft,akRight]，Font.Height=243
- `SB_AddSeparator`：TSpeedButton，Left=354, Top=72, Width=23, Height=25，Anchors=[akTop,akRight]，Flat=True，Hint="Separate multiple items"，OnClick=`SB_AddSeparatorClick`
- `Edit_NewPattern`（TabOrder=4）：TEdit，标签 `Label2`（"New pattern:"），Left=24, Top=123, Width=329, Anchors=[akTop,akLeft,akRight]，Font.Height=243，OnKeyDown=`Edit_NewPatternKeyDown`
- `SB_InsertMetaTag`：TSpeedButton，Left=354, Top=123, Width=23, Height=25，Anchors=[akTop,akRight]，Flat=True，Hint="Insert Meta Tag (Ctrl+Ins)"，OnClick=`SB_InsertMetaTagClick`

### 提示
- `Label3`：Left=24, Top=149, Width=353, Height=54, AutoSize=False, WordWrap=True，"Hint: Use $1..$N to reference delimited parts in the new pattern, $-1..$-N to reference from the end, $0 for the original name."

### 选项
- `CB_SkipExtension`（TabOrder=5）："Skip extension"，Left=24, Top=202，默认**已勾选**（Checked=True, State=cbChecked）
- `CB_RightToLeft`（TabOrder=6）："Right-to-left"，Left=200, Top=202，默认**未勾选**

---

## 14. Reformat Date（日期重格式化）

Frame: `TFrame_RuleReformatDate`，尺寸 409×274

### 日期格式
- `Label_InputFormats`：Left=24, Top=16，"Find date/time formats:"
  - `ComboBox_SourceFormats`（TabOrder=0）：TComboBox，Left=24, Top=35, Width=346，Anchors=[akTop,akLeft,akRight]，Font.Height=243，ItemHeight=17，AutoCompleteText=[cbactSearchAscending]
- `Label_OutputFormat`：Left=24, Top=64，"Convert to date/time format:"
  - `ComboBox_TargetFormat`（TabOrder=1）：TComboBox，Left=24, Top=83, Width=346，Anchors=[akTop,akLeft,akRight]，Font.Height=243，ItemHeight=17，AutoCompleteText=[cbactSearchAscending]
- `SpeedButton_Help`：TSpeedButton，Left=371, Top=35, Width=23, Height=24，Anchors=[akTop,akRight]，Flat=True，OnClick=`SpeedButton_HelpClick`，点击弹出 `PopupMenu_Help`：
  - `MenuItem_AddSeparator`：Caption="Separate multiple items"，OnClick=`MenuItem_AddSeparatorClick`
  - ---（`MenuItem_Break1` 分隔线）
  - `MenuItem_DateTimeFormatHelp`：Caption="Date/time format reference (online)"，OnClick=`MenuItem_DateTimeFormatHelpClick`

### 选项
- `CheckBox_WholeWordsOnly`（TabOrder=2）："Match as whole words only"，Left=24, Top=120，默认**已勾选**（Checked=True, State=cbChecked）
- `CheckBox_SkipExtension`（TabOrder=3）："Skip extension"，Left=24, Top=147，默认**已勾选**（Checked=True, State=cbChecked）

### 自定义月份名称
- `CheckBox_UseCustomShortMonths`（TabOrder=4）："Use custom short months:"，Left=24, Top=176，默认**未勾选**，OnClick=`CheckBox_UseCustomShortMonthsClick`
  - `ComboBox_CustomShortMonths`（TabOrder=5）：TComboBox，Left=216, Top=176, Width=176，Anchors=[akTop,akLeft,akRight]，ItemHeight=15，AutoCompleteText=[cbactSearchAscending]
- `CheckBox_UseCustomLongMonths`（TabOrder=6）："Use custom long months:"，Left=24, Top=205，默认**未勾选**，OnClick=`CheckBox_UseCustomLongMonthsClick`
  - `ComboBox_CustomLongMonths`（TabOrder=7）：TComboBox，Left=216, Top=205, Width=176，Anchors=[akTop,akLeft,akRight]，ItemHeight=15

### 时间调整
- `CheckBox_AdjustTime`（TabOrder=8）："Adjust time by:"，Left=24, Top=238，默认**未勾选**，OnClick=`CheckBox_AdjustTimeClick`
  - `SpinEdit_AdjustTimeBy`（TabOrder=9）：TSpinEdit，Left=216, Top=236, Width=58，范围 -1000000 ~ 1000000
  - `ComboBox_AdjustTimePart`（TabOrder=10）：TComboBox，Left=282, Top=236, Width=112，Anchors=[akTop,akLeft,akRight]，Style=csDropDownList，ItemHeight=15

---

## 15. Randomize（随机化）

Frame: `TFrame_RuleRandomize`，尺寸 440×272

### 长度
- `Label_Length`：Left=32, Top=24，"Length of random sequence:"
  - `SpinEdit_Length`（TabOrder=0）：TSpinEdit，Left=224, Top=21, Width=56，范围 1 ~ 260，默认值 1，OnKeyDown=`SpinEditKeyDown`

### 唯一性
- `CheckBox_Unique`（TabOrder=1）："Unique if possible"，Left=32, Top=50，默认**已勾选**（Checked=True, State=cbChecked）

### 字符集（`GroupBox_Characters`: TGroupBox，Left=16, Top=88, Width=185, Height=144，ClientWidth=181, ClientHeight=124，TabOrder=2）
- Caption="Characters to use:"
- `CheckBox_UseDigits`（GroupBox 内 TabOrder=0）："Digits (0..9)"，Left=16, Top=8，默认**已勾选**（Checked=True, State=cbChecked）
- `CheckBox_UseEnglishLetters`（GroupBox 内 TabOrder=1）："English letters (a..z)"，Left=16, Top=32，默认**已勾选**（Checked=True, State=cbChecked）
- `CheckBox_UseUserDefined`（GroupBox 内 TabOrder=2）："User defined:"，Left=16, Top=56，默认**未勾选**
  - `Edit_UserDefined`（GroupBox 内 TabOrder=3）：TEdit，Left=16, Top=80, Width=144，Font.Height=243，OnChange=`Edit_UserDefinedChange`

### 插入位置（`GroupBox_InsertWhere`: TGroupBox，Left=224, Top=88, Width=193, Height=144，ClientWidth=189, ClientHeight=124，TabOrder=3）
- Caption="Insert where:"
- `RadioButton_Prefix`（GroupBox 内 TabOrder=0）："Prefix"，Left=14, Top=6，**默认选中**（Checked=True, TabStop=True）
- `RadioButton_Suffix`（GroupBox 内 TabOrder=1）："Suffix"，Left=14, Top=27
- `RadioButton_Position`（GroupBox 内 TabOrder=2）："Position:"，Left=14, Top=48
  - `SpinEdit_Position`（GroupBox 内 TabOrder=4）：TSpinEdit，Left=120, Top=46, Width=49，范围 1 ~ 260，默认值 1，OnChange=`SpinEdit_PositionChange`，OnKeyDown=`SpinEditKeyDown`
- `RadioButton_ReplaceCurrentName`（GroupBox 内 TabOrder=3）："Replace current name"，Left=14, Top=69
- `CheckBox_SkipExtension`（GroupBox 内 TabOrder=5）："Skip extension"，Left=14, Top=96，默认**已勾选**（Checked=True, State=cbChecked）

---

## 16. PascalScript（Pascal 脚本）

Frame: `TFrame_RulePascalScript`，尺寸 395×195

### 代码编辑器
- `SynEdit_Script`：TSynEdit（内联组件，TabOrder=0），Align=alClient，BorderSpacing: Left=8, Top=8, Right=8
- 字体：Courier New，Font.Height=243, Pitch=fpFixed, Quality=fqNonAntialiased
- OnKeyDown=`AnyControlKeyDown`，OnMouseWheel=`SynEdit_ScriptMouseWheel`

#### 语法高亮：`SynPasSyn_Script`（TSynPasSyn）
- **Enabled=False**（初始禁用，运行时启用）
- CompilerMode=pcmDelphi，NestedComments=False，TypeHelpers=True
- CommentAttri: Foreground=clGreen, Style=[fsBold]（绿色加粗）
- NumberAttri: Foreground=clFuchsia（品红）
- StringAttri: Foreground=clBlue（蓝色）
- SymbolAttri: Foreground=clRed（红色）
- IDEDirectiveAttri: Foreground=clRed
- DirectiveAttri: Foreground=clRed

#### 自动补全：`SynCompletion_Script`（TSynCompletion）
- Width=400，CaseSensitive=False，LinesInWindow=16
- ShortCut=16416(**Ctrl+Space**)，EndOfTokenChr='()[].'  
- AutoUseSingleIdent=True，ShowSizeDrag=True，ToggleReplaceWhole=False
- OnExecute=`SynCompletion_ScriptExecute`，OnPaintItem=`SynCompletion_ScriptPaintItem`
- Editor=SynEdit_Script

#### SynEdit 配置详细
- Options: eoAutoIndent, eoBracketHighlight, eoGroupUndo, eoScrollPastEol, eoTabIndent, eoTabsToSpaces, eoTrimTrailingSpaces
- TabWidth=2，ScrollBars=ssAutoBoth，MaxLeftChar=1
- VisibleSpecialChars=[vscSpace, vscTabAtLast]（显示空格和行末 Tab）
- BracketHighlightStyle=sbhsBoth（匹配括号高亮显示两侧）
- BracketMatchColor: Background=clNone, Foreground=clNone, Style=[fsBold]（加粗显示）
- FoldedCodeColor: Background=clNone, Foreground=clGray, FrameColor=clGray
- MouseLinkColor: Background=clNone, Foreground=clBlue
- LineHighlightColor: Background=clNone, Foreground=clNone
- SelectedColor: BackPriority=50, ForePriority=50, FramePriority=50 等

#### Gutter（左侧边栏，总宽 57px）
`SynLeftGutterPartList1` 包含 5 个部分（从左到右）：
1. `SynGutterMarks1`：TSynGutterMarks，Width=24 —— 书签/断点标记区
2. `SynGutterLineNumber1`：TSynGutterLineNumber，Width=17，DigitCount=2，ZeroStart=False，LeadingZeros=False，MarkupInfo.Background=clBtnFace —— 行号
3. `SynGutterChanges1`：TSynGutterChanges，Width=4，ModifiedColor=59900（黄色），SavedColor=clGreen —— 变更标记
4. `SynGutterSeparator1`：TSynGutterSeparator，Width=2，MarkupInfo: Background=clWhite, Foreground=clGray —— 分隔线
5. `SynGutterCodeFolding1`：TSynGutterCodeFolding，MarkupInfo: Background=clNone, Foreground=clGray —— 代码折叠

#### 快捷键映射（完整列表，60+ 项）
主要快捷键分组：
- **导航**：Up/Down/Left/Right，Home/End，PgUp/PgDn，Ctrl+Home/End（文档头/尾），Ctrl+Left/Right（按词移动）
- **选择**：Shift+上述导航键，Ctrl+A（全选）
- **编辑**：Ctrl+C/V/X/Z（复制/粘贴/剪切/撤销），Ctrl+Shift+Z（重做），Ctrl+Y（删除行），Ctrl+T（删除词），Del，Backspace
- **缩进**：Tab，Shift+Tab，Ctrl+Shift+I（块缩进），Ctrl+Shift+U（块取消缩进）
- **书签**：Ctrl+0..9（跳转书签），Ctrl+Shift+0..9（设置书签）
- **折叠**：Ctrl+Shift+1..9（折叠层级），Ctrl+Shift+Minus（折叠当前），Ctrl+Shift+Plus（展开当前）
- **列选择**：Ctrl+Shift+Alt+方向键
- **其他**：Insert（切换插入/覆盖模式），Ctrl+Shift+B（匹配括号），Alt+M（切换标记词）

### 工具栏（`ToolBar_Actions`，Align=alBottom，AutoSize=True，Height=22）
Images=`ImageList_Icons`（Frame 内部专用 ImageList），ShowCaptions=True，List=True，EdgeBorders=[]
- `ToolButton_Compile`（Left=2）：Caption="Try to Compile"，Hint="Try to Compile (Ctrl+T)"，ImageIndex=0，OnClick=`ToolButton_CompileClick`
- `ToolButton_GoToLine`（Left=109）：Caption="Go To"，Hint="Go To Line (Ctrl+G)"，ImageIndex=1，OnClick=`ToolButton_GoToLineClick`
- `ToolButton_Scripts`（Left=169）：Caption="Scripts"，Hint="Scripts (Ctrl+L)"，ImageIndex=2，OnClick=`ToolButton_ScriptsClick` —— 弹出 `PM_Scripts`（OnPopup=`PM_ScriptsPopup`，动态填充预设脚本列表）
- `ToolButton_Help`（Left=233）：Caption="Help"，Hint="Online Help (F1)"，ImageIndex=3，OnClick=`ToolButton_HelpClick`

---

## 17. User Input（用户输入）

Frame: `TFrame_RuleUserInput`，尺寸 440×240

### 输入
- `Label1`：说明文字，Left=16, Top=11，"Type your new filenames here (one per line):"
- `Memo_Input`（TabOrder=0）：TMemo，Left=16, Top=32, Width=409, Height=161，Anchors=[akTop,akLeft,akRight,akBottom]，Font.Height=243，ScrollBars=ssAutoBoth，WordWrap=False，OnKeyDown=`Memo_InputKeyDown`
- `SB_Options`：TSpeedButton，Left=336, Top=9, Width=89, Height=20，Anchors=[akTop,akRight]，Caption="Options"，Flat=True，带内嵌 Glyph (17×15)，OnClick=`SB_OptionsClick`，点击弹出 `PopupMenu_Options`：
  - `MenuItem_Load`：Caption="Load from Text File"，OnClick=`MenuItem_LoadClick`
  - ---（N1 分隔线）
  - `MenuItem_Paste`：Caption="Paste from Clipboard"，OnClick=`MenuItem_PasteClick`

### 插入模式（RadioButton 三选一）
- `RB_InsertInFront`（TabOrder=1）："Insert in front of the current name"，Left=16, Top=197，Anchors=[akLeft,akBottom]，默认**未选中**
- `RB_InsertAfter`（TabOrder=2）："Insert after the current name"，Left=16, Top=216，Anchors=[akLeft,akBottom]，默认**未选中**
- `RB_Replace`（TabOrder=3）："Replace the current name"，Left=240, Top=197，Anchors=[akLeft,akBottom]，**默认选中**（Checked=True, TabStop=True）

### 选项
- `CB_SkipExtension`（TabOrder=4）："Skip extension"，Left=240, Top=216，Anchors=[akLeft,akBottom]，默认**已勾选**（Checked=True, State=cbChecked）

---

## 18. Mapping（映射表）

Frame: `TFrame_RuleMapping`，尺寸 466×294

### 映射表操作
- `BitBtn_TableMenu`（TabOrder=0）：TBitBtn，Left=8, Top=8, Width=123, Height=30，Caption="Table"，带内嵌 Glyph (17×17)，OnClick=`BitBtn_TableMenuClick`，点击弹出 `PopupMenu_Options`：
  - `MenuItem_LoadFile`：Caption="Load from file..."，OnClick=`MenuItem_LoadFileClick`
  - `MenuItem_LoadClipboard`：Caption="Load from clipboard"，OnClick=`MenuItem_LoadClipboardClick`
  - ---（`Separator2` 分隔线）
  - `MenuItem_SaveFile`：Caption="Save to file..."，OnClick=`MenuItem_SaveFileClick`
  - `MenuItem_SaveClipboard`：Caption="Save to clipboard"，OnClick=`MenuItem_SaveClipboardClick`
  - ---（`Separator1` 分隔线）
  - `MenuItem_Clear`：Caption="Clear"，OnClick=`MenuItem_ClearClick`

### 映射数据
- `StringGrid_Mapping`（TabOrder=6）：TStringGrid，Left=0, Top=70, Width=466, Align=alClient，BorderSpacing.Top=70，配置：
  - ColCount=2，FixedCols=0，AutoFillColumns=True，Flat=True
  - 列标题：`Match`（Width=233） | `New Name`（Width=232）
  - Options: [goFixedVertLine, goFixedHorzLine, goVertLine, goHorzLine, goRangeSelect, goEditing, goAutoAddRows, goAlwaysShowEditor, goSmoothScroll]
  - 初始 RowCount=2（1行标题 + 1行数据）

### 选项
- `CheckBox_AllowReuse`（TabOrder=1）："Allow reuse"，Left=152, Top=6，默认**未勾选**，OnChange=**`CheckBox_InverseMappingChange`**（注意：与 InverseMapping 共享同一事件处理器）
- `CheckBox_PartialMatch`（TabOrder=2）："Partial match"，Left=152, Top=25，默认**未勾选**
- `CheckBox_InverseMapping`（TabOrder=3）："Inverse mapping"，Left=152, Top=44，默认**未勾选**，OnChange=`CheckBox_InverseMappingChange`
- `CheckBox_CaseSensitive`（TabOrder=4）："Case sensitive"，Left=320, Top=6，默认**未勾选**
- `CheckBox_SkipExtension`（TabOrder=5）："Skip extension"，Left=320, Top=25，默认**未勾选**（注意：此规则默认不跳过扩展名）

---

## 通用设计模式

1. **Skip extension**：几乎所有规则都有此选项，大多数默认已勾选。操作仅作用于不含扩展名的文件名部分。
2. **Meta Tag 支持**：Replace、Insert、Rearrange 等规则支持插入 Meta Tag（文件属性占位符），通过 SpeedButton 触发。
3. **分隔符按钮**：Replace、Remove、Rearrange、Reformat Date 等支持多值输入，通过 SpeedButton 插入分隔符将单个输入框拆分为多个值。
4. **插入位置模式**：Insert、Serialize、Randomize 共享相同的位置选择模式（Prefix/Suffix/Position/Replace current name）。
5. **OnChange/OnClick 联动**：许多 CheckBox 和 RadioButton 有事件处理器，用于在选项间实现互斥或启用/禁用关联控件。
6. **SpinEdit 的 OnKeyDown**：所有 SpinEdit 都响应 `SpinEditKeyDown`，可能用于统一处理上下键步进或 Enter 确认。

---

# 主界面（Form_Main）功能需求

基于反编译 Form_Main.dfm 资源的详细分析。

### 核心概念说明

- **标记（Mark）vs 选择（Select）**：这是两个独立的概念。“标记”是通过复选框（CheckBox）勾选文件，决定哪些文件参与重命名操作；“选择”是用鼠标/键盘高亮行，用于批量操作（删除、移动等）的目标。两者可独立存在：一个文件可以被标记但未选中，或被选中但未标记。
- **状态（State）**：文件列表第 0 列显示文件的当前状态图标，可能的状态包括：未处理（无图标）、预览有效（绿色✓）、预览无效/冲突（红色×）、重命名成功（绿色✓）、重命名失败（红色×）。图标来自 `ImageListSmall`。
- **清除（Clear）**：指从文件列表中移除条目（不删除磁盘文件），与 "Remove Selected" 类似但按条件过滤。
- **预览（Preview）**：应用规则计算新文件名并显示在 "New Name" 列中，不实际重命名。
- **重命名（Rename）**：实际执行文件系统重命名操作，仅对已标记的文件生效。

---

## 窗口属性

- 类名：`TForm_Main`
- 初始尺寸：Width=600, Height=470（ClientWidth=600, ClientHeight=470）
- 初始位置：Left=510, Top=202（被 poScreenCenter 覆盖）
- Caption："ReNamer"
- Position：`poScreenCenter`（屏幕居中显示）
- KeyPreview：True（窗体优先截获键盘事件，用于全局快捷键如 F3/F5/F6 等）
- AllowDropFiles：True（支持从资源管理器拖放文件到窗口，触发 `FormDropFiles` 事件将文件添加到列表）
- Menu：绑定 `MainMenu`
- LCLVersion：'3.8.0.0'

### 窗口事件
- `OnCreate` → `FormCreate`：初始化应用状态、加载配置文件、恢复上次窗口位置/大小、加载上次的规则预设、填充语言菜单、注册文件关联等
- `OnShow` → `FormShow`：窗口首次显示后的处理（如检查命令行参数、自动添加拖放到快捷方式的文件等）
- `OnResize` → `FormResize`：窗口大小变化时重新计算并调整内部控件布局
- `OnDropFiles` → `FormDropFiles`：接收拖放进来的文件路径列表，添加到文件列表并触发预览
- `OnClose` → `FormClose`：关闭时保存窗口位置/大小、列宽、分隔条位置、当前规则等状态到配置文件
- `OnDestroy` → `FormDestroy`：释放所有动态创建的对象（规则列表、文件列表、图标缓存等）

---

## 整体布局

从上到下依次为：
1. **ToolBar_Main**（Align=alTop, AutoSize=True, Height=38px）：主操作工具栏
2. **Panel_Main**（Align=alClient, Top=40, Height=407, BevelOuter=bvNone, BorderSpacing.Top=2）：主面板，与工具栏间有 2px 间距，包含：
   - **Panel_Rules**（Align=alTop, Height=145）：规则列表面板
   - **Splitter_Middle**（Align=alTop, Height=5）：水平拖动分隔条
   - **Panel_Files**（Align=alClient, Height=257）：文件列表面板
3. **StatusBar**（Align=alBottom, Height=23）：状态栏

---

## 主工具栏（ToolBar_Main）

- 属性：AutoSize=True, ButtonHeight=36, ButtonWidth=23, Images=`ImageListBig`（32×32）, ShowCaptions=True, List=True（图标在左文字在右）, EdgeBorders=[ebBottom]（仅底部有边框线）

按钮从左到右排列（Left 值）：
1. `ToolButton_AddFiles`（Left=1）：Caption="Add Files"，Action=`Action_AddFiles`，快捷键 **F3**（ShortCut=114），ImageIndex=3 —— 打开文件选择对话框，支持多选，将选中文件添加到列表
2. `ToolButton_AddFolders`（Left=94）：Caption="Add Folders"，Action=`Action_AddFolders`，快捷键 **F4**（ShortCut=115），ImageIndex=4 —— 打开文件夹选择对话框，递归添加文件夹内所有文件（受 Filters 过滤）
3. `Image_Arrow1`（Left=202, TImage）：14×13px PNG 箭头图，外框 24×36，Center=True，纯装饰作用，无事件
4. `ToolButton_Preview`（Left=226）：Caption="Preview"，Action=`Action_Preview`，快捷键 **F5**（ShortCut=116），ImageIndex=0 —— 对所有已标记文件应用规则计算新名，显示在 "New Name" 列中，并更新 State 列状态
5. `Image_Arrow2`（Left=312, TImage）：同 Image_Arrow1，装饰性箭头
6. `ToolButton_Rename`（Left=336）：Caption="Rename"，Action=`Action_Rename`，快捷键 **F6**（ShortCut=117），ImageIndex=1 —— 对所有已标记且预览有效的文件执行实际重命名，完成后更新 State 列为成功/失败

**工作流程示意**：`[Add Files]` → `[→]` → `[Preview]` → `[→]` → `[Rename]`，箭头图片起到可视化流程引导作用。

---

## 规则面板（Panel_Rules）

BevelOuter=bvNone，初始高度 145px。

### 规则工具栏（ToolBar_Rules）

使用 `ImageListToolbarRules`（13×13 图标），ShowCaptions=True，List=True，ButtonHeight=20，EdgeBorders=[]。

按钮：
1. `ToolButton_AddRule`：Caption="Add"，ImageIndex=0，点击事件 `ToolButton_AddRuleClick` —— 打开添加规则对话框
2. `ToolButton_RemoveRule`：Caption="Remove"，ImageIndex=1，点击事件 `ToolButton_RemoveRuleClick` —— 删除选中的规则
3. `ToolButton_MoveUpRule`：Caption="Up"，ImageIndex=2，点击事件 `ToolButton_MoveUpRuleClick` —— 上移规则
4. `ToolButton_MoveDownRule`：Caption="Down"，ImageIndex=3，点击事件 `ToolButton_MoveDownRuleClick` —— 下移规则

### 规则列表（ListView_Rules）

- 类型：TListView，Align=alClient，ViewStyle=vsReport
- **Checkboxes=True**：每条规则前有复选框，勾选表示规则启用，取消勾选表示禁用
- MultiSelect=True：支持多选
- ReadOnly=True：不可直接编辑文本
- RowSelect=True：整行选中
- PopupMenu：`PM_Rules`

#### 列定义
- `#`：序号列，MinWidth=50
- `Rule`：规则类型名称，MinWidth=100, Width=100
- `Statement`：规则描述/参数摘要，MinWidth=100, Width=300

#### 事件
- `OnDblClick` → `ListView_RulesDblClick`：双击编辑规则
- `OnDragDrop` / `OnDragOver`：支持拖放排序规则
- `OnItemChecked` → `ListView_RulesItemChecked`：勾选/取消规则时触发预览更新
- `OnMouseDown` → `ListView_RulesMouseDown`：鼠标按下（可能用于启动拖放）
- `OnResize` → `ListView_RulesResize`：调整列宽
- `OnSelectItem` → `ListView_RulesSelectItem`：选中规则时更新工具栏状态

---

## 分隔条（Splitter_Middle）

- 类型：TSplitter，Cursor=crVSplit（垂直拖动光标）
- Align=alTop，Height=5
- AutoSnap=False
- MinSize=100：规则面板和文件面板最小高度 100px
- ResizeAnchor=akTop
- `OnMoved` → `Splitter_MiddleMoved`：拖动结束后保存面板比例

---

## 文件面板（Panel_Files）

BevelOuter=bvNone，Align=alClient。

### 文件工具栏（ToolBar_Files）

使用 `ImageListToolbarFiles`（12×12 图标），ShowCaptions=True，List=True，ButtonHeight=18，EdgeBorders=[]。

按钮（每个都是下拉菜单触发器）：
1. `ToolButton_Files`：Caption="Files"，ImageIndex=6，点击 `ToolButton_FilesClick` —— 弹出文件操作菜单（添加/清除等）
2. `ToolButton_Columns`：Caption="Columns"，ImageIndex=6，点击 `ToolButton_ColumnsClick` —— 弹出列显示/隐藏菜单
3. `ToolButton_Options`：Caption="Options"，ImageIndex=2，点击 `ToolButton_OptionsClick` —— 弹出 `PM_Options` 菜单
4. `ToolButton_Export`：Caption="Export"，ImageIndex=5，点击 `ToolButton_ExportClick` —— 弹出 `PM_Export` 菜单
5. `ToolButton_Filters`：Caption="Filters"，ImageIndex=3，点击 `ToolButton_FiltersClick` —— 打开过滤器设置
6. `ToolButton_Analyze`：Caption="Analyze"，ImageIndex=7，点击 `ToolButton_AnalyzeClick` —— 分析文件名

### 文件列表（VST_Files）

- 类型：`TLazVirtualStringTree`（虚拟字符串树，高性能列表控件）
- Align=alClient
- DragMode=dmAutomatic，DragOperations=[doMove]，DragType=dtVCL
- Images：`ImageListSmall`（16×16 图标）
- PopupMenu：`PM_Files`
- Header.PopupMenu：`PM_FilesColumns`（右键列标题弹出列管理菜单）
- Header.Height=24，Header.Options 包含 hoColumnResize, hoDblClickResize, hoDrag, hoHotTrack, hoShowSortGlyphs, hoVisible

#### Header.Options 详解
- `hoColumnResize`：允许用户拖拽列边框调整列宽
- `hoDblClickResize`：双击列边框自动调整为内容最佳宽度（触发 `OnColumnWidthDblClickResize`）
- `hoDrag`：允许拖拽列标题重新排列列顺序
- `hoHotTrack`：鼠标悬停在列标题上时高亮显示
- `hoShowSortGlyphs`：在当前排序列的标题上显示升序/降序箭头图标
- `hoVisible`：显示列标题行

#### TreeOptions 详解

**AutoOptions**：
- `toAutoDropExpand`：拖放时自动展开目标节点（本项目无树形结构，实际不触发）
- `toAutoScroll`：拖放时鼠标接近边缘自动滚动列表
- `toAutoScrollOnExpand`：展开节点时自动滚动以确保子节点可见
- `toAutoTristateTracking`：复选框支持三态跟踪（父节点自动反映子节点状态）
- `toAutoDeleteMovedNodes`：拖动节点后自动删除原位置的节点
- `toAutoChangeScale`：DPI 变化时自动调整缩放

**MiscOptions**：
- `toCheckSupport`：**启用复选框**，每行前显示复选框用于标记文件是否参与重命名
- `toFullRepaintOnResize`：窗口大小变化时完全重绘（而非仅重绘新增区域）
- `toInitOnSave`：保存时初始化未初始化的节点
- `toToggleOnDblClick`：双击切换复选框状态（双击文件行时切换其标记状态）
- `toWheelPanning`：支持鼠标滚轮平滑滚动
- `toFullRowDrag`：在整行任意位置开始拖拽（而非仅在图标/文本上）
- `toEditOnClick`：单击已选中的单元格时进入编辑模式（用于行内编辑 "New Name" 列）

**PaintOptions**：
- `toHideFocusRect`：隐藏焦点矩形虚线框
- `toShowButtons`：显示展开/折叠按钮（本项目无树形结构，实际不可见）
- `toShowDropmark`：拖放时显示插入位置指示线
- `toShowHorzGridLines`：显示水平网格线（行之间的分隔线）
- `toShowVertGridLines`：显示垂直网格线（列之间的分隔线）
- `toThemeAware`：使用系统主题绘制（Windows 视觉样式）
- `toUseBlendedImages`：图标使用半透明混合效果（禁用的节点图标变淡）

**SelectionOptions**：
- `toExtendedFocus`：焦点可在列间移动（按左右箭头切换焦点列）
- `toFullRowSelect`：点击任意列选中整行（而非仅选中单个单元格）
- `toMultiSelect`：支持 Ctrl+点击多选和 Shift+点击范围选
- `toSimpleDrawSelection`：使用简单的矩形绘制选中高亮（而非复杂的渐变效果）

#### 21 列定义

默认可见列（Position 对应显示顺序）：
- 0: `State`（状态，Width=150）—— 显示重命名状态图标（成功/失败/未处理等）
- 3: `Name`（文件名，Width=150）
- 4: `New Name`（新文件名，Width=150）
- 20: `Error`（错误信息）

默认隐藏列（Options 不含 coVisible）：
- 1: `Path`（完整路径）
- 2: `Folder`（所在文件夹）
- 5: `New Path`（新完整路径）
- 6: `Size`（文件大小，右对齐）
- 7: `Size KB`（KB 大小，右对齐）
- 8: `Size MB`（MB 大小，右对齐）
- 9: `Created`（创建时间）
- 10: `Modified`（修改时间）
- 11: `Extension`（扩展名）
- 12: `Name Digits`（文件名中的数字，右对齐）
- 13: `Path Digits`（路径中的数字）
- 14: `Name Length`（文件名长度，右对齐）
- 15: `New Name Length`（新文件名长度，右对齐）
- 16: `Path Length`（路径长度，右对齐）
- 17: `New Path Length`（新路径长度，右对齐）
- 18: `Exif Date`（EXIF 日期）
- 19: `Old Path`（旧路径，用于撤销）

#### 事件与工作流

##### 虚拟模式机制（数据获取）
VST_Files 使用虚拟模式（Virtual Mode），不在控件内部存储数据，所有显示内容通过事件回调按需获取：
- `OnGetNodeDataSize`：树控件初始化时调用，返回每个节点附加数据结构的字节大小（即 `SizeOf(TFileData)` 或类似结构体），控件据此分配内存
- `OnGetText` → `VST_FilesGetText`：控件需要显示某单元格时调用，参数包含 Node（节点）和 Column（列索引），根据 Column 值返回对应文本：
  - Column=0 (State)：返回状态描述文本（"Ready"/"Renamed"/"Error" 等）
  - Column=3 (Name)：返回原始文件名（从节点数据的文件路径中提取）
  - Column=4 (New Name)：返回规则计算后的新文件名
  - Column=6/7/8 (Size/KB/MB)：返回格式化的文件大小字符串
  - Column=9/10 (Created/Modified)：返回格式化的日期时间字符串
  - 其他列类推，均从节点附加数据中读取
- `OnGetImageIndex`：返回节点前方的图标索引（来自 ImageListSmall），通常根据文件状态返回不同图标（如绿色勾=成功，红色叉=失败，空白=未处理）
- `OnPaintText` → `VST_FilesPaintText`：自定义文本绘制样式，在文本渲染前调用：
  - 根据重命名状态改变字体颜色（如成功=绿色，失败=红色，冲突=橙色）
  - 对 "New Name" 列，若与原名相同则可能显示为灰色
  - 对已标记（Checked）和未标记的节点可能使用不同样式
- `OnFreeNode`：节点被销毁时调用，释放节点附加数据中动态分配的内存（如字符串）

##### 排序机制
- `OnHeaderClick` → `VST_FilesHeaderClick`：用户点击列标题时触发排序流程：
  1. 记录点击的列索引（Column）
  2. 若点击的是当前排序列，切换排序方向（升序↔降序）；否则设为新的排序列，默认升序
  3. 更新 Header.SortColumn 和 Header.SortDirection，显示排序箭头图标（由 hoShowSortGlyphs 控制）
  4. 调用 VST_Files.SortTree 触发实际排序
- `OnCompareNodes` → `VST_FilesCompareNodes`：排序过程中被反复调用的比较函数：
  - 接收两个节点 Node1、Node2 和当前排序列 Column
  - 根据 Column 类型选择比较方式：文本列用字符串比较，数值列用数值比较，日期列用日期比较
  - 返回 <0（Node1 排前面）、0（相等）、>0（Node2 排前面）
- 取消排序：通过 PM_FilesColumns 的 "Cancel Sorting" 菜单项恢复文件的原始添加顺序

##### 行内编辑流程（编辑 New Name）
用户可直接在文件列表中编辑 "New Name" 列，流程如下：
1. **触发编辑**：用户按 F2 或单击已选中的 "New Name" 单元格（由 toEditOnClick 选项控制）
2. `OnEditing`：控件询问是否允许编辑，返回 True 仅当焦点列是 "New Name" 列（Column=4），其他列返回 False 拒绝编辑
3. `OnCreateEditor`：创建行内编辑器（TEdit 控件），设置初始文本为当前 New Name 值，调整编辑器大小与单元格对齐
4. 用户在编辑器中修改文本，按 **Enter** 确认或按 **Esc** 取消
5. `OnNewText`：用户按 Enter 确认时调用，接收新输入的文本，将其存储到节点数据中作为手动覆盖的新名（此值优先于规则计算结果）
6. `OnEditCancelled`：用户按 Esc 取消时调用，丢弃编辑内容，恢复原始显示

##### 拖放文件排序
- DragMode=dmAutomatic：开始拖拽时自动进入拖拽模式
- DragOperations=[doMove]：仅支持移动操作（不支持复制）
- DragType=dtVCL：使用 VCL 内部拖放机制
- `OnDragOver`：拖拽过程中持续调用，判断当前鼠标位置是否为合法的放置目标（允许在节点之间放置，由 toShowDropmark 显示插入位置指示线）
- `OnDragDrop`：释放鼠标完成拖放，将拖拽的节点移动到目标位置（调整文件在列表中的顺序），对 Serialize 规则等依赖顺序的场景有意义
- toFullRowDrag：在整行任意位置都可以开始拖拽
- toAutoDeleteMovedNodes：拖动后自动删除原位置节点（配合插入新位置实现移动效果）
- toAutoScroll：拖拽到边缘时自动滚动列表

##### 选中与状态变化
- `OnChange` → `VST_FilesChange`：选中项变化时触发（包括键盘导航和鼠标点击），用于更新状态栏文件统计信息、启用/禁用相关菜单项等
- `OnDblClick` → `VST_FilesDblClick`：双击文件行时触发，若启用了 toToggleOnDblClick 则切换复选框状态；也可能执行打开文件操作

##### 键盘处理
- `OnKeyDown`：按键按下时调用，处理：
  - Del 键：移除选中文件（调用 MenuItem_RemoveSelectedItemsClick）
  - Ctrl+Del：清除所有文件
  - Ctrl+A：全选
  - Ctrl+Up/Down：移动选中文件
  - F2：进入行内编辑模式
  - Enter/Shift+Enter/Ctrl+Enter/Alt+Enter：各种打开文件操作
  - 其他快捷键映射到对应的 Action 或菜单项
- `OnKeyUp`：按键释放时调用，用于清理按键状态

##### 其他事件
- `OnColumnWidthDblClickResize`：双击列边框时自动调整该列为最佳宽度（遍历所有可见节点计算最大文本宽度）
- `OnResize`：控件大小变化时调用，可能重新分配列宽或调整布局
- `OnUpdating`：批量操作（如添加大量文件）开始/结束时调用，用于控制 BeginUpdate/EndUpdate 以提高性能

---

## 状态栏（StatusBar）

- 类型：TStatusBar，Height=23
- SimplePanel=False（使用多面板模式）
- ShowHint=True

### 面板定义

**Panel[0]：进度条面板**
- Width=166，Style=**psOwnerDraw**
- 空闲状态：显示空白或应用程序版本信息
- 预览/重命名过程中：通过 `StatusBarDrawPanel` 自绘为进度条，填充矩形按当前完成百分比绘制（使用 Canvas.FillRect），并在矩形上居中绘制百分比文本（如 "Processing: 45%"）
- 自绘细节：左侧填充色为进度色（通常为蓝色/绿色），右侧为背景色，双色分割点按 (Panel宽度 × 完成率) 计算

**Panel[1]：文件统计面板**
- Width=200，Style=psText（标准文本）
- 显示当前文件列表的统计信息，格式示例："Files: 150 | Marked: 120 | Selected: 5"
- 在 `VST_FilesChange` 和文件添加/移除操作后更新

**Panel[2]：辅助信息面板**
- Width=50，Style=psText
- 显示当前操作状态或辅助提示（如当前排序列名、视图模式等）

### 事件
- `OnClick` → `StatusBarClick`：点击状态栏，可能根据点击的面板索引执行不同操作（如点击进度条取消操作）
- `OnDrawPanel` → `StatusBarDrawPanel`：仅对 Style=psOwnerDraw 的 Panel[0] 触发，参数提供 Panel 索引和绘制区域 Rect，在此实现进度条绘制逻辑
- `OnMouseMove` → `StatusBarMouseMove`：鼠标移动时根据当前所在面板更新 Hint 提示文本（如悬停在进度条上显示详细进度，悬停在统计面板上显示完整信息）

---

## 主菜单（MainMenu）

### File 菜单（`MenuItem_File`）
- `MI_NewProject` → Action=`Action_NewProject`：Caption="New Project"（Ctrl+N）—— 新建项目，清空所有规则和文件
- `MI_NewInstance` → Action=`Action_NewInstance`：Caption="New Instance"（Ctrl+Shift+N）—— 打开新的 ReNamer 实例
- ---（N22 分隔线）
- `MenuItem_Undo` → Action=`Action_UndoRename`：Caption="Undo Renaming"（Ctrl+Shift+Z）—— 撤销上一次重命名操作
- `MenuItem_PasteFiles` → Action=`Action_PasteFiles`：Caption="Paste Files"（Ctrl+Shift+V）—— 从剪贴板粘贴文件路径
- `MenuItem_AddPaths` → Action=`Action_AddPaths`：Caption="Add Paths" —— 手动输入文件路径添加
- ---（N6 分隔线）
- `MenuItem_AddFiles` → Action=`Action_AddFiles`：Caption="Add Files"（F3）—— 打开文件选择对话框
- `MenuItem_AddFolders` → Action=`Action_AddFolders`：Caption="Add Folders"（F4）—— 打开文件夹选择对话框
- `MenuItem_Preview` → Action=`Action_Preview`：Caption="Preview"（F5）—— 预览重命名结果
- `MenuItem_Rename` → Action=`Action_Rename`：Caption="Rename"（F6）—— 执行重命名
- ---（N10 分隔线）
- `MenuItem_Exit`：Caption="Exit"，ShortCut=32883(**Alt+F4**)，OnClick=`MenuItem_ExitClick` —— 退出程序（无 Action 绑定，直接 OnClick）

### Settings 菜单（`MenuItem_Settings`）
- `MenuItem_SettingsShow` → Action=`Action_Settings`：Caption="All Settings"（F8）—— 打开完整设置对话框
- ---（N12 分隔线）
- `MenuItem_SettingsGeneral`：Caption="General"，OnClick=`MenuItem_SettingsGeneralClick` —— 直接打开常规设置页
- `MenuItem_SettingsPreview`：Caption="Preview"，OnClick=`MenuItem_SettingsPreviewClick` —— 直接打开预览设置页
- `MenuItem_SettingsRename`：Caption="Rename"，OnClick=`MenuItem_SettingsRenameClick` —— 直接打开重命名设置页
- `MenuItem_SettingsMetaTags`：Caption="Meta Tags"，OnClick=`MenuItem_SettingsMetaTagsClick` —— 直接打开 Meta Tags 设置页
- `MenuItem_SettingsMisc`：Caption="Miscellaneous"，OnClick=`MenuItem_SettingsMiscClick` —— 直接打开杂项设置页
- ---（N17 分隔线）
- `MenuItem_Filters` → Action=`Action_Filters`：Caption="Filters"（Ctrl+F）—— 打开过滤器设置
- ---（N24 分隔线）
- `MenuItem_ToggleViewMode` → Action=`Action_ToggleViewMode`：Caption="Toggle view mode" —— 切换视图模式

### Presets 菜单（`MenuItem_Presets`）
- `MI_LoadPreset`：Caption="Load"，Enabled=False（初始禁用）—— 加载预设（运行时动态填充子菜单，无 Action/OnClick）
- `MI_Save` → Action=`Action_PresetSave`：Caption="Save"（Ctrl+Shift+S）—— 保存当前预设
- `MI_SavePreset` → Action=`Action_PresetSaveAs`：Caption="Save As"（Ctrl+S）—— 另存为新预设
- ---（N9 分隔线）
- `MI_ManagePresets` → Action=`Action_ManagePresets`：Caption="Manage..."（Ctrl+P）—— 打开预设管理器
- `MI_BrowsePresets`：Caption="Browse..."，Enabled=False，OnClick=`MI_BrowsePresetsClick` —— 浏览预设文件夹（初始禁用，无 Action）
- `MI_AddPresets`：Caption="Import..."，OnClick=`MI_AddPresetsClick` —— 导入预设文件
- ---（N7 分隔线）
- `MI_CreatePresetsLinks`：Caption="Create Links"，OnClick=`MI_CreatePresetsLinksClick` —— 为预设创建快捷方式
- `MI_RescanPresets`：Caption="Rescan"，OnClick=`MI_RescanPresetsClick` —— 重新扫描预设目录

### Language 菜单（`MI_Language`）
- SubMenuImages=`ImageListLanguages`（国旗图标）
- 菜单项在运行时动态填充，每个语言一个条目，点击后切换界面语言并重新加载翻译文本

### Help 菜单（`MenuItem_Help`）
- `MI_Help`：Caption="Help (online)"，ShortCut=112(**F1**)，OnClick=`MI_HelpClick` —— 打开在线帮助
- `MI_Forum`：Caption="Forum (online)"，OnClick=`MI_ForumClick` —— 打开论坛
- ---（N5 分隔线）
- `MI_QuickGuide`：Caption="Quick Guide"，OnClick=`MI_QuickGuideClick` —— 快速入门
- `MI_UserManual`：Caption="User Manual"，OnClick=`MI_UserManualClick` —— 用户手册
- ---（N23 分隔线）
- `MI_LiteVsPro`：Caption="Lite vs Pro (online)"，OnClick=`MI_LiteVsProClick` —— 版本对比
- `MI_Purchase`：Caption="Purchase (online)"，OnClick=`MI_PurchaseClick` —— 购买页面
- ---（N8 分隔线）
- `MI_Register` → Action=`Action_Register`：Caption="Register" —— 注册
- `MI_Unregister` → Action=`Action_Unregister`：Caption="Unregister" —— 取消注册
- ---（N15 分隔线）
- `MI_History`：Caption="Version History"，OnClick=`MI_HistoryClick` —— 版本更新历史
- `MI_Copyrights`：Caption="Copyrights"，OnClick=`MI_CopyrightsClick` —— 版权信息
- `MI_About` → Action=`Action_About`：Caption="About"（Shift+F1）—— 关于对话框

---

## 右键菜单：PM_Rules（规则列表）

Images=`ImageListSmall`，OnPopup=`PM_RulesPopup`（弹出时根据是否有选中规则来启用/禁用 Edit/Remove/Move/Duplicate 等菜单项）。

- `MenuItem_AddRule`: Caption="Add Rule", ImageIndex=18, ShortCut=45(**Ins**), OnClick=`MenuItem_AddRuleClick` —— 在末尾添加规则，打开 Form_AddRule 对话框
- `MenuItem_AddRuleAbove`: Caption="Add Rule (above)", ImageIndex=18, OnClick=`MenuItem_AddRuleAboveClick` —— 在选中规则上方插入新规则
- `MenuItem_AddRuleBelow`: Caption="Add Rule (below)", ImageIndex=18, OnClick=`MenuItem_AddRuleBelowClick` —— 在选中规则下方插入新规则
- `MenuItem_EditRule`: Caption="Edit Rule", ShortCut=13(**Enter**), OnClick=`MenuItem_EditRuleClick` —— 打开 Form_AddRule 编辑选中规则的参数
- `MenuItem_DuplicateRule`: Caption="Duplicate Rule", ShortCut=8237(**Shift+Ins**), OnClick=`MenuItem_DuplicateRuleClick` —— 复制选中规则并插入到其下方
- `MenuItem_RemoveRule`: Caption="Remove Rule", ImageIndex=19, ShortCut=46(**Del**), OnClick=`MenuItem_RemoveRuleClick` —— 删除选中规则（支持多选删除）
- `MenuItem_RemoveAllRules`: Caption="Remove All Rules", ShortCut=8238(**Shift+Del**), OnClick=`MenuItem_RemoveAllRulesClick` —— 删除规则列表中的所有规则
- ---（N18 分隔线）
- `MenuItem_RuleMoveUp`: Caption="Move Up", ImageIndex=20, ShortCut=16422(**Ctrl+Up**), OnClick=`MenuItem_RuleMoveUpClick` —— 将选中规则上移一位，影响规则执行顺序
- `MenuItem_RuleMoveDown`: Caption="Move Down", ImageIndex=21, ShortCut=16424(**Ctrl+Down**), OnClick=`MenuItem_RuleMoveDownClick` —— 将选中规则下移一位
- ---（N20 分隔线）
- `MenuItem_RulesSelectAll`: Caption="Select All", ShortCut=16449(**Ctrl+A**), OnClick=`MenuItem_RulesSelectAllClick` —— 高亮选中所有规则行
- `MenuItem_RulesMarkAll`: Caption="Mark All", ImageIndex=14, ShortCut=8269(**Shift+M**), OnClick=`MenuItem_RulesMarkAllClick` —— 勾选所有规则的复选框（启用所有规则）
- `MenuItem_RulesUnmarkAll`: Caption="Unmark All", ImageIndex=16, ShortCut=8277(**Shift+U**), OnClick=`MenuItem_RulesUnmarkAllClick` —— 取消勾选所有规则的复选框（禁用所有规则）
- ---（N4 分隔线）
- `MI_RulesToClipboard`: Caption="Export to Clipboard", 无快捷键, OnClick=`MI_RulesToClipboardClick` —— 将规则序列化为文本格式复制到剪贴板
- `MI_RuleComment`: Caption="Comment...", ShortCut=8205(**Shift+Enter**), OnClick=`MI_RuleCommentClick` —— 弹出输入框为选中规则添加/编辑注释文本，注释显示在 Statement 列中

---

## 右键菜单：PM_Files（文件列表）

Images=`ImageListSmall`，OnPopup=`PM_FilesPopup`（弹出时根据是否有选中文件、是否已预览等状态启用/禁用各菜单项）。

### 顶层项
- `MenuItem_AnalyzeName`: Caption="Analyze Name", ImageIndex=11, OnClick=`MenuItem_AnalyzeNameClick` —— 弹出窗口显示选中文件名的结构分析（如分隔符位置、数字序列等）
- `MenuItem_EditNewName`: Caption="Edit New Name", ImageIndex=17, ShortCut=113(**F2**), OnClick=`MenuItem_EditNewNameClick` —— 在文件列表中直接行内编辑 "New Name" 列的值，编辑结果覆盖规则计算的新名
- ---（N2 分隔线）

### Shell 子菜单（`MenuItem_Shell`）
- `MI_OpenFile`: Caption="Open File", ShortCut=13(**Enter**), OnClick=`MI_OpenFileClick` —— 调用 ShellExecute 用默认关联程序打开文件
- `MI_OpenWithNotepad`: Caption="Open with Notepad", ShortCut=8205(**Shift+Enter**), OnClick=`MI_OpenWithNotepadClick` —— 调用 notepad.exe 打开文件
- `MI_OpenContainingFolder`: Caption="Open containing folder", ShortCut=16397(**Ctrl+Enter**), OnClick=`MI_OpenContainingFolderClick` —— 调用 explorer.exe /select 打开文件所在目录并选中该文件
- `MI_FileProperties`: Caption="File Properties", ShortCut=32781(**Alt+Enter**), OnClick=`MI_FilePropertiesClick` —— 调用 Windows Shell API 显示文件属性对话框
- ---（N1 分隔线）
- `MI_CutFilesToClipboard`: Caption="Cut Files to Clipboard", ShortCut=24664(**Ctrl+Shift+X**), OnClick=`MI_CutFilesToClipboardClick` —— 将选中文件以剪切模式放入系统剪贴板（可粘贴到资源管理器）
- `MI_CopyFilesToClipboard`: Caption="Copy Files to Clipboard", ShortCut=24643(**Ctrl+Shift+C**), OnClick=`MI_CopyFilesToClipboardClick` —— 将选中文件以复制模式放入系统剪贴板
- ---（N14 分隔线）
- `MI_DeleteFiles`: Caption="Delete Files to Recycle Bin", 无快捷键, OnClick=`MI_DeleteFilesClick` —— 将选中文件移到 Windows 回收站（调用 SHFileOperation），同时从列表中移除

### Mark 子菜单（`MenuItem_Marking`）
- `MenuItem_Mark`: Caption="Mark", ImageIndex=14, ShortCut=8269(**Shift+M**), OnClick=`MenuItem_MarkClick` —— 勾选选中文件的复选框（使其参与重命名）
- `MenuItem_Unmark`: Caption="Unmark", ImageIndex=16, ShortCut=8277(**Shift+U**), OnClick=`MenuItem_UnmarkClick` —— 取消选中文件的复选框
- `MenuItem_InvertMarking`: Caption="Invert Marking", ImageIndex=15, ShortCut=45(**Insert**), OnClick=`MenuItem_InvertMarkingClick` —— 反转所有文件的标记状态
- ---（N11 分隔线）
- `MenuItem_MarkOnlyChangedIncCase`: Caption="Mark Only Changed (Inc. Case)", OnClick=`MenuItem_MarkOnlyChangedIncCaseClick` —— 取消全部标记，然后仅标记新名与原名不同的文件（大小写变化也算变化）
- `MenuItem_MarkOnlyChangedExcCase`: Caption="Mark Only Changed (Exc. Case)", OnClick=`MenuItem_MarkOnlyChangedExcCaseClick` —— 同上，但忽略大小写差异（仅标记字符内容真正变化的文件）
- ---（N19 分隔线）
- `MenuItem_MarkOnlySelected`: Caption="Mark Only Selected", OnClick=`MenuItem_MarkOnlySelectedClick` —— 取消全部标记，然后仅标记当前鼠标选中的文件
- `MenuItem_MarkByMask`: Caption="Mark by Mask", OnClick=`MenuItem_MarkByMaskClick` —— 弹出输入框输入通配符模式（如 *.jpg），标记匹配的文件

### Clear 子菜单（`MenuItem_Clearing`）
- `MenuItem_ClearAll`: Caption="Clear All", ShortCut=16430(**Ctrl+Del**), OnClick=`MenuItem_ClearAllClick` —— 从列表中移除所有文件条目
- `MenuItem_ClearRenamed`: Caption="Clear Renamed", OnClick=`MenuItem_ClearRenamedClick` —— 移除状态为“已成功重命名”的文件
- `MenuItem_ClearFailed`: Caption="Clear Failed", OnClick=`MenuItem_ClearFailedClick` —— 移除状态为“重命名失败”的文件
- `MenuItem_ClearValid`: Caption="Clear Valid", OnClick=`MenuItem_ClearValidClick` —— 移除预览状态为“有效”的文件
- `MenuItem_ClearInvalid`: Caption="Clear Invalid", OnClick=`MenuItem_ClearInvalidClick` —— 移除预览状态为“无效”的文件（如新名含非法字符）
- ---（N16 分隔线）
- `MenuItem_ClearMarked`: Caption="Clear Marked", OnClick=`MenuItem_ClearMarkedClick` —— 移除所有已勾选（已标记）的文件
- `MenuItem_ClearNotMarked`: Caption="Clear Not Marked", OnClick=`MenuItem_ClearNotMarkedClick` —— 移除所有未勾选（未标记）的文件
- ---（N21 分隔线）
- `MI_ClearNotChanged`: Caption="Clear Not Changed", ShortCut=16452(**Ctrl+D**), OnClick=`MI_ClearNotChangedClick` —— 移除新名与原名相同（未发生变化）的文件，便于仅保留有效变更

### Select 子菜单（`MenuItem_Selecting`）
- `MenuItem_SelectAll`: Caption="Select All", ShortCut=16449(**Ctrl+A**), OnClick=`MenuItem_SelectAllClick` —— 高亮选中所有文件行
- `MenuItem_InvertSelection`: Caption="Invert Selection", ShortCut=16457(**Ctrl+I**), OnClick=`MenuItem_InvertSelectionClick` —— 反转选中状态
- ---（N13 分隔线）
- `MenuItem_SelectByNameLength`: Caption="Select by Name Length", ShortCut=16460(**Ctrl+L**), OnClick=`MenuItem_SelectByNameLengthClick` —— 弹出对话框输入长度范围，选中文件名长度在该范围内的文件
- `MenuItem_SelectByExtension`: Caption="Select by Extension", ShortCut=16453(**Ctrl+E**), OnClick=`MenuItem_SelectByExtensionClick` —— 选中与当前焦点文件相同扩展名的所有文件
- `MenuItem_SelectByMask`: Caption="Select by Mask", ShortCut=16461(**Ctrl+M**), OnClick=`MenuItem_SelectByMaskClick` —— 弹出输入框输入通配符模式，选中匹配的文件

### Move 子菜单（`MenuItem_Moving`）
- `MenuItem_MoveUp`: Caption="Up", ShortCut=16422(**Ctrl+Up**), OnClick=`MenuItem_MoveUpClick` —— 将选中文件在列表中上移（影响重命名顺序，尤其对 Serialize 规则重要）
- `MenuItem_MoveDown`: Caption="Down", ShortCut=16424(**Ctrl+Down**), OnClick=`MenuItem_MoveDownClick` —— 将选中文件在列表中下移

### 底部
- ---（N3 分隔线）
- `MenuItem_RemoveSelectedItems`: Caption="Remove Selected", ImageIndex=1, ShortCut=46(**Del**), OnClick=`MenuItem_RemoveSelectedItemsClick` —— 从列表中移除鼠标选中的文件条目（不删除磁盘文件，仅从列表中去除）

---

## 右键菜单：PM_Options（选项）

Images=`ImageListSmall`。通过 `ToolButton_Options` 点击弹出。所有菜单项均绑定 Action，Caption/ShortCut/ImageIndex/OnExecute 均由 Action 定义。

- `MenuItem_AutoSizeColumns` → Action_AutosizeColumns: ImageIndex=7, ShortCut=8275(**Shift+S**), OnExecute=`Action_AutosizeColumnsExecute` —— 遍历所有可见列，根据内容自动计算并设置最佳列宽
- `MenuItem_Validate` → Action_ValidateNewNames: ImageIndex=8, ShortCut=8278(**Shift+V**), OnExecute=`Action_ValidateNewNamesExecute` —— 检查所有新文件名的合法性（非法字符如 \\/:*?"<>|、路径长度超过 260 字符、空文件名等），更新 State 列
- `MI_FixConflictingNewNames` → Action_FixConflictingNewNames: ImageIndex=9, ShortCut=8262(**Shift+F**), OnExecute=`Action_FixConflictingNewNamesExecute` —— 检测多个文件产生相同新名的情况，自动在后续重复名后添加序号后缀解决冲突
- `MenuItem_Analyze` → Action_AnalyzeSampleText: ImageIndex=11, ShortCut=8257(**Shift+A**), OnExecute=`Action_AnalyzeSampleTextExecute` —— 弹出对话框输入样例文本，应用当前规则并显示转换结果，用于测试规则效果
- `MenuItem_ApplyRulesToClipboard` → Action_ApplyRulesToClipboard: ImageIndex=11, ShortCut=8259(**Shift+C**), OnExecute=`Action_ApplyRulesToClipboardExecute` —— 读取剪贴板中的文本，应用当前规则后写回剪贴板
- `MI_CountMarkedAndSelectedFiles` → Action_CountMarkedAndSelectedFiles: ImageIndex=13, ShortCut=8265(**Shift+I**), OnExecute=`Action_CountMarkedAndSelectedFilesExecute` —— 弹出消息框显示当前已标记文件数、已选中文件数、总文件数
- `MI_SortForRenamingFolders` → Action_SortForRenamingFolders: ImageIndex=12, ShortCut=8274(**Shift+R**), OnExecute=`Action_SortForRenamingFoldersExecute` —— 按路径深度从深到浅排序，确保重命名文件夹时先处理子目录再处理父目录，避免路径失效

---

## 右键菜单：PM_Export（导出导入）

Images=`ImageListSmall`。通过 `ToolButton_Export` 点击弹出。大部分菜单项绑定 Action。

### 文件路径导出/导入
- `MI_ExportFilesAndUndo` → Action_ExportFilesAndUndo: ImageIndex=23, OnExecute=`Action_ExportFilesAndUndoExecute` —— 弹出保存对话框，导出每行格式为 "原路径|撤销路径" 的文本文件，用于外部撤销
- `MI_ExportFilesAndPreview` → Action_ExportFilesAndPreview: ImageIndex=23, OnExecute=`Action_ExportFilesAndPreviewExecute` —— 导出每行格式为 "原路径|新名" 的文本文件
- `MI_ImportFilesAndPreview` → Action_ImportFilesAndPreview: ImageIndex=22, OnExecute=`Action_ImportFilesAndPreviewExecute` —— 从上述格式的文本文件导入，加载文件并设置预览新名
- ---（MI_Break25 分隔线）
- `MI_ImportListOfFiles` → Action_ImportListOfFiles: ImageIndex=22, OnExecute=`Action_ImportListOfFilesExecute` —— 从 .txt/.m3u/.pls 等文本/播放列表文件中读取文件路径并添加到列表

### 批处理文件
- `MI_ExportAsBatchFileFullPaths` → Action_ExportAsBatchFileFullPaths: ImageIndex=23, OnExecute=`Action_ExportAsBatchFileFullPathsExecute` —— 导出为 Windows .bat 批处理文件，包含 `ren "C:\full\path\old" "new"` 命令
- `MI_ExportAsBatchFileOnlyNames` → Action_ExportAsBatchFileOnlyNames: ImageIndex=23, OnExecute=`Action_ExportAsBatchFileOnlyNamesExecute` —— 同上但仅使用文件名（需在目标目录执行）

### 剪贴板操作
- ---（MI_Break26 分隔线）
- `MI_ExportNewNamesToClipboard` → Action_ExportNewNamesToClipboard: ImageIndex=23, OnExecute=`Action_ExportNewNamesToClipboardExecute` —— 将所有文件的新名称逐行复制到剪贴板
- `MI_ImportNewNamesFromClipboard` → Action_ImportNewNamesFromClipboard: ImageIndex=22, OnExecute=`Action_ImportNewNamesFromClipboardExecute` —— 从剪贴板读取多行文本，按顺序设置为各文件的新名称
- `MenuItem_ExportNamesAndNewNamesToClipboard` → Action_ExportNamesAndNewNamesToClipboard: ImageIndex=23, OnExecute=`Action_ExportNamesAndNewNamesToClipboardExecute` —— 将 "原名|Tab|新名" 格式复制到剪贴板
- `MI_ExportPathsToClipboard` → Action_ExportPathsToClipboard: ImageIndex=23, OnExecute=`Action_ExportPathsToClipboardExecute` —— 将所有文件的完整路径逐行复制到剪贴板
- `MI_ExportColumnsToClipboard`: Caption="Export all columns to clipboard", ImageIndex=23, OnClick=`MI_ExportColumnsToClipboardClick`（此项无 Action 绑定，直接 OnClick） —— 将所有可见列的数据以 Tab 分隔的表格格式复制到剪贴板

---

## 右键菜单：PM_FilesColumns（文件列表列标题）

通过右键点击文件列表的列标题弹出。OnPopup=`PM_FilesColumnsPopup`（动态生成列显示/隐藏切换菜单项）。

固定菜单项：
- ---（分隔线，Tag=255）
- `Cancel Sorting`（Tag=255）—— 取消当前排序，恢复原始顺序

运行时动态生成 21 列对应的菜单项（勾选=显示，取消=隐藏），位于分隔线之前。

---

## ActionList（动作列表）

Images=`ImageListSmall`。28 个 Action 按类别组织：

### Main 类别
- `Action_AddFiles`：Caption="Add Files"，ShortCut=F3
- `Action_AddFolders`：Caption="Add Folders"，ShortCut=F4
- `Action_AddPaths`：Caption="Add Paths"（无快捷键）
- `Action_Preview`：Caption="Preview"，ShortCut=F5
- `Action_Rename`：Caption="Rename"，ShortCut=F6
- `Action_NewProject`：Caption="New Project"，ShortCut=Ctrl+N
- `Action_NewInstance`：Caption="New Instance"，ShortCut=Ctrl+Shift+N
- `Action_UndoRename`：Caption="Undo Renaming"，ShortCut=Ctrl+Shift+Z
- `Action_PasteFiles`：Caption="Paste Files"，ShortCut=Ctrl+Shift+V
- `Action_Settings`：Caption="All Settings"，ShortCut=F8
- `Action_Filters`：Caption="Filters"，ShortCut=Ctrl+F
- `Action_ToggleViewMode`：Caption="Toggle view mode"（无快捷键）
- `Action_About`：Caption="About"，ShortCut=Shift+F1
- `Action_Register`：Caption="Register"（无快捷键）
- `Action_Unregister`：Caption="Unregister"（无快捷键）

### Options 类别
- `Action_AutosizeColumns`：Shift+S，ImageIndex=7
- `Action_ValidateNewNames`：Shift+V，ImageIndex=8
- `Action_FixConflictingNewNames`：Shift+F，ImageIndex=9
- `Action_AnalyzeSampleText`：Shift+A，ImageIndex=11
- `Action_ApplyRulesToClipboard`：Shift+C，ImageIndex=11
- `Action_CountMarkedAndSelectedFiles`：Shift+I，ImageIndex=13
- `Action_SortForRenamingFolders`：Shift+R，ImageIndex=12

### Export 类别
- `Action_ExportFilesAndUndo`：ImageIndex=23（导出图标）
- `Action_ExportFilesAndPreview`：ImageIndex=23
- `Action_ImportFilesAndPreview`：ImageIndex=22（导入图标）
- `Action_ImportListOfFiles`：ImageIndex=22
- `Action_ExportNewNamesToClipboard`：ImageIndex=23
- `Action_ImportNewNamesFromClipboard`：ImageIndex=22
- `Action_ExportAsBatchFileFullPaths`：ImageIndex=23
- `Action_ExportAsBatchFileOnlyNames`：ImageIndex=23
- `Action_ExportPathsToClipboard`：ImageIndex=23
- `Action_ExportNamesAndNewNamesToClipboard`：ImageIndex=23

### Presets 类别
- `Action_PresetSave`：Caption="Save"，ShortCut=Ctrl+Shift+S
- `Action_PresetSaveAs`：Caption="Save As"，ShortCut=Ctrl+S
- `Action_ManagePresets`：Caption="Manage..."，ShortCut=Ctrl+P

---

## ImageList 资源

- `ImageListBig`：32×32，主工具栏图标（Add Files/Folders/Preview/Rename 等）
- `ImageListSmall`：16×16（默认），文件列表图标 + 菜单图标 + Action 图标
- `ImageListToolbarRules`：13×13，规则工具栏图标（Add/Remove/Up/Down）
- `ImageListToolbarFiles`：12×12，文件工具栏图标（Files/Columns/Options/Export/Filters/Analyze）
- `ImageListFilters`：16×16，过滤器相关图标
- `ImageListLanguages`：16×11，语言菜单国旗图标（运行时加载）

---

## 快捷键完整参考表

### 全局快捷键（通过 KeyPreview 截获）
- **F3** —— 添加文件
- **F4** —— 添加文件夹
- **F5** —— 预览
- **F6** —— 重命名
- **F8** —— 所有设置
- **F1** —— 在线帮助
- **Shift+F1** —— 关于
- **Ctrl+N** —— 新建项目
- **Ctrl+Shift+N** —— 新实例
- **Ctrl+Shift+Z** —— 撤销重命名
- **Ctrl+Shift+V** —— 粘贴文件
- **Ctrl+F** —— 过滤器
- **Ctrl+S** —— 预设另存为
- **Ctrl+Shift+S** —— 保存预设
- **Ctrl+P** —— 管理预设
- **Alt+F4** —— 退出

### 文件列表快捷键
- **F2** —— 行内编辑新名
- **Enter** —— 打开文件
- **Shift+Enter** —— 用记事本打开
- **Ctrl+Enter** —— 打开所在文件夹
- **Alt+Enter** —— 文件属性
- **Del** —— 从列表移除选中文件
- **Ctrl+Del** —— 清除所有文件
- **Ctrl+D** —— 清除未变化的文件
- **Ctrl+A** —— 全选
- **Ctrl+I** —— 反选
- **Ctrl+L** —— 按名称长度选择
- **Ctrl+E** —— 按扩展名选择
- **Ctrl+M** —— 按掩码选择
- **Ctrl+Up** —— 上移文件
- **Ctrl+Down** —— 下移文件
- **Shift+M** —— 标记
- **Shift+U** —— 取消标记
- **Insert** —— 反转标记
- **Ctrl+Shift+X** —— 剪切文件到剪贴板
- **Ctrl+Shift+C** —— 复制文件到剪贴板

### 规则列表快捷键
- **Ins** —— 添加规则
- **Enter** —— 编辑规则
- **Del** —— 删除规则
- **Shift+Ins** —— 复制规则
- **Shift+Del** —— 删除所有规则
- **Ctrl+Up** —— 上移规则
- **Ctrl+Down** —— 下移规则
- **Ctrl+A** —— 全选规则
- **Shift+M** —— 启用所有规则
- **Shift+U** —— 禁用所有规则
- **Shift+Enter** —— 添加规则注释

### Options 快捷键
- **Shift+S** —— 自动调整列宽
- **Shift+V** —— 验证新名
- **Shift+F** —— 修复冲突名
- **Shift+A** —— 分析样例文本
- **Shift+C** —— 应用规则到剪贴板
- **Shift+I** —— 统计标记/选中文件数
- **Shift+R** —— 按路径排序

---

## VST_Files 列详细属性补充

每列的 Options 属性决定其行为：

**默认可见列**（Options 包含 coVisible，即默认 Options 集合）：
- Column 0 (`State`)：Width=150, Position=0, 默认全部 Options（coAllowClick, coDraggable, coEnabled, coParentBidiMode, coParentColor, coResizable, coShowDropMark, coVisible, coAllowFocus）
- Column 3 (`Name`)：Width=150, Position=3, 同上
- Column 4 (`New Name`)：Width=150, Position=4, 同上
- Column 20 (`Error`)：Width=默认, Position=20, 同上

**默认隐藏列**（Options 不含 coVisible，仅包含: coAllowClick, coDraggable, coEnabled, coParentBidiMode, coParentColor, coResizable, coShowDropMark, coAllowFocus）：
- Column 1 (`Path`)：Position=1, 左对齐
- Column 2 (`Folder`)：Position=2, 左对齐
- Column 5 (`New Path`)：Position=5, 左对齐
- Column 6 (`Size`)：Position=6, **Alignment=taRightJustify**（右对齐）
- Column 7 (`Size KB`)：Position=7, **Alignment=taRightJustify**
- Column 8 (`Size MB`)：Position=8, **Alignment=taRightJustify**
- Column 9 (`Created`)：Position=9, 左对齐
- Column 10 (`Modified`)：Position=10, 左对齐
- Column 11 (`Extension`)：Position=11, 左对齐
- Column 12 (`Name Digits`)：Position=12, **Alignment=taRightJustify**
- Column 13 (`Path Digits`)：Position=13, 左对齐
- Column 14 (`Name Length`)：Position=14, **Alignment=taRightJustify**
- Column 15 (`New Name Length`)：Position=15, **Alignment=taRightJustify**
- Column 16 (`Path Length`)：Position=16, **Alignment=taRightJustify**
- Column 17 (`New Path Length`)：Position=17, **Alignment=taRightJustify**
- Column 18 (`Exif Date`)：Position=18, 左对齐
- Column 19 (`Old Path`)：Position=19, 左对齐

所有列均支持 coAllowClick（点击标题排序）、coDraggable（拖拽调整列顺序）、coResizable（调整列宽）。

---

## 核心工作流程

### 1. 文件添加流程

**通过对话框添加文件**：
1. 用户点击工具栏 "Add" 按钮或菜单 File → Add Files (F3)
2. 弹出文件选择对话框（TOpenDialog），支持多选
3. 用户选择文件并确认
4. 对每个文件创建 VST_Files 节点，分配 TFileData 节点数据，填充文件路径、大小、日期等属性
5. 节点默认为 Checked=True（参与重命名），State=未处理
6. 刷新列表显示，更新状态栏文件统计

**通过对话框添加文件夹**：
1. 用户点击 File → Add Folders (F4)
2. 弹出文件夹选择对话框
3. 递归扫描文件夹中的文件（受 Filters 设置的过滤条件影响）
4. 将符合条件的文件批量添加到列表（同上述节点创建流程）

**其他添加方式**：
- 拖放（从 Windows 资源管理器拖拽文件/文件夹到窗口）：通过 `FormCreate` 中设置 `DragAcceptFiles` 或使用 WM_DROPFILES 消息接受拖放
- 粘贴（Ctrl+Shift+V / `Action_PasteFiles`）：从系统剪贴板读取文件路径列表（支持 CF_HDROP 和文本格式），添加到列表
- 手动输入（`Action_AddPaths`）：弹出多行输入框，用户粘贴或输入文件路径（每行一个）
- 导入（`Action_ImportListOfFiles`）：从 .txt/.m3u/.pls 文件读取文件路径列表

**重复文件处理**：若添加的文件已在列表中存在（路径完全相同），跳过不重复添加

### 2. 预览流程（Preview）

1. 用户点击工具栏 "Preview" 或按 F5（触发 `Action_Preview`）
2. 检查文件列表是否为空、规则列表是否为空，若为空则提示并返回
3. StatusBar Panel[0] 开始显示进度条
4. 遍历文件列表中所有 Checked=True 的节点：
   a. 获取原始文件名
   b. 按规则列表顺序（上到下）依次应用每个 Checked=True 的规则：前一个规则的输出作为下一个规则的输入（链式处理）
   c. 将最终结果存储为节点的 New Name
   d. 比较 New Name 与原名，设置 State 列状态（Changed/Unchanged/Error）
   e. 更新进度条
5. 如果节点有手动编辑的 New Name（通过行内编辑设置），则使用手动值而非规则计算结果
6. 刷新 VST_Files 显示，更新所有可见列的文本和状态图标
7. 进度条显示 100%，然后清除

### 3. 重命名流程（Rename）

1. 用户点击工具栏 "Rename" 或按 F6（触发 `Action_Rename`）
2. 如果尚未执行预览，先自动执行预览流程
3. 弹出确认对话框（可通过设置禁用），显示将要重命名的文件数量
4. 保存当前文件列表的撤销信息（原路径 → Old Path 列）
5. StatusBar Panel[0] 显示进度条
6. 遍历所有 Checked=True 且有有效 New Name 的节点：
   a. 调用 Windows API（MoveFile/MoveFileEx）执行实际文件重命名
   b. 成功：设置 State=Renamed（绿色勾），更新节点数据中的文件名为新名
   c. 失败：设置 State=Error（红色叉），在 Error 列显示错误原因（如文件被占用、权限不足、目标已存在等）
   d. 更新进度条
7. 完成后刷新显示，状态栏显示统计结果（成功 X 个，失败 Y 个）

### 4. 撤销机制（Undo Renaming）

1. 用户点击 File → Undo Renaming 或按 Ctrl+Shift+Z（触发 `Action_UndoRename`）
2. 系统检查是否有可撤销的操作（Old Path 列是否有数据）
3. 弹出确认对话框
4. 遍历文件列表，对每个有 Old Path 数据的节点：
   a. 调用 MoveFile 将文件从当前路径移动回 Old Path
   b. 成功则恢复节点数据为原始状态
   c. 失败则标记错误
5. 清除 Old Path 数据，撤销后不能再次撤销（单级撤销）

### 5. 规则链式处理机制

规则按 ListView_Rules 中的顺序（从上到下）依次应用：
- 每个规则接收上一个规则的输出作为输入（第一个规则接收原始文件名）
- 仅 Checked=True（复选框勾选）的规则参与处理，未勾选的规则被跳过
- 规则顺序影响结果：例如先替换后序列化 ≠ 先序列化后替换
- 可通过 PM_Rules 的 Move Up/Move Down 或拖放调整规则顺序
- 每个规则的处理范围取决于规则自身的 "Apply to" 设置（Name Only / Extension Only / Full Name）

### 6. 文件外部拖放行为

- 支持从 Windows 资源管理器拖放文件/文件夹到主窗口
- 使用 WM_DROPFILES 消息处理，或通过 `DragAcceptFiles(Handle, True)` 注册
- 拖放文件：直接添加到文件列表
- 拖放文件夹：递归扫描其中的文件（应用 Filters 过滤）后添加
- 拖放 .rnp 预设文件：加载预设而非作为普通文件添加

### 7. 项目与预设机制

**新建项目**（`Action_NewProject` / Ctrl+N）：
- 清空规则列表和文件列表
- 重置所有状态（撤销数据、预览结果等）
- 重置窗口标题

**预设保存**（`Action_PresetSaveAs` / Ctrl+S）：
- 将当前规则列表序列化为 .rnp 文件（包含每个规则的类型、参数、启用状态）
- 弹出保存对话框选择保存位置

**预设加载**（通过 Presets → Load 子菜单）：
- 从 .rnp 文件反序列化规则列表
- 替换当前规则列表内容
- 文件列表保持不变

### 8. Filters 过滤机制

通过 Settings → Filters 或工具栏 "Filters" 按钮配置：
- 文件名匹配模式（包含/排除），支持通配符
- 文件大小范围限制
- 文件日期范围限制
- 文件属性过滤（只读、隐藏、系统文件等）
- 过滤器影响文件夹添加时的递归扫描，不符合条件的文件不会被添加到列表
