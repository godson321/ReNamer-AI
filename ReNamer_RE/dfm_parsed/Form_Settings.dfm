object Form_Settings: TForm_Settings
  Left = 507
  Height = 417
  Top = 312
  Width = 424
  BorderIcons = [biSystemMenu]
  Caption = 'Settings'
  ClientHeight = 417
  ClientWidth = 424
  OnCreate = FormCreate
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object BitBtn_Save: TBitBtn
    Left = 48
    Height = 25
    Top = 376
    Width = 160
    Caption = 'Save'
    Default = True
    Glyph.Data = {Binary: 1082 bytes}
    ModalResult = 1
    OnClick = BitBtn_SaveClick
    TabOrder = 1
  end
  object BitBtn_Cancel: TBitBtn
    Left = 232
    Height = 25
    Top = 376
    Width = 137
    Cancel = True
    Caption = 'Cancel'
    ModalResult = 2
    OnClick = BitBtn_CancelClick
    TabOrder = 2
  end
  object PageControl: TPageControl
    Left = 0
    Height = 360
    Top = 0
    Width = 424
    ActivePage = TabSheet_General
    Align = alTop
    TabIndex = 0
    TabOrder = 0
    object TabSheet_General: TTabSheet
      Caption = 'General'
      ClientHeight = 332
      ClientWidth = 416
      object SpeedButton_FilterSettings: TSpeedButton
        Left = 24
        Height = 25
        Top = 264
        Width = 368
        Caption = 'Configure Filter Settings'
        Flat = True
        Glyph.Data = {Binary: 1082 bytes}
        OnClick = SpeedButton_FilterSettingsClick
      end
      object Bevel1: TBevel
        Left = 24
        Height = 9
        Top = 240
        Width = 368
        Anchors = [akTop, akLeft, akRight]
        Shape = bsBottomLine
      end
      object Bevel3: TBevel
        Left = 24
        Height = 9
        Top = 141
        Width = 368
        Anchors = [akTop, akLeft, akRight]
        Shape = bsBottomLine
      end
      object Label3: TLabel
        Left = 42
        Height = 32
        Top = 208
        Width = 350
        Anchors = [akTop, akLeft, akRight]
        AutoSize = False
        Caption = 'Example: file1, file2 ... file9, file10, file11, and so on.'
        ParentColor = False
        WordWrap = True
      end
      object CheckBox_RememberLocation: TCheckBox
        Left = 24
        Height = 19
        Top = 48
        Width = 265
        Caption = 'Remember position and size of major windows'
        TabOrder = 1
      end
      object CheckBox_SaveAndLoadDefaultPreset: TCheckBox
        Left = 24
        Height = 19
        Top = 96
        Width = 270
        Caption = 'Save rules configuration on exit, load on startup'
        TabOrder = 3
      end
      object CheckBox_AlwaysOnTop: TCheckBox
        Left = 24
        Height = 19
        Top = 24
        Width = 173
        Caption = 'Stay on top of other windows'
        TabOrder = 0
      end
      object CheckBox_UseNaturalSort: TCheckBox
        Left = 24
        Height = 19
        Top = 184
        Width = 186
        Caption = 'Use natural sort order algorithm'
        TabOrder = 5
      end
      object CheckBox_SortingRemember: TCheckBox
        Left = 24
        Height = 19
        Top = 72
        Width = 220
        Caption = 'Remember sorting options (files table)'
        TabOrder = 2
      end
      object CB_AllowProcessingExtensionsInFolders: TCheckBox
        Left = 24
        Height = 19
        Top = 160
        Width = 286
        Caption = 'Folders also have extensions, process them as such'
        TabOrder = 4
      end
      object CheckBox_ShowMainToolbar: TCheckBox
        Left = 24
        Height = 19
        Top = 120
        Width = 118
        Caption = 'Show main toolbar'
        TabOrder = 6
      end
    end
    object TabSheet_Preview: TTabSheet
      Caption = 'Preview'
      ClientHeight = 332
      ClientWidth = 416
      ImageIndex = 4
      object Label5: TLabel
        Left = 24
        Height = 53
        Top = 243
        Width = 361
        Anchors = [akTop, akLeft, akRight]
        AutoSize = False
        Caption = 'Note: disabling any of these options will increase performance when processing large amount of files.'
        ParentColor = False
        WordWrap = True
      end
      object Bevel6: TBevel
        Left = 24
        Height = 9
        Top = 144
        Width = 367
        Anchors = [akTop, akLeft, akRight]
        Shape = bsBottomLine
      end
      object SB_RealtimePreviewWarning: TSpeedButton
        Left = 2
        Height = 19
        Top = 219
        Width = 19
        Flat = True
        Glyph.Data = {Binary: 1082 bytes}
        OnClick = SB_RealtimePreviewWarningClick
      end
      object Bevel7: TBevel
        Left = 24
        Height = 9
        Top = 203
        Width = 367
        Anchors = [akTop, akLeft, akRight]
        Shape = bsBottomLine
      end
      object CheckBox_ValidateOnPreview: TCheckBox
        Left = 24
        Height = 19
        Top = 160
        Width = 200
        Caption = 'Validate new filenames on preview'
        TabOrder = 5
      end
      object CheckBox_AutoPreviewOnRulesChange: TCheckBox
        Left = 24
        Height = 19
        Top = 48
        Width = 269
        Caption = 'Auto preview on change of rules configurations'
        TabOrder = 1
      end
      object CheckBox_AutoSizeTable: TCheckBox
        Left = 24
        Height = 19
        Top = 24
        Width = 154
        Caption = 'Auto size table on change'
        TabOrder = 0
      end
      object CheckBox_PreviewRealTimeUpdate: TCheckBox
        Left = 24
        Height = 19
        Top = 219
        Width = 187
        Caption = 'Real-time update of the preview'
        TabOrder = 7
      end
      object CheckBox_AutoPreviewOnFilesAdded: TCheckBox
        Left = 24
        Height = 19
        Top = 72
        Width = 224
        Caption = 'Auto preview when new files are added'
        TabOrder = 2
      end
      object CheckBox_FixConflictingNewNamesOnPreview: TCheckBox
        Left = 24
        Height = 19
        Top = 184
        Width = 217
        Caption = 'Fix conflicting new names on preview'
        TabOrder = 6
      end
      object CB_HighlightChangedNames: TCheckBox
        Left = 24
        Height = 19
        Top = 96
        Width = 174
        Caption = 'Highlight changed file names'
        TabOrder = 3
      end
      object ColorBox_HighlightChangedNames: TColorBox
        Left = 40
        Height = 22
        Top = 120
        Width = 145
        Style = [cbStandardColors, cbExtendedColors, cbCustomColor, cbPrettyNames]
        ItemHeight = 16
        TabOrder = 4
      end
    end
    object TabSheet_Rename: TTabSheet
      Caption = 'Rename'
      ClientHeight = 332
      ClientWidth = 416
      ImageIndex = 1
      object Bevel2: TBevel
        Left = 24
        Height = 9
        Top = 192
        Width = 368
        Anchors = [akTop, akLeft, akRight]
        Shape = bsBottomLine
      end
      object CheckBox_DisplayMsgOnSuccessfulRename: TCheckBox
        Left = 24
        Height = 19
        Top = 24
        Width = 222
        Caption = 'Display message on successful rename'
        TabOrder = 0
      end
      object CheckBox_ClearRulesListOnRename: TCheckBox
        Left = 24
        Height = 19
        Top = 96
        Width = 151
        Caption = 'Clear rules list on rename'
        TabOrder = 3
      end
      object CheckBox_ClearFilesTableOnRename: TCheckBox
        Left = 24
        Height = 19
        Top = 120
        Width = 158
        Caption = 'Clear files table on rename'
        TabOrder = 4
      end
      object CheckBox_OverwriteWithNewName: TCheckBox
        Left = 24
        Height = 19
        Top = 216
        Width = 186
        Caption = 'Overwrite files with New Names'
        TabOrder = 7
        OnClick = CheckBox_OverwriteWithNewNameClick
      end
      object StaticText5: TStaticText
        Left = 24
        Height = 48
        Top = 272
        Width = 368
        Anchors = [akTop, akLeft, akRight]
        Caption = 'Warning: Overwritten files will be deleted permanently.'
        TabOrder = 9
      end
      object CheckBox_CloseProgramAfterSuccessfulRename: TCheckBox
        Left = 24
        Height = 19
        Top = 48
        Width = 223
        Caption = 'Close program after successful rename'
        TabOrder = 1
      end
      object CheckBox_ClearRenamedFilesOnRename: TCheckBox
        Left = 24
        Height = 19
        Top = 144
        Width = 179
        Caption = 'Clear renamed files on rename'
        TabOrder = 5
      end
      object CB_OverwriteWithNewNameConfirm: TCheckBox
        Left = 40
        Height = 19
        Top = 240
        Width = 226
        Caption = 'Must be successfully confirmed by user'
        TabOrder = 8
      end
      object CheckBox_SkipRenamingUnchangedFiles: TCheckBox
        Left = 24
        Height = 19
        Top = 168
        Width = 195
        Caption = 'Skip renaming of unchanged files'
        TabOrder = 6
      end
      object CheckBox_UnmarkRulesListOnRename: TCheckBox
        Left = 24
        Height = 19
        Top = 72
        Width = 166
        Caption = 'Unmark rules list on rename'
        TabOrder = 2
      end
    end
    object TabSheet_MetaTags: TTabSheet
      Caption = 'Meta Tags'
      ClientHeight = 332
      ClientWidth = 416
      ImageIndex = 3
      object Label1: TLabel
        Left = 24
        Height = 15
        Top = 123
        Width = 68
        Caption = 'Date Format:'
        ParentColor = False
      end
      object Label_MetaTagFormatNote: TLabel
        Left = 24
        Height = 48
        Top = 232
        Width = 367
        Anchors = [akTop, akLeft, akRight]
        AutoSize = False
        Caption = 'Note: formats will be saved only if validated successfully.'
        ParentColor = False
        WordWrap = True
      end
      object SpeedButton_MetaTagFormatsInfo: TSpeedButton
        Left = 319
        Height = 81
        Hint = 'Online Help (F1)'
        Top = 144
        Width = 72
        Anchors = [akTop, akRight]
        Caption = 'Help'
        Flat = True
        Glyph.Data = {Binary: 1082 bytes}
        Layout = blGlyphTop
        OnClick = SpeedButton_MetaTagFormatsInfoClick
        ShowHint = True
        ParentShowHint = False
      end
      object Bevel5: TBevel
        Left = 24
        Height = 9
        Top = 96
        Width = 367
        Anchors = [akTop, akLeft, akRight]
        Shape = bsBottomLine
      end
      object Label2: TLabel
        Left = 24
        Height = 15
        Top = 179
        Width = 44
        Caption = 'Preview:'
        ParentColor = False
      end
      object CheckBox_MetaTagSupport: TCheckBox
        Left = 24
        Height = 19
        Top = 24
        Width = 108
        Caption = 'MetaTag support'
        TabOrder = 0
      end
      object Edit_MetaTagDateTimeFormat: TEdit
        Left = 24
        Height = 25
        Top = 144
        Width = 264
        Anchors = [akTop, akLeft, akRight]
        Font.Height = 243
        MaxLength = 100
        ParentFont = False
        TabOrder = 1
        OnKeyDown = DateTimeFormatKeyDown
        OnKeyUp = Edit_MetaTagDateTimeFormatKeyUp
      end
      object Edit_MetaTagDateTimeFormatTest: TEdit
        Left = 24
        Height = 25
        Top = 200
        Width = 264
        Anchors = [akTop, akLeft, akRight]
        Color = clBtnFace
        Font.Height = 243
        ParentFont = False
        ReadOnly = True
        TabOrder = 2
        OnKeyDown = DateTimeFormatKeyDown
      end
      object StaticText1: TStaticText
        Left = 24
        Height = 48
        Top = 48
        Width = 367
        Anchors = [akTop, akLeft, akRight]
        Caption = 'Note: Disabling MetaTag support will increase performance when processing large amount of files.'
        TabOrder = 3
      end
    end
    object TabSheet_Misc: TTabSheet
      Caption = 'Misc.'
      ClientHeight = 332
      ClientWidth = 416
      ImageIndex = 2
      object Label_DragFilesText: TLabel
        Left = 24
        Height = 15
        Top = 136
        Width = 196
        Caption = 'Change text of "Drag your files here":'
        ParentColor = False
      end
      object Bevel4: TBevel
        Left = 24
        Height = 9
        Top = 104
        Width = 367
        Anchors = [akTop, akLeft, akRight]
        Shape = bsBottomLine
      end
      object Label_AddRulesText: TLabel
        Left = 24
        Height = 15
        Top = 192
        Width = 215
        Caption = 'Change text of "Click here to add a rule":'
        ParentColor = False
      end
      object CheckBox_RegisterPresetExtension: TCheckBox
        Left = 24
        Height = 19
        Top = 24
        Width = 186
        Caption = 'Register preset extension (*.rnp)'
        TabOrder = 0
      end
      object CheckBox_AddToFolderContextMenu: TCheckBox
        Left = 24
        Height = 19
        Top = 48
        Width = 170
        Caption = 'Add to folders context menu'
        TabOrder = 1
      end
      object Edit_DragFilesText: TEdit
        Left = 24
        Height = 25
        Top = 157
        Width = 360
        Anchors = [akTop, akLeft, akRight]
        Font.Height = 243
        ParentFont = False
        TabOrder = 3
      end
      object Edit_AddRulesText: TEdit
        Left = 24
        Height = 25
        Top = 213
        Width = 360
        Anchors = [akTop, akLeft, akRight]
        Font.Height = 243
        ParentFont = False
        TabOrder = 4
      end
      object CheckBox_AddToSendToContextMenu: TCheckBox
        Left = 24
        Height = 19
        Top = 72
        Width = 186
        Caption = 'Add to "Send To" context menu'
        TabOrder = 2
      end
    end
  end
end

