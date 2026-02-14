object Frame_RuleRandomize: TFrame_RuleRandomize
  Left = 0
  Height = 272
  Top = 0
  Width = 440
  ClientHeight = 272
  ClientWidth = 440
  TabOrder = 0
  DesignLeft = 547
  DesignTop = 267
  object Label_Length: TLabel
    Left = 32
    Height = 15
    Top = 24
    Width = 152
    Caption = 'Length of random sequence:'
    ParentColor = False
  end
  object SpinEdit_Length: TSpinEdit
    Left = 224
    Height = 23
    Top = 21
    Width = 56
    MaxValue = 260
    MinValue = 1
    OnKeyDown = SpinEditKeyDown
    TabOrder = 0
    Value = 1
  end
  object GroupBox_InsertWhere: TGroupBox
    Left = 224
    Height = 144
    Top = 88
    Width = 193
    Caption = 'Insert where:'
    ClientHeight = 124
    ClientWidth = 189
    TabOrder = 3
    object RadioButton_Position: TRadioButton
      Left = 14
      Height = 19
      Top = 48
      Width = 64
      Caption = 'Position:'
      TabOrder = 2
    end
    object RadioButton_Prefix: TRadioButton
      Left = 14
      Height = 19
      Top = 6
      Width = 48
      Caption = 'Prefix'
      Checked = True
      TabOrder = 0
      TabStop = True
    end
    object RadioButton_Suffix: TRadioButton
      Left = 14
      Height = 19
      Top = 27
      Width = 48
      Caption = 'Suffix'
      TabOrder = 1
    end
    object SpinEdit_Position: TSpinEdit
      Left = 120
      Height = 23
      Top = 46
      Width = 49
      MaxValue = 260
      MinValue = 1
      OnChange = SpinEdit_PositionChange
      OnKeyDown = SpinEditKeyDown
      TabOrder = 4
      Value = 1
    end
    object CheckBox_SkipExtension: TCheckBox
      Left = 14
      Height = 19
      Top = 96
      Width = 94
      Caption = 'Skip extension'
      Checked = True
      State = cbChecked
      TabOrder = 5
    end
    object RadioButton_ReplaceCurrentName: TRadioButton
      Left = 14
      Height = 19
      Top = 69
      Width = 133
      Caption = 'Replace current name'
      TabOrder = 3
    end
  end
  object CheckBox_Unique: TCheckBox
    Left = 32
    Height = 19
    Top = 50
    Width = 112
    Caption = 'Unique if possible'
    Checked = True
    State = cbChecked
    TabOrder = 1
  end
  object GroupBox_Characters: TGroupBox
    Left = 16
    Height = 144
    Top = 88
    Width = 185
    Caption = 'Characters to use:'
    ClientHeight = 124
    ClientWidth = 181
    TabOrder = 2
    object CheckBox_UseDigits: TCheckBox
      Left = 16
      Height = 19
      Top = 8
      Width = 77
      Caption = 'Digits (0..9)'
      Checked = True
      State = cbChecked
      TabOrder = 0
    end
    object CheckBox_UseEnglishLetters: TCheckBox
      Left = 16
      Height = 19
      Top = 32
      Width = 119
      Caption = 'English letters (a..z)'
      Checked = True
      State = cbChecked
      TabOrder = 1
    end
    object CheckBox_UseUserDefined: TCheckBox
      Left = 16
      Height = 19
      Top = 56
      Width = 87
      Caption = 'User defined:'
      TabOrder = 2
    end
    object Edit_UserDefined: TEdit
      Left = 16
      Height = 25
      Top = 80
      Width = 144
      Font.Height = 243
      ParentFont = False
      TabOrder = 3
      OnChange = Edit_UserDefinedChange
    end
  end
end

