using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;



namespace ClearDashboard.Wpf.ViewModels
{
    public class CreateNewProjectsViewModel : ApplicationScreen
    {
        #region Properties
        private ProjectManager ProjectManager { get; set; }

        protected Canvas DrawCanvasTop { get; set; }
        protected Canvas DrawCanvasBottom { get; set; }

        public bool ParatextVisible = false;
        public bool ShowWaitingIcon = true;

        private bool _isDragging = false;
        private Point _dragStartPoint;

        private double _boxWidth;
        private double _boxHeight;

        private ParatextProject _targetProject;
        private readonly List<ParatextProject> _lwcProjects = new List<ParatextProject>();
        private readonly List<ParatextProject> _backTranslationProjects = new List<ParatextProject>();
        private ParatextProject _interlinearizerProject;

        // variables that hold the arrow connection points
        private Point _sourceConnectionPoint = new Point(0, 0);
        private readonly List<Point> _lwcConnectionPointsLeft = new List<Point>();
        private Point _targetConnectionPointLeft = new Point(0, 0);
        private Point _targetConnectionPointRight = new Point(0, 0);
        private Point _targetConnectionPtBottom = new Point(0, 0);
        private Point _inConnectionPointLeft = new Point(0, 0);

        private enum DropZones
        {
            Source,
            LWC,
            Target,
            BackTranslation,
            Interlinearizer,
            Blank
        }

        private string _helpText;
        public string HelpText
        {
            get => _helpText;
            set
            {
                _helpText = value;
                NotifyOfPropertyChange(() => HelpText);
            }
        }


        private bool _canCreateNewProject;
        public bool CanCreateNewProject
        {
            get => _canCreateNewProject;
            set
            {
                _canCreateNewProject = value;
                NotifyOfPropertyChange(() => CanCreateNewProject);
            }
        }



        

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextProjects { get; set; } =
            new ObservableRangeCollection<ParatextProjectViewModel>();

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextResources { get; set; } =
            new ObservableRangeCollection<ParatextProjectViewModel>();


        private Task _textXamlChangeEvent;
        private string _textXaml;
        public string TextXaml
        {
            get => _textXaml;
            set
            {
                if (_textXaml == value) return;
                _textXaml = value;
                if (_textXamlChangeEvent == null || _textXamlChangeEvent.Status >= TaskStatus.RanToCompletion)
                {
                    _textXamlChangeEvent = Task.Run(() =>
                    {
                        Task.Delay(100);
                        retry:
                        var oldVal = _textXaml;

                        Thread.MemoryBarrier();
                        _textXaml = value;
                        NotifyOfPropertyChange(() => TextXaml);

                        Thread.MemoryBarrier();
                        if (oldVal != _textXaml) goto retry;
                    });
                }
            }
        }

        #endregion

        #region Observable Properties

        private string _projectName;

        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                NotifyOfPropertyChange(() => ProjectName);
                ValidateProjectData();
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Required for design-time support
        /// </summary>
        public CreateNewProjectsViewModel()
        {
            
        }

        public CreateNewProjectsViewModel(INavigationService navigationService, ILogger<CreateNewProjectsViewModel> logger, ProjectManager projectManager) : base(navigationService, logger)
        {
            ProjectManager = projectManager;
        }

        #endregion

        #region Startup

       
        protected override void OnViewAttached(object view, object context)
        {
            var createNewProjectView = ((CreateNewProjectsView)view);
            DrawCanvasTop = (Canvas)createNewProjectView.FindName("DrawCanvasTop");
            DrawCanvasBottom = (Canvas)createNewProjectView.FindName("DrawCanvasBottom");
            base.OnViewAttached(view, context);
        }
        protected override async void OnViewLoaded(object view)
        {
            await Init();
            DrawTheCanvas();

            base.OnViewLoaded(view);
        }

        public async Task Init()
        {
            SetupUserHelp();
            await SetupParatext();
        }

