# Paratext Installation Guide

This serves as a guide to installing and configuring Paratext for use as a developer.

## Instructions

1. Obtain a Paratext license key by either pinging Dirk or Mike B.

2. Download Paratext from here: https://paratext.org/download/

3. Run the installer and put in the registration information into the appropriate places.

4. Follow the advice on this page to setup your Visual Studio environment https://github.com/ubsicap/paratext_demo_plugins/wiki/Debugging-a-Plugin so that you can debug the plugins.

5. Clone the Github repository for ClearDashboard from here: https://github.com/Clear-Bible/ClearDashboard

6. After you installed Paratext, it has created a new folder on your computer that follows the pattern `{drive}:\My Paratext 9 Projects\`.  Extract the two zip files that can be found under the `ClearDashboard\example_paratext_data` folder into the root of your Paratext data directory.  You should then have subfolders for the standard translation project `zz_SUR` and it's back translation `zz_SURBT`.  These are project files that can be loaded up into Paratext.

7. Load up ClearDashboard into Visual Studio.  As long as the environmental paths are correct for your machine as outlined in step 4, when you do a Rebuild, it will copy into the Paratext install path the ClearDashboard Plugin under a new directory called `plugins`.

8. After you load a Paratext project up (e.g., zz_SUR), there will be a new menu item on the far right under Tools called ClearDashboard.  Clicking it should open up a new Paratext window with the plugin running and starting up it's named pipes instance.