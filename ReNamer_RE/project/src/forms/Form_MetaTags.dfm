object Form_MetaTags: TForm_MetaTags
  Left = 530
  Height = 299
  Top = 361
  Width = 321
  BorderIcons = []
  Caption = 'Meta Tags'
  ClientHeight = 299
  ClientWidth = 321
  OnCreate = FormCreate
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object Label_Note: TLabel
    Left = 216
    Height = 112
    Top = 152
    Width = 97
    Anchors = [akTop, akRight]
    AutoSize = False
    Caption = 'Note: Most of the tags can only work for files, not folders.'
    ParentColor = False
    WordWrap = True
  end
  object ListBox_MetaTags: TListBox
    Left = 16
    Height = 217
    Top = 48
    Width = 185
    Anchors = [akTop, akLeft, akRight, akBottom]
    IntegralHeight = True
    ItemHeight = 0
    TabOrder = 1
    OnClick = ListBox_MetaTagsClick
    OnDblClick = ListBox_MetaTagsDblClick
  end
  object BitBtn_Insert: TBitBtn
    Left = 216
    Height = 41
    Top = 48
    Width = 89
    Anchors = [akTop, akRight]
    Caption = 'Insert'
    Default = True
    Glyph.Data = {Binary: 1082 bytes}
    ModalResult = 1
    OnClick = BitBtn_InsertClick
    TabOrder = 2
  end
  object BitBtn_Cancel: TBitBtn
    Left = 216
    Height = 33
    Top = 96
    Width = 89
    Anchors = [akTop, akRight]
    Cancel = True
    Caption = 'Cancel'
    ModalResult = 2
    OnClick = BitBtn_CancelClick
    TabOrder = 3
  end
  object StatusBar: TStatusBar
    Left = 0
    Height = 23
    Top = 276
    Width = 321
<
    Panels = >
  end
  object ComboBox_Filter: TComboBox
    Left = 16
    Height = 25
    Top = 16
    Width = 289
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ItemHeight = 17
    ParentFont = False
    TabOrder = 0
    OnChange = ComboBox_FilterChange
  end
end

