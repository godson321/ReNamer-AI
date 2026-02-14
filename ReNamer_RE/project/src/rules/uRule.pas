unit uRule;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils;

type
  { 规则配置基类 }
  TRuleConfig = class(TPersistent)
  private
    FEnabled: Boolean;
    FName: string;
  public
    constructor Create; virtual;
    procedure LoadFromStream(AStream: TStream); virtual;
    procedure SaveToStream(AStream: TStream); virtual;
    procedure Assign(Source: TPersistent); override;
  published
    property Enabled: Boolean read FEnabled write FEnabled default True;
    property Name: string read FName write FName;
  end;

  { 重命名文件对象 }
  TRenFile = class
  private
    FOriginalPath: string;
    FOriginalName: string;
    FNewName: string;
    FIsRenamed: Boolean;
    FIsFolder: Boolean;
    FFileSize: Int64;
    FCreated: TDateTime;
    FModified: TDateTime;
  public
    constructor Create(const APath: string);
    function GetExtension: string;
    function GetBaseName: string;
    procedure SetNewName(const AName: string);
    procedure Rename;
    procedure UndoRename;
  published
    property OriginalPath: string read FOriginalPath;
    property OriginalName: string read FOriginalName;
    property NewName: string read FNewName write FNewName;
    property IsRenamed: Boolean read FIsRenamed;
    property IsFolder: Boolean read FIsFolder;
    property FileSize: Int64 read FFileSize;
    property Created: TDateTime read FCreated;
    property Modified: TDateTime read FModified;
  end;

  { 规则基类 - 所有规则继承此类 }
  TRule = class(TPersistent)
  private
    FEnabled: Boolean;
  protected
    FConfig: TRuleConfig;
    function GetRuleName: string; virtual; abstract;
    function GetRuleDescription: string; virtual;
  public
    constructor Create; virtual;
    destructor Destroy; override;
    
    { 核心方法 - 子类必须实现 }
    function Execute(const AFileName: string; AFile: TRenFile): string; virtual; abstract;
    
    { 配置序列化 }
    procedure LoadConfig(AStream: TStream); virtual;
    procedure SaveConfig(AStream: TStream); virtual;
    
    property Config: TRuleConfig read FConfig;
    property Enabled: Boolean read FEnabled write FEnabled;
    property RuleName: string read GetRuleName;
    property RuleDescription: string read GetRuleDescription;
  end;

  TRuleClass = class of TRule;

  { 规则列表 }
  TRuleList = class(TList)
  private
    function GetRule(Index: Integer): TRule;
  public
    procedure Clear; override;
    function Add(ARule: TRule): Integer;
    procedure Remove(ARule: TRule);
    function ApplyRules(AFile: TRenFile): string;
    property Rules[Index: Integer]: TRule read GetRule; default;
  end;

  { 文件列表 }
  TRenFileList = class(TList)
  private
    function GetFile(Index: Integer): TRenFile;
  public
    procedure Clear; override;
    function Add(const APath: string): TRenFile; overload;
    function Add(AFile: TRenFile): Integer; overload;
    procedure Preview(ARules: TRuleList);
    procedure Rename;
    procedure UndoRename;
    property Files[Index: Integer]: TRenFile read GetFile; default;
  end;

implementation

{ TRuleConfig }

constructor TRuleConfig.Create;
begin
  inherited Create;
  FEnabled := True;
  FName := '';
end;

procedure TRuleConfig.LoadFromStream(AStream: TStream);
begin
  // Override in subclasses
end;

procedure TRuleConfig.SaveToStream(AStream: TStream);
begin
  // Override in subclasses
end;

procedure TRuleConfig.Assign(Source: TPersistent);
begin
  if Source is TRuleConfig then
  begin
    FEnabled := TRuleConfig(Source).FEnabled;
    FName := TRuleConfig(Source).FName;
  end
  else
    inherited Assign(Source);
end;

{ TRenFile }

