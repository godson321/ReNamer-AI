unit uRuleReplace;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, uRule;

type
  { 替换规则配置 }
  TRuleConfigReplace = class(TRuleConfig)
  private
    FFindText: string;
    FReplaceText: string;
    FCaseSensitive: Boolean;
    FSkipExtension: Boolean;
    FWholeWordsOnly: Boolean;
    FUseWildcards: Boolean;
    FOccurrences: Integer;  // 0=All, 1=First, 2=Last
  public
    constructor Create; override;
    procedure Assign(Source: TPersistent); override;
  published
    property FindText: string read FFindText write FFindText;
    property ReplaceText: string read FReplaceText write FReplaceText;
    property CaseSensitive: Boolean read FCaseSensitive write FCaseSensitive;
    property SkipExtension: Boolean read FSkipExtension write FSkipExtension;
    property WholeWordsOnly: Boolean read FWholeWordsOnly write FWholeWordsOnly;
    property UseWildcards: Boolean read FUseWildcards write FUseWildcards;
    property Occurrences: Integer read FOccurrences write FOccurrences;
  end;

  { 替换规则 }
  TRuleReplace = class(TRule)
  private
    function GetConfig: TRuleConfigReplace;
    function ReplaceOccurrence(const AText, AFind, AReplace: string; 
      AOccurrence: Integer; ACaseSensitive: Boolean): string;
    function WildcardMatch(const AText, APattern: string): Boolean;
    function WildcardReplace(const AText, APattern, AReplacement: string): string;
  protected
    function GetRuleName: string; override;
    function GetRuleDescription: string; override;
  public
    constructor Create; override;
    function Execute(const AFileName: string; AFile: TRenFile): string; override;
    property Config: TRuleConfigReplace read GetConfig;
  end;

implementation

{ TRuleConfigReplace }

constructor TRuleConfigReplace.Create;
begin
  inherited Create;
  FFindText := '';
  FReplaceText := '';
  FCaseSensitive := False;
  FSkipExtension := True;
  FWholeWordsOnly := False;
  FUseWildcards := False;
  FOccurrences := 0;  // All
end;

procedure TRuleConfigReplace.Assign(Source: TPersistent);
begin
  if Source is TRuleConfigReplace then
  begin
    inherited Assign(Source);
    FFindText := TRuleConfigReplace(Source).FFindText;
    FReplaceText := TRuleConfigReplace(Source).FReplaceText;
    FCaseSensitive := TRuleConfigReplace(Source).FCaseSensitive;
    FSkipExtension := TRuleConfigReplace(Source).FSkipExtension;
    FWholeWordsOnly := TRuleConfigReplace(Source).FWholeWordsOnly;
    FUseWildcards := TRuleConfigReplace(Source).FUseWildcards;
    FOccurrences := TRuleConfigReplace(Source).FOccurrences;
  end
  else
    inherited Assign(Source);
end;

{ TRuleReplace }

constructor TRuleReplace.Create;
begin
  inherited Create;
  FConfig.Free;
  FConfig := TRuleConfigReplace.Create;
end;

function TRuleReplace.GetConfig: TRuleConfigReplace;
begin
  Result := TRuleConfigReplace(FConfig);
end;

function TRuleReplace.GetRuleName: string;
begin
  Result := 'Replace';
end;

function TRuleReplace.GetRuleDescription: string;
begin
  Result := Format('Replace "%s" with "%s"', [Config.FindText, Config.ReplaceText]);
end;

function TRuleReplace.ReplaceOccurrence(const AText, AFind, AReplace: string;
  AOccurrence: Integer; ACaseSensitive: Boolean): string;
var
  SearchText, SearchFind: string;
  Positions: array of Integer;
  i, p, LastPos: Integer;
begin
  Result := AText;
  if AFind = '' then Exit;
  
  // 根据大小写敏感设置准备搜索文本
  if ACaseSensitive then
  begin
    SearchText := AText;
    SearchFind := AFind;
  end
  else
  begin
    SearchText := LowerCase(AText);
    SearchFind := LowerCase(AFind);
  end;
  
  // 查找所有出现位置
  SetLength(Positions, 0);
  p := 1;
  while p <= Length(SearchText) - Length(SearchFind) + 1 do
  begin
    if Copy(SearchText, p, Length(SearchFind)) = SearchFind then
    begin
      SetLength(Positions, Length(Positions) + 1);
      Positions[High(Positions)] := p;
      p := p + Length(SearchFind);
    end
    else
      Inc(p);
  end;
  
  if Length(Positions) = 0 then Exit;
  
  // 根据替换模式执行替换
  case AOccurrence of
    0: // All - 替换所有，从后往前
      begin
        for i := High(Positions) downto 0 do
        begin
          Delete(Result, Positions[i], Length(AFind));
          Insert(AReplace, Result, Positions[i]);
        end;
      end;
    1: // First - 只替换第一个
      begin
        Delete(Result, Positions[0], Length(AFind));
        Insert(AReplace, Result, Positions[0]);
      end;
    2: // Last - 只替换最后一个
      begin
        LastPos := Positions[High(Positions)];
        Delete(Result, LastPos, Length(AFind));
        Insert(AReplace, Result, LastPos);
      end;
  end;
end;

function TRuleReplace.WildcardMatch(const AText, APattern: string): Boolean;
begin
  // TODO: Implement wildcard matching
  Result := False;
end;

function TRuleReplace.WildcardReplace(const AText, APattern, AReplacement: string): string;
begin
  // TODO: Implement wildcard replacement with backreferences
  Result := AText;
end;

function TRuleReplace.Execute(const AFileName: string; AFile: TRenFile): string;
var
  BaseName, Ext: string;
begin
  if Config.SkipExtension then
  begin
    Ext := ExtractFileExt(AFileName);
    BaseName := ChangeFileExt(AFileName, '');
  end
  else
  begin
    Ext := '';
    BaseName := AFileName;
  end;
  
  if Config.UseWildcards then
  begin
    // 通配符模式
    BaseName := WildcardReplace(BaseName, Config.FindText, Config.ReplaceText);
  end
  else
  begin
    // 普通替换
    BaseName := ReplaceOccurrence(BaseName, Config.FindText, Config.ReplaceText,
      Config.Occurrences, Config.CaseSensitive);
  end;
  
  Result := BaseName + Ext;
end;

end.
