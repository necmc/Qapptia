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
CloseApplications=force
AppMutex=Global\Qapptia.capture,Global\Qapptia.editor,Global\Qapptia.config

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[CustomMessages]
english.AutoStartDescription=Launch automatically on system startup
spanish.AutoStartDescription=Iniciar automáticamente al encender el equipo
english.AdditionalOptions=Additional options:
spanish.AdditionalOptions=Opciones adicionales:
english.ExistingInstallTitle=Existing Installation Detected
spanish.ExistingInstallTitle=Instalación previa detectada
english.ExistingInstallDesc=An existing installation of Qapptia was found on this computer:
spanish.ExistingInstallDesc=Se detectó una instalación previa de Qapptia en este equipo:
english.ExistingInstallSubCaption=Select how you would like to proceed:
spanish.ExistingInstallSubCaption=Seleccione cómo desea continuar:
english.OptionReinstall=Reinstall or update Qapptia (Recommended)
spanish.OptionReinstall=Reinstalar o actualizar Qapptia (Recomendado)
english.OptionUninstall=Uninstall previous version of Qapptia
spanish.OptionUninstall=Desinstalar la versión anterior de Qapptia
english.ConfirmUninstall=Are you sure you want to completely uninstall Qapptia from this computer?
spanish.ConfirmUninstall=¿Está seguro de que desea desinstalar por completo Qapptia de este equipo?
english.UninstallSuccess=Qapptia was uninstalled successfully.
spanish.UninstallSuccess=Qapptia se desinstaló correctamente.

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
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Qapptia"; ValueData: """{app}\Qapptia.exe"" --capture"; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\Qapptia.exe"; Description: "{cm:LaunchProgram,Qapptia}"; Flags: nowait postinstall skipifsilent

[Code]
procedure ExitProcess(uExitCode: Integer);
  external 'ExitProcess@kernel32.dll stdcall';

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

var
  MaintenancePage: TInputOptionWizardPage;
  PreviousInstallPath: string;
  PreviousUninstallExe: string;
  HasPreviousInstall: Boolean;
  BroughtToFrontOnce: Boolean;

function CheckUninstallKey(RootKey: Integer; SubKey: string; var UninsExe: string; var InstPath: string): Boolean;
var
  RawUnins: string;
begin
  Result := False;
  if RegQueryStringValue(RootKey, SubKey, 'UninstallString', RawUnins) then
  begin
    RawUnins := RemoveQuotes(RawUnins);
    if (RawUnins <> '') and FileExists(RawUnins) then
    begin
      UninsExe := RawUnins;
      if not RegQueryStringValue(RootKey, SubKey, 'InstallLocation', InstPath) or (InstPath = '') then
        InstPath := ExtractFileDir(UninsExe);
      Result := True;
    end;
  end;
end;

