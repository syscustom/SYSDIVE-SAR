using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace WpfApp1
{
    /// <summary>
    /// frmMicronDST.xaml 的交互逻辑
    /// </summary>
    public partial class frmMicronDST : Window
    {
        private DispatcherTimer t = new DispatcherTimer();

        int WIDTH = 300, HEIGHT = 300, HAND = 150;

        int u;  //in degree
        int cx, cy;     //center of the circle
        int x, y;       //HAND coordinate

        int tx, ty, lim = 5;


        Bitmap bmp;
        Pen p;
        Graphics g;

        public frmMicronDST()
        {
            InitializeComponent();


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //create Bitmap
            bmp = new Bitmap(WIDTH + 1, HEIGHT + 1);

            //background color
            //this.BackColor = Color.Black;

            //center
            cx = WIDTH / 2;
            cy = HEIGHT / 2;

            //initial degree of HAND
            u = 0;

            //timer
            t.Interval = TimeSpan.FromMilliseconds(5);
            t.Tick += new EventHandler(this.t_Tick);
            t.Start();
        }


        private void t_Tick(object sender, EventArgs e)
        {
            //pen
            p = new Pen(Color.Green, 1f);

            //graphics
            g = Graphics.FromImage(bmp);

            //calculate x, y coordinate of HAND
            int tu = (u - lim) % 360;

            if (u >= 0 && u <= 180)
            {
                //right half
                //u in degree is converted into radian.

                x = cx + (int)(HAND * Math.Sin(Math.PI * u / 180));
                y = cy - (int)(HAND * Math.Cos(Math.PI * u / 180));
            }
            else
            {
                x = cx - (int)(HAND * -Math.Sin(Math.PI * u / 180));
                y = cy - (int)(HAND * Math.Cos(Math.PI * u / 180));
            }

            if (tu >= 0 && tu <= 180)
            {
                //right half
                //tu in degree is converted into radian.

                tx = cx + (int)(HAND * Math.Sin(Math.PI * tu / 180));
                ty = cy - (int)(HAND * Math.Cos(Math.PI * tu / 180));
            }
            else
            {
                tx = cx - (int)(HAND * -Math.Sin(Math.PI * tu / 180));
                ty = cy - (int)(HAND * Math.Cos(Math.PI * tu / 180));
            }

            /*
            nbins = self.config["nbins"]
        r_step = self.range / nbins
        x_unit = math.cos(self.heading) * r_step
        y_unit = math.sin(self.heading) * r_step
        cloud.points = [
            Point32(x = x_unit * r, y = y_unit * r, z = 0.00)
            for r in range(1, nbins + 1)
           ]
           */

            /*
            def to_sonar_angles(rad):
             """Converts radians to units of 1/16th of a gradian.
                Args:
                 rad: Angle in radians.
                Returns:
                Integral angle in units of 1 / 16th of a gradian.
                 """
                return int(rad * 3200 / math.pi) % 6400
            */
            /*
            def to_radians(angle):
            """Converts units of 1/16th of a gradian to radians.
            Args:
                angle: Angle in units of 1 / 16th of a gradian.
            Returns:
                    Angle in radians.
            """
            return angle / 3200.0 * math.pi
            */

            //draw circle
            g.DrawEllipse(p, 0, 0, WIDTH, HEIGHT);  //bigger circle
            //g.DrawEllipse(p, 80, 80, WIDTH - 160, HEIGHT - 160);    //smaller circle

            //draw perpendicular line 垂直线
            g.DrawLine(p, new System.Drawing.Point(cx, 0), new System.Drawing.Point(cx, HEIGHT)); // UP-DOWN
            g.DrawLine(p, new System.Drawing.Point(0, cy), new System.Drawing.Point(WIDTH, cy)); //LEFT-RIGHT

            //draw HAND
            g.DrawLine(new Pen(Color.Black, 1f), new System.Drawing.Point(cx, cy), new System.Drawing.Point(tx, ty));
            g.DrawLine(p, new System.Drawing.Point(cx, cy), new System.Drawing.Point(x, y));

            //load bitmap in picturebox1
            image.Source = BitmapToImageSource(bmp);

            //dispose
            p.Dispose();
            g.Dispose();

            //update
            u++;
            if (u == 180)
            {
                u = 0;
            }
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
