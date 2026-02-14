object Frame_RuleRearrange: TFrame_RuleRearrange
  Left = 0
  Height = 233
  Top = 0
  Width = 400
  ClientHeight = 233
  ClientWidth = 400
  TabOrder = 0
  DesignLeft = 382
  DesignTop = 228
  object Label2: TLabel
    Left = 24
    Height = 15
    Top = 104
    Width = 68
    Caption = 'New pattern:'
    ParentColor = False
  end
  object Label3: TLabel
    Left = 24
    Height = 54
    Top = 149
    Width = 353
    AutoSize = False
    Caption = 'Hint: Use $1..$N to reference delimited parts in the new pattern, $-1..$-N to reference from the end, $0 for the original name.'
    ParentColor = False
    WordWrap = True
  end
  object SB_AddSeparator: TSpeedButton
    Left = 354
    Height = 25
    Hint = 'Separate multiple items'
    Top = 72
    Width = 23
    Anchors = [akTop, akRight]
    Flat = True
    Glyph.Data = {Binary: 734 bytes}
    OnClick = SB_AddSeparatorClick
    ShowHint = True
    ParentShowHint = False
  end
  object SB_InsertMetaTag: TSpeedButton
    Left = 354
    Height = 25
    Hint = 'Insert Meta Tag (Ctrl+Ins)'
    Top = 123
    Width = 23
    Anchors = [akTop, akRight]
    Flat = True
    Glyph.Data = {Binary: 1082 bytes}
    OnClick = SB_InsertMetaTagClick
    ShowHint = True
    ParentShowHint = False
  end
  object Label1: TLabel
    Left = 24
    Height = 15
    Top = 27
    Width = 58
    Caption = 'Split using:'
    ParentColor = False
  end
  object RB_UsingDelimiters: TRadioButton
    Left = 112
    Height = 19
    Top = 8
    Width = 71
    Caption = 'Delimiters'
    Checked = True
    TabOrder = 0
    TabStop = True
  end
  object RB_UsingPositions: TRadioButton
    Left = 112
    Height = 19
    Top = 26
    Width = 66
    Caption = 'Positions'
    TabOrder = 1
  end
  object Edit_Delimiters: TEdit
    Left = 24
    Height = 25
    Top = 72
    Width = 329
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ParentFont = False
    TabOrder = 3
  end
  object Edit_NewPattern: TEdit
    Left = 24
    Height = 25
    Top = 123
    Width = 329
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ParentFont = False
    TabOrder = 4
    OnKeyDown = Edit_NewPatternKeyDown
  end
  object RB_UsingExactPatternOfDelimiters: TRadioButton
    Left = 112
    Height = 19
    Top = 44
    Width = 156
    Caption = 'Exact pattern of delimiters'
    TabOrder = 2
  end
  object CB_SkipExtension: TCheckBox
    Left = 24
    Height = 19
    Top = 202
    Width = 94
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 5
  end
  object CB_RightToLeft: TCheckBox
    Left = 200
    Height = 19
    Top = 202
    Width = 84
    Caption = 'Right-to-left'
    TabOrder = 6
  end
end

