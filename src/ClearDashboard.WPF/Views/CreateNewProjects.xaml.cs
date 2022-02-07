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
        private List<Point> _LWCconnectionPtsRight = new List<Point>();
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
            DrawTheCanvas();


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
                        text.Text = "SOURCE";
                        break;
                    case eDropZones.LWC:
                        text.Text = "LWC";
                        _LWCproject.Add(project);
                        break;
                    case eDropZones.Target:
                        text.Text = "TARGET";
                        _targetProject = project;
                        break;
                    case eDropZones.BackTranslation:
                        text.Text = "BACKTRANSLATION";
                        _BackTransProject.Add(project);
                        break;

                }
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                Canvas.SetTop(text, p.Y);
                Canvas.SetLeft(text, p.X);
                DrawCanvas.Children.Add(text);

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
                return eDropZones.LWC;
            }
            if (pt.X > _boxWidth * 2 && pt.X < _boxWidth * 3)
            {
                return eDropZones.Target;
            }
            return eDropZones.BackTranslation;
        }


        private void DrawTheCanvas()
        {
            _boxWidth = DrawCanvas.ActualWidth / 4; //width of each box
            _boxHeight = DrawCanvas.ActualHeight;  // Height of each box

            const int iHeaderHeight = 50;
            const int cornerRadius = 8;
            int projectBoxWidth = (int)(_boxWidth * 0.5);
            const int projectBoxHeight = 40;

            for (int i = 0; i < Application.Current.Resources.Count; i++)
            {
                Debug.WriteLine(Application.Current.Resources[i].ToString());
            }

            DrawCanvas.Children.Clear();
            // ========================
            // BUILD THE HEADERS AND DROP BOXES
            // ========================
            for (int i = 0; i < 4; i++)
            {
                TextBlock text = new TextBlock();
                text.FontSize = 20;
                text.FontWeight = FontWeights.Bold;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.Width = _boxWidth;
                
                Rectangle rect = new Rectangle();
                rect.Height = DrawCanvas.ActualHeight - iHeaderHeight;
                rect.Width = _boxWidth;
                rect.RadiusX = cornerRadius;
                rect.RadiusY = cornerRadius;
                rect.Opacity = 0.75;

                Point point = new Point();
                point.Y = iHeaderHeight;
                switch (i)
                {
                    case 0:
                        rect.Fill = Application.Current.FindResource("TealLightBrush") as Brush;
                        point.X = 0;
                        text.Text = "Source";
                        break;
                    case 1:
                        rect.Fill = Application.Current.FindResource("PurpleLightBrush") as Brush;
                        point.X = _boxWidth;
                        text.Text = "LWC(s)";
                        break;
                    case 2:
                        rect.Fill = Application.Current.FindResource("OrangeLightBrush") as Brush;
                        point.X = _boxWidth * 2;
                        text.Text = "Target";
                        break;
                    case 3:
                        rect.Fill = Application.Current.FindResource("BlueLightBrush") as Brush;
                        point.X = _boxWidth * 3;
                        text.Text = "Back Translation";
                        break;
                }

                Canvas.SetLeft(rect, point.X);
                Canvas.SetTop(rect, point.Y);
                DrawCanvas.Children.Add(rect);

                // center the text in the block
                Size sz = DrawingUtils.MeasureString(text.Text, text);
                var additionalX = (_boxWidth - sz.Width)/2;
                Canvas.SetLeft(text, point.X + additionalX);
                Canvas.SetTop(text, 10);
                DrawCanvas.Children.Add(text);

                // ========================
                // Draw the Source Box
                // ========================
                point.X = (_boxWidth / 2) - (projectBoxWidth / 2);
                point.Y = (_boxHeight / 2) - (projectBoxHeight / 2);

                Rectangle sourceRect = new Rectangle();
                sourceRect.Width = projectBoxWidth;
                sourceRect.Height = projectBoxHeight;
                sourceRect.Fill = Application.Current.FindResource("TealLightBrush") as Brush;
                sourceRect.Stroke = Application.Current.FindResource("TealDarkBrush") as Brush;
                sourceRect.StrokeThickness = 2;
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

                // draw the 'Source' word
                text = new TextBlock();
                text.FontSize = 20;
                text.FontWeight = FontWeights.Bold;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.Width = _boxWidth;
                text.Text = "SOURCE";
                sz = DrawingUtils.MeasureString(text.Text, text);
                additionalX = (projectBoxWidth - sz.Width) / 2;
                Canvas.SetLeft(text, point.X + additionalX);
                var additionalY = (projectBoxHeight - sz.Height) / 3;
                Canvas.SetTop(text, point.Y + additionalY);
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

            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTheCanvas();
        }
    }
}
