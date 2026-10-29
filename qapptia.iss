; Script de Inno Setup para Qapptia
#define MyAppName "Qapptia"
#define MyAppExeName "Qapptia.exe"
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
CloseApplications=force
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
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs restartreplace

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
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent runascurrentuser

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

function TerminateProcess(hProcess: THandle; uExitCode: UINT): BOOL;
  external 'TerminateProcess@kernel32.dll stdcall';

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


procedure ProcessSleep(Milliseconds: Integer);
var
  Elapsed: Integer;
begin
  Elapsed := 0;
  while Elapsed < Milliseconds do
  begin
    AppProcessMessage;
    Sleep(50);
    Elapsed := Elapsed + 50;
  end;
  AppProcessMessage;
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
  SelfExe: string;
  ProcPath: string;
  TempDir: string;
  NormDir: string;
  CleanExe: string;
begin
  Result := False;
  // 1. Autoprotección absoluta: nunca evaluar PID 0 ni el propio proceso instalador/desinstalador
  if (ProcessID = 0) or (ProcessID = GetCurrentProcessId) then Exit;

  CleanExe := Uppercase(ExeName);

  // 2. Autoprotección: nunca evaluar ejecutables propios ni temporales del instalador
  SelfExe := GetSelfExeName;
  if (SelfExe <> '') and ((CleanExe = SelfExe) or (CleanExe = ChangeFileExt(SelfExe, '.TMP'))) then Exit;
  if (Pos('SETUP', CleanExe) > 0) or (Pos('UNINS', CleanExe) > 0) or (Pos('IS-', CleanExe) > 0) then Exit;

  // 3. Autoprotección adicional por ruta en carpetas temporales
  ProcPath := Uppercase(GetProcessExePath(ProcessID));
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

  // 5. Identificación transversal: cualquier proceso de la suite que contenga el nombre de la aplicación
  if Pos(Uppercase('{#MyAppName}'), CleanExe) > 0 then
    Result := True;
end;

function HasProcessesInDir(const TargetDir: string): Boolean;
var
  hSnap: THandle;
  pe: TProcessEntry32;
  ExeName: string;
begin
  Result := False;

  hSnap := CreateToolhelp32Snapshot($00000002 {TH32CS_SNAPPROCESS}, 0);
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

procedure KillProcessesInDir(const TargetDir: string);
var
  hSnap: THandle;
  pe: TProcessEntry32;
  ExeName: string;
  hProc: THandle;
  ResultCode: Integer;
