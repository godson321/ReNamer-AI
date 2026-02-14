// ========== WriteFile @ 00401100 ==========

BOOL WriteFile(HANDLE hFile,LPCVOID lpBuffer,DWORD nNumberOfBytesToWrite,
              LPDWORD lpNumberOfBytesWritten,LPOVERLAPPED lpOverlapped)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401100. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = WriteFile(hFile,lpBuffer,nNumberOfBytesToWrite,lpNumberOfBytesWritten,lpOverlapped);
  return BVar1;
}



// ========== ReadFile @ 00401110 ==========

BOOL ReadFile(HANDLE hFile,LPVOID lpBuffer,DWORD nNumberOfBytesToRead,LPDWORD lpNumberOfBytesRead,
             LPOVERLAPPED lpOverlapped)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401110. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = ReadFile(hFile,lpBuffer,nNumberOfBytesToRead,lpNumberOfBytesRead,lpOverlapped);
  return BVar1;
}



// ========== SetFilePointer @ 00401130 ==========

DWORD SetFilePointer(HANDLE hFile,LONG lDistanceToMove,PLONG lpDistanceToMoveHigh,DWORD dwMoveMethod
                    )

{
  DWORD DVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401130. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DVar1 = SetFilePointer(hFile,lDistanceToMove,lpDistanceToMoveHigh,dwMoveMethod);
  return DVar1;
}



// ========== GetFileSize @ 00401140 ==========

DWORD GetFileSize(HANDLE hFile,LPDWORD lpFileSizeHigh)

{
  DWORD DVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401140. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DVar1 = GetFileSize(hFile,lpFileSizeHigh);
  return DVar1;
}



// ========== SetEndOfFile @ 00401150 ==========

BOOL SetEndOfFile(HANDLE hFile)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401150. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = SetEndOfFile(hFile);
  return BVar1;
}



// ========== DeleteFileW @ 004011c0 ==========

BOOL DeleteFileW(LPCWSTR lpFileName)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x004011c0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = DeleteFileW(lpFileName);
  return BVar1;
}



// ========== MoveFileW @ 004011d0 ==========

BOOL MoveFileW(LPCWSTR lpExistingFileName,LPCWSTR lpNewFileName)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x004011d0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = MoveFileW(lpExistingFileName,lpNewFileName);
  return BVar1;
}



// ========== CreateFileW @ 004011e0 ==========

HANDLE CreateFileW(LPCWSTR lpFileName,DWORD dwDesiredAccess,DWORD dwShareMode,
                  LPSECURITY_ATTRIBUTES lpSecurityAttributes,DWORD dwCreationDisposition,
                  DWORD dwFlagsAndAttributes,HANDLE hTemplateFile)

{
  HANDLE pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x004011e0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = CreateFileW(lpFileName,dwDesiredAccess,dwShareMode,lpSecurityAttributes,
                       dwCreationDisposition,dwFlagsAndAttributes,hTemplateFile);
  return pvVar1;
}



// ========== GetFileAttributesW @ 004011f0 ==========

DWORD GetFileAttributesW(LPCWSTR lpFileName)

