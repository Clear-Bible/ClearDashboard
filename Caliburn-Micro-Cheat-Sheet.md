# Caliburn.Micro Cheat Sheet

## Using Microsoft.Extensions.DependencyInjection with Caliburn.Micro

1. Add a reference to the `Microsoft.Extensions.DependencyInjection` nuget package
2. In Bootstrapper.cs, add the following property:

```
  public static IHost Host { get; private set; }
```
3. In the constructor, add the following code:

```
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
```
 protected  void ConfigureServices(IServiceCollection serviceCollection)
        {
            FrameSet = serviceCollection.AddCaliburnMicro();
           
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
        }
```

5. Override the appropriate Caliburn.Micro dependency injection methods:

```
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
 ```
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
```
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
 ```
    protected  void ConfigureServices(IServiceCollection serviceCollection)
    {
        FrameSet = serviceCollection.AddCaliburnMicro();
        serviceCollection.AddClearDashboardDataAccessLayer();
        serviceCollection.AddLogging();
        serviceCollection.AddLocalization();
    }
 ```

   4. Add a method to add the Frame to the visual tree:

 ```
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

   ```
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
  ```
    private readonly INavigationService _navigationService
    public YourViewModel(INavigationService navigationService)
    {
       _navigationService = navigationService;
    }
  ```
  2. To navigate to a different viewmodel/view:
  ```
   _navigationService.NavigateToViewModel<AnotherViewModel>();
             
  ```
  3. To pass data to a view model...  define properties on the target view model, i.e. 

  ```
  public int ProductId { get ;set;}
  public int CategoryId { get; set;}
  ```
  ```
  navigationService.For<ProductViewModel>()
    .WithParam(v => v.ProductId, 42)
    .WithParam(v => v.CategoryId, 2)
    .Navigate();
  ```
  * This also works with more complex objects (in WPF at least)
  ```
    _navigationService.For<AnotherViewModel>()
        .WithParam(v => v.ComplexObject, new ComplexObject()).Navigate();
  ```
