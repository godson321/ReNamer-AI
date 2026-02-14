# DFM Binary to Text Parser for Delphi/Lazarus Forms
# 解析Delphi/Lazarus的二进制DFM格式

param(
    [string]$ExePath = "E:\软件\文件重命名工具(ReNamer)7.8\ReNamer.exe",
    [string]$OutputDir = "E:\90 AI\项目\ReNamer_RE\dfm_parsed"
)

class DfmParser {
    [byte[]]$Data
    [int]$Pos
    [int]$Indent
    [System.Text.StringBuilder]$Result
    
    DfmParser([byte[]]$data) {
        $this.Data = $data
        $this.Pos = 0
        $this.Indent = 0
        $this.Result = [System.Text.StringBuilder]::new()
    }
    
    [byte] ReadByte() {
        return $this.Data[$this.Pos++]
    }
    
    [uint16] ReadWord() {
        $val = [BitConverter]::ToUInt16($this.Data, $this.Pos)
        $this.Pos += 2
        return $val
    }
    
    [int32] ReadInt() {
        $val = [BitConverter]::ToInt32($this.Data, $this.Pos)
        $this.Pos += 4
        return $val
    }
    
    [int64] ReadInt64() {
        $val = [BitConverter]::ToInt64($this.Data, $this.Pos)
        $this.Pos += 8
        return $val
    }
    
    [double] ReadDouble() {
        $val = [BitConverter]::ToDouble($this.Data, $this.Pos)
        $this.Pos += 8
        return $val
    }
    
    [single] ReadSingle() {
        $val = [BitConverter]::ToSingle($this.Data, $this.Pos)
        $this.Pos += 4
        return $val
    }
    
    [string] ReadString([int]$len) {
        if ($len -le 0) { return "" }
        $str = [System.Text.Encoding]::UTF8.GetString($this.Data, $this.Pos, $len)
        $this.Pos += $len
        return $str
    }
    
    [string] GetIndent() {
        return "  " * $this.Indent
    }
    
    [void] WriteLine([string]$line) {
        $this.Result.AppendLine($this.GetIndent() + $line) | Out-Null
    }
    
    [string] ReadValue() {
        $valType = $this.ReadByte()
        
        switch ($valType) {
            0 { return "[]" }  # vaNull
            1 { # vaList
                $items = @()
                while ($this.Data[$this.Pos] -ne 0) {
                    $items += $this.ReadValue()
                }
                $this.Pos++
                return "(" + ($items -join ", ") + ")"
            }
            2 { return $this.ReadByte().ToString() }  # vaInt8
            3 { return $this.ReadWord().ToString() }  # vaInt16
            4 { return $this.ReadInt().ToString() }   # vaInt32
            5 { # vaExtended (10 bytes)
                $bytes = $this.Data[$this.Pos..($this.Pos + 9)]
                $this.Pos += 10
                return "[Extended]"
            }
            6 { # vaString
                $len = $this.ReadByte()
                return "'" + $this.ReadString($len) + "'"
            }
            7 { # vaIdent
                $len = $this.ReadByte()
                return $this.ReadString($len)
            }
            8 { return "False" }  # vaFalse
            9 { return "True" }   # vaTrue
            10 { # vaBinary
                $len = $this.ReadInt()
                $this.Pos += $len
                return "{Binary: $len bytes}"
            }
            11 { # vaSet
                $items = @()
                while ($true) {
                    $len = $this.ReadByte()
                    if ($len -eq 0) { break }
                    $items += $this.ReadString($len)
                }
                return "[" + ($items -join ", ") + "]"
            }
            12 { # vaLString (UTF-8)
                $len = $this.ReadInt()
                return "'" + $this.ReadString($len) + "'"
            }
            13 { return "nil" }  # vaNil
            14 { # vaCollection
                $this.Result.AppendLine("<") | Out-Null
                $this.Indent++
                while ($this.Data[$this.Pos] -ne 0) {
                    $this.WriteLine("item")
                    $this.Indent++
                    $this.ReadProperties()
                    $this.Indent--
                    $this.WriteLine("end")
                }
                $this.Pos++
                $this.Indent--
                return ">"
            }
            15 { # vaSingle
                return $this.ReadSingle().ToString()
            }
            17 { # vaDouble  
                return $this.ReadDouble().ToString()
            }
            18 { # vaWString
                $len = $this.ReadInt()
                $str = [System.Text.Encoding]::Unicode.GetString($this.Data, $this.Pos, $len * 2)
                $this.Pos += $len * 2
                return "'" + $str + "'"
            }
            19 { return $this.ReadInt64().ToString() }  # vaInt64
            20 { # vaUTF8String
                $len = $this.ReadInt()
                return "'" + $this.ReadString($len) + "'"
            }
            default { 
                return "[Unknown:$valType]"
            }
        }
        return ""
    }
    
