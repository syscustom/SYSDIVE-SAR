﻿
namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.Windows.Shapes;

    public interface IShapable
    {
        void RegenerateShape(GMapControl map);
    }

    public class GMapRoute : GMapMarker, IShapable
    {
        public readonly List<PointLatLng> Points = new List<PointLatLng>();

        public GMapRoute(IEnumerable<PointLatLng> points)
        {
            Points.AddRange(points);
            RegenerateShape(null);
        }
        
        public override void Clear()
        {
            base.Clear();
            Points.Clear();
        }

        /// <summary>
        /// regenerates shape of route
        /// </summary>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map != null)
            {
                this.Map = map;

                if(Points.Count > 1)
                {
                   Position = Points[0];
                    
                   var localPath = new List<System.Windows.Point>(Points.Count);
                   var offset = Map.FromLatLngToLocal(Points[0]);
                   foreach(var i in Points)
                   {
                      var p = Map.FromLatLngToLocal(i);
                        localPath.Add(new System.Windows.Point(p.X - offset.X, p.Y - offset.Y));
                    }
    
                   var shape = map.CreateRoutePath(localPath);
    
                   if(this.Shape is Path)
                   {
                      (this.Shape as Path).Data = shape.Data;
                   }
                   else
                   {
                      this.Shape = shape;
                   }
                }
                else
                {
                   this.Shape = null;
                }
            }
        }
    }
}
