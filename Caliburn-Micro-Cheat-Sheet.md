# Caliburn.Micro Cheat Sheet

<!--
---
layout: page
title: Cheat Sheet
---
-->

This serves as a quick guide to the most frequently used conventions and features in the Caliburn.Micro project. 

### Wiring Events
This is automatically wiring events on controls to call methods on the ViewModel.

#### Convention

``` xml
<Button x:Name="Save">
```
This will cause the Click event of the Button to call "Save" method on the ViewModel. 

#### Short Syntax

``` xml
<Button cal:Message.Attach="Save">
```

This will again cause the "Click" event of the Button to call "Save" method on the ViewModel. 

Different events can be used like this: 

``` xml
<Button cal:Message.Attach="[Event MouseEnter] = [Action Save]">
```

Different parameters can be passed to the method like this:

``` xml
<Button cal:Message.Attach="[Event MouseEnter] = [Action Save($this)]"> 
``` 

<dl>
	<dt>$eventArgs</dt>
	<dd>Passes the EventArgs or input parameter to your Action. Note: This will be null for guard methods since the trigger hasn’t actually occurred.</dd>
	<dt>$dataContext</dt>
	<dd>Passes the DataContext of the element that the ActionMessage is attached to. This is very useful in Master/Detail scenarios where the ActionMessage may bubble to a parent VM but needs to carry with it the child instance to be acted upon.</dd>
	<dt>$source</dt>
	<dd>The actual FrameworkElement that triggered the ActionMessage to be sent.</dd>
	<dt>$view</dt>
	<dd>The view (usually a UserControl or Window) that is bound to the ViewModel.</dd>
	<dt>$executionContext</dt>
	<dd>The action's execution context, which contains all the above information and more. This is useful in advanced scenarios.</dd>
	<dt>$this</dt>
	<dd>The actual UI element to which the action is attached. In this case, the element itself won't be passed as a parameter, but rather its default property.</dd>
</dl>


#### Long Syntax

``` xml
<UserControl x:Class="Caliburn.Micro.CheatSheet.ShellView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" 
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
    xmlns:i="clr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity" 
    xmlns:cal="http://www.caliburnproject.org"> 
    <StackPanel> 
        <TextBox x:Name="Name" />
        <Button Content="Save"> 
            <i:Interaction.Triggers> 
                <i:EventTrigger EventName="Click"> 
                    <cal:ActionMessage MethodName="Save"> 
                       <cal:Parameter Value="{Binding ElementName=Name, Path=Text}" /> 
                    </cal:ActionMessage> 
                </i:EventTrigger> 
            </i:Interaction.Triggers> 
        </Button> 
    </StackPanel> 
</UserControl>
```

This syntax is Expression Blend friendly. 

### Databinding

This is automatically binding dependency properties on controls to properties on the ViewModel.

#### Convention

``` xml
<TextBox x:Name="FirstName" />
```

Will cause the "Text" property of the TextBox to be bound to the "FirstName" property on the ViewModel. 

#### Explicit

``` xml
<TextBox Text="{Binding Path=FirstName, Mode=TwoWay}" />
```

This is the normal way of binding properties.

#### Event Aggregator

The three different methods on the Event Aggregator are:

``` csharp
public interface IEventAggregator {  
    void Subscribe(object instance);  
    void Unsubscribe(object instance);  
    void Publish(object message, Action<System.Action> marshal);  
}
```

An event can be a simple class such as:

``` csharp
public class MyEvent {
    public MyEvent(string myData) {
        this.MyData = myData;
    }

    public string MyData { get; private set; }
}
```

## Using Microsoft.Extensions.DependencyInjection with Caliburn.Micro

1. Add a reference to the `Microsoft.Extensions.DependencyInjection` nuget package
2. In Bootstrapper.cs, add the following property:

``` csharp
  public static IHost Host { get; private set; }
```
3. In the constructor, add the following code:

``` csharp
public Bootstrapper()
{
    Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            ConfigureServices(services);
        })
        .Build();
        ... code elided
}
```

4. Add a method named `ConfigureServices` and set up your DI wire up as appropriate for your project.
``` csharp
 protected  void ConfigureServices(IServiceCollection serviceCollection)
        {
            FrameSet = serviceCollection.AddCaliburnMicro();
           
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
        }
```

5. Override the appropriate Caliburn.Micro dependency injection methods:

