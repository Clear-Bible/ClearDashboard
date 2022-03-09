# How to localize the ClearDashboard

See the PDF "Microsoft How to Localize a WPF Application.pdf" in the Github repo under /docs for best practices during development

As you prepare the WPF application for being localized into other languages, there are several steps involved.  The first is making sure that text/button/labels/... have the `x:Uid=""` XAML filled out into a unique code.  We want to use the following format to keep things organized.  If you are creating controls on the `startpageview.xml` file, we want Uid's that reference not only the control but also the view.  This way we can better organize the localization files.

For instance, in the ShellView.xaml view, we have the TextBlock that has the text of "Paratext User".  For that control's Uid, we want the format to be the combination of the view's name and the controls name (e.g., `x:Uid="ShellView_ParatextUser"`)

```
    <TextBlock
        x:Uid="ShellView_ParatextUser"
        Margin="0,0,5,0"
        Foreground="{DynamicResource PrimaryHueMidBrush}"
        VerticalAlignment="Center"
        Text="Paratext User" />
```

Once the controls are set for a view, go add that UID key and the English text to this Google Sheet's document by copying a line, inserting it and then modifying the copied line being sure to put the control under the right section for that view:

https://docs.google.com/spreadsheets/d/1WESe3CRTlCUt1Aamq9jNs70KnjdFhllh/edit#gid=643886908

Put in the English wording for that control into column B.  Google Sheets will automatically send off the other cells to be translated into their respective languages.

## Getting Actual Translation Words for a Language

If sometime in the future a language is actually sent off for real translation, all we need to do is to paste into the appropriate cell the new wording.  This will replace the formula with the correct wording.

## Preparing a New ClearDashboard Release

Prior to creating the release, we will need to ensure that Uid's are present.  There is a batch file that uses the MSBuild tool to generate missing UIDs.  You can find it in the directory:
`ClearDashboard\src\generateUids.bat`  Run that tool and then check for any new Uids in your views that need to be localized.

