using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        public ProjectManager _projectManager { get; set; }
        private readonly ILogger _logger;

        protected Canvas DrawCanvasTop { get; set; }
        protected Canvas DrawCanvasBottom { get; set; }

        public bool ParatextVisible = false;
        //public bool ShowWaitingIcon = true;

        private bool _isDragging = false;
        private Point _dragStartPoint;

        private double _boxWidth;
        private double _boxHeight;

        public DashboardProject DashboardProject { get; set; }

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
            LanguageOfWiderCommunication,
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

        //private Task _textXamlChangeEvent;
        //private string _textXaml;
        //public string TextXaml
        //{
        //    get => _textXaml;
        //    set
        //    {
        //        if (_textXaml == value) return;
        //        _textXaml = value;
        //        if (_textXamlChangeEvent == null || _textXamlChangeEvent.Status >= TaskStatus.RanToCompletion)
        //        {
        //            _textXamlChangeEvent = Task.Run(() =>
        //            {
        //                Task.Delay(100);
        //                retry:
        //                var oldVal = _textXaml;

        //                Thread.MemoryBarrier();
        //                _textXaml = value;
        //                NotifyOfPropertyChange(() => TextXaml);

        //                Thread.MemoryBarrier();
        //                if (oldVal != _textXaml) goto retry;
        //            });
        //        }
        //    }
        //}

        #endregion

        #region Observable Properties

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection flowDirection
        {
            get => _flowDirection;
            set
            {
                _flowDirection = value;
                NotifyOfPropertyChange(() => flowDirection);
            }
        }
        public string ProjectName
        {
            get => DashboardProject?.ProjectName;
            set
            {
                DashboardProject.ProjectName = value;
                NotifyOfPropertyChange(() => ProjectName);
                CanCreateNewProject = DashboardProject.ValidateProjectData();
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
            _projectManager = projectManager;
            _logger = logger;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;
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

            await _projectManager.SetupParatext();
            DashboardProject = _projectManager.CreateDashboardProject();

        }

   

        private void SetupUserHelp()
        {
            // get the right help text
            // TODO Work on the help regionalization
            var fileInfo = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (fileInfo.Directory != null)
            {
                var helpFile = System.IO.Path.Combine(fileInfo.Directory.ToString(), @"HelpFiles\NewProjectHelp_us.md");

                if (File.Exists(helpFile))
                {
                    var markdownTxt = File.ReadAllText(helpFile);
                    HelpText = string.Join("\r\n", Regex.Split(markdownTxt, "\r?\n").Select(ln => ln.TrimStart()));
                }
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
                    DragDrop.DoDragDrop(listView, data, DragDropEffects.Copy);
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
                    case DropZones.LanguageOfWiderCommunication:
                        text.Text = "LWC";
                        if (!DashboardProject.LanguageOfWiderCommunicationProjects.Any(p=>project != null && p.Name == project.Name))
                        {
                            // only allow it to be added once
                            DashboardProject.LanguageOfWiderCommunicationProjects.Add(project);
                        }
                        break;
                    case DropZones.Target:
                        text.Text = "TARGET";
                        if (project is { ProjectType: ProjectType.Standard })
                        {
                            DashboardProject.TargetProject = project;

                            // look for linked back translations
                            var backTranslationProjects = _projectManager.ParatextProjects.Where(p => p.TranslationInfo?.projectGuid == project.Guid).ToList();
                            foreach (var backTranslationProject in backTranslationProjects)
                            {
                                if (DashboardProject.BackTranslationProjects.All(p => p.Name != backTranslationProject.Name))
                                {
                                    DashboardProject.BackTranslationProjects.Add(backTranslationProject);
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
                        if (!DashboardProject.BackTranslationProjects.Any(p => project != null && p.Name == project.Name))
                        {
                            DashboardProject.BackTranslationProjects.Add(project);
                        }
                        break;
                    case DropZones.Blank:
                        break;
                    case DropZones.Interlinearizer:
                        text.Text = "INTERLINEARIZER";
                        if (project != null && project.ProjectType == ProjectType.Resource)
                        {
                            DashboardProject.InterlinearizerProject = project;
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
            return DropZones.LanguageOfWiderCommunication;
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
            if (DashboardProject.TargetProject != null)
            {
                DrawTargetBox(projectBoxWidth, projectBoxHeight);
            }

            // ===============================
            // Draw the LWC Box(es) Contents
            // ===============================
            if (DashboardProject.LanguageOfWiderCommunicationProjects.Count > 0)
            {
                DrawLanguageOfWiderCommunicationBoxes(projectBoxWidth, projectBoxHeight);
            }

            // =============================================
            // Draw the Back Translation Box(es) Contents
            // =============================================
            if (DashboardProject.BackTranslationProjects.Count > 0)
            {
                DrawBackTransBoxes(projectBoxWidth, projectBoxHeight);
            }

            // =============================================
            // Draw the Interlinearizer Contents
            // =============================================
            if (DashboardProject.InterlinearizerProject != null)
            {
                DrawInterlinearizerBox(projectBoxWidth, projectBoxHeight);
            }

            // ===============================
            // Draw the ConnectionLines
            // ===============================
            // connect target to source
            if (DashboardProject.TargetProject != null)
            {
                var leftPoint = new Point(_sourceConnectionPoint.X + 6, _sourceConnectionPoint.Y + 2);
                var rightPoint = new Point(_targetConnectionPointLeft.X - 6, _targetConnectionPointLeft.Y + 2);
                var controlPointSource = new Point(_sourceConnectionPoint.X + 20, _sourceConnectionPoint.Y);
                var controlPointTarget = new Point(_targetConnectionPointLeft.X - 20, _targetConnectionPointLeft.Y);
                var path = GenerateLine(leftPoint, rightPoint, controlPointSource, controlPointTarget);
                DrawCanvasTop.Children.Add(path);
            }

            // some LWC's so connect to Target
            for (var i = 0; i < DashboardProject.LanguageOfWiderCommunicationProjects.Count; i++)
            {
                if (DashboardProject.TargetProject != null)
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
            if (DashboardProject.TargetProject != null && DashboardProject.InterlinearizerProject != null)
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
                // ReSharper disable PossibleLossOfFraction
                X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 6),
                Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset
                // ReSharper restore PossibleLossOfFraction
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
                Text = DashboardProject.InterlinearizerProject.Name
            };
            var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
            var additionalX = (projectBoxWidth - textSize.Width) / 2;
            Canvas.SetLeft(textBlock, point.X + additionalX);
            var additionalY = (projectBoxHeight - textSize.Height) / 3;
            Canvas.SetTop(textBlock, point.Y + additionalY);
            DrawCanvasBottom.Children.Add(textBlock);


            // Draw circle at connect point (left side)
            var leftPoint = new Point(point.X, point.Y);
            // ReSharper disable PossibleLossOfFraction
            leftPoint.Y += projectBoxHeight / 2;
            // ReSharper restore PossibleLossOfFraction

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
            button.Uid = "IN:" + DashboardProject.InterlinearizerProject.Name;
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
            if (DashboardProject.BackTranslationProjects.Count > 1)
            {
                separation = 30;
                offset = DashboardProject.BackTranslationProjects.Count * separation / 2;
            }

            for (var i = 0; i < DashboardProject.BackTranslationProjects.Count; i++)
            {
                // ========================
                // Draw the Back Translation Boxes
                // ========================
                var point = new Point
                {
                    // ReSharper disable PossibleLossOfFraction
                    X = (_boxWidth / 2) - (projectBoxWidth / 2)
                    // ReSharper restore PossibleLossOfFraction
                };
                if (DashboardProject.BackTranslationProjects.Count > 1)
                {
                    // ReSharper disable PossibleLossOfFraction
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset + 30;
                    // ReSharper restore PossibleLossOfFraction
                }
                else
                {
                    // ReSharper disable PossibleLossOfFraction
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);
                    // ReSharper restore PossibleLossOfFraction
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
                    Text = DashboardProject.BackTranslationProjects[i].Name
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
                button.Uid = "BT:" + DashboardProject.BackTranslationProjects[i].Name;
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
        private void DrawLanguageOfWiderCommunicationBoxes(int projectBoxWidth, int projectBoxHeight)
        {
            _lwcConnectionPointsLeft.Clear();

            double separation = 0;
            double offset = 0;
            if (DashboardProject.LanguageOfWiderCommunicationProjects.Count > 1)
            {
                separation = 30;
                offset = DashboardProject.LanguageOfWiderCommunicationProjects.Count * separation / 2;
            }

            for (var i = 0; i < DashboardProject.LanguageOfWiderCommunicationProjects.Count; i++)
            {
                // ========================
                // Draw the LWC Boxes
                // ========================
                var point = new Point
                {
                    // ReSharper disable PossibleLossOfFraction
                    X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 6)
                    // ReSharper restore PossibleLossOfFraction
                };
                if (DashboardProject.LanguageOfWiderCommunicationProjects.Count > 1)
                {
                    // ReSharper disable PossibleLossOfFraction
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset + 30;
                    // ReSharper restore PossibleLossOfFraction
                }
                else
                {
                    // ReSharper disable PossibleLossOfFraction
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);
                    // ReSharper restore PossibleLossOfFraction
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
                    Text = DashboardProject.LanguageOfWiderCommunicationProjects[i].Name
                };
                var textSize = DrawingUtils.MeasureString(textBlock.Text, textBlock);
                var additionalX = (projectBoxWidth - textSize.Width) / 2;
                Canvas.SetLeft(textBlock, point.X + additionalX);
                var additionalY = (projectBoxHeight - textSize.Height) / 3;
                Canvas.SetTop(textBlock, point.Y + additionalY);
                DrawCanvasTop.Children.Add(textBlock);

                // Draw circle at connect point (left side)
                var leftPoint = new Point(point.X, point.Y);
                // ReSharper disable PossibleLossOfFraction
                leftPoint.Y += projectBoxHeight / 2;
                // ReSharper restore PossibleLossOfFraction

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
                button.Uid = "LWC:" + DashboardProject.LanguageOfWiderCommunicationProjects[i].Name;
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
                // ReSharper disable PossibleLossOfFraction
                X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 3),
                Y = (_boxHeight / 2) - (projectBoxHeight / 2)
                // ReSharper restore PossibleLossOfFraction
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
                Text = DashboardProject.TargetProject.Name
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
            // ReSharper disable PossibleLossOfFraction
            rightPoint.Y += projectBoxHeight / 2;
            // ReSharper restore PossibleLossOfFraction

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
            // ReSharper disable PossibleLossOfFraction
            leftPoint.Y += projectBoxHeight / 2;
            // ReSharper restore PossibleLossOfFraction

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
            // ReSharper disable PossibleLossOfFraction
            bottomPoint.X = point.X + (projectBoxWidth / 2);
            // ReSharper restore PossibleLossOfFraction

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
                // ReSharper disable PossibleLossOfFraction
                X = (_boxWidth / 2) - (projectBoxWidth / 2),
                Y = (_boxHeight / 2) - (projectBoxHeight / 2)
                // ReSharper restore PossibleLossOfFraction
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
            // ReSharper disable PossibleLossOfFraction
            leftPoint.Y += projectBoxHeight / 2;
            // ReSharper restore PossibleLossOfFraction

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
                    DashboardProject.TargetProject = null;
                }
                else if (button.Uid.StartsWith("LWC:"))
                {
                    var name = button.Uid.Substring(4);
                    var index = DashboardProject.LanguageOfWiderCommunicationProjects.FindIndex(a => a.Name == name);
                    try
                    {
                        DashboardProject.LanguageOfWiderCommunicationProjects.RemoveAt(index);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "An unexpected error occurred while removing an LWC.");
                    }

                }
                else if (button.Uid.StartsWith("BT:"))
                {
                    var name = button.Uid.Substring(3);
                    var index = DashboardProject.BackTranslationProjects.FindIndex(a => a.Name == name);
                    try
                    {
                        DashboardProject.BackTranslationProjects.RemoveAt(index);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "An unexpected error occurred while removing a back translation.");
                    }
                }
                else if (button.Uid.StartsWith("IN:"))
                {
                    DashboardProject.InterlinearizerProject = null;
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
            if (DashboardProject.TargetProject == null)
            {
                // unlikely ever to be true but just in case
                return;
            }
            await _projectManager.CreateNewProject(DashboardProject).ConfigureAwait(false);
        }

    
    }
}
