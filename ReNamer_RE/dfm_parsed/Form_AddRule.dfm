object Form_AddRule: TForm_AddRule
  Left = 426
  Height = 385
  Top = 247
  Width = 617
  AllowDropFiles = True
  Caption = 'Add Rule'
  ClientHeight = 385
  ClientWidth = 617
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  OnDropFiles = FormDropFiles
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object ListBox_Rules: TListBox
    Left = 16
    Height = 316
    Top = 16
    Width = 129
    Anchors = [akTop, akLeft, akBottom]
    Font.Height = 243
    ItemHeight = 0
    ParentFont = False
    PopupMenu = PopupMenu_GlobalRules
    TabOrder = 0
    OnClick = ListBox_RulesClick
    OnDblClick = ListBox_RulesDblClick
  end
  object BitBtn_Add: TBitBtn
    Left = 16
    Height = 33
    Top = 342
    Width = 409
    Anchors = [akLeft, akRight, akBottom]
    Caption = 'Add Rule'
    Default = True
    Glyph.Data = {Binary: 734 bytes}
    ModalResult = 6
    OnClick = BitBtn_AddClick
    TabOrder = 2
  end
  object BitBtn_Close: TBitBtn
    Left = 440
    Height = 33
    Top = 342
    Width = 161
    Anchors = [akRight, akBottom]
    Cancel = True
    Caption = 'Close'
    ModalResult = 2
    OnClick = BitBtn_CloseClick
    TabOrder = 3
  end
  object GroupBox_Config: TGroupBox
    Left = 152
    Height = 321
    Top = 11
    Width = 449
    Anchors = [akTop, akLeft, akRight, akBottom]
    Caption = 'Configuration:'
    TabOrder = 1
  end
  object PopupMenu_GlobalRules: TPopupMenu
    Left = 64
    Top = 64
    object MenuItem_RuleDescription: TMenuItem
      Caption = 'Description'
      OnClick = MenuItem_RuleDescriptionClick
    end
  end
end

