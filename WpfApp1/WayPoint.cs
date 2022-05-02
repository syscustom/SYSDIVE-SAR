using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace WpfApp1
{
    public class WayPoint
    {
        public int ID { get; set; } = 0;

        public string Name { get; set; } = "";

        public bool AddedToMap { get; set; } = false;

        private PointLatLng pointlatlng = new PointLatLng();
        private PointLatLng pointlatlnggcj02 = new PointLatLng();

        public PointLatLng PointLATLNG //wgs84
        {
            get
            {
                return pointlatlng;
                //return position;
            }
            set
            {
                if (pointlatlng != value)
                {
                    pointlatlng = value;
                    double[] latlng = wgs84togcj02(pointlatlng.Lat, pointlatlng.Lng);
                    pointlatlnggcj02 = new PointLatLng(latlng[0], latlng[1]);
                }
            }
        }

        public PointLatLng PointLATLNGGCJ02 //wgs84
        {
            get
            {
                return pointlatlnggcj02;
            }
            set
            {
                if (pointlatlnggcj02 != value)
                {
                    pointlatlnggcj02 = value;
                    double[] latlng = gcj02towgs84(pointlatlnggcj02.Lat, pointlatlnggcj02.Lng);
                    pointlatlng = new PointLatLng(latlng[0], latlng[1]);
                }
            }
        }

        public double Depth { get; set; } = 0.0;

        public double Heading { get; set; } = 0.0;

        public double Distance { get; set; } = 0.0;

        public double Bearing { get; set; } = 0.0;

        public string SelectedMarkerName { get; set; } = "";

        public int Type { get; set; } = 1;


        double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        // π
        double pi = 3.1415926535897932384626;
        // 长半轴
        double a = 6378245.0;
        // 扁率
        double ee = 0.00669342162296594323;

        /**
	 * GCJ02(火星坐标系)转GPS84
	 * 
	 * @param lng 火星坐标系的经度
	 * @param lat 火星坐标系纬度
	 * @return WGS84坐标数组
	 */
        private double[] gcj02towgs84(double lat, double lng)
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
            return new double[] { lat * 2 - mglat, lng * 2 - mglng };
        }

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
