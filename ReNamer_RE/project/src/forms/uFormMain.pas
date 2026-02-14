unit uFormMain;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, Forms, Controls, Graphics, Dialogs, ComCtrls, StdCtrls,
  ExtCtrls, Menus, uRule, uRuleRegistry, uRuleReplace, uRuleRegEx;

type
  { TForm_Main }
  TForm_Main = class(TForm)
    ToolBar_Main: TToolBar;
    ToolButton_AddFiles: TToolButton;
    ToolButton_AddFolders: TToolButton;
    ToolButton_Sep1: TToolButton;
    ToolButton_Preview: TToolButton;
    ToolButton_Rename: TToolButton;
    Panel_Main: TPanel;
    Splitter_Middle: TSplitter;
    Panel_Rules: TPanel;
    ToolBar_Rules: TToolBar;
    ToolButton_AddRule: TToolButton;
    ToolButton_RemoveRule: TToolButton;
    ToolButton_MoveUpRule: TToolButton;
    ToolButton_MoveDownRule: TToolButton;
    ListView_Rules: TListView;
    Panel_Files: TPanel;
    ToolBar_Files: TToolBar;
    ToolButton_ClearFiles: TToolButton;
    ToolButton_Sep2: TToolButton;
    ToolButton_Sorting: TToolButton;
    ListView_Files: TListView;
    StatusBar: TStatusBar;
    PM_Files: TPopupMenu;
    MI_RemoveSelected: TMenuItem;
    MI_ClearAll: TMenuItem;
    N1: TMenuItem;
    MI_SelectAll: TMenuItem;
    MI_InvertSelection: TMenuItem;
    MainMenu: TMainMenu;
    MenuItem_File: TMenuItem;
    MI_AddFiles: TMenuItem;
    MI_AddFolders: TMenuItem;
    N2: TMenuItem;
    MI_Preview: TMenuItem;
    MI_Rename: TMenuItem;
    N3: TMenuItem;
    MI_Undo: TMenuItem;
    N4: TMenuItem;
    MI_Exit: TMenuItem;
    MenuItem_Rules: TMenuItem;
    MI_AddRule: TMenuItem;
    MI_RemoveRule: TMenuItem;
    MenuItem_Help: TMenuItem;
    MI_About: TMenuItem;
    OpenDialog: TOpenDialog;
    SelectDirectoryDialog: TSelectDirectoryDialog;

    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var CloseAction: TCloseAction);
    procedure FormDestroy(Sender: TObject);
    procedure FormDropFiles(Sender: TObject; const FileNames: array of string);
    procedure FormShow(Sender: TObject);
    procedure ToolButton_AddFilesClick(Sender: TObject);
    procedure ToolButton_AddFoldersClick(Sender: TObject);
    procedure ToolButton_PreviewClick(Sender: TObject);
    procedure ToolButton_RenameClick(Sender: TObject);
    procedure ToolButton_AddRuleClick(Sender: TObject);
    procedure ToolButton_RemoveRuleClick(Sender: TObject);
    procedure ToolButton_MoveUpRuleClick(Sender: TObject);
    procedure ToolButton_MoveDownRuleClick(Sender: TObject);
    procedure ListView_RulesDblClick(Sender: TObject);
    procedure ToolButton_ClearFilesClick(Sender: TObject);
    procedure ToolButton_SortingClick(Sender: TObject);
    procedure MI_RemoveSelectedClick(Sender: TObject);
    procedure MI_ClearAllClick(Sender: TObject);
    procedure MI_SelectAllClick(Sender: TObject);
    procedure MI_InvertSelectionClick(Sender: TObject);
    procedure MI_UndoClick(Sender: TObject);
    procedure MI_ExitClick(Sender: TObject);
    procedure MI_AboutClick(Sender: TObject);
  private
    FRuleList: TRuleList;
    FFileList: TRenFileList;
    procedure UpdateFileListView;
    procedure UpdateRuleListView;
    procedure UpdateStatusBar;
    procedure ShowRuleEditDialog(ARule: TRule);
  public
  end;

