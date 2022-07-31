using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Peanut.Libs.Specialized {
    /// <summary>
    /// Allows an application to check internet connection against a specific url or ip address.<br/>
    /// </summary>
    public class NetworkConnectionMonitor : IDisposable {
        /// <summary>
        /// Gets or sets the url or the ip address which this class should check internet
        /// connection against.<br/>
        /// </summary>
        public string HostNameOrAddress { get; set; }

        /// <summary>
        /// Gets or sets the timeout for each ping (in milliseconds).<br/>
        /// </summary>
        public int Timeout {
            get => timeout;
            set {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException(nameof(Timeout));
                }
                timeout = value;
            }
        }
        private int timeout = 10000;

        /// <summary>
        /// Gets or sets the interval between pings (in milliseconds).<br/>
        /// </summary>
        public int CheckInterval {
            get => checkInterval;
            set {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException(nameof(CheckInterval));
                }
                checkInterval = value;
            }
        }
        private int checkInterval = 5000;

        /// <summary>
        /// Gets whether the application is connected to the <see cref="HostNameOrAddress"/>.<br/>
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Occurs when the <see cref="Connected"/> property changes its value.<br/>
        /// </summary>
        public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

        private readonly Timer timer;
        private readonly Ping ping;
        private int checkCount;

        /// <summary>
        /// Initializes an instance of the <see cref="NetworkConnectionMonitor"/> class.<br/>
        /// </summary>
        /// <param name="hostNameOrAddress">
        ///     The url or ip address used to check internet connectivity.
        /// </param>
        public NetworkConnectionMonitor(string hostNameOrAddress) {
            timer = new(
                TimerCallback,
                null,
                System.Threading.Timeout.Infinite,
                System.Threading.Timeout.Infinite);
            ping = new();
            HostNameOrAddress = hostNameOrAddress;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="NetworkConnectionMonitor"/> class.<br/>
        /// </summary>
        /// <param name="hostNameOrAddress">
        ///     The url or ip address used to check internet connectivity.
        /// </param>
        /// <param name="timeout">
        ///     The timeout for each ping (in milliseconds) against the url or ip address.
        /// </param>
        public NetworkConnectionMonitor(string hostNameOrAddress, int timeout) :
            this(hostNameOrAddress) {
            Timeout = timeout;
        }

        /// <summary>
        /// Starts checking internet connection each interval.<br/>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void StartMonitoring() {
            if (disposedValue) {
                throw new InvalidOperationException("Cannot start monitoring when already disposed.");
            }
            timer.Change(0, checkInterval);
        }

        /// <summary>
        /// Stops checking internet connection.<br/>
        /// This method internally calls the <see cref="Dispose()"/> method.<br/>
        /// </summary>
        public void StopMonitoring() {
            Dispose();
        }

        /// <summary>
        /// Performs one single connection check.<br/>
        /// </summary>
        public void CheckOnce() {
            CheckOnceAsync().Wait();
        }

        /// <summary>
        /// Asynchronously Performs one single connection check.<br/>
        /// </summary>
        /// <returns></returns>
        public async Task CheckOnceAsync() {
            bool connected = false;
            try {
                PingReply? pingReply =
                    await ping.SendPingAsync(HostNameOrAddress, timeout).ConfigureAwait(false);
                connected = pingReply.Status == IPStatus.Success;
            }
            catch { }
            if (Connected != connected || checkCount++ == 0) {
                Connected = connected;
                ConnectivityChanged?.Invoke(this, new(Connected));
            }
        }

        private async void TimerCallback(object? state) {
            await CheckOnceAsync();
        }

        #region IDisposable implementation
        private bool disposedValue;

        /// <summary>
        /// Protected implementation of Dispose pattern.<br/>
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true"/> if calling directly or indirectly.
        ///     Otherwise, <see langword="false"/> when calling from a finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    timer.Dispose();
                    ping.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NetworkConnectionMonitor()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Not necessary if <see cref="StopMonitoring"/> method has been called.<br/>
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// Event arguments for <see cref="NetworkConnectionMonitor.ConnectivityChanged"/>.<br/>
    /// </summary>
    public class ConnectivityChangedEventArgs : EventArgs {
        /// <summary>
        /// Gets whether the application is connected to the internet.<br/>
        /// </summary>
        public bool Connected { get; }

        /// <summary>
        /// Initializes an instance of the <see cref="ConnectivityChangedEventArgs"/> class.<br/>
        /// </summary>
        /// <param name="connected">
        ///     A value indicating whether the application is connected to the internet.
        /// </param>
        public ConnectivityChangedEventArgs(bool connected) {
            Connected = connected;
        }
    }
}