        private async Task SetupParatext()
        {
            // detect if Paratext is installed
            var paratextUtils = new ParatextUtils();
            ParatextVisible = paratextUtils.IsParatextInstalled();

            if (ParatextVisible)
            {
                // get all the Paratext Projects (Projects/Backtranslations)
                ParatextProjects.Clear();
                var projects = await paratextUtils.GetParatextProjectsOrResources(ParatextUtils.eFolderType.Projects);
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectViewModel>();
                    foreach (var project in projects)
                    {
                        ParatextProjects.Add(TinyMapper.Map<ParatextProjectViewModel>(project));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unexpected error while initializing");
                }

                // get all the Paratext Resources (LWC)
                ParatextResources.Clear();
                var resources = paratextUtils.GetParatextResources();
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectViewModel>();
                    foreach (var resource in resources)
                    {
                        ParatextResources.Add(TinyMapper.Map<ParatextProjectViewModel>(resource));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unexpected error while initializing");
                }
            }
        }

        private void SetupUserHelp()
        {
            // get the right help text
            // TODO Work on the help regionalization
            var fileInfo = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var helpFile = System.IO.Path.Combine(fileInfo.Directory.ToString(), @"HelpFiles\NewProjectHelp_us.md");

            if (File.Exists(helpFile))
            {
                var markdownTxt = File.ReadAllText(helpFile);
                HelpText = string.Join("\r\n", Regex.Split(markdownTxt, "\r?\n").Select(ln => ln.TrimStart()));
            }
        }

        #endregion

        #region Page management


       

        public void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        public void ListView_PreviewMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var listView = (ListView)sender;

            if (listView.SelectedItem == null) return;

            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                var position = mouseEventArgs.GetPosition(null);

                if (Math.Abs(position.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    var data = new DataObject(typeof(ParatextProject), listView.SelectedItem);
                    var dragDropEffects = DragDrop.DoDragDrop(listView, data, DragDropEffects.Copy);
                    _isDragging = false;
                }
            }
        }

        public void Canvas_PreviewDropTop(object sender, DragEventArgs dragEventArgs)
        {
            var data = dragEventArgs.Data;
            var position = dragEventArgs.GetPosition(DrawCanvasTop);
            var dropZone = GetDropZone(position);

            if (data.GetDataPresent(typeof(ParatextProject)))
            {
                var project = data.GetData(typeof(ParatextProject)) as ParatextProject;
                var text = new TextBlock();

                switch (dropZone)
                {
                    case DropZones.Source:
                        text.Text = "MANUSCRIPT";
                        break;
                    case DropZones.LWC:
                        text.Text = "LWC";
                        if (!_lwcProjects.Any(p=>project != null && p.Name == project.Name))
                        {
                            // only allow it to be added once
                            _lwcProjects.Add(project);
                        }
                        break;
                    case DropZones.Target:
                        text.Text = "TARGET";
                        if (project.ProjectType == ParatextProject.eProjectType.Standard)
                        {
                            _targetProject = project;

                            // look for linked back translations
                            var backTranslationProjects = ParatextProjects.Where(p => p.TranslationInfo?.projectGuid == project.Guid).ToList();
                            foreach (var backTranslationProject in backTranslationProjects)
                            {
                                if (!_backTranslationProjects.Any(p => p.Name == backTranslationProject.Name))
                                {
                                    _backTranslationProjects.Add(backTranslationProject);
                                }
                            }
                        }
                        break;
                }
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                Canvas.SetTop(text, position.Y);
                Canvas.SetLeft(text, position.X);
                DrawCanvasTop.Children.Add(text);

                DrawTheCanvas();
            }
        }

        public void Canvas_PreviewDropBottom(object sender, DragEventArgs dragEventArgs)
        {
            var data = dragEventArgs.Data;
            var position = dragEventArgs.GetPosition(DrawCanvasBottom);

            var dropZone = GetDropZoneBottom(position);

            if (data.GetDataPresent(typeof(ParatextProject)))
            {
                var project = data.GetData(typeof(ParatextProject)) as ParatextProject;
                var text = new TextBlock();

                switch (dropZone)
                {
                    case DropZones.BackTranslation:
                        text.Text = "BACK TRANSLATION";
                        // look for linked back translations
                        if (!_backTranslationProjects.Any(p => project != null && p.Name == project.Name))
                        {
                            _backTranslationProjects.Add(project);
                        }
                        break;
                    case DropZones.Blank:
                        break;
                    case DropZones.Interlinearizer:
                        text.Text = "INTERLINEARIZER";
                        if (project != null && project.ProjectType == ParatextProject.eProjectType.Resource)
                        {
                            _interlinearizerProject = project;
                        }
                        break;
                }
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                Canvas.SetTop(text, position.Y);
                Canvas.SetLeft(text, position.X);
                DrawCanvasBottom.Children.Add(text);
                DrawTheCanvas();
            }
        }
        private DropZones GetDropZone(Point point)
        {
            if (point.X < _boxWidth)
            {
                return DropZones.Source;
            }
            if (point.X > _boxWidth && point.X < _boxWidth * 2)
            {
                return DropZones.Target;
            }
            return DropZones.LWC;
        }

