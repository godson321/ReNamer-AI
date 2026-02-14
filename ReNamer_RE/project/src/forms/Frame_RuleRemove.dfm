object Frame_RuleRemove: TFrame_RuleRemove
  Left = 0
  Height = 230
  Top = 0
  Width = 400
  ClientHeight = 230
  ClientWidth = 400
  TabOrder = 0
  DesignLeft = 384
  DesignTop = 235
  object SpeedButton_RemoveAddSeparator: TSpeedButton
    Left = 354
    Height = 25
    Hint = 'Separate multiple items'
    Top = 24
    Width = 23
    Anchors = [akTop, akRight]
    Flat = True
    Glyph.Data = {Binary: 734 bytes}
    OnClick = SpeedButton_RemoveAddSeparatorClick
    ShowHint = True
    ParentShowHint = False
  end
  object Label_RemoveWhat: TLabel
    Left = 16
    Height = 15
    Top = 27
    Width = 46
    Caption = 'Remove:'
    ParentColor = False
  end
  object Label_RemoveOccurances: TLabel
    Left = 80
    Height = 15
    Top = 64
    Width = 69
    Caption = 'Occurrences:'
    ParentColor = False
  end
  object CheckBox_SkipExtension: TCheckBox
    Left = 232
    Height = 19
    Top = 128
    Width = 94
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 6
  end
  object RadioButton_RemoveLast: TRadioButton
    Left = 80
    Height = 19
    Top = 124
    Width = 39
    Caption = 'Last'
    TabOrder = 3
  end
  object RadioButton_RemoveAll: TRadioButton
    Left = 80
    Height = 19
    Top = 84
    Width = 32
    Caption = 'All'
    Checked = True
    TabOrder = 1
    TabStop = True
  end
  object Edit_Remove: TEdit
    Left = 80
    Height = 25
    Top = 24
    Width = 273
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ParentFont = False
    TabOrder = 0
  end
  object RadioButton_RemoveFirst: TRadioButton
    Left = 80
    Height = 19
    Top = 104
    Width = 40
    Caption = 'First'
    TabOrder = 2
  end
  object CheckBox_CaseSensitive: TCheckBox
    Left = 232
    Height = 19
    Top = 80
    Width = 91
    Caption = 'Case sensitive'
    TabOrder = 4
  end
  object CheckBox_UseWildcards: TCheckBox
    Left = 24
    Height = 19
    Top = 168
    Width = 245
    Caption = 'Interpret symbols '?', '*', '[', ']' as wildcards?'
    TabOrder = 7
    OnClick = CheckBox_UseWildcardsClick
  end
  object CheckBox_WholeWordsOnly: TCheckBox
    Left = 232
    Height = 19
    Top = 104
    Width = 113
    Caption = 'Whole words only'
    TabOrder = 5
  end
end

