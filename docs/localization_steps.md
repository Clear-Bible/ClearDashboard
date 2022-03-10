# How to localize the ClearDashboard

As you prepare the WPF application for being localized into other languages, there are several steps involved.  The first is making sure that text/button/labels/... have unique keys made for them.  Once the controls are set for a view, go add the unique keys and the English text to this Google Sheet's document by copying a line, inserting it and then modifying the copied line being sure to put the control under the right section for that view:

https://docs.google.com/spreadsheets/d/1WESe3CRTlCUt1Aamq9jNs70KnjdFhllh/edit#gid=643886908

Put in the English wording for that control into column B.  Google Sheets will automatically send off the other cells to be translated into their respective languages.

## How to create a KEY

We want to use the following format to keep things organized.  If you are creating controls on the `startpageview.xml` file, we want key's that reference not only the control but also the view.  This way we can better organize the localization files.

For instance, in the ShellView.xaml view, we have the TextBlock that has the text of "Paratext User".  For that control's key, we want the format to be the combination of the view's name and the controls name (e.g., `ShellView_user`)

## Getting Actual Translation Words for a Language

If sometime in the future a language is actually sent off for real translation, all we need to do is to paste into the appropriate cell the new wording.  This will replace the formula with the correct wording.

## Using the GenerateTranslationsForDashboard program to update the .resx files




## Using a translated KEY in the Dashboard code

Add the following lines to each XAML header:

```
    xmlns:helpers="clr-namespace:ClearDashboard.Wpf.Helpers"
    xmlns:resx="clr-namespace:ClearDashboard.Wpf.Strings"
    helpers:Translation.ResourceManager="{x:Static resx:Resources.ResourceManager}"
```

For each text element to translate, you pass in the translated text in the following binding manner: `{helpers:Loc Landing_projects}`

Example:
```
    <TextBlock
        x:Uid="TextBlock_1"
        Margin="15,5,5,0"
        FontSize="20"
        Foreground="{StaticResource PrimaryHueDarkBrush}">
        <Run Text="Dashboard " />
        <Run Text="{helpers:Loc Landing_projects}" />
    </TextBlock>
```
