﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class RequestBuilder
    {
        private readonly ISession _session;
        private readonly Lazy<IBatchWriter> _lazyBatchWriter;

        public bool IsBatch { get { return _lazyBatchWriter != null; } }
        public string Host
        {
            get
            {
                if (string.IsNullOrEmpty(_session.UrlBase)) return null;
                var substr = _session.UrlBase.Substring(_session.UrlBase.IndexOf("//") + 2);
                return substr.Substring(0, substr.IndexOf("/"));
            }
        }

        public RequestBuilder(ISession session, bool isBatch = false)
        {
            _session = session;
            if (isBatch)
            {
                _lazyBatchWriter = new Lazy<IBatchWriter>(() => _session.Adapter.GetBatchWriter());
            }
        }

        public Task<ODataRequest> CreateGetRequestAsync(string commandText, bool scalarResult = false)
        {
            var request = new ODataRequest(RestVerbs.Get, _session, commandText)
            {
                ReturnsScalarResult = scalarResult,
            };
            return Utils.GetTaskFromResult(request);
        }

        public Task<ODataRequest> CreateInsertRequestAsync(string collection, IDictionary<string, object> entryData, bool resultRequired)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateInsertRequestAsync(collection, entryData, resultRequired);
        }

        public Task<ODataRequest> CreateUpdateRequestAsync(string commandText, string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateUpdateRequestAsync(commandText, collection, entryKey, entryData, resultRequired);
        }

        public Task<ODataRequest> CreateDeleteRequestAsync(string commandText, string collection)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateDeleteRequestAsync(commandText, collection);
        }

        public Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, string formattedEntryKey, string formattedLinkKey)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateLinkRequestAsync(collection, linkName, formattedEntryKey, formattedLinkKey);
        }

        public Task<ODataRequest> CreateUnlinkRequestAsync(string collection, string linkName, string formattedEntryKey)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateUnlinkRequestAsync(collection, linkName, formattedEntryKey);
        }

        public async Task<ODataRequest> CreateBatchRequestAsync()
        {
            var requestMessage = await _lazyBatchWriter.Value.EndBatchAsync();
            var request = new ODataRequest(RestVerbs.Post, _session, ODataLiteral.Batch, requestMessage);
            return request;
        }
    }
}
