object Form_PresetsLinks: TForm_PresetsLinks
  Left = 547
  Height = 161
  Top = 416
  Width = 345
  BorderStyle = bsDialog
  Caption = 'Presets Links'
  ClientHeight = 161
  ClientWidth = 345
  KeyPreview = True
  OnKeyPress = FormKeyPress
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object Label_Folder: TLabel
    Left = 16
    Height = 15
    Top = 16
    Width = 298
    Caption = 'Please specify a folder where you want to place the links:'
    ParentColor = False
  end
  object Label_LinkType: TLabel
    Left = 16
    Height = 15
    Top = 76
    Width = 68
    Caption = 'Type of links:'
    ParentColor = False
  end
  object SB_Browse: TSpeedButton
    Left = 290
    Height = 25
    Hint = 'Browse for Folder'
    Top = 32
    Width = 23
    Flat = True
    Glyph.Data = {Binary: 1082 bytes}
    OnClick = SB_BrowseClick
    ShowHint = True
    ParentShowHint = False
  end
  object SB_UseSendTo: TSpeedButton
    Left = 313
    Height = 25
    Hint = 'Use "Send To" Folder'
    Top = 32
    Width = 23
    Flat = True
    Glyph.Data = {Binary: 1214 bytes}
    OnClick = SB_UseSendToClick
    ShowHint = True
    ParentShowHint = False
  end
  object Edit_Folder: TEdit
    Left = 16
    Height = 25
    Top = 32
    Width = 273
    Font.Height = 243
    ParentFont = False
    TabOrder = 0
  end
  object RB_Load: TRadioButton
    Left = 120
    Height = 19
    Top = 72
    Width = 105
    Caption = 'Load with Preset'
    Checked = True
    TabOrder = 1
    TabStop = True
  end
  object RB_Rename: TRadioButton
    Left = 120
    Height = 19
    Top = 90
    Width = 122
    Caption = 'Rename with Preset'
    TabOrder = 2
  end
  object BB_CreateLinks: TBitBtn
    Left = 16
    Height = 25
    Top = 120
    Width = 313
    Caption = 'Create Links'
    Glyph.Data = {Binary: 1354 bytes}
    OnClick = BB_CreateLinksClick
    TabOrder = 3
  end
end