{
  DWORD DVar1;
  
                    /* WARNING: Could not recover jumptable at 0x004011f0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DVar1 = GetFileAttributesW(lpFileName);
  return DVar1;
}



// ========== GetModuleFileNameW @ 00401610 ==========

DWORD GetModuleFileNameW(HMODULE hModule,LPWSTR lpFilename,DWORD nSize)

{
  DWORD DVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401610. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DVar1 = GetModuleFileNameW(hModule,lpFilename,nSize);
  return DVar1;
}



// ========== SetFileAttributesW @ 00401640 ==========

BOOL SetFileAttributesW(LPCWSTR lpFileName,DWORD dwFileAttributes)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401640. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = SetFileAttributesW(lpFileName,dwFileAttributes);
  return BVar1;
}



// ========== FindNextFileW @ 00401660 ==========

BOOL FindNextFileW(HANDLE hFindFile,LPWIN32_FIND_DATAW lpFindFileData)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401660. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = FindNextFileW(hFindFile,lpFindFileData);
  return BVar1;
}



// ========== FindFirstFileExW @ 004016b0 ==========

HANDLE FindFirstFileExW(LPCWSTR lpFileName,FINDEX_INFO_LEVELS fInfoLevelId,LPVOID lpFindFileData,
                       FINDEX_SEARCH_OPS fSearchOp,LPVOID lpSearchFilter,DWORD dwAdditionalFlags)

{
  HANDLE pvVar1;
  
                    /* WARNING: Could not recover jumptable at 0x004016b0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pvVar1 = FindFirstFileExW(lpFileName,fInfoLevelId,lpFindFileData,fSearchOp,lpSearchFilter,
                            dwAdditionalFlags);
  return pvVar1;
}



// ========== GetFileTime @ 00401790 ==========

BOOL GetFileTime(HANDLE hFile,LPFILETIME lpCreationTime,LPFILETIME lpLastAccessTime,
                LPFILETIME lpLastWriteTime)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401790. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = GetFileTime(hFile,lpCreationTime,lpLastAccessTime,lpLastWriteTime);
  return BVar1;
}



// ========== SetFileTime @ 004017a0 ==========

BOOL SetFileTime(HANDLE hFile,FILETIME *lpCreationTime,FILETIME *lpLastAccessTime,
                FILETIME *lpLastWriteTime)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x004017a0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = SetFileTime(hFile,lpCreationTime,lpLastAccessTime,lpLastWriteTime);
  return BVar1;
}



// ========== SystemTimeToFileTime @ 00401810 ==========

BOOL SystemTimeToFileTime(SYSTEMTIME *lpSystemTime,LPFILETIME lpFileTime)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401810. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = SystemTimeToFileTime(lpSystemTime,lpFileTime);
  return BVar1;
}



// ========== FileTimeToLocalFileTime @ 00401820 ==========

BOOL FileTimeToLocalFileTime(FILETIME *lpFileTime,LPFILETIME lpLocalFileTime)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401820. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = FileTimeToLocalFileTime(lpFileTime,lpLocalFileTime);
  return BVar1;
}



// ========== LocalFileTimeToFileTime @ 00401830 ==========

BOOL LocalFileTimeToFileTime(FILETIME *lpLocalFileTime,LPFILETIME lpFileTime)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401830. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = LocalFileTimeToFileTime(lpLocalFileTime,lpFileTime);
  return BVar1;
}



// ========== FileTimeToSystemTime @ 00401840 ==========

BOOL FileTimeToSystemTime(FILETIME *lpFileTime,LPSYSTEMTIME lpSystemTime)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401840. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = FileTimeToSystemTime(lpFileTime,lpSystemTime);
  return BVar1;
}



// ========== FileTimeToDosDateTime @ 00401850 ==========

BOOL FileTimeToDosDateTime(FILETIME *lpFileTime,LPWORD lpFatDate,LPWORD lpFatTime)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401850. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = FileTimeToDosDateTime(lpFileTime,lpFatDate,lpFatTime);
  return BVar1;
}



// ========== DosDateTimeToFileTime @ 00401860 ==========

BOOL DosDateTimeToFileTime(WORD wFatDate,WORD wFatTime,LPFILETIME lpFileTime)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00401860. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = DosDateTimeToFileTime(wFatDate,wFatTime,lpFileTime);
  return BVar1;
}



// ========== StgIsStorageFile @ 004026d0 ==========

HRESULT StgIsStorageFile(WCHAR *pwcsName)

{
  HRESULT HVar1;
  
                    /* WARNING: Could not recover jumptable at 0x004026d0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  HVar1 = StgIsStorageFile(pwcsName);
  return HVar1;
}



// ========== GetFileVersionInfoSizeA @ 00402e70 ==========

DWORD GetFileVersionInfoSizeA(LPCSTR lptstrFilename,LPDWORD lpdwHandle)

{
  DWORD DVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402e70. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DVar1 = GetFileVersionInfoSizeA(lptstrFilename,lpdwHandle);
  return DVar1;
}



// ========== GetFileVersionInfoA @ 00402e80 ==========

BOOL GetFileVersionInfoA(LPCSTR lptstrFilename,DWORD dwHandle,DWORD dwLen,LPVOID lpData)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402e80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = GetFileVersionInfoA(lptstrFilename,dwHandle,dwLen,lpData);
  return BVar1;
}



// ========== GetFileVersionInfoSizeW @ 00402ea0 ==========

DWORD GetFileVersionInfoSizeW(LPCWSTR lptstrFilename,LPDWORD lpdwHandle)

{
  DWORD DVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402ea0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DVar1 = GetFileVersionInfoSizeW(lptstrFilename,lpdwHandle);
  return DVar1;
}



// ========== GetFileVersionInfoW @ 00402eb0 ==========

BOOL GetFileVersionInfoW(LPCWSTR lptstrFilename,DWORD dwHandle,DWORD dwLen,LPVOID lpData)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402eb0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = GetFileVersionInfoW(lptstrFilename,dwHandle,dwLen,lpData);
  return BVar1;
}



// ========== DragQueryFileA @ 00402ed0 ==========

UINT DragQueryFileA(HDROP hDrop,UINT iFile,LPSTR lpszFile,UINT cch)

{
  UINT UVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402ed0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  UVar1 = DragQueryFileA(hDrop,iFile,lpszFile,cch);
  return UVar1;
}



// ========== DragQueryFileW @ 00402ee0 ==========

UINT DragQueryFileW(HDROP hDrop,UINT iFile,LPWSTR lpszFile,UINT cch)

{
  UINT UVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402ee0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  UVar1 = DragQueryFileW(hDrop,iFile,lpszFile,cch);
  return UVar1;
}



// ========== ShellExecuteW @ 00402ef0 ==========

HINSTANCE ShellExecuteW(HWND hwnd,LPCWSTR lpOperation,LPCWSTR lpFile,LPCWSTR lpParameters,
                       LPCWSTR lpDirectory,INT nShowCmd)

{
  HINSTANCE pHVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402ef0. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  pHVar1 = ShellExecuteW(hwnd,lpOperation,lpFile,lpParameters,lpDirectory,nShowCmd);
  return pHVar1;
}



// ========== DragAcceptFiles @ 00402f10 ==========

void DragAcceptFiles(HWND hWnd,BOOL fAccept)

{
                    /* WARNING: Could not recover jumptable at 0x00402f10. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DragAcceptFiles(hWnd,fAccept);
  return;
}



// ========== SHFileOperationW @ 00402f60 ==========

int SHFileOperationW(LPSHFILEOPSTRUCTW lpFileOp)

{
  int iVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402f60. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  iVar1 = SHFileOperationW(lpFileOp);
  return iVar1;
}



// ========== ShellExecuteExW @ 00402f70 ==========

BOOL ShellExecuteExW(SHELLEXECUTEINFOW *pExecInfo)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402f70. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = ShellExecuteExW(pExecInfo);
  return BVar1;
}



// ========== SHGetFileInfoW @ 00402f80 ==========

DWORD_PTR SHGetFileInfoW(LPCWSTR pszPath,DWORD dwFileAttributes,SHFILEINFOW *psfi,UINT cbFileInfo,
                        UINT uFlags)

{
  DWORD_PTR DVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00402f80. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  DVar1 = SHGetFileInfoW(pszPath,dwFileAttributes,psfi,cbFileInfo,uFlags);
  return DVar1;
}



// ========== GetOpenFileNameW @ 00403120 ==========

BOOL GetOpenFileNameW(LPOPENFILENAMEW param_1)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00403120. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = GetOpenFileNameW(param_1);
  return BVar1;
}



// ========== GetSaveFileNameW @ 00403130 ==========

BOOL GetSaveFileNameW(LPOPENFILENAMEW param_1)

{
  BOOL BVar1;
  
                    /* WARNING: Could not recover jumptable at 0x00403130. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  BVar1 = GetSaveFileNameW(param_1);
  return BVar1;
}



// ========== tls_callback_0 @ 00403190 ==========

void tls_callback_0(undefined4 param_1,int param_2)

{
  LPVOID pvVar1;
  
  if ((DAT_007ab6c0 == '\0') && (param_2 != 0)) {
    if (param_2 == 1) {
      FUN_00404850();
      FUN_00412c60();
      FUN_00413930();
    }
    else if (param_2 == 2) {
      FUN_00413260();
      FUN_00412e40();
    }
    else if ((param_2 == 3) &&
            (pvVar1 = TlsGetValue(*(DWORD *)PTR_DAT_007ab040), pvVar1 != (LPVOID)0x0)) {
      FUN_00412f10();
    }
  }
  return;
}



// ========== FUN_00403da0 @ 00403da0 ==========

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void __fastcall FUN_00403da0(uint param_1,int param_2)

{
  int in_EAX;
  
  if (param_1 < 0x25) {
    if (param_2 < in_EAX) {
      FUN_00403570();
      return;
    }
    if (in_EAX != param_2) {
      FUN_00403710();
      return;
    }
  }
  else if (0x24 < (int)param_1) {
    if (in_EAX <= param_2) {
      if (in_EAX == param_2) {
        return;
      }
      if (param_2 < (int)(in_EAX + param_1)) {
                    /* WARNING: Could not recover jumptable at 0x00403dd2. Too many branches */
                    /* WARNING: Treating indirect jump as call */
        (*_DAT_007ab990)();
        return;
      }
    }
                    /* WARNING: Could not recover jumptable at 0x00403dcc. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (*_DAT_007ab980)();
    return;
  }
  return;
}



// ========== FUN_00403e70 @ 00403e70 ==========

int __fastcall FUN_00403e70(uint param_1,uint param_2)

{
  uint *in_EAX;
  uint *puVar1;
  uint *puVar2;
  char cVar3;
  uint uVar4;
  bool bVar5;
  
  puVar1 = in_EAX;
  if (3 < param_2) {
    cVar3 = (char)param_1;
    param_1 = CONCAT22(CONCAT11(cVar3,cVar3),CONCAT11(cVar3,cVar3));
    for (; ((uint)puVar1 & 3) != 0; puVar1 = (uint *)((int)puVar1 + 1)) {
      puVar2 = puVar1;
      if (*(char *)puVar1 == cVar3) goto LAB_00403f6e;
      param_2 = param_2 - 1;
    }
    while (bVar5 = 0xf < param_2, param_2 = param_2 - 0x10, bVar5) {
      uVar4 = *puVar1 ^ param_1;
      uVar4 = ~uVar4 & 0x80808080 & (uVar4 + 0xfefefeff ^ uVar4);
      if (uVar4 != 0) goto LAB_00403f53;
      uVar4 = puVar1[1] ^ param_1;
      uVar4 = ~uVar4 & 0x80808080 & (uVar4 + 0xfefefeff ^ uVar4);
      if (uVar4 != 0) {
LAB_00403f50:
        puVar1 = puVar1 + 1;
        goto LAB_00403f53;
      }
      uVar4 = puVar1[2] ^ param_1;
      uVar4 = ~uVar4 & 0x80808080 & (uVar4 + 0xfefefeff ^ uVar4);
      if (uVar4 != 0) {
LAB_00403f4d:
        puVar1 = puVar1 + 1;
        goto LAB_00403f50;
      }
      uVar4 = puVar1[3] ^ param_1;
      uVar4 = ~uVar4 & 0x80808080 & (uVar4 + 0xfefefeff ^ uVar4);
      if (uVar4 != 0) {
        puVar1 = puVar1 + 1;
        goto LAB_00403f4d;
      }
      puVar1 = puVar1 + 4;
    }
    for (; 0xfffffff3 < param_2; param_2 = param_2 - 4) {
      uVar4 = *puVar1 ^ param_1;
      uVar4 = ~uVar4 & 0x80808080 & (uVar4 + 0xfefefeff ^ uVar4);
      if (uVar4 != 0) goto LAB_00403f53;
      puVar1 = puVar1 + 1;
    }
  }
  param_2 = param_2 & 3;
  puVar2 = puVar1;
  while( true ) {
    if (param_2 == 0) {
      return -1;
    }
    if (*(char *)puVar2 == (char)param_1) break;
    puVar2 = (uint *)((int)puVar2 + 1);
    param_2 = param_2 - 1;
  }
LAB_00403f6e:
  return (int)puVar2 - (int)in_EAX;
LAB_00403f53:
  puVar2 = puVar1;
  if ((((uVar4 & 0xff) == 0) && (puVar2 = (uint *)((int)puVar1 + 1), (uVar4 & 0xff00) == 0)) &&
     (puVar2 = (uint *)((int)puVar1 + 2), (uVar4 & 0xff0000) == 0)) {
    puVar2 = (uint *)((int)puVar1 + 3);
  }
  goto LAB_00403f6e;
}



// ========== FUN_00403f80 @ 00403f80 ==========

int __fastcall FUN_00403f80(short param_1,int param_2)

{
  short *in_EAX;
  int iVar1;
  bool bVar2;
  
  bVar2 = param_2 == 0;
  iVar1 = param_2;
  if (!bVar2) {
    do {
      if (iVar1 == 0) break;
      iVar1 = iVar1 + -1;
      bVar2 = param_1 == *in_EAX;
      in_EAX = in_EAX + 1;
    } while (!bVar2);
    if (bVar2) {
      return param_2 - (iVar1 + 1);
    }
  }
  return -1;
}



// ========== FUN_00403fc0 @ 00403fc0 ==========

int __fastcall FUN_00403fc0(int param_1,byte *param_2)

{
  byte bVar1;
  byte bVar2;
  byte *in_EAX;
  uint uVar3;
  uint uVar4;
  byte *pbVar5;
  byte *pbVar6;
  byte *pbVar7;
  byte *pbVar8;
  bool bVar9;
  
  if (0x39 < param_1) {
    uVar4 = -(int)in_EAX & 3;
    uVar3 = param_1 - uVar4;
    bVar9 = uVar4 == 0;
    do {
      pbVar6 = param_2;
      pbVar8 = in_EAX;
      if (uVar4 == 0) break;
      uVar4 = uVar4 - 1;
      pbVar8 = in_EAX + 1;
      pbVar6 = param_2 + 1;
      bVar9 = *param_2 == *in_EAX;
      param_2 = pbVar6;
      in_EAX = pbVar8;
    } while (bVar9);
    if (bVar9) {
      uVar4 = uVar3 & 3;
      uVar3 = uVar3 >> 2;
      bVar9 = uVar3 == 0;
      do {
        pbVar5 = pbVar6;
        pbVar7 = pbVar8;
        if (uVar3 == 0) break;
        uVar3 = uVar3 - 1;
        pbVar7 = pbVar8 + 4;
        pbVar5 = pbVar6 + 4;
        bVar9 = *(int *)pbVar6 == *(int *)pbVar8;
        pbVar6 = pbVar5;
        pbVar8 = pbVar7;
      } while (bVar9);
      if (!bVar9) {
        uVar4 = 4;
        pbVar5 = pbVar5 + -4;
        pbVar7 = pbVar7 + -4;
      }
      do {
        pbVar6 = pbVar5;
        pbVar8 = pbVar7;
        if (uVar4 == 0) break;
        uVar4 = uVar4 - 1;
        pbVar8 = pbVar7 + 1;
        pbVar6 = pbVar5 + 1;
        bVar1 = *pbVar7;
        bVar2 = *pbVar5;
        pbVar5 = pbVar6;
        pbVar7 = pbVar8;
      } while (bVar2 == bVar1);
    }
    return (uint)pbVar8[-1] - (uint)pbVar6[-1];
  }
  if (param_1 == 0) {
    return 0;
  }
  do {
    pbVar6 = param_2;
    bVar2 = *in_EAX;
    in_EAX = in_EAX + 1;
    if (bVar2 != *pbVar6) break;
    param_1 = param_1 + -1;
    param_2 = pbVar6 + 1;
  } while (param_1 != 0);
  return (uint)bVar2 - (uint)*pbVar6;
}



// ========== FUN_00404030 @ 00404030 ==========

int __fastcall FUN_00404030(uint param_1,ushort *param_2)

{
  ushort uVar1;
  ushort uVar2;
  ushort *in_EAX;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  uint uVar6;
  int *piVar7;
  int *piVar8;
  ushort *puVar9;
  int *piVar10;
  int *piVar11;
  ushort *puVar12;
  bool bVar13;
  
  if (0x20 < (int)param_1) {
    if (*(int *)in_EAX == *(int *)param_2) {
      uVar5 = -(int)in_EAX & 3;
      uVar3 = param_1 * 2 - uVar5;
      uVar4 = uVar3 & 3;
      uVar6 = -(int)in_EAX & 1;
      uVar3 = uVar3 >> 2;
      bVar13 = uVar3 == 0;
      piVar7 = (int *)((int)param_2 + uVar5);
      piVar10 = (int *)((int)in_EAX + uVar5);
      do {
        piVar8 = piVar7;
        piVar11 = piVar10;
        if (uVar3 == 0) break;
        uVar3 = uVar3 - 1;
        piVar11 = piVar10 + 1;
        piVar8 = piVar7 + 1;
        bVar13 = *piVar7 == *piVar10;
        piVar7 = piVar8;
        piVar10 = piVar11;
      } while (bVar13);
      if (!bVar13) {
        piVar8 = piVar8 + -1;
        piVar11 = piVar11 + -1;
        uVar4 = 5;
      }
      param_2 = (ushort *)((int)piVar8 - uVar6);
      in_EAX = (ushort *)((int)piVar11 - uVar6);
      param_1 = uVar4 + uVar6 >> 1;
    }
    do {
      puVar9 = param_2;
      puVar12 = in_EAX;
      if (param_1 == 0) break;
      param_1 = param_1 - 1;
      puVar12 = in_EAX + 1;
      puVar9 = param_2 + 1;
      uVar1 = *in_EAX;
      uVar2 = *param_2;
      param_2 = puVar9;
      in_EAX = puVar12;
    } while (uVar2 == uVar1);
    return (uint)puVar12[-1] - (uint)puVar9[-1];
  }
  if (param_1 == 0) {
    return 0;
  }
  do {
    puVar9 = param_2;
    uVar2 = *in_EAX;
    in_EAX = in_EAX + 1;
    if (uVar2 != *puVar9) break;
    param_1 = param_1 - 1;
    param_2 = puVar9 + 1;
  } while (param_1 != 0);
  return (uint)uVar2 - (uint)*puVar9;
}



// ========== FUN_004040c0 @ 004040c0 ==========

void __fastcall FUN_004040c0(byte *param_1,uint param_2)

{
  undefined *in_EAX;
  uint uVar1;
  uint uVar2;
  uint uVar3;
  byte *pbVar4;
  byte *pbVar5;
  
  pbVar4 = param_1 + 1;
  uVar2 = (uint)*param_1;
  if (param_2 < *param_1) {
    uVar2 = param_2;
  }
  pbVar5 = in_EAX + 1;
  *in_EAX = (char)uVar2;
  if (6 < (int)uVar2) {
    uVar3 = -(int)pbVar5 & 3;
    uVar1 = uVar2 - uVar3;
    for (; uVar3 != 0; uVar3 = uVar3 - 1) {
      *pbVar5 = *pbVar4;
      pbVar4 = pbVar4 + 1;
      pbVar5 = pbVar5 + 1;
    }
    uVar2 = uVar1 & 3;
    for (uVar1 = uVar1 >> 2; uVar1 != 0; uVar1 = uVar1 - 1) {
      *(undefined4 *)pbVar5 = *(undefined4 *)pbVar4;
      pbVar4 = pbVar4 + 4;
      pbVar5 = pbVar5 + 4;
    }
  }
  for (; uVar2 != 0; uVar2 = uVar2 - 1) {
    *pbVar5 = *pbVar4;
    pbVar4 = pbVar4 + 1;
    pbVar5 = pbVar5 + 1;
  }
  return;
}



// ========== FUN_00404110 @ 00404110 ==========

int __fastcall FUN_00404110(undefined4 param_1,byte *param_2)

{
  byte *in_EAX;
  uint uVar1;
  uint uVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  byte *pbVar6;
  byte *pbVar7;
  byte *pbVar8;
  bool bVar9;
  
  uVar1 = (uint)*param_2;
  uVar5 = (uint)*in_EAX;
  param_2 = param_2 + 1;
  pbVar7 = in_EAX + 1;
  uVar3 = uVar1;
  if (uVar5 < uVar1) {
    uVar3 = uVar5;
  }
  if (uVar3 < 7) {
LAB_0040415f:
    bVar9 = uVar3 == 0;
    do {
      pbVar6 = param_2;
      pbVar8 = pbVar7;
      if (uVar3 == 0) break;
      uVar3 = uVar3 - 1;
      pbVar8 = pbVar7 + 1;
      pbVar6 = param_2 + 1;
      bVar9 = *param_2 == *pbVar7;
      param_2 = pbVar6;
      pbVar7 = pbVar8;
    } while (bVar9);
    if (bVar9) goto LAB_0040416f;
  }
  else {
    uVar4 = -(int)pbVar7 & 3;
    uVar2 = uVar3 - uVar4;
    bVar9 = uVar4 == 0;
    do {
      pbVar6 = param_2;
      pbVar8 = pbVar7;
      if (uVar4 == 0) break;
      uVar4 = uVar4 - 1;
      pbVar8 = pbVar7 + 1;
      pbVar6 = param_2 + 1;
      bVar9 = *param_2 == *pbVar7;
      param_2 = pbVar6;
      pbVar7 = pbVar8;
    } while (bVar9);
    if (bVar9) {
      uVar3 = uVar2 & 3;
      uVar2 = uVar2 >> 2;
      bVar9 = uVar2 == 0;
      do {
        param_2 = pbVar6;
        pbVar7 = pbVar8;
        if (uVar2 == 0) break;
        uVar2 = uVar2 - 1;
        pbVar7 = pbVar8 + 4;
        param_2 = pbVar6 + 4;
        bVar9 = *(int *)pbVar6 == *(int *)pbVar8;
        pbVar6 = param_2;
        pbVar8 = pbVar7;
      } while (bVar9);
      if (!bVar9) {
        uVar3 = 4;
        param_2 = param_2 + -4;
        pbVar7 = pbVar7 + -4;
      }
      goto LAB_0040415f;
    }
  }
  uVar1 = (uint)pbVar6[-1];
  uVar5 = (uint)pbVar8[-1];
LAB_0040416f:
  return uVar5 - uVar1;
}



// ========== FUN_00404180 @ 00404180 ==========

uint * __fastcall FUN_00404180(uint *param_1)

{
  char *in_EAX;
  uint uVar1;
  byte bVar2;
  uint uVar3;
  uint uVar4;
  byte bVar5;
  uint uVar6;
  uint *puVar7;
  uint *puVar8;
  uint *puVar9;
  
  uVar3 = 1;
  if (param_1 != (uint *)0x0) {
    uVar6 = ((uint)((int)param_1 + 3U) & 0xfffffffc) - (int)param_1;
    uVar4 = uVar3;
    puVar7 = param_1;
    puVar9 = (uint *)(in_EAX + 1);
    puVar8 = (uint *)(in_EAX + 1);
    if (uVar6 != 0) {
      do {
        bVar2 = *(byte *)puVar7;
        param_1 = (uint *)(uint)bVar2;
        puVar7 = (uint *)((int)puVar7 + 1);
        if (bVar2 == 0) goto LAB_00404226;
        puVar8 = (uint *)((int)puVar9 + 1);
        uVar3 = (uint)(byte)((char)uVar3 + 1);
        bVar5 = (char)uVar6 - 1;
        uVar6 = (uint)bVar5;
        *(byte *)puVar9 = bVar2;
        uVar4 = uVar3;
        puVar9 = puVar8;
      } while (bVar5 != 0);
    }
    do {
      uVar6 = *puVar7;
      puVar9 = puVar8 + 1;
      puVar7 = puVar7 + 1;
      uVar3 = uVar4 + 4;
      uVar1 = uVar6 + 0xfefefeff & ~uVar6 & 0x80808080;
      *puVar8 = uVar6;
      if (uVar1 != 0) {
        param_1 = (uint *)(uVar1 >> 8);
        uVar3 = uVar4;
        if ((uVar1 >> 7 & 1) == 0) {
          uVar3 = uVar4 + 1;
          param_1 = (uint *)(uVar1 >> 0x10);
          if ((uVar1 >> 0xf & 1) == 0) {
            uVar3 = uVar4 + 2;
            param_1 = (uint *)(uVar1 >> 0x18);
            if ((uVar1 >> 0x17 & 1) == 0) {
              uVar3 = uVar4 + 3;
            }
          }
        }
        goto LAB_00404226;
      }
      uVar4 = uVar3;
      puVar8 = puVar9;
    } while (uVar3 < 0xfd);
    param_1 = (uint *)0x0;
    if ((char)uVar3 != '\0') {
      param_1 = (uint *)*puVar7;
      do {
        if ((byte)param_1 == 0) break;
        *(byte *)puVar9 = (byte)param_1;
        param_1 = (uint *)((uint)param_1 >> 8);
        puVar9 = (uint *)((int)puVar9 + 1);
        bVar2 = (char)uVar3 + 1;
        uVar3 = (uint)bVar2;
      } while (bVar2 != 0);
    }
  }
LAB_00404226:
  *in_EAX = (char)uVar3 + -1;
  return param_1;
}



// ========== FUN_004042e0 @ 004042e0 ==========

void FUN_004042e0(void)

{
  int *piVar1;
  int iVar2;
  char cVar3;
  int *in_EAX;
  
  if (*in_EAX != 0) {
    iVar2 = *in_EAX;
    *in_EAX = 0;
    if (-1 < *(int *)(iVar2 + -8)) {
      if (DAT_007ab6a0 == 0) {
        piVar1 = (int *)(iVar2 + -8);
        *piVar1 = *piVar1 + -1;
        if (*piVar1 != 0) {
          return;
        }
      }
      else {
        cVar3 = FUN_00404260();
        if (cVar3 == '\0') {
          return;
        }
      }
      FUN_00411bb0();
    }
  }
  return;
}



// ========== FUN_004043a0 @ 004043a0 ==========

unkbyte10 FUN_004043a0(float10 param_1)

{
  float10 fVar1;
  unkbyte10 Var2;
  float10 fVar3;
  
  fVar1 = ROUND((float10)1.4426950408889634 * param_1);
  fVar3 = (float10)1.4426950408889634 *
          ((param_1 - (float10)DAT_007ab9a0 * fVar1) - (float10)DAT_007ab9b0 * fVar1);
  if (NAN((float10)1) || NAN(ABS(fVar3))) {
LAB_004043f0:
    fVar3 = (float10)0;
  }
  else if ((float10)1 < ABS(fVar3)) {
    if (ABS(fVar1) < (float10)DAT_007ab9c0) {
      fVar3 = (float10)f2xm1(fVar3 * (float10)DAT_007ab9e0);
      fVar3 = (fVar3 + (float10)DAT_007ab9d0) * fVar3;
      goto LAB_0040440c;
    }
    goto LAB_004043f0;
  }
  fVar3 = (float10)f2xm1(fVar3);
LAB_0040440c:
  Var2 = fscale((float10)1 + fVar3,fVar1);
  return Var2;
}



// ========== FUN_00404420 @ 00404420 ==========

float10 FUN_00404420(float10 param_1)

{
  return param_1 - ROUND(param_1);
}



// ========== FUN_00404490 @ 00404490 ==========

int FUN_00404490(uint param_1,uint param_2,uint param_3,uint param_4)

{
  ulonglong uVar1;
  longlong lVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  uint uVar6;
  uint uVar7;
  uint uVar8;
  uint uVar9;
  bool bVar10;
  
  if (param_4 == 0 && param_3 == 0) {
    iVar3 = FUN_004116c0();
  }
  else {
    uVar8 = (int)(param_4 ^ param_2) >> 0x1f;
    uVar9 = (int)param_2 >> 0x1f;
    uVar4 = (param_1 ^ uVar9) - uVar9;
    uVar6 = ((param_2 ^ uVar9) - uVar9) - (uint)((param_1 ^ uVar9) < uVar9);
    uVar9 = (int)param_4 >> 0x1f;
    uVar7 = (param_3 ^ uVar9) - uVar9;
    uVar9 = ((param_4 ^ uVar9) - uVar9) - (uint)((param_3 ^ uVar9) < uVar9);
    if (uVar9 == 0) {
      if (uVar6 < uVar7) {
        return ((uint)(CONCAT44(uVar6,uVar4) / (ulonglong)uVar7) ^ uVar8) - uVar8;
      }
      uVar4 = (uint)(((ulonglong)uVar6 % (ulonglong)uVar7 << 0x20 | (ulonglong)uVar4) /
                    (ulonglong)uVar7);
    }
    else {
      bVar10 = (uVar9 & 1) != 0;
      iVar3 = 0x1f;
      if (uVar9 != 0) {
        for (; uVar9 >> iVar3 == 0; iVar3 = iVar3 + -1) {
        }
      }
      bVar5 = (byte)iVar3;
      uVar1 = CONCAT44((uVar6 >> 1) >> (bVar5 & 0x1f),
                       (uVar4 >> 1 | (uint)((uVar6 & 1) != 0) << 0x1f) >> (bVar5 & 0x1f) |
                       (uVar6 >> 1) << 0x20 - (bVar5 & 0x1f)) /
              (ulonglong)
              ((uVar7 >> 1 | (uint)bVar10 << 0x1f) >> (bVar5 & 0x1f) |
              (uVar9 >> 1 | (uint)bVar10 << 0x1f) << 0x20 - (bVar5 & 0x1f));
      iVar3 = (int)uVar1;
      lVar2 = (uVar1 & 0xffffffff) * (ulonglong)uVar7;
      uVar9 = (int)((ulonglong)lVar2 >> 0x20) + ((uVar9 >> 1) << 1 | (uint)bVar10) * iVar3;
      uVar4 = iVar3 - (uint)(uVar6 < uVar9 || uVar6 - uVar9 < (uint)(uVar4 < (uint)lVar2));
    }
    iVar3 = (uVar4 ^ uVar8) - uVar8;
  }
  return iVar3;
}



// ========== FUN_00404570 @ 00404570 ==========

int FUN_00404570(uint param_1,uint param_2,uint param_3,uint param_4)

{
  ulonglong uVar1;
  longlong lVar2;
  int iVar3;
  uint uVar4;
  uint uVar5;
  byte bVar6;
  uint uVar7;
  uint uVar8;
  uint uVar9;
  uint uVar10;
  bool bVar11;
  
  if (param_4 == 0 && param_3 == 0) {
    iVar3 = FUN_004116c0();
  }
  else {
    uVar9 = (int)param_2 >> 0x1f;
    uVar4 = (param_1 ^ uVar9) - uVar9;
    uVar7 = ((param_2 ^ uVar9) - uVar9) - (uint)((param_1 ^ uVar9) < uVar9);
    uVar10 = (int)param_4 >> 0x1f;
    uVar8 = (param_3 ^ uVar10) - uVar10;
    uVar10 = ((param_4 ^ uVar10) - uVar10) - (uint)((param_3 ^ uVar10) < uVar10);
    if (uVar10 == 0) {
      if (uVar7 < uVar8) {
        return ((uint)(CONCAT44(uVar7,uVar4) % (ulonglong)uVar8) ^ uVar9) - uVar9;
      }
      uVar4 = (uint)(((ulonglong)uVar7 % (ulonglong)uVar8 << 0x20 | (ulonglong)uVar4) %
                    (ulonglong)uVar8);
    }
    else {
      bVar11 = (uVar10 & 1) != 0;
      iVar3 = 0x1f;
      if (uVar10 != 0) {
        for (; uVar10 >> iVar3 == 0; iVar3 = iVar3 + -1) {
        }
      }
      bVar6 = (byte)iVar3;
      uVar1 = CONCAT44((uVar7 >> 1) >> (bVar6 & 0x1f),
                       (uVar4 >> 1 | (uint)((uVar7 & 1) != 0) << 0x1f) >> (bVar6 & 0x1f) |
                       (uVar7 >> 1) << 0x20 - (bVar6 & 0x1f)) /
              (ulonglong)
              ((uVar8 >> 1 | (uint)bVar11 << 0x1f) >> (bVar6 & 0x1f) |
              (uVar10 >> 1 | (uint)bVar11 << 0x1f) << 0x20 - (bVar6 & 0x1f));
      lVar2 = (uVar1 & 0xffffffff) * (ulonglong)uVar8;
      uVar5 = (uint)lVar2;
      uVar10 = (int)((ulonglong)lVar2 >> 0x20) + ((uVar10 >> 1) << 1 | (uint)bVar11) * (int)uVar1;
      uVar4 = (-(uint)(uVar7 < uVar10 || uVar7 - uVar10 < (uint)(uVar4 < uVar5)) & uVar8) +
              (uVar4 - uVar5);
    }
    iVar3 = (uVar4 ^ uVar9) - uVar9;
  }
  return iVar3;
}



// ========== FUN_00404660 @ 00404660 ==========

int FUN_00404660(uint param_1,uint param_2,uint param_3,uint param_4)

{
  ulonglong uVar1;
  longlong lVar2;
  int iVar3;
  byte bVar4;
  uint uVar5;
  uint uVar6;
  bool bVar7;
  
  if (param_4 == 0 && param_3 == 0) {
    iVar3 = FUN_004116c0();
  }
  else if (param_4 == 0) {
    if (param_2 < param_3) {
      iVar3 = (int)(CONCAT44(param_2,param_1) / (ulonglong)param_3);
    }
    else {
      iVar3 = (int)(((ulonglong)param_2 % (ulonglong)param_3 << 0x20 | (ulonglong)param_1) /
                   (ulonglong)param_3);
    }
  }
  else {
    bVar7 = (param_4 & 1) != 0;
    iVar3 = 0x1f;
    if (param_4 != 0) {
      for (; param_4 >> iVar3 == 0; iVar3 = iVar3 + -1) {
      }
    }
    bVar4 = (byte)iVar3;
    uVar1 = CONCAT44((param_2 >> 1) >> (bVar4 & 0x1f),
                     (param_1 >> 1 | (uint)((param_2 & 1) != 0) << 0x1f) >> (bVar4 & 0x1f) |
                     (param_2 >> 1) << 0x20 - (bVar4 & 0x1f)) /
            (ulonglong)
            ((param_3 >> 1 | (uint)bVar7 << 0x1f) >> (bVar4 & 0x1f) |
            (param_4 >> 1 | (uint)bVar7 << 0x1f) << 0x20 - (bVar4 & 0x1f));
    iVar3 = (int)uVar1;
    uVar6 = ((param_4 >> 1) << 1 | (uint)bVar7) * iVar3;
    lVar2 = (uVar1 & 0xffffffff) * (ulonglong)param_3;
    uVar5 = (uint)((ulonglong)lVar2 >> 0x20);
    bVar7 = CARRY4(uVar5,uVar6);
    uVar5 = uVar5 + uVar6;
    iVar3 = iVar3 - (uint)(bVar7 != false ||
                          (byte)-bVar7 <
                          (param_2 < uVar5 || param_2 - uVar5 < (uint)(param_1 < (uint)lVar2)));
  }
  return iVar3;
}



// ========== FUN_00404700 @ 00404700 ==========

int FUN_00404700(uint param_1,uint param_2,uint param_3,uint param_4)

{
  ulonglong uVar1;
  longlong lVar2;
  int iVar3;
  uint uVar4;
  byte bVar5;
  uint uVar6;
  uint uVar7;
  bool bVar8;
  
  if (param_4 == 0 && param_3 == 0) {
    iVar3 = FUN_004116c0();
  }
  else if (param_4 == 0) {
    if (param_2 < param_3) {
      iVar3 = (int)(CONCAT44(param_2,param_1) % (ulonglong)param_3);
    }
    else {
      iVar3 = (int)(((ulonglong)param_2 % (ulonglong)param_3 << 0x20 | (ulonglong)param_1) %
                   (ulonglong)param_3);
    }
  }
  else {
    bVar8 = (param_4 & 1) != 0;
    iVar3 = 0x1f;
    if (param_4 != 0) {
      for (; param_4 >> iVar3 == 0; iVar3 = iVar3 + -1) {
      }
    }
    bVar5 = (byte)iVar3;
    uVar1 = CONCAT44((param_2 >> 1) >> (bVar5 & 0x1f),
                     (param_1 >> 1 | (uint)((param_2 & 1) != 0) << 0x1f) >> (bVar5 & 0x1f) |
                     (param_2 >> 1) << 0x20 - (bVar5 & 0x1f)) /
            (ulonglong)
            ((param_3 >> 1 | (uint)bVar8 << 0x1f) >> (bVar5 & 0x1f) |
            (param_4 >> 1 | (uint)bVar8 << 0x1f) << 0x20 - (bVar5 & 0x1f));
    uVar7 = ((param_4 >> 1) << 1 | (uint)bVar8) * (int)uVar1;
    lVar2 = (uVar1 & 0xffffffff) * (ulonglong)param_3;
    uVar6 = (uint)((ulonglong)lVar2 >> 0x20);
    uVar4 = (uint)lVar2;
    bVar8 = CARRY4(uVar6,uVar7);
    uVar6 = uVar6 + uVar7;
    iVar3 = (param_3 &
            -(uint)(bVar8 != false ||
                   (byte)-bVar8 < (param_2 < uVar6 || param_2 - uVar6 < (uint)(param_1 < uVar4)))) +
            (param_1 - uVar4);
  }
  return iVar3;
}



// ========== FUN_00404850 @ 00404850 ==========

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_00404850(void)

{
  undefined4 *in_EAX;
  int iVar1;
  undefined4 *puVar2;
  undefined4 *puVar3;
  
  puVar2 = in_EAX;
  puVar3 = &DAT_00995300;
  for (iVar1 = 0xb; iVar1 != 0; iVar1 = iVar1 + -1) {
    *puVar3 = *puVar2;
    puVar2 = puVar2 + 1;
    puVar3 = puVar3 + 1;
  }
  _DAT_00995330 = in_EAX[3];
  _DAT_00995340 = in_EAX[2];
  _DAT_00995350 = in_EAX[4];
  FUN_00416720();
  return;
}



// ========== FUN_00404890 @ 00404890 ==========

void FUN_00404890(void)

{
  uint in_EAX;
  short *psVar1;
  short sVar2;
  
  sVar2 = (short)in_EAX;
  if (0x12 < in_EAX) {
    if (in_EAX < 0x20) {
      sVar2 = sVar2 + 0x83;
    }
    else {
      if (in_EAX != 0x20) {
        if (in_EAX == 0x70) {
          sVar2 = 0x65;
          goto LAB_004048e8;
        }
        if ((in_EAX != 0x91) && (in_EAX != 0xb7)) {
          if (in_EAX == 0xce) {
            sVar2 = 3;
          }
          goto LAB_004048e8;
        }
      }
      sVar2 = 5;
    }
  }
LAB_004048e8:
  if (DAT_00996070 == (code *)0x0) {
    psVar1 = &DAT_00995164;
  }
  else {
    psVar1 = (short *)(*DAT_00996070)();
  }
  *psVar1 = sVar2;
  return;
}



// ========== FUN_00404b30 @ 00404b30 ==========

int __fastcall FUN_00404b30(int param_1,int *param_2)

{
  int *piVar1;
  int in_EAX;
  
  piVar1 = (int *)*param_2;
  if (piVar1 != (int *)0x0) {
    if ((in_EAX == 0) && (*piVar1 != 0)) {
      FUN_00411b20();
      *param_2 = -1;
    }
    if (in_EAX != 0) {
      FUN_00403de0();
      *(int **)(in_EAX + param_1) = piVar1;
    }
  }
  return in_EAX;
}



// ========== FUN_00404ba0 @ 00404ba0 ==========

void __fastcall FUN_00404ba0(int param_1,int param_2)

{
  int iVar1;
  int in_EAX;
  
  if (((in_EAX != 0) && (param_2 == -1)) && (*(int *)(in_EAX + param_1) != 0)) {
    iVar1 = **(int **)(in_EAX + param_1);
    if ((iVar1 == 0) || (iVar1 + (*(int **)(in_EAX + param_1))[1] != 0)) {
      FUN_00404240();
      FUN_00411690();
    }
    *(undefined4 *)(in_EAX + param_1) = 0;
    FUN_00411b60();
  }
  return;
}



// ========== FUN_00404c00 @ 00404c00 ==========

void __fastcall FUN_00404c00(undefined4 param_1,int *param_2)

{
  int *in_EAX;
  
  if (((in_EAX == (int *)0x0) || (*in_EAX == 0)) || (*in_EAX + in_EAX[1] != 0)) {
    FUN_00404240();
    FUN_00411690();
  }
  while( true ) {
    if (in_EAX == (int *)0x0) {
      FUN_00404240();
      FUN_00411690();
      return;
    }
    if (param_2 == in_EAX) break;
    if (in_EAX[2] == 0) {
      in_EAX = (int *)0x0;
    }
    else {
      in_EAX = *(int **)in_EAX[2];
    }
  }
  return;
}



// ========== FUN_00404c70 @ 00404c70 ==========

void __fastcall FUN_00404c70(byte *param_1_00,uint param_2,byte *param_1)

{
  byte bVar1;
  byte *in_EAX;
  undefined local_c;
  undefined4 local_8;
  
  local_8 = (uint)*param_1_00;
  local_c = *param_1;
  bVar1 = *param_1_00;
  if ((int)param_2 < (int)(local_8 + local_c)) {
    if ((int)param_2 < (int)local_8) {
      local_8 = param_2;
    }
    local_c = (char)param_2 - (byte)local_8;
    bVar1 = (byte)local_8;
  }
  local_8._0_1_ = bVar1;
  if (in_EAX == param_1_00) {
    FUN_00403da0();
  }
  else if (in_EAX == param_1) {
    FUN_00403da0();
    FUN_00403da0();
  }
  else {
    FUN_00403da0();
    FUN_00403da0();
  }
  *in_EAX = (byte)local_8 + local_c;
  return;
}



// ========== FUN_00404d80 @ 00404d80 ==========

void __fastcall FUN_00404d80(undefined4 *param_1_00,int param_2,int param_1)

{
  bool bVar1;
  byte *in_EAX;
  uint uVar2;
  byte local_124;
  uint local_120;
  int local_11c;
  byte local_104 [256];
  
  if (param_1 == 0) {
    *in_EAX = 0;
  }
  else {
    uVar2 = (uint)(in_EAX == (byte *)*param_1_00);
    bVar1 = false;
    if ((int)uVar2 <= param_1) {
      local_11c = uVar2 - 1;
      do {
        local_11c = local_11c + 1;
        if (in_EAX == (byte *)param_1_00[local_11c]) {
          bVar1 = true;
          break;
        }
      } while (local_11c < param_1);
    }
    if (bVar1) {
      uVar2 = 0;
      local_104[0] = 0;
      in_EAX = local_104;
    }
    else if (uVar2 == 0) {
      *in_EAX = 0;
    }
    local_120 = (uint)*in_EAX;
    if ((int)uVar2 <= param_1) {
      local_11c = uVar2 - 1;
      do {
        local_11c = local_11c + 1;
        if ((byte *)param_1_00[local_11c] != (byte *)0x0) {
          local_124 = *(byte *)param_1_00[local_11c];
          if (param_2 < (int)(local_124 + local_120)) {
            local_124 = (char)param_2 - (char)local_120;
          }
          FUN_00403da0();
          local_120 = local_120 + local_124;
        }
      } while (local_11c < param_1);
    }
    *in_EAX = (byte)local_120;
    if (bVar1) {
      FUN_004040c0();
    }
  }
  return;
}



// ========== FUN_00404f70 @ 00404f70 ==========

void __fastcall FUN_00404f70(undefined4 param_1_00,int param_2_00,char param_1,int param_2)

{
  undefined *in_EAX;
  int iVar1;
  int local_8;
  
  param_2 = param_2 + 1;
  local_8 = param_2_00;
  if ((param_2 < param_2_00 + 1) && (local_8 = param_2, param_2 < 0)) {
    local_8 = 0;
  }
  if ((param_1 != '\0') && (iVar1 = FUN_00403e70(), -1 < iVar1)) {
    local_8._0_1_ = (undefined)iVar1;
  }
  FUN_00403da0();
  *in_EAX = (undefined)local_8;
  return;
}



// ========== FUN_00405027 @ 00405027 ==========

void __fastcall FUN_00405027(undefined4 param_1,undefined4 param_2)

{
  int iVar1;
  uint in_EAX;
  uint uVar2;
  int unaff_EBP;
  char *pcVar3;
  char *pcVar4;
  
  *(undefined4 *)(unaff_EBP + -0x28) = param_2;
  *(undefined4 *)(unaff_EBP + -0x30) = param_1;
  iVar1 = *(int *)(unaff_EBP + -0x28) + 1;
  *(int *)(unaff_EBP + -0x2c) = iVar1;
  *(int *)(unaff_EBP + -0x34) = iVar1;
  if ((int)in_EAX < 0) {
    **(undefined **)(unaff_EBP + -0x34) = 0x2d;
    *(int *)(unaff_EBP + -0x34) = *(int *)(unaff_EBP + -0x34) + 1;
    in_EAX = -in_EAX;
  }
  pcVar4 = (char *)(unaff_EBP + -0x24);
  pcVar3 = pcVar4;
  do {
    uVar2 = in_EAX / 10;
    pcVar3 = pcVar3 + 1;
    *pcVar3 = (char)in_EAX + (char)uVar2 * -10 + '0';
    in_EAX = uVar2;
  } while (uVar2 != 0);
  if (0 < (int)(pcVar3 + (((*(int *)(unaff_EBP + -0x34) - *(int *)(unaff_EBP + -0x2c)) - (int)pcVar4
                          ) - *(int *)(unaff_EBP + -0x30)))) {
    pcVar4 = pcVar4 + (int)(pcVar3 + (((*(int *)(unaff_EBP + -0x34) - *(int *)(unaff_EBP + -0x2c)) -
                                      (int)pcVar4) - *(int *)(unaff_EBP + -0x30)));
  }
  for (; pcVar4 < pcVar3; pcVar3 = pcVar3 + -1) {
    **(char **)(unaff_EBP + -0x34) = *pcVar3;
    *(int *)(unaff_EBP + -0x34) = *(int *)(unaff_EBP + -0x34) + 1;
  }
  **(char **)(unaff_EBP + -0x28) =
       (char)*(undefined4 *)(unaff_EBP + -0x34) - (char)*(undefined4 *)(unaff_EBP + -0x2c);
  return;
}



// ========== FUN_004050d0 @ 004050d0 ==========

void __fastcall FUN_004050d0(int param_1,char *param_2)

{
  char *pcVar1;
  uint in_EAX;
  char *pcVar2;
  uint uVar3;
  char *pcVar4;
  char *local_38;
  char local_28 [36];
  
  pcVar1 = param_2 + 1;
  pcVar4 = local_28;
  pcVar2 = pcVar4;
  do {
    pcVar2 = pcVar2 + 1;
    uVar3 = in_EAX / 10;
    *pcVar2 = (char)in_EAX + (char)uVar3 * -10 + '0';
    in_EAX = uVar3;
  } while (uVar3 != 0);
  local_38 = pcVar1;
  if (0 < (int)(pcVar2 + (-param_1 - (int)pcVar4))) {
    pcVar4 = pcVar4 + (int)(pcVar2 + (-param_1 - (int)pcVar4));
  }
  for (; pcVar4 < pcVar2; pcVar2 = pcVar2 + -1) {
    *local_38 = *pcVar2;
    local_38 = local_38 + 1;
  }
  *param_2 = (char)local_38 - (char)pcVar1;
  return;
}



// ========== FUN_00405160 @ 00405160 ==========

void __fastcall FUN_00405160(undefined4 param_1_00,int param_2_00,int param_1,uint param_2)

{
  char *pcVar1;
  longlong lVar2;
  char *in_EAX;
  longlong lVar3;
  char *local_44;
  int local_40;
  char *local_3c;
  char *local_38;
  char local_28 [36];
  
  pcVar1 = in_EAX + 1;
  if ((int)param_2 < 0) {
    *pcVar1 = '-';
    local_40 = -param_1;
    param_2 = (~param_2 + 1) - (uint)(param_1 != 0);
    local_38 = in_EAX + 2;
  }
  else {
    local_40 = param_1;
    local_38 = pcVar1;
  }
  local_3c = local_28;
  local_44 = local_3c;
  lVar2 = CONCAT44(param_2,local_40);
  do {
    lVar3 = FUN_00404660(lVar2,10,0);
    local_44 = local_44 + 1;
    *local_44 = (char)lVar2 + (char)lVar3 * -10 + '0';
    lVar2 = lVar3;
  } while (lVar3 != 0);
  if (0 < (int)(local_44 + (int)(local_38 + ((-(int)pcVar1 - (int)local_3c) - param_2_00)))) {
    local_3c = local_3c +
               (int)(local_44 + (int)(local_38 + ((-(int)pcVar1 - (int)local_3c) - param_2_00)));
  }
  for (; local_3c < local_44; local_44 = local_44 + -1) {
    *local_38 = *local_44;
    local_38 = local_38 + 1;
  }
  *in_EAX = (char)local_38 - (char)pcVar1;
  return;
}



// ========== FUN_00405250 @ 00405250 ==========

void __fastcall
FUN_00405250(undefined4 param_1_00,int param_2_00,undefined4 param_1,undefined4 param_2)

{
  char *pcVar1;
  longlong lVar2;
  char *in_EAX;
  char *pcVar3;
  longlong lVar4;
  char *local_64;
  char *local_58;
  char local_48 [68];
  
  pcVar1 = in_EAX + 1;
  local_64 = local_48;
  pcVar3 = local_64;
  lVar2 = CONCAT44(param_2,param_1);
  do {
    pcVar3 = pcVar3 + 1;
    lVar4 = FUN_00404660(lVar2,10,0);
    *pcVar3 = (char)lVar2 + (char)lVar4 * -10 + '0';
    lVar2 = lVar4;
  } while (lVar4 != 0);
  local_58 = pcVar1;
  if (0 < (int)(pcVar3 + (-param_2_00 - (int)local_64))) {
    local_64 = local_64 + (int)(pcVar3 + (-param_2_00 - (int)local_64));
  }
  for (; local_64 < pcVar3; pcVar3 = pcVar3 + -1) {
    *local_58 = *pcVar3;
    local_58 = local_58 + 1;
  }
  *in_EAX = (char)local_58 - (char)pcVar1;
  return;
}



// ========== FUN_00405350 @ 00405350 ==========

undefined8 FUN_00405350(uint param_1,uint param_2)

{
  uint uVar1;
  uint uVar2;
  
  uVar1 = (param_1 & 0xff00ff) << 8 | param_1 >> 8 & 0xff00ff;
  uVar2 = (param_2 & 0xff00ff) << 8 | param_2 >> 8 & 0xff00ff;
  return CONCAT44(uVar1 << 0x10 | uVar1 >> 0x10,uVar2 << 0x10 | uVar2 >> 0x10);
}



// ========== FUN_004053d0 @ 004053d0 ==========

undefined8 FUN_004053d0(uint param_1,uint param_2)

{
  uint uVar1;
  uint uVar2;
  
  uVar1 = (param_1 & 0xff00ff) << 8 | param_1 >> 8 & 0xff00ff;
  uVar2 = (param_2 & 0xff00ff) << 8 | param_2 >> 8 & 0xff00ff;
  return CONCAT44(uVar1 << 0x10 | uVar1 >> 0x10,uVar2 << 0x10 | uVar2 >> 0x10);
}



// ========== FUN_00405450 @ 00405450 ==========

void FUN_00405450(int param_1)

{
  if (0 < param_1) {
    FUN_00403de0();
  }
  FUN_00403da0();
  FUN_00403de0();
  return;
}



// ========== FUN_004054d0 @ 004054d0 ==========

void __fastcall FUN_004054d0(uint param_1,int param_2)

{
  FUN_00403da0();
  *(byte *)(param_2 + (param_1 >> 3)) =
       *(byte *)(param_2 + (param_1 >> 3)) | (byte)(1 << ((byte)param_1 & 7));
  return;
}



// ========== FUN_00405510 @ 00405510 ==========

void __fastcall FUN_00405510(int param_1_00,int param_2,int param_1)

{
  int in_EAX;
  int iVar1;
  
  if (-1 < param_1 + -1) {
    iVar1 = -1;
    do {
      iVar1 = iVar1 + 1;
      *(byte *)(param_1_00 + iVar1) = *(byte *)(param_2 + iVar1) | *(byte *)(in_EAX + iVar1);
    } while (iVar1 < param_1 + -1);
  }
  return;
}



// ========== FUN_00405560 @ 00405560 ==========

void __fastcall FUN_00405560(int param_1_00,int param_2,int param_1)

{
  int in_EAX;
  int iVar1;
  
  if (-1 < param_1 + -1) {
    iVar1 = -1;
    do {
      iVar1 = iVar1 + 1;
      *(byte *)(param_1_00 + iVar1) = *(byte *)(param_2 + iVar1) & *(byte *)(in_EAX + iVar1);
    } while (iVar1 < param_1 + -1);
  }
  return;
}



// ========== FUN_004055b0 @ 004055b0 ==========

void __fastcall FUN_004055b0(int param_1_00,int param_2,int param_1)

{
  int in_EAX;
  int iVar1;
  
  if (-1 < param_1 + -1) {
    iVar1 = -1;
    do {
      iVar1 = iVar1 + 1;
      *(byte *)(param_1_00 + iVar1) = ~*(byte *)(param_2 + iVar1) & *(byte *)(in_EAX + iVar1);
    } while (iVar1 < param_1 + -1);
  }
  return;
}



// ========== FUN_004055f0 @ 004055f0 ==========

undefined __fastcall FUN_004055f0(int param_1,int param_2)

{
  int in_EAX;
  int iVar1;
  
  if (-1 < param_1 + -1) {
    iVar1 = -1;
    do {
      iVar1 = iVar1 + 1;
      if (*(char *)(in_EAX + iVar1) != *(char *)(param_2 + iVar1)) {
        return 0;
      }
    } while (iVar1 < param_1 + -1);
  }
  return 1;
}



// ========== FUN_00405690 @ 00405690 ==========

void __fastcall FUN_00405690(int param_1_00,int param_2,undefined *param_1)

{
  byte *in_EAX;
  
  if (param_1_00 < 0) {
    param_1_00 = 0;
  }
  if (param_2 < 2) {
    param_2 = 0;
  }
  else {
    param_2 = param_2 + -1;
  }
  if ((int)(uint)*in_EAX < param_2) {
    param_1_00 = 0;
  }
  else if ((int)((uint)*in_EAX - param_2) < param_1_00) {
    param_1_00 = (uint)*in_EAX - param_2;
  }
  *param_1 = (char)param_1_00;
  FUN_00403da0();
  return;
}



// ========== FUN_004056f0 @ 004056f0 ==========

int __fastcall FUN_004056f0(uint param_1,byte *param_2)

{
  byte in_AL;
  byte *pbVar1;
  int iVar2;
  
  if ((0 < (int)param_1) && ((int)param_1 <= (int)(uint)*param_2)) {
    pbVar1 = param_2 + (param_1 & 0xff);
    if ((int)param_1 <= (int)(uint)*param_2) {
      iVar2 = param_1 - 1;
      do {
        iVar2 = iVar2 + 1;
        if (in_AL == *pbVar1) {
          return iVar2;
        }
        pbVar1 = pbVar1 + 1;
      } while (iVar2 < (int)(uint)*param_2);
    }
  }
  return 0;
}



// ========== FUN_004057e0 @ 004057e0 ==========

void __fastcall FUN_004057e0(byte *param_1,byte param_2)

{
  uint in_EAX;
  uint uVar1;
  
  *param_1 = param_2;
  if (param_2 != 0) {
    uVar1 = param_2 + 1;
    do {
      uVar1 = uVar1 - 1;
      param_1[uVar1 & 0xff] = s_0123456789ABCDEF_007ab9f0[in_EAX & 0xf];
      in_EAX = in_EAX >> 4;
    } while (1 < (int)uVar1);
  }
  return;
}



// ========== FUN_00405820 @ 00405820 ==========

void __fastcall FUN_00405820(undefined4 param_1_00,byte *param_2_00,uint param_1,uint param_2)

{
  byte in_AL;
  uint uVar1;
  uint local_8;
  
  local_8 = param_2;
  *param_2_00 = in_AL;
  if (in_AL != 0) {
    uVar1 = in_AL + 1;
    do {
      uVar1 = uVar1 - 1;
      param_2_00[uVar1 & 0xff] = s_0123456789ABCDEF_007ab9f0[param_1 & 0xf];
      param_1 = param_1 >> 4 | local_8 << 0x1c;
      local_8 = local_8 >> 4;
    } while (1 < (int)uVar1);
  }
  return;
}



// ========== FUN_00405900 @ 00405900 ==========

void __fastcall FUN_00405900(byte *param_1,int param_2)

{
  FUN_00405020();
  if ((int)(uint)*param_1 < param_2) {
    FUN_004058e0(param_1);
    FUN_00404c70(param_1);
    FUN_004040c0();
  }
  return;
}



// ========== FUN_00405970 @ 00405970 ==========

void __fastcall FUN_00405970(byte *param_1,int param_2)

{
  FUN_004050d0();
  if ((int)(uint)*param_1 < param_2) {
    FUN_004058e0(param_1);
    FUN_00404c70(param_1);
    FUN_004040c0();
  }
  return;
}



// ========== FUN_004059e0 @ 004059e0 ==========

void __fastcall
FUN_004059e0(undefined4 param_1_00,byte *param_2_00,undefined4 param_1,undefined4 param_2)

{
  int in_EAX;
  
  FUN_00405250(param_1,param_2);
  if ((int)(uint)*param_2_00 < in_EAX) {
    FUN_004058e0(param_2_00);
    FUN_00404c70(param_2_00);
    FUN_004040c0();
  }
  return;
}



// ========== FUN_00405a70 @ 00405a70 ==========

void __fastcall
FUN_00405a70(undefined4 param_1_00,byte *param_2_00,undefined4 param_1,undefined4 param_2)

{
  int in_EAX;
  
  FUN_00405160(param_1,param_2);
  if ((int)(uint)*param_2_00 < in_EAX) {
    FUN_004058e0(param_2_00);
    FUN_00404c70(param_2_00);
    FUN_004040c0();
  }
  return;
}



// ========== FUN_00405b00 @ 00405b00 ==========

void __fastcall FUN_00405b00(ushort param_1,uint *param_2)

{
  uint *in_EAX;
  int iVar1;
  byte bVar2;
  byte bVar3;
  uint uVar4;
  uint uVar5;
  
  if (param_1 != 0) {
    if (param_1 == 1) {
      uVar5 = *param_2 + *param_2;
      uVar4 = param_2[1] + param_2[1] + (uint)CARRY4(*param_2,*param_2);
      if ((uVar4 < param_2[1]) || ((uVar4 <= param_2[1] && (uVar5 < *param_2)))) {
        iVar1 = 1;
      }
      else {
        iVar1 = 0;
      }
      *in_EAX = *in_EAX + *in_EAX + iVar1;
      *param_2 = uVar5;
      param_2[1] = uVar4;
    }
    else if ((short)param_1 < 0x40) {
      bVar2 = (byte)param_1;
      if (param_1 == 0x20) {
        *in_EAX = param_2[1];
      }
      else if ((short)param_1 < 0x20) {
        *in_EAX = (*in_EAX << (bVar2 & 0x1f)) + (param_2[1] >> (0x20 - bVar2 & 0x1f));
      }
      else {
        bVar3 = (byte)(0x40U - (int)(short)param_1);
        if ((0x40U - (int)(short)param_1 & 0x20) == 0) {
          bVar3 = bVar3 & 0x1f;
          uVar4 = *param_2 >> bVar3 | param_2[1] << 0x20 - bVar3;
        }
        else {
          uVar4 = param_2[1] >> (bVar3 & 0x1f);
        }
        *in_EAX = uVar4;
      }
      uVar4 = *param_2;
      if ((param_1 & 0x20) == 0) {
        uVar5 = param_2[1] << (bVar2 & 0x1f) | uVar4 >> 0x20 - (bVar2 & 0x1f);
        uVar4 = uVar4 << (bVar2 & 0x1f);
      }
      else {
        uVar5 = uVar4 << (bVar2 & 0x1f);
        uVar4 = 0;
      }
      *param_2 = uVar4;
      param_2[1] = uVar5;
    }
    else {
      if ((short)param_1 < 0x41) {
        *in_EAX = *param_2;
      }
      else {
        if (((int)(short)param_1 - 0x40U & 0x20) == 0) {
          uVar4 = *param_2 << ((byte)((int)(short)param_1 - 0x40U) & 0x1f);
        }
        else {
          uVar4 = 0;
        }
        *in_EAX = uVar4;
      }
      *param_2 = 0;
      param_2[1] = 0;
    }
  }
  return;
}



// ========== FUN_00405c90 @ 00405c90 ==========

void __fastcall FUN_00405c90(short param_1,uint *param_2)

{
  byte bVar1;
  uint *in_EAX;
  byte bVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  uint uVar6;
  
  if (param_1 != 0) {
    if (param_1 == 1) {
      *param_2 = *param_2 >> 1 | param_2[1] << 0x1f;
      param_2[1] = param_2[1] >> 1;
      if ((*in_EAX & 1) != 0) {
        *param_2 = *param_2;
        param_2[1] = param_2[1] | 0x80000000;
      }
      *in_EAX = *in_EAX >> 1;
    }
    else {
      bVar2 = (byte)param_1;
      if (param_1 < 0x40) {
        uVar4 = *in_EAX;
        uVar3 = -(int)param_1;
        bVar1 = (byte)uVar3;
        if ((uVar3 & 0x20) == 0) {
          uVar3 = 0 << (bVar1 & 0x1f) | uVar4 >> 0x20 - (bVar1 & 0x1f);
          uVar4 = uVar4 << (bVar1 & 0x1f);
        }
        else {
          uVar3 = uVar4 << (bVar1 & 0x1f);
          uVar4 = 0;
        }
        uVar6 = param_2[1];
        if (((int)param_1 & 0x20U) == 0) {
          uVar5 = *param_2 >> (bVar2 & 0x1f) | uVar6 << 0x20 - (bVar2 & 0x1f);
          uVar6 = uVar6 >> (bVar2 & 0x1f);
        }
        else {
          uVar5 = uVar6 >> (bVar2 & 0x1f);
          uVar6 = 0;
        }
        *param_2 = uVar4 | uVar5;
        param_2[1] = uVar3 | uVar6;
        if (param_1 < 0x20) {
          *in_EAX = *in_EAX >> (bVar2 & 0x1f);
        }
        else {
          *in_EAX = 0;
        }
      }
      else {
        if (param_1 < 0x60) {
          *param_2 = *in_EAX >> (bVar2 & 0x1f);
          param_2[1] = 0;
        }
        else {
          *param_2 = 0;
          param_2[1] = 0;
        }
        *in_EAX = 0;
      }
    }
  }
  return;
}



// ========== FUN_00405df0 @ 00405df0 ==========

void __fastcall FUN_00405df0(char param_1_00,uint *param_2,uint *param_1)

{
  uint uVar1;
  uint *in_EAX;
  uint uVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  uint uVar6;
  int iVar7;
  uint uVar8;
  uint uVar9;
  uint uVar10;
  uint uVar11;
  uint uVar12;
  uint uVar13;
  uint uVar14;
  uint uVar15;
  uint uVar16;
  uint uVar17;
  uint uVar18;
  uint uVar19;
  uint local_40;
  uint local_3c;
  uint local_28;
  uint local_24;
  
  uVar6 = in_EAX[2];
  uVar19 = in_EAX[1];
  uVar10 = *in_EAX;
  uVar1 = param_2[2];
  uVar11 = param_2[1];
  uVar17 = *param_2;
  uVar2 = (uint)((ulonglong)uVar1 * (ulonglong)uVar6);
  uVar12 = (uint)((ulonglong)uVar1 * (ulonglong)uVar10 >> 0x20);
  uVar13 = (uint)((ulonglong)uVar11 * (ulonglong)uVar6 >> 0x20);
  uVar3 = (uint)((ulonglong)uVar11 * (ulonglong)uVar6);
  uVar14 = (uint)((ulonglong)uVar11 * (ulonglong)uVar10 >> 0x20);
  uVar15 = (uint)((ulonglong)uVar17 * (ulonglong)uVar6 >> 0x20);
  uVar4 = (uint)((ulonglong)uVar17 * (ulonglong)uVar6);
  uVar16 = (uint)((ulonglong)uVar17 * (ulonglong)uVar19 >> 0x20);
  uVar5 = (uint)((ulonglong)uVar17 * (ulonglong)uVar19);
  uVar17 = (uint)((ulonglong)uVar17 * (ulonglong)uVar10 >> 0x20);
  uVar18 = (uint)CARRY4(uVar5,uVar17) +
           (uint)CARRY4(uVar5 + uVar17,(uint)((ulonglong)uVar11 * (ulonglong)uVar10));
  uVar17 = uVar18 + 0x80000000;
  uVar5 = uVar17 + uVar16;
  uVar8 = uVar5 + uVar14;
  uVar9 = uVar8 + uVar4;
  local_40 = (uint)((ulonglong)uVar11 * (ulonglong)uVar19);
  uVar5 = (uint)(0x7fffffff < uVar18) + (uint)CARRY4(uVar17,uVar16) + (uint)CARRY4(uVar5,uVar14) +
          (uint)CARRY4(uVar8,uVar4) + (uint)CARRY4(uVar9,local_40) +
          (uint)CARRY4(uVar9 + local_40,(uint)((ulonglong)uVar1 * (ulonglong)uVar10));
  uVar10 = uVar15 + uVar5;
  local_3c = (uint)((ulonglong)uVar11 * (ulonglong)uVar19 >> 0x20);
  uVar11 = uVar10 + local_3c;
  uVar17 = uVar11 + uVar12;
  uVar4 = uVar17 + uVar3;
  local_28 = (uint)((ulonglong)uVar1 * (ulonglong)uVar19);
  uVar10 = (uint)CARRY4(uVar15,uVar5) + (uint)CARRY4(uVar10,local_3c) + (uint)CARRY4(uVar11,uVar12)
           + (uint)CARRY4(uVar17,uVar3) + (uint)CARRY4(uVar4,local_28);
  uVar11 = uVar13 + uVar2;
  local_24 = (uint)((ulonglong)uVar1 * (ulonglong)uVar19 >> 0x20);
  uVar19 = uVar11 + local_24;
  *param_1 = uVar4 + local_28;
  param_1[1] = uVar19 + uVar10;
  param_1[2] = (int)((ulonglong)uVar1 * (ulonglong)uVar6 >> 0x20) + (uint)CARRY4(uVar13,uVar2) +
               (uint)CARRY4(uVar11,local_24) + (uint)CARRY4(uVar19,uVar10);
  *(short *)(param_1 + 3) = *(short *)(in_EAX + 3) + *(short *)(param_2 + 3) + 0x60;
  if ((param_1_00 != '\0') && ((param_1[2] & 0x80000000) == 0)) {
    uVar19 = *param_1 + *param_1;
    uVar6 = param_1[1] + param_1[1] + (uint)CARRY4(*param_1,*param_1);
    if ((uVar6 < param_1[1]) || ((uVar6 <= param_1[1] && (uVar19 < *param_1)))) {
      iVar7 = 1;
    }
    else {
      iVar7 = 0;
    }
    param_1[2] = iVar7 + param_1[2] + param_1[2];
    *param_1 = uVar19;
    param_1[1] = uVar6;
    *(short *)(param_1 + 3) = *(short *)(param_1 + 3) + -1;
  }
  return;
}



// ========== FUN_00406067 @ 00406067 ==========

void __fastcall FUN_00406067(undefined4 param_1,undefined4 param_2)

{
  char cVar1;
  uint *puVar2;
  short in_AX;
  short sVar3;
  int iVar4;
  short sVar5;
  uint uVar6;
  int *piVar7;
  ushort uVar8;
  int unaff_EBP;
  undefined4 *puVar9;
  undefined4 *puVar10;
  uint uVar11;
  
  *(undefined4 *)(unaff_EBP + -0x2c) = param_2;
  sVar3 = DAT_007aba50 + DAT_007abd38;
  if (sVar3 < in_AX) {
    *(short *)(unaff_EBP + -0x30) = (short)(((int)in_AX - (int)sVar3) / 0x25);
    if (*(short *)(unaff_EBP + -0x30) * 0x25 + (int)sVar3 != (int)in_AX) {
      *(short *)(unaff_EBP + -0x30) = *(short *)(unaff_EBP + -0x30) + 1;
    }
    if (0x10f < *(short *)(unaff_EBP + -0x30)) {
      *(undefined2 *)(unaff_EBP + -0x30) = 0x10f;
    }
  }
  else {
    *(undefined2 *)(unaff_EBP + -0x30) = 0;
  }
  uVar6 = (int)*(short *)(unaff_EBP + -0x30) >> 0x1f & 0xf;
  uVar6 = ((int)*(short *)(unaff_EBP + -0x30) + uVar6 & 0xf) - uVar6;
  sVar3 = (short)((int)((int)*(short *)(unaff_EBP + -0x30) +
                       ((int)*(short *)(unaff_EBP + -0x30) >> 0x1f & 0xfU)) >> 4);
  sVar5 = sVar3 + -8;
  if (sVar5 == 0) {
    puVar9 = (undefined4 *)(&DAT_007aba40 + (uVar6 & 0xffff) * 0x18);
    puVar10 = *(undefined4 **)(unaff_EBP + -0x2c);
    for (iVar4 = 6; iVar4 != 0; iVar4 = iVar4 + -1) {
      *puVar10 = *puVar9;
      puVar9 = puVar9 + 1;
      puVar10 = puVar10 + 1;
    }
  }
  else {
    puVar9 = (undefined4 *)(&DAT_007aba40 + (uVar6 & 0xffff) * 0x18);
    puVar10 = (undefined4 *)(unaff_EBP + -0x18);
    for (iVar4 = 6; iVar4 != 0; iVar4 = iVar4 + -1) {
      *puVar10 = *puVar9;
      puVar9 = puVar9 + 1;
      puVar10 = puVar10 + 1;
    }
    if (sVar5 < 1) {
      *(short *)(*(int *)(unaff_EBP + -0x2c) + 0x10) =
           *(short *)(unaff_EBP + -8) + *(short *)(&DAT_007abc90 + (-(sVar5 + 1) & 0xffffU) * 0x18);
      if (*(short *)(unaff_EBP + -8) == 0) {
        puVar9 = (undefined4 *)(&DAT_007abc80 + (-(sVar5 + 1) & 0xffffU) * 0x18);
        puVar10 = *(undefined4 **)(unaff_EBP + -0x2c);
        for (iVar4 = 4; iVar4 != 0; iVar4 = iVar4 + -1) {
          *puVar10 = *puVar9;
          puVar9 = puVar9 + 1;
          puVar10 = puVar10 + 1;
        }
        return;
      }
      FUN_00405df0(unaff_EBP + -0x28);
      puVar9 = (undefined4 *)(unaff_EBP + -0x28);
      puVar10 = *(undefined4 **)(unaff_EBP + -0x2c);
      for (iVar4 = 4; iVar4 != 0; iVar4 = iVar4 + -1) {
        *puVar10 = *puVar9;
        puVar9 = puVar9 + 1;
        puVar10 = puVar10 + 1;
      }
    }
    else {
      uVar8 = sVar3 - 9;
      *(short *)(*(int *)(unaff_EBP + -0x2c) + 0x10) =
           *(short *)(&DAT_007abbd0 + (uint)uVar8 * 0x18) + *(short *)(unaff_EBP + -8);
      if (*(short *)(unaff_EBP + -8) == 0) {
        puVar9 = (undefined4 *)(&DAT_007abbc0 + (uint)uVar8 * 0x18);
        puVar10 = *(undefined4 **)(unaff_EBP + -0x2c);
        for (iVar4 = 4; iVar4 != 0; iVar4 = iVar4 + -1) {
          *puVar10 = *puVar9;
          puVar9 = puVar9 + 1;
          puVar10 = puVar10 + 1;
        }
        return;
      }
      FUN_00405df0(unaff_EBP + -0x28);
      puVar9 = (undefined4 *)(unaff_EBP + -0x28);
      puVar10 = *(undefined4 **)(unaff_EBP + -0x2c);
      for (iVar4 = 4; iVar4 != 0; iVar4 = iVar4 + -1) {
        *puVar10 = *puVar9;
        puVar9 = puVar9 + 1;
        puVar10 = puVar10 + 1;
      }
    }
    cVar1 = (&DAT_007abd40)[*(ushort *)(unaff_EBP + -0x30)];
    if (cVar1 != '\0') {
      *(undefined4 *)(unaff_EBP + -0x34) = 0;
      if (cVar1 < '\0') {
        *(int *)(unaff_EBP + -0x34) = *(int *)(unaff_EBP + -0x34) + -1;
      }
      uVar6 = (uint)cVar1;
      puVar2 = *(uint **)(unaff_EBP + -0x2c);
      piVar7 = (int *)(*(int *)(unaff_EBP + -0x2c) + 8);
      uVar11 = *puVar2 + uVar6;
      uVar6 = puVar2[1] + ((int)uVar6 >> 0x1f) + (uint)CARRY4(*puVar2,uVar6);
      if ((uVar6 < puVar2[1]) || ((uVar6 <= puVar2[1] && (uVar11 < *puVar2)))) {
        iVar4 = 1;
      }
      else {
        iVar4 = 0;
      }
      *piVar7 = iVar4 + *piVar7 + *(int *)(unaff_EBP + -0x34);
      *puVar2 = uVar11;
      puVar2[1] = uVar6;
    }
  }
  return;
}



// ========== FUN_00406290 @ 00406290 ==========

void __fastcall
FUN_00406290(int param_1_00,short param_2_00,undefined4 param_1,undefined4 param_2,
            undefined *param_3,undefined4 param_4,ushort param_5)

{
  short sVar1;
  short sVar2;
  char cVar3;
  byte bVar4;
  short in_AX;
  short sVar5;
  undefined2 uVar6;
  int iVar7;
  undefined **ppuVar8;
  uint *puVar9;
  uint uVar10;
  ushort uVar11;
  uint *puVar12;
  bool bVar13;
  uint local_e4;
  uint local_e0;
  ushort local_dc;
  short local_d0;
  undefined *local_a4 [3];
  ushort local_98;
  byte local_90 [40];
  uint local_68;
  uint local_64;
  uint local_60;
  uint local_5c;
  uint local_58;
  uint local_54;
  uint local_50;
  uint local_4c;
  uint local_48;
  bool local_44;
  ushort local_40;
  short local_2c;
  uint local_24 [3];
  ushort local_18;
  uint local_14 [3];
  short local_8;
  
  if (0xd8 < param_2_00) {
    param_2_00 = 0xd8;
  }
  if (in_AX < -0x7ffe) {
    in_AX = -1;
  }
  else if (in_AX < 0) {
    in_AX = 0;
  }
  else if (0xff < in_AX) {
    in_AX = 0xff;
  }
  sVar5 = *(short *)(s_0123456789ABCDEF_007ab9f0 + param_1_00 * 4 + 0x10);
  sVar1 = *(short *)(&DAT_007aba02 + param_1_00 * 4);
  sVar2 = DAT_007aba08;
  if (sVar5 < DAT_007aba08) {
    sVar2 = sVar5;
  }
  local_d0 = sVar5;
  if (-1 < in_AX) {
    local_d0 = (in_AX + -4) - sVar1;
    if (local_d0 < 2) {
      local_d0 = 2;
    }
    if (sVar5 < local_d0) {
      local_d0 = sVar5;
    }
  }
  local_44 = (param_5 & 0x8000) != 0;
  local_98 = param_5 & 0x7fff;
  local_a4[0] = param_3;
  local_a4[1] = (undefined *)param_4;
  local_a4[2] = (undefined *)0x0;
  ppuVar8 = local_a4;
  puVar9 = local_14;
  for (iVar7 = 4; iVar7 != 0; iVar7 = iVar7 + -1) {
    *puVar9 = (uint)*ppuVar8;
    ppuVar8 = ppuVar8 + 1;
    puVar9 = puVar9 + 1;
  }
  if (((local_8 == 0) && (local_14[1] == 0)) && (local_14[0] == 0)) {
    local_90[0] = 0;
    if ((param_2_00 < 0) ||
       (cVar3 = FUN_004073c0(param_2_00,in_AX,1,0,local_90,local_44), cVar3 == '\0')) {
      FUN_00407130(in_AX,sVar1,0,local_d0,0,local_90,local_44);
    }
  }
  else {
    if (((local_8 != 0) && (local_8 != 0x7fff)) && ((local_14[1] & 0x80000000) == 0)) {
      local_14[0] = 0xffffffff;
      local_14[1] = 0xffffffff;
      local_8 = 0x7fff;
    }
    uVar11 = (ushort)((uint)param_1_00 >> 0x10);
    if (local_8 == 0x7fff) {
      if (in_AX < 0) {
        in_AX = *(short *)(&DAT_007aba02 + param_1_00 * 4) +
                *(short *)(s_0123456789ABCDEF_007ab9f0 + param_1_00 * 4 + 0x10) + 4;
      }
      if ((local_14[1] == 0x80000000) && (local_14[0] == 0)) {
        FUN_00407020(in_AX,&DAT_007aba20,CONCAT22(uVar11,(ushort)local_44 * -2 + 1));
      }
      else {
        FUN_00407020(in_AX,&DAT_007aba30,(uint)uVar11 << 0x10);
      }
    }
    else {
      if (local_8 == 0) {
        local_a4[2] = &stack0xfffffffc;
        sVar5 = FUN_00404370(local_14[0],local_14[1]);
        sVar5 = 0x5f - sVar5;
        local_8 = local_8 + 1;
      }
      else {
        sVar5 = 0x20;
      }
      FUN_00405b00();
      local_8 = local_8 - (sVar5 + 0x403e);
      if ((local_8 < -0x5d) || (0x1e < local_8)) {
        FUN_00406be0();
        FUN_00406060();
        if (local_2c == 0) {
          puVar9 = local_14;
          puVar12 = local_24;
          for (iVar7 = 4; iVar7 != 0; iVar7 = iVar7 + -1) {
            *puVar12 = *puVar9;
            puVar9 = puVar9 + 1;
            puVar12 = puVar12 + 1;
          }
        }
        else {
          FUN_00405df0(local_24);
        }
      }
      else {
        puVar9 = local_14;
        puVar12 = local_24;
        for (iVar7 = 4; iVar7 != 0; iVar7 = iVar7 + -1) {
          *puVar12 = *puVar9;
          puVar9 = puVar9 + 1;
          puVar12 = puVar12 + 1;
        }
        local_2c = 0;
      }
      local_4c = local_24[0];
      local_48 = local_24[1];
      local_60 = local_24[2];
      if ((short)local_18 < 1) {
        FUN_00405c90();
        local_5c = local_60;
        local_58 = 0;
      }
      else {
        if ((local_18 & 0x20) == 0) {
          bVar4 = (byte)local_18 & 0x1f;
          local_58 = 0 << bVar4 | local_24[2] >> 0x20 - bVar4;
        }
        else {
          local_58 = local_24[2] << ((byte)local_18 & 0x1f);
        }
        local_5c = 0;
        FUN_00405b00();
        bVar13 = CARRY4(local_5c,local_60);
        local_5c = local_5c + local_60;
        local_58 = local_58 + bVar13;
      }
      if ((local_58 == 0) && (local_5c == 0)) {
        local_40 = FUN_00407860(0,local_4c,local_48);
      }
      else {
        FUN_00406c50(0x89e80000,0x8ac72304,local_4c,local_48,local_5c,local_58);
        local_40 = FUN_00407860(0,local_5c,local_58);
        sVar5 = FUN_00407860(0 < (short)local_40,local_4c,local_48);
        local_40 = local_40 + sVar5;
      }
      local_dc = local_40;
      local_e4 = 0;
      if ((short)local_18 < 0) {
        if ((short)local_18 < -0x3f) {
          local_54 = 0xffffffff;
          local_50 = 0xffffffff;
          local_64 = (1 << (0xc0U - (char)local_18 & 0x1f)) - 1;
        }
        else {
          bVar4 = (byte)-(int)(short)local_18;
          if ((-(int)(short)local_18 & 0x20U) == 0) {
            local_50 = 0 << (bVar4 & 0x1f) | 1U >> 0x20 - (bVar4 & 0x1f);
            iVar7 = 1 << (bVar4 & 0x1f);
          }
          else {
            local_50 = 1 << (bVar4 & 0x1f);
            iVar7 = 0;
          }
          local_54 = iVar7 - 1;
          local_50 = local_50 - (iVar7 == 0);
          local_64 = 0;
        }
        local_4c = local_24[0] & local_54;
        local_48 = local_24[1] & local_50;
        uVar11 = local_18;
        local_a4[2] = &stack0xfffffffc;
        for (local_60 = local_64 & local_24[2];
            (((short)uVar11 < -0x3d && ((int)(short)local_40 < sVar2 + 1)) &&
            ((local_48 != 0 || (local_60 != 0 || local_4c != 0)))); local_60 = local_64 & local_60)
        {
          local_5c = local_4c;
          local_58 = local_48;
          local_68 = local_60;
          FUN_00405b00();
          uVar10 = local_48 + local_58 + (uint)CARRY4(local_4c,local_5c);
          if ((uVar10 < local_48) || ((uVar10 <= local_48 && (local_4c + local_5c < local_4c)))) {
            iVar7 = 1;
          }
          else {
            iVar7 = 0;
          }
          local_60 = iVar7 + local_68 + local_60;
          local_4c = local_4c + local_5c;
          local_48 = uVar10;
          FUN_00405c90();
          uVar11 = uVar11 + 1;
          local_5c = local_4c;
          local_58 = local_48;
          local_68 = local_60;
          FUN_00405c90();
          local_90[local_40] = (byte)local_5c;
          local_4c = local_4c & local_54;
          local_48 = local_48 & local_50;
          local_40 = local_40 + 1;
        }
        if ((int)(short)local_40 < sVar2 + 1) {
          for (; (((short)uVar11 < -0x1d && ((int)(short)local_40 < sVar2 + 1)) &&
                 ((local_48 != 0 || (local_4c != 0)))); local_4c = local_4c & local_54) {
            uVar10 = local_4c >> 0x1e;
            bVar13 = CARRY4(local_4c,local_4c * 4);
            local_4c = local_4c * 5;
            local_48 = local_48 + (local_48 << 2 | uVar10) + (uint)bVar13;
            local_54 = local_54 >> 1 | local_50 << 0x1f;
            local_50 = local_50 >> 1;
            uVar11 = uVar11 + 1;
            bVar4 = (byte)-(int)(short)uVar11;
            if ((-(int)(short)uVar11 & 0x20U) == 0) {
              bVar4 = bVar4 & 0x1f;
              bVar4 = (byte)(local_4c >> bVar4) | (byte)(local_48 << 0x20 - bVar4);
            }
            else {
              bVar4 = (byte)(local_48 >> (bVar4 & 0x1f));
            }
            local_90[local_40] = bVar4;
            local_48 = local_48 & local_50;
            local_40 = local_40 + 1;
          }
          if ((int)(short)local_40 < sVar2 + 1) {
            local_e0 = local_54;
            for (local_e4 = local_4c; ((int)(short)local_40 < sVar2 + 1 && (local_e4 != 0));
                local_e4 = local_e0 & local_e4 * 5) {
              local_e0 = local_e0 >> 1;
              uVar11 = uVar11 + 1;
              local_90[local_40] = (byte)(local_e4 * 5 >> (-(char)uVar11 & 0x1fU));
              local_40 = local_40 + 1;
            }
          }
          else if ((local_48 == 0) && (local_4c == 0)) {
            local_e4 = 0;
          }
          else {
            local_e4 = 1;
          }
        }
        else if ((local_48 == 0) && (local_60 == 0 && local_4c == 0)) {
          local_e4 = 0;
        }
        else {
          local_e4 = 1;
        }
      }
      if ((local_e4 != 0) && (sVar2 + 1 <= (int)(short)local_40)) {
        local_40 = sVar2 + 2;
        local_90[sVar2 + 1] = 1;
      }
      if (sVar2 < (short)local_40) {
        sVar5 = FUN_00407700(1,sVar2);
        local_dc = local_dc + sVar5;
      }
      if ((param_2_00 < 0) ||
         (cVar3 = FUN_004073c0(param_2_00,in_AX,CONCAT22(local_2c >> 0xf,local_dc - local_2c),
                               local_40,local_90,local_44), cVar3 == '\0')) {
        if (local_d0 < (short)local_40) {
          sVar5 = FUN_00407700(0,local_d0);
          local_dc = local_dc + sVar5;
        }
        iVar7 = ((int)(short)local_dc - (int)local_2c) + -1;
        uVar6 = (undefined2)((uint)iVar7 >> 0x10);
        FUN_00407130(CONCAT22(uVar6,in_AX),CONCAT22(uVar6,sVar1),iVar7,local_d0,local_40,local_90,
                     local_44);
      }
    }
  }
  return;
}



// ========== FUN_00406be0 @ 00406be0 ==========

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined4 __fastcall FUN_00406be0(short param_1,short param_2)

{
  double dVar1;
  int iVar2;
  undefined2 local_24;
  
  iVar2 = (int)(short)(param_1 - param_2);
  dVar1 = (double)iVar2;
  local_24 = (short)(longlong)ROUND(dVar1 * _DAT_007abe80);
  if ((0 < (short)(param_1 - param_2)) &&
     (iVar2 = (uint)(ushort)(local_24 >> 0xf) << 0x10,
     dVar1 * _DAT_007abe80 != (double)(int)local_24)) {
    local_24 = local_24 + 1;
  }
  return CONCAT22((short)((uint)iVar2 >> 0x10),local_24);
}



// ========== FUN_00406c50 @ 00406c50 ==========

/* WARNING: Removing unreachable block (ram,0x00406e45) */
/* WARNING: Removing unreachable block (ram,0x00406f73) */

