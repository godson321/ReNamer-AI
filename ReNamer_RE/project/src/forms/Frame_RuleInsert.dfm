object Frame_RuleInsert: TFrame_RuleInsert
  Left = 0
  Height = 263
  Top = 0
  Width = 400
  ClientHeight = 263
  ClientWidth = 400
  TabOrder = 0
  DesignLeft = 394
  DesignTop = 270
  object Label_InsertWhere: TLabel
    Left = 16
    Height = 15
    Top = 106
    Width = 37
    Caption = 'Where:'
    ParentColor = False
  end
  object Label_InsertWhat: TLabel
    Left = 16
    Height = 15
    Top = 27
    Width = 32
    Caption = 'Insert:'
    ParentColor = False
  end
  object CheckBox_InsertSkipExtension: TCheckBox
    Left = 80
    Height = 19
    Top = 228
    Width = 94
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 8
  end
  object Edit_Insert: TEdit
    Left = 80
    Height = 25
    Top = 24
    Width = 289
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ParentFont = False
    TabOrder = 0
    OnKeyDown = Edit_InsertKeyDown
  end
  object RadioButton_InsertPrefix: TRadioButton
    Left = 80
    Height = 19
    Top = 80
    Width = 48
    Caption = 'Prefix'
    Checked = True
    TabOrder = 2
    TabStop = True
  end
  object RadioButton_InsertSuffix: TRadioButton
    Left = 80
    Height = 19
    Top = 104
    Width = 48
    Caption = 'Suffix'
    TabOrder = 3
  end
  object SpinEdit_InsertPosition: TSpinEdit
    Left = 208
    Height = 23
    Top = 126
    Width = 57
    MaxValue = 260
    MinValue = 1
    OnChange = SpinEdit_InsertPositionChange
    OnKeyDown = SpinEditKeyDown
    TabOrder = 9
    Value = 1
  end
  object RadioButton_InsertPosition: TRadioButton
    Left = 80
    Height = 19
    Top = 128
    Width = 64
    Caption = 'Position:'
    TabOrder = 4
  end
  object RadioButton_InsertAfterText: TRadioButton
    Left = 80
    Height = 19
    Top = 152
    Width = 70
    Caption = 'After text:'
    TabOrder = 5
  end
  object Edit_InsertAfterText: TEdit
    Left = 208
    Height = 25
    Top = 150
    Width = 137
    Font.Height = 243
    ParentFont = False
    TabOrder = 11
    OnChange = Edit_InsertAfterTextChange
  end
  object RadioButton_InsertBeforeText: TRadioButton
    Left = 80
    Height = 19
    Top = 176
    Width = 78
    Caption = 'Before text:'
    TabOrder = 6
  end
  object Edit_InsertBeforeText: TEdit
    Left = 208
    Height = 25
    Top = 174
    Width = 137
    Font.Height = 243
    ParentFont = False
    TabOrder = 12
    OnChange = Edit_InsertBeforeTextChange
  end
  object CheckBox_InsertRight: TCheckBox
    Left = 272
    Height = 19
    Top = 128
    Width = 84
    Caption = 'Right-to-left'
    TabOrder = 10
    OnClick = CheckBox_InsertRightClick
  end
  object RadioButton_ReplaceCurrentName: TRadioButton
    Left = 80
    Height = 19
    Top = 200
    Width = 133
    Caption = 'Replace current name'
    TabOrder = 7
  end
  object BitBtn_InsertMetaTag: TBitBtn
    Left = 80
    Height = 26
    Hint = 'Insert Meta Tag (Ctrl+Ins)'
    Top = 49
    Width = 127
    AutoSize = True
    Caption = 'Insert Meta Tag'
    Glyph.Data = {Binary: 1082 bytes}
    OnClick = BitBtn_InsertMetaTagClick
    ParentShowHint = False
    ShowHint = True
    TabOrder = 1
  end
end

