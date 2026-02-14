unit uRuleRegEx;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, RegExpr, uRule;

type
  { 正则表达式规则配置 }
  TRuleConfigRegEx = class(TRuleConfig)
  private
    FExpression: string;
    FReplaceText: string;
    FCaseSensitive: Boolean;
    FSkipExtension: Boolean;
  public
    constructor Create; override;
    procedure Assign(Source: TPersistent); override;
  published
    property Expression: string read FExpression write FExpression;
    property ReplaceText: string read FReplaceText write FReplaceText;
    property CaseSensitive: Boolean read FCaseSensitive write FCaseSensitive;
    property SkipExtension: Boolean read FSkipExtension write FSkipExtension;
  end;

  { 正则表达式规则 }
  TRuleRegEx = class(TRule)
  private
    FRegEx: TRegExpr;
    function GetConfig: TRuleConfigRegEx;
  protected
    function GetRuleName: string; override;
    function GetRuleDescription: string; override;
  public
    constructor Create; override;
    destructor Destroy; override;
    function Execute(const AFileName: string; AFile: TRenFile): string; override;
    property Config: TRuleConfigRegEx read GetConfig;
  end;

implementation

{ TRuleConfigRegEx }

constructor TRuleConfigRegEx.Create;
begin
  inherited Create;
  FExpression := '';
  FReplaceText := '';
  FCaseSensitive := False;
  FSkipExtension := True;
end;

procedure TRuleConfigRegEx.Assign(Source: TPersistent);
begin
  if Source is TRuleConfigRegEx then
  begin
    inherited Assign(Source);
    FExpression := TRuleConfigRegEx(Source).FExpression;
    FReplaceText := TRuleConfigRegEx(Source).FReplaceText;
    FCaseSensitive := TRuleConfigRegEx(Source).FCaseSensitive;
    FSkipExtension := TRuleConfigRegEx(Source).FSkipExtension;
  end
  else
    inherited Assign(Source);
end;

{ TRuleRegEx }

constructor TRuleRegEx.Create;
begin
  inherited Create;
  FConfig.Free;
  FConfig := TRuleConfigRegEx.Create;
  FRegEx := TRegExpr.Create;
end;

destructor TRuleRegEx.Destroy;
begin
  FRegEx.Free;
  inherited Destroy;
end;

function TRuleRegEx.GetConfig: TRuleConfigRegEx;
begin
  Result := TRuleConfigRegEx(FConfig);
end;

function TRuleRegEx.GetRuleName: string;
begin
  Result := 'Regular Expressions';
end;

function TRuleRegEx.GetRuleDescription: string;
begin
  Result := Format('RegEx: %s -> %s', [Config.Expression, Config.ReplaceText]);
end;

function TRuleRegEx.Execute(const AFileName: string; AFile: TRenFile): string;
var
  BaseName, Ext: string;
begin
  Result := AFileName;
  
  if Config.Expression = '' then Exit;
  
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
  
  try
    FRegEx.Expression := Config.Expression;
    FRegEx.ModifierI := not Config.CaseSensitive;
    
    // 使用 Replace 函数，支持 $1..$9 反向引用
    BaseName := FRegEx.Replace(BaseName, Config.ReplaceText, True);
    
    Result := BaseName + Ext;
  except
    on E: Exception do
    begin
      // 正则表达式错误，返回原文件名
      Result := AFileName;
    end;
  end;
end;

end.
