﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BLL
{
    public class KeepAlive
    {
        private Timer _timer = null;

        private bool _isRunning { set; get; }
        private int _IntervelInMinutes { set; get; }
        private string _SiteURL { set; get; }

        public bool Start()
        {
            if (_isRunning) return false;
            _timer.Enabled = true;
            return true;
        }

        public void Stop()
        {
            _timer.Enabled = false;
            _isRunning = false;
        }

        private void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isRunning) return;

            _isRunning = true;

            try
            {
                WebClient client = new WebClient();
                client.DownloadString(_SiteURL);
            }
            catch { }
            finally { _isRunning = false; }
        }

        public KeepAlive(int IntervelInMinutes, string SampleURL)
        {
            _timer = new Timer();
            _SiteURL = SampleURL.Trim();
            _IntervelInMinutes = IntervelInMinutes;
            _timer.Interval = _IntervelInMinutes * 60 * 1000;
            _timer.Elapsed += new ElapsedEventHandler(Elapsed);

            if (_SiteURL == string.Empty)
                throw new ArgumentException("Param cannot be Empty", "SiteURL");

            if (_IntervelInMinutes <= 0)
                throw new ArgumentException("Param MUST be greater then 0", "IntervelInMinutes");
        }
    }
}
