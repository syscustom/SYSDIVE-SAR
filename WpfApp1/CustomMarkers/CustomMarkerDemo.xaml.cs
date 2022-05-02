using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using System.Diagnostics;
using WpfApp1;

namespace Demo.WindowsPresentation.CustomMarkers
{
   /// <summary>
   /// Interaction logic for CustomMarkerDemo.xaml
   /// </summary>
   public partial class CustomMarkerDemo
   {
      Popup Popup;
      Label Label;
      GMapMarker Marker;
      frmNavigation MainWindow;

      public CustomMarkerDemo(frmNavigation window, GMapMarker marker, string title)
      {
         this.InitializeComponent();

         this.MainWindow = window;
         this.Marker = marker;
         

         Popup = new Popup();
         Label = new Label();

         this.Unloaded += new RoutedEventHandler(CustomMarkerDemo_Unloaded);
         this.Loaded += new RoutedEventHandler(CustomMarkerDemo_Loaded);
         this.SizeChanged += new SizeChangedEventHandler(CustomMarkerDemo_SizeChanged);
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

         lblName.Content = title;
        
         Popup.Child = Label;
      }


        public CustomMarkerDemo(GMapMarker marker, string title)
        {
            this.InitializeComponent();

            this.Marker = marker;


            Popup = new Popup();
            Label = new Label();

            //this.Unloaded += new RoutedEventHandler(CustomMarkerDemo_Unloaded);
            this.Loaded += new RoutedEventHandler(CustomMarkerDemo_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(CustomMarkerDemo_SizeChanged);
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

            lblName.Content = title;

            Popup.Child = Label;
        }

        void CustomMarkerDemo_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }

      void CustomMarkerDemo_Unloaded(object sender, RoutedEventArgs e)
      {
         this.Unloaded -= new RoutedEventHandler(CustomMarkerDemo_Unloaded);
         this.Loaded -= new RoutedEventHandler(CustomMarkerDemo_Loaded);
         this.SizeChanged-= new SizeChangedEventHandler(CustomMarkerDemo_SizeChanged);
         this.MouseMove -= new MouseEventHandler(CustomMarkerDemo_MouseMove);
         this.MouseLeftButtonUp -= new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonUp);
         this.MouseLeftButtonDown -= new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonDown);

         Popup.IsOpen = false;

         Marker.Shape = null;
         icon.Source = null;
         icon = null;
         Popup = null;
         Label = null;         
      }

      void CustomMarkerDemo_SizeChanged(object sender, SizeChangedEventArgs e)
      {
            Marker.Offset = new Point(-e.NewSize.Width / 2, (-e.NewSize.Height + 15) / 2);
            //Marker.Position
            //GMap.NET.GPoint gpoint = MainWindow.MainMap.FromLatLngToLocal(Marker.Position);
            //Popup.HorizontalOffset = gpoint.X;
            //Popup.VerticalOffset = gpoint.Y;
            //Popup.IsOpen = true;
        }

      void CustomMarkerDemo_MouseMove(object sender, MouseEventArgs e)
      {
         //if(e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
         //{
         //   Point p = e.GetPosition(MainWindow.MainMap);
         //   Marker.Position = MainWindow.MainMap.FromLocalToLatLng((int) (p.X), (int) (p.Y));
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
   }
}