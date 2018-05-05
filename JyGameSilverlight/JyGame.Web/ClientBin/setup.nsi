Name "金庸群侠传X"
OutFile "金庸群侠传X.exe"
#InstallDir "$PROGRAMFILES\JyGame"
InstallDir "D:\jygame"

XPStyle on

Section
    SetOutPath "$INSTDIR"
    SetOverwrite ifnewer
    File "Silverlight.exe"
    ExecWait "$INSTDIR\Silverlight.exe /q /doNotRequireDRMPrompt"
    File "JyGame_secure.xap"
    ExecWait '"$PROGRAMFILES\Microsoft Silverlight\sllauncher.exe" /install:"$INSTDIR\JyGame_secure.xap" /origin:"$INSTDIR\JyGame_secure.xap" /overwrite /shortcut:desktop+startmenu'
SectionEnd

Section
	SetOutPath "$INSTDIR\Resource"
	SetOverwrite ifnewer
	File /r "Resource\*"
SectionEnd