constructor TRenFile.Create(const APath: string);
begin
  inherited Create;
  FOriginalPath := APath;
  FOriginalName := ExtractFileName(APath);
  FNewName := FOriginalName;
  FIsRenamed := False;
  FIsFolder := DirectoryExists(APath);
  
  // TODO: Get file info
  FFileSize := 0;
  FCreated := Now;
  FModified := Now;
end;

function TRenFile.GetExtension: string;
begin
  Result := ExtractFileExt(FOriginalName);
end;

function TRenFile.GetBaseName: string;
begin
  Result := ChangeFileExt(FOriginalName, '');
end;

procedure TRenFile.SetNewName(const AName: string);
begin
  FNewName := AName;
end;

procedure TRenFile.Rename;
var
  NewPath: string;
begin
  if FIsRenamed then Exit;
  if FNewName = FOriginalName then Exit;
  
  NewPath := ExtractFilePath(FOriginalPath) + FNewName;
  if RenameFile(FOriginalPath, NewPath) then
    FIsRenamed := True;
end;

procedure TRenFile.UndoRename;
var
  CurrentPath: string;
begin
  if not FIsRenamed then Exit;
  
  CurrentPath := ExtractFilePath(FOriginalPath) + FNewName;
  if RenameFile(CurrentPath, FOriginalPath) then
    FIsRenamed := False;
end;

{ TRule }

constructor TRule.Create;
begin
  inherited Create;
  FEnabled := True;
  FConfig := TRuleConfig.Create;
end;

destructor TRule.Destroy;
begin
  FConfig.Free;
  inherited Destroy;
end;

function TRule.GetRuleDescription: string;
begin
  Result := '';
end;

procedure TRule.LoadConfig(AStream: TStream);
begin
  FConfig.LoadFromStream(AStream);
end;

procedure TRule.SaveConfig(AStream: TStream);
begin
  FConfig.SaveToStream(AStream);
end;

{ TRuleList }

function TRuleList.GetRule(Index: Integer): TRule;
begin
  Result := TRule(Items[Index]);
end;

procedure TRuleList.Clear;
var
  i: Integer;
begin
  for i := 0 to Count - 1 do
    TRule(Items[i]).Free;
  inherited Clear;
end;

function TRuleList.Add(ARule: TRule): Integer;
begin
  Result := inherited Add(ARule);
end;

procedure TRuleList.Remove(ARule: TRule);
begin
  inherited Remove(ARule);
  ARule.Free;
end;

function TRuleList.ApplyRules(AFile: TRenFile): string;
var
  i: Integer;
  CurrentName: string;
begin
  CurrentName := AFile.OriginalName;
  
  for i := 0 to Count - 1 do
  begin
    if Rules[i].Enabled then
      CurrentName := Rules[i].Execute(CurrentName, AFile);
  end;
  
  Result := CurrentName;
end;

{ TRenFileList }

function TRenFileList.GetFile(Index: Integer): TRenFile;
begin
  Result := TRenFile(Items[Index]);
end;

procedure TRenFileList.Clear;
var
  i: Integer;
begin
  for i := 0 to Count - 1 do
    TRenFile(Items[i]).Free;
  inherited Clear;
end;

function TRenFileList.Add(const APath: string): TRenFile;
begin
  Result := TRenFile.Create(APath);
  inherited Add(Result);
end;

function TRenFileList.Add(AFile: TRenFile): Integer;
begin
  Result := inherited Add(AFile);
end;

procedure TRenFileList.Preview(ARules: TRuleList);
var
  i: Integer;
begin
  for i := 0 to Count - 1 do
    Files[i].NewName := ARules.ApplyRules(Files[i]);
end;

procedure TRenFileList.Rename;
var
  i: Integer;
begin
  for i := 0 to Count - 1 do
    Files[i].Rename;
end;

procedure TRenFileList.UndoRename;
var
  i: Integer;
begin
  // Reverse order for undo
  for i := Count - 1 downto 0 do
    Files[i].UndoRename;
end;

end.
