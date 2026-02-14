object Frame_RuleReplace: TFrame_RuleReplace
  Left = 0
  Height = 230
  Top = 0
  Width = 400
  ClientHeight = 230
  ClientWidth = 400
  TabOrder = 0
  DesignLeft = 373
  DesignTop = 285
  object SpeedButton_ReplaceAddSeparator: TSpeedButton
    Left = 354
    Height = 25
    Hint = 'Separate multiple items'
    Top = 24
    Width = 23
    Anchors = [akTop, akRight]
    Flat = True
    Glyph.Data = {Binary: 734 bytes}
    OnClick = SpeedButton_ReplaceAddSeparatorClick
    ShowHint = True
    ParentShowHint = False
  end
  object SpeedButton_ReplaceWithMetaTag: TSpeedButton
    Left = 354
    Height = 25
    Hint = 'Insert Meta Tag (Ctrl+Ins)'
    Top = 56
    Width = 23
    Anchors = [akTop, akRight]
    Flat = True
    Glyph.Data = {Binary: 1082 bytes}
    OnClick = SpeedButton_ReplaceWithMetaTagClick
    ShowHint = True
    ParentShowHint = False
  end
  object Label_ReplaceOccurances: TLabel
    Left = 80
    Height = 15
    Top = 91
    Width = 69
    Caption = 'Occurrences:'
    ParentColor = False
  end
  object Label_ReplaceWith: TLabel
    Left = 16
    Height = 15
    Top = 59
    Width = 44
    Caption = 'Replace:'
    ParentColor = False
  end
  object Label_ReplaceWhat: TLabel
    Left = 16
    Height = 15
    Top = 27
    Width = 26
    Caption = 'Find:'
    ParentColor = False
  end
  object RadioButton_ReplaceLast: TRadioButton
    Left = 80
    Height = 19
    Top = 151
    Width = 39
    Caption = 'Last'
    TabOrder = 4
  end
  object RadioButton_ReplaceFirst: TRadioButton
    Left = 80
    Height = 19
    Top = 131
    Width = 40
    Caption = 'First'
    TabOrder = 3
  end
  object RadioButton_ReplaceAll: TRadioButton
    Left = 80
    Height = 19
    Top = 111
    Width = 32
    Caption = 'All'
    Checked = True
    TabOrder = 2
    TabStop = True
  end
  object Edit_ReplaceWhat: TEdit
    Left = 80
    Height = 25
    Top = 24
    Width = 273
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ParentFont = False
    TabOrder = 0
  end
  object Edit_ReplaceWith: TEdit
    Left = 80
    Height = 25
    Top = 56
    Width = 273
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ParentFont = False
    TabOrder = 1
    OnKeyDown = Edit_ReplaceWithKeyDown
  end
  object CheckBox_SkipExtension: TCheckBox
    Left = 232
    Height = 19
    Top = 152
    Width = 94
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 7
  end
  object CheckBox_CaseSensitive: TCheckBox
    Left = 232
    Height = 19
    Top = 104
    Width = 91
    Caption = 'Case sensitive'
    TabOrder = 5
  end
  object CheckBox_UseWildcards: TCheckBox
    Left = 16
    Height = 19
    Top = 192
    Width = 339
    Caption = 'Interpret '?', '*', '[', ']' as wildcards and '$n' as backreferences?'
    TabOrder = 8
    OnClick = CheckBox_UseWildcardsClick
  end
  object CheckBox_WholeWordsOnly: TCheckBox
    Left = 232
    Height = 19
    Top = 128
    Width = 113
    Caption = 'Whole words only'
    TabOrder = 6
  end
end

