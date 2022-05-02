using System;
using System.Xml;
using System.Collections.Generic;
using GMap.NET;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WpfApp1
{
    public class SelectXMLData
    {
        public static List<WayPoint> GetWayPoints(string _filename)
        {
            List<WayPoint> LstWayPoints = new List<WayPoint>();
            //string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (!File.Exists(_filename)) return LstWayPoints;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_filename);
            }
            catch { return LstWayPoints; }

            XmlNodeList nodeList = xmlDoc.SelectSingleNode("WayPoints").ChildNodes;
            foreach (XmlNode xn in nodeList)
            {
                WayPoint _waypoint = new WayPoint();
                PointLatLng _pointlatlng = new PointLatLng();
                foreach (XmlNode xn1 in xn.ChildNodes)
                {
                    XmlElement xe = (XmlElement)xn1;//将子节点类型转换为XmlElement类型
                    switch(xe.Name)
                    {
                        case "ID":
                            _waypoint.ID = Convert.ToInt32(xe.InnerText);
                            break;
                        case "Title":
                            _waypoint.Name = xe.InnerText;
                            break;     
                        case "Lat":
                            _pointlatlng.Lat = Convert.ToDouble(xe.InnerText);
                            break;
                        case "Lng":
                            _pointlatlng.Lng = Convert.ToDouble(xe.InnerText);
                            break;
                        case "Depth":
                            _waypoint.Depth = Convert.ToDouble(xe.InnerText);
                            break;
                        case "Type":
                            _waypoint.Type = Convert.ToInt32(xe.InnerText);
                            break;
                    }
                }
                _waypoint.PointLATLNG = _pointlatlng;
               // double[] latlng = wgs84togcj02(_waypoint.PointLATLNG.Lat, _waypoint.PointLATLNG.Lng);
               // _waypoint.PointLATLNGGCJ02 = new PointLatLng(latlng[0], latlng[1]);
                LstWayPoints.Add(_waypoint);
            }
            return LstWayPoints;
        }

        public static string GetConfiguration(string _filename, string _nodename, string _attribute)
        {
            string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(assemblyFolder + "\\" + _filename);
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("Configuration").ChildNodes;
            foreach (XmlNode xn in nodeList)
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                if (xe.Name == _nodename)
                {
                    return xe.GetAttribute(_attribute);
                }
            }
            return "";
        }

        public static string GetConfiguration(string _nodename, string _attribute)
        {
            string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(assemblyFolder + "\\config.xml");
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("Configuration").ChildNodes;
            foreach (XmlNode xn in nodeList)
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                if (xe.Name == _nodename)
                {
                    return xe.GetAttribute(_attribute);
                }
            }
            return "";
        }

        public static void SaveConfiguration(string _config)
        {
            string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //string assemblyFolder = System.Windows.Forms.Application.StartupPath;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(assemblyFolder + "\\config.xml");
            string[] tokens = _config.Split(new Char[] { '|' });
            int tokensindex = 0;
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("Configuration").ChildNodes;
            foreach (XmlNode xn in nodeList)
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                xe.SetAttribute("value", tokens[tokensindex]);
                tokensindex++;
            }
            assemblyFolder = assemblyFolder.Substring(6);
            xmlDoc.Save(assemblyFolder + "\\config.xml");
        }

        public static void SaveConfiguration(string _nodename, string _attribute, string _config)
        {
            string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            assemblyFolder = Environment.CurrentDirectory;
            //string assemblyFolder = System.Windows.Forms.Application.StartupPath;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(assemblyFolder + "\\config.xml");
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("Configuration").ChildNodes;
            foreach (XmlNode xn in nodeList)
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                if (xe.Name == _nodename)
                {
                    xe.SetAttribute(_attribute, _config);
                }
            }
            //assemblyFolder = assemblyFolder.Substring(6);
            xmlDoc.Save(assemblyFolder + "\\config.xml");
        }

        public static void CreateXMLFile(string _filename, string _root)
        {
            string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            assemblyFolder = Environment.CurrentDirectory;

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration dec = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(dec);

            //创建根节点 
            XmlElement root = xmlDoc.CreateElement(_root);
            xmlDoc.AppendChild(root);
            xmlDoc.Save(_filename);
            //string path = assemblyFolder + "\\" + _filename;
            //xmlDoc.Save(path);
        }
        
        public static void InsertConfiguration(string _filename, string _element,string _id, string _title, string _lat, string _lng, string _depth, string _type)
        {
            //string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //assemblyFolder = Environment.CurrentDirectory;
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(_filename);

            XmlNode root = xmlDoc.SelectSingleNode("WayPoints");

            //节点及元素
            XmlNode waypoint = xmlDoc.CreateElement(_element);
            XmlElement id = GetXmlElement(xmlDoc, "ID", _id);
            XmlElement title = GetXmlElement(xmlDoc, "Title", _title);
            XmlElement lat = GetXmlElement(xmlDoc, "Lat", _lat);  //纬度
            XmlElement lng = GetXmlElement(xmlDoc, "Lng", _lng); //经度
            XmlElement depth = GetXmlElement(xmlDoc, "Depth", _depth); //深度
            XmlElement type = GetXmlElement(xmlDoc, "Type", _type); //节点类型
            //price.SetAttribute("Lat", "123");
            //price.SetAttribute("Lng", "456");

            waypoint.AppendChild(id);
            waypoint.AppendChild(title);
            waypoint.AppendChild(lat);
            waypoint.AppendChild(lng);
            waypoint.AppendChild(depth);
            waypoint.AppendChild(type);
            root.AppendChild(waypoint);

            xmlDoc.Save(_filename);

        }

        public static void EditConfiguration(string _filename, string _id, string _lat, string _lng, string _depth, string _type)
        {
            string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            assemblyFolder = Environment.CurrentDirectory;
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(_filename);

            XmlNode root = xmlDoc.SelectSingleNode("WayPoints");
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("WayPoints").ChildNodes;
            foreach (XmlNode xn in nodeList)
            {
                if((xn.ChildNodes[0].Name == "ID") && (xn.ChildNodes[0].InnerText == _id))
                {
                    foreach (XmlNode xn1 in xn.ChildNodes)
                    {
                        XmlElement xe = (XmlElement)xn1;//将子节点类型转换为XmlElement类型
                        switch (xe.Name)
                        {
                            case "Lat":
                                xe.InnerText = _lat;
                                break;
                            case "Lng":
                                xe.InnerText = _lng;
                                break;
                            case "Depth":
                                xe.InnerText = _depth;
                                break;
                            case "Type":
                                xe.InnerText = _type;
                                break;
                        }
                    }
                }
            }
            xmlDoc.Save(_filename);

        }

        public static void DeleteConfiguration(string _filename, int _id)
        {
            string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            assemblyFolder = Environment.CurrentDirectory;
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(_filename);

            XmlNode root = xmlDoc.SelectSingleNode("WayPoints");
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("WayPoints").ChildNodes;
            foreach (XmlNode xn in nodeList)
            {
                foreach (XmlNode xn1 in xn.ChildNodes)
                {
                    XmlElement xe = (XmlElement)xn1;//将子节点类型转换为XmlElement类型
                    switch (xe.Name)
                    {
                        case "ID":
                            if (_id == Convert.ToInt32(xe.InnerText))
                                root.RemoveChild(xn);
                            break;
                    }
                }
            }
            xmlDoc.Save(_filename);
        }

        private static XmlElement GetXmlElement(XmlDocument doc, string elementName, string value)
        {
            XmlElement element = doc.CreateElement(elementName);
            element.InnerText = value;
            return element;
        }

    }
}
