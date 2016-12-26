/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Tug.Client.Util
{
    // TODO:  this should pby be moved to a common assembly,
    // no reason to keep it exclusive to just Client
    //
    // IMPL NOTE:  Regardless of what you set BypassOnLocal to,
    // .NET will still bypass what it resolves as a local host
    // as per:
    //    http://docs.telerik.com/fiddler/Configure-Fiddler/Tasks/MonitorLocalTraffic
    public class BasicWebProxy : IWebProxy
    {
        private static readonly ILogger LOG = AppLog.Create<BasicWebProxy>();

        private IEnumerable<string> _BypassList;
        private Regex[] _bypassRegex;

        private bool _isInit = false;

        public BasicWebProxy()
        { }

        public BasicWebProxy(string address, bool bypass = false,
                IEnumerable<string> bypassList = null, ICredentials credentials = null)
            : this(new Uri(address), bypass, bypassList, credentials)
        { }

        public BasicWebProxy(Uri address, bool bypass = false,
                IEnumerable<string> bypassList = null, ICredentials credentials = null)
        {
            ProxyAddress = address;
            BypassOnLocal = bypass;
            BypassList = bypassList;
            Credentials = credentials;
        }

        public Uri ProxyAddress
        { get; set; }


        public bool BypassOnLocal
        { get; set; }

        protected void AssertInit()
        {
            if (_isInit)
                return;

            _isInit = true;

            if (LOG.IsEnabled(LogLevel.Debug))
                LOG.LogDebug("Constructed Basic Web Proxy:"
                        + " [{proxyAddress}][{bypass}][{bypassCount}][{credentials}]",
                        ProxyAddress, BypassOnLocal, _bypassRegex?.Length, Credentials != null);
        }

        /// <summary>
        /// Collection of regular expressions that contain URIs to bypass.
        /// </summary>
        public IEnumerable<string> BypassList
        { 
            get { return _BypassList; }
            set
            {
                if (value != null)
                    _bypassRegex = _BypassList.Select(x => new Regex(x)).ToArray();
                else
                    _bypassRegex = null;
            }
        }

        /// <summary>
        /// Credentials to submit to the proxy server.
        /// </summary>
        public ICredentials Credentials
        { get; set; }

        public bool IsBypassed(Uri host)
        {
            AssertInit();

            if (LOG.IsEnabled(LogLevel.Debug))
                LOG.LogDebug("{method}: [{host}]", nameof(IsBypassed), host);

            var ret = BypassOnLocal &&
                    (_bypassRegex?.Any(x => x.IsMatch(host.ToString()))).GetValueOrDefault();

            LOG.LogDebug("  => " + ret);

            return ret;
        }

        public Uri GetProxy(Uri destination)
        {
            AssertInit();

            if (LOG.IsEnabled(LogLevel.Debug))
                LOG.LogDebug("{method}: [{host}]", nameof(GetProxy), destination);

            LOG.LogDebug("  => " + ProxyAddress);

            return ProxyAddress;
        }

        public override string ToString()
        {
            AssertInit();

            return $"ProxyAddress=[{ProxyAddress}] BypassOnLocal=[{BypassOnLocal}] Credentials=[{Credentials != null}]";
        }
    }
}