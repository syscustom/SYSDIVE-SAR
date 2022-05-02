
namespace GMap.NET.WindowsPresentation
{
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Windows;
   using System.Windows.Controls;
   using GMap.NET;
   using System.Windows.Media;
   using System.Diagnostics;
   using System.Windows.Shapes;
   using System;

   /// <summary>
   /// GMap.NET marker
   /// </summary>
   public class GMapMarker : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      protected void OnPropertyChanged(string name)
      {
         if(PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }

      protected void OnPropertyChanged(PropertyChangedEventArgs name)
      {
         if(PropertyChanged != null)
         {
            PropertyChanged(this, name);
         }
      }

      UIElement shape;
      static readonly PropertyChangedEventArgs Shape_PropertyChangedEventArgs = new PropertyChangedEventArgs("Shape");

      /// <summary>
      /// marker visual
      /// </summary>
      public UIElement Shape
      {
         get
         {
            return shape;
         }
         set
         {
            if(shape != value)
            {
               shape = value;
               OnPropertyChanged(Shape_PropertyChangedEventArgs);

               UpdateLocalPosition();
            }
         }
      }

      private PointLatLng positiongcj02 = new PointLatLng();
      private PointLatLng positionwgs84= new PointLatLng();
        /// <summary>
        /// coordinate of marker
        /// </summary>
        public PointLatLng Position //wgs84
        {
            get
            {
                return positionwgs84;
                //return position;
            }
            set
            {
                if(positionwgs84 != value)
                {
                    positionwgs84 = value;
                    double[] latlng = wgs84togcj02(positionwgs84.Lat, positionwgs84.Lng);
                    positiongcj02 = new PointLatLng(latlng[0], latlng[1]);
                    UpdateLocalPosition();
                }
            }
        }

        public PointLatLng PositionGCJ02
        {
            get
            {
                return positiongcj02;
            }
        }

            GMapControl map;

      /// <summary>
      /// the map of this marker
      /// </summary>
      public GMapControl Map
      {
         get
         {
            if(Shape != null && map == null)
            {
               DependencyObject visual = Shape;
               while(visual != null && !(visual is GMapControl))
               {
                  visual = VisualTreeHelper.GetParent(visual);
               }

               map = visual as GMapControl;
            }

            return map;
         }
          internal set
          {
              map = value;
          }
      }

      /// <summary>
      /// custom object
      /// </summary>
      public object Tag;

      System.Windows.Point offset;
      /// <summary>
      /// offset of marker
      /// </summary>
      public System.Windows.Point Offset
      {
         get
         {
            return offset;
         }
         set
         {
            if(offset != value)
            {
               offset = value;
               UpdateLocalPosition();
            }
         }
      }

      int localPositionX;
      static readonly PropertyChangedEventArgs LocalPositionX_PropertyChangedEventArgs = new PropertyChangedEventArgs("LocalPositionX");

      /// <summary>
      /// local X position of marker
      /// </summary>
      public int LocalPositionX
      {
         get
         {
            return localPositionX;
         }
         internal set
         {
            if(localPositionX != value)
            {
               localPositionX = value;
               OnPropertyChanged(LocalPositionX_PropertyChangedEventArgs);
            }
         }
      }

      int localPositionY;
      static readonly PropertyChangedEventArgs LocalPositionY_PropertyChangedEventArgs = new PropertyChangedEventArgs("LocalPositionY");

      /// <summary>
      /// local Y position of marker
      /// </summary>
      public int LocalPositionY
      {
         get
         {
            return localPositionY;
         }
         internal set
         {
            if(localPositionY != value)
            {
               localPositionY = value;
               OnPropertyChanged(LocalPositionY_PropertyChangedEventArgs);
            }
         }
      }

      int zIndex;
      static readonly PropertyChangedEventArgs ZIndex_PropertyChangedEventArgs = new PropertyChangedEventArgs("ZIndex");

      /// <summary>
      /// the index of Z, render order
      /// </summary>
      public int ZIndex
      {
         get
         {
            return zIndex;
         }
         set
         {
            if(zIndex != value)
            {
               zIndex = value;
               OnPropertyChanged(ZIndex_PropertyChangedEventArgs);
            }
         }
      }

