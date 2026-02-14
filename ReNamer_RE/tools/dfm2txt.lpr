program dfm2txt;

{$mode objfpc}{$H+}

uses
  Classes, SysUtils;

{ 简单的 Delphi/Lazarus DFM 二进制转文本工具 }

type
  TValueType = (
    vaNull = 0,
    vaList = 1,
    vaInt8 = 2,
    vaInt16 = 3,
    vaInt32 = 4,
    vaExtended = 5,
    vaString = 6,
    vaIdent = 7,
    vaFalse = 8,
    vaTrue = 9,
    vaBinary = 10,
    vaSet = 11,
    vaLString = 12,
    vaNil = 13,
    vaCollection = 14,
    vaSingle = 15,
    vaCurrency = 16,
    vaDate = 17,
    vaWString = 18,
    vaInt64 = 19,
    vaUTF8String = 20
  );

var
  InStream: TFileStream;
  OutFile: TextFile;
  IndentLevel: Integer;

function EOS: Boolean;
begin
  Result := InStream.Position >= InStream.Size;
end;

function ReadByte: Byte;
begin
  Result := 0;
  if EOS then raise Exception.Create('Unexpected end of stream');
  InStream.Read(Result, 1);
end;

function ReadWord: Word;
begin
  Result := 0;
  if InStream.Size - InStream.Position < 2 then raise Exception.Create('Unexpected end of stream');
  InStream.Read(Result, 2);
end;

function ReadDWord: LongWord;
begin
  Result := 0;
  if InStream.Size - InStream.Position < 4 then raise Exception.Create('Unexpected end of stream');
  InStream.Read(Result, 4);
end;

function ReadInt64Value: Int64;
begin
  Result := 0;
  if InStream.Size - InStream.Position < 8 then raise Exception.Create('Unexpected end of stream');
  InStream.Read(Result, 8);
end;

function ReadShortString: string;
var
  Len: Byte;
  Buf: array of Byte;
begin
  Len := ReadByte;
  if Len = 0 then
  begin
    Result := '';
    Exit;
  end;
  SetLength(Buf, Len);
  InStream.Read(Buf[0], Len);
  SetLength(Result, Len);
  Move(Buf[0], Result[1], Len);
end;

function ReadLongString: string;
var
  Len: LongWord;
  Buf: array of Byte;
begin
  Len := ReadDWord;
  if Len = 0 then
  begin
    Result := '';
    Exit;
  end;
  SetLength(Buf, Len);
  InStream.Read(Buf[0], Len);
  SetLength(Result, Len);
  Move(Buf[0], Result[1], Len);
end;

function ReadUTF8String: string;
var
  Len: LongWord;
  Buf: array of Byte;
begin
  Len := ReadDWord;
  if Len = 0 then
  begin
    Result := '';
    Exit;
  end;
  SetLength(Buf, Len);
  InStream.Read(Buf[0], Len);
  SetLength(Result, Len);
  Move(Buf[0], Result[1], Len);
end;

function ReadWideString: string;
var
  Len: LongWord;
  Buf: array of Byte;
  WS: WideString;
  i: LongWord;
begin
  Len := ReadDWord;
  if Len = 0 then
  begin
    Result := '';
    Exit;
  end;
  SetLength(Buf, Len * 2);
  InStream.Read(Buf[0], Len * 2);
  SetLength(WS, Len);
  Move(Buf[0], WS[1], Len * 2);
  Result := UTF8Encode(WS);
end;

function Indent: string;
var
  i: Integer;
begin
  Result := '';
  for i := 1 to IndentLevel do
    Result := Result + '  ';
end;

procedure WriteIndented(const S: string);
begin
  WriteLn(OutFile, Indent + S);
end;

function EscapeString(const S: string): string;
var
  i: Integer;
  InStr: Boolean;