undefined __fastcall
FUN_00406c50(uint *param_1_00,undefined4 *param_2_00,uint param_1,uint param_2,uint param_3,
            uint param_4,uint param_5,uint param_6)

{
  longlong lVar1;
  short sVar2;
  uint uVar3;
  byte bVar4;
  byte bVar5;
  ushort uVar6;
  uint uVar7;
  uint uVar8;
  uint uVar9;
  int iVar10;
  bool bVar11;
  ulonglong uVar12;
  uint local_88;
  uint local_84;
  int local_50;
  int local_44;
  int local_30;
  int local_2c;
  undefined local_c;
  
  if ((param_2 < param_6) || ((param_2 <= param_6 && (param_1 <= param_5)))) {
    local_c = 0;
  }
  else {
    sVar2 = FUN_00404370(param_1,param_2);
    uVar6 = 0x3f - sVar2;
    bVar4 = (byte)uVar6;
    if ((uVar6 & 0x20) == 0) {
      uVar7 = param_2 << (bVar4 & 0x1f) | param_1 >> 0x20 - (bVar4 & 0x1f);
      param_1 = param_1 << (bVar4 & 0x1f);
    }
    else {
      uVar7 = param_1 << (bVar4 & 0x1f);
      param_1 = 0;
    }
    if ((uVar6 & 0x20) == 0) {
      uVar3 = param_6 << (bVar4 & 0x1f) | param_5 >> 0x20 - (bVar4 & 0x1f);
      param_5 = param_5 << (bVar4 & 0x1f);
    }
    else {
      uVar3 = param_5 << (bVar4 & 0x1f);
      param_5 = 0;
    }
    if (0 < (short)uVar6) {
      bVar5 = (byte)(0x40U - (int)(short)uVar6);
      if ((0x40U - (int)(short)uVar6 & 0x20) == 0) {
        uVar8 = param_3 >> (bVar5 & 0x1f) | param_4 << 0x20 - (bVar5 & 0x1f);
        uVar9 = param_4 >> (bVar5 & 0x1f);
      }
      else {
        uVar8 = param_4 >> (bVar5 & 0x1f);
        uVar9 = 0;
      }
      param_5 = uVar8 | param_5;
      uVar3 = uVar9 | uVar3;
    }
    if ((uVar6 & 0x20) == 0) {
      uVar8 = param_4 << (bVar4 & 0x1f) | param_3 >> 0x20 - (bVar4 & 0x1f);
      param_3 = param_3 << (bVar4 & 0x1f);
    }
    else {
      uVar8 = param_3 << (bVar4 & 0x1f);
      param_3 = 0;
    }
    uVar12 = FUN_00404660(param_5,uVar3,uVar7,0);
    lVar1 = (ulonglong)uVar7 * (uVar12 & 0xffffffff);
    uVar9 = (uint)lVar1;
    local_84 = param_5 - uVar9;
    uVar3 = (uVar3 - ((int)((ulonglong)lVar1 >> 0x20) + (int)(uVar12 >> 0x20) * uVar7)) -
            (uint)(param_5 < uVar9);
    while( true ) {
      local_30 = (int)(uVar12 >> 0x20);
      local_2c = (int)uVar12;
      if (((uVar12 < 0x200000000) && (local_30 == 0)) &&
         ((ulonglong)param_1 * (uVar12 & 0xffffffff) <= CONCAT44(local_84,uVar8))) break;
      uVar12 = CONCAT44(local_30 - (uint)(local_2c == 0),local_2c + -1);
      bVar11 = CARRY4(local_84,uVar7);
      local_84 = local_84 + uVar7;
      uVar3 = uVar3 + bVar11;
      if ((1 < uVar3) || (uVar3 != 0)) break;
    }
    local_2c = (int)uVar12;
    lVar1 = uVar12 * CONCAT44(uVar7,param_1);
    uVar3 = (uint)lVar1;
    uVar9 = uVar8 - uVar3;
    iVar10 = (param_5 - (int)((ulonglong)lVar1 >> 0x20)) - (uint)(uVar8 < uVar3);
    uVar12 = FUN_00404660(uVar9,iVar10,uVar7,0);
    lVar1 = (ulonglong)uVar7 * (uVar12 & 0xffffffff);
    uVar3 = (uint)lVar1;
    local_88 = uVar9 - uVar3;
    uVar3 = (iVar10 - ((int)((ulonglong)lVar1 >> 0x20) + (int)(uVar12 >> 0x20) * uVar7)) -
            (uint)(uVar9 < uVar3);
    while( true ) {
      local_50 = (int)(uVar12 >> 0x20);
      local_44 = (int)uVar12;
      if (((uVar12 < 0x200000000) && (local_50 == 0)) &&
         ((ulonglong)param_1 * (uVar12 & 0xffffffff) <= CONCAT44(local_88,param_3))) break;
      uVar12 = CONCAT44(local_50 - (uint)(local_44 == 0),local_44 + -1);
      bVar11 = CARRY4(local_88,uVar7);
      local_88 = local_88 + uVar7;
      uVar3 = uVar3 + bVar11;
      if ((1 < uVar3) || (uVar3 != 0)) break;
    }
    local_50 = (int)(uVar12 >> 0x20);
    local_44 = (int)uVar12;
    lVar1 = uVar12 * CONCAT44(uVar7,param_1);
    uVar7 = (uint)lVar1;
    uVar3 = (uVar9 - (int)((ulonglong)lVar1 >> 0x20)) - (uint)(param_3 < uVar7);
    if ((uVar6 & 0x20) == 0) {
      uVar7 = param_3 - uVar7 >> (bVar4 & 0x1f) | uVar3 << 0x20 - (bVar4 & 0x1f);
      uVar3 = uVar3 >> (bVar4 & 0x1f);
    }
    else {
      uVar7 = uVar3 >> (bVar4 & 0x1f);
      uVar3 = 0;
    }
    *param_1_00 = uVar7;
    param_1_00[1] = uVar3;
    *param_2_00 = local_44;
    param_2_00[1] = local_2c + local_50;
    local_c = 1;
  }
  return local_c;
}



