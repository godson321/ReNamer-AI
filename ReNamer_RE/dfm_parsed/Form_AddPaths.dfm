object Form_AddPaths: TForm_AddPaths
  Left = 498
  Height = 226
  Top = 317
  Width = 393
  BorderIcons = [biSystemMenu]
  Caption = 'Add Paths'
  ClientHeight = 226
  ClientWidth = 393
  KeyPreview = True
  OnCreate = FormCreate
  OnKeyPress = FormKeyPress
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object Edit_WorkingDir: TEdit
    Left = 12
    Height = 23
    Top = 152
    Width = 369
    Align = alBottom
    BorderSpacing.Left = 12
    BorderSpacing.Top = 4
    BorderSpacing.Right = 12
    BorderSpacing.Bottom = 6
    TabOrder = 2
  end
  object Label_EnterPaths: TLabel
    Left = 12
    Height = 15
    Top = 12
    Width = 369
    Align = alTop
    BorderSpacing.Left = 12
    BorderSpacing.Top = 12
    BorderSpacing.Right = 12
    BorderSpacing.Bottom = 4
    Caption = 'Enter file paths:'
    ParentColor = False
  end
  object Memo_Paths: TMemo
    Left = 12
    Height = 92
    Top = 31
    Width = 369
    Align = alClient
    BorderSpacing.Left = 12
    BorderSpacing.Top = 4
    BorderSpacing.Right = 12
    BorderSpacing.Bottom = 6
    ScrollBars = ssAutoBoth
    TabOrder = 0
    WordWrap = False
  end
  object Panel_Buttons: TPanel
    Left = 0
    Height = 45
    Top = 181
    Width = 393
    Align = alBottom
    BevelOuter = bvNone
    ClientHeight = 45
    ClientWidth = 393
    TabOrder = 3
    object BitBtn_Add: TBitBtn
      Left = 72
      Height = 26
      Top = 8
      Width = 115
      Caption = 'Add'
      Default = True
      Glyph.Data = {Binary: 1082 bytes}
      OnClick = BitBtn_AddClick
      TabOrder = 0
    end
    object BitBtn_Cancel: TBitBtn
      Left = 208
      Height = 26
      Top = 8
      Width = 112
      Cancel = True
      Caption = 'Cancel'
      OnClick = BitBtn_CancelClick
      TabOrder = 1
    end
  end
  object CheckBox_WorkingDir: TCheckBox
    Left = 12
    Height = 19
    Top = 129
    Width = 369
    Align = alBottom
    BorderSpacing.Left = 12
    BorderSpacing.Top = 6
    BorderSpacing.Right = 12
    BorderSpacing.Bottom = 4
    Caption = 'Working directory:'
    TabOrder = 1
    OnChange = CheckBox_WorkingDirChange
  end
end