var
  Form_Main: TForm_Main;

implementation

{$R *.lfm}

procedure TForm_Main.FormCreate(Sender: TObject);
begin
  FRuleList := TRuleList.Create;
  FFileList := TRenFileList.Create;
  UpdateStatusBar;
end;

procedure TForm_Main.FormClose(Sender: TObject; var CloseAction: TCloseAction);
begin
  CloseAction := caFree;
end;

procedure TForm_Main.FormDestroy(Sender: TObject);
begin
  FRuleList.Free;
  FFileList.Free;
end;

procedure TForm_Main.FormDropFiles(Sender: TObject; const FileNames: array of string);
var
  i: Integer;
begin
  for i := 0 to High(FileNames) do
    FFileList.Add(FileNames[i]);
  UpdateFileListView;
  UpdateStatusBar;
end;

procedure TForm_Main.FormShow(Sender: TObject);
begin
  UpdateStatusBar;
end;

procedure TForm_Main.ToolButton_AddFilesClick(Sender: TObject);
var
  i: Integer;
begin
  if OpenDialog.Execute then
  begin
    for i := 0 to OpenDialog.Files.Count - 1 do
      FFileList.Add(OpenDialog.Files[i]);
    UpdateFileListView;
    UpdateStatusBar;
  end;
end;

procedure TForm_Main.ToolButton_AddFoldersClick(Sender: TObject);
var
  SR: TSearchRec;
begin
  if SelectDirectoryDialog.Execute then
  begin
    if FindFirst(SelectDirectoryDialog.FileName + PathDelim + '*', faAnyFile, SR) = 0 then
    begin
      repeat
        if (SR.Name <> '.') and (SR.Name <> '..') then
          FFileList.Add(SelectDirectoryDialog.FileName + PathDelim + SR.Name);
      until FindNext(SR) <> 0;
      FindClose(SR);
    end;
    UpdateFileListView;
    UpdateStatusBar;
  end;
end;

procedure TForm_Main.ToolButton_PreviewClick(Sender: TObject);
begin
  StatusBar.Panels[2].Text := 'Previewing...';
  Application.ProcessMessages;
  FFileList.Preview(FRuleList);
  UpdateFileListView;
  StatusBar.Panels[2].Text := 'Preview complete';
end;

procedure TForm_Main.ToolButton_RenameClick(Sender: TObject);
begin
  if FFileList.Count = 0 then
  begin
    ShowMessage('No files to rename.');
    Exit;
  end;
  if MessageDlg('Confirm', Format('Rename %d file(s)?', [FFileList.Count]),
    mtConfirmation, [mbYes, mbNo], 0) = mrYes then
  begin
    FFileList.Rename;
    UpdateFileListView;
    StatusBar.Panels[2].Text := Format('Renamed %d file(s)', [FFileList.Count]);
  end;
end;

procedure TForm_Main.ToolButton_AddRuleClick(Sender: TObject);
var
  RuleNames: TStringList;
  NewRule: TRule;
  Dlg: TForm;
  LB: TListBox;
  BtnOK, BtnCancel: TButton;
  BtnPanel: TPanel;
  Selected: Integer;
