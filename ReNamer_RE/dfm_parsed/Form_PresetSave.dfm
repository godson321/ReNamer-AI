object Form_PresetSave: TForm_PresetSave
  Left = 544
  Height = 257
  Top = 339
  Width = 289
  BorderIcons = [biSystemMenu]
  Caption = 'Save Preset'
  ClientHeight = 257
  ClientWidth = 289
  KeyPreview = True
  OnCreate = FormCreate
  OnHide = FormHide
  OnKeyPress = FormKeyPress
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object Label1: TLabel
    Left = 16
    Height = 15
    Top = 93
    Width = 133
    Caption = 'Overwrite existing preset:'
    ParentColor = False
  end
  object LE_Name: TLabeledEdit
    Left = 16
    Height = 25
    Top = 32
    Width = 257
    Anchors = [akTop, akLeft, akRight]
    EditLabel.Height = 15
    EditLabel.Width = 257
    EditLabel.Caption = 'Preset Name:'
    EditLabel.ParentColor = False
    Font.Height = 243
    ParentFont = False
    TabOrder = 0
    OnChange = LE_NameChange
  end
  object BitBtn_Save: TBitBtn
    Left = 16
    Height = 25
    Top = 216
    Width = 257
    Anchors = [akLeft, akBottom]
    Caption = '&Save'
    Default = True
    Glyph.Data = {Binary: 1082 bytes}
    ModalResult = 6
    TabOrder = 3
  end
  object LB_Presets: TListBox
    Left = 16
    Height = 89
    Top = 112
    Width = 257
    Anchors = [akTop, akLeft, akRight, akBottom]
    ItemHeight = 0
    ParentColor = True
    TabOrder = 2
    OnClick = LB_PresetsClick
  end
  object CB_PresetSaveFilters: TCheckBox
    Left = 16
    Height = 19
    Top = 58
    Width = 202
    Caption = 'Save Filter Settings with the preset?'
    TabOrder = 1
  end
end