// ========== FUN_00407020 @ 00407020 ==========

void __fastcall
FUN_00407020(undefined4 param_1_00,int param_2_00,short param_1,byte *param_2,short param_3)

{
  ushort uVar1;
  short sVar2;
  short local_1c;
  ushort local_10;
  
  local_10 = (ushort)*param_2;
  uVar1 = local_10;
  if (param_3 != 0) {
    uVar1 = local_10 + 1;
  }
  sVar2 = param_1 - uVar1;
  if ((short)(0xff - uVar1) < (short)(param_1 - uVar1)) {
    sVar2 = 0xff - uVar1;
  }
  FUN_00405670();
  local_1c = 1;
  if (0 < sVar2) {
    FUN_00404af0();
    local_1c = sVar2 + 1;
  }
  if (param_3 != 0) {
    if (param_3 < 1) {
      *(undefined *)(param_2_00 + (uint)(byte)local_1c) = 0x2d;
    }
    else {
      *(undefined *)(param_2_00 + (uint)(byte)local_1c) = 0x2b;
    }
    local_1c = local_1c + 1;
  }
  for (; 0 < (short)local_10; local_10 = local_10 - 1) {
    *(byte *)(param_2_00 + (((int)local_1c + (int)(short)local_10) - 1U & 0xff)) =
         param_2[(byte)local_10];
  }
  return;
}



