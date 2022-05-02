using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System;
using GMap.NET.WindowsPresentation;
using WpfApp1;

namespace Demo.WindowsPresentation.CustomMarkers
{
   /// <summary>
   /// Interaction logic for CustomMarkerDemo.xaml
   /// </summary>
   public partial class WayPointMarkerActive
   {
      Popup Popup;
      Label Label;
      GMapMarker Marker;
      
      frmNavigation MainWindow;

      public WayPointMarkerActive(frmNavigation window, GMapMarker marker, string title)
      {
         this.InitializeComponent();

         this.MainWindow = window;
         this.Marker = marker;

         Popup = new Popup();
         Label = new Label();

         this.Loaded += new RoutedEventHandler(WayPointMarkerActive_Loaded);
         this.SizeChanged += new SizeChangedEventHandler(WayPointMarkerActive_SizeChanged);
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

        public WayPointMarkerActive(GMapMarker marker, string title)
        {
            this.InitializeComponent();
            this.Marker = marker;

            Popup = new Popup();
            Label = new Label();

            this.Loaded += new RoutedEventHandler(WayPointMarkerActive_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(WayPointMarkerActive_SizeChanged);
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


        void WayPointMarkerActive_Loaded(object sender, RoutedEventArgs e)
      {
         if(icon.Source.CanFreeze)
         {
            icon.Source.Freeze();
         }
      }

      void WayPointMarkerActive_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         Marker.Offset = new Point(-e.NewSize.Width/2, -e.NewSize.Height/2);
      }

      void CustomMarkerDemo_MouseMove(object sender, MouseEventArgs e)
      {
         ///if(e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
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
         Popup.IsOpen = true;
      }
   }
}