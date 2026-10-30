; Script de Inno Setup para Qapptia
#define MyAppName "Qapptia"
#define MyAppExeName "Qapptia.exe"
#define EditorExeName "QapptiaEditor.exe"
#define CaptureExeName "QapptiaCapture.exe"
#define ConfigExeName "QapptiaConfig.exe"
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
AppName={#MyAppName}
AppVersion={#AppVersion}
AppPublisher={#MyAppName}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma2/ultra64
SolidCompression=yes
SetupMutex=QapptiaInstaller,Global\QapptiaInstaller,Global\QapptiaUninstaller
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
DisableWelcomePage=yes
DisableDirPage=auto
UsePreviousAppDir=yes
UsePreviousPrivileges=no
UsePreviousTasks=yes
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

english.PrepareInstallFailed=Setup could not prepare the application files because another process has locked them.%n%nPlease close any running applications and try again.
spanish.PrepareInstallFailed=El programa de instalación no pudo preparar los archivos de la aplicación porque otro proceso los tiene bloqueados.%n%nPor favor, cierre cualquier aplicación abierta y vuelva a intentarlo.

english.AppRunningPromptInstall=Setup has detected that {#MyAppName} is currently running.%n%nPlease close the application from the system tray, then click Retry to continue, or Exit to cancel.
spanish.AppRunningPromptInstall=El programa de instalación ha detectado que {#MyAppName} está ejecutándose.%n%nPor favor, ciérrelo desde la bandeja del sistema y haga clic en Reintentar para continuar, o en Salir para cancelar.
english.AppRunningPromptUninstall=Uninstall has detected that {#MyAppName} is currently running.%n%nPlease close the application from the system tray, then click Retry to continue, or Exit to cancel.
spanish.AppRunningPromptUninstall=El programa de desinstalación ha detectado que {#MyAppName} está ejecutándose.%n%nPor favor, ciérrelo desde la bandeja del sistema y haga clic en Reintentar para continuar, o en Salir para cancelar.
english.AppStillRunning={#MyAppName} is still running.
spanish.AppStillRunning={#MyAppName} sigue en ejecución.
english.ButtonRetry=&Retry
spanish.ButtonRetry=&Reintentar
english.ButtonExit=&Exit
spanish.ButtonExit=&Salir
english.AppRunningStillOpen={#MyAppName} is still running. Please close it from the system tray and click Retry.
spanish.AppRunningStillOpen={#MyAppName} aún se encuentra en ejecución. Ciérrela desde la bandeja del sistema y presione Reintentar.

english.ScopeTransitionUserToAll=A per-user installation of Qapptia already exists on this computer.%n%nDo you want Setup to automatically remove it to proceed with the installation for all users?
spanish.ScopeTransitionUserToAll=Ya existe una instalación de Qapptia para el usuario actual en este equipo.%n%n¿Desea que el instalador la desinstale y limpie automáticamente para proceder con la instalación para todos los usuarios?

english.ScopeTransitionAllToUser=An all-users installation of Qapptia already exists on this computer.%n%nDo you want Setup to automatically remove it to proceed with the per-user installation?
spanish.ScopeTransitionAllToUser=Ya existe una instalación de Qapptia para todos los usuarios en este equipo.%n%n¿Desea que el instalador la desinstale y limpie automáticamente para proceder con la instalación por usuario?

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart"; Description: "{cm:AutoStartDescription}"; GroupDescription: "{cm:AdditionalOptions}"

[InstallDelete]
Type: files; Name: "{app}\Qapptia*.old*"
Type: files; Name: "{app}\Qapptia_old*.exe"
Type: files; Name: "{app}\Qapptia_old_*.exe"
Type: filesandordirs; Name: "{app}\app_old_*"
Type: files; Name: "{app}\app\*.old*"
Type: files; Name: "{app}\app\*_old_*"
Type: files; Name: "{app}\app\Qapptia.App.*"
Type: files; Name: "{app}\Qapptia.App.*"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[UninstallDelete]
Type: files; Name: "{autodesktop}\{#MyAppName}.lnk"
Type: files; Name: "{userdesktop}\{#MyAppName}.lnk"
Type: files; Name: "{commondesktop}\{#MyAppName}.lnk"
Type: files; Name: "{app}\Qapptia*.old*"
Type: files; Name: "{app}\Qapptia_old*.exe"
Type: files; Name: "{app}\Qapptia_old_*.exe"
Type: filesandordirs; Name: "{app}\app_old_*"
Type: files; Name: "{app}\app\*.old*"
Type: files; Name: "{app}\app\*_old_*"
Type: filesandordirs; Name: "{app}\app"
Type: filesandordirs; Name: "{app}\logs"
Type: filesandordirs; Name: "{localappdata}\{#MyAppName}\logs"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKA; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"" --capture"; Flags: uninsdeletevalue; Tasks: autostart

; Compatibilidad TLS 1.2 para Windows 7 / 8.x (Schannel y WinHttp)
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client"; ValueType: dword; ValueName: "DisabledByDefault"; ValueData: "0"; Flags: createvalueifdoesntexist; Check: ShouldConfigureTls12
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client"; ValueType: dword; ValueName: "Enabled"; ValueData: "1"; Flags: createvalueifdoesntexist; Check: ShouldConfigureTls12
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\WinHttp"; ValueType: dword; ValueName: "DefaultSecureProtocols"; ValueData: "$00000800"; Flags: createvalueifdoesntexist; Check: ShouldConfigureTls12
Root: HKLM; Subkey: "SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Internet Settings\WinHttp"; ValueType: dword; ValueName: "DefaultSecureProtocols"; ValueData: "$00000800"; Flags: createvalueifdoesntexist; Check: ShouldConfigureTls12

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent runascurrentuser; Check: CanLaunchInstalledApplication

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
  TAppMsg = record
    hwnd: HWND;
    message: UINT;
    wParam: LongInt;
    lParam: LongInt;
    time: DWORD;
    ptX: LongInt;
    ptY: LongInt;
    lPrivate: DWORD;
  end;

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

  TStartupInfo32 = record
    cb: DWORD;
    lpReserved, lpDesktop, lpTitle: LongInt;
    dwX, dwY, dwXSize, dwYSize: DWORD;
    dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags: DWORD;
    wShowWindow, cbReserved2: Word;
    lpReserved2: LongInt;
    hStdInput, hStdOutput, hStdError: THandle;
  end;

  TProcessInformation32 = record
    hProcess, hThread: THandle;
    dwProcessId, dwThreadId: DWORD;
  end;

function PeekMessage(var lpMsg: TAppMsg; hWnd: HWND; wMsgFilterMin, wMsgFilterMax, wRemoveMsg: UINT): BOOL;
  external 'PeekMessageW@user32.dll stdcall';

function TranslateMessage(var lpMsg: TAppMsg): BOOL;
  external 'TranslateMessage@user32.dll stdcall';

function DispatchMessage(var lpMsg: TAppMsg): LongInt;
  external 'DispatchMessageW@user32.dll stdcall';

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

function CreateProcessW(const ApplicationName, CommandLine: string;
  ProcessAttributes, ThreadAttributes: LongInt; InheritHandles: BOOL;
  CreationFlags: DWORD; Environment: LongInt; const CurrentDirectory: string;
  var StartupInfo: TStartupInfo32; var ProcessInformation: TProcessInformation32): BOOL;
  external 'CreateProcessW@kernel32.dll stdcall';

function WaitForSingleObject(hHandle: THandle; dwMilliseconds: DWORD): DWORD;
  external 'WaitForSingleObject@kernel32.dll stdcall';

function GetExitCodeProcess(hProcess: THandle; var ExitCode: DWORD): BOOL;
  external 'GetExitCodeProcess@kernel32.dll stdcall';

function CreateMutexW(MutexAttributes: LongInt; InitialOwner: BOOL; const Name: string): THandle;
  external 'CreateMutexW@kernel32.dll stdcall';

function GetLastError: DWORD;
  external 'GetLastError@kernel32.dll stdcall';

function CreateFileW(const FileName: string; DesiredAccess, ShareMode: DWORD;
  SecurityAttributes: LongInt; CreationDisposition, FlagsAndAttributes: DWORD;
  TemplateFile: THandle): THandle;
  external 'CreateFileW@kernel32.dll stdcall';

function QueryFullProcessImageName(hProcess: THandle; dwFlags: DWORD; lpExeName: string; var lpdwSize: DWORD): BOOL;
  external 'QueryFullProcessImageNameW@kernel32.dll stdcall';

procedure AppProcessMessage;
var
  Msg: TAppMsg;
begin
  while PeekMessage(Msg, 0, 0, 0, 1 {PM_REMOVE}) do
  begin
    TranslateMessage(Msg);
    DispatchMessage(Msg);
  end;
end;


const
  AppSubKeyStandard   = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{' + '{#MyAppId}' + '}_is1';
  AppSubKeyLegacy     = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{' + '{#MyAppId}' + '}}_is1';

var
  PreviousInstallPath: string;
  PreviousUninstallExe: string;
  PreviousRootKey: Integer;
  HasPreviousInstall: Boolean;
  BroughtToFrontOnce: Boolean;
  RecoveryDir: string;
  RecoveryCommitFailed: Boolean;
  MigrationNeeded: Boolean;
  MigrationRootKey: Integer;
  MigrationInstallPath, MigrationUninstallExe: string;
  UninstallMutex: THandle;
  ProcessScanFailed: Boolean;
  BlockingProcess: string;

function GetProcessExePath(PID: DWORD): string;
var
  hProc: THandle;
  Buf: string;
  Size: DWORD;
begin
  Result := '';
  hProc := OpenProcess($1000 {PROCESS_QUERY_LIMITED_INFORMATION}, False, PID);
  if hProc = 0 then
    hProc := OpenProcess($0400 {PROCESS_QUERY_INFORMATION}, False, PID);

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
    Result := Uppercase(ExpandConstant('{autopf}\{#MyAppName}'))
  else
    Result := Uppercase(ExpandConstant('{localappdata}\Programs\{#MyAppName}'));
end;

function ProcessEntryExeName(const pe: TProcessEntry32): string;
var
  I: Integer;
begin
  Result := '';
  for I := 0 to 259 do
  begin
    if pe.szExeFile[I] = #0 then Break;
    Result := Result + pe.szExeFile[I];
  end;
end;

function IsProcessInDir(ProcessID: DWORD; const TargetDir: string; const ExeName: string): Boolean;
var
  ProcPath: string;
  TempDir: string;
  NormDir: string;
  CleanExe: string;
begin
  Result := False;
  // 1. Autoprotección absoluta: nunca evaluar PID 0 ni el propio proceso instalador/desinstalador
  if (ProcessID = 0) or (ProcessID = GetCurrentProcessId) then Exit;

  CleanExe := Uppercase(ExeName);
  ProcPath := Uppercase(GetProcessExePath(ProcessID));
  if ProcPath <> '' then
  begin
    if IsUninstaller then
    begin
      if ProcPath = Uppercase(ExpandConstant('{uninstallexe}')) then Exit;
    end
    else if ProcPath = Uppercase(ExpandConstant('{srcexe}')) then Exit;
  end;

  // Reconocer los ejecutables conocidos en cualquier ubicación.
  if (CleanExe = Uppercase('{#MyAppExeName}')) or (CleanExe = Uppercase('{#EditorExeName}')) or
     (CleanExe = Uppercase('{#CaptureExeName}')) or (CleanExe = Uppercase('{#ConfigExeName}')) then
  begin
    Result := True;
    Exit;
  end;

  // No evaluar procesos auxiliares del instalador o desinstalador.
  if (Pos('SETUP', CleanExe) > 0) or (Pos('UNINS', CleanExe) > 0) or (Pos('IS-', CleanExe) > 0) then Exit;

  // 3. Autoprotección adicional por ruta en carpetas temporales
  if ProcPath <> '' then
  begin
    TempDir := Uppercase(ExpandConstant('{tmp}'));
    if (TempDir <> '') and (Pos(TempDir, ProcPath) = 1) then Exit;

    TempDir := Uppercase(GetEnv('TEMP'));
    if (TempDir <> '') and (Pos(TempDir, ProcPath) = 1) then Exit;

    TempDir := Uppercase(GetEnv('TMP'));
    if (TempDir <> '') and (Pos(TempDir, ProcPath) = 1) then Exit;

    // 4. Inmutabilidad por carpeta: cualquier proceso que corra dentro del directorio de instalación
    if TargetDir <> '' then
    begin
      NormDir := TargetDir;
      if NormDir[Length(NormDir)] <> '\' then
        NormDir := NormDir + '\';

      if Pos(NormDir, ProcPath) = 1 then
      begin
        Result := True;
        Exit;
      end;
    end;
  end;

end;

function HasProcessesInDir(const TargetDir: string): Boolean;
var
  hSnap: THandle;
  pe: TProcessEntry32;
  ExeName: string;
begin
  Result := False;
  ProcessScanFailed := False;
  BlockingProcess := '';

  hSnap := CreateToolhelp32Snapshot($00000002 {TH32CS_SNAPPROCESS}, 0);
  if hSnap = -1 then
  begin
    Log('No se pudo comprobar la lista de procesos.');
    ProcessScanFailed := True;
    Result := True;
    Exit;
  end;
  if hSnap <> -1 then
  begin
    try
      pe.dwSize := SizeOf(pe);
      if Process32First(hSnap, pe) then
      begin
        repeat
          ExeName := ProcessEntryExeName(pe);
          if IsProcessInDir(pe.th32ProcessID, TargetDir, ExeName) then
          begin
            BlockingProcess := ExeName + ' (PID: ' + IntToStr(pe.th32ProcessID) + ')';
            Result := True;
            Exit;
          end;
        until not Process32Next(hSnap, pe);
      end
      else
      begin
        Log('No se pudo leer la lista de procesos.');
        ProcessScanFailed := True;
        Result := True;
      end;
    finally
      CloseHandle(hSnap);
    end;
  end;
end;

function IsAppRunning: Boolean;
begin
  Result := HasProcessesInDir(GetActiveInstallDir);
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
  DeleteFile(ExpandConstant('{autodesktop}\{#MyAppName}.lnk'));
  DeleteFile(ExpandConstant('{userdesktop}\{#MyAppName}.lnk'));
  DeleteFile(ExpandConstant('{commondesktop}\{#MyAppName}.lnk'));

  UserProfile := GetEnv('USERPROFILE');
  if UserProfile <> '' then
  begin
    DeleteFile(UserProfile + '\Desktop\{#MyAppName}.lnk');
    DeleteFile(UserProfile + '\OneDrive\Desktop\{#MyAppName}.lnk');
  end;
end;

// Diálogo nativo estándar para advertir procesos en ejecución en instalador y desinstalador

function EnsureQapptiaClosed(IsUninstalling: Boolean): Boolean;
var
  PromptMsg: string;
begin
  Result := True;

  // Si la aplicación no está en ejecución, continuar inmediatamente
  if not IsAppRunning then Exit;

  // En modo desatendido, no se interrumpe el trabajo de otra instancia.
  if IsSilentMode then
  begin
    Log('No se puede continuar sin cerrar Qapptia: ' + BlockingProcess);
    Result := False;
    Exit;
  end;

  // En modo interactivo: Diálogo modal nativo de Windows con Reintentar / Salir
  if IsUninstalling then
    PromptMsg := ExpandConstant('{cm:AppRunningPromptUninstall}')
  else
    PromptMsg := ExpandConstant('{cm:AppRunningPromptInstall}');

  while IsAppRunning do
  begin
    if ProcessScanFailed then
      PromptMsg := 'No se pudo comprobar si Qapptia está en ejecución. Reintente o cancele.'
    else if BlockingProcess <> '' then
    begin
      if IsUninstalling then
        PromptMsg := ExpandConstant('{cm:AppRunningPromptUninstall}')
      else
        PromptMsg := ExpandConstant('{cm:AppRunningPromptInstall}');
      PromptMsg := PromptMsg + #13#10#13#10 + BlockingProcess;
    end;
    if SuppressibleMsgBox(PromptMsg, mbConfirmation, MB_RETRYCANCEL, IDRETRY) <> IDRETRY then
    begin
      Result := False;
      Exit;
    end;
  end;

  Result := not IsAppRunning;
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

function FindInstallInHKLM(var RootKey: Integer; var UninsExe: string; var InstPath: string): Boolean;
begin
  RootKey := HKLM;
  Result := FindInstallInHive(HKLM, UninsExe, InstPath);
  if not Result and IsWin64 then
  begin
    RootKey := HKLM64;
    Result := FindInstallInHive(HKLM64, UninsExe, InstPath);
  end;
  if not Result then
  begin
    RootKey := HKLM32;
    Result := FindInstallInHive(HKLM32, UninsExe, InstPath);
  end;
end;

function FindPreviousInstall(var RootKey: Integer; var UninsExe: string; var InstPath: string): Boolean;
begin
  if FindInstallInHive(HKCU, UninsExe, InstPath) then
  begin
    RootKey := HKCU;
    Result := True;
  end
  else if FindInstallInHKLM(RootKey, UninsExe, InstPath) then
  begin
    Result := True;
  end
  else
  begin
    Result := False;
  end;
end;

function RunUninstallerBounded(const UninsExe: string; var ExitCode: DWORD): Boolean;
var
  StartupInfo: TStartupInfo32;
  ProcessInfo: TProcessInformation32;
  CommandLine: string;
  WaitResult: DWORD;
  WaitCount: Integer;
begin
  Result := False;
  ExitCode := DWORD(-1);
  StartupInfo.cb := SizeOf(StartupInfo);
  StartupInfo.lpReserved := 0;
  StartupInfo.lpDesktop := 0;
  StartupInfo.lpTitle := 0;
  StartupInfo.dwX := 0;
  StartupInfo.dwY := 0;
  StartupInfo.dwXSize := 0;
  StartupInfo.dwYSize := 0;
  StartupInfo.dwXCountChars := 0;
  StartupInfo.dwYCountChars := 0;
  StartupInfo.dwFillAttribute := 0;
  StartupInfo.dwFlags := 0;
  StartupInfo.wShowWindow := 0;
  StartupInfo.cbReserved2 := 0;
  StartupInfo.lpReserved2 := 0;
  StartupInfo.hStdInput := 0;
  StartupInfo.hStdOutput := 0;
  StartupInfo.hStdError := 0;
  CommandLine := '"' + UninsExe + '" /SILENT /NORESTART /SUPPRESSMSGBOXES /QAPPTIAMIGRATION=1';
  if not CreateProcessW(UninsExe, CommandLine, 0, 0, False, 0, 0,
    ExtractFileDir(UninsExe), StartupInfo, ProcessInfo) then
  begin
    Log('No se pudo iniciar el desinstalador anterior.');
    Result := True;
    Exit;
  end;
  try
    WaitCount := 0;
    repeat
      WaitResult := WaitForSingleObject(ProcessInfo.hProcess, 100);
      AppProcessMessage;
      WaitCount := WaitCount + 1;
    until (WaitResult <> $00000102) or (WaitCount >= 600);
    if WaitResult <> 0 then
    begin
      Log('El desinstalador anterior no terminó dentro del plazo.');
      Exit;
    end;
    Result := GetExitCodeProcess(ProcessInfo.hProcess, ExitCode);
  finally
    CloseHandle(ProcessInfo.hThread);
    CloseHandle(ProcessInfo.hProcess);
  end;
end;

function CanDeleteFile(const FileName: string): Boolean; forward;
function CanDeleteDirectoryFiles(const DirPath: string; var BlockedPath: string): Boolean; forward;

function PurgeInstallationHive(RootKey: Integer; const InstPath, UninsExe: string): Boolean;
var
  ExitCode: DWORD;
  NeedsRescue: Boolean;
  AppDir, AppExe, NormalPath, UninsName, BlockedPath: string;
begin
  Result := False;
  if (RootKey <> HKCU) and not IsAdmin then
  begin
    Log('La instalación anterior requiere permisos de administrador.');
    Exit;
  end;

  NormalPath := '';
  if InstPath <> '' then
    NormalPath := RemoveBackslashUnlessRoot(ExpandFileName(InstPath));
  if (NormalPath <> '') and
     ((NormalPath = ExtractFileDrive(NormalPath) + '\') or
      (CompareText(NormalPath, ExpandConstant('{win}')) = 0)) then Exit;

  NeedsRescue := True;
  UninsName := Uppercase(ExtractFileName(UninsExe));
  if (NormalPath <> '') and (Pos('UNINS', UninsName) = 1) and
     (CompareText(ExtractFileExt(UninsExe), '.exe') = 0) and
     (CompareText(RemoveBackslashUnlessRoot(ExpandFileName(ExtractFileDir(UninsExe))), NormalPath) = 0) and
     FileExists(UninsExe) then
  begin
    if not RunUninstallerBounded(UninsExe, ExitCode) then Exit;
    Log('Desinstalador anterior finalizó con código: ' + IntToStr(ExitCode));
    NeedsRescue := ExitCode <> 0;
  end;

  if NeedsRescue then
  begin
    if WizardSilent then
    begin
      Log('La instalación anterior requiere recuperación interactiva.');
      Exit;
    end;
    if MsgBox('No se pudo ejecutar el desinstalador anterior. ¿Desea eliminar los binarios de Qapptia para continuar?', mbConfirmation, MB_YESNO) <> IDYES then Exit;
  end;

  if NormalPath <> '' then
  begin
    AppDir := NormalPath + '\app';
    AppExe := NormalPath + '\{#MyAppExeName}';
    if NeedsRescue then
    begin
      BlockedPath := AppExe;
      if not CanDeleteFile(AppExe) or not CanDeleteDirectoryFiles(AppDir, BlockedPath) then
      begin
        Log('No se puede completar la recuperación: ' + BlockedPath);
        Exit;
      end;
    end;
    if DirExists(AppDir) and NeedsRescue then
    begin
      if not FileExists(AppDir + '\Qapptia.dll') and
         not FileExists(AppDir + '\{#EditorExeName}') and
         not FileExists(AppDir + '\{#CaptureExeName}') then Exit;
      DelTree(AppDir, True, True, True);
    end;
    if NeedsRescue and FileExists(AppExe) then DeleteFile(AppExe);
    if DirExists(AppDir) or FileExists(AppExe) then
    begin
      Log('La instalación anterior conserva archivos; no se eliminará su registro: ' + NormalPath);
      Exit;
    end;
  end;

  RegDeleteKeyIncludingSubkeys(RootKey, AppSubKeyStandard);
  RegDeleteKeyIncludingSubkeys(RootKey, AppSubKeyLegacy);
  if RegKeyExists(RootKey, AppSubKeyStandard) or RegKeyExists(RootKey, AppSubKeyLegacy) then Exit;
  DeleteDesktopIcons;
  Result := True;
end;

function InitializeSetup: Boolean;
var
  UserUninsExe, UserInstPath: string;
  SystemUninsExe, SystemInstPath: string;
  SystemRootKey: Integer;
begin
  Result := False;

  // Carga de instalación previa en el registro para resolución de rutas
  HasPreviousInstall := FindPreviousInstall(PreviousRootKey, PreviousUninstallExe, PreviousInstallPath);

  // 1. Validación al arrancar el instalador: Qapptia no debe estar en ejecución
  if not EnsureQapptiaClosed(False) then
    Exit;

  Result := True;

  // Detección de colisión de ámbito entre instalación de usuario y máquina con contingencia de rescate
  if IsAdminInstallMode then
  begin
    // Instalando para todos los usuarios: comprobar si existe versión previa en HKCU
    if FindInstallInHive(HKCU, UserUninsExe, UserInstPath) then
    begin
      if WizardSilent then
      begin
        Log('El cambio de ámbito requiere confirmación interactiva.');
        Result := False;
        Exit;
      end;
      if SuppressibleMsgBox(ExpandConstant('{cm:ScopeTransitionUserToAll}'), mbConfirmation, MB_YESNO, IDYES) = IDYES then
      begin
        MigrationNeeded := True;
        MigrationRootKey := HKCU;
        MigrationInstallPath := UserInstPath;
        MigrationUninstallExe := UserUninsExe;
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
    if FindInstallInHKLM(SystemRootKey, SystemUninsExe, SystemInstPath) then
    begin
      if WizardSilent then
      begin
        Log('El cambio de ámbito requiere confirmación interactiva.');
        Result := False;
        Exit;
      end;
      if not IsAdmin then
      begin
        Log('El cambio de ámbito requiere permisos de administrador.');
        Result := False;
        MsgBox('Para cambiar la instalación existente se requieren permisos de administrador.', mbError, MB_OK);
        Exit;
      end;
      if SuppressibleMsgBox(ExpandConstant('{cm:ScopeTransitionAllToUser}'), mbConfirmation, MB_YESNO, IDYES) = IDYES then
      begin
        MigrationNeeded := True;
        MigrationRootKey := SystemRootKey;
        MigrationInstallPath := SystemInstPath;
        MigrationUninstallExe := SystemUninsExe;
      end
      else
      begin
        Result := False;
        Exit;
      end;
    end;
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
begin
  Result := True;

  // Validación en pasos críticos del asistente antes de proceder
  if (CurPageID = wpSelectDir) or (CurPageID = wpReady) then
  begin
    if not EnsureQapptiaClosed(False) then
    begin
      Result := False;
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

function RestorePreviousInstallation(const BackupDir, TargetDir: string): Boolean;
var
  OldApp, OldExe, AppDir, AppExe: string;
  Owner: AnsiString;
begin
  Result := False;
  if not DirExists(BackupDir) then
  begin
    Result := True;
    Exit;
  end;

  if not LoadStringFromFile(BackupDir + '\owner', Owner) or (Owner <> '{#MyAppId}') then
  begin
    Log('El directorio de recuperación no pertenece a esta instalación: ' + BackupDir);
    Exit;
  end;

  if FileExists(BackupDir + '\committed') then
  begin
    DelTree(BackupDir, True, True, True);
    Result := not DirExists(BackupDir);
    Exit;
  end;

  OldApp := BackupDir + '\app';
  OldExe := BackupDir + '\{#MyAppExeName}';
  AppDir := TargetDir + '\app';
  AppExe := TargetDir + '\{#MyAppExeName}';
  if DirExists(OldApp) then
  begin
    if DirExists(AppDir) then DelTree(AppDir, True, True, True);
    if DirExists(AppDir) or not RenameFile(OldApp, AppDir) then Exit;
  end;
  if FileExists(OldExe) then
  begin
    if FileExists(AppExe) and not DeleteFile(AppExe) then Exit;
    if not RenameFile(OldExe, AppExe) then Exit;
  end;
  if not DeleteFile(BackupDir + '\owner') then Exit;
  RemoveDir(BackupDir);
  Result := not DirExists(BackupDir);
end;

function PreservePreviousInstallation(const TargetDir: string): Boolean;
var
  AppDir, AppExe: string;
begin
  Result := False;
  RecoveryDir := TargetDir + '\.qapptia-recovery';
  if not RestorePreviousInstallation(RecoveryDir, TargetDir) then Exit;
  AppDir := TargetDir + '\app';
  AppExe := TargetDir + '\{#MyAppExeName}';
  if not DirExists(AppDir) and not FileExists(AppExe) then
  begin
    RecoveryDir := '';
    Result := True;
    Exit;
  end;
  if DirExists(AppDir) and not FileExists(AppDir + '\Qapptia.dll') and
     not FileExists(AppDir + '\{#EditorExeName}') and
     not FileExists(AppDir + '\{#CaptureExeName}') and
     not FileExists(AppDir + '\Qapptia.App.Editor.exe') and
     not FileExists(AppDir + '\Qapptia.App.Capture.exe') then
  begin
    Log('No se reemplazará una carpeta app que no se identifica como Qapptia: ' + AppDir);
    Exit;
  end;
  if not ForceDirectories(RecoveryDir) then Exit;
  if not SaveStringToFile(RecoveryDir + '\owner', '{#MyAppId}', False) then
  begin
    RemoveDir(RecoveryDir);
    Exit;
  end;
  if DirExists(AppDir) and not RenameFile(AppDir, RecoveryDir + '\app') then Exit;
  if FileExists(AppExe) and not RenameFile(AppExe, RecoveryDir + '\{#MyAppExeName}') then
  begin
    RestorePreviousInstallation(RecoveryDir, TargetDir);
    Exit;
  end;
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  MigrationSucceeded: Boolean;
begin
  if CurStep <> ssPostInstall then Exit;
  if not FileExists(ExpandConstant('{app}\{#MyAppExeName}')) or
     not FileExists(ExpandConstant('{app}\app\Qapptia.dll')) or
     not FileExists(ExpandConstant('{app}\app\coreclr.dll')) or
     not FileExists(ExpandConstant('{app}\app\{#EditorExeName}')) or
     not FileExists(ExpandConstant('{app}\app\{#CaptureExeName}')) or
     not FileExists(ExpandConstant('{app}\app\{#ConfigExeName}')) then
  begin
    RecoveryCommitFailed := True;
    Log('No se encontraron todos los binarios necesarios después de la instalación.');
    if not WizardSilent then
      MsgBox('La instalación no contiene todos los archivos necesarios. Se intentará restaurar la versión anterior.', mbError, MB_OK);
    Exit;
  end;
  if (RecoveryDir <> '') and DirExists(RecoveryDir) then
  begin
    if SaveStringToFile(RecoveryDir + '\committed', 'ok', False) then
      DelTree(RecoveryDir, True, True, True)
    else
    begin
      RecoveryCommitFailed := True;
      Log('No se pudo confirmar la instalación; se intentará restaurar: ' + RecoveryDir);
      if not WizardSilent then
        MsgBox('No se pudo confirmar la actualización. Se intentará restaurar la versión anterior.', mbError, MB_OK);
    end;
  end;
  if MigrationNeeded and not RecoveryCommitFailed then
  begin
    if (MigrationInstallPath <> '') and
       (CompareText(RemoveBackslashUnlessRoot(ExpandFileName(MigrationInstallPath)),
                    RemoveBackslashUnlessRoot(ExpandFileName(ExpandConstant('{app}')))) = 0) then
    begin
      Log('No se eliminará la instalación anterior porque comparte el directorio de la instalación nueva.');
      MigrationSucceeded := False;
    end
    else
      MigrationSucceeded := PurgeInstallationHive(MigrationRootKey, MigrationInstallPath, MigrationUninstallExe);
    if WizardIsTaskSelected('desktopicon') then
    begin
      try
        CreateShellLink(ExpandConstant('{autodesktop}\{#MyAppName}.lnk'), '',
          ExpandConstant('{app}\{#MyAppExeName}'), '', ExpandConstant('{app}'), '', 0, SW_SHOWNORMAL);
      except
        Log('No se pudo recrear el acceso directo: ' + GetExceptionMessage);
      end;
    end;
    if not MigrationSucceeded then
    begin
      RecoveryCommitFailed := True;
      Log('La instalación nueva está disponible, pero no se pudo retirar la instalación anterior: ' + MigrationInstallPath);
      if not WizardSilent then
        MsgBox('Qapptia se instaló, pero no se pudo retirar la instalación anterior. Revise los permisos y archivos en uso antes de reintentar.', mbError, MB_OK);
    end;
  end;
end;

function GetCustomSetupExitCode: Integer;
begin
  if RecoveryCommitFailed then Result := 1 else Result := 0;
end;

function CanLaunchInstalledApplication: Boolean;
begin
  Result := not RecoveryCommitFailed;
end;

procedure DeinitializeSetup;
begin
  if (RecoveryDir <> '') and DirExists(RecoveryDir) then
    if not RestorePreviousInstallation(RecoveryDir, ExpandConstant('{app}')) then
    begin
      Log('No se pudo restaurar la instalación anterior: ' + RecoveryDir);
      if not WizardSilent then
        MsgBox('No se pudo restaurar la instalación anterior. Los archivos de recuperación se conservan en: ' + RecoveryDir, mbError, MB_OK);
    end;
end;

function CanDeleteFile(const FileName: string): Boolean;
var
  FileHandle: THandle;
begin
  Result := True;
  if not FileExists(FileName) then Exit;
  FileHandle := CreateFileW(FileName, $00010000 {DELETE}, 7 {compartir lectura, escritura y eliminación},
    0, 3 {OPEN_EXISTING}, $80 {FILE_ATTRIBUTE_NORMAL}, 0);
  Result := FileHandle <> -1;
  if Result then CloseHandle(FileHandle)
  else Log('No se puede eliminar el archivo en uso o sin permisos: ' + FileName);
end;

function CanDeleteDirectoryFiles(const DirPath: string; var BlockedPath: string): Boolean;
var
  Entry: TFindRec;
  Path: string;
begin
  Result := True;
  if not DirExists(DirPath) then Exit;
  if not FindFirst(DirPath + '\*', Entry) then
  begin
    BlockedPath := DirPath;
    Result := False;
    Exit;
  end;
  try
    repeat
      if (Entry.Name <> '.') and (Entry.Name <> '..') then
      begin
        Path := DirPath + '\' + Entry.Name;
        if (Entry.Attributes and $400 {FILE_ATTRIBUTE_REPARSE_POINT}) <> 0 then
        begin
          BlockedPath := Path;
          Result := False;
          Exit;
        end;
        if (Entry.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
        begin
          if not CanDeleteDirectoryFiles(Path, BlockedPath) then
          begin
            Result := False;
            Exit;
          end;
        end
        else if not CanDeleteFile(Path) then
        begin
          BlockedPath := Path;
          Result := False;
          Exit;
        end;
      end;
    until not FindNext(Entry);
  finally
    FindClose(Entry);
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  NeedsRestart := False;

  // 1. Garantizar que ninguna instancia de Qapptia esté activa
  if not EnsureQapptiaClosed(False) then
  begin
    Result := ExpandConstant('{cm:AppStillRunning}');
    Exit;
  end;

  // 2. Preparar y aislar directorios para evitar colisiones de archivos
  if not PreservePreviousInstallation(ExpandConstant('{app}')) then
  begin
    Result := ExpandConstant('{cm:PrepareInstallFailed}');
    Exit;
  end;
end;

function InitializeUninstall: Boolean;
var
  ErrorCode: DWORD;
  BackupDir, BlockedPath: string;
begin
  Result := False;
  if (ExpandConstant('{param:QAPPTIAMIGRATION|}') <> '1') and
     CheckForMutexes('QapptiaInstaller,Global\QapptiaInstaller') then
  begin
    if not UninstallSilent then MsgBox('Hay una instalación de Qapptia en curso. Espere a que termine.', mbError, MB_OK);
    Exit;
  end;
  UninstallMutex := CreateMutexW(0, False, 'Global\QapptiaUninstaller');
  ErrorCode := GetLastError;
  if UninstallMutex = 0 then Exit;
  if ErrorCode = 183 then
  begin
    CloseHandle(UninstallMutex);
    UninstallMutex := 0;
    if not UninstallSilent then MsgBox('Ya hay una desinstalación de Qapptia en curso.', mbError, MB_OK);
    Exit;
  end;
  if not EnsureQapptiaClosed(True) then Exit;
  BackupDir := ExpandConstant('{app}\.qapptia-recovery');
  if not RestorePreviousInstallation(BackupDir, ExpandConstant('{app}')) then
  begin
    if not UninstallSilent then MsgBox('No se pudo recuperar la instalación anterior. Revise los archivos en: ' + BackupDir, mbError, MB_OK);
    Exit;
  end;
  BlockedPath := ExpandConstant('{app}\{#MyAppExeName}');
  if not CanDeleteFile(BlockedPath) or
     not CanDeleteDirectoryFiles(ExpandConstant('{app}\app'), BlockedPath) then
  begin
    Log('No se puede comenzar la desinstalación: ' + BlockedPath);
    if not UninstallSilent then
      MsgBox('No se puede desinstalar porque un archivo está en uso o no tiene permisos: ' + BlockedPath, mbError, MB_OK);
    Exit;
  end;
  Result := True;
end;

procedure DeinitializeUninstall;
begin
  if UninstallMutex <> 0 then CloseHandle(UninstallMutex);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    if not EnsureQapptiaClosed(True) then
      Abort;
  end
  else if CurUninstallStep = usPostUninstall then
  begin
    DeleteDesktopIcons;
    DelTree(ExpandConstant('{localappdata}\{#MyAppName}\logs'), True, True, True);
    RemoveDir(ExpandConstant('{app}\app'));
    RemoveDir(ExpandConstant('{app}'));
    if FileExists(ExpandConstant('{app}\{#MyAppExeName}')) or
       FileExists(ExpandConstant('{app}\app\Qapptia.dll')) or
       FileExists(ExpandConstant('{app}\app\{#EditorExeName}')) or
       FileExists(ExpandConstant('{app}\app\{#CaptureExeName}')) or
       FileExists(ExpandConstant('{app}\app\{#ConfigExeName}')) then
    begin
      Log('La desinstalación no pudo eliminar todos los binarios de Qapptia: ' + ExpandConstant('{app}'));
      if not UninstallSilent then
        MsgBox('La desinstalación quedó incompleta. Cierre los procesos que usan los archivos y ejecute el instalador para reparar o reintentar: ' + ExpandConstant('{app}'), mbError, MB_OK);
      Abort;
    end;
  end;
end;