// ========== FUN_00407130 @ 00407130 ==========

void __fastcall
FUN_00407130(undefined4 param_1_00,int param_2_00,short param_1,short param_2,short param_3,
            short param_4,short param_5,char *param_6,char param_7)

{
  short sVar1;
  short sVar2;
  uint uVar3;
  short sVar4;
  ushort uVar5;
  bool bVar6;
  short local_48;
  char local_30 [44];
  
  bVar6 = param_3 < 0;
  if (bVar6) {
    param_3 = -param_3;
  }
  sVar1 = FUN_004079a0(0,(int)param_3);
  sVar2 = sVar1;
  if (sVar1 <= param_2) {
    sVar2 = param_2;
  }
  sVar2 = sVar2 + param_4 + 3;
  if (1 < param_4) {
    sVar2 = sVar2 + 1;
  }
  sVar4 = param_1 - sVar2;
  if ((short)(0xff - sVar2) < (short)(param_1 - sVar2)) {
    sVar4 = 0xff - sVar2;
  }
  FUN_00405670();
  local_48 = 1;
  if (0 < sVar4) {
    FUN_00404af0();
    local_48 = sVar4 + 1;
  }
  sVar2 = local_48;
  if (param_7 == '\0') {
    *(undefined *)(param_2_00 + (uint)(byte)local_48) = 0x20;
  }
  else {
    *(undefined *)(param_2_00 + (uint)(byte)local_48) = 0x2d;
  }
  local_48._0_1_ = (byte)local_48 + 1;
  if (param_5 < 1) {
    *(undefined *)(param_2_00 + (uint)(byte)local_48) = 0x30;
  }
  else {
    *(char *)(param_2_00 + (uint)(byte)local_48) = *param_6 + '0';
  }
  local_48 = sVar2 + 2;
  if (1 < param_4) {
    *(undefined *)(param_2_00 + (uint)(byte)local_48) = 0x2e;
    local_48 = sVar2 + 3;
  }
  uVar3 = 1;
  while ((sVar2 = (short)uVar3, sVar2 < param_5 && (sVar2 < param_4))) {
    *(char *)(param_2_00 + (uint)(byte)local_48) = param_6[uVar3] + '0';
    local_48 = local_48 + 1;
    uVar3 = (uint)(ushort)(sVar2 + 1);
  }
  if (0 < (short)(param_4 - sVar2)) {
    FUN_00404af0();
    local_48 = local_48 + (param_4 - sVar2);
  }
  sVar2 = local_48;
  *(undefined *)(param_2_00 + (uint)(byte)local_48) = 0x45;
  local_48._0_1_ = (byte)local_48 + 1;
  if (bVar6) {
    *(undefined *)(param_2_00 + (uint)(byte)local_48) = 0x2d;
  }
  else {
    *(undefined *)(param_2_00 + (uint)(byte)local_48) = 0x2b;
  }
  local_48 = sVar2 + 2;
  if (0 < (short)(param_2 - sVar1)) {
    FUN_00404af0();
    local_48 = local_48 + (param_2 - sVar1);
  }
  if (-1 < (short)(sVar1 + -1)) {
    uVar5 = 0xffff;
    do {
      uVar5 = uVar5 + 1;
      *(char *)(param_2_00 + (uint)(byte)local_48) = local_30[uVar5] + '0';
      local_48 = local_48 + 1;
    } while ((short)uVar5 < (short)(sVar1 + -1));
  }
  return;
}