        private DropZones GetDropZoneBottom(Point point)
        {
            if (point.X < _boxWidth)
            {
                return DropZones.BackTranslation;
            }
            if (point.X > _boxWidth && point.X < _boxWidth * 2)
            {
                return DropZones.Blank;
            }
            return DropZones.Interlinearizer;
        }

        private void DrawTheCanvas()
        {
            _boxWidth = DrawCanvasTop.ActualWidth / 3; //width of each box
            _boxHeight = DrawCanvasTop.ActualHeight;  // Height of each box

            var projectBoxWidth = (int)(_boxWidth * 0.33);
            const int projectBoxHeight = 40;

            DrawCanvasTop.Children.Clear();
            DrawCanvasBottom.Children.Clear();

            // ===========================================
            // Draw the Box Outlines and header label
            // ===========================================
            DrawAllTheBoxes();


            // ===============================
            // Draw the Source Box Contents
            // ===============================
            DrawSourceBox(projectBoxWidth, projectBoxHeight);

            // ===============================
            // Draw the Target Box Contents
            // ===============================
            if (_targetProject != null)
            {
                DrawTargetBox(projectBoxWidth, projectBoxHeight);
            }

            // ===============================
            // Draw the LWC Box(es) Contents
            // ===============================
            if (_lwcProjects.Count > 0)
            {
                DrawLWCBoxes(projectBoxWidth, projectBoxHeight);
            }

            // =============================================
            // Draw the Back Translation Box(es) Contents
            // =============================================
            if (_backTranslationProjects.Count > 0)
            {
                DrawBackTransBoxes(projectBoxWidth, projectBoxHeight);
            }

            // =============================================
            // Draw the Interlinearizer Contents
            // =============================================
            if (_interlinearizerProject != null)
            {
                DrawInterlinearizerBox(projectBoxWidth, projectBoxHeight);
            }

            // ===============================
            // Draw the ConnectionLines
            // ===============================
            // connect target to source
            if (_targetProject != null)
            {
                var leftPoint = new Point(_sourceConnectionPoint.X + 6, _sourceConnectionPoint.Y + 2);
                var rightPoint = new Point(_targetConnectionPointLeft.X - 6, _targetConnectionPointLeft.Y + 2);
                var controlPointSource = new Point(_sourceConnectionPoint.X + 20, _sourceConnectionPoint.Y);
                var controlPointTarget = new Point(_targetConnectionPointLeft.X - 20, _targetConnectionPointLeft.Y);
                var path = GenerateLine(leftPoint, rightPoint, controlPointSource, controlPointTarget);
                DrawCanvasTop.Children.Add(path);
            }

            // some LWC's so connect to Target
            for (var i = 0; i < _lwcProjects.Count; i++)
            {
                if (_targetProject != null)
                {
                    var leftPoint = new Point(_lwcConnectionPointsLeft[i].X + 6, _lwcConnectionPointsLeft[i].Y + 2);
                    var rightPoint = new Point(_targetConnectionPointRight.X + 6, _targetConnectionPointRight.Y + 2);
                    // draw from right LWC to Target
                    var controlPointSource = new Point(_lwcConnectionPointsLeft[i].X - 20, _lwcConnectionPointsLeft[i].Y);
                    var controlPointTarget = new Point(_targetConnectionPointRight.X + 20, _targetConnectionPointRight.Y);
                    var path = GenerateLine(leftPoint, rightPoint, controlPointSource, controlPointTarget);
                    DrawCanvasTop.Children.Add(path);
                }
            }


            // connection lines between target and Intelinear
            if (_targetProject != null && _interlinearizerProject != null)
            {
                var bottomPoint = new Point(_targetConnectionPtBottom.X, _targetConnectionPtBottom.Y + 6);
                var leftPoint = new Point(_inConnectionPointLeft.X - 6, _inConnectionPointLeft.Y + 10 + DrawCanvasTop.ActualHeight);
                var controlPtSource = new Point(_targetConnectionPtBottom.X, _targetConnectionPtBottom.Y + (DrawCanvasTop.ActualHeight / 3));
                var controlPtTarget = new Point(_inConnectionPointLeft.X - (DrawCanvasTop.ActualHeight / 3), _inConnectionPointLeft.Y + DrawCanvasTop.ActualHeight);
                var path = GenerateLine(bottomPoint, leftPoint, controlPtSource, controlPtTarget);
                DrawCanvasTop.Children.Add(path);
            }

        }

