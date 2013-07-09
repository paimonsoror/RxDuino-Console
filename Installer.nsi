; RxDuino Console Installation Script

!define APPNAME "RxDuino Console"
!define COMPANYNAME "RxDuino"
!define DESCRIPTION "Console used to safely interact and update the RxDuino device from a Windows machine."

!define VERSIONMAJOR 1
!define VERSIONMINOR 2
!define VERSIONBUILD 4

!define INSTALLSIZE 8500

Name "The RxDuino Console Installer"
OutFile "setup.exe"

InstallDir "$PROGRAMFILES\${APPNAME}"

page license
page directory
page instfiles

section "install"
	# Files for the install directory - to build the installer, these should be in the same directory as the install script (this file)
	setOutPath $INSTDIR
	# Files added here should be removed by the uninstaller (see section "uninstall")
	file "RxDuinoConsole.application"
	file "RxDuinoConsole.exe"
	file "RxDuinoConsole.exe.config"
	file "RxDuinoConsole.exe.manifest"
	file "RxDuinoConsole.vshost.application"
	file "RxDuinoConsole.vshost.exe"
	file "RxDuinoConsole.vshost.exe.Config"
	file "RxDuinoConsole.vshost.exe.manifest"
	file "log4net.dll"
	file "RxDuinoCryptLib.exe"
	# Add any other files for the install directory (license files, app data, etc) here
	file "..\..\Resources\log4net.xml"
	#file "..\..\Resources\avrdude.conf"
	#file "..\..\Resources\avrdude.exe"
	#file "..\..\Resources\libusb0.dll"
 
	# Uninstaller - See function un.onInit and section "uninstall" for configuration
	writeUninstaller "$INSTDIR\uninstall.exe"
 
	# Start Menu
	createDirectory "$SMPROGRAMS\${COMPANYNAME}"
	createShortCut "$SMPROGRAMS\${COMPANYNAME}\${APPNAME}.lnk" "$INSTDIR\RxDuinoConsole.exe" "" ""
 
	# Registry information for add/remove programs
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "DisplayName" "${COMPANYNAME} - ${APPNAME} - ${DESCRIPTION}"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "QuietUninstallString" "$\"$INSTDIR\uninstall.exe$\" /S"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "InstallLocation" "$\"$INSTDIR$\""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "DisplayIcon" ""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "Publisher" "$\"${COMPANYNAME}$\""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "HelpLink" ""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "URLUpdateInfo" ""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "URLInfoAbout" ""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "DisplayVersion" "$\"${VERSIONMAJOR}.${VERSIONMINOR}.${VERSIONBUILD}$\""
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "VersionMajor" ${VERSIONMAJOR}
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "VersionMinor" ${VERSIONMINOR}
	# There is no option for modifying or repairing the install
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "NoModify" 1
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "NoRepair" 1
	# Set the INSTALLSIZE constant (!defined at the top of this script) so Add/Remove Programs can accurately report the size
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}" "EstimatedSize" ${INSTALLSIZE}
sectionEnd

# Uninstaller
 
function un.onInit
	SetShellVarContext all
 
	#Verify the uninstaller - last chance to back out
	MessageBox MB_OKCANCEL "Permanantly remove ${APPNAME}?" IDOK next
		Abort
	next:
functionEnd
 
section "uninstall"
 
	# Remove Start Menu launcher
	delete "$SMPROGRAMS\${COMPANYNAME}\${APPNAME}.lnk"
	# Try to remove the Start Menu folder - this will only happen if it is empty
	rmDir "$SMPROGRAMS\${COMPANYNAME}"
 
	# Remove files
	delete $INSTDIR\RxDuinoConsole.application
	delete $INSTDIR\RxDuinoConsole.exe
	delete $INSTDIR\RxDuinoConsole.exe.config
	delete $INSTDIR\RxDuinoConsole.exe.manifest
	delete $INSTDIR\RxDuinoConsole.vshost.application
	delete $INSTDIR\RxDuinoConsole.vshost.exe
	delete $INSTDIR\RxDuinoConsole.vshost.exe.Config
	delete $INSTDIR\RxDuinoConsole.vshost.exe.manifest
	delete $INSTDIR\RxDuinoCryptLib.exe
	delete $INSTDIR\log4net.dll
	delete $INSTDIR\log4net.xml
	delete $INSTDIR\avrdude.conf
	delete $INSTDIR\avrdude.exe
	delete $INSTDIR\libusb0.dll

	# Always delete uninstaller as the last action
	delete $INSTDIR\uninstall.exe
 
	# Try to remove the install directory - this will only happen if it is empty
	rmDir $INSTDIR
 
	# Remove uninstaller information from the registry
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANYNAME} ${APPNAME}"
sectionEnd

; eof