// ========== FUN_004073c0 @ 004073c0 ==========

undefined __fastcall
FUN_004073c0(undefined4 param_1_00,int param_2_00,short param_1,short param_2,short param_3,
            short param_4,int param_5,byte param_6)

{
  bool bVar1;
  short sVar2;
  short sVar3;
  short sVar4;
  short sVar5;
  ushort uVar6;
  short local_64;
  short local_5c;
  short local_54;
  short local_50;
  undefined local_38;
  char local_30 [44];
  
  local_38 = 0;
  bVar1 = false;
  sVar2 = param_3 + param_1;
  if (sVar2 < 0) {
    param_4 = 0;
  }
  else if (sVar2 < param_4) {
    if (0 < param_4) {
      FUN_00403da0();
    }
    sVar2 = FUN_00407700(0,sVar2);
    param_3 = param_3 + sVar2;
    bVar1 = true;
  }
  if ((param_3 < 1) || (param_4 == 0)) {
    local_64 = 0;
    local_54 = 1;
  }
  else if (param_4 < param_3) {
    local_64 = param_4;
    local_54 = param_3 - param_4;
  }
  else {
    local_54 = 0;
    local_64 = param_3;
  }
  if (param_3 < 0) {
    param_3 = -param_3;
  }
  else {
    param_3 = 0;
  }
  if (param_1 < param_3) {
    param_3 = param_1;
  }
  local_50 = param_4 - local_64;
  sVar4 = (param_1 - local_50) - param_3;
  sVar2 = (ushort)param_6 + local_64 + local_54;
  if (0 < param_1) {
    sVar2 = sVar2 + local_50 + param_3 + sVar4 + 1;
  }
  sVar3 = 0xff - sVar2;
  if (-1 < sVar3) {
    sVar5 = param_2 - sVar2;
    if (sVar3 < (short)(param_2 - sVar2)) {
      sVar5 = sVar3;
    }
    FUN_00405670();
    local_5c = 1;
    if (0 < sVar5) {
      FUN_00404af0();
      local_5c = sVar5 + 1;
    }
    if (param_6 != 0) {
      *(undefined *)(param_2_00 + (uint)(byte)local_5c) = 0x2d;
      local_5c = local_5c + 1;
    }
    uVar6 = 0;
    if (bVar1) {
      for (; 0 < local_64; local_64 = local_64 + -1) {
        *(char *)(param_2_00 + (uint)(byte)local_5c) = local_30[uVar6] + '0';
        local_5c = local_5c + 1;
        uVar6 = uVar6 + 1;
      }
    }
    else {
      for (; 0 < local_64; local_64 = local_64 + -1) {
        *(char *)(param_2_00 + (uint)(byte)local_5c) = *(char *)(param_5 + (uint)uVar6) + '0';
        local_5c = local_5c + 1;
        uVar6 = uVar6 + 1;
      }
    }
    if (0 < local_54) {
      FUN_00404af0();
      local_5c = local_5c + local_54;
    }
    if (param_1 != 0) {
      *(undefined *)(param_2_00 + (uint)(byte)local_5c) = 0x2e;
      local_5c = local_5c + 1;
      if (0 < param_3) {
        FUN_00404af0();
        local_5c = local_5c + param_3;
      }
      if (bVar1) {
        for (; 0 < local_50; local_50 = local_50 + -1) {
          *(char *)(param_2_00 + (uint)(byte)local_5c) = local_30[uVar6] + '0';
          local_5c = local_5c + 1;
          uVar6 = uVar6 + 1;
        }
      }
      else {
        for (; 0 < local_50; local_50 = local_50 + -1) {
          *(char *)(param_2_00 + (uint)(byte)local_5c) = *(char *)(param_5 + (uint)uVar6) + '0';
          local_5c = local_5c + 1;
          uVar6 = uVar6 + 1;
        }
      }
      if (0 < sVar4) {
        FUN_00404af0();
      }
    }
    local_38 = 1;
  }
  return local_38;
}



// ========== FUN_00407700 @ 00407700 ==========

undefined2 __fastcall
FUN_00407700(ushort *param_1_00,undefined *param_2_00,char param_1,ushort param_2)

{
  byte bVar1;
  char cVar2;
  ushort uVar3;
  ushort uVar4;
  ushort local_18;
  
  uVar3 = *param_1_00;
  local_18 = param_2;
  *param_1_00 = param_2;
  bVar1 = param_2_00[param_2];
  if ((((param_1 == '\0') && (bVar1 == 4)) && ((int)(short)param_2 < (short)uVar3 + -3)) &&
     (7 < (byte)param_2_00[(short)uVar3 + -2])) {
    uVar4 = uVar3 - 2;
    do {
      uVar4 = uVar4 - 1;
      if (param_2 == uVar4) break;
    } while (param_2_00[uVar4] == '\t');
    if (param_2 == uVar4) {
      bVar1 = 9;
    }
  }
  if (4 < bVar1) {
    uVar4 = local_18;
    if (((bVar1 == 5) && (param_1 != '\0')) &&
       ((param_2 == 0 || ((*(ushort *)(param_2_00 + (short)param_2 + -1) & 1) == 0)))) {
      cVar2 = '\0';
      while (((short)param_2 + 1 < (int)(short)uVar3 && (cVar2 == '\0'))) {
        uVar3 = uVar3 - 1;
        cVar2 = param_2_00[uVar3];
      }
      if (cVar2 == '\0') {
        return 0;
      }
    }
    do {
      local_18 = uVar4;
      if ((short)local_18 < 1) {
        *param_2_00 = 1;
        *param_1_00 = 1;
        return 1;
      }
      uVar4 = local_18 - 1;
      param_2_00[uVar4] = param_2_00[uVar4] + '\x01';
    } while (9 < (byte)param_2_00[uVar4]);
    *param_1_00 = local_18;
  }
  return 0;
}



// ========== FUN_00407860 @ 00407860 ==========