    [void] ReadProperties() {
        while ($this.Pos -lt $this.Data.Length) {
            $nameLen = $this.ReadByte()
            if ($nameLen -eq 0) { break }
            
            $propName = $this.ReadString($nameLen)
            $value = $this.ReadValue()
            $this.WriteLine("$propName = $value")
        }
    }
    
    [bool] ReadObject() {
        if ($this.Pos -ge $this.Data.Length) { return $false }
        
        $flags = $this.Data[$this.Pos]
        $inherited = ($flags -band 0x80) -ne 0
        $inline = ($flags -band 0x40) -ne 0
        
        $classLen = $this.ReadByte() -band 0x3F
        if ($classLen -eq 0) { return $false }
        
        $className = $this.ReadString($classLen)
        $nameLen = $this.ReadByte()
        $objName = $this.ReadString($nameLen)
        
        $prefix = if ($inherited) { "inherited" } elseif ($inline) { "inline" } else { "object" }
        $this.WriteLine("$prefix $objName`: $className")
        $this.Indent++
        
        $this.ReadProperties()
        
        # Read child objects
        while ($this.Pos -lt $this.Data.Length -and $this.Data[$this.Pos] -ne 0) {
            if (-not $this.ReadObject()) { break }
        }
        if ($this.Pos -lt $this.Data.Length) { $this.Pos++ }
        
        $this.Indent--
        $this.WriteLine("end")
        return $true
    }
    
    [string] Parse() {
        # Check for TPF0 signature
        if ($this.Data[0] -eq 0x54 -and $this.Data[1] -eq 0x50 -and 
            $this.Data[2] -eq 0x46 -and $this.Data[3] -eq 0x30) {
            $this.Pos = 4
        }
        
        $this.ReadObject() | Out-Null
        return $this.Result.ToString()
    }
}

# 主程序
Write-Host "=== DFM Parser ===" -ForegroundColor Cyan
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

$bytes = [System.IO.File]::ReadAllBytes($ExePath)

# 查找所有TPF0位置
$positions = @()
for ($i = 0; $i -lt $bytes.Length - 4; $i++) {
    if ($bytes[$i] -eq 0x54 -and $bytes[$i+1] -eq 0x50 -and 
        $bytes[$i+2] -eq 0x46 -and $bytes[$i+3] -eq 0x30) {
        $positions += $i
    }
}

Write-Host "找到 $($positions.Count) 个DFM资源" -ForegroundColor Green

$count = 0
foreach ($pos in $positions) {
    # 提取DFM名称
    $classLen = $bytes[$pos + 4] -band 0x3F
    if ($classLen -le 0 -or $classLen -gt 100) { continue }
    $className = [System.Text.Encoding]::ASCII.GetString($bytes, $pos + 5, $classLen)
    
    $namePos = $pos + 5 + $classLen
    $nameLen = $bytes[$namePos]
    if ($nameLen -le 0 -or $nameLen -gt 100) { continue }
    $objName = [System.Text.Encoding]::ASCII.GetString($bytes, $namePos + 1, $nameLen)
    
    # 估算大小
    $nextPos = ($positions | Where-Object { $_ -gt $pos } | Select-Object -First 1)
    if (-not $nextPos) { $nextPos = $bytes.Length }
    $size = [Math]::Min($nextPos - $pos, 100000)
    
    # 提取并解析
    $dfmBytes = $bytes[$pos..($pos + $size - 1)]
    
    try {
        $parser = [DfmParser]::new($dfmBytes)
        $text = $parser.Parse()
        
        $outFile = Join-Path $OutputDir "$objName.dfm"
        $text | Out-File $outFile -Encoding UTF8
        
        Write-Host "[$count] $objName ($className) - $([Math]::Round($size/1024, 1))KB" -ForegroundColor White
        $count++
    } catch {
        Write-Host "[$count] $objName - 解析失败: $_" -ForegroundColor Red
    }
}

Write-Host "`n解析完成: $count 个文件" -ForegroundColor Cyan
Write-Host "输出目录: $OutputDir" -ForegroundColor Yellow
