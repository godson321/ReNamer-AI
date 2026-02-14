object Frame_RuleRegEx: TFrame_RuleRegEx
  Left = 0
  Height = 200
  Top = 0
  Width = 400
  ClientHeight = 200
  ClientWidth = 400
  TabOrder = 0
  DesignLeft = 385
  DesignTop = 282
  object SpeedButton_Help: TSpeedButton
    Left = 320
    Height = 44
    Hint = 'Online Help (F1)'
    Top = 109
    Width = 57
    Caption = 'Help'
    Flat = True
    Glyph.Data = {Binary: 1082 bytes}
    Layout = blGlyphTop
    Spacing = 2
    OnClick = SpeedButton_HelpClick
    ShowHint = True
    ParentShowHint = False
  end
  object Label5: TLabel
    Left = 16
    Height = 15
    Top = 59
    Width = 44
    Caption = 'Replace:'
    ParentColor = False
  end
  object Label4: TLabel
    Left = 16
    Height = 15
    Top = 27
    Width = 59
    Caption = 'Expression:'
    ParentColor = False
  end
  object Label1: TLabel
    Left = 88
    Height = 15
    Top = 82
    Width = 233
    Caption = 'Hint: Use $1..$9 to reference subexpressions.'
    ParentColor = False
  end
  object SB_InsertExpression: TSpeedButton
    Left = 354
    Height = 25
    Hint = 'Insert expression (Ctrl+Ins)'
    Top = 24
    Width = 23
    Anchors = [akTop, akRight]
    Flat = True
    Glyph.Data = {Binary: 1082 bytes}
    OnClick = SB_InsertExpressionClick
    ShowHint = True
    ParentShowHint = False
  end
  object CheckBox_CaseSensitive: TCheckBox
    Left = 88
    Height = 19
    Top = 112
    Width = 93
    Caption = 'Case-sensitive'
    TabOrder = 2
    OnKeyDown = AnyControlKeyDown
  end
  object CheckBox_SkipExtension: TCheckBox
    Left = 88
    Height = 19
    Top = 136
    Width = 94
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 3
    OnKeyDown = AnyControlKeyDown
  end
  object Edit_Replace: TEdit
    Left = 88
    Height = 25
    Top = 56
    Width = 289
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ParentFont = False
    TabOrder = 1
    OnKeyDown = AnyControlKeyDown
  end
  object Edit_Expression: TEdit
    Left = 88
    Height = 25
    Top = 24
    Width = 265
    Anchors = [akTop, akLeft, akRight]
    Font.Height = 243
    ParentFont = False
    TabOrder = 0
    OnKeyDown = AnyControlKeyDown
  end
  object PM_InsertExpression: TPopupMenu
    Left = 240
    Top = 120
  end
end