        private void DrawAllTheBoxes()
        {
            const int cornerRadius = 8;

            // =======================================================
            // BUILD THE HEADERS AND DROP BOXES - LOWER CANVAS
            // =======================================================
            for (var i = 0; i < 3; i++)
            {
                var textBlock = new TextBlock
                {
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = _boxWidth
                };

                var rectangle = new Rectangle
                {
                    Height = DrawCanvasBottom.ActualHeight,
                    Width = _boxWidth - 2,
                    RadiusX = cornerRadius,
                    RadiusY = cornerRadius,
                    StrokeThickness = 3,
                    Fill = Brushes.Transparent,
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 2,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    }
                };

                var point = new Point
                {
                    Y = 0
                };
                switch (i)
                {
                    case 0:
                        rectangle.Stroke = Application.Current.FindResource("PurpleMidBrush") as Brush;
                        textBlock.Foreground = Application.Current.FindResource("PurpleMidBrush") as Brush;
                        point.X = 0;
                        textBlock.Text = "Back Translation(s)";

                        Canvas.SetLeft(rectangle, point.X);
                        Canvas.SetTop(rectangle, point.Y);
                        DrawCanvasBottom.Children.Add(rectangle);

                        // center the text in the block
                        var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
                        var additionalX = (_boxWidth - textSize.Width) / 2;
                        Canvas.SetLeft(textBlock, point.X + additionalX);
                        Canvas.SetTop(textBlock, 10);
                        DrawCanvasBottom.Children.Add(textBlock);
                        break;
                    case 1:
                        break;
                    case 2:
                        rectangle.Stroke = Application.Current.FindResource("OrangeDarkBrush") as Brush;
                        textBlock.Foreground = Application.Current.FindResource("OrangeDarkBrush") as Brush;
                        point.X = _boxWidth * 2;
                        textBlock.Text = "Interlinearizer";

                        Canvas.SetLeft(rectangle, point.X);
                        Canvas.SetTop(rectangle, point.Y);
                        DrawCanvasBottom.Children.Add(rectangle);

                        // center the text in the block
                        textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
                        additionalX = (_boxWidth - textSize.Width) / 2;
                        Canvas.SetLeft(textBlock, point.X + additionalX);
                        Canvas.SetTop(textBlock, 10);
                        DrawCanvasBottom.Children.Add(textBlock);
                        break;
                }


            }

            // =======================================================
            // BUILD THE HEADERS AND DROP BOXES - UPPER CANVAS
            // =======================================================
            for (int i = 0; i < 3; i++)
            {
                var textBlock = new TextBlock
                {
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = _boxWidth
                };

                var rectangle = new Rectangle
                {
                    Height = DrawCanvasTop.ActualHeight,
                    Width = _boxWidth - 2,
                    RadiusX = cornerRadius,
                    RadiusY = cornerRadius,
                    StrokeThickness = 3,
                    Fill = Brushes.Transparent,
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 2,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    }
                };

                var point = new Point
                {
                    Y = 0
                };
                switch (i)
                {
                    case 0:
                        rectangle.Stroke = Application.Current.FindResource("TealVeryDarkBrush") as Brush;
                        textBlock.Foreground = Application.Current.FindResource("TealVeryDarkBrush") as Brush;
                        point.X = 0;
                        textBlock.Text = "Manuscript";
                        break;
                    case 1:
                        rectangle.Stroke = Application.Current.FindResource("BlueDarkBrush") as Brush;
                        textBlock.Foreground = Application.Current.FindResource("BlueDarkBrush") as Brush;
                        point.X = _boxWidth;
                        textBlock.Text = "Target";
                        break;
                    case 2:
                        rectangle.Stroke = Application.Current.FindResource("OrangeDarkBrush") as Brush;
                        textBlock.Foreground = Application.Current.FindResource("OrangeDarkBrush") as Brush;
                        point.X = _boxWidth * 2;
                        textBlock.Text = "LWC(s)";
                        break;
                }

