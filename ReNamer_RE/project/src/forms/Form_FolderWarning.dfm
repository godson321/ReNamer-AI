object Form_FolderWarning: TForm_FolderWarning
  Left = 519
  Height = 339
  Top = 284
  Width = 401
  BorderStyle = bsDialog
  Caption = 'Warning: Renaming Folders'
  ClientHeight = 339
  ClientWidth = 401
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object Image_Bad: TImage
    Left = 16
    Height = 112
    Top = 168
    Width = 176
    Center = True
    Picture.Data = {Binary: 20130 bytes}
  end
  object Image_Good: TImage
    Left = 209
    Height = 112
    Top = 168
    Width = 176
    Center = True
    Picture.Data = {Binary: 20578 bytes}
  end
  object Label_Warning: TLabel
    Left = 16
    Height = 80
    Top = 16
    Width = 369
    AutoSize = False
    Caption = 'Renaming a folder also affects all of its content. A problem can occur if you try to rename folders and their content in a single run. Items in the renaming list are processed from top to bottom. The order of items in this case is extremely important for successful renaming.'
    ParentColor = False
    WordWrap = True
  end
  object CB_NeverShowWarning: TCheckBox
    Left = 128
    Height = 19
    Top = 299
    Width = 148
    Caption = 'Never show this warning'
    TabOrder = 1
  end
  object BB_Continue: TBitBtn
    Left = 296
    Height = 25
    Top = 296
    Width = 89
    Caption = 'Continue'
    Default = True
    OnClick = BB_ContinueClick
    TabOrder = 2
  end
  object BB_Cancel: TBitBtn
    Left = 16
    Height = 25
    Top = 296
    Width = 89
    Cancel = True
    Caption = 'Cancel'
    OnClick = BB_CancelClick
    TabOrder = 0
  end
  object Label_Warning2: TLabel
    Left = 16
    Height = 64
    Top = 96
    Width = 369
    AutoSize = False
    Caption = 'Parent folders must always appear below their contained items. This can be easily achieved by sorting items in descending order by the Folder or Path column.'
    ParentColor = False
    WordWrap = True
  end
end

