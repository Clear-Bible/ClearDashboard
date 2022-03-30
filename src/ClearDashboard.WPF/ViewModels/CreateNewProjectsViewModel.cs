using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.Helpers;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Views;
using Path = System.Windows.Shapes.Path;

//using Path = System.IO.Path;

//using Path = System.IO.Path;


namespace ClearDashboard.Wpf.ViewModels
{
    public class CreateNewProjectsViewModel : ApplicationScreen
    {
        #region Properties
        private ProjectManager ProjectManager { get; set; }

        public bool ParatextVisible = false;
        public bool ShowWaitingIcon = true;

        //public DragAndDropManager DragAndDropManager { get; private set; }


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


        private bool _buttonEnabled;
        public bool ButtonEnabled
        {
            get => _buttonEnabled;
            set
            {
                _buttonEnabled = value;
                NotifyOfPropertyChange(() => ButtonEnabled);
            }
        }



        //private List<ParatextProject> _lwcProjects = new List<ParatextProject>();
        //private ParatextProject _targetProject = null; 
        //private List<ParatextProject> _backTransProjects = new List<ParatextProject>();

        public ObservableRangeCollection<ParatextProjectDisplay> ParatextProjects { get; set; } =
            new ObservableRangeCollection<ParatextProjectDisplay>();

        public ObservableRangeCollection<ParatextProjectDisplay> ParatextResources { get; set; } =
            new ObservableRangeCollection<ParatextProjectDisplay>();