begin
  RuleNames := TRuleRegistry.Instance.GetRuleNames;
  try
    Dlg := TForm.CreateNew(Self);
    try
      Dlg.Caption := 'Add Rule';
      Dlg.Width := 320;
      Dlg.Height := 220;
      Dlg.Position := poMainFormCenter;
      Dlg.BorderStyle := bsDialog;

      LB := TListBox.Create(Dlg);
      LB.Parent := Dlg;
      LB.Align := alClient;
      LB.Items.Assign(RuleNames);
      LB.ItemIndex := 0;

      BtnPanel := TPanel.Create(Dlg);
      BtnPanel.Parent := Dlg;
      BtnPanel.Align := alBottom;
      BtnPanel.Height := 40;
      BtnPanel.BevelOuter := bvNone;

      BtnOK := TButton.Create(Dlg);
      BtnOK.Parent := BtnPanel;
      BtnOK.Caption := 'OK';
      BtnOK.Left := 80;
      BtnOK.Top := 8;
      BtnOK.Width := 75;
      BtnOK.ModalResult := mrOK;
      BtnOK.Default := True;

      BtnCancel := TButton.Create(Dlg);
      BtnCancel.Parent := BtnPanel;
      BtnCancel.Caption := 'Cancel';
      BtnCancel.Left := 164;
      BtnCancel.Top := 8;
      BtnCancel.Width := 75;
      BtnCancel.ModalResult := mrCancel;
      BtnCancel.Cancel := True;

      if Dlg.ShowModal = mrOK then
      begin
        Selected := LB.ItemIndex;
        if Selected >= 0 then
        begin
          NewRule := TRuleRegistry.Instance.CreateRule(RuleNames[Selected]);
          if NewRule <> nil then
          begin
            FRuleList.Add(NewRule);
            UpdateRuleListView;
            ShowRuleEditDialog(NewRule);
            ToolButton_PreviewClick(nil);
          end;
        end;
      end;
    finally
      Dlg.Free;
    end;
  finally
    RuleNames.Free;
  end;
end;

procedure TForm_Main.ToolButton_RemoveRuleClick(Sender: TObject);
var
  Idx: Integer;
begin
  if ListView_Rules.Selected <> nil then
  begin
    Idx := ListView_Rules.Selected.Index;
    FRuleList.Remove(FRuleList[Idx]);
    UpdateRuleListView;
    ToolButton_PreviewClick(nil);
  end;
end;

procedure TForm_Main.ToolButton_MoveUpRuleClick(Sender: TObject);
var
  Idx: Integer;
begin
  if ListView_Rules.Selected = nil then Exit;
  Idx := ListView_Rules.Selected.Index;
  if Idx > 0 then
  begin
    FRuleList.Exchange(Idx, Idx - 1);
    UpdateRuleListView;
    ListView_Rules.Items[Idx - 1].Selected := True;
    ToolButton_PreviewClick(nil);
  end;
end;

procedure TForm_Main.ToolButton_MoveDownRuleClick(Sender: TObject);
var
  Idx: Integer;
begin
  if ListView_Rules.Selected = nil then Exit;
  Idx := ListView_Rules.Selected.Index;
  if Idx < FRuleList.Count - 1 then
  begin
    FRuleList.Exchange(Idx, Idx + 1);
    UpdateRuleListView;
    ListView_Rules.Items[Idx + 1].Selected := True;
    ToolButton_PreviewClick(nil);
  end;
end;

procedure TForm_Main.ListView_RulesDblClick(Sender: TObject);
begin
  if ListView_Rules.Selected <> nil then
  begin
    ShowRuleEditDialog(FRuleList[ListView_Rules.Selected.Index]);
    UpdateRuleListView;
    ToolButton_PreviewClick(nil);
  end;
end;

procedure TForm_Main.ToolButton_ClearFilesClick(Sender: TObject);
begin
  FFileList.Clear;
  UpdateFileListView;
  UpdateStatusBar;
end;

procedure TForm_Main.ToolButton_SortingClick(Sender: TObject);
begin
  ShowMessage('Sorting not yet implemented.');
end;

procedure TForm_Main.MI_RemoveSelectedClick(Sender: TObject);
var
  i: Integer;
begin
  for i := ListView_Files.Items.Count - 1 downto 0 do
    if ListView_Files.Items[i].Selected then
    begin
      TRenFile(FFileList[i]).Free;
      FFileList.Delete(i);
    end;
  UpdateFileListView;
  UpdateStatusBar;
end;

