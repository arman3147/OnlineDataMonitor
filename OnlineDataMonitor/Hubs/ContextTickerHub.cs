using Microsoft.AspNet.SignalR;
using OnlineDataMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineDataMonitor.Hubs
{
    public class ContextTickerHub : Hub
    {
        private readonly ContextTicker _stockTicker;

        public ContextTickerHub() :
            this(ContextTicker.Instance)
        {

        }

        public ContextTickerHub(ContextTicker stockTicker)
        {
            _stockTicker = stockTicker;
        }

        public IEnumerable<HttpContextLog> GetAllContexts()
        {
            return _stockTicker.GetAllStocks();
        }
    }
}