                Canvas.SetLeft(rectangle, point.X);
                Canvas.SetTop(rectangle, point.Y);
                DrawCanvasTop.Children.Add(rectangle);

                // center the text in the block
                var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
                var additionalX = (_boxWidth - textSize.Width) / 2;
                Canvas.SetLeft(textBlock, point.X + additionalX);
                Canvas.SetTop(textBlock, 10);
                DrawCanvasTop.Children.Add(textBlock);
            }


        }


        private static Path GenerateLine(Point leftPoint, Point rightPt, Point controlPointLeft, Point controlPointRight)
        {
            // since we are converting to a string, we need to make sure that we are using decimal points and
            // not commas otherwise this will crash
            // convert like: 
            // value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)

            var sourcePath = $"M {leftPoint.X.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{leftPoint.Y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} C {controlPointLeft.X.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{controlPointLeft.Y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} {controlPointRight.X.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{controlPointRight.Y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} {rightPt.X.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)},{rightPt.Y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}";

            var path = new Path
            {
                Stroke = Application.Current.FindResource("GreenDarkBrush") as Brush,
                Data = Geometry.Parse(sourcePath),
                StrokeThickness = 3
            };

            return path;
        }

        #region Box Contents Drawing

        private void DrawInterlinearizerBox(int projectBoxWidth, int projectBoxHeight)
        {
            
            double offset = 0;
            
            // ==================================
            // Draw the Interlinearizer Boxes
            // ==================================
            var point = new Point
            {
                X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 6),
                Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset
            };

            var rectangle = new Rectangle
            {
                Width = projectBoxWidth,
                Height = projectBoxHeight,
                Fill = Application.Current.FindResource("OrangeMidBrush") as Brush,
                RadiusX = 3,
                RadiusY = 3,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                }
            };
            Canvas.SetTop(rectangle, point.Y);
            Canvas.SetLeft(rectangle, point.X);
            DrawCanvasBottom.Children.Add(rectangle);

            // draw the 'Source' word
            var textBlock = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = _boxWidth,
                Text = _interlinearizerProject.Name
            };
            var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
            var additionalX = (projectBoxWidth - textSize.Width) / 2;
            Canvas.SetLeft(textBlock, point.X + additionalX);
            var additionalY = (projectBoxHeight - textSize.Height) / 3;
            Canvas.SetTop(textBlock, point.Y + additionalY);
            DrawCanvasBottom.Children.Add(textBlock);


            // Draw circle at connect point (left side)
            var leftPoint = new Point(point.X, point.Y);
            leftPoint.Y += projectBoxHeight / 2;

            var ellipse = new Ellipse();
            var brushCircle = new SolidColorBrush
            {
                Color = Colors.AliceBlue
            };
            ellipse.Fill = brushCircle;
            ellipse.StrokeThickness = 1;
            ellipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            ellipse.Width = 8;
            ellipse.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(ellipse, leftPoint.Y - 2.5);
            Canvas.SetLeft(ellipse, leftPoint.X - 5);
            DrawCanvasBottom.Children.Add(ellipse);
            _inConnectionPointLeft = new Point(leftPoint.X, leftPoint.Y);

            // draw the remove button
            var button = new Button
            {
                Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton"),
                Content = new MaterialDesignThemes.Wpf.PackIcon
                    { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle },
                Width = 25,
                Height = 25
            };
            button.Click += RemoveItem_Click;
            button.Uid = "IN:" + _interlinearizerProject.Name;
            button.Foreground = Brushes.Red;
            button.Effect =
                new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                };

            Canvas.SetTop(button, point.Y - button.Height / 2);
            Canvas.SetLeft(button, point.X + projectBoxWidth - button.Width / 2);
            DrawCanvasBottom.Children.Add(button);
        }

        /// <summary>
        /// Draw the BackTranslation boxes
        /// </summary>
        /// <param name="projectBoxWidth"></param>
        /// <param name="projectBoxHeight"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DrawBackTransBoxes(int projectBoxWidth, int projectBoxHeight)
        {
            double separation = 0;
            double offset = 30;
            if (_backTranslationProjects.Count > 1)
            {
                separation = 30;
                offset = _backTranslationProjects.Count * separation / 2;
            }

            for (int i = 0; i < _backTranslationProjects.Count; i++)
            {
                // ========================
                // Draw the Back Translation Boxes
                // ========================
                var point = new Point
                {
                    X = (_boxWidth / 2) - (projectBoxWidth / 2)
                };
                if (_backTranslationProjects.Count > 1)
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset + 30;
                }
                else
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);
                }

                var rectangle = new Rectangle
                {
                    Width = projectBoxWidth,
                    Height = projectBoxHeight,
                    Fill = Application.Current.FindResource("PurpleLightBrush") as Brush,
                    RadiusX = 3,
                    RadiusY = 3,
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    }
                };
                Canvas.SetTop(rectangle, point.Y);
                Canvas.SetLeft(rectangle, point.X);
                DrawCanvasBottom.Children.Add(rectangle);

                // draw the 'Source' word
                var textBlock = new TextBlock
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = _boxWidth,
                    Text = _backTranslationProjects[i].Name
                };
                var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
                var additionalX = (projectBoxWidth - textSize.Width) / 2;
                Canvas.SetLeft(textBlock, point.X + additionalX);
                var additionalY = (projectBoxHeight - textSize.Height) / 3;
                Canvas.SetTop(textBlock, point.Y + additionalY);
                DrawCanvasBottom.Children.Add(textBlock);

                // draw the remove button
                var button = new Button
                {
                    Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton"),
                    Content = new MaterialDesignThemes.Wpf.PackIcon{ Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle },
                    Width = 25,
                    Height = 25
                };
                button.Click += RemoveItem_Click;
                button.Uid = "BT:" + _backTranslationProjects[i].Name;
                button.Foreground = Brushes.Red;
                button.Effect =
                    new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    };

                Canvas.SetTop(button, point.Y - button.Height / 2);
                Canvas.SetLeft(button, point.X + projectBoxWidth - button.Width / 2);
                DrawCanvasBottom.Children.Add(button);

                offset -= separation * 2;
            }
        }

        /// <summary>
        /// Draw the LWC boxes
        /// </summary>
        /// <param name="projectBoxWidth"></param>
        /// <param name="projectBoxHeight"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DrawLWCBoxes(int projectBoxWidth, int projectBoxHeight)
        {
            _lwcConnectionPointsLeft.Clear();

            double separation = 0;
            double offset = 0;
            if (_lwcProjects.Count > 1)
            {
                separation = 30;
                offset = _lwcProjects.Count * separation / 2;
            }

            for (int i = 0; i < _lwcProjects.Count; i++)
            {
                // ========================
                // Draw the LWC Boxes
                // ========================
                var point = new Point
                {
                    X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 6)
                };
                if (_lwcProjects.Count > 1)
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset + 30;
                }
                else
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);
                }


                var rectangle = new Rectangle
                {
                    Width = projectBoxWidth,
                    Height = projectBoxHeight,
                    Fill = Application.Current.FindResource("OrangeMidBrush") as Brush,
                    RadiusX = 3,
                    RadiusY = 3,
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    }
                };
                Canvas.SetTop(rectangle, point.Y);
                Canvas.SetLeft(rectangle, point.X);
                DrawCanvasTop.Children.Add(rectangle);

                // draw the 'Source' word
                var textBlock = new TextBlock
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = _boxWidth,
                    Text = _lwcProjects[i].Name
                };
                var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
                var additionalX = (projectBoxWidth - textSize.Width) / 2;
                Canvas.SetLeft(textBlock, point.X + additionalX);
                var additionalY = (projectBoxHeight - textSize.Height) / 3;
                Canvas.SetTop(textBlock, point.Y + additionalY);
                DrawCanvasTop.Children.Add(textBlock);

                // Draw circle at connect point (left side)
                var leftPoint = new Point(point.X, point.Y);
                leftPoint.Y += projectBoxHeight / 2;

                var ellipse = new Ellipse();
                var brushCircle = new SolidColorBrush
                {
                    Color = Colors.AliceBlue
                };
                ellipse.Fill = brushCircle;
                ellipse.StrokeThickness = 1;
                ellipse.Stroke = Brushes.Black;

                // Set the width and height of the Ellipse.
                ellipse.Width = 8;
                ellipse.Height = 8;

                // How to set center of ellipse
                Canvas.SetTop(ellipse, leftPoint.Y - 2.5);
                Canvas.SetLeft(ellipse, leftPoint.X - 5);
                DrawCanvasTop.Children.Add(ellipse);
                _lwcConnectionPointsLeft.Add(new Point(leftPoint.X - 15, leftPoint.Y - 2.5));

                // draw the remove button
                var button = new Button
                {
                    Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton"),
                    Content = new MaterialDesignThemes.Wpf.PackIcon
                        { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle },
                    Width = 25,
                    Height = 25
                };
                button.Click += RemoveItem_Click;
                button.Uid = "LWC:" + _lwcProjects[i].Name;
                button.Foreground = Brushes.Red;
                button.Effect =
                    new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    };

                Canvas.SetTop(button, point.Y - button.Height / 2);
                Canvas.SetLeft(button, point.X + projectBoxWidth - button.Width / 2);
                DrawCanvasTop.Children.Add(button);

                offset -= separation * 2;
            }
        }

        /// <summary>
        /// Draw the target box
        /// </summary>
        /// <param name="projectBoxWidth"></param>
        /// <param name="projectBoxHeight"></param>
        private void DrawTargetBox(int projectBoxWidth, int projectBoxHeight)
        {
            // ========================
            // Draw the Target Box
            // ========================
            var point = new Point
            {
                X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 3),
                Y = (_boxHeight / 2) - (projectBoxHeight / 2)
            };

            var rectangle = new Rectangle
            {
                Width = projectBoxWidth,
                Height = projectBoxHeight,
                Fill = Application.Current.FindResource("BlueMidBrush") as Brush,
                RadiusX = 3,
                RadiusY = 3,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                }
            };
            Canvas.SetTop(rectangle, point.Y);
            Canvas.SetLeft(rectangle, point.X);
            DrawCanvasTop.Children.Add(rectangle);

            // draw the 'Source' word
            var textBlock = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = _boxWidth,
                Text = _targetProject.Name
            };
            var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
            var additionalX = (projectBoxWidth - textSize.Width) / 2;
            Canvas.SetLeft(textBlock, point.X + additionalX);
            var additionalY = (projectBoxHeight - textSize.Height) / 3;
            Canvas.SetTop(textBlock, point.Y + additionalY);
            DrawCanvasTop.Children.Add(textBlock);


            // Draw circle at connect point (right side)
            var rightPoint = new Point(point.X, point.Y);
            rightPoint.X += projectBoxWidth;
            rightPoint.Y += projectBoxHeight / 2;

            var ellipse = new Ellipse();
            var brushCircle = new SolidColorBrush
            {
                Color = Colors.AliceBlue
            };
            ellipse.Fill = brushCircle;
            ellipse.StrokeThickness = 1;
            ellipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            ellipse.Width = 8;
            ellipse.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(ellipse, rightPoint.Y - 2.5);
            Canvas.SetLeft(ellipse, rightPoint.X - 2.5);
            DrawCanvasTop.Children.Add(ellipse);
            _targetConnectionPointRight = rightPoint;



            // Draw circle at connect point (left side)
            var leftPoint = new Point(point.X, point.Y);
            leftPoint.Y += projectBoxHeight / 2;

            ellipse = new Ellipse();
            brushCircle = new SolidColorBrush
            {
                Color = Colors.AliceBlue
            };
            ellipse.Fill = brushCircle;
            ellipse.StrokeThickness = 1;
            ellipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            ellipse.Width = 8;
            ellipse.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(ellipse, leftPoint.Y - 2.5);
            Canvas.SetLeft(ellipse, leftPoint.X - 5);
            DrawCanvasTop.Children.Add(ellipse);
            _targetConnectionPointLeft = leftPoint;



            // Draw circle at connect point (bottom side)
            var bottomPoint = new Point(point.X, point.Y);
            bottomPoint.Y += projectBoxHeight;
            bottomPoint.X = point.X + (projectBoxWidth / 2);

            ellipse = new Ellipse();
            brushCircle = new SolidColorBrush
            {
                Color = Colors.AliceBlue
            };
            ellipse.Fill = brushCircle;
            ellipse.StrokeThickness = 1;
            ellipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            ellipse.Width = 8;
            ellipse.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(ellipse, bottomPoint.Y - 2.5);
            Canvas.SetLeft(ellipse, bottomPoint.X - 5);
            DrawCanvasTop.Children.Add(ellipse);
            _targetConnectionPtBottom = bottomPoint;


            // draw the remove button
            var button = new Button
            {
                Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton"),
                Content = new MaterialDesignThemes.Wpf.PackIcon
                    { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle },
                Width = 25,
                Height = 25
            };
            button.Click += RemoveItem_Click;
            button.Uid = "TARGET:";
            button.Foreground = Brushes.Red;
            button.Effect =
                new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                };
            Canvas.SetTop(button, point.Y - button.Height / 2);
            Canvas.SetLeft(button, point.X + projectBoxWidth - button.Width / 2);
            DrawCanvasTop.Children.Add(button);
        }

        /// <summary>
        /// Draw the source box
        /// </summary>
        /// <param name="projectBoxWidth"></param>
        /// <param name="projectBoxHeight"></param>
        private void DrawSourceBox(int projectBoxWidth, int projectBoxHeight)
        {
            // ========================
            // Draw the Source Box
            // ========================
            var point = new Point
            {
                X = (_boxWidth / 2) - (projectBoxWidth / 2),
                Y = (_boxHeight / 2) - (projectBoxHeight / 2)
            };

            // draw the 'Source' word
            var textBlock = new TextBlock
            {
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = _boxWidth,
                Text = "MANUSCRIPT"
            };
            var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
            var additionalX = (projectBoxWidth - textSize.Width) / 2;
            Canvas.SetLeft(textBlock, point.X + additionalX);
            var additionalY = (projectBoxHeight - textSize.Height) / 3;
            Canvas.SetTop(textBlock, point.Y + additionalY);

            var rectangle = new Rectangle
            {
                Width = projectBoxWidth,
                Height = projectBoxHeight,
                Fill = Application.Current.FindResource("TealDarkBrush") as Brush,
                RadiusX = 3,
                RadiusY = 3,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                }
            };
            Canvas.SetTop(rectangle, point.Y);
            Canvas.SetLeft(rectangle, point.X);
            DrawCanvasTop.Children.Add(rectangle);
            DrawCanvasTop.Children.Add(textBlock);


            // Draw circle at connect point
            var leftPoint = new Point(point.X, point.Y);
            leftPoint.X += projectBoxWidth;
            leftPoint.Y += projectBoxHeight / 2;

            var ellipse = new Ellipse();
            var brushCircle = new SolidColorBrush
            {
                Color = Colors.AliceBlue
            };
            ellipse.Fill = brushCircle;
            ellipse.StrokeThickness = 1;
            ellipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            ellipse.Width = 8;
            ellipse.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(ellipse, leftPoint.Y - 2.5);
            Canvas.SetLeft(ellipse, leftPoint.X - 2.5);
            DrawCanvasTop.Children.Add(ellipse);
            _sourceConnectionPoint = leftPoint;
        }

        #endregion

        public void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Uid.StartsWith("TARGET:"))
                {
                    _targetProject = null;
                }
                else if (button.Uid.StartsWith("LWC:"))
                {
                    var name = button.Uid.Substring(4);
                    var index = _lwcProjects.FindIndex(a => a.Name == name);
                    try
                    {
                        _lwcProjects.RemoveAt(index);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "An unexpected error occurred while removing an LWC.");
                    }

                }
                else if (button.Uid.StartsWith("BT:"))
                {
                    var name = button.Uid.Substring(3);
                    var index = _backTranslationProjects.FindIndex(a => a.Name == name);
                    try
                    {
                        _backTranslationProjects.RemoveAt(index);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "An unexpected error occurred while removing a back translation.");
                    }
                }
                else if (button.Uid.StartsWith("IN:"))
                {
                    _interlinearizerProject = null;
                }
            }

            DrawTheCanvas();
        }


        public void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTheCanvas();
        }

        #endregion 

        public async void CreateNewProject()
        {
            if (_targetProject == null)
            {
                // unlikely ever to be true but just in case
                return;
            }

            var dashboardProject = new DashboardProject
            {
                ProjectName = ProjectName,
                TargetProject = _targetProject,
                LWCProjects = _lwcProjects,
                BTProjects = _backTranslationProjects,
                CreationDate = DateTime.Now,
                ParatextUser = ProjectManager.ParatextUserName
            };

            await ProjectManager.CreateNewProject(dashboardProject).ConfigureAwait(false);
        }

        private void ValidateProjectData()
        {
            if (ProjectName is "" or null)
            {
                CanCreateNewProject = false;
                return;
            }

            // check to see if we have at least a target project
            if (_targetProject is null)
            {
                CanCreateNewProject = false;
            }
            else
            {
                CanCreateNewProject = true;
            }
        }
    }
}
