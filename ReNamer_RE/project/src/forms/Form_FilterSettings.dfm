object Form_FilterSettings: TForm_FilterSettings
  Left = 557
  Height = 447
  Top = 321
  Width = 329
  BorderIcons = [biSystemMenu]
  Caption = 'Filter Settings'
  ClientHeight = 447
  ClientWidth = 329
  OnCreate = FormCreate
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object GroupBox_Folders: TGroupBox
    Left = 16
    Height = 177
    Top = 16
    Width = 297
    Anchors = [akTop, akLeft, akRight]
    Caption = 'Default behavior for adding folders:'
    ClientHeight = 157
    ClientWidth = 293
    TabOrder = 0
    object SB_FoldersWarning: TSpeedButton
      Left = 8
      Height = 19
      Top = 32
      Width = 19
      Flat = True
      Glyph.Data = {Binary: 1082 bytes}
      OnClick = SB_FoldersWarningClick
    end
    object CheckBox_IncludeSubfolders: TCheckBox
      Left = 56
      Height = 19
      Top = 56
      Width = 115
      Caption = 'Include subfolders'
      TabOrder = 2
    end
    object CheckBox_IncludeHidden: TCheckBox
      Left = 56
      Height = 19
      Top = 80
      Width = 129
      Caption = 'Include hidden items'
      TabOrder = 3
    end
    object CheckBox_AddFilesFromFolders: TCheckBox
      Left = 56
      Height = 19
      Top = 8
      Width = 139
      Caption = 'Add files within folders'
      TabOrder = 0
    end
    object CheckBox_AddFoldersAsFiles: TCheckBox
      Left = 56
      Height = 19
      Top = 32
      Width = 117
      Caption = 'Add folders as files'
      TabOrder = 1
    end
    object CheckBox_SkipRootFoldersAsFiles: TCheckBox
      Left = 56
      Height = 19
      Top = 128
      Width = 210
      Caption = 'Skip root folders when added as files'
      TabOrder = 5
    end
    object CheckBox_IncludeSystem: TCheckBox
      Left = 56
      Height = 19
      Top = 104
      Width = 129
      Caption = 'Include system items'
      TabOrder = 4
    end
    object Image_AddFilesFromFolders: TImage
      Left = 31
      Height = 19
      Top = 8
      Width = 19
      Center = True
      Picture.Data = {Binary: 668 bytes}
    end
    object Image_AddFoldersAsFiles: TImage
      Left = 31
      Height = 19
      Top = 32
      Width = 19
      Center = True
      Picture.Data = {Binary: 800 bytes}
    end
    object Image_IncludeSubfolders: TImage
      Left = 31
      Height = 19
      Top = 56
      Width = 19
      Center = True
      Picture.Data = {Binary: 843 bytes}
    end
    object Image_IncludeHidden: TImage
      Left = 31
      Height = 19
      Top = 80
      Width = 19
      Center = True
      Picture.Data = {Binary: 3148 bytes}
    end
    object Image_IncludeSystem: TImage
      Left = 31
      Height = 19
      Top = 104
      Width = 19
      Center = True
      Picture.Data = {Binary: 3175 bytes}
    end
    object Image_SkipRootFoldersAsFiles: TImage
      Left = 31
      Height = 19
      Top = 128
      Width = 19
      Center = True
      Picture.Data = {Binary: 3383 bytes}
    end
  end
  object BitBtn_Save: TBitBtn
    Left = 32
    Height = 25
    Top = 384
    Width = 121
    Caption = 'Save'
    Default = True
    Glyph.Data = {Binary: 1082 bytes}
    ModalResult = 1
    OnClick = BitBtn_SaveClick
    TabOrder = 2
  end
  object BitBtn_Cancel: TBitBtn
    Left = 176
    Height = 25
    Top = 384
    Width = 121
    Cancel = True
    Caption = 'Cancel'
    ModalResult = 2
    OnClick = BitBtn_CancelClick
    TabOrder = 3
  end
  object GroupBox_Masks: TGroupBox
    Left = 16
    Height = 168
    Top = 200
    Width = 297
    Anchors = [akTop, akLeft, akRight]
    Caption = 'Masks:'
    ClientHeight = 148
    ClientWidth = 293
    TabOrder = 1
    object Label_MasksNote: TLabel
      Left = 14
      Height = 15
      Top = 128
      Width = 275
      Caption = 'Note: separate multiple masks with ";" (semicolons).'
      ParentColor = False
    end
    object Edit_MasksInclude: TEdit
      Left = 14
      Height = 25
      Top = 24
      Width = 265
      Anchors = [akTop, akLeft, akRight]
      Font.Height = 243
      ParentFont = False
      TabOrder = 0
      OnChange = Edit_MasksIncludeChange
    end
    object CB_MasksOnFileName: TCheckBox
      Left = 14
      Height = 19
      Top = 104
      Width = 269
      Caption = 'Apply only to the file name, instead of full path.'
      TabOrder = 2
    end
    object Label_MasksInclude: TLabel
      Left = 14
      Height = 15
      Top = 6
      Width = 42
      Caption = 'Include:'
      ParentColor = False
    end
    object Edit_MasksExclude: TEdit
      Left = 14
      Height = 25
      Top = 72
      Width = 265
      Anchors = [akTop, akLeft, akRight]
      Font.Height = 243
      ParentFont = False
      TabOrder = 1
      OnChange = Edit_MasksExcludeChange
    end
    object Label_MasksExclude: TLabel
      Left = 14
      Height = 15
      Top = 55
      Width = 44
      Caption = 'Exclude:'
      ParentColor = False
    end
  end
  object CheckBox_SaveAsDefault: TCheckBox
    Left = 32
    Height = 19
    Top = 416
    Width = 195
    Caption = 'Save as default for future sessions'
    TabOrder = 4
  end
end

