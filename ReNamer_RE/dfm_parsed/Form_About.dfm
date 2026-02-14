object Form_About: TForm_About
  Left = 527
  Height = 321
  Top = 296
  Width = 337
  BorderIcons = [biSystemMenu, biMinimize]
  BorderStyle = bsDialog
  Caption = 'About'
  ClientHeight = 321
  ClientWidth = 337
  KeyPreview = True
  OnCreate = FormCreate
  OnKeyPress = FormKeyPress
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object Label_Author2: TLabel
    Left = 32
    Height = 15
    Top = 112
    Width = 42
    Caption = 'Author:'
    Font.Style = [fsBold]
    ParentColor = False
    ParentFont = False
  end
  object Label_Author: TLabel
    Left = 128
    Height = 15
    Top = 112
    Width = 64
    Caption = 'den4b Team'
    ParentColor = False
  end
  object Label_Email2: TLabel
    Left = 32
    Height = 15
    Top = 136
    Width = 32
    Caption = 'Email:'
    Font.Style = [fsBold]
    ParentColor = False
    ParentFont = False
  end
  object Label_Email: TLabel
    Cursor = crHandPoint
    Left = 128
    Height = 15
    Top = 136
    Width = 112
    Caption = 'support@den4b.com'
    Font.Color = clBlue
    ParentColor = False
    ParentFont = False
    OnClick = Label_EmailClick
  end
  object Label_Website2: TLabel
    Left = 32
    Height = 15
    Top = 160
    Width = 49
    Caption = 'Website:'
    Font.Style = [fsBold]
    ParentColor = False
    ParentFont = False
  end
  object Label_Website: TLabel
    Cursor = crHandPoint
    Left = 128
    Height = 15
    Top = 160
    Width = 90
    Caption = 'www.den4b.com'
    Font.Color = clBlue
    ParentColor = False
    ParentFont = False
    OnClick = Label_WebsiteClick
  end
  object Panel_Header: TPanel
    Left = 0
    Height = 97
    Top = 0
    Width = 337
    Align = alTop
    BevelOuter = bvNone
    ClientHeight = 97
    ClientWidth = 337
    Color = clWhite
    ParentBackground = False
    ParentColor = False
    TabOrder = 0
    OnMouseUp = Panel_HeaderMouseUp
    object Bevel: TBevel
      Left = 0
      Height = 9
      Top = 88
      Width = 337
      Align = alBottom
      Shape = bsBottomLine
    end
    object Label_Title: TLabel
      Left = 126
      Height = 40
      Top = 8
      Width = 118
      Caption = 'ReNamer'
      Font.Height = 227
      ParentColor = False
      ParentFont = False
    end
    object Image: TImage
      Left = 22
      Height = 81
      Top = 6
      Width = 81
      Center = True
      Picture.Data = {Binary: 5597 bytes}
    end
    object Label_Version: TLabel
      Left = 126
      Height = 15
      Top = 54
      Width = 41
      Caption = 'Version:'
      ParentColor = False
    end
    object Label_Date: TLabel
      Left = 126
      Height = 15
      Top = 72
      Width = 27
      Caption = 'Date:'
      ParentColor = False
    end
  end
  object Memo_License: TMemo
    Left = 0
    Height = 129
    Top = 192
    Width = 337
    Align = alBottom
    ReadOnly = True
    ScrollBars = ssVertical
    TabOrder = 1
  end
end