      public GMapMarker(PointLatLng pos)
      {
         Position = pos;
      }

      internal GMapMarker()
      {
      }

      /// <summary>
      /// calls Dispose on shape if it implements IDisposable, sets shape to null and clears route
      /// </summary>
      public virtual void Clear()
      {
         var s = (Shape as IDisposable);
         if(s != null)
         {
            s.Dispose();
            s = null;
         }
         Shape = null;
      }

      /// <summary>
      /// updates marker position, internal access usualy
      /// </summary>
      void UpdateLocalPosition()
      {
         if(Map != null)
         {
            //GPoint p = Map.FromLatLngToLocal(Position);
            GPoint p = Map.FromLatLngToLocal(PositionGCJ02);
            p.Offset(-(long)Map.MapTranslateTransform.X, -(long)Map.MapTranslateTransform.Y);

            LocalPositionX = (int)(p.X + (long)(Offset.X));
            LocalPositionY = (int)(p.Y + (long)(Offset.Y));
         }
      }

      /// <summary>
      /// forces to update local marker  position
      /// dot not call it if you don't really need to ;}
      /// </summary>
      /// <param name="m"></param>
      internal void ForceUpdateLocalPosition(GMapControl m)
      {
         if(m != null)
         {
            map = m;
         }
         UpdateLocalPosition();
      }


        // π
        double pi = 3.1415926535897932384626;
        // 长半轴
        double a = 6378245.0;
        // 扁率
        double ee = 0.00669342162296594323;

        /**
	 * WGS84转GCJ02(火星坐标系)
	 * 
	 * @param lng WGS84坐标系的经度
	 * @param lat WGS84坐标系的纬度
	 * @return 火星坐标数组
	 */
        private double[] wgs84togcj02(double lat, double lng)
        {
            if (out_of_china(lng, lat))
            {
                return new double[] { lng, lat };
            }
            double dlat = transformlat(lng - 105.0, lat - 35.0);
            double dlng = transformlng(lng - 105.0, lat - 35.0);
            double radlat = lat / 180.0 * pi;
            double magic = Math.Sin(radlat);
            magic = 1 - ee * magic * magic;
            double sqrtmagic = Math.Sqrt(magic);
            dlat = (dlat * 180.0) / ((a * (1 - ee)) / (magic * sqrtmagic) * pi);
            dlng = (dlng * 180.0) / (a / sqrtmagic * Math.Cos(radlat) * pi);
            double mglat = lat + dlat;
            double mglng = lng + dlng;
            return new double[] { mglat, mglng };
        }

                /**
         * 判断是否在国内，不在国内不做偏移
         * 
         * @param lng
         * @param lat
         * @return
         */
        private bool out_of_china(double lng, double lat)
        {
            if (lng < 72.004 || lng > 137.8347)
            {
                return true;
            }
            else if (lat < 0.8293 || lat > 55.8271)
            {
                return true;
            }
            return false;
        }

                /**
         * 纬度转换
         * 
         * @param lng
         * @param lat
         * @return
         */
        private double transformlat(double lng, double lat)
        {
            double ret = -100.0 + 2.0 * lng + 3.0 * lat + 0.2 * lat * lat + 0.1 * lng * lat + 0.2 * Math.Sqrt(Math.Abs(lng));
            ret += (20.0 * Math.Sin(6.0 * lng * pi) + 20.0 * Math.Sin(2.0 * lng * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lat * pi) + 40.0 * Math.Sin(lat / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(lat / 12.0 * pi) + 320 * Math.Sin(lat * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        /**
         * 经度转换
         * 
         * @param lng
         * @param lat
         * @return
         */
        private double transformlng(double lng, double lat)
        {
            double ret = 300.0 + lng + 2.0 * lat + 0.1 * lng * lng + 0.1 * lng * lat + 0.1 * Math.Sqrt(Math.Abs(lng));
            ret += (20.0 * Math.Sin(6.0 * lng * pi) + 20.0 * Math.Sin(2.0 * lng * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lng * pi) + 40.0 * Math.Sin(lng / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(lng / 12.0 * pi) + 300.0 * Math.Sin(lng / 30.0 * pi)) * 2.0 / 3.0;
            return ret;
        }
    }
}