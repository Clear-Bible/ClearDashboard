HOW TO MAKE A RELEASE INSTALLER

1. Navigate to the desired branch (typically called “version-x.x.x.x”)
2. Update the Dashboard_Instructions.pdf if needed/desired
3. Update windowsdesktop-runtime-x.x.x-win-x64.exe to the latest version
4. Update VC_redist.x64.exe to the latest version


5. Shut down Paratext
8. SetVersionInfo tool
7. (optional) Update the version number in manifest.app
8. (optional) Update the version number in the DashboardInstaller.iss file
9. (optional) Change Visual Studio to Release mode

10. (optional) Clean bin and obj files
11. (optional) Restore Nuget packages
12. (optional) Rebuild the Solution


13. (optional) Run the app to make sure it works.  Shut down Paratext if you open it.


14. (optional) Publish ClearDashboard.Wpf.Application
15. (optional) Publish PluginManager
15. (optional) Publish ResetCurrentUser
16. (optional) Try to compile the installer in the Inno compiler app to test if the script is working.  If it starts to compress files then everything is working.  Cancel the compiler.
17. Run the codesign_exe.bat file in ClearDashboard/installer.  If it doesn’t seem to be properly cleaning/rebuilding you may have an extra .sln file in ClearDashboard/src


18. Install locally to test out that it works


19. Upload the installer to google drive
20. Upload the installer to CLEAR_External_Releases/Files
21. Update pico-composer/content/index.md to point to the new installer in CLEAR_External_Releases/Files
22. Create the Version Notes json file.  Call it ClearDashboard.json and place it in CLEAR_External_Releases/VersionHistory