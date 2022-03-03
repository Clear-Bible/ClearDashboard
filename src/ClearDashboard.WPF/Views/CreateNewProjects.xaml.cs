using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels;
using MvvmHelpers;
using Newtonsoft.Json;

namespace ClearDashboard.Wpf.Views
{
    public partial class CreateNewProjects : Page
    {
        CreateNewProjectsViewModel _vm;

        private bool _IsDraging = false;
        private Point _DragStartPoint;

        private double _boxWidth;
        private double _boxHeight;

        ParatextProject _targetProject;
        private List<ParatextProject> _LWCproject = new List<ParatextProject>();
        private List<ParatextProject> _BackTransProject = new List<ParatextProject>();

        // variables that hold the arrow connection points
        private Point _SourceConnectionPt = new Point(0, 0);
        private List<Point> _LWCconnectionPtsLeft = new List<Point>();
        private Point _TargetConnectionPtLeft = new Point(0, 0);
        private Point _TargetConnectionPtRight = new Point(0, 0);
        private List<Point> _BTconnectionPtsLeft = new List<Point>();
        private List<Point> _BTconnectionPtsRight = new List<Point>();

        private enum eDropZones
        {
            Source,
            LWC,
            Target,
            BackTranslation
        }


        public CreateNewProjects()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DrawTheTopCanvas();


            if (this.DataContext is CreateNewProjectsViewModel)
            {
                _vm = (CreateNewProjectsViewModel)this.DataContext;

                _vm.Init();
            }
        }

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

        private void Canvas_PreviewDrop(object sender, DragEventArgs e)
        {
            var data = e.Data;
            Point p = e.GetPosition(DrawCanvas);

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
                        //case eDropZones.BackTranslation:
                        //    text.Text = "BACKTRANSLATION";
                        //    if (project.ProjectType == ParatextProject.eProjectType.BackTranslation)
                        //    {
                        //        t = _BackTransProject.Where(p => p.Name == project.Name).ToList();
                        //        if (t.Count() == 0)
                        //        {
                        //            _BackTransProject.Add(project);
                        //        }
                        //    }
                        //    break;
                }
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                Canvas.SetTop(text, p.Y);
                Canvas.SetLeft(text, p.X);
                DrawCanvas.Children.Add(text);

                _vm.SetProjects(_LWCproject, _targetProject, _BackTransProject);

