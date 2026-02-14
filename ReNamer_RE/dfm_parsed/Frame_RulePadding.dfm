object Frame_RulePadding: TFrame_RulePadding
  Left = 0
  Height = 240
  Top = 0
  Width = 409
  ClientHeight = 240
  ClientWidth = 409
  TabOrder = 0
  DesignLeft = 467
  DesignTop = 200
  object GroupBox_NumberSequences: TGroupBox
    Left = 16
    Height = 80
    Top = 8
    Width = 368
    Caption = 'Number sequences'
    ClientHeight = 60
    ClientWidth = 364
    TabOrder = 0
    object SpinEdit_ZeroPaddingLength: TSpinEdit
      Left = 288
      Height = 23
      Top = 6
      Width = 58
      MaxValue = 260
      MinValue = 1
      OnChange = SpinEdit_ZeroPaddingLengthChange
      OnKeyDown = SpinEditKeyDown
      TabOrder = 1
      Value = 1
    end
    object CheckBox_AddZeroPadding: TCheckBox
      Left = 16
      Height = 19
      Top = 8
      Width = 166
      Caption = 'Add zero padding to length:'
      TabOrder = 0
      OnChange = CheckBox_AddZeroPaddingChange
    end
    object CheckBox_RemoveZeroPadding: TCheckBox
      Left = 16
      Height = 19
      Top = 32
      Width = 133
      Caption = 'Remove zero padding'
      TabOrder = 2
      OnChange = CheckBox_RemoveZeroPaddingChange
    end
  end
  object GroupBox_TextPadding: TGroupBox
    Left = 16
    Height = 113
    Top = 96
    Width = 368
    Caption = 'Text padding'
    ClientHeight = 93
    ClientWidth = 364
    TabOrder = 1
    object CheckBox_AddTextPadding: TCheckBox
      Left = 16
      Height = 19
      Top = 8
      Width = 141
      Caption = 'Add padding to length:'
      TabOrder = 0
    end
    object SpinEdit_TextPaddingLength: TSpinEdit
      Left = 288
      Height = 23
      Top = 6
      Width = 58
      MaxValue = 260
      MinValue = 1
      OnChange = SpinEdit_TextPaddingLengthChange
      OnKeyDown = SpinEditKeyDown
      TabOrder = 1
      Value = 1
    end
    object Label_PaddingCharacters: TLabel
      Left = 17
      Height = 15
      Top = 40
      Width = 104
      Caption = 'Padding characters:'
      ParentColor = False
    end
    object Edit_PaddingCharacters: TEdit
      Left = 216
      Height = 23
      Top = 35
      Width = 130
      TabOrder = 2
    end
    object RadioButton_PositionLeft: TRadioButton
      Left = 216
      Height = 19
      Top = 64
      Width = 38
      Caption = 'Left'
      Checked = True
      TabOrder = 3
      TabStop = True
    end
    object RadioButton_PositionRight: TRadioButton
      Left = 288
      Height = 19
      Top = 64
      Width = 46
      Caption = 'Right'
      TabOrder = 4
    end
    object Label_Position: TLabel
      Left = 17
      Height = 15
      Top = 66
      Width = 46
      Caption = 'Position:'
      ParentColor = False
    end
  end
  object CheckBox_SkipExtension: TCheckBox
    Left = 34
    Height = 19
    Top = 216
    Width = 94
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 2
  end
end

