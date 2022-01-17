using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OnlineDataMonitor.Hubs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace OnlineDataMonitor.Models
{
    public class ContextTicker
    {
        private readonly static Lazy<ContextTicker> _instance = new Lazy<ContextTicker>(
        () => new ContextTicker(GlobalHost.ConnectionManager.GetHubContext<ContextTickerHub>().Clients));

        private static SqlTableDependency<HttpContextLog> _tableDependency;

        private ContextTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            var mapper = new ModelToTableMapper<HttpContextLog>();

            _tableDependency = new SqlTableDependency<HttpContextLog>(
                ConfigurationManager.ConnectionStrings["MainConnectionString"].ConnectionString,
                "HttpContextLog",
                "dbo",
                mapper);

            _tableDependency.OnChanged += SqlTableDependency_Changed;
            _tableDependency.OnError += SqlTableDependency_OnError;
            _tableDependency.Start();
        }

        public static ContextTicker Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public IEnumerable<HttpContextLog> GetAllStocks()
        {
            var contextModel = new List<HttpContextLog>();

            var connectionString = ConfigurationManager.ConnectionStrings
                    ["MainConnectionString"].ConnectionString;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SELECT * FROM [HttpContextLogs]";

                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            var ID = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ID"));
                            var upstreamGatewayURL = sqlDataReader.GetString(sqlDataReader.GetOrdinal("UpstreamGatewayURL"));
                            var requestHeaders = sqlDataReader.GetString(sqlDataReader.GetOrdinal("RequestHeaders"));
                            var requestBody = sqlDataReader.GetString(sqlDataReader.GetOrdinal("RequestBody"));
                            var responseHeaders = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ResponseHeaders"));
                            var responseBody = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ResponseBody"));
                            var requestTime = sqlDataReader.GetString(sqlDataReader.GetOrdinal("RequestTime"));

                            contextModel.Add(new HttpContextLog { ID = Guid.Parse(ID), UpstreamGatewayURL = upstreamGatewayURL, RequestHeaders = requestHeaders,
                                                                  RequestBody = requestBody, ResponseHeaders = responseHeaders, ResponseBody = responseBody,
                                                                  RequestTime = requestTime
                            });
                        }
                    }
                }
            }

            return contextModel;
        }

        void SqlTableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            throw e.Error;
        }

        /// <summary>
        /// Broadcast New Stock Price
        /// </summary>
        void SqlTableDependency_Changed(object sender, RecordChangedEventArgs<HttpContextLog> e)
        {
            if (e.ChangeType != ChangeType.None)
            {
                BroadcastHttpRequest(e.Entity);
            }
        }

        private void BroadcastHttpRequest(HttpContextLog stock)
        {
            Clients.All.updateStockPrice(stock);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _tableDependency.Stop();
                }

                disposedValue = true;
            }
        }

        ~ContextTicker()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}