procedure TForm_Main.MI_ClearAllClick(Sender: TObject);
begin
  ToolButton_ClearFilesClick(Sender);
end;

procedure TForm_Main.MI_SelectAllClick(Sender: TObject);
var
  i: Integer;
begin
  for i := 0 to ListView_Files.Items.Count - 1 do
    ListView_Files.Items[i].Selected := True;
end;

procedure TForm_Main.MI_InvertSelectionClick(Sender: TObject);
var
  i: Integer;
begin
  for i := 0 to ListView_Files.Items.Count - 1 do
    ListView_Files.Items[i].Selected := not ListView_Files.Items[i].Selected;
end;

procedure TForm_Main.MI_UndoClick(Sender: TObject);
begin
  FFileList.UndoRename;
  UpdateFileListView;
  StatusBar.Panels[2].Text := 'Undo complete';
end;

procedure TForm_Main.MI_ExitClick(Sender: TObject);
begin
  Close;
end;

procedure TForm_Main.MI_AboutClick(Sender: TObject);
begin
  ShowMessage('ReNamer Clone' + LineEnding +
    'File batch renaming tool' + LineEnding + LineEnding +
    'Based on reverse engineering study of ReNamer 7.8');
end;

procedure TForm_Main.UpdateFileListView;
var
  i: Integer;
  Item: TListItem;
  RenFile: TRenFile;
begin
  ListView_Files.Items.BeginUpdate;
  try
    ListView_Files.Items.Clear;
    for i := 0 to FFileList.Count - 1 do
    begin
      RenFile := FFileList[i];
      Item := ListView_Files.Items.Add;
      Item.Caption := IntToStr(i + 1);
      Item.SubItems.Add(RenFile.OriginalName);
      Item.SubItems.Add(RenFile.NewName);
      Item.SubItems.Add(ExtractFilePath(RenFile.OriginalPath));
    end;
  finally
    ListView_Files.Items.EndUpdate;
  end;
end;

procedure TForm_Main.UpdateRuleListView;
var
  i: Integer;
  Item: TListItem;
begin
  ListView_Rules.Items.BeginUpdate;
  try
    ListView_Rules.Items.Clear;
    for i := 0 to FRuleList.Count - 1 do
    begin
      Item := ListView_Rules.Items.Add;
      Item.Caption := FRuleList[i].RuleName;
      Item.SubItems.Add(FRuleList[i].RuleDescription);
      Item.Checked := FRuleList[i].Enabled;
    end;
  finally
    ListView_Rules.Items.EndUpdate;
  end;
end;

procedure TForm_Main.UpdateStatusBar;
begin
  StatusBar.Panels[0].Text := Format('Files: %d', [FFileList.Count]);
  StatusBar.Panels[1].Text := Format('Rules: %d', [FRuleList.Count]);
end;

procedure TForm_Main.ShowRuleEditDialog(ARule: TRule);
var
  FindText, ReplaceText: string;
begin
  if ARule is TRuleReplace then
  begin
    FindText := TRuleReplace(ARule).Config.FindText;
    ReplaceText := TRuleReplace(ARule).Config.ReplaceText;
    if InputQuery('Replace Rule', 'Find text:', FindText) then
    begin
      TRuleReplace(ARule).Config.FindText := FindText;
      if InputQuery('Replace Rule', 'Replace with:', ReplaceText) then
        TRuleReplace(ARule).Config.ReplaceText := ReplaceText;
    end;
  end
  else if ARule is TRuleRegEx then
  begin
    FindText := TRuleRegEx(ARule).Config.Expression;
    ReplaceText := TRuleRegEx(ARule).Config.ReplaceText;
    if InputQuery('RegEx Rule', 'Expression:', FindText) then
    begin
      TRuleRegEx(ARule).Config.Expression := FindText;
      if InputQuery('RegEx Rule', 'Replace with:', ReplaceText) then
        TRuleRegEx(ARule).Config.ReplaceText := ReplaceText;
    end;
  end;
end;

end.