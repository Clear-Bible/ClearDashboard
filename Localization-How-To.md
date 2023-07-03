GenerateTranslationsForDashboard

# How to localize the ClearDashboard

As you prepare the WPF application for being localized into other languages, there are several steps involved.  The first is making sure that text/button/labels/... have unique keys made for them.  Once the controls are set for a view, go add the unique keys and the English text to this Google Sheet's document by copying a line, inserting it and then modifying the copied line being sure to put the control under the right section for that view:

https://docs.google.com/spreadsheets/d/1WESe3CRTlCUt1Aamq9jNs70KnjdFhllh/edit#gid=643886908

Put in the English wording for that control into column B.  Google Sheets will automatically send off the other cells to be translated into their respective languages.

## How to create a KEY

We want to use the following format to keep things organized.  If you are creating controls on the `startpageview.xml` file, we want key's that reference not only the control but also the view.  This way we can better organize the localization files.

For instance, in the ShellView.xaml view, we have the TextBlock that has the text of "Paratext User".  For that control's key, we want the format to be the combination of the view's name and the controls name (e.g., `ShellView_User`)

## Getting Actual Translation Words for a Language

If sometime in the future a language is actually sent off for real translation, all we need to do is to paste into the appropriate cell the new wording.  This will replace the formula with the correct wording.

## Using the GenerateTranslationsForDashboard program to update the .resx files

The latest version of the program can be downloaded from here: https://github.com/Clear-Bible/GenerateTranslationsForDashboard/releases

It should be pretty obvious as to how the program operates. From the Google Sheets spreadsheet, export the data as a *.tsv file.  With the top button of the program, use it to select the downloaded TSV file.  Then use the second button to point to your `ClearDashboard.Wpf.Strings`
directory.  Click on the "Generate Data from TSV" and the program will iterate through all the TSV data and validating the keys.   If there are any dupblicate keys found, you'll need to make note of them 
and first return back to the Google Sheets file and correct any dups.  

If there are no duplicates, click on the Populate Resources button which will iterate through the dataset and update the various *.resx files with the new key/value pairs.


## Using a translated KEY in the Dashboard code

Add the following lines to each XAML header:

```
    xmlns:helpers="clr-namespace:ClearDashboard.Wpf.Application.Helpers;assembly=ClearDashboard.Wpf.Application.Abstractions"
    xmlns:strings="clr-namespace:ClearDashboard.Wpf.Application.Strings"
    helpers:Translation.ResourceManager="{x:Static strings:Resources.ResourceManager}"
```

For each text element to translate, you pass in the translated text in the following binding manner: `{helpers:Localization Landing_Projects}`

Example:
```
    <TextBlock
        x:Uid="TextBlock_1"
        Margin="15,5,5,0"
        FontSize="20"
        Foreground="{StaticResource PrimaryHueDarkBrush}">
        <Run Text="Dashboard " />
        <Run Text="{helpers:Localization Landing_Projects}" />
    </TextBlock>
```

## To get the localized string from your code, call the following function
```
    var localizedString = _localizationService["NewCollabUserView_SavedToRemoteServer"];
```