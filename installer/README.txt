HOW TO MAKE A RELEASE INSTALLER

1. Navigate to the desired branch (typically called “version-x.x.x.x”)
2. Update the Dashboard_Instructions.pdf if needed/desired
3. Update windowsdesktop-runtime-x.x.x-win-x64.exe to the latest version
4. Update VC_redist.x64.exe to the latest version


5. Shut down Paratext
6. Search the solution for the 5 usages of the previous version number and update them
7. Update the version number in the DashboardInstaller.iss file
8. Change Visual Studio to Release mode


Steps 9-10 are necessary to rebuild the Paratext Plugin (since codesign_exe.bat can’t) and rebuild ClearDashboard.Wpf.Application for publishing.


9. Clean bin and obj files
10. Restore Nuget packages
11. Rebuild the Solution


12. (optional) Run the app to make sure it works.  Shut down Paratext if you open it.


13. Publish ClearDashboard.Wpf.Application
14. Try to compile the installer in the Inno compiler app to test if the script is working.  If it starts to compress files then everything is working.  Cancel the compiler.
15. Run the codesign_exe.bat file in ClearDashboard/installer.  If it doesn’t seem to be properly cleaning/rebuilding you may have an extra .sln file in ClearDashboard/src


16. Install locally to test out that it works


17. Upload the installer to google drive
18. Upload the installer to CLEAR_External_Releases/Files
19. Update pico-composer/content/index.md to point to the new installer in CLEAR_External_Releases/Files
20. Create the Version Notes json file.  Call it ClearDashboard.json and place it in CLEAR_External_Releases/VersionHistory