``` csharp
protected override object GetInstance(Type service, string key)
{
    return Host.Services.GetService(service);
}

protected override IEnumerable<object> GetAllInstances(Type service)
{
    return Host.Services.GetServices(service);
}
```

## Navigation Configuration 

 ### WPF Configuration

 1. Add a class named `Frameset`
 ``` csharp
  public class FrameSet
  {
      public FrameSet()
      {
          Frame = new Frame
          {
              NavigationUIVisibility = NavigationUIVisibility.Hidden
          };
          FrameAdapter = new FrameAdapter(Frame);
      }

      public Frame Frame { get; private set; }
      public FrameAdapter FrameAdapter { get; private set; }
      public INavigationService NavigationService => FrameAdapter as INavigationService;
  }
```
 2. Add an extension method which creates a Frame and sets up the Caliburn.Micro infratructure.
``` csharp
  public static FrameSet AddCaliburnMicro(this IServiceCollection serviceCollection)
  {
      var frameSet = new FrameSet();
      // wire up the interfaces required by Caliburn.Micro
      serviceCollection.AddSingleton<IWindowManager, WindowManager>();
      serviceCollection.AddSingleton<IEventAggregator, EventAggregator>();

      // Register the FrameAdapter which wraps a Frame as INavigationService
      serviceCollection.AddSingleton<INavigationService>(sp => frameSet.NavigationService);

      // wire up all of the view models in the project.
      typeof(Bootstrapper).Assembly.GetTypes()
          .Where(type => type.IsClass)
          .Where(type => type.Name.EndsWith("ViewModel"))
          .ToList()
          .ForEach(viewModelType => serviceCollection.AddTransient(viewModelType));

      return frameSet;
  }
 ```

   3. In the Bootstrapper class store an instance of FrameSet in the ConfigureServices method:
 ``` csharp
    protected  void ConfigureServices(IServiceCollection serviceCollection)
    {
        FrameSet = serviceCollection.AddCaliburnMicro();
        serviceCollection.AddClearDashboardDataAccessLayer();
        serviceCollection.AddLogging();
        serviceCollection.AddLocalization();
    }
 ```

   4. Add a method to add the Frame to the visual tree:

 ``` csharp
    private void AddFrameToMainWindow(Frame frame)
    {
        Logger.LogInformation("Adding Frame to ShellView grid control.");

        var mainWindow = Application.MainWindow;
        if (mainWindow == null)
        {
            throw new NullReferenceException("'Application.MainWindow' is null.");
        }


        if (mainWindow.Content is not Grid grid)
        {
            throw new NullReferenceException("The grid on 'Application.MainWindow' is null.");
        }

        Grid.SetRow(frame, 1);
        Grid.SetColumn(frame, 0);
        grid.Children.Add(frame);
    }
   ``` 

   5. Then override OnStartup and call the method to add the Frame to the visual tree:

   ``` csharp
    protected override async void OnStartup(object sender, StartupEventArgs e)
    {
        // Allow the ShellView to be created.
        await DisplayRootViewForAsync<ShellViewModel>();

        // Now add the Frame to be added to the Grid in ShellView
        AddFrameToMainWindow(FrameSet.Frame);

        ... code elided
    }
  ```

  ## Navigating to Viewmodels

  1. Inject INavigationService into the contructor of your view model:
  ``` csharp
    private readonly INavigationService _navigationService
    public YourViewModel(INavigationService navigationService)
    {
       _navigationService = navigationService;
    }
  ```
  2. To navigate to a different viewmodel/view:
  ``` csharp
   _navigationService.NavigateToViewModel<AnotherViewModel>();
             
  ```
  3. To pass data to a view model...  define properties on the target view model, i.e. 

  ``` csharp
  public int ProductId { get ;set;}
  public int CategoryId { get; set;}
  ```
  ``` csharp
  navigationService.For<ProductViewModel>()
    .WithParam(v => v.ProductId, 42)
    .WithParam(v => v.CategoryId, 2)
    .Navigate();
  ```
  * This also works with more complex objects (in WPF at least)
  ``` csharp
    _navigationService.For<AnotherViewModel>()
        .WithParam(v => v.ComplexObject, new ComplexObject()).Navigate();
  ```
  * And finally multiple parameters may be passed by chaining WithParam:
  ``` csharp
   _navigationService.For<AnotherViewModel>()
        .WithParam(v => v.ComplexObject, new ComplexObject())
        .WithParam(v => v.SomeString, _theString)
        .WithParam(v => v.ComplexObject2, new ComplexObject2()).Navigate();
  ```
