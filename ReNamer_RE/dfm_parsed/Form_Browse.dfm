object Form_Browse: TForm_Browse
  Left = 539
  Height = 405
  Top = 311
  Width = 377
  BorderIcons = [biSystemMenu]
  Caption = 'Browse'
  ClientHeight = 405
  ClientWidth = 377
  KeyPreview = True
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  OnKeyPress = FormKeyPress
  OnShow = FormShow
  Position = poMainFormCenter
  LCLVersion = '3.8.0.0'
  object Label_Title: TLabel
    Left = 16
    Height = 15
    Top = 8
    Width = 186
    Caption = 'Select folders that you want to add:'
    ParentColor = False
  end
  object SB_GoToFolder: TSpeedButton
    Left = 313
    Height = 19
    Top = 307
    Width = 48
    Anchors = [akRight, akBottom]
    AutoSize = True
    Caption = 'Go To'
    Flat = True
    Glyph.Data = {Binary: 270 bytes}
    OnClick = SB_GoToFolderClick
  end
  object SpeedButton_FilterSettings: TSpeedButton
    Left = 272
    Height = 19
    Top = 325
    Width = 89
    Anchors = [akRight, akBottom]
    AutoSize = True
    Caption = 'Filter Settings'
    Flat = True
    Glyph.Data = {Binary: 270 bytes}
    OnClick = SpeedButton_FilterSettingsClick
  end
  object TreeView: TTreeView
    Left = 16
    Height = 276
    Top = 26
    Width = 345
    Anchors = [akTop, akLeft, akRight, akBottom]
    HideSelection = False
    Images = ImageList
    MultiSelect = True
    MultiSelectStyle = [msControlSelect, msShiftSelect, msVisibleOnly]
    PopupMenu = PopupMenu
    ReadOnly = True
    TabOrder = 0
    OnClick = TreeViewClick
    OnDeletion = TreeViewDeletion
    OnExpanding = TreeViewExpanding
    OnKeyUp = TreeViewKeyUp
    OnMouseDown = TreeViewMouseDown
    Options = [tvoAllowMultiselect, tvoAutoItemHeight, tvoKeepCollapsedNodes, tvoReadOnly, tvoShowButtons, tvoShowLines, tvoShowRoot, tvoToolTips, tvoThemedDraw]
  end
  object StatusBar: TStatusBar
    Left = 0
    Height = 23
    Top = 382
    Width = 377
<
    Panels = >
  end
  object BitBtn_Cancel: TBitBtn
    Left = 256
    Height = 26
    Top = 348
    Width = 105
    Anchors = [akRight, akBottom]
    Caption = 'Close'
    OnClick = BitBtn_CancelClick
    TabOrder = 4
  end
  object BitBtn_Ok: TBitBtn
    Left = 16
    Height = 26
    Top = 348
    Width = 225
    Anchors = [akLeft, akRight, akBottom]
    Caption = 'Add Folders'
    Default = True
    Glyph.Data = {Binary: 1082 bytes}
    OnClick = BitBtn_OkClick
    TabOrder = 3
  end
  object CheckBox_ShowHidden: TCheckBox
    Left = 16
    Height = 19
    Top = 307
    Width = 126
    Anchors = [akLeft, akBottom]
    Caption = 'Show hidden folders'
    TabOrder = 1
    OnClick = CheckBox_ShowHiddenClick
  end
  object CheckBox_ShowSystem: TCheckBox
    Left = 16
    Height = 19
    Top = 325
    Width = 126
    Anchors = [akLeft, akBottom]
    Caption = 'Show system folders'
    TabOrder = 2
    OnClick = CheckBox_ShowSystemClick
  end
  object ImageList: TImageList
    Left = 104
    Top = 144
  end
  object PopupMenu: TPopupMenu
    Left = 176
    Top = 144
    object MenuItem_ExploreFolder: TMenuItem
      Caption = 'Explore selected folder'
      OnClick = MenuItem_ExploreFolderClick
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object MenuItem_RefreshTree: TMenuItem
      Caption = 'Refresh Tree'
      ShortCut = 116
      OnClick = MenuItem_RefreshTreeClick
    end
    object MenuItem_CollapseAll: TMenuItem
      Caption = 'Collapse All'
      OnClick = MenuItem_CollapseAllClick
    end
    object MenuItem_GoToFolder: TMenuItem
      Caption = 'Go To Folder'
      OnClick = MenuItem_GoToFolderClick
    end
  end
end