begin
  hSnap := CreateToolhelp32Snapshot($00000002 {TH32CS_SNAPPROCESS}, 0);
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
            hProc := OpenProcess($0001 {PROCESS_TERMINATE}, False, pe.th32ProcessID);
            if hProc <> 0 then
            begin
              try
                TerminateProcess(hProc, 0);
              finally
                CloseHandle(hProc);
              end;
            end;
          end;
        until not Process32Next(hSnap, pe);
      end;
    finally
      CloseHandle(hSnap);
    end;
  end;

  // Fallback con taskkill del sistema para procesos colgados o con bloqueos de kernel/antivirus
  try
    Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /T /IM {#MyAppName}*.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  except
  end;
end;

// Diálogo nativo estándar para advertir procesos en ejecución en instalador y desinstalador

function EnsureQapptiaClosed(IsUninstalling: Boolean): Boolean;
var
  ActiveDir: string;
  WaitCount: Integer;
  PromptMsg: string;
begin
  Result := True;

  // Si la aplicación no está en ejecución, continuar inmediatamente
  if not IsAppRunning then Exit;

  // En modo desatendido (silencioso), liberar procesos para permitir la actualización
  if IsSilentMode then
  begin
    ActiveDir := GetActiveInstallDir;
    KillProcessesInDir(ActiveDir);
    WaitCount := 0;
    while IsAppRunning and (WaitCount < 10) do
    begin
      ProcessSleep(100);
      WaitCount := WaitCount + 1;
    end;
    Result := not IsAppRunning;
    Exit;
  end;

  // En modo interactivo: Diálogo modal nativo de Windows con Reintentar / Salir
  if IsUninstalling then
    PromptMsg := ExpandConstant('{cm:AppRunningPromptUninstall}')
  else
    PromptMsg := ExpandConstant('{cm:AppRunningPromptInstall}');

  while IsAppRunning do
  begin
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

procedure PurgeInstallationHive(RootKey: Integer; const InstPath, UninsExe: string);
var
  ResultCode: Integer;
  RetryCount: Integer;
begin
  // 1. Intentar desinstalación limpia y desatendida
  if (UninsExe <> '') and FileExists(UninsExe) then
  begin
    Exec(UninsExe, '/SILENT /NORESTART /SUPPRESSMSGBOXES', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;

  // 2. Contingencia de Purga de Rescate: si el desinstalador anterior falló o dejó residuos
  if (InstPath <> '') and DirExists(InstPath) then
  begin
    RetryCount := 0;
    while DirExists(InstPath) and (RetryCount < 10) do
    begin
      DelTree(InstPath, True, True, True);
      RemoveDir(InstPath);
      if not DirExists(InstPath) then Break;
      ProcessSleep(100);
      RetryCount := RetryCount + 1;
    end;
  end;

  // 3. Limpieza de claves de registro huérfanas en el hive previo
  RegDeleteKeyIncludingSubkeys(RootKey, AppSubKeyStandard);
  RegDeleteKeyIncludingSubkeys(RootKey, AppSubKeyLegacy);
  DeleteDesktopIcons;
end;

function InitializeSetup: Boolean;
var
  UserUninsExe, UserInstPath: string;
  SystemUninsExe, SystemInstPath: string;
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
      if SuppressibleMsgBox(ExpandConstant('{cm:ScopeTransitionUserToAll}'), mbConfirmation, MB_YESNO, IDYES) = IDYES then
      begin
        PurgeInstallationHive(HKCU, UserInstPath, UserUninsExe);
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
      if SuppressibleMsgBox(ExpandConstant('{cm:ScopeTransitionAllToUser}'), mbConfirmation, MB_YESNO, IDYES) = IDYES then
      begin
        PurgeInstallationHive(HKLM, SystemInstPath, SystemUninsExe);
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

function GetUniqueQuarantineDir(const BaseDir, Prefix: string): string;
var
  Timestamp: string;
  Candidate: string;
  Counter: Integer;
begin
  Timestamp := GetDateTimeString('yyyymmdd_hhnnss', #0, #0);
  Candidate := BaseDir + '\' + Prefix + '_' + Timestamp;
  if not DirExists(Candidate) and not FileExists(Candidate) then
  begin
    Result := Candidate;
    Exit;
  end;
  Counter := 1;
  while True do
  begin
    Candidate := BaseDir + '\' + Prefix + '_' + Timestamp + '_' + IntToStr(Counter);
    if not DirExists(Candidate) and not FileExists(Candidate) then
    begin
      Result := Candidate;
      Exit;
    end;
    Counter := Counter + 1;
  end;
end;

function GetUniqueQuarantineFile(const BaseDir, FileName: string): string;
var
  Timestamp: string;
  Candidate: string;
  Counter: Integer;
begin
  Timestamp := GetDateTimeString('yyyymmdd_hhnnss', #0, #0);
  Candidate := BaseDir + '\' + FileName + '.old_' + Timestamp;
  if not FileExists(Candidate) and not DirExists(Candidate) then
  begin
    Result := Candidate;
    Exit;
  end;
  Counter := 1;
  while True do
  begin
    Candidate := BaseDir + '\' + FileName + '.old_' + Timestamp + '_' + IntToStr(Counter);
    if not FileExists(Candidate) and not DirExists(Candidate) then
    begin
      Result := Candidate;
      Exit;
    end;
    Counter := Counter + 1;
  end;
end;

function QuarantineFilesInDir(const DirPath: string): Boolean;
var
  FindRec: TFindRec;
  FilePath: string;
  OldFilePath: string;
begin
  Result := True;
  if not DirExists(DirPath) then Exit;

  if FindFirst(DirPath + '\*', FindRec) then
  begin
    try
      repeat
        if (FindRec.Name <> '.') and (FindRec.Name <> '..') then
        begin
          FilePath := DirPath + '\' + FindRec.Name;
          if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
          begin
            if not QuarantineFilesInDir(FilePath) then
              Result := False;
            RemoveDir(FilePath);
          end
          else
          begin
            // Si el archivo ya es un residuo de cuarentena de una ejecución previa
            if (Pos('.old_', FindRec.Name) > 0) or (Pos('_old_', FindRec.Name) > 0) then
            begin
              // Intentar borrarlo; si sigue bloqueado, se ignora pues no colisiona con el archivo canónico
              DeleteFile(FilePath);
            end
            else
            begin
              // Es un archivo canónico: intentar borrarlo directamente
              if not DeleteFile(FilePath) then
              begin
                // Si está bloqueado, renombrarlo a cuarentena para liberar el nombre canónico
                OldFilePath := GetUniqueQuarantineFile(DirPath, FindRec.Name);
                if not RenameFile(FilePath, OldFilePath) then
                begin
                  Result := False;
                end
                else
                begin
                  // Intento de borrado no bloqueante del archivo recién puesto en cuarentena
                  DeleteFile(OldFilePath);
                end;
              end;
            end;
          end;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

function PrepareApplicationDirectory(const TargetDir: string): Boolean;
var
  AppBinDir: string;
  QuarantineDir: string;
  ExeFile: string;
  OldExeFile: string;
  RetryCount: Integer;
  CmdResult: Integer;
begin
  Result := True;
  if (TargetDir = '') or (not DirExists(TargetDir)) then Exit;

  AppBinDir := TargetDir + '\app';

  // 1. Manejo determinista de la subcarpeta de binarios 'app'
  if DirExists(AppBinDir) then
  begin
    // Nivel 1: Eliminación limpia con reintentos para dar tiempo al kernel a liberar descriptores
    RetryCount := 0;
    while DirExists(AppBinDir) and (RetryCount < 5) do
    begin
      DelTree(AppBinDir, True, True, True);
      RemoveDir(AppBinDir);
      if not DirExists(AppBinDir) then Break;
      ProcessSleep(100);
      RetryCount := RetryCount + 1;
    end;

    // Contingencia de carpeta bloqueada: renombrar a directorio de cuarentena único
    if DirExists(AppBinDir) then
    begin
      QuarantineDir := GetUniqueQuarantineDir(TargetDir, 'app_old');
      if RenameFile(AppBinDir, QuarantineDir) then
      begin
        // Limpieza no bloqueante del directorio de cuarentena
        DelTree(QuarantineDir, True, True, True);
        RemoveDir(QuarantineDir);
      end
      else
      begin
        // Contingencia por comando nativo del sistema desde carpeta temporal
        Exec(ExpandConstant('{cmd}'), '/c rd /s /q "' + AppBinDir + '"', ExpandConstant('{tmp}'), SW_HIDE, ewWaitUntilTerminated, CmdResult);
        ProcessSleep(150);

        if DirExists(AppBinDir) then
        begin
          // Cuarentena granular de archivos individuales bloqueados dentro de app
          if not QuarantineFilesInDir(AppBinDir) then
          begin
            // Abortar si algún archivo esencial no pudo liberarse ni ponerse en cuarentena
            Result := False;
            Exit;
          end;
        end;
      end;
    end;
  end;

  // 2. Manejo determinista del ejecutable lanzador en la raíz ({app}\Qapptia.exe)
  ExeFile := TargetDir + '\{#MyAppExeName}';
  if FileExists(ExeFile) then
  begin
    RetryCount := 0;
    while FileExists(ExeFile) and (RetryCount < 5) do
    begin
      if DeleteFile(ExeFile) then Break;
      ProcessSleep(100);
      RetryCount := RetryCount + 1;
    end;

    if FileExists(ExeFile) then
    begin
      // Contingencia atómica: Renombrar ejecutable bloqueado a cuarentena con nombre único garantizado
      OldExeFile := GetUniqueQuarantineFile(TargetDir, '{#MyAppExeName}');
      if not RenameFile(ExeFile, OldExeFile) then
      begin
        Result := False;
        Exit;
      end;
      // Intento de borrado no bloqueante del archivo en cuarentena
      DeleteFile(OldExeFile);
    end;
  end;

  // 3. Verificación final estricta de seguridad: el ejecutable lanzador canónico no debe existir
  if FileExists(ExeFile) then
  begin
    Result := False;
    Exit;
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
  if not PrepareApplicationDirectory(ExpandConstant('{app}')) then
  begin
    Result := ExpandConstant('{cm:PrepareInstallFailed}');
    Exit;
  end;
end;

function InitializeUninstall: Boolean;
begin
  Result := EnsureQapptiaClosed(True);
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
  end;
end;
