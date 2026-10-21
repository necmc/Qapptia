; Script de Inno Setup para Qapptia
#define MyAppId "8B45E1B4-3F25-4A92-A746-3C62077DA820"
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "dist\Qapptia"
#endif
#ifndef OutputDir
  #define OutputDir "dist"
#endif
#ifndef OutputBaseFilename
  #define OutputBaseFilename "Qapptia-setup"
#endif

[Setup]
AppId={{{#MyAppId}}
AppName=Qapptia
AppVersion={#AppVersion}
AppPublisher=Qapptia
DefaultDirName={autopf}\Qapptia
DefaultGroupName=Qapptia
AllowNoIcons=yes
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
DisableWelcomePage=yes
DisableDirPage=auto
UsePreviousAppDir=yes
UsePreviousPrivileges=yes
UsePreviousTasks=no
ShowLanguageDialog=no
MinVersion=6.1sp1
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\Qapptia.exe
SetupIconFile=compiler:SetupClassicIcon.ico
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[CustomMessages]
english.AutoStartDescription=Start Qapptia on system startup
spanish.AutoStartDescription=Iniciar Qapptia al encender el equipo
english.AdditionalOptions=Additional options:
spanish.AdditionalOptions=Opciones adicionales:

english.ExistingInstallTitle=Existing Installation
spanish.ExistingInstallTitle=Instalación existente
english.ExistingInstallDesc=An existing installation was found in:
spanish.ExistingInstallDesc=Se encontró una instalación previa en:
english.ExistingInstallSubCaption=Select an option:
spanish.ExistingInstallSubCaption=Seleccione una opción:
english.OptionReinstall=Update or repair Qapptia (Recommended)
spanish.OptionReinstall=Actualizar o reparar Qapptia (Recomendado)
english.OptionUninstall=Uninstall Qapptia
spanish.OptionUninstall=Desinstalar Qapptia

english.ConfirmUninstall=Are you sure you want to uninstall Qapptia?
spanish.ConfirmUninstall=¿Está seguro de que desea desinstalar Qapptia?
english.UninstallSuccess=Qapptia was uninstalled successfully.
spanish.UninstallSuccess=Qapptia se desinstaló correctamente.
english.UninstallSuccessPromptInstall=Qapptia was uninstalled successfully.%n%nDo you want to install the new version now?
spanish.UninstallSuccessPromptInstall=Qapptia se desinstaló correctamente.%n%n¿Desea instalar la nueva versión ahora?
english.UninstallFailed=Failed to execute the uninstaller: %1
spanish.UninstallFailed=No se pudo ejecutar el desinstalador: %1
english.UninstallIncomplete=Could not remove all files in:%n%1%n%nPlease close any running applications or restart your computer.
spanish.UninstallIncomplete=No se pudieron eliminar todos los archivos en:%n%1%n%nCierre las aplicaciones abiertas o reinicie el equipo para completar la eliminación.

english.RescuePurgePrompt=Previous uninstaller not found.%n%nDo you want to clean up residual files directly?
spanish.RescuePurgePrompt=No se encontró el desinstalador anterior.%n%n¿Desea limpiar los archivos residuales directamente?

english.AppRunningPromptInstall=Qapptia is running.%n%nDo you want to close it to continue?
spanish.AppRunningPromptInstall=Qapptia está en ejecución.%n%n¿Desea cerrarla para continuar?
english.AppRunningPromptUninstall=Qapptia is running.%n%nDo you want to close it to continue?
spanish.AppRunningPromptUninstall=Qapptia está en ejecución.%n%n¿Desea cerrarla para continuar?
english.AppStillRunning=Qapptia is still running. Please close it and click Retry.
spanish.AppStillRunning=Qapptia sigue en ejecución. Ciérrela y haga clic en Reintentar.

english.ScopeTransitionUserToAll=A per-user installation of Qapptia was detected.%n%nDo you want to uninstall it to install for all users?
spanish.ScopeTransitionUserToAll=Se detectó una versión de Qapptia para el usuario actual.%n%n¿Desea desinstalarla para instalar la versión para todos los usuarios?

english.ScopeTransitionAllToUser=An all-users installation of Qapptia already exists.%n%nPlease uninstall it before installing for your user only.
spanish.ScopeTransitionAllToUser=Ya existe una instalación de Qapptia para todos los usuarios.%n%nDesinstálela antes de instalar la versión por usuario.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart"; Description: "{cm:AutoStartDescription}"; GroupDescription: "{cm:AdditionalOptions}"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[UninstallDelete]
Type: files; Name: "{autodesktop}\Qapptia.lnk"
Type: files; Name: "{userdesktop}\Qapptia.lnk"
Type: files; Name: "{commondesktop}\Qapptia.lnk"
Type: filesandordirs; Name: "{app}\app"
Type: filesandordirs; Name: "{app}\logs"

[Icons]
Name: "{group}\Qapptia"; Filename: "{app}\Qapptia.exe"; IconFilename: "{app}\Qapptia.exe"
Name: "{group}\{cm:UninstallProgram,Qapptia}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Qapptia"; Filename: "{app}\Qapptia.exe"; IconFilename: "{app}\Qapptia.exe"; Tasks: desktopicon

[Registry]
Root: HKA; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Qapptia"; ValueData: """{app}\Qapptia.exe"" --capture"; Flags: uninsdeletevalue; Tasks: autostart

; Compatibilidad TLS 1.2 para Windows 7 / 8.x (Schannel y WinHttp)
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client"; ValueType: dword; ValueName: "DisabledByDefault"; ValueData: "0"; Flags: createvalueifdoesntexist; Check: ShouldConfigureTls12
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client"; ValueType: dword; ValueName: "Enabled"; ValueData: "1"; Flags: createvalueifdoesntexist; Check: ShouldConfigureTls12
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\WinHttp"; ValueType: dword; ValueName: "DefaultSecureProtocols"; ValueData: "$00000800"; Flags: createvalueifdoesntexist; Check: ShouldConfigureTls12
Root: HKLM; Subkey: "SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Internet Settings\WinHttp"; ValueType: dword; ValueName: "DefaultSecureProtocols"; ValueData: "$00000800"; Flags: createvalueifdoesntexist; Check: ShouldConfigureTls12

[Run]
Filename: "{app}\Qapptia.exe"; Description: "{cm:LaunchProgram,Qapptia}"; Flags: nowait postinstall skipifsilent

[Code]
function GetCurrentProcessId: DWORD;
  external 'GetCurrentProcessId@kernel32.dll stdcall';

function SetWindowPos(hWnd: HWND; hWndInsertAfter: HWND; X, Y, cx, cy: Integer; uFlags: UINT): BOOL;
  external 'SetWindowPos@user32.dll stdcall';

function SetForegroundWindow(hWnd: HWND): BOOL;
  external 'SetForegroundWindow@user32.dll stdcall';

function BringWindowToTop(hWnd: HWND): BOOL;
  external 'BringWindowToTop@user32.dll stdcall';

function GetForegroundWindow: HWND;
  external 'GetForegroundWindow@user32.dll stdcall';

function GetCurrentThreadId: DWORD;
  external 'GetCurrentThreadId@kernel32.dll stdcall';

function GetWindowThreadProcessId(hWnd: HWND; var lpdwProcessId: DWORD): DWORD;
  external 'GetWindowThreadProcessId@user32.dll stdcall';

function AttachThreadInput(idAttach: DWORD; idAttachTo: DWORD; fAttach: BOOL): BOOL;
  external 'AttachThreadInput@user32.dll stdcall';

type
  TProcessEntry32 = record
    dwSize: DWORD;
    cntUsage: DWORD;
    th32ProcessID: DWORD;
    th32DefaultHeapID: LongInt;
    th32ModuleID: DWORD;
    cntThreads: DWORD;
    th32ParentProcessID: DWORD;
    pcPriClassBase: LongInt;
    dwFlags: DWORD;
    szExeFile: array[0..259] of Char;
  end;

function CreateToolhelp32Snapshot(dwFlags, th32ProcessID: DWORD): THandle;
  external 'CreateToolhelp32Snapshot@kernel32.dll stdcall';

function Process32First(hSnapshot: THandle; var lppe: TProcessEntry32): BOOL;
  external 'Process32FirstW@kernel32.dll stdcall';

function Process32Next(hSnapshot: THandle; var lppe: TProcessEntry32): BOOL;
  external 'Process32NextW@kernel32.dll stdcall';

function CloseHandle(hObject: THandle): BOOL;
  external 'CloseHandle@kernel32.dll stdcall';

function OpenProcess(dwDesiredAccess: DWORD; bInheritHandle: BOOL; dwProcessId: DWORD): THandle;
  external 'OpenProcess@kernel32.dll stdcall';

function TerminateProcess(hProcess: THandle; uExitCode: UINT): BOOL;
  external 'TerminateProcess@kernel32.dll stdcall';

function WaitForSingleObject(hHandle: THandle; dwMilliseconds: DWORD): DWORD;
  external 'WaitForSingleObject@kernel32.dll stdcall';

function QueryFullProcessImageName(hProcess: THandle; dwFlags: DWORD; lpExeName: string; var lpdwSize: DWORD): BOOL;
  external 'QueryFullProcessImageNameW@kernel32.dll stdcall';

const
  AppSubKeyStandard   = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{' + '{#MyAppId}' + '}_is1';
  AppSubKeyLegacy     = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{' + '{#MyAppId}' + '}}_is1';

var
  MaintenancePage: TInputOptionWizardPage;
  PreviousInstallPath: string;
  PreviousUninstallExe: string;
  PreviousRootKey: Integer;
  HasPreviousInstall: Boolean;
  BroughtToFrontOnce: Boolean;

function GetSelfExeName: string;
begin
  if IsUninstaller then
    Result := Uppercase(ExtractFileName(ExpandConstant('{uninstallexe}')))
  else
    Result := Uppercase(ExtractFileName(ExpandConstant('{srcexe}')));
end;

function GetProcessExePath(PID: DWORD): string;
var
  hProc: THandle;
  Buf: string;
  Size: DWORD;
begin
  Result := '';
  hProc := OpenProcess($1000 {PROCESS_QUERY_LIMITED_INFORMATION}, False, PID);
  if hProc <> 0 then
  begin
    try
      SetLength(Buf, 1024);
      Size := Length(Buf);
      if QueryFullProcessImageName(hProc, 0, Buf, Size) then
        Result := Copy(Buf, 1, Size);
    finally
      CloseHandle(hProc);
    end;
  end;
end;

function GetActiveInstallDir: string;
var
  AppDir: string;
begin
  Result := '';
  // 1. Si existe una instalación previa detectada en el registro
  if PreviousInstallPath <> '' then
  begin
    Result := Uppercase(PreviousInstallPath);
    Exit;
  end;

  // 2. Si {app} está disponible (en desinstalador o tras selección de carpeta)
  try
    AppDir := Uppercase(ExpandConstant('{app}'));
    if (AppDir <> '') and (Pos('{APP}', AppDir) = 0) then
    begin
      Result := AppDir;
      Exit;
    end;
  except
  end;

  // 3. Fallback a la carpeta estándar según ámbito de instalación
  if IsAdminInstallMode then
    Result := Uppercase(ExpandConstant('{autopf}\Qapptia'))
  else
    Result := Uppercase(ExpandConstant('{localappdata}\Programs\Qapptia'));
end;

function IsProcessInDir(ProcessID: DWORD; const TargetDir: string): Boolean;
var
  SelfExe: string;
  ProcPath: string;
  TempDir: string;
  NormDir: string;
  ExeFileName: string;
begin
  Result := False;
  // 1. Autoprotección absoluta: nunca evaluar PID 0 ni el propio proceso instalador/desinstalador
  if (ProcessID = 0) or (ProcessID = GetCurrentProcessId) then Exit;
  if TargetDir = '' then Exit;

  ProcPath := Uppercase(GetProcessExePath(ProcessID));
  if ProcPath = '' then Exit;

  // 2. Autoprotección: nunca evaluar ejecutables propios ni temporales del instalador
  SelfExe := GetSelfExeName;
  ExeFileName := ExtractFileName(ProcPath);
  if (SelfExe <> '') and ((ExeFileName = SelfExe) or (ExeFileName = ChangeFileExt(SelfExe, '.TMP'))) then Exit;
  if (Pos('SETUP', ExeFileName) > 0) or (Pos('UNINS', ExeFileName) > 0) or (Pos('IS-', ExeFileName) > 0) then Exit;

  // 3. Autoprotección: descartar procesos en carpetas temporales
  TempDir := Uppercase(ExpandConstant('{tmp}'));
  if (TempDir <> '') and (Pos(TempDir, ProcPath) = 1) then Exit;

  TempDir := Uppercase(GetEnv('TEMP'));
  if (TempDir <> '') and (Pos(TempDir, ProcPath) = 1) then Exit;

  TempDir := Uppercase(GetEnv('TMP'));
  if (TempDir <> '') and (Pos(TempDir, ProcPath) = 1) then Exit;

  // 4. Inmutabilidad por carpeta: cualquier proceso que corra dentro del directorio pertenece a la aplicación
  NormDir := TargetDir;
  if NormDir[Length(NormDir)] <> '\' then
    NormDir := NormDir + '\';

  if Pos(NormDir, ProcPath) = 1 then
    Result := True;
end;

function HasProcessesInDir(const TargetDir: string): Boolean;
var
  hSnap: THandle;
  pe: TProcessEntry32;
begin
  Result := False;
  if TargetDir = '' then Exit;

  hSnap := CreateToolhelp32Snapshot($00000002 {TH32CS_SNAPPROCESS}, 0);
  if hSnap <> -1 then
  begin
    try
      pe.dwSize := SizeOf(pe);
      if Process32First(hSnap, pe) then
      begin
        repeat
          if IsProcessInDir(pe.th32ProcessID, TargetDir) then
          begin
            Result := True;
            Exit;
          end;
        until not Process32Next(hSnap, pe);
      end;
    finally
      CloseHandle(hSnap);
    end;
  end;
end;

procedure KillProcessesInDir(const TargetDir: string);
var
  hSnap: THandle;
  pe: TProcessEntry32;
  hProc: THandle;
  WaitCount: Integer;
begin
  if TargetDir = '' then Exit;

  hSnap := CreateToolhelp32Snapshot($00000002 {TH32CS_SNAPPROCESS}, 0);
  if hSnap <> -1 then
  begin
    try
      pe.dwSize := SizeOf(pe);
      if Process32First(hSnap, pe) then
      begin
        repeat
          if IsProcessInDir(pe.th32ProcessID, TargetDir) then
          begin
            // Solicitar PROCESS_TERMINATE y SYNCHRONIZE para esperar que Windows libere todos los locks
            hProc := OpenProcess($00100001 {PROCESS_TERMINATE or SYNCHRONIZE}, False, pe.th32ProcessID);
            if hProc = 0 then
              hProc := OpenProcess($0001 {PROCESS_TERMINATE fallback}, False, pe.th32ProcessID);

            if hProc <> 0 then
            begin
              TerminateProcess(hProc, 0);
              WaitForSingleObject(hProc, 3000);
              CloseHandle(hProc);
            end;
          end;
        until not Process32Next(hSnap, pe);
      end;
    finally
      CloseHandle(hSnap);
    end;
  end;

  // Espera activa garantizada hasta que el kernel de Windows libere los ejecutables en disco
  WaitCount := 0;
  while HasProcessesInDir(TargetDir) and (WaitCount < 30) do
  begin
    Sleep(100);
    WaitCount := WaitCount + 1;
  end;
  Sleep(200);
end;

function IsAppRunning: Boolean;
begin
  Result := HasProcessesInDir(GetActiveInstallDir);
end;

procedure KillQapptiaProcesses;
begin
  KillProcessesInDir(GetActiveInstallDir);
end;

function IsSilentMode: Boolean;
begin
  if IsUninstaller then
    Result := UninstallSilent
  else
    Result := WizardSilent;
end;

procedure DeleteDesktopIcons;
var
  UserProfile: string;
begin
  DeleteFile(ExpandConstant('{autodesktop}\Qapptia.lnk'));
  DeleteFile(ExpandConstant('{userdesktop}\Qapptia.lnk'));
  DeleteFile(ExpandConstant('{commondesktop}\Qapptia.lnk'));

  UserProfile := GetEnv('USERPROFILE');
  if UserProfile <> '' then
  begin
    DeleteFile(UserProfile + '\Desktop\Qapptia.lnk');
    DeleteFile(UserProfile + '\OneDrive\Desktop\Qapptia.lnk');
  end;
end;

function EnsureQapptiaClosed(IsUninstalling: Boolean): Boolean;
var
  PromptMsg: string;
  UserChoice: Integer;
  WaitCount: Integer;
begin
  Result := True;
  while IsAppRunning do
  begin
    if IsSilentMode then
    begin
      KillQapptiaProcesses;
      WaitCount := 0;
      while IsAppRunning and (WaitCount < 30) do
      begin
        Sleep(100);
        WaitCount := WaitCount + 1;
      end;
      Result := not IsAppRunning;
      Exit;
    end;

    if IsUninstalling then
      PromptMsg := ExpandConstant('{cm:AppRunningPromptUninstall}')
    else
      PromptMsg := ExpandConstant('{cm:AppRunningPromptInstall}');

    UserChoice := SuppressibleMsgBox(PromptMsg, mbConfirmation, MB_YESNO, IDYES);

    if UserChoice = IDYES then
    begin
      KillQapptiaProcesses;
      WaitCount := 0;
      while IsAppRunning and (WaitCount < 30) do
      begin
        Sleep(100);
        WaitCount := WaitCount + 1;
      end;

      if not IsAppRunning then
      begin
        Result := True;
        Exit;
      end
      else
      begin
        if SuppressibleMsgBox(ExpandConstant('{cm:AppStillRunning}'), mbError, MB_RETRYCANCEL, IDRETRY) <> IDRETRY then
        begin
          Result := False;
          Exit;
        end;
      end;
    end
    else
    begin
      Result := False;
      Exit;
    end;
  end;
end;

function CheckUninstallKey(RootKey: Integer; SubKey: string; var UninsExe: string; var InstPath: string): Boolean;
var
  RawUnins: string;
begin
  Result := False;
  if RegQueryStringValue(RootKey, SubKey, 'UninstallString', RawUnins) then
  begin
    RawUnins := RemoveQuotes(RawUnins);
    UninsExe := RawUnins;
    if not RegQueryStringValue(RootKey, SubKey, 'InstallLocation', InstPath) or (InstPath = '') then
    begin
      if UninsExe <> '' then
        InstPath := ExtractFileDir(UninsExe);
    end;
    Result := True;
  end;
end;

function FindInstallInHive(RootKey: Integer; var UninsExe: string; var InstPath: string): Boolean;
begin
  Result := CheckUninstallKey(RootKey, AppSubKeyStandard, UninsExe, InstPath);
  if not Result then
    Result := CheckUninstallKey(RootKey, AppSubKeyLegacy, UninsExe, InstPath);
end;

function FindInstallInHKLM(var UninsExe: string; var InstPath: string): Boolean;
begin
  Result := FindInstallInHive(HKLM, UninsExe, InstPath);
  if not Result and IsWin64 then
    Result := FindInstallInHive(HKLM64, UninsExe, InstPath);
  if not Result then
    Result := FindInstallInHive(HKLM32, UninsExe, InstPath);
end;

function FindPreviousInstall(var RootKey: Integer; var UninsExe: string; var InstPath: string): Boolean;
begin
  if FindInstallInHive(HKCU, UninsExe, InstPath) then
  begin
    RootKey := HKCU;
    Result := True;
  end
  else if FindInstallInHKLM(UninsExe, InstPath) then
  begin
    RootKey := HKLM;
    Result := True;
  end
  else
  begin
    Result := False;
  end;
end;

function InitializeSetup: Boolean;
var
  UserUninsExe, UserInstPath: string;
  SystemUninsExe, SystemInstPath: string;
  ResultCode: Integer;
begin
  Result := True;

  // Detección de Transición de Ámbito (Scope Transition)
  if IsAdminInstallMode then
  begin
    // Instalando para todos los usuarios: comprobar si existe versión previa en HKCU
    if FindInstallInHive(HKCU, UserUninsExe, UserInstPath) then
    begin
      if SuppressibleMsgBox(ExpandConstant('{cm:ScopeTransitionUserToAll}'), mbConfirmation, MB_YESNO, IDYES) = IDYES then
      begin
        EnsureQapptiaClosed(True);
        KillQapptiaProcesses;

        if (UserUninsExe <> '') and FileExists(UserUninsExe) then
        begin
          Exec(UserUninsExe, '/SILENT /NORESTART /SUPPRESSMSGBOXES', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
        end;

        if (UserInstPath <> '') and DirExists(UserInstPath) then
          DelTree(UserInstPath, True, True, True);

        DeleteDesktopIcons;
        RegDeleteKeyIncludingSubkeys(HKCU, AppSubKeyStandard);
        RegDeleteKeyIncludingSubkeys(HKCU, AppSubKeyLegacy);
      end
      else
      begin
        Result := False;
        Exit;
      end;
    end;
  end
  else
  begin
    // Instalando solo para el usuario: comprobar si existe versión en HKLM
    if FindInstallInHKLM(SystemUninsExe, SystemInstPath) then
    begin
      SuppressibleMsgBox(ExpandConstant('{cm:ScopeTransitionAllToUser}'), mbError, MB_OK, IDOK);
      Result := False;
      Exit;
    end;
  end;
end;

procedure InitializeWizard;
begin
  HasPreviousInstall := FindPreviousInstall(PreviousRootKey, PreviousUninstallExe, PreviousInstallPath);

  if HasPreviousInstall then
  begin
    MaintenancePage := CreateInputOptionPage(
      wpWelcome,
      ExpandConstant('{cm:ExistingInstallTitle}'),
      ExpandConstant('{cm:ExistingInstallDesc}') + #13#10#13#10 + PreviousInstallPath,
      ExpandConstant('{cm:ExistingInstallSubCaption}'),
      True,
      False
    );
    MaintenancePage.Add(ExpandConstant('{cm:OptionReinstall}'));
    MaintenancePage.Add(ExpandConstant('{cm:OptionUninstall}'));
    MaintenancePage.Values[0] := True;
  end;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if HasPreviousInstall and (PageID = wpSelectDir) then
    Result := True;
end;

procedure BringWizardToFront;
var
  ForeWnd: HWND;
  ForeThread, CurThread: DWORD;
  EmptyPid: DWORD;
begin
  try
    WizardForm.BringToFront;
    SetWindowPos(WizardForm.Handle, -1 {HWND_TOPMOST}, 0, 0, 0, 0, $0001 or $0002 {SWP_NOSIZE or SWP_NOMOVE});
    SetWindowPos(WizardForm.Handle, -2 {HWND_NOTOPMOST}, 0, 0, 0, 0, $0001 or $0002 {SWP_NOSIZE or SWP_NOMOVE});

    ForeWnd := GetForegroundWindow;
    if (ForeWnd <> 0) and (ForeWnd <> WizardForm.Handle) then
    begin
      CurThread := GetCurrentThreadId;
      ForeThread := GetWindowThreadProcessId(ForeWnd, EmptyPid);
      if (ForeThread <> 0) and (ForeThread <> CurThread) then
      begin
        AttachThreadInput(CurThread, ForeThread, True);
        try
          BringWindowToTop(WizardForm.Handle);
          SetForegroundWindow(WizardForm.Handle);
        finally
          AttachThreadInput(CurThread, ForeThread, False);
        end;
      end
      else
      begin
        BringWindowToTop(WizardForm.Handle);
        SetForegroundWindow(WizardForm.Handle);
      end;
    end
    else
    begin
      BringWindowToTop(WizardForm.Handle);
      SetForegroundWindow(WizardForm.Handle);
    end;
  except
  end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if not BroughtToFrontOnce then
  begin
    BroughtToFrontOnce := True;
    BringWizardToFront;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
  ExecutedOk: Boolean;
  FileStillExists: Boolean;
  RegStillExists: Boolean;
  RetryCount: Integer;
begin
  Result := True;

  if HasPreviousInstall and (MaintenancePage <> nil) and (CurPageID = MaintenancePage.ID) then
  begin
    if MaintenancePage.Values[1] then
    begin
      // Acción: Desinstalar versión anterior
      if SuppressibleMsgBox(ExpandConstant('{cm:ConfirmUninstall}'), mbConfirmation, MB_YESNO, IDNO) = IDYES then
      begin
        if not EnsureQapptiaClosed(True) then
        begin
          Result := False;
          Exit;
        end;

        KillQapptiaProcesses;
        ExecutedOk := False;

        if (PreviousUninstallExe <> '') and FileExists(PreviousUninstallExe) then
        begin
          if Exec(PreviousUninstallExe, '/SILENT /NORESTART /SUPPRESSMSGBOXES', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
          begin
            if ResultCode = 0 then
              ExecutedOk := True;
          end;
        end
        else
        begin
          // Desinstalador faltante o corrupto: Purga de Rescate
          if SuppressibleMsgBox(ExpandConstant('{cm:RescuePurgePrompt}'), mbConfirmation, MB_YESNO, IDYES) = IDYES then
          begin
            ExecutedOk := True;
          end
          else
          begin
            Result := False;
            Exit;
          end;
        end;

        Sleep(500);
        DeleteDesktopIcons;

        if (PreviousInstallPath <> '') and DirExists(PreviousInstallPath) then
        begin
          RetryCount := 0;
          while DirExists(PreviousInstallPath) and (RetryCount < 15) do
          begin
            DelTree(PreviousInstallPath, True, True, True);
            if DirExists(PreviousInstallPath) then
            begin
              Sleep(200);
              RetryCount := RetryCount + 1;
            end;
          end;
        end;

        // Limpiar claves huérfanas de registro en el hive previo
        RegDeleteKeyIncludingSubkeys(PreviousRootKey, AppSubKeyStandard);
        RegDeleteKeyIncludingSubkeys(PreviousRootKey, AppSubKeyLegacy);

        // TRIPLE VERIFICACIÓN DETERMINISTA ANTI-FALSOS POSITIVOS (Inmutable ante nombres)
        FileStillExists := (PreviousInstallPath <> '') and (DirExists(PreviousInstallPath) or FileExists(PreviousInstallPath + '\Qapptia.exe'));
        RegStillExists  := RegKeyExists(PreviousRootKey, AppSubKeyStandard) or RegKeyExists(PreviousRootKey, AppSubKeyLegacy);

        if ExecutedOk and (not FileStillExists) and (not RegStillExists) then
        begin
          HasPreviousInstall := False;
          PreviousInstallPath := '';
          PreviousUninstallExe := '';

          if SuppressibleMsgBox(ExpandConstant('{cm:UninstallSuccessPromptInstall}'), mbConfirmation, MB_YESNO, IDYES) = IDYES then
          begin
            // Iniciar una instancia limpia del instalador para que consulte el modo
            // de instalación (Por Usuario vs. Para Todos los Usuarios) desde cero
            Exec(ExpandConstant('{srcexe}'), '', '', SW_SHOWNORMAL, ewNoWait, ResultCode);
            WizardForm.Close;
            Result := False;
            Exit;
          end
          else
          begin
            // El usuario solo deseaba desinstalar: cerramos limpiamente la ventana
            WizardForm.Close;
            Result := False;
            Exit;
          end;
        end
        else
        begin
          SuppressibleMsgBox(FmtMessage(ExpandConstant('{cm:UninstallIncomplete}'), [PreviousInstallPath]), mbError, MB_OK, IDOK);
          Result := False;
          Exit;
        end;
      end;
      Result := False;
      Exit;
    end
    else
    begin
      // Acción: Reinstalar, reparar o actualizar
      if not EnsureQapptiaClosed(False) then
      begin
        Result := False;
        Exit;
      end;
      Result := True;
      Exit;
    end;
  end;
end;

function ShouldConfigureTls12: Boolean;
var
  Version: TWindowsVersion;
begin
  GetWindowsVersionEx(Version);
  // Aplica en Windows 7 y 8.x (NT 6.1 a 6.3) cuando el instalador cuenta con privilegios administrativos
  Result := IsAdmin and (Version.Major = 6) and (Version.Minor >= 1);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if not EnsureQapptiaClosed(False) then
    Result := ExpandConstant('{cm:AppStillRunning}');
end;

function InitializeUninstall: Boolean;
begin
  Result := EnsureQapptiaClosed(True);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DeleteDesktopIcons;
  end;
end;
