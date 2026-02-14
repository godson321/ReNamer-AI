object Frame_RuleUserInput: TFrame_RuleUserInput
  Left = 0
  Height = 240
  Top = 0
  Width = 440
  ClientHeight = 240
  ClientWidth = 440
  TabOrder = 0
  DesignLeft = 389
  DesignTop = 288
  object Label1: TLabel
    Left = 16
    Height = 15
    Top = 11
    Width = 232
    Caption = 'Type your new filenames here (one per line):'
    ParentColor = False
  end
  object SB_Options: TSpeedButton
    Left = 336
    Height = 20
    Top = 9
    Width = 89
    Anchors = [akTop, akRight]
    Caption = 'Options'
    Flat = True
    Glyph.Data = {Binary: 1078 bytes}
    OnClick = SB_OptionsClick
  end
  object Memo_Input: TMemo
    Left = 16
    Height = 161
    Top = 32
    Width = 409
    Anchors = [akTop, akLeft, akRight, akBottom]
    Font.Height = 243
    ParentFont = False
    ScrollBars = ssAutoBoth
    TabOrder = 0
    WordWrap = False
    OnKeyDown = Memo_InputKeyDown
  end
  object RB_InsertInFront: TRadioButton
    Left = 16
    Height = 19
    Top = 197
    Width = 197
    Anchors = [akLeft, akBottom]
    Caption = 'Insert in front of the current name'
    TabOrder = 1
  end
  object RB_InsertAfter: TRadioButton
    Left = 16
    Height = 19
    Top = 216
    Width = 168
    Anchors = [akLeft, akBottom]
    Caption = 'Insert after the current name'
    TabOrder = 2
  end
  object RB_Replace: TRadioButton
    Left = 240
    Height = 19
    Top = 197
    Width = 153
    Anchors = [akLeft, akBottom]
    Caption = 'Replace the current name'
    Checked = True
    TabOrder = 3
    TabStop = True
  end
  object CB_SkipExtension: TCheckBox
    Left = 240
    Height = 19
    Top = 216
    Width = 94
    Anchors = [akLeft, akBottom]
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 4
  end
  object PopupMenu_Options: TPopupMenu
    Left = 272
    Top = 56
    object MenuItem_Load: TMenuItem
      Caption = 'Load from Text File'
      OnClick = MenuItem_LoadClick
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object MenuItem_Paste: TMenuItem
      Caption = 'Paste from Clipboard'
      OnClick = MenuItem_PasteClick
    end
  end
end

