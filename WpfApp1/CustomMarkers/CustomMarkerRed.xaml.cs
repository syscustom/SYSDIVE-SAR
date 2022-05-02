using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using WpfApp1;

namespace Demo.WindowsPresentation.CustomMarkers
{
   /// <summary>
   /// Interaction logic for CustomMarkerDemo.xaml
   /// </summary>
   public partial class CustomMarkerRed
   {
      Popup Popup;
      Label Label;
      GMapMarker Marker;
      
      frmNavigation MainWindow;

      public CustomMarkerRed(frmNavigation window, GMapMarker marker, string title)
      {
         this.InitializeComponent();

         this.MainWindow = window;
         this.Marker = marker;

         Popup = new Popup();
         Label = new Label();

         this.Loaded += new RoutedEventHandler(CustomMarkerDemo_Loaded);
         this.SizeChanged += new SizeChangedEventHandler(CustomMarkerDemo_SizeChanged);
         this.MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
         this.MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);
         this.MouseMove += new MouseEventHandler(CustomMarkerDemo_MouseMove);
         this.MouseLeftButtonUp += new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonUp);
         this.MouseLeftButtonDown += new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonDown);

         Popup.Placement = PlacementMode.Mouse;
         {
            Label.Background = Brushes.Blue;
            Label.Foreground = Brushes.White;
            Label.BorderBrush = Brushes.WhiteSmoke;
            Label.BorderThickness = new Thickness(2);
            Label.Padding = new Thickness(5);
            Label.FontSize = 22;
            Label.Content = title;
         }
         Popup.Child = Label;
      }

        public CustomMarkerRed(GMapMarker marker, string title)
        {
            this.InitializeComponent();

            this.Marker = marker;

            Popup = new Popup();
            Label = new Label();

            this.Loaded += new RoutedEventHandler(CustomMarkerDemo_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(CustomMarkerDemo_SizeChanged);
            this.MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonDown);

            
            Popup.Placement = PlacementMode.Mouse;
            {
                Label.Background = Brushes.Blue;
                Label.Foreground = Brushes.White;
                Label.BorderBrush = Brushes.WhiteSmoke;
                Label.BorderThickness = new Thickness(2);
                Label.Padding = new Thickness(5);
                Label.FontSize = 22;
                Label.Content = title;
            }
            Popup.Child = Label;
            
        }


        void CustomMarkerDemo_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }

            Rotate(0);
        }

      void CustomMarkerDemo_SizeChanged(object sender, SizeChangedEventArgs e)
      {
            //Marker.Offset = new Point(-e.NewSize.Width/2, -e.NewSize.Height);
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

      void CustomMarkerDemo_MouseMove(object sender, MouseEventArgs e)
      {
         //if(e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
         //{
         //   Point p = e.GetPosition(MainWindow.MainMap);
         //   Marker.Position = MainWindow.MainMap.FromLocalToLatLng((int) p.X, (int) p.Y);
         //}
      }

      void CustomMarkerDemo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         if(!IsMouseCaptured)
         {
            Mouse.Capture(this);
         }
      }

      void CustomMarkerDemo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         if(IsMouseCaptured)
         {
            Mouse.Capture(null);
         }
      }

      void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
      {
         Marker.ZIndex -= 10000;
         Popup.IsOpen = false;
      }

      void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
      {
         Marker.ZIndex += 10000;
         Popup.IsOpen = false;
      }

        public void Rotate(double _angel)
        {
            /*
            _angel += 180;

            double width = icon.ActualWidth;
            double height = icon.ActualHeight;
            icon.LayoutTransform = new RotateTransform(_angel, width / 2, height / 2);
            */

            _angel += 180;

            double width = icon.ActualWidth;
            double height = icon.ActualHeight;

            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = -1;
            transformGroup.Children.Add(scaleTransform);

            RotateTransform rotateTransform = new RotateTransform(_angel);
            transformGroup.Children.Add(rotateTransform);
            icon.RenderTransform = transformGroup;
            icon.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }
}