using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace ClearDashboard.Wpf.Views
{
    public partial class CreateNewProjectsView : Page
    {
        #region props

        CreateNewProjectsViewModel _vm;

        private bool _IsDraging = false;
        private Point _DragStartPoint;

        private double _boxWidth;
        private double _boxHeight;

        private ParatextProject _targetProject;
        private List<ParatextProject> _LWCproject = new List<ParatextProject>();
        private List<ParatextProject> _BackTransProject = new List<ParatextProject>();
        private ParatextProject _interlinearizerProject;

        // variables that hold the arrow connection points
        private Point _SourceConnectionPt = new Point(0, 0);
        private List<Point> _LWCconnectionPtsLeft = new List<Point>();
        private Point _TargetConnectionPtLeft = new Point(0, 0);
        private Point _TargetConnectionPtRight = new Point(0, 0);
        private Point _TargetConnectionPtBottom = new Point(0, 0);
        private Point _INconnectionPtsLeft = new Point(0, 0);

        private enum eDropZones
        {
            Source,
            LWC,
            Target,
            BackTranslation,
            Interlinearizer,
            Blank
        }

        #endregion


        #region Startup

        public CreateNewProjectsView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DrawTheCanvas();

            // NG:  GERFEN - move this call to CreateNewProjectViewModel.OnViewAttached();
            //if (this.DataContext is CreateNewProjectsViewModel)
            //{
            //    _vm = (CreateNewProjectsViewModel)this.DataContext;

            //    //_vm.Init();
            //}
        }

        #endregion

        #region Methods

        private void listview_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _DragStartPoint = e.GetPosition(null);
        }

        private void listview_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var lv = sender as ListView;

            if (lv.SelectedItem == null) return;

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDraging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _DragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _DragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _IsDraging = true;
                    DataObject data = new DataObject(typeof(ParatextProject), lv.SelectedItem);
                    DragDropEffects de = DragDrop.DoDragDrop(lv, data, DragDropEffects.Copy);
                    _IsDraging = false;
                }
            }
        }

        private void Canvas_PreviewDropTop(object sender, DragEventArgs e)
        {
            var data = e.Data;
            Point p = e.GetPosition(DrawCanvasTop);

            var dropZone = GetDropZone(p);

            if (data.GetDataPresent(typeof(ParatextProject)))
            {
                var project = data.GetData(typeof(ParatextProject)) as ParatextProject;

                TextBlock text = new TextBlock();

                switch (dropZone)
                {
                    case eDropZones.Source:
                        text.Text = "MANUSCRIPT";
                        break;
                    case eDropZones.LWC:
                        text.Text = "LWC";
                        var t = _LWCproject.Where(p => p.Name == project.Name).ToList();
                        if (t.Count() == 0)
                        {
                            // only allow it to be added once
                            _LWCproject.Add(project);
                        }
                        break;
                    case eDropZones.Target:
                        text.Text = "TARGET";
                        if (project.ProjectType == ParatextProject.eProjectType.Standard)
                        {
                            _targetProject = project;

                            // look for linked back translations
                            var btProjs = _vm.ParatextProjects.Where(p => p.TranslationInfo?.projectGuid == project.Guid).ToList();
                            foreach (var btProject in btProjs)
                            {
                                t = _BackTransProject.Where(p => p.Name == btProject.Name).ToList();
                                if (t.Count() == 0)
                                {
                                    _BackTransProject.Add(btProject);
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
                _vm.SetProjects(_LWCproject, _targetProject, _BackTransProject, _interlinearizerProject);

                DrawTheCanvas();
            }
        }

        private void Canvas_PreviewDropBottom(object sender, DragEventArgs e)
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
                    case eDropZones.BackTranslation:
                        text.Text = "BACK TRANSLATION";
                        // look for linked back translations
                        var t = _BackTransProject.Where(p => p.Name == project.Name).ToList();
                        if (t.Count() == 0)
                        {
                            _BackTransProject.Add(project);
                        }
                        break;
                    case eDropZones.Blank:
                        break;
                    case eDropZones.Interlinearizer:
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
                _vm.SetProjects(_LWCproject, _targetProject, _BackTransProject, _interlinearizerProject);

                DrawTheCanvas();
            }
        }
        private eDropZones GetDropZone(Point pt)
        {
            if (pt.X < _boxWidth)
            {
                return eDropZones.Source;
            }
            if (pt.X > _boxWidth && pt.X < _boxWidth * 2)
            {
                return eDropZones.Target;
            }
            return eDropZones.LWC;
        }

        private eDropZones GetDropZoneBottom(Point pt)
        {
            if (pt.X < _boxWidth)
            {
                return eDropZones.BackTranslation;
            }
            if (pt.X > _boxWidth && pt.X < _boxWidth * 2)
            {
                return eDropZones.Blank;
            }
            return eDropZones.Interlinearizer;
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
            if (_LWCproject.Count > 0)
            {
                DrawLWCBoxes(projectBoxWidth, projectBoxHeight);
            }

            // =============================================
            // Draw the Back Translation Box(es) Contents
            // =============================================
            if (_BackTransProject.Count > 0)
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
                var leftPt = new Point(_SourceConnectionPt.X + 6, _SourceConnectionPt.Y + 2);
                var rightPt = new Point(_TargetConnectionPtLeft.X - 6, _TargetConnectionPtLeft.Y + 2);
                Point controlPtSource = new Point(_SourceConnectionPt.X + 20, _SourceConnectionPt.Y);
                Point controlPtTarget = new Point(_TargetConnectionPtLeft.X - 20, _TargetConnectionPtLeft.Y);
                Path path = GenerateLine(leftPt, rightPt, controlPtSource, controlPtTarget);
                DrawCanvasTop.Children.Add(path);
            }

            // some LWC's so connect to Target
            for (int i = 0; i < _LWCproject.Count; i++)
            {
                if (_targetProject != null)
                {
                    var leftPt = new Point(_LWCconnectionPtsLeft[i].X + 6, _LWCconnectionPtsLeft[i].Y + 2);
                    var rightPt = new Point(_TargetConnectionPtRight.X + 6, _TargetConnectionPtRight.Y + 2);
                    // draw from right LWC to Target
                    Point controlPtSource = new Point(_LWCconnectionPtsLeft[i].X - 20, _LWCconnectionPtsLeft[i].Y);
                    Point controlPtTarget = new Point(_TargetConnectionPtRight.X + 20, _TargetConnectionPtRight.Y);
                    Path path = GenerateLine(leftPt, rightPt, controlPtSource, controlPtTarget);
                    DrawCanvasTop.Children.Add(path);
                }
            }


            // connection lines between target and Intelinear
            if (_targetProject != null && _interlinearizerProject != null)
            {
                var bottomPt = new Point(_TargetConnectionPtBottom.X, _TargetConnectionPtBottom.Y + 6);
                var leftPt = new Point(_INconnectionPtsLeft.X - 6, _INconnectionPtsLeft.Y + 10 + DrawCanvasTop.ActualHeight);
                Point controlPtSource = new Point(_TargetConnectionPtBottom.X, _TargetConnectionPtBottom.Y + (DrawCanvasTop.ActualHeight / 3));
                Point controlPtTarget = new Point(_INconnectionPtsLeft.X - (DrawCanvasTop.ActualHeight / 3), _INconnectionPtsLeft.Y + DrawCanvasTop.ActualHeight);
                Path path = GenerateLine(bottomPt,  leftPt, controlPtSource, controlPtTarget);
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
            Point rPt = new Point(point.X, point.Y + offset);
            rPt.X += projectBoxWidth;
            rPt.Y += projectBoxHeight / 2 - offset;

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
            DrawCanvasBottom.Children.Add(circlePt);

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
            DrawCanvasBottom.Children.Add(circlePt);
            _INconnectionPtsLeft = new Point(lPt.X, lPt.Y);

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
            if (_BackTransProject.Count > 1)
            {
                separation = 30;
                offset = _BackTransProject.Count * separation / 2;
            }

            for (int i = 0; i < _BackTransProject.Count; i++)
            {
                // ========================
                // Draw the Back Translation Boxes
                // ========================
                Point point = new Point();
                point.X = (_boxWidth / 2) - (projectBoxWidth / 2);
                if (_BackTransProject.Count > 1)
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
                text.Text = _BackTransProject[i].Name;
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
                btn.Uid = "BT:" + _BackTransProject[i].Name;
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
            _LWCconnectionPtsLeft.Clear();

            double separation = 0;
            double offset = 0;
            if (_LWCproject.Count > 1)
            {
                separation = 30;
                offset = _LWCproject.Count * separation / 2;
            }

            for (int i = 0; i < _LWCproject.Count; i++)
            {
                // ========================
                // Draw the LWC Boxes
                // ========================
                Point point = new Point();
                point.X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 6);
                if (_LWCproject.Count > 1)
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
                text.Text = _LWCproject[i].Name;
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
                _LWCconnectionPtsLeft.Add(new Point(lPt.X - 15, lPt.Y - 2.5));

                // draw the remove button
                Button btn = new Button();
                btn.Style = (Style)Application.Current.TryFindResource("MaterialDesignIconButton");
                btn.Content = new MaterialDesignThemes.Wpf.PackIcon
                { Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle };
                btn.Width = 25;
                btn.Height = 25;
                btn.Click += RemoveItem_Click;
                btn.Uid = "LWC:" + _LWCproject[i].Name;
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
            _TargetConnectionPtRight = rPt;



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
            _TargetConnectionPtLeft = lPt;



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
            _TargetConnectionPtBottom = bPt;


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
            _SourceConnectionPt = lPt;
        }

        #endregion

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
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
                    var index = _LWCproject.FindIndex(a => a.Name == name);
                    try
                    {
                        _LWCproject.RemoveAt(index);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                    }

                }
                else if (btn.Uid.StartsWith("BT:"))
                {
                    var name = btn.Uid.Substring(3);
                    var index = _BackTransProject.FindIndex(a => a.Name == name);
                    try
                    {
                        _BackTransProject.RemoveAt(index);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                    }
                }
                _vm.SetProjects(_LWCproject, _targetProject, _BackTransProject, _interlinearizerProject);
            }

            DrawTheCanvas();
        }


        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTheCanvas();
        }

        #endregion
    }


}