const
  AppSubKeyStandard = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{' + '{#MyAppId}' + '}_is1';
  AppSubKeyLegacy   = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{' + '{#MyAppId}' + '}}_is1';

function TryCheckKeyInAllHives(SubKey: string; var UninsExe: string; var InstPath: string): Boolean;
begin
  Result := CheckUninstallKey(HKCU, SubKey, UninsExe, InstPath);
  if not Result then
    Result := CheckUninstallKey(HKLM, SubKey, UninsExe, InstPath);
  if not Result and IsWin64 then
    Result := CheckUninstallKey(HKLM64, SubKey, UninsExe, InstPath);
  if not Result then
    Result := CheckUninstallKey(HKLM32, SubKey, UninsExe, InstPath);
end;

function FindPreviousInstall(var UninsExe: string; var InstPath: string): Boolean;
begin
  Result := TryCheckKeyInAllHives(AppSubKeyStandard, UninsExe, InstPath);
  if not Result then
    Result := TryCheckKeyInAllHives(AppSubKeyLegacy, UninsExe, InstPath);
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if HasPreviousInstall and (PageID = wpSelectDir) then
    Result := True;
end;


procedure InitializeWizard;
begin
  HasPreviousInstall := FindPreviousInstall(PreviousUninstallExe, PreviousInstallPath);

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

procedure KillQapptiaProcesses;
var
  ResultCode: Integer;
  CurrentExeName: string;
  CurrentTmpName: string;
  CurrentUninsExe: string;
  CurrentUninsTmp: string;
  CurrentPid: DWORD;
  TaskkillArgs: string;
begin
  CurrentExeName := ExtractFileName(ExpandConstant('{srcexe}'));
  if CurrentExeName = '' then
    CurrentExeName := ExpandConstant('{param:CALLEREXE}');

  CurrentUninsExe := ExtractFileName(ExpandConstant('{uninstallexe}'));
  CurrentPid := GetCurrentProcessId;

  TaskkillArgs := '/F /T /FI "IMAGENAME eq Qapptia*"';

  if CurrentExeName <> '' then
  begin
    TaskkillArgs := TaskkillArgs + ' /FI "IMAGENAME ne ' + CurrentExeName + '"';
    CurrentTmpName := ChangeFileExt(CurrentExeName, '.tmp');
    if CurrentTmpName <> '' then
      TaskkillArgs := TaskkillArgs + ' /FI "IMAGENAME ne ' + CurrentTmpName + '"';
  end;

  if CurrentUninsExe <> '' then
  begin
    TaskkillArgs := TaskkillArgs + ' /FI "IMAGENAME ne ' + CurrentUninsExe + '"';
    CurrentUninsTmp := ChangeFileExt(CurrentUninsExe, '.tmp');
    if CurrentUninsTmp <> '' then
      TaskkillArgs := TaskkillArgs + ' /FI "IMAGENAME ne ' + CurrentUninsTmp + '"';
  end;

  if CurrentPid <> 0 then
    TaskkillArgs := TaskkillArgs + ' /FI "PID ne ' + IntToStr(CurrentPid) + '"';

  Exec('taskkill.exe', TaskkillArgs, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(250);
end;

procedure DeleteDesktopIcons;
var
  AppDir: string;
  UserDir: string;
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

  AppDir := ExpandConstant('{app}');
  if Pos('\AppData\Local\Programs', AppDir) > 0 then
  begin
    UserDir := Copy(AppDir, 1, Pos('\AppData\Local\Programs', AppDir) - 1);
    if UserDir <> '' then
    begin
      DeleteFile(UserDir + '\Desktop\Qapptia.lnk');
      DeleteFile(UserDir + '\OneDrive\Desktop\Qapptia.lnk');
    end;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;

  if HasPreviousInstall and (MaintenancePage <> nil) and (CurPageID = MaintenancePage.ID) then
  begin
    if MaintenancePage.Values[1] then
    begin
      if SuppressibleMsgBox(ExpandConstant('{cm:ConfirmUninstall}'), mbConfirmation, MB_YESNO, IDNO) = IDYES then
      begin
        WizardForm.Hide;
        try
          KillQapptiaProcesses;
          DeleteDesktopIcons;
          if Exec(PreviousUninstallExe, '/SILENT /NORESTART /SUPPRESSMSGBOXES /FORCECLOSEAPPLICATIONS /CALLEREXE="' + ExtractFileName(ExpandConstant('{srcexe}')) + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
          begin
            DeleteDesktopIcons;
            if (PreviousInstallPath <> '') and DirExists(PreviousInstallPath) then
            begin
              DelTree(PreviousInstallPath, True, True, True);
            end;
            SuppressibleMsgBox(ExpandConstant('{cm:UninstallSuccess}'), mbInformation, MB_OK, IDOK);
          end;
        finally
          ExitProcess(0);
        end;
      end;
      Result := False;
    end
    else
    begin
      KillQapptiaProcesses;
    end;
  end;
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

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    KillQapptiaProcesses;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppDir: string;
begin
  if CurUninstallStep = usUninstall then
  begin
    KillQapptiaProcesses;
    DeleteDesktopIcons;
  end
  else if CurUninstallStep = usPostUninstall then
  begin
    DeleteDesktopIcons;
    AppDir := ExpandConstant('{app}');
    if (AppDir <> '') and DirExists(AppDir) then
    begin
      if DirExists(AppDir + '\app') then
        DelTree(AppDir + '\app', True, True, True);
      if DirExists(AppDir + '\logs') then
        DelTree(AppDir + '\logs', True, True, True);
      DelTree(AppDir, True, True, True);
    end;
  end;
end;
