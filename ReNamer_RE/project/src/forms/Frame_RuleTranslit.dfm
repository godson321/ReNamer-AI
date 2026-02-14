object Frame_RuleTranslit: TFrame_RuleTranslit
  Left = 0
  Height = 260
  Top = 0
  Width = 440
  ClientHeight = 260
  ClientWidth = 440
  TabOrder = 0
  DesignLeft = 395
  DesignTop = 280
  object Memo_Alphabet: TMemo
    Left = 24
    Height = 202
    Top = 40
    Width = 113
    Anchors = [akTop, akLeft, akRight, akBottom]
    Font.Height = 243
    ParentFont = False
    ScrollBars = ssAutoBoth
    TabOrder = 1
    WordWrap = False
  end
  object BitBtn_Translits: TBitBtn
    Left = 24
    Height = 25
    Top = 15
    Width = 113
    Anchors = [akTop, akLeft, akRight]
    Caption = 'Alphabet'
    Glyph.Data = {Binary: 1082 bytes}
    OnClick = BitBtn_TranslitsClick
    TabOrder = 0
  end
  object Panel_Options: TPanel
    Left = 142
    Height = 260
    Top = 0
    Width = 298
    Align = alRight
    BevelOuter = bvNone
    ClientHeight = 260
    ClientWidth = 298
    TabOrder = 2
    object Label2: TLabel
      Left = 16
      Height = 15
      Top = 16
      Width = 51
      Caption = 'Direction:'
      ParentColor = False
    end
    object Label_HintText: TLabel
      Left = 16
      Height = 80
      Top = 112
      Width = 264
      AutoSize = False
      Caption = 'Alphabet is a set of couples represented by letters and separated with an equal sign; they stand for translation of non-english characters to their english representation.'
      ParentColor = False
      WordWrap = True
    end
    object Label3: TLabel
      Left = 16
      Height = 15
      Top = 96
      Width = 26
      Caption = 'Hint:'
      ParentColor = False
    end
    object RadioButton_DirectionBackward: TRadioButton
      Left = 24
      Height = 19
      Top = 64
      Width = 69
      Caption = 'Backward'
      TabOrder = 1
    end
    object RadioButton_DirectionForward: TRadioButton
      Left = 24
      Height = 19
      Top = 40
      Width = 61
      Caption = 'Forward'
      Checked = True
      TabOrder = 0
      TabStop = True
    end
    object CheckBox_SkipExtension: TCheckBox
      Left = 16
      Height = 19
      Top = 223
      Width = 94
      Caption = 'Skip extension'
      Checked = True
      State = cbChecked
      TabOrder = 3
    end
    object CheckBox_AdjustCase: TCheckBox
      Left = 16
      Height = 19
      Top = 200
      Width = 133
      Caption = 'Auto case adjustment'
      Checked = True
      State = cbChecked
      TabOrder = 2
    end
  end
  object PopupMenu_Translits: TPopupMenu
    OnPopup = PopupMenu_TranslitsPopup
    Left = 304
    Top = 32
  end
end

