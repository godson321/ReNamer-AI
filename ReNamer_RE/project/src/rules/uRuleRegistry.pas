unit uRuleRegistry;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, uRule;

type
  TRuleInfo = record
    RuleClass: TRuleClass;
    Name: string;
    Description: string;
  end;

  { 规则注册表 - 管理所有可用的规则类型 }
  TRuleRegistry = class
  private
    FRules: array of TRuleInfo;
    class var FInstance: TRuleRegistry;
    class function GetInstance: TRuleRegistry; static;
  public
    class property Instance: TRuleRegistry read GetInstance;
    
    procedure RegisterRule(ARuleClass: TRuleClass; const AName, ADescription: string);
    function CreateRule(const AName: string): TRule;
    function GetRuleCount: Integer;
    function GetRuleInfo(Index: Integer): TRuleInfo;
    function GetRuleNames: TStringList;
  end;

procedure RegisterAllRules;

implementation

uses
  uRuleReplace, uRuleRegEx;

{ TRuleRegistry }

class function TRuleRegistry.GetInstance: TRuleRegistry;
begin
  if FInstance = nil then
    FInstance := TRuleRegistry.Create;
  Result := FInstance;
end;

procedure TRuleRegistry.RegisterRule(ARuleClass: TRuleClass; const AName, ADescription: string);
var
  Index: Integer;
begin
  Index := Length(FRules);
  SetLength(FRules, Index + 1);
  FRules[Index].RuleClass := ARuleClass;
  FRules[Index].Name := AName;
  FRules[Index].Description := ADescription;
end;

function TRuleRegistry.CreateRule(const AName: string): TRule;
var
  i: Integer;
begin
  Result := nil;
  for i := 0 to High(FRules) do
  begin
    if FRules[i].Name = AName then
    begin
      Result := FRules[i].RuleClass.Create;
      Exit;
    end;
  end;
end;

function TRuleRegistry.GetRuleCount: Integer;
begin
  Result := Length(FRules);
end;

function TRuleRegistry.GetRuleInfo(Index: Integer): TRuleInfo;
begin
  Result := FRules[Index];
end;

function TRuleRegistry.GetRuleNames: TStringList;
var
  i: Integer;
begin
  Result := TStringList.Create;
  for i := 0 to High(FRules) do
    Result.Add(FRules[i].Name);
end;

{ 注册所有规则 }
procedure RegisterAllRules;
begin
  with TRuleRegistry.Instance do
  begin
    RegisterRule(TRuleReplace, 'Replace', 'Find and replace text');
    RegisterRule(TRuleRegEx, 'Regular Expressions', 'Use regular expressions for pattern matching');
    // TODO: 添加更多规则
    // RegisterRule(TRuleInsert, 'Insert', 'Insert text at specific position');
    // RegisterRule(TRuleDelete, 'Delete', 'Delete characters from specific position');
    // RegisterRule(TRuleCase, 'Case', 'Change letter case');
    // RegisterRule(TRuleSerialize, 'Serialize', 'Add sequential numbers');
    // RegisterRule(TRuleExtension, 'Extension', 'Modify file extension');
    // RegisterRule(TRulePadding, 'Padding', 'Pad numbers with zeros');
    // RegisterRule(TRuleStrip, 'Strip', 'Strip leading/trailing characters');
    // RegisterRule(TRuleCleanUp, 'Clean Up', 'Clean up file names');
    // RegisterRule(TRuleTranslit, 'Transliterate', 'Convert characters');
    // RegisterRule(TRuleRearrange, 'Rearrange', 'Rearrange filename parts');
    // RegisterRule(TRuleReformatDate, 'Reformat Date', 'Reformat date in filename');
    // RegisterRule(TRuleRandomize, 'Randomize', 'Add random characters');
    // RegisterRule(TRulePascalScript, 'PascalScript', 'Use custom Pascal script');
    // RegisterRule(TRuleUserInput, 'User Input', 'Prompt for user input');
  end;
end;

initialization
  RegisterAllRules;

finalization
  if TRuleRegistry.FInstance <> nil then
    TRuleRegistry.FInstance.Free;

end.