                DrawTheTopCanvas();
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
            //if (pt.X > _boxWidth * 2 && pt.X < _boxWidth * 3)
            //{
            //    return eDropZones.LWC;
            //}
            return eDropZones.LWC;
        }


        private void DrawTheTopCanvas()
        {
            _boxWidth = DrawCanvas.ActualWidth / 3; //width of each box
            _boxHeight = DrawCanvas.ActualHeight;  // Height of each box

            const int cornerRadius = 8;
            int projectBoxWidth = (int)(_boxWidth * 0.33);
            const int projectBoxHeight = 40;

            DrawCanvas.Children.Clear();
            // ========================
            // BUILD THE HEADERS AND DROP BOXES
            // ========================
            for (int i = 0; i < 3; i++)
            {
                TextBlock text = new TextBlock();
                text.FontSize = 20;
                text.FontWeight = FontWeights.Bold;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.Width = _boxWidth;

                Rectangle rect = new Rectangle();
                rect.Height = DrawCanvas.ActualHeight;
                rect.Width = _boxWidth - 2;
                rect.RadiusX = cornerRadius;
                rect.RadiusY = cornerRadius;
                rect.StrokeThickness = 3;
                //rect.Opacity = 0.5;
                //rect.Fill = Application.Current.FindResource("MaterialDesignCardBackground") as Brush;
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
                        rect.Stroke = Application.Current.FindResource("TealDarkBrush") as Brush;
                        text.Foreground = Application.Current.FindResource("TealDarkBrush") as Brush;
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
                        //case 3:
                        //    rect.Stroke = Application.Current.FindResource("TealDarkBrush") as Brush;
                        //    text.Foreground = Application.Current.FindResource("TealDarkBrush") as Brush;
                        //    point.X = _boxWidth * 3;
                        //    text.Text = "Back Translation";
                        //    break;
                }

                Canvas.SetLeft(rect, point.X);
                Canvas.SetTop(rect, point.Y);
                DrawCanvas.Children.Add(rect);

                // center the text in the block
                Size sz = DrawingUtils.MeasureString(text.Text, text);
                var additionalX = (_boxWidth - sz.Width) / 2;
                Canvas.SetLeft(text, point.X + additionalX);
                Canvas.SetTop(text, 10);
                DrawCanvas.Children.Add(text);
            }


            // ========================
            // Draw the Source Box
            // ========================
            DrawSourceBox(projectBoxWidth, projectBoxHeight);

            // ========================
            // Draw the Target Box
            // ========================
            if (_targetProject != null)
            {
                DrawTargetBox(projectBoxWidth, projectBoxHeight);
            }

            // ========================
            // Draw the LWC Box(es)
            // ========================
            if (_LWCproject.Count > 0)
            {
                DrawLWCBoxes(projectBoxWidth, projectBoxHeight);
            }

            //// ========================
            //// Draw the Back Translation Box(es)
            //// ========================
            //if (_BackTransProject.Count > 0)
            //{
            //    DrawBackTransBoxes(projectBoxWidth, projectBoxHeight);
            //}

            // ========================
            // Draw the ConnectionLines
            // ========================
            // connect target to source
            if (_targetProject != null)
            {
                var leftPt = new Point(_SourceConnectionPt.X + 6, _SourceConnectionPt.Y + 2);
                var rightPt = new Point(_TargetConnectionPtLeft.X - 6, _TargetConnectionPtLeft.Y + 2);
                Point controlPtSource = new Point(_SourceConnectionPt.X + 20, _SourceConnectionPt.Y);
                Point controlPtTarget = new Point(_TargetConnectionPtLeft.X - 20, _TargetConnectionPtLeft.Y);
                Path path = GenerateLine(leftPt, rightPt, controlPtSource, controlPtTarget);
                DrawCanvas.Children.Add(path);
            }

            // some LWC's so connect to Target
            for (int i = 0; i < _LWCproject.Count; i++)
            {
                if (_targetProject != null)
                {
                    var leftPt = new Point(_LWCconnectionPtsLeft[i].X + 6, _LWCconnectionPtsLeft[i].Y + 2);
                    var rightPt = new Point(_TargetConnectionPtRight.X - 6, _TargetConnectionPtRight.Y + 2);
                    // draw from right LWC to Target
                    Point controlPtSource = new Point(_LWCconnectionPtsLeft[i].X + 20, _LWCconnectionPtsLeft[i].Y);
                    Point controlPtTarget = new Point(_TargetConnectionPtRight.X - 20, _TargetConnectionPtRight.Y);
                    Path path = GenerateLine(leftPt, rightPt, controlPtSource, controlPtTarget);
                    DrawCanvas.Children.Add(path);
                }
            }




            // connection lines between target and BTs
            if (_targetProject != null && _BackTransProject.Count > 0)
            {
                // 
                for (int i = 0; i < _BackTransProject.Count; i++)
                {
                    var leftPt = new Point(_TargetConnectionPtRight.X + 6, _TargetConnectionPtRight.Y + 2);
                    var rightPt = new Point(_BTconnectionPtsLeft[i].X - 6, _BTconnectionPtsLeft[i].Y + 2);
                    Point controlPtSource = new Point(_TargetConnectionPtRight.X + 20, _TargetConnectionPtRight.Y);
                    Point controlPtTarget = new Point(_BTconnectionPtsLeft[i].X - 20, _BTconnectionPtsLeft[i].Y);
                    Path path = GenerateLine(leftPt, rightPt, controlPtSource, controlPtTarget);
                    DrawCanvas.Children.Add(path);
                }
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

        /// <summary>
        /// Draw the LWC boxes
        /// </summary>
        /// <param name="projectBoxWidth"></param>
        /// <param name="projectBoxHeight"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DrawBackTransBoxes(int projectBoxWidth, int projectBoxHeight)
        {
            _BTconnectionPtsRight.Clear();
            _BTconnectionPtsLeft.Clear();

            const double separation = 75;
            double offset = 0;
            if (_BackTransProject.Count > 1)
            {
                offset = _BackTransProject.Count * separation / 2;
            }

            for (int i = 0; i < _BackTransProject.Count; i++)
            {
                // ========================
                // Draw the Back Translation Boxes
                // ========================
                Point point = new Point();
                point.X = (_boxWidth / 2) - (projectBoxWidth / 2) + (projectBoxWidth * 6);
                point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset;

                Rectangle targetRect = new Rectangle();
                targetRect.Width = projectBoxWidth;
                targetRect.Height = projectBoxHeight;
                targetRect.Fill = Application.Current.FindResource("TealMidBrush") as Brush;
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
                //targetRect.Fill = Application.Current.FindResource("TealLightBrush") as Brush;
                //targetRect.Stroke = Application.Current.FindResource("TealDarkBrush") as Brush;
                //targetRect.StrokeThickness = 2;
                //targetRect.Effect =
                //    new DropShadowEffect
                //    {
                //        BlurRadius = 5,
                //        ShadowDepth = 2,
                //        Opacity = 0.75
                //    };
                Canvas.SetTop(targetRect, point.Y);
                Canvas.SetLeft(targetRect, point.X);
                DrawCanvas.Children.Add(targetRect);

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
                DrawCanvas.Children.Add(text);


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
                DrawCanvas.Children.Add(circlePt);
                _BTconnectionPtsRight.Add(new Point(rPt.X, rPt.Y));

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
                DrawCanvas.Children.Add(circlePt);
                _BTconnectionPtsLeft.Add(new Point(lPt.X, lPt.Y));

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
                DrawCanvas.Children.Add(btn);

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
                if (_LWCproject.Count > 0)
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset + 30;
                }
                else
                {
                    point.Y = (_boxHeight / 2) - (projectBoxHeight / 2) + offset;
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
                DrawCanvas.Children.Add(targetRect);

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
                DrawCanvas.Children.Add(text);

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
                DrawCanvas.Children.Add(circlePt);
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
                DrawCanvas.Children.Add(btn);

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
            DrawCanvas.Children.Add(targetRect);

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
            DrawCanvas.Children.Add(text);


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
            DrawCanvas.Children.Add(circlePt);
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
            DrawCanvas.Children.Add(circlePt);
            _TargetConnectionPtLeft = lPt;

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
            DrawCanvas.Children.Add(btn);
        }

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
                _vm.SetProjects(_LWCproject, _targetProject, _BackTransProject);
            }

            DrawTheTopCanvas();
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
            sourceRect.Fill = Application.Current.FindResource("TealMidBrush") as Brush;
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
            DrawCanvas.Children.Add(sourceRect);


            DrawCanvas.Children.Add(text);


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
            DrawCanvas.Children.Add(circlePt);
            _SourceConnectionPt = lPt;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTheTopCanvas();
        }
    }


}