begin
  Result := '';
  InStr := False;
  for i := 1 to Length(S) do
  begin
    if (Ord(S[i]) >= 32) and (Ord(S[i]) < 127) and (S[i] <> '''') then
    begin
      if not InStr then
      begin
        Result := Result + '''';
        InStr := True;
      end;
      Result := Result + S[i];
    end
    else
    begin
      if InStr then
      begin
        Result := Result + '''';
        InStr := False;
      end;
      if S[i] = '''' then
        Result := Result + ''''''
      else
        Result := Result + '#' + IntToStr(Ord(S[i]));
    end;
  end;
  if InStr then
    Result := Result + '''';
  if Result = '' then
    Result := '''''';
end;

procedure ReadValue; forward;

procedure ReadBinaryData;
var
  Size: LongWord;
  Buf: array of Byte;
  i: LongWord;
  Line: string;
begin
  Size := ReadDWord;
  SetLength(Buf, Size);
  if Size > 0 then
    InStream.Read(Buf[0], Size);
  WriteLn(OutFile, Indent + '{');
  Inc(IndentLevel);
  Line := '';
  for i := 0 to Size - 1 do
  begin
    if (i > 0) and (i mod 16 = 0) then
    begin
      WriteIndented(Line);
      Line := '';
    end;
    Line := Line + IntToHex(Buf[i], 2);
  end;
  if Line <> '' then
    WriteIndented(Line);
  Dec(IndentLevel);
  WriteIndented('}');
end;

procedure ReadProperty;
var
  Name: string;
begin
  Name := ReadShortString;
  if Name = '' then Exit;
  Write(OutFile, Indent + Name + ' = ');
  ReadValue;
end;

procedure ReadItems;
var
  B: Byte;
begin
  WriteLn(OutFile, '<');
  Inc(IndentLevel);
  while not EOS do
  begin
    B := ReadByte;
    if B = 0 then Break; // end of collection
    // B should be vaList (1) = item start marker, consume it (don't put back)
    WriteIndented('item');
    Inc(IndentLevel);
    // read properties until 0 byte
    while not EOS do
    begin
      if ReadByte = 0 then Break;
      InStream.Position := InStream.Position - 1;
      ReadProperty;
    end;
    Dec(IndentLevel);
    WriteIndented('end');
  end;
  Dec(IndentLevel);
  WriteIndented('>');
end;

procedure ReadValue;
var
  VType: Byte;
  B: Byte;
  W: Word;
  DW: LongWord;
  I64: Int64;
  S: string;
  SingleVal: Single;
  DoubleVal: Double;
  ExtVal: Extended;
  CurrVal: Currency;
begin
  VType := ReadByte;
  case TValueType(VType) of
    vaNull:
      WriteLn(OutFile, 'null');
    vaList:
      begin
        WriteLn(OutFile, '(');
        Inc(IndentLevel);
        while True do
        begin
          if InStream.Position >= InStream.Size then Break;
          B := ReadByte;
          if B = 0 then Break;
          InStream.Position := InStream.Position - 1;
          Write(OutFile, Indent);
          ReadValue;
        end;
        Dec(IndentLevel);
        WriteIndented(')');
      end;
    vaInt8:
      begin
        B := ReadByte;
        WriteLn(OutFile, IntToStr(B));
      end;
    vaInt16:
      begin
        W := ReadWord;
        WriteLn(OutFile, IntToStr(SmallInt(W)));
      end;
    vaInt32:
      begin
        DW := ReadDWord;
        WriteLn(OutFile, IntToStr(LongInt(DW)));
      end;
    vaInt64:
      begin
        I64 := ReadInt64Value;
        WriteLn(OutFile, IntToStr(I64));
      end;
    vaString, vaLString:
      begin
        if TValueType(VType) = vaString then
          S := ReadShortString
        else
          S := ReadLongString;
        WriteLn(OutFile, EscapeString(S));
      end;
    vaUTF8String:
      begin
        S := ReadUTF8String;
        WriteLn(OutFile, EscapeString(S));
      end;
    vaWString:
      begin
        S := ReadWideString;
        WriteLn(OutFile, EscapeString(S));
      end;
    vaIdent:
      begin
        S := ReadShortString;
        WriteLn(OutFile, S);
      end;
    vaFalse:
      WriteLn(OutFile, 'False');
    vaTrue:
      WriteLn(OutFile, 'True');
    vaNil:
      WriteLn(OutFile, 'nil');
    vaBinary:
      begin
        WriteLn(OutFile, '');
        ReadBinaryData;
      end;
    vaSet:
      begin
        Write(OutFile, '[');
        DW := 0;
        while True do
        begin
          S := ReadShortString;
          if S = '' then Break;
          if DW > 0 then Write(OutFile, ', ');
          Write(OutFile, S);
          Inc(DW);
        end;
        WriteLn(OutFile, ']');
      end;
    vaCollection:
      ReadItems;
    vaSingle:
      begin
        InStream.Read(SingleVal, 4);
        WriteLn(OutFile, FloatToStr(SingleVal));
      end;
    vaCurrency:
      begin
        InStream.Read(CurrVal, 8);
        WriteLn(OutFile, CurrToStr(CurrVal));
      end;
    vaDate:
      begin
        InStream.Read(DoubleVal, 8);
        WriteLn(OutFile, FloatToStr(DoubleVal));
      end;
    vaExtended:
      begin
        // Extended is 10 bytes in x86 DFM but may be 8 bytes on x86_64
        // Always read 10 bytes from stream
        FillChar(ExtVal, SizeOf(ExtVal), 0);
        if SizeOf(ExtVal) >= 10 then
          InStream.Read(ExtVal, 10)
        else
        begin
          // On x86_64, Extended=8 bytes; read 10 bytes, use first 8 as Double
          InStream.Read(DoubleVal, 8);
          ReadWord; // skip 2 extra bytes
          ExtVal := DoubleVal;
        end;
        WriteLn(OutFile, FloatToStr(ExtVal));
      end;
  else
    WriteLn(OutFile, '{ unknown value type: ', VType, ' }');
  end;
end;

procedure ReadObject;
var
  B: Byte;
  Prefix: string;
  ClassName, ObjName: string;
  ChildPos: LongInt;
begin
  // Check for prefix/flags byte (used by inherited/inline components)
  // Format: $F0 | flags, where flags are in low nibble:
  //   bit 0 = ffInherited, bit 1 = ffChildPos, bit 2 = ffInline
  // Class name lengths are always < $F0, so byte >= $F0 means prefix
  B := ReadByte;
  Prefix := 'object ';
  if B >= $F0 then
  begin
    // Extract flags from low nibble
    if (B and $04) <> 0 then
      Prefix := 'inline '
    else if (B and $01) <> 0 then
      Prefix := 'inherited ';
    // If ffChildPos flag is set, skip the child position integer
    if (B and $02) <> 0 then
      ChildPos := LongInt(ReadDWord);
  end
  else
  begin
    // No prefix byte - put back the class name length byte
    InStream.Position := InStream.Position - 1;
  end;
  
  ClassName := ReadShortString;
  ObjName := ReadShortString;
  
  if ObjName <> '' then
    WriteIndented(Prefix + ObjName + ': ' + ClassName)
  else
    WriteIndented(Prefix + ClassName);
  
  Inc(IndentLevel);
  
  // Read properties
  while True do
  begin
    if InStream.Position >= InStream.Size then Break;
    if ReadByte = 0 then Break;
    InStream.Position := InStream.Position - 1;
    ReadProperty;
  end;
  
  // Read child objects
  while True do
  begin
    if InStream.Position >= InStream.Size then Break;
    if ReadByte = 0 then Break;
    InStream.Position := InStream.Position - 1;
    ReadObject;
  end;
  
  Dec(IndentLevel);
  WriteIndented('end');
end;

procedure ConvertFile(const InFile, OutFileName: string);
var
  Sig: array[0..3] of Byte;
begin
  InStream := TFileStream.Create(InFile, fmOpenRead or fmShareDenyNone);
  try
    AssignFile(OutFile, OutFileName);
    Rewrite(OutFile);
    try
      IndentLevel := 0;
      
      // Check for binary DFM signature: TPF0
      InStream.Read(Sig, 4);
      if (Sig[0] <> $54) or (Sig[1] <> $50) or (Sig[2] <> $46) or (Sig[3] <> $30) then
      begin
        WriteLn('Error: Not a valid binary DFM file: ', InFile);
        Exit;
      end;
      
      ReadObject;
    finally
      CloseFile(OutFile);
    end;
  finally
    InStream.Free;
  end;
end;

var
  InputDir, OutputDir: string;
  SR: TSearchRec;
  BaseName: string;
begin
  if ParamCount >= 2 then
  begin
    InputDir := ParamStr(1);
    OutputDir := ParamStr(2);
  end
  else
  begin
    WriteLn('Usage: dfm2txt <input_dir> <output_dir>');
    WriteLn('Converts all .dfm.bin files in input_dir to .dfm.txt in output_dir');
    Halt(1);
  end;
  
  if not DirectoryExists(OutputDir) then
    CreateDir(OutputDir);
  
  if FindFirst(InputDir + PathDelim + '*.dfm.bin', faAnyFile, SR) = 0 then
  begin
    repeat
      BaseName := SR.Name;
      // Remove .dfm.bin and extract form name
      BaseName := Copy(BaseName, 1, Pos('.', BaseName) - 1);
      
      Write('Converting ', SR.Name, ' -> ', BaseName + '.dfm.txt', ' ... ');
      try
        ConvertFile(
          InputDir + PathDelim + SR.Name,
          OutputDir + PathDelim + BaseName + '.dfm.txt'
        );
        WriteLn('OK');
      except
        on E: Exception do
          WriteLn('ERROR: ', E.Message);
      end;
    until FindNext(SR) <> 0;
    FindClose(SR);
  end
  else
    WriteLn('No .dfm.bin files found in ', InputDir);
end.
