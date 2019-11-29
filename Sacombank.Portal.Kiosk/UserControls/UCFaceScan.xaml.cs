using System.Windows.Controls;
using System;
using log4net;
using Emgu.CV;
using Emgu.CV.Structure;
using Sacombank.Portal.Kiosk.Helper;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using System.Drawing.Imaging;

namespace Sacombank.Portal.Kiosk.UserControls
{
    /// <summary>
    /// Interaction logic for UCFaceScan.xaml
    /// </summary>
    public partial class UCFaceScan : UserControl
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private CameraCapture camera_object = null;
        private CascadeClassifier _cascadeClassifier;
        private BitmapSource modified_image = null;
        string fullPath = string.Empty;
        string path = string.Empty;
        bool detectedFace = false;

        public UCFaceScan()
        {
            InitializeComponent();
            Loaded += MyLoadedRoutedEventHandler;
        }

        private void btnStartCamera_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Bitmap image = camera_object.queryFrame().Bitmap;
            //image.Save(@"C:\NguyenKeThan\Software\Image\"+ DateTime.Now.ToString("yyyyMMdd_hhmmss")+".png", ImageFormat.Png);
            detectFacesImage(new Image<Bgr,Byte>(image));
            return;
        }

        private void MyLoadedRoutedEventHandler(Object sender, EventArgs e)
        {
            path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "");
            string facePath = Path.GetFullPath(@"../../data/haarcascade_frontalface_default.xml");
            _cascadeClassifier = new CascadeClassifier(facePath);
            if (camera_object == null)
            {
                /* initialize the cameramode object and pass it the event handler */
                camera_object = new CameraCapture(timer_Tick);
            }
            camera_object.startTimer();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            /* Grab a frame from the camera */
            Image<Bgr, Byte> currentFrame = camera_object.queryFrame();
            if (currentFrame != null)
            {
                detectFaces(currentFrame);
            }
        }

        private void detectFaces(Image<Bgr, Byte> image)
        {
            /* Check to see that there was a frame collected */
            /* convert the frame from the camera to a transformed Image that improves facial detection */
            Image<Gray, Byte> grayFrame = image.Convert<Gray, Byte>();
            /* Detect how many faces are there on the image */
            var faces = _cascadeClassifier.DetectMultiScale(grayFrame, 1.1, 10, System.Drawing.Size.Empty);
            if (faces.Length > 0 && faces.Length == 1)
            {
                detectedFace = true;
                btnStartCamera.IsHitTestVisible = true;
                foreach (var face in faces)
                {
                    image.Draw(face, new Bgr(System.Drawing.Color.Red), 4);
                }
            }
            else
            {
                detectedFace = false;
                btnStartCamera.IsHitTestVisible = false;
            }
            modified_image = ToBitmapSource(image);
            WebcamViewer.Source = modified_image;
        }

        private void detectFacesImage(Image<Bgr, Byte> image)
        {
            /* Check to see that there was a frame collected */
            /* convert the frame from the camera to a transformed Image that improves facial detection */
            Image<Gray, Byte> grayFrame = image.Convert<Gray, Byte>();
            /* Detect how many faces are there on the image */
            var faces = _cascadeClassifier.DetectMultiScale(grayFrame, 1.1, 10, System.Drawing.Size.Empty);
            if (faces.Length > 0 && faces.Length == 1)
            {
                foreach (var face in faces)
                {
                    image.Draw(face, new Bgr(System.Drawing.Color.Red), 4);
                    int w = face.Width;
                    int h = face.Height;
                    int x = face.X;
                    int y = face.Y;

                    int r = Math.Max(250, 250) / 2;
                    int centerx = x + w / 2;
                    int centery = y + h / 2;
                    int nx = (int)(centerx - r);
                    int ny = (int)(centery - r);
                    int nr = (int)(r * 5);


                    double zoomFactor = (double)197 / (double)face.Width;
                    System.Drawing.Size newSize = new System.Drawing.Size((int)(image.Width * zoomFactor), (int)(image.Height * zoomFactor));
                    Bitmap bmp = new Bitmap(image.ToBitmap(), newSize);
                    var imgextract = CropImage(bmp, nx + 4, ny - 25, 248, 340);
                    imgextract.Save(@"C:\NguyenKeThan\Software\Image\" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".png", ImageFormat.Png);
                }
            }
        }
        private static Bitmap CropImage(System.Drawing.Image source, int x, int y, int width, int height)
        {
            Rectangle crop = new Rectangle(x, y, width, height);

            var bmp = new Bitmap(crop.Width, crop.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }
            return bmp;
        }


        private BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); /* obtain the Hbitmap */

                /* Transform the IImage to a BitmapSource */
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); /* release the HBitmap */
                return bs; /* return the newly converted BitmapSource */
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

    }
}
