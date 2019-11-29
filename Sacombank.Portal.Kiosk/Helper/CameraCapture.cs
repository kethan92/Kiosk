﻿using System;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using System.Windows.Threading;

namespace Sacombank.Portal.Kiosk.Helper
{
    public class CameraCapture
    {
        private Capture capture;            /* a Capture object that gets images from the camera */
        private DispatcherTimer timer;              /* dispatchertimer object used for ticks */

        public CameraCapture(EventHandler myEventHandler)
        {
            /* initialize the camera object */
            capture = new Capture();

            /* create a new timer */
            timer = new DispatcherTimer();
            /* attach the event handler */
            timer.Tick += new EventHandler(myEventHandler);
            /* Set the clock's tick intervals */
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        /* Stop the timer if not stopped already */
        public void stopTimer()
        {
            /* if timer is started */
            if(timer.IsEnabled)
                timer.Stop();
        }

        /* Start the timer if not started already*/
        public void startTimer()
        {
            /* if timer is not started */
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }

        /* return a single frame from the camera */
        public Image<Bgr, Byte> queryFrame()
        {
            return capture.QueryFrame().ToImage<Bgr, Byte>();
        }
    }
}
