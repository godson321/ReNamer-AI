object Frame_RuleCleanUp: TFrame_RuleCleanUp
  Left = 0
  Height = 298
  Top = 0
  Width = 440
  ClientHeight = 298
  ClientWidth = 440
  TabOrder = 0
  DesignLeft = 382
  DesignTop = 261
  object CheckBox_SpacesFix: TCheckBox
    Left = 24
    Height = 19
    Top = 128
    Width = 380
    Caption = 'Fix spaces: only one space at a time, no spaces on sides of basename'
    Checked = True
    State = cbChecked
    TabOrder = 2
  end
  object CheckBox_SkipExtension: TCheckBox
    Left = 24
    Height = 19
    Top = 269
    Width = 94
    Caption = 'Skip extension'
    Checked = True
    State = cbChecked
    TabOrder = 8
  end
  object CheckBox_PrepareForSharePoint: TCheckBox
    Left = 24
    Height = 19
    Top = 246
    Width = 258
    Caption = 'Prepare for SharePoint (always inc. extension)'
    TabOrder = 7
  end
  object CB_InsertSpaceBeforeCapitals: TCheckBox
    Left = 24
    Height = 19
    Top = 223
    Width = 331
    Caption = 'Insert a space in front of capitalized letters (e.g. CamelCase)'
    TabOrder = 6
  end
  object CB_NormalizeSpaces: TCheckBox
    Left = 24
    Height = 19
    Top = 151
    Width = 372
    Caption = 'Normalize unicode spaces by replacing them with a standard space'
    Checked = True
    State = cbChecked
    TabOrder = 3
  end
  object GroupBox_Brackets: TGroupBox
    Left = 16
    Height = 37
    Top = 8
    Width = 408
    Caption = 'Strip out content of brackets:'
    ClientHeight = 17
    ClientWidth = 404
    TabOrder = 0
    object CheckBox_BracketsRound: TCheckBox
      Left = 192
      Height = 19
      Top = 251
      Width = 35
      Caption = '(...)'
      TabOrder = 0
    end
    object CheckBox_BracketsSquare: TCheckBox
      Left = 264
      Height = 19
      Top = 251
      Width = 35
      Caption = '[...]'
      TabOrder = 1
    end
    object CheckBox_BracketsCurvy: TCheckBox
      Left = 336
      Height = 19
      Top = 251
      Width = 35
      Caption = '{...}'
      TabOrder = 2
    end
  end
  object GroupBox1: TGroupBox
    Left = 16
    Height = 67
    Top = 48
    Width = 408
    Caption = 'Replace these characters with spaces:'
    ClientHeight = 47
    ClientWidth = 404
    TabOrder = 1
    object CheckBox_SpacesComma: TCheckBox
      Left = 96
      Height = 19
      Top = 1
      Width = 73
      Caption = ', (comma)'
      TabOrder = 1
    end
    object CheckBox_SpacesWeb: TCheckBox
      Left = 344
      Height = 19
      Top = 1
      Width = 40
      Caption = '%20'
      TabOrder = 5
    end
    object CheckBox_SpacesUnderscore: TCheckBox
      Left = 200
      Height = 19
      Top = 1
      Width = 23
      Caption = '_'
      TabOrder = 2
    end
    object CheckBox_SpacesSkipVersions: TCheckBox
      Left = 14
      Height = 19
      Top = 22
      Width = 289
      Caption = 'Skip number sequences, for example version 1.2.3.4'
      TabOrder = 6
    end
    object CheckBox_SpacesPlus: TCheckBox
      Left = 248
      Height = 19
      Top = 1
      Width = 26
      Caption = '+'
      TabOrder = 3
    end
    object CheckBox_SpacesHyphen: TCheckBox
      Left = 296
      Height = 19
      Top = 1
      Width = 23
      Caption = '-'
      TabOrder = 4
    end
    object CheckBox_SpacesDot: TCheckBox
      Left = 14
      Height = 19
      Top = 1
      Width = 50
      Caption = '. (dot)'
      TabOrder = 0
    end
  end
  object CheckBox_StripUnicodeMarks: TCheckBox
    Left = 24
    Height = 19
    Top = 200
    Width = 291
    Caption = 'Strip unicode marks (combining, diacritics, accents)'
    TabOrder = 5
  end
  object CheckBox_StripUnicodeEmoji: TCheckBox
    Left = 24
    Height = 19
    Top = 176
    Width = 121
    Caption = 'Strip unicode emoji'
    TabOrder = 4
  end
end