short __fastcall
FUN_00407860(ushort param_1_00,int param_2_00,char param_1,uint param_2,int param_3)

{
  short sVar1;
  short sVar2;
  short sVar3;
  undefined8 uVar4;
  int local_20;
  uint local_18;
  uint local_10;
  
  if ((param_3 == 0) && (param_2 < 1000000000)) {
    local_20 = 0;
    local_18 = 0;
    local_10 = param_2;
  }
  else {
    uVar4 = FUN_00404660(param_2,param_3,1000000000,0);
    local_18 = (uint)uVar4;
    local_10 = param_2 + local_18 * -1000000000;
    if (((int)((ulonglong)uVar4 >> 0x20) == 0) && (local_18 < 1000000000)) {
      local_20 = 0;
    }
    else {
      local_20 = FUN_00404660(uVar4,1000000000,0);
      local_18 = local_18 + local_20 * -1000000000;
    }
  }
  sVar1 = FUN_004079a0(0,local_20);
  if ((param_1 != '\0') && (sVar1 == 0)) {
    *(undefined *)(param_2_00 + (uint)param_1_00) = 0;
    sVar1 = 1;
  }
  sVar2 = FUN_004079a0(CONCAT31((int3)(local_18 >> 8),sVar1 != 0),local_18);
  sVar3 = FUN_004079a0(CONCAT31((int3)(local_10 >> 8),(short)(sVar1 + sVar2) != 0),local_10);
  return sVar1 + sVar2 + sVar3;
}



// ========== FUN_004079a0 @ 004079a0 ==========

undefined4 __fastcall FUN_004079a0(short param_1_00,int param_2_00,char param_1,uint param_2)

{
  undefined4 in_EAX;
  uint uVar1;
  short sVar2;
  short sVar3;
  int iVar4;
  uint uVar5;
  
  uVar1 = CONCAT31((int3)((uint)in_EAX >> 8),param_1);
  if (param_2 == 0) {
    sVar2 = 0;
  }
  else {
    iVar4 = 0x1f;
    if (param_2 != 0) {
      for (; param_2 >> iVar4 == 0; iVar4 = iVar4 + -1) {
      }
    }
    if (param_2 == 0) {
      iVar4 = 0xff;
    }
    uVar5 = (uint)((iVar4 + 1) * 0x4d1) >> 0xc;
    sVar2 = (short)uVar5;
    if (*(uint *)((uVar5 & 0xffff) * 4 + 0x7abe50) <= param_2) {
      sVar2 = sVar2 + 1;
    }
  }
  sVar3 = sVar2;
  if ((param_1 != '\0') && (sVar2 < 9)) {
    sVar2 = 9;
    sVar3 = sVar2;
  }
  while (0 < sVar2) {
    sVar2 = sVar2 + -1;
    if (param_2 == 0) {
      uVar1 = (int)param_1_00 + (int)sVar2;
      *(undefined *)(param_2_00 + uVar1) = 0;
    }
    else {
      uVar1 = param_2 % 10;
      *(char *)(param_2_00 + (int)sVar2 + (int)param_1_00) = (char)uVar1;
      param_2 = param_2 / 10;
    }
  }
  return CONCAT22((short)(uVar1 >> 0x10),sVar3);
}



// ========== FUN_00407a70 @ 00407a70 ==========

/* WARNING: Removing unreachable block (ram,0x0040819b) */

unkbyte10 __fastcall FUN_00407a70(undefined4 param_1,int *param_2)

{
  short sVar1;
  char cVar2;
  byte bVar3;
  ushort uVar4;
  byte *in_EAX;
  uint uVar5;
  int iVar6;
  uint uVar7;
  uint uVar8;
  int iVar9;
  uint *puVar10;
  uint uVar11;
  uint *puVar12;
  bool bVar13;
  bool bVar14;
  short local_98;
  short local_94;
  byte local_90;
  byte local_8c;
  short local_88;
  short local_78;
  short local_74;
  byte local_70;
  uint local_64 [5];
  undefined *local_50;
  uint local_4c [3];
  short local_40;
  short local_2c;
  uint local_20;
  uint local_1c;
  uint local_18;
  unkbyte10 local_14;
  
  *param_2 = 1;
  local_78 = 1;
  uVar4 = (ushort)*in_EAX;
  FUN_00408530(0,0,0);
  for (; local_78 <= (short)uVar4; local_78 = local_78 + 1) {
    bVar3 = in_EAX[(byte)local_78];
    if (0x2a < bVar3) {
      if (bVar3 == 0x2b) {
        local_78 = local_78 + 1;
        break;
      }
      if (bVar3 == 0x2d) {
        local_78 = local_78 + 1;
        break;
      }
    }
    if (bVar3 != 0x20) break;
  }
  if ((short)uVar4 < local_78) {
    *param_2 = (int)local_78;
  }
  else {
    bVar3 = in_EAX[(byte)local_78];
    if ((bVar3 < 0x2e) ||
       ((bVar3 != 0x2e &&
        (((byte)(bVar3 - 0x2e) < 2 ||
         (((8 < (byte)(bVar3 - 0x30) && bVar3 != 0x39 && (bVar3 != 0x45)) &&
          ((byte)(bVar3 - 0x30) != 0x35)))))))) {
      bVar14 = true;
    }
    else {
      bVar14 = false;
    }
    if (bVar14) {
      local_1c = 0;
      local_18 = 0x80000000;
      cVar2 = FUN_004083d0(&DAT_007aba20);
      if (cVar2 == '\0') {
        cVar2 = FUN_004083d0(&DAT_007aba30);
        if (cVar2 == '\0') {
          bVar14 = false;
        }
        else {
          local_1c = 0;
          local_18 = 0xc0000000;
        }
      }
      if (bVar14) {
        FUN_00408530(local_1c,local_18,0x7fff);
        local_78 = 0;
      }
      *param_2 = (int)local_78;
    }
    else {
      local_1c = 0;
      local_18 = 0;
      local_20 = 0;
      local_94 = 0;
      local_74 = 0;
      local_8c = 0;
      local_90 = 0;
      while ((local_78 <= (short)uVar4 && (in_EAX[(byte)local_78] == 0x30))) {
        local_78 = local_78 + 1;
      }
      while (((local_78 <= (short)uVar4 && (bVar3 = in_EAX[(byte)local_78], 0x2f < bVar3)) &&
             (bVar3 < 0x3a))) {
        bVar3 = bVar3 - 0x30;
        if (local_94 < 0x1c) {
          uVar7 = local_1c >> 0x1d;
          bVar14 = CARRY4(local_1c * 8,local_1c);
          uVar8 = local_1c * 9;
          uVar5 = bVar3 + uVar8;
          bVar13 = CARRY4(local_1c,uVar5);
          local_1c = local_1c + uVar5;
          uVar5 = uVar7 + bVar14 + (uint)CARRY4((uint)bVar3,uVar8) + (uint)bVar13;
          uVar7 = local_18 >> 0x1d;
          bVar14 = CARRY4(local_18 * 8,local_18);
          uVar8 = local_18 * 9;
          uVar11 = uVar5 + uVar8;
          bVar13 = CARRY4(local_18,uVar11);
          local_18 = local_18 + uVar11;
          local_20 = (local_20 * 8 | uVar7) + local_20 * 2 + (uint)bVar14 +
                     (uint)CARRY4(uVar5,uVar8) + (uint)bVar13;
          local_50 = &stack0xfffffffc;
        }
        else {
          if (local_94 != 0x1c) {
            local_90 = bVar3 | local_90;
            bVar3 = local_8c;
          }
          local_8c = bVar3;
          local_74 = local_74 + 1;
        }
        local_94 = local_94 + 1;
        local_78 = local_78 + 1;
      }
      if ((local_78 <= (short)uVar4) && (in_EAX[(byte)local_78] == 0x2e)) {
        local_78 = local_78 + 1;
        if (local_94 == 0) {
          while ((local_78 <= (short)uVar4 && (in_EAX[(byte)local_78] == 0x30))) {
            local_74 = local_74 + -1;
            local_78 = local_78 + 1;
          }
        }
        for (; local_78 <= (short)uVar4; local_78 = local_78 + 1) {
          bVar3 = in_EAX[(byte)local_78];
          if ((bVar3 < 0x30) || (0x39 < bVar3)) break;
          bVar3 = bVar3 - 0x30;
          if (local_94 < 0x1c) {
            uVar7 = local_1c >> 0x1d;
            bVar14 = CARRY4(local_1c * 8,local_1c);
            uVar8 = local_1c * 9;
            uVar5 = bVar3 + uVar8;
            bVar13 = CARRY4(local_1c,uVar5);
            local_1c = local_1c + uVar5;
            uVar5 = uVar7 + bVar14 + (uint)CARRY4((uint)bVar3,uVar8) + (uint)bVar13;
            uVar7 = local_18 >> 0x1d;
            bVar14 = CARRY4(local_18 * 8,local_18);
            uVar8 = local_18 * 9;
            uVar11 = uVar5 + uVar8;
            bVar13 = CARRY4(local_18,uVar11);
            local_18 = local_18 + uVar11;
            local_20 = (local_20 * 8 | uVar7) + local_20 * 2 + (uint)bVar14 +
                       (uint)CARRY4(uVar5,uVar8) + (uint)bVar13;
            local_74 = local_74 + -1;
            bVar3 = local_8c;
            local_50 = &stack0xfffffffc;
          }
          else if (local_94 != 0x1c) {
            local_90 = bVar3 | local_90;
            bVar3 = local_8c;
          }
          local_8c = bVar3;
          local_94 = local_94 + 1;
        }
      }
      sVar1 = local_78;
      if (((local_8c == 5) && (local_90 == 0)) && ((local_1c & 1) == 0)) {
        local_8c = 4;
      }
      if (4 < local_8c) {
        bVar14 = 0xfffffffe < local_1c;
        local_1c = local_1c + 1;
        local_18 = local_18 + bVar14;
        if ((local_18 == 0) && (local_1c == 0)) {
          local_20 = local_20 + 1;
        }
      }
      iVar9 = 0;
      if (local_78 <= (short)uVar4) {
        bVar14 = false;
        bVar3 = in_EAX[(byte)local_78];
        if ((bVar3 < 0x45) || ((bVar3 != 0x45 && (bVar3 != 0x65)))) {
          *param_2 = (int)local_78;
          return local_14;
        }
        local_78 = local_78 + 1;
        if ((short)uVar4 < local_78) {
          *param_2 = (int)local_78;
          return local_14;
        }
        bVar3 = in_EAX[(byte)local_78];
        if (0x2a < bVar3) {
          if (bVar3 == 0x2b) {
            local_78 = sVar1 + 2;
          }
          else if (bVar3 == 0x2d) {
            bVar14 = true;
            local_78 = sVar1 + 2;
          }
        }
        for (; local_78 <= (short)uVar4; local_78 = local_78 + 1) {
          bVar3 = in_EAX[(byte)local_78];
          if ((bVar3 < 0x30) || (0x39 < bVar3)) {
            *param_2 = (int)local_78;
            return local_14;
          }
          if (iVar9 < 100000) {
            iVar9 = iVar9 * 10 + (uint)bVar3 + -0x30;
          }
        }
        if (bVar14) {
          iVar9 = -iVar9;
        }
      }
      iVar6 = local_74 + iVar9;
      if ((iVar9 < 100000) && (iVar6 < 10000)) {
        if ((iVar9 < -99999) || (iVar6 < -9999)) {
          local_74 = -10000;
        }
        else {
          local_74 = (short)iVar6;
        }
      }
      else {
        local_74 = 10000;
      }
      if ((short)uVar4 < local_78) {
        if (((local_18 == 0) && (local_20 == 0 && local_1c == 0)) || (local_74 < -9999)) {
          FUN_00408530(0,0,0);
          *param_2 = 0;
        }
        else {
          if (local_74 < 10000) {
            if (local_20 == 0) {
              local_50 = &stack0xfffffffc;
              local_98 = FUN_00404370(local_1c,local_18);
              local_98 = 0x5f - local_98;
            }
            else {
              iVar9 = 0x1f;
              if (local_20 != 0) {
                for (; local_20 >> iVar9 == 0; iVar9 = iVar9 + -1) {
                }
              }
              local_98 = (short)iVar9;
              if (local_20 == 0) {
                local_98 = 0xff;
              }
              local_98 = 0x1f - local_98;
              local_50 = &stack0xfffffffc;
            }
            local_4c[0] = local_1c;
            local_4c[1] = local_18;
            local_4c[2] = local_20;
            FUN_00405b00();
            local_40 = -local_98;
            local_88 = FUN_00408460();
          }
          else {
            local_88 = 1;
          }
          if (local_88 == 0) {
            if (local_2c != 0) {
              FUN_00405df0(local_64);
              puVar10 = local_64;
              puVar12 = local_4c;
              for (iVar9 = 4; iVar9 != 0; iVar9 = iVar9 + -1) {
                *puVar12 = *puVar10;
                puVar10 = puVar10 + 1;
                puVar12 = puVar12 + 1;
              }
            }
            bVar14 = (local_4c[0] & 0x80000000) != 0;
            FUN_00405c90();
            if (bVar14) {
              uVar7 = (uint)bVar14;
              uVar8 = local_4c[0] + uVar7;
              uVar7 = local_4c[1] + CARRY4(local_4c[0],uVar7);
              if ((uVar7 < local_4c[1]) || ((uVar7 <= local_4c[1] && (uVar8 < local_4c[0])))) {
                iVar9 = 1;
              }
              else {
                iVar9 = 0;
              }
              local_4c[2] = iVar9 + local_4c[2];
              local_4c[0] = uVar8;
              local_4c[1] = uVar7;
            }
            if (local_4c[2] != 0) {
              FUN_00405c90();
              local_40 = local_40 + 1;
            }
            local_40 = local_40 + 0x405e;
            if (local_40 < 0x7fff) {
              if (local_40 < -0x3f) {
                local_88 = -1;
              }
              else if (local_40 < 1) {
                uVar4 = 1 - local_40;
                bVar3 = (byte)((int)(short)uVar4 - 1U);
                if (((int)(short)uVar4 - 1U & 0x20) == 0) {
                  uVar7 = 0 << (bVar3 & 0x1f) | 1U >> 0x20 - (bVar3 & 0x1f);
                  uVar8 = 1 << (bVar3 & 0x1f);
                }
                else {
                  uVar7 = 1 << (bVar3 & 0x1f);
                  uVar8 = 0;
                }
                if (((local_4c[1] & (uVar7 << 2 | uVar8 >> 0x1e) - (uint)(uVar8 * 4 == 0)) == uVar7)
                   && ((local_4c[0] & uVar8 * 4 - 1) == uVar8)) {
                  local_70 = 0;
                }
                else if (((local_4c[1] & uVar7) == 0) && ((local_4c[0] & uVar8) == 0)) {
                  local_70 = 0;
                }
                else {
                  local_70 = 1;
                }
                if (local_40 == -0x3f) {
                  local_4c[0] = (uint)local_70;
                  local_4c[1] = 0;
                  if (local_70 == 0) {
                    local_88 = -1;
                  }
                }
                else {
                  bVar3 = (byte)uVar4;
                  if ((uVar4 & 0x20) == 0) {
                    uVar7 = local_4c[0] >> (bVar3 & 0x1f) | local_4c[1] << 0x20 - (bVar3 & 0x1f);
                    local_4c[1] = local_4c[1] >> (bVar3 & 0x1f);
                  }
                  else {
                    uVar7 = local_4c[1] >> (bVar3 & 0x1f);
                    local_4c[1] = 0;
                  }
                  local_4c[0] = uVar7 + local_70;
                  local_4c[1] = local_4c[1] + CARRY4(uVar7,(uint)local_70);
                }
                local_40 = 0;
              }
            }
            else {
              local_88 = 1;
            }
          }
          if (local_88 < 0) {
            FUN_00408530(0,0,0);
          }
          else if (local_88 < 1) {
            FUN_00408530(local_4c[0],local_4c[1],local_40);
          }
          else {
            FUN_00408530(0,0x80000000,0x7fff);
          }
          *param_2 = 0;
        }
      }
      else {
        *param_2 = (int)local_78;
      }
    }
  }
  return local_14;
}



// ========== FUN_004083d0 @ 004083d0 ==========

undefined __fastcall FUN_004083d0(byte *param_1_00,short param_2,byte *param_1)

{
  char cVar1;
  char cVar2;
  ushort local_14;
  undefined local_c;
  
  local_c = 0;
  local_14 = (ushort)*param_1;
  if (((int)(short)(ushort)*param_1_00 - (int)param_2) + 1 == (int)(short)local_14) {
    do {
      cVar1 = FUN_00405750();
      cVar2 = FUN_00405750();
      if (cVar1 != cVar2) {
        return 0;
      }
      local_14 = local_14 - 1;
    } while (0 < (short)local_14);
    local_c = 1;
  }
  return local_c;
}



// ========== FUN_00408460 @ 00408460 ==========

undefined4 __fastcall FUN_00408460(undefined4 *param_1,short param_2)

