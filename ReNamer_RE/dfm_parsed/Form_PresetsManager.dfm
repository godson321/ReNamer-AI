object Form_PresetsManager: TForm_PresetsManager
  Left = 742
  Height = 291
  Top = 333
  Width = 399
  BorderIcons = [biSystemMenu]
  Caption = 'Presets Manager'
  ClientHeight = 291
  ClientWidth = 399
  KeyPreview = True
  OnCreate = FormCreate
  OnKeyDown = FormKeyDown
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object Label_PresetsFolder: TLabel
    Left = 0
    Height = 20
    Top = 43
    Width = 399
    Align = alTop
    AutoSize = False
    Font.Height = 243
    Layout = tlCenter
    ParentColor = False
    ParentFont = False
    OnMouseDown = Label_PresetsFolderMouseDown
  end
  object ListBox_Presets: TListBox
    Left = 0
    Height = 228
    Top = 63
    Width = 399
    Align = alClient
    Font.Height = 243
    ItemHeight = 18
    ParentFont = False
    Style = lbOwnerDrawFixed
    TabOrder = 0
    OnDblClick = ListBox_PresetsDblClick
    OnDrawItem = ListBox_PresetsDrawItem
  end
  object ToolBar_Actions: TToolBar
    Left = 0
    Height = 42
    Top = 1
    Width = 399
    AutoSize = True
    BorderSpacing.Top = 1
    ButtonHeight = 40
    ButtonWidth = 36
    EdgeBorders = [ebBottom]
    Images = ImageList_Actions
    List = True
    ParentShowHint = False
    ShowHint = True
    TabOrder = 1
    object TB_Load: TToolButton
      Left = 1
      Hint = 'Load Preset'
      Top = 0
      ImageIndex = 0
      OnClick = TB_LoadClick
    end
    object TB_Append: TToolButton
      Left = 41
      Hint = 'Append Preset'
      Top = 0
      ImageIndex = 1
      OnClick = TB_AppendClick
    end
    object TB_Copy: TToolButton
      Left = 81
      Hint = 'Duplicate Preset'
      Top = 0
      ImageIndex = 2
      OnClick = TB_CopyClick
    end
    object TB_Rename: TToolButton
      Left = 121
      Hint = 'Rename Preset'
      Top = 0
      ImageIndex = 3
      OnClick = TB_RenameClick
    end
    object TB_Edit: TToolButton
      Left = 161
      Hint = 'Manually Edit Preset'
      Top = 0
      ImageIndex = 4
      OnClick = TB_EditClick
    end
    object TB_Delete: TToolButton
      Left = 201
      Hint = 'Delete Preset'
      Top = 0
      ImageIndex = 5
      OnClick = TB_DeleteClick
    end
  end
  object ImageList_Actions: TImageList
    Height = 36
    Width = 36
    Left = 56
    Top = 112
    Bitmap = {Binary: 5444 bytes}
  end
end