        private Task _textXamlChangeEvent;
        public string _textXaml;
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
                SetProjects(null, null,null,null);
            }
        }
        #endregion

        #region Commands

        private ICommand _createNewProjectCommand; 
        public ICommand CreateNewProjectCommand
        {
            get => _createNewProjectCommand;
            set => _createNewProjectCommand = value;
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
            _createNewProjectCommand = new RelayCommand(CreateNewProject);
          
        }

        #endregion


        #region Startup

        protected Canvas DrawCanvasTop { get; set; }
        protected Canvas DrawCanvasBottom { get; set; }
        protected override void OnViewAttached(object view, object context)
        {
            // DragAndDropManager = new DragAndDropManager(view as CreateNewProjectsView);

            var createNewProjectView = (view as CreateNewProjectsView);

            DrawCanvasTop = (Canvas)createNewProjectView.FindName("DrawCanvasTop");
            DrawCanvasBottom = (Canvas)createNewProjectView.FindName("DrawCanvasBottom");
            base.OnViewAttached(view, context);
        }
        protected override async void OnViewLoaded(object view)
        {
            await Init();
            base.OnViewLoaded(view);
            DrawTheCanvas();
        }

        public async Task Init()
        {
            // get the right help text
            // TODO Work on the help regionalization
            var fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var helpFile = System.IO.Path.Combine(fi.Directory.ToString(), @"HelpFiles\NewProjectHelp_us.md");

            if (File.Exists(helpFile))
            {
                var markdownTxt = File.ReadAllText(helpFile);
                HelpText = String.Join("\r\n", Regex.Split(markdownTxt, "\r?\n").Select(ln => ln.TrimStart()));
            }


            // detect if Paratext is installed
            var paratextUtils = new ParatextUtils();
            ParatextVisible = paratextUtils.IsParatextInstalled();

            if (ParatextVisible)
            {
                // get all the Paratext Projects (Projects/Backtranslations)
                ParatextProjects.Clear();
                List<ParatextProject> projects = await paratextUtils.GetParatextProjectsOrResources(ParatextUtils.eFolderType.Projects);
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectDisplay>();
                    foreach (var project in projects)
                    {
                        ParatextProjects.Add(TinyMapper.Map<ParatextProjectDisplay>(project));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                // get all the Paratext Resources (LWC)
                ParatextResources.Clear();
                List<ParatextProject> resources = paratextUtils.GetParatextResources();
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectDisplay>();
                    foreach (var resource in resources)
                    {
                        ParatextResources.Add(TinyMapper.Map<ParatextProjectDisplay>(resource));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unexpected error while initializing");
                }
            }

        }

        #endregion

        #region Page management


        private bool _isDraging = false;
        private Point _dragStartPoint;

        private double _boxWidth;
        private double _boxHeight;

        private ParatextProject _targetProject;
        private List<ParatextProject> _lwcProjects = new List<ParatextProject>();
        private List<ParatextProject> _backTranslationProjects = new List<ParatextProject>();
        private ParatextProject _interlinearizerProject;

        // variables that hold the arrow connection points
        private Point _sourceConnectionPt = new Point(0, 0);
        private readonly List<Point> _lwcConnectionPtsLeft = new List<Point>();
        private Point _targetConnectionPtLeft = new Point(0, 0);
        private Point _targetConnectionPtRight = new Point(0, 0);
        private Point _targetConnectionPtBottom = new Point(0, 0);
        private Point _inConnectionPtsLeft = new Point(0, 0);

        private enum DropZones
        {
            Source,
            LWC,
            Target,
            BackTranslation,
            Interlinearizer,
            Blank
        }

        public void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        public void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var listView = sender as ListView;

            if (listView.SelectedItem == null) return;

            if (e.LeftButton == MouseButtonState.Pressed && !_isDraging)
            {
                var position = e.GetPosition(null);

                if (Math.Abs(position.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDraging = true;
                    var data = new DataObject(typeof(ParatextProject), listView.SelectedItem);
                    DragDropEffects de = DragDrop.DoDragDrop(listView, data, DragDropEffects.Copy);
                    _isDraging = false;
                }
            }
        }

        public void Canvas_PreviewDropTop(object sender, DragEventArgs e)
        {
            var data = e.Data;
            var p = e.GetPosition(DrawCanvasTop);

            var dropZone = GetDropZone(p);

            if (data.GetDataPresent(typeof(ParatextProject)))
            {
                var project = data.GetData(typeof(ParatextProject)) as ParatextProject;

                TextBlock text = new TextBlock();

                switch (dropZone)
                {
                    case DropZones.Source:
                        text.Text = "MANUSCRIPT";
                        break;
                    case DropZones.LWC:
                        text.Text = "LWC";
                        var t = _lwcProjects.Where(p => p.Name == project.Name).ToList();
                        if (t.Count() == 0)
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
                            var btProjs = ParatextProjects.Where(p => p.TranslationInfo?.projectGuid == project.Guid).ToList();
                            foreach (var btProject in btProjs)
                            {
                                t = _backTranslationProjects.Where(p => p.Name == btProject.Name).ToList();
                                if (t.Count() == 0)
                                {
                                    _backTranslationProjects.Add(btProject);
                                }
                            }
                        }
                        break;
                }
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                Canvas.SetTop(text, p.Y);
                Canvas.SetLeft(text, p.X);
                DrawCanvasTop.Children.Add(text);

                // update the ViewModel with the current set
                SetProjects(_lwcProjects, _targetProject, _backTranslationProjects, _interlinearizerProject);

                DrawTheCanvas();
            }
        }

        public void Canvas_PreviewDropBottom(object sender, DragEventArgs e)
        {
            var data = e.Data;
            Point p = e.GetPosition(DrawCanvasBottom);

            var dropZone = GetDropZoneBottom(p);

            if (data.GetDataPresent(typeof(ParatextProject)))
            {
                var project = data.GetData(typeof(ParatextProject)) as ParatextProject;

                TextBlock text = new TextBlock();

                switch (dropZone)
                {
                    case DropZones.BackTranslation:
                        text.Text = "BACK TRANSLATION";
                        // look for linked back translations
                        var t = _backTranslationProjects.Where(p => p.Name == project.Name).ToList();
                        if (t.Count() == 0)
                        {
                            _backTranslationProjects.Add(project);
                        }
                        break;
                    case DropZones.Blank:
                        break;
                    case DropZones.Interlinearizer:
                        text.Text = "INTERLINEARIZER";
                        if (project.ProjectType == ParatextProject.eProjectType.Resource)
                        {
                            _interlinearizerProject = project;
                        }
                        break;
                }
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                Canvas.SetTop(text, p.Y);
                Canvas.SetLeft(text, p.X);
                DrawCanvasBottom.Children.Add(text);

                // update the ViewModel with the current set
                SetProjects(_lwcProjects, _targetProject, _backTranslationProjects, _interlinearizerProject);

                DrawTheCanvas();
            }
        }
        private DropZones GetDropZone(Point pt)
        {
            if (pt.X < _boxWidth)
            {
                return DropZones.Source;
            }
            if (pt.X > _boxWidth && pt.X < _boxWidth * 2)
            {
                return DropZones.Target;
            }
            return DropZones.LWC;
        }

        private DropZones GetDropZoneBottom(Point pt)
        {
            if (pt.X < _boxWidth)
            {
                return DropZones.BackTranslation;
            }
            if (pt.X > _boxWidth && pt.X < _boxWidth * 2)
            {
                return DropZones.Blank;
            }
            return DropZones.Interlinearizer;
        }

        private void DrawTheCanvas()
        {
            _boxWidth = DrawCanvasTop.ActualWidth / 3; //width of each box
            _boxHeight = DrawCanvasTop.ActualHeight;  // Height of each box

            int projectBoxWidth = (int)(_boxWidth * 0.33);
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
                var leftPt = new Point(_sourceConnectionPt.X + 6, _sourceConnectionPt.Y + 2);
                var rightPt = new Point(_targetConnectionPtLeft.X - 6, _targetConnectionPtLeft.Y + 2);
                Point controlPtSource = new Point(_sourceConnectionPt.X + 20, _sourceConnectionPt.Y);
                Point controlPtTarget = new Point(_targetConnectionPtLeft.X - 20, _targetConnectionPtLeft.Y);
                Path path = GenerateLine(leftPt, rightPt, controlPtSource, controlPtTarget);
                DrawCanvasTop.Children.Add(path);
            }

            // some LWC's so connect to Target
            for (int i = 0; i < _lwcProjects.Count; i++)
            {
                if (_targetProject != null)
                {
                    var leftPt = new Point(_lwcConnectionPtsLeft[i].X + 6, _lwcConnectionPtsLeft[i].Y + 2);
                    var rightPt = new Point(_targetConnectionPtRight.X + 6, _targetConnectionPtRight.Y + 2);
                    // draw from right LWC to Target
                    Point controlPtSource = new Point(_lwcConnectionPtsLeft[i].X - 20, _lwcConnectionPtsLeft[i].Y);
                    Point controlPtTarget = new Point(_targetConnectionPtRight.X + 20, _targetConnectionPtRight.Y);
                    Path path = GenerateLine(leftPt, rightPt, controlPtSource, controlPtTarget);
                    DrawCanvasTop.Children.Add(path);
                }
            }


            // connection lines between target and Intelinear
            if (_targetProject != null && _interlinearizerProject != null)
            {
                var bottomPt = new Point(_targetConnectionPtBottom.X, _targetConnectionPtBottom.Y + 6);
                var leftPt = new Point(_inConnectionPtsLeft.X - 6, _inConnectionPtsLeft.Y + 10 + DrawCanvasTop.ActualHeight);
                Point controlPtSource = new Point(_targetConnectionPtBottom.X, _targetConnectionPtBottom.Y + (DrawCanvasTop.ActualHeight / 3));
                Point controlPtTarget = new Point(_inConnectionPtsLeft.X - (DrawCanvasTop.ActualHeight / 3), _inConnectionPtsLeft.Y + DrawCanvasTop.ActualHeight);
                Path path = GenerateLine(bottomPt, leftPt, controlPtSource, controlPtTarget);
                DrawCanvasTop.Children.Add(path);
            }

        }

        private void DrawAllTheBoxes()
        {
            const int cornerRadius = 8;

            // =======================================================
            // BUILD THE HEADERS AND DROP BOXES - LOWER CANVAS
            // =======================================================
            for (int i = 0; i < 3; i++)
            {
                TextBlock text = new TextBlock();
                text.FontSize = 20;
                text.FontWeight = FontWeights.Bold;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.Width = _boxWidth;

                Rectangle rect = new Rectangle();
                rect.Height = DrawCanvasBottom.ActualHeight;
                rect.Width = _boxWidth - 2;
                rect.RadiusX = cornerRadius;
                rect.RadiusY = cornerRadius;
                rect.StrokeThickness = 3;
                rect.Fill = Brushes.Transparent;
                rect.Effect =
                    new DropShadowEffect
                    {
                        BlurRadius = 2,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    };

                Point point = new Point();
                point.Y = 0;
                switch (i)
                {
                    case 0:
                        rect.Stroke = Application.Current.FindResource("PurpleMidBrush") as Brush;
                        text.Foreground = Application.Current.FindResource("PurpleMidBrush") as Brush;
                        point.X = 0;
                        text.Text = "Back Translation(s)";

                        Canvas.SetLeft(rect, point.X);
                        Canvas.SetTop(rect, point.Y);
                        DrawCanvasBottom.Children.Add(rect);

                        // center the text in the block
                        Size sz = DrawingUtils.MeasureString(text.Text, text);
                        var additionalX = (_boxWidth - sz.Width) / 2;
                        Canvas.SetLeft(text, point.X + additionalX);
                        Canvas.SetTop(text, 10);
                        DrawCanvasBottom.Children.Add(text);
                        break;
                    case 1:
                        break;
                    case 2:
                        rect.Stroke = Application.Current.FindResource("OrangeDarkBrush") as Brush;
                        text.Foreground = Application.Current.FindResource("OrangeDarkBrush") as Brush;
                        point.X = _boxWidth * 2;
                        text.Text = "Interlinearizer";

                        Canvas.SetLeft(rect, point.X);
                        Canvas.SetTop(rect, point.Y);
                        DrawCanvasBottom.Children.Add(rect);

                        // center the text in the block
                        sz = DrawingUtils.MeasureString(text.Text, text);
                        additionalX = (_boxWidth - sz.Width) / 2;
                        Canvas.SetLeft(text, point.X + additionalX);
                        Canvas.SetTop(text, 10);
                        DrawCanvasBottom.Children.Add(text);
                        break;
                }


            }

            // =======================================================
            // BUILD THE HEADERS AND DROP BOXES - UPPER CANVAS
            // =======================================================
            for (int i = 0; i < 3; i++)
            {
                TextBlock text = new TextBlock();
                text.FontSize = 20;
                text.FontWeight = FontWeights.Bold;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.Width = _boxWidth;

                Rectangle rect = new Rectangle();
                rect.Height = DrawCanvasTop.ActualHeight;
                rect.Width = _boxWidth - 2;
                rect.RadiusX = cornerRadius;
                rect.RadiusY = cornerRadius;
                rect.StrokeThickness = 3;
                rect.Fill = Brushes.Transparent;
                rect.Effect =
                    new DropShadowEffect
                    {
                        BlurRadius = 2,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    };

                Point point = new Point();
                point.Y = 0;
                switch (i)
                {
                    case 0:
                        rect.Stroke = Application.Current.FindResource("TealVeryDarkBrush") as Brush;
                        text.Foreground = Application.Current.FindResource("TealVeryDarkBrush") as Brush;
                        point.X = 0;
                        text.Text = "Manuscript";
                        break;
                    case 1:
                        rect.Stroke = Application.Current.FindResource("BlueDarkBrush") as Brush;
                        text.Foreground = Application.Current.FindResource("BlueDarkBrush") as Brush;
                        point.X = _boxWidth;
                        text.Text = "Target";
                        break;
                    case 2:
                        rect.Stroke = Application.Current.FindResource("OrangeDarkBrush") as Brush;
                        text.Foreground = Application.Current.FindResource("OrangeDarkBrush") as Brush;
                        point.X = _boxWidth * 2;
                        text.Text = "LWC(s)";
                        break;
                }

                Canvas.SetLeft(rect, point.X);
                Canvas.SetTop(rect, point.Y);
                DrawCanvasTop.Children.Add(rect);

                // center the text in the block
                Size sz = DrawingUtils.MeasureString(text.Text, text);
                var additionalX = (_boxWidth - sz.Width) / 2;
                Canvas.SetLeft(text, point.X + additionalX);
                Canvas.SetTop(text, 10);
                DrawCanvasTop.Children.Add(text);
            }


        }


        private static Path GenerateLine(Point leftPt, Point rightPt, Point controlPtLeft, Point controlPtRight)
        {
            // since we are converting to a string, we need to make sure that we are using decimal points and
            // not commas otherwise this will crash
            // convert like: 
            // value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)

            string sPath = string.Format("M {0},{1} C {2},{3} {4},{5} {6},{7}",
                leftPt.X.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), leftPt.Y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                controlPtLeft.X.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), controlPtLeft.Y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                controlPtRight.X.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), controlPtRight.Y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                rightPt.X.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), rightPt.Y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));

            var path = new Path();

            path.Stroke = Application.Current.FindResource("GreenDarkBrush") as Brush;
            path.Data = Geometry.Parse(sPath);
            path.StrokeThickness = 3;

            return path;
        }

        #region Box Contents Drawing

        private void DrawInterlinearizerBox(int projectBoxWidth, int projectBoxHeight)
        {
            const double separation = 75;
            double offset = 0;


            // ==================================
            // Draw the Interlinearizer Boxes
            // ==================================
            Point point = new Point();
            point.X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 6);
            point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset;

            Rectangle targetRect = new Rectangle();
            targetRect.Width = projectBoxWidth;
            targetRect.Height = projectBoxHeight;
            targetRect.Fill = Application.Current.FindResource("OrangeMidBrush") as Brush;
            targetRect.RadiusX = 3;
            targetRect.RadiusY = 3;
            targetRect.Effect =
                new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                };
            Canvas.SetTop(targetRect, point.Y);
            Canvas.SetLeft(targetRect, point.X);
            DrawCanvasBottom.Children.Add(targetRect);

            // draw the 'Source' word
            TextBlock text = new TextBlock();
            text.FontSize = 16;
            text.FontWeight = FontWeights.Bold;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.Width = _boxWidth;
            text.Text = _interlinearizerProject.Name;
            Size sz = DrawingUtils.MeasureString(text.Text, text);
            var additionalX = (projectBoxWidth - sz.Width) / 2;
            Canvas.SetLeft(text, point.X + additionalX);
            var additionalY = (projectBoxHeight - sz.Height) / 3;
            Canvas.SetTop(text, point.Y + additionalY);
            DrawCanvasBottom.Children.Add(text);


            // Draw circle at connect point (right side)
            //Point rPt = new Point(point.X, point.Y + offset);
            //rPt.X += projectBoxWidth;
            //rPt.Y += projectBoxHeight / 2 - offset;

            //Ellipse circlePt = new Ellipse();
            //SolidColorBrush brushCircle = new SolidColorBrush();
            //brushCircle.Color = Colors.AliceBlue;
            //circlePt.Fill = brushCircle;
            //circlePt.StrokeThickness = 1;
            //circlePt.Stroke = Brushes.Black;

            //// Set the width and height of the Ellipse.
            //circlePt.Width = 8;
            //circlePt.Height = 8;

            //// How to set center of ellipse
            //Canvas.SetTop(circlePt, rPt.Y - 2.5);
            //Canvas.SetLeft(circlePt, rPt.X - 2.5);
            //DrawCanvasBottom.Children.Add(circlePt);

            // Draw circle at connect point (left side)
            Point lPt = new Point(point.X, point.Y);
            lPt.Y += projectBoxHeight / 2;

            var circlePt = new Ellipse();
            var brushCircle = new SolidColorBrush();
            brushCircle.Color = Colors.AliceBlue;
            circlePt.Fill = brushCircle;
            circlePt.StrokeThickness = 1;
            circlePt.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            circlePt.Width = 8;
            circlePt.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(circlePt, lPt.Y - 2.5);
            Canvas.SetLeft(circlePt, lPt.X - 5);
            DrawCanvasBottom.Children.Add(circlePt);
            _inConnectionPtsLeft = new Point(lPt.X, lPt.Y);

            // draw the remove button
            Button btn = new Button();
            btn.Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton");
            btn.Content = new MaterialDesignThemes.Wpf.PackIcon
            { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle };
            btn.Width = 25;
            btn.Height = 25;
            btn.Click += RemoveItem_Click;
            btn.Uid = "IN:" + _interlinearizerProject.Name;
            btn.Foreground = Brushes.Red;
            btn.Effect =
                new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                };

            Canvas.SetTop(btn, point.Y - btn.Height / 2);
            Canvas.SetLeft(btn, point.X + projectBoxWidth - btn.Width / 2);
            DrawCanvasBottom.Children.Add(btn);
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
                Point point = new Point();
                point.X = (_boxWidth / 2) - (projectBoxWidth / 2);
                if (_backTranslationProjects.Count > 1)
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset + 30;
                }
                else
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);
                }

                Rectangle targetRect = new Rectangle();
                targetRect.Width = projectBoxWidth;
                targetRect.Height = projectBoxHeight;
                targetRect.Fill = Application.Current.FindResource("PurpleLightBrush") as Brush;
                targetRect.RadiusX = 3;
                targetRect.RadiusY = 3;
                targetRect.Effect =
                    new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    };
                Canvas.SetTop(targetRect, point.Y);
                Canvas.SetLeft(targetRect, point.X);
                DrawCanvasBottom.Children.Add(targetRect);

                // draw the 'Source' word
                TextBlock text = new TextBlock();
                text.FontSize = 16;
                text.FontWeight = FontWeights.Bold;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.Width = _boxWidth;
                text.Text = _backTranslationProjects[i].Name;
                Size sz = DrawingUtils.MeasureString(text.Text, text);
                var additionalX = (projectBoxWidth - sz.Width) / 2;
                Canvas.SetLeft(text, point.X + additionalX);
                var additionalY = (projectBoxHeight - sz.Height) / 3;
                Canvas.SetTop(text, point.Y + additionalY);
                DrawCanvasBottom.Children.Add(text);


                //// Draw circle at connect point (right side)
                //Point rPt = new Point(point.X, point.Y + offset);
                //rPt.X += projectBoxWidth;
                //rPt.Y += projectBoxHeight / 2 - offset;

                //Ellipse circlePt = new Ellipse();
                //SolidColorBrush brushCircle = new SolidColorBrush();
                //brushCircle.Color = Colors.AliceBlue;
                //circlePt.Fill = brushCircle;
                //circlePt.StrokeThickness = 1;
                //circlePt.Stroke = Brushes.Black;

                //// Set the width and height of the Ellipse.
                //circlePt.Width = 8;
                //circlePt.Height = 8;

                //// How to set center of ellipse
                //Canvas.SetTop(circlePt, rPt.Y - 2.5);
                //Canvas.SetLeft(circlePt, rPt.X - 2.5);
                //DrawCanvasBottom.Children.Add(circlePt);

                //// Draw circle at connect point (left side)
                //Point lPt = new Point(point.X, point.Y);
                //lPt.Y += projectBoxHeight / 2;

                //circlePt = new Ellipse();
                //brushCircle = new SolidColorBrush();
                //brushCircle.Color = Colors.AliceBlue;
                //circlePt.Fill = brushCircle;
                //circlePt.StrokeThickness = 1;
                //circlePt.Stroke = Brushes.Black;

                //// Set the width and height of the Ellipse.
                //circlePt.Width = 8;
                //circlePt.Height = 8;

                //// How to set center of ellipse
                //Canvas.SetTop(circlePt, lPt.Y - 2.5);
                //Canvas.SetLeft(circlePt, lPt.X - 5);
                //DrawCanvasBottom.Children.Add(circlePt);

                // draw the remove button
                Button btn = new Button();
                btn.Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton");
                btn.Content = new MaterialDesignThemes.Wpf.PackIcon
                { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle };
                btn.Width = 25;
                btn.Height = 25;
                btn.Click += RemoveItem_Click;
                btn.Uid = "BT:" + _backTranslationProjects[i].Name;
                btn.Foreground = Brushes.Red;
                btn.Effect =
                    new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    };

                Canvas.SetTop(btn, point.Y - btn.Height / 2);
                Canvas.SetLeft(btn, point.X + projectBoxWidth - btn.Width / 2);
                DrawCanvasBottom.Children.Add(btn);

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
            _lwcConnectionPtsLeft.Clear();

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
                Point point = new Point();
                point.X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 6);
                if (_lwcProjects.Count > 1)
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset + 30;
                }
                else
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);
                }


                Rectangle targetRect = new Rectangle();
                targetRect.Width = projectBoxWidth;
                targetRect.Height = projectBoxHeight;
                targetRect.Fill = Application.Current.FindResource("OrangeMidBrush") as Brush;
                targetRect.RadiusX = 3;
                targetRect.RadiusY = 3;
                targetRect.Effect =
                    new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    };
                Canvas.SetTop(targetRect, point.Y);
                Canvas.SetLeft(targetRect, point.X);
                DrawCanvasTop.Children.Add(targetRect);

                // draw the 'Source' word
                TextBlock text = new TextBlock();
                text.FontSize = 16;
                text.FontWeight = FontWeights.Bold;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.Width = _boxWidth;
                text.Text = _lwcProjects[i].Name;
                Size sz = DrawingUtils.MeasureString(text.Text, text);
                var additionalX = (projectBoxWidth - sz.Width) / 2;
                Canvas.SetLeft(text, point.X + additionalX);
                var additionalY = (projectBoxHeight - sz.Height) / 3;
                Canvas.SetTop(text, point.Y + additionalY);
                DrawCanvasTop.Children.Add(text);

                // Draw circle at connect point (left side)
                Point lPt = new Point(point.X, point.Y);
                lPt.Y += projectBoxHeight / 2;

                var circlePt = new Ellipse();
                var brushCircle = new SolidColorBrush();
                brushCircle.Color = Colors.AliceBlue;
                circlePt.Fill = brushCircle;
                circlePt.StrokeThickness = 1;
                circlePt.Stroke = Brushes.Black;

                // Set the width and height of the Ellipse.
                circlePt.Width = 8;
                circlePt.Height = 8;

                // How to set center of ellipse
                Canvas.SetTop(circlePt, lPt.Y - 2.5);
                Canvas.SetLeft(circlePt, lPt.X - 5);
                DrawCanvasTop.Children.Add(circlePt);
                _lwcConnectionPtsLeft.Add(new Point(lPt.X - 15, lPt.Y - 2.5));

                // draw the remove button
                Button btn = new Button();
                btn.Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton");
                btn.Content = new MaterialDesignThemes.Wpf.PackIcon
                { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle };
                btn.Width = 25;
                btn.Height = 25;
                btn.Click += RemoveItem_Click;
                btn.Uid = "LWC:" + _lwcProjects[i].Name;
                btn.Foreground = Brushes.Red;
                btn.Effect =
                    new DropShadowEffect
                    {
                        BlurRadius = 5,
                        ShadowDepth = 2,
                        Opacity = 0.75
                    };

                Canvas.SetTop(btn, point.Y - btn.Height / 2);
                Canvas.SetLeft(btn, point.X + projectBoxWidth - btn.Width / 2);
                DrawCanvasTop.Children.Add(btn);

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
            TextBlock text;
            Size sz;
            double additionalX;
            // ========================
            // Draw the Target Box
            // ========================
            Point point = new Point();
            point.X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 3);
            point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);

            Rectangle targetRect = new Rectangle();
            targetRect.Width = projectBoxWidth;
            targetRect.Height = projectBoxHeight;
            targetRect.Fill = Application.Current.FindResource("BlueMidBrush") as Brush;
            targetRect.RadiusX = 3;
            targetRect.RadiusY = 3;
            //sourceRect.Stroke = Application.Current.FindResource("TealDarkBrush") as Brush;
            //sourceRect.StrokeThickness = 2;
            targetRect.Effect =
                new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                };
            Canvas.SetTop(targetRect, point.Y);
            Canvas.SetLeft(targetRect, point.X);
            DrawCanvasTop.Children.Add(targetRect);

            // draw the 'Source' word
            text = new TextBlock();
            text.FontSize = 16;
            text.FontWeight = FontWeights.Bold;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.Width = _boxWidth;
            text.Text = _targetProject.Name;
            sz = DrawingUtils.MeasureString(text.Text, text);
            additionalX = (projectBoxWidth - sz.Width) / 2;
            Canvas.SetLeft(text, point.X + additionalX);
            var additionalY = (projectBoxHeight - sz.Height) / 3;
            Canvas.SetTop(text, point.Y + additionalY);
            DrawCanvasTop.Children.Add(text);


            // Draw circle at connect point (right side)
            Point rPt = new Point(point.X, point.Y);
            rPt.X += projectBoxWidth;
            rPt.Y += projectBoxHeight / 2;

            Ellipse circlePt = new Ellipse();
            SolidColorBrush brushCircle = new SolidColorBrush();
            brushCircle.Color = Colors.AliceBlue;
            circlePt.Fill = brushCircle;
            circlePt.StrokeThickness = 1;
            circlePt.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            circlePt.Width = 8;
            circlePt.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(circlePt, rPt.Y - 2.5);
            Canvas.SetLeft(circlePt, rPt.X - 2.5);
            DrawCanvasTop.Children.Add(circlePt);
            _targetConnectionPtRight = rPt;



            // Draw circle at connect point (left side)
            Point lPt = new Point(point.X, point.Y);
            lPt.Y += projectBoxHeight / 2;

            circlePt = new Ellipse();
            brushCircle = new SolidColorBrush();
            brushCircle.Color = Colors.AliceBlue;
            circlePt.Fill = brushCircle;
            circlePt.StrokeThickness = 1;
            circlePt.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            circlePt.Width = 8;
            circlePt.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(circlePt, lPt.Y - 2.5);
            Canvas.SetLeft(circlePt, lPt.X - 5);
            DrawCanvasTop.Children.Add(circlePt);
            _targetConnectionPtLeft = lPt;



            // Draw circle at connect point (bottom side)
            Point bPt = new Point(point.X, point.Y);
            bPt.Y += projectBoxHeight;
            bPt.X = point.X + (projectBoxWidth / 2);

            circlePt = new Ellipse();
            brushCircle = new SolidColorBrush();
            brushCircle.Color = Colors.AliceBlue;
            circlePt.Fill = brushCircle;
            circlePt.StrokeThickness = 1;
            circlePt.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            circlePt.Width = 8;
            circlePt.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(circlePt, bPt.Y - 2.5);
            Canvas.SetLeft(circlePt, bPt.X - 5);
            DrawCanvasTop.Children.Add(circlePt);
            _targetConnectionPtBottom = bPt;


            // draw the remove button
            Button btn = new Button();
            btn.Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton");
            btn.Content = new MaterialDesignThemes.Wpf.PackIcon
            { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle };
            btn.Width = 25;
            btn.Height = 25;
            btn.Click += RemoveItem_Click;
            btn.Uid = "TARGET:";
            btn.Foreground = Brushes.Red;
            btn.Effect =
                new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                };
            Canvas.SetTop(btn, point.Y - btn.Height / 2);
            Canvas.SetLeft(btn, point.X + projectBoxWidth - btn.Width / 2);
            DrawCanvasTop.Children.Add(btn);
        }

        /// <summary>
        /// Draw the source box
        /// </summary>
        /// <param name="projectBoxWidth"></param>
        /// <param name="projectBoxHeight"></param>
        private void DrawSourceBox(int projectBoxWidth, int projectBoxHeight)
        {
            TextBlock text;
            Size sz;
            double additionalX;
            // ========================
            // Draw the Source Box
            // ========================
            Point point = new Point();
            point.X = (_boxWidth / 2) - (projectBoxWidth / 2);
            point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);

            // draw the 'Source' word
            text = new TextBlock();
            text.FontSize = 12;
            text.FontWeight = FontWeights.Bold;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.Width = _boxWidth;
            text.Text = "MANUSCRIPT";
            sz = DrawingUtils.MeasureString(text.Text, text);
            additionalX = (projectBoxWidth - sz.Width) / 2;
            Canvas.SetLeft(text, point.X + additionalX);
            var additionalY = (projectBoxHeight - sz.Height) / 3;
            Canvas.SetTop(text, point.Y + additionalY);

            Rectangle sourceRect = new Rectangle();
            sourceRect.Width = projectBoxWidth;
            sourceRect.Height = projectBoxHeight;
            sourceRect.Fill = Application.Current.FindResource("TealDarkBrush") as Brush;
            sourceRect.RadiusX = 3;
            sourceRect.RadiusY = 3;
            sourceRect.Effect =
                new DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.75
                };
            Canvas.SetTop(sourceRect, point.Y);
            Canvas.SetLeft(sourceRect, point.X);
            DrawCanvasTop.Children.Add(sourceRect);


            DrawCanvasTop.Children.Add(text);


            // Draw circle at connect point
            Point lPt = new Point(point.X, point.Y);
            lPt.X += projectBoxWidth;
            lPt.Y += projectBoxHeight / 2;

            Ellipse circlePt = new Ellipse();
            SolidColorBrush brushCircle = new SolidColorBrush();
            brushCircle.Color = Colors.AliceBlue;
            circlePt.Fill = brushCircle;
            circlePt.StrokeThickness = 1;
            circlePt.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            circlePt.Width = 8;
            circlePt.Height = 8;

            // How to set center of ellipse
            Canvas.SetTop(circlePt, lPt.Y - 2.5);
            Canvas.SetLeft(circlePt, lPt.X - 2.5);
            DrawCanvasTop.Children.Add(circlePt);
            _sourceConnectionPt = lPt;
        }

        #endregion

        public void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                var btn = sender as Button;
                if (btn.Uid.StartsWith("TARGET:"))
                {
                    _targetProject = null;
                }
                else if (btn.Uid.StartsWith("LWC:"))
                {
                    var name = btn.Uid.Substring(4);
                    var index = _lwcProjects.FindIndex(a => a.Name == name);
                    try
                    {
                        _lwcProjects.RemoveAt(index);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                    }

                }
                else if (btn.Uid.StartsWith("BT:"))
                {
                    var name = btn.Uid.Substring(3);
                    var index = _backTranslationProjects.FindIndex(a => a.Name == name);
                    try
                    {
                        _backTranslationProjects.RemoveAt(index);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                    }
                }
                else if (btn.Uid.StartsWith("IN:"))
                {
                    _interlinearizerProject = null;
                }
                SetProjects(_lwcProjects, _targetProject, _backTranslationProjects, _interlinearizerProject);
            }

            DrawTheCanvas();
        }


        public void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTheCanvas();
        }

        #endregion 

        public async void CreateNewProject(object obj)
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

        internal void SetProjects(List<ParatextProject> lwcProjects = null, 
            ParatextProject targetProject = null, 
            List<ParatextProject> backTranslationProjects = null, 
            ParatextProject _interlinearizerProject = null)
        {
            if (lwcProjects is not null)
            {
                _lwcProjects = new List<ParatextProject>(lwcProjects);
            }

            if (targetProject is not null)
            {
                _targetProject = targetProject;
            }

            if (backTranslationProjects is not null)
            {
                _backTranslationProjects = new List<ParatextProject>(backTranslationProjects);
            }

            if (ProjectName == "" || ProjectName is null)
            {
                ButtonEnabled = false;
                return;
            }

            // check to see if we have at least a target project
            if (_targetProject is null)
            {
                ButtonEnabled = false;
            }
            else
            {
                ButtonEnabled = true;
            }
        }

    }

    public class ParatextProjectDisplay : ParatextProject
    {
        private bool _inUse;
        public bool InUse
        {
            get => _inUse;
            set
            {
                _inUse = value;
                OnPropertyChanged(nameof(InUse));
            }
        }

    }

    public class DragAndDropManager
    {
        private CreateNewProjectsView _view;

        public DragAndDropManager(CreateNewProjectsView view)
        {
            
        }
        public void PreviewDrop(object sender, DragEventArgs e)
        {
            var s = e.Data;
        }

        public void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            var s = e.Device.Target;
        }

        public void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var _dragStartPoint = e.GetPosition(null);
        }
    }
}