{
  int iVar1;
  undefined2 extraout_var;
  int iVar2;
  short sVar3;
  undefined2 uVar4;
  undefined4 *puVar5;
  undefined4 *puVar6;
  undefined4 local_4c [4];
  undefined4 local_3c [4];
  short local_2c;
  undefined4 local_24 [4];
  short local_14;
  
  FUN_00406060();
  iVar1 = (int)param_2;
  sVar3 = (short)(local_14 - iVar1);
  if (sVar3 < 0) {
    uVar4 = 1;
  }
  else if (sVar3 < 0x25) {
    iVar1 = (local_14 - iVar1 & 0xffffU) * 0x18;
    puVar5 = (undefined4 *)(&DAT_007abe90 + iVar1);
    puVar6 = local_3c;
    for (iVar2 = 6; iVar2 != 0; iVar2 = iVar2 + -1) {
      *puVar6 = *puVar5;
      puVar5 = puVar5 + 1;
      puVar6 = puVar6 + 1;
    }
    if (local_2c == 0) {
      puVar5 = local_24;
      for (iVar2 = 6; iVar2 != 0; iVar2 = iVar2 + -1) {
        *param_1 = *puVar5;
        puVar5 = puVar5 + 1;
        param_1 = param_1 + 1;
      }
    }
    else if (local_14 == 0) {
      puVar5 = local_3c;
      for (iVar2 = 6; iVar2 != 0; iVar2 = iVar2 + -1) {
        *param_1 = *puVar5;
        puVar5 = puVar5 + 1;
        param_1 = param_1 + 1;
      }
    }
    else {
      FUN_00405df0(local_4c);
      puVar5 = local_4c;
      puVar6 = param_1;
      for (iVar1 = 4; iVar1 != 0; iVar1 = iVar1 + -1) {
        *puVar6 = *puVar5;
        puVar5 = puVar5 + 1;
        puVar6 = puVar6 + 1;
      }
      iVar1 = CONCAT22(extraout_var,param_2);
      *(short *)(param_1 + 4) = param_2;
    }
    uVar4 = 0;
  }
  else {
    uVar4 = 0xffff;
  }
  return CONCAT22((short)((uint)iVar1 >> 0x10),uVar4);
}



// ========== FUN_00408530 @ 00408530 ==========

void __fastcall
FUN_00408530(char param_1_00,undefined4 *param_2_00,undefined4 param_1,undefined4 param_2,
            ushort param_3)

{
  ushort local_14;
  
  local_14 = param_3 & 0x7fff;
  if (param_1_00 != '\0') {
    local_14 = local_14 | 0x8000;
  }
  *param_2_00 = param_1;
  param_2_00[1] = param_2;
  *(ushort *)(param_2_00 + 2) = local_14;
  return;
}



// ========== FUN_004085b0 @ 004085b0 ==========

undefined4 __fastcall
FUN_004085b0(int param_1_00,int param_2_00,undefined4 param_1,byte *param_2,int *param_3)

{
  int in_EAX;
  int iVar1;
  int iVar2;
  int iVar3;
  undefined4 local_8;
  
  local_8 = 0x6b;
  if (*param_3 == 0) {
    iVar2 = (uint)*(byte *)(param_1_00 + 1) + param_1_00 + 2;
    if (((*(int *)(iVar2 + 1) <= in_EAX) && (in_EAX <= *(int *)(iVar2 + 5))) &&
       (param_3[(in_EAX - *(int *)(iVar2 + 1)) + 1] != 0)) {
      FUN_004040c0();
LAB_00408675:
      if ((int)(uint)*param_2 < param_2_00) {
        param_2_00 = param_2_00 - (uint)*param_2;
        if (0 < param_2_00) {
          iVar2 = 0;
          do {
            iVar2 = iVar2 + 1;
            param_2[(uint)*param_2 + iVar2 & 0xff] = 0x20;
          } while (iVar2 < param_2_00);
        }
        *param_2 = *param_2 + (char)param_2_00;
      }
      local_8 = 0;
    }
  }
  else {
    iVar2 = 0;
    iVar1 = param_3[1] + -1;
    do {
      iVar3 = (iVar2 + iVar1) / 2;
      if (param_3[iVar3 * 2 + 2] < in_EAX) {
        iVar2 = iVar3 + 1;
      }
      else {
        if (param_3[iVar3 * 2 + 2] <= in_EAX) {
          FUN_004040c0();
          goto LAB_00408675;
        }
        iVar1 = iVar3 + -1;
      }
    } while (iVar2 <= iVar1);
  }
  return local_8;
}



// ========== FUN_004086d7 @ 004086d7 ==========

void __fastcall FUN_004086d7(undefined4 param_1,undefined4 param_2)

{
  int *piVar1;
  char *pcVar2;
  char cVar3;
  undefined4 in_EAX;
  int iVar4;
  uint uVar5;
  int iVar6;
  int unaff_EBP;
  bool bVar7;
  longlong lVar8;
  
  *(undefined4 *)(unaff_EBP + -0x34) = in_EAX;
  *(undefined4 *)(unaff_EBP + -0x2c) = param_2;
  *(undefined4 *)(unaff_EBP + -0x18) = param_1;
  *(undefined4 *)(unaff_EBP + -0x30) = *(undefined4 *)(unaff_EBP + 8);
  FUN_00404af0();
  if (*(int *)(unaff_EBP + -0x34) == -0x7fff) {
    *(undefined4 *)(unaff_EBP + -0x34) = 0x19;
  }
  if (*(int *)(unaff_EBP + 0x10) < 0) {
    *(undefined4 *)(unaff_EBP + -0x1c) = 1;
    iVar6 = -*(int *)(unaff_EBP + 0xc);
    iVar4 = (~*(uint *)(unaff_EBP + 0x10) + 1) - (uint)(*(int *)(unaff_EBP + 0xc) != 0);
  }
  else {
    iVar6 = *(int *)(unaff_EBP + 0xc);
    iVar4 = *(int *)(unaff_EBP + 0x10);
    *(undefined4 *)(unaff_EBP + -0x1c) = 0;
  }
  lVar8 = CONCAT44(iVar4,iVar6);
  iVar6 = 0;
  do {
    iVar4 = iVar6;
    iVar6 = iVar4 + 1;
    cVar3 = FUN_00404700(lVar8,10,0);
    *(char *)(unaff_EBP + -0x15 + iVar6) = cVar3 + '0';
    lVar8 = FUN_00404660(lVar8,10,0);
  } while (lVar8 != 0);
  *(int *)(unaff_EBP + -0x28) = iVar6;
  if (*(int *)(unaff_EBP + -0x2c) != 0) {
    iVar6 = iVar4 + 2;
  }
  if (*(int *)(unaff_EBP + -0x2c) < 0) {
    uVar5 = iVar6 + 5;
    if (*(int *)(unaff_EBP + -0x34) < 8) {
      *(undefined4 *)(unaff_EBP + -0x34) = 8;
    }
    iVar4 = uVar5 - *(int *)(unaff_EBP + -0x34);
    if ((int)uVar5 < *(int *)(unaff_EBP + -0x34)) {
      uVar5 = *(uint *)(unaff_EBP + -0x34);
    }
    if (iVar4 < 1) {
      *(undefined4 *)(unaff_EBP + -0x20) = *(undefined4 *)(unaff_EBP + -0x28);
    }
    else {
      uVar5 = *(uint *)(unaff_EBP + -0x34);
      *(int *)(unaff_EBP + -0x20) = *(int *)(unaff_EBP + -0x28) - iVar4;
    }
  }
  else {
    iVar6 = iVar6 + *(int *)(unaff_EBP + -0x1c);
    while (*(int *)(unaff_EBP + -0x28) < 5) {
      iVar6 = iVar6 + 1;
      *(int *)(unaff_EBP + -0x28) = *(int *)(unaff_EBP + -0x28) + 1;
      *(undefined *)(unaff_EBP + -0x15 + *(int *)(unaff_EBP + -0x28)) = 0x30;
    }
    iVar4 = 4 - *(int *)(unaff_EBP + -0x2c);
    *(int *)(unaff_EBP + -0x20) = *(int *)(unaff_EBP + -0x2c);
    if (*(int *)(unaff_EBP + -0x2c) != 0) {
      if (4 < *(int *)(unaff_EBP + -0x20)) {
        *(undefined4 *)(unaff_EBP + -0x20) = 4;
      }
      *(int *)(unaff_EBP + -0x20) = *(int *)(unaff_EBP + -0x20) + 1;
    }
    uVar5 = iVar6 - iVar4;
  }
  if (0 < iVar4) {
    bVar7 = false;
    *(int *)(unaff_EBP + -0x24) = iVar4 + 2;
    if (*(int *)(unaff_EBP + -0x28) < iVar4 + 2) {
      *(int *)(unaff_EBP + -0x24) = *(int *)(unaff_EBP + -0x28) + 1;
    }
    if (0x34 < *(byte *)(unaff_EBP + -0x17 + *(int *)(unaff_EBP + -0x24))) {
      bVar7 = 0x38 < *(byte *)(unaff_EBP + -0x16 + *(int *)(unaff_EBP + -0x24));
      if (bVar7) {
        *(undefined *)(unaff_EBP + -0x16 + *(int *)(unaff_EBP + -0x24)) = 0x30;
      }
      else {
        pcVar2 = (char *)(unaff_EBP + *(int *)(unaff_EBP + -0x24) + -0x16);
        *pcVar2 = *pcVar2 + '\x01';
      }
    }
    if ((bVar7) && (*(char *)(unaff_EBP + -0x16 + *(int *)(unaff_EBP + -0x24)) == '0')) {
      while (*(char *)(unaff_EBP + -0x15 + *(int *)(unaff_EBP + -0x24)) == '9') {
        *(undefined *)(unaff_EBP + -0x15 + *(int *)(unaff_EBP + -0x24)) = 0x30;
        *(int *)(unaff_EBP + -0x24) = *(int *)(unaff_EBP + -0x24) + 1;
      }
      *(char *)(unaff_EBP + -0x15 + *(int *)(unaff_EBP + -0x24)) =
           *(char *)(unaff_EBP + -0x15 + *(int *)(unaff_EBP + -0x24)) + '\x01';
      if (*(int *)(unaff_EBP + -0x28) < *(int *)(unaff_EBP + -0x24)) {
        uVar5 = uVar5 + 1;
        *(int *)(unaff_EBP + -0x28) = *(int *)(unaff_EBP + -0x28) + 1;
      }
    }
  }
  if ((int)uVar5 < *(int *)(unaff_EBP + -0x34)) {
    uVar5 = *(uint *)(unaff_EBP + -0x34);
  }
  if (*(int *)(unaff_EBP + -0x30) < (int)uVar5) {
    if (iVar4 < 0) {
      iVar4 = iVar4 + (uVar5 - *(int *)(unaff_EBP + -0x30));
    }
    uVar5 = *(uint *)(unaff_EBP + -0x30);
  }
  FUN_00405670();
  if (*(int *)(unaff_EBP + -0x2c) < 0) {
    if ((*(int *)(unaff_EBP + 0x10) == 0) && (*(int *)(unaff_EBP + 0xc) == 0)) {
      iVar6 = 0;
    }
    else {
      iVar6 = *(int *)(unaff_EBP + -0x28) + -5;
    }
    if (iVar6 < 0) {
      *(undefined *)(*(int *)(unaff_EBP + -0x18) + (uVar5 - 2 & 0xff)) = 0x2d;
      iVar6 = -iVar6;
    }
    else {
      *(undefined *)(*(int *)(unaff_EBP + -0x18) + (uVar5 - 2 & 0xff)) = 0x2b;
    }
    *(char *)(*(int *)(unaff_EBP + -0x18) + (uVar5 & 0xff)) = (char)(iVar6 % 10) + '0';
    *(char *)(*(int *)(unaff_EBP + -0x18) + (uVar5 - 1 & 0xff)) = (char)(iVar6 / 10) + '0';
    *(undefined *)(*(int *)(unaff_EBP + -0x18) + (uVar5 - 3 & 0xff)) = 0x45;
    uVar5 = uVar5 - 4;
  }
  for (; iVar4 < 0; iVar4 = iVar4 + 1) {
    *(undefined *)(*(int *)(unaff_EBP + -0x18) + (uVar5 & 0xff)) = 0x30;
    uVar5 = uVar5 - 1;
  }
  iVar6 = *(int *)(unaff_EBP + -0x28);
  if (iVar4 + 1 <= iVar6) {
    *(int *)(unaff_EBP + -0x24) = iVar4 + 1;
    *(int *)(unaff_EBP + -0x24) = iVar4;
    do {
      *(int *)(unaff_EBP + -0x24) = *(int *)(unaff_EBP + -0x24) + 1;
      piVar1 = (int *)(unaff_EBP + -0x20);
      *piVar1 = *piVar1 + -1;
      if (*piVar1 == 0) {
        *(undefined *)(*(int *)(unaff_EBP + -0x18) + (uVar5 & 0xff)) = 0x2e;
        uVar5 = uVar5 - 1;
      }
      *(undefined *)(*(int *)(unaff_EBP + -0x18) + (uVar5 & 0xff)) =
           *(undefined *)(unaff_EBP + -0x15 + *(int *)(unaff_EBP + -0x24));
      uVar5 = uVar5 - 1;
    } while (*(int *)(unaff_EBP + -0x24) < iVar6);
  }
  if (*(int *)(unaff_EBP + -0x1c) == 1) {
    *(undefined *)(*(int *)(unaff_EBP + -0x18) + (uVar5 & 0xff)) = 0x2d;
    uVar5 = uVar5 - 1;
  }
  for (; 0 < (int)uVar5; uVar5 = uVar5 - 1) {
    *(undefined *)(*(int *)(unaff_EBP + -0x18) + (uVar5 & 0xff)) = 0x20;
  }
  return;
}



// ========== FUN_00408a00 @ 00408a00 ==========

uint __fastcall FUN_00408a00(undefined *param_1,undefined *param_2)

{
  byte bVar1;
  byte *in_EAX;
  uint local_8;
  
  local_8 = 1;
  *param_2 = 0;
  *param_1 = 10;
  if (*in_EAX == 0) {
    return 1;
  }
  for (; ((int)local_8 <= (int)(uint)*in_EAX &&
         ((in_EAX[local_8 & 0xff] == 9 || (in_EAX[local_8 & 0xff] == 0x20)))); local_8 = local_8 + 1
      ) {
  }
  bVar1 = in_EAX[local_8 & 0xff];
  if (0x2a < bVar1) {
    if (bVar1 == 0x2b) {
      local_8 = local_8 + 1;
    }
    else if (bVar1 == 0x2d) {
      *param_2 = 1;
      local_8 = local_8 + 1;
    }
  }
  if (((int)(uint)*in_EAX < (int)local_8) || (bVar1 = in_EAX[local_8 & 0xff], bVar1 < 0x24))
  goto LAB_00408b04;
  if (bVar1 != 0x24) {
    if (bVar1 == 0x25) {
      *param_1 = 2;
      local_8 = local_8 + 1;
      goto LAB_00408b04;
    }
    if (bVar1 == 0x26) {
      *param_1 = 8;
      local_8 = local_8 + 1;
      goto LAB_00408b04;
    }
    if (bVar1 == 0x30) {
      if (((int)local_8 < (int)(uint)*in_EAX) &&
         ((in_EAX[local_8 + 1 & 0xff] == 0x58 || (in_EAX[local_8 + 1 & 0xff] == 0x78)))) {
        local_8 = local_8 + 2;
        *param_1 = 0x10;
      }
      goto LAB_00408b04;
    }
    if ((bVar1 != 0x58) && (bVar1 != 0x78)) goto LAB_00408b04;
  }
  *param_1 = 0x10;
  local_8 = local_8 + 1;
LAB_00408b04:
  for (; ((int)local_8 < (int)(uint)*in_EAX && (in_EAX[local_8 & 0xff] == 0x30));
      local_8 = local_8 + 1) {
  }
  return local_8;
}



// ========== FUN_00408b30 @ 00408b30 ==========

uint __fastcall FUN_00408b30(byte *param_1,byte *param_2)

{
  byte bVar1;
  int in_EAX;
  int iVar2;
  uint uVar3;
  byte local_20;
  uint local_10;
  byte local_c;
  byte local_8;
  
  local_10 = 0;
  uVar3 = 0;
  iVar2 = FUN_00408a00();
  *(int *)param_1 = iVar2;
  if (*(int *)param_1 <= (int)(uint)*param_2) {
    if (param_2[*param_1] == 0) {
      if ((1 < *(int *)param_1) && (param_2[*(int *)param_1 - 1U & 0xff] == 0x30)) {
        param_1[0] = 0;
        param_1[1] = 0;
        param_1[2] = 0;
        param_1[3] = 0;
      }
    }
    else {
      if (local_8 == 10) {
        iVar2 = local_c + 0x7fffffff;
      }
      else {
        iVar2 = -1;
      }
      while ((*(int *)param_1 <= (int)(uint)*param_2 && (bVar1 = param_2[*param_1], bVar1 != 0))) {
        if (bVar1 < 0x30) {
LAB_00408c70:
          local_20 = 0x10;
        }
        else if ((byte)(bVar1 - 0x30) < 9 || bVar1 == 0x39) {
          local_20 = param_2[*param_1] - 0x30;
        }
        else {
          if ((byte)(bVar1 - 0x39) < 8) goto LAB_00408c70;
          if ((byte)(bVar1 + 0xbf) < 5 || bVar1 == 0x46) {
            local_20 = param_2[*param_1] - 0x37;
          }
          else {
            if (((byte)(bVar1 + 0xba) < 0x1b) || (5 < (byte)(bVar1 + 0x9f))) goto LAB_00408c70;
            local_20 = param_2[*param_1] + 0xa9;
          }
        }
        if (((local_8 <= local_20) || (iVar2 - (uint)local_20 < local_8 * uVar3)) ||
           ((uint)(0xffffffff / (ulonglong)local_8) < uVar3)) {
          return 0;
        }
        uVar3 = local_8 * uVar3 + (uint)local_20;
        *(int *)param_1 = *(int *)param_1 + 1;
      }
      param_1[0] = 0;
      param_1[1] = 0;
      param_1[2] = 0;
      param_1[3] = 0;
      if (local_c == 0) {
        local_10 = uVar3;
        if ((local_8 != 10) && (0 < in_EAX)) {
          if (in_EAX == 1) {
            local_10._0_1_ = (char)uVar3;
            local_10 = (int)(char)local_10;
          }
          else if (in_EAX == 2) {
            local_10._0_2_ = (short)uVar3;
            local_10 = (int)(short)local_10;
          }
        }
      }
      else {
        local_10 = -uVar3;
      }
    }
  }
  return local_10;
}



