object Frame_RuleReformatDate: TFrame_RuleReformatDate
  Left = 0
  Height = 274
  Top = 0
  Width = 409
  ClientHeight = 274
  ClientWidth = 409
  TabOrder = 0
  DesignLeft = 367
  DesignTop = 288
  object Label_InputFormats: TLabel
    Left = 24
    Height = 15
    Top = 16
    Width = 125
    Caption = 'Find date/time formats:'
    ParentColor = False
  end
  object Label_OutputFormat: TLabel
    Left = 24
    Height = 15
    Top = 64
    Width = 153
    Caption = 'Convert to date/time format:'
    ParentColor = False
  end
  object CheckBox_UseCustomShortMonths: TCheckBox
    Left = 24
    Height = 19
    Top = 176
    Width = 157
    Caption = 'Use custom short months:'
    TabOrder = 4
    OnClick = CheckBox_UseCustomShortMonthsClick
  end
  object CheckBox_UseCustomLongMonths: TCheckBox
    Left = 24
    Height = 19
    Top = 205
    Width = 154
    Caption = 'Use custom long months:'
    TabOrder = 6
    OnClick = CheckBox_UseCustomLongMonthsClick
  end
  object CheckBox_WholeWordsOnly: TCheckBox
    Left = 24
    Height = 19
    Top = 120
    Width = 162
    Caption = 'Match as whole words only'
    Checked = True
    State = cbChecked
    TabOrder = 2
  end
  object CheckBox_SkipExtension: TCheckBox
    Left = 24
    Height = 19
    Top = 147
    Width = 94
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 3
  end
  object ComboBox_CustomShortMonths: TComboBox
    Left = 216
    Height = 23
    Top = 176
    Width = 176
    Anchors = [akTop, akLeft, akRight]
    AutoCompleteText = [cbactSearchAscending]
    ItemHeight = 15
    ParentFont = False
    TabOrder = 5
  end
  object ComboBox_CustomLongMonths: TComboBox
    Left = 216
    Height = 23
    Top = 205
    Width = 176
    Anchors = [akTop, akLeft, akRight]
    AutoCompleteText = [cbactSearchAscending]
    ItemHeight = 15
    TabOrder = 7
  end
  object SpeedButton_Help: TSpeedButton
    Left = 371
    Height = 24
    Top = 35
    Width = 23
    Anchors = [akTop, akRight]
    Flat = True
    Glyph.Data = {Binary: 1082 bytes}
    OnClick = SpeedButton_HelpClick
  end
  object ComboBox_SourceFormats: TComboBox
    Left = 24
    Height = 25
    Top = 35
    Width = 346
    Anchors = [akTop, akLeft, akRight]
    AutoCompleteText = [cbactSearchAscending]
    Font.Height = 243
    ItemHeight = 17
    ParentFont = False
    TabOrder = 0
  end
  object ComboBox_TargetFormat: TComboBox
    Left = 24
    Height = 25
    Top = 83
    Width = 346
    Anchors = [akTop, akLeft, akRight]
    AutoCompleteText = [cbactSearchAscending]
    Font.Height = 243
    ItemHeight = 17
    ParentFont = False
    TabOrder = 1
  end
  object CheckBox_AdjustTime: TCheckBox
    Left = 24
    Height = 19
    Top = 238
    Width = 98
    Caption = 'Adjust time by:'
    TabOrder = 8
    OnClick = CheckBox_AdjustTimeClick
  end
  object SpinEdit_AdjustTimeBy: TSpinEdit
    Left = 216
    Height = 23
    Top = 236
    Width = 58
    MaxValue = 1000000
    MinValue = -1000000
    TabOrder = 9
  end
  object ComboBox_AdjustTimePart: TComboBox
    Left = 282
    Height = 23
    Top = 236
    Width = 112
    Anchors = [akTop, akLeft, akRight]
    ItemHeight = 15
    Style = csDropDownList
    TabOrder = 10
  end
  object PopupMenu_Help: TPopupMenu
    Left = 320
    Top = 112
    object MenuItem_AddSeparator: TMenuItem
      Caption = 'Separate multiple items'
      OnClick = MenuItem_AddSeparatorClick
    end
    object MenuItem_Break1: TMenuItem
      Caption = '-'
    end
    object MenuItem_DateTimeFormatHelp: TMenuItem
      Caption = 'Date/time format reference (online)'
      OnClick = MenuItem_DateTimeFormatHelpClick
    end
  end
end

