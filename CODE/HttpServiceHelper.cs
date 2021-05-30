﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DanHotelsConnector
{
    /*
        USING
        -----
        var result = new HttpServiceHelper().POST(<url>, <model>, null, new Dictionary<string, string> { 
            ["Content-Type"] = "application/json"
        });

        if (!result.Success) 
            throw new Exception(result.Content);
        return Request.CreateResponse(HttpStatusCode.OK, result);        

        -
        
        var result = new HttpServiceHelper().GET<SystemReportData<string>>(<url>, headers: <headers>);

        if (!result.Success) 
            throw new Exception(result.Content);
        return Request.CreateResponse(HttpStatusCode.OK, result);  

        -

        var result = await this.HttpServiceHelper.POST_ASYNC(<url>, <model>, null, new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["Authorization"] = $"JWT {this.TokenData.Token}"
        });

        if (!result.Success) 
            throw new Exception(result.Content);
        return Request.CreateResponse(HttpStatusCode.OK, result);

        - 

        var result = await this.HttpServiceHelper.POST_DATA_ASYNC<T>(<url>, new List<string> { <param-1>, <param-2>...<param-N> }, null, new Dictionary<string, string>
        {
            ["Content-Type"] = "application/x-www-form-urlencoded"
        });

        if (!result.Success) 
            throw new Exception(result.Content);
        return Request.CreateResponse(HttpStatusCode.OK, result);

    */
    public class HttpServiceHelper : IHttpServiceHelper, IAsyncHttpServiceHelper
    {
        private const double TimeOutSec = 10;        

        private WebClient client { get; set; } = new WebClient {
            Proxy = null,
            Encoding = Encoding.UTF8
        };

        public (bool Success, string Content) GET(string url, string querystring = null, Dictionary<string, string> headers = null)
        {
            try
            {
                client.Headers.Clear();
                if (headers != null)
                    foreach (var header in headers)
                        client.Headers.Add(header.Key, header.Value);

                if (!string.IsNullOrEmpty(querystring))
                    url = string.Concat(url, "?", querystring);

                return (true, client.DownloadString(url));
            }
            catch (WebException ex)
            {
                ///var WebRespEX = ((HttpWebResponse)ex.Response);
                ///STATUS = WebRespEX.StatusCode;

                var stream = ex?.Response?.GetResponseStream();
                if(stream == null) return (false, $"{ex.Message}");

                using (var reader = new StreamReader(stream))
                    return (false, $"{ex.Message} | {reader.ReadToEnd()}");
            }
            catch (Exception ex) {                
                return (false, ex.Message);
            }
        }

        public (bool Success, string Content, T Model) GET<T>(string url, string querystring = null, Dictionary<string, string> headers = null)
        {
            try
            {
                var response = this.GET(url, querystring, headers);
                return (response.Success, response.Content, response.Success ? JsonConvert.DeserializeObject<T>(response.Content) : default(T));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, default(T));
            }
        }

        public (bool Success, string Content) POST<T>(string url, T payload, string querystring = null, Dictionary<string, string> headers = null) {
            return UPLOAD(url, payload, "JSON", querystring, headers, "POST");
        }
        
        public (bool Success, string Content, TResult Model) POST<TPayload, TResult>(string url, TPayload payload, string querystring = null, Dictionary<string, string> headers = null) {
            return UPLOAD<TPayload, TResult>(url, payload, "JSON", querystring, headers, "POST");
        }

        public (bool Success, string Content) PUT<T>(string url, T payload, string querystring = null, Dictionary<string, string> headers = null) {
            return UPLOAD(url, payload, "JSON", querystring, headers, "PUT");
        }

        public (bool Success, string Content, TResult Model) PUT<TPayload, TResult>(string url, TPayload payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return UPLOAD<TPayload, TResult>(url, payload, "JSON", querystring, headers, "PUT");
        }

        public (bool Success, string Content) DELETE<T>(string url, T payload, string querystring = null, Dictionary<string, string> headers = null) {
            return UPLOAD(url, payload, "JSON", querystring, headers, "DELETE");
        }

        public (bool Success, string Content, TResult Model) DELETE<TPayload, TResult>(string url, TPayload payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return UPLOAD<TPayload, TResult>(url, payload, "JSON", querystring, headers, "DELETE");
        }

        public (bool Success, string Content) POST_DATA(string url, Dictionary<string, string> payload, string querystring = null, Dictionary<string, string> headers = null) 
        {
            return POST_DATA(url, payload.Select(p => $"{p.Key}={payload[p.Key]}"), querystring, headers);
        }
        public (bool Success, string Content) POST_DATA(string url, IEnumerable<string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {            
            return UPLOAD(url, string.Join("&", payload), "DATA", querystring, headers, "POST");
        }

        public (bool Success, string Content, TResult Model) POST_DATA<TResult>(string url, IEnumerable<string> payload, string querystring = null, Dictionary<string, string> headers = null) {
            return UPLOAD<string, TResult>(url, string.Join("&", payload), "DATA", querystring, headers, "POST");
        }

        public (bool Success, string Content) PUT_DATA(string url, Dictionary<string, string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return PUT_DATA(url, payload.Select(p => $"{p.Key}={payload[p.Key]}"), querystring, headers);
        }
        public (bool Success, string Content) PUT_DATA(string url, IEnumerable<string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return UPLOAD(url, string.Join("&", payload), "DATA", querystring, headers, "PUT");
        }

        public (bool Success, string Content) DELETE_DATA(string url, Dictionary<string, string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return DELETE_DATA(url, payload.Select(p => $"{p.Key}={payload[p.Key]}"), querystring, headers);
        }
        public (bool Success, string Content) DELETE_DATA(string url, IEnumerable<string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return UPLOAD(url, string.Join("&", payload), "DATA", querystring, headers, "DELETE");
        }

        protected (bool Success, string Content) UPLOAD<T>(string url, T payload, string payloadMode = "JSON" /*JSON|DATA*/, string querystring = null, Dictionary<string, string> headers = null, string method = "POST")
        {            
            try
            {
                client.Headers.Clear();
                if (headers != null)
                    foreach (var header in headers)
                        client.Headers.Add(header.Key, header.Value);
                    
                if (!string.IsNullOrEmpty(querystring))
                    url = string.Concat(url, "?", querystring);

                string response = null;

                // as form-variables (aka post-data)
                if (payloadMode == "DATA") {
                    response = Encoding.UTF8.GetString(client.UploadData(url, method, Encoding.UTF8.GetBytes(payload.ToString())));
                }
                else // as json payload                
                {
                    var payloadType = payload.GetType();
                    string sPayload;
                    if (payloadType.IsPrimitive || payloadType == typeof(System.String))
                        sPayload = payload.ToString();
                    else
                        sPayload = JsonConvert.SerializeObject(payload);

                    response = client.UploadString(url, method, sPayload);
                }

                return (true, response);
            }
            catch (WebException ex)
            {
                ///var WebRespEX = ((HttpWebResponse)ex.Response);
                ///STATUS = WebRespEX.StatusCode;

                var stream = ex?.Response?.GetResponseStream();
                if (stream == null) return (false, $"{ex.Message}");

                using (var reader = new StreamReader(stream))
                    return (false, $"{ex.Message} | {reader.ReadToEnd()}");                
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        protected (bool Success, string Content, TResult Model) UPLOAD<TPayload, TResult>(string url, TPayload payload, string payloadMode = "JSON" /*JSON|DATA*/, string querystring = null, Dictionary<string, string> headers = null, string method = "POST") {
            try
            {
                var response = this.UPLOAD<TPayload>(url, payload, payloadMode, querystring, headers, method);
                return (response.Success, response.Content, response.Success ? JsonConvert.DeserializeObject<TResult>(response.Content) : default(TResult));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, default(TResult));
            }
        }

        // --------------------------------

        public async Task<(bool Success, string Content)> GET_ASYNC(string url, string querystring = null, Dictionary<string, string> headers = null)
        {
            try
            {
                client.Headers.Clear();
                if (headers != null)
                    foreach (var header in headers)
                        client.Headers.Add(header.Key, header.Value);

                if (!string.IsNullOrEmpty(querystring))
                    url = string.Concat(url, "?", querystring);

                return (true, await client.DownloadStringTaskAsync(url));
            }
            catch (WebException ex)
            {
                ///var WebRespEX = ((HttpWebResponse)ex.Response);
                ///STATUS = WebRespEX.StatusCode;

                var stream = ex?.Response?.GetResponseStream();
                if (stream == null) return (false, $"{ex.Message}");

                using (var reader = new StreamReader(stream))
                    return (false, $"{ex.Message} | {reader.ReadToEnd()}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Content, T Model)> GET_ASYNC<T>(string url, string querystring = null, Dictionary<string, string> headers = null)
        {
            try
            {
                var response = await this.GET_ASYNC(url, querystring, headers);
                return (response.Success, response.Content, response.Success ? JsonConvert.DeserializeObject<T>(response.Content) : default(T));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, default(T));
            }
        }

        public async Task<(bool Success, string Content)> POST_ASYNC<T>(string url, T payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC(url, payload, "JSON", querystring, headers, "POST");
        }

        public async Task<(bool Success, string Content, TResult Model)> POST_ASYNC<TPayload, TResult>(string url, TPayload payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC<TPayload, TResult>(url, payload, "JSON", querystring, headers, "POST");
        }

        public async Task<(bool Success, string Content)> PUT_ASYNC<T>(string url, T payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC(url, payload, "JSON", querystring, headers, "PUT");
        }

        public async Task<(bool Success, string Content, TResult Model)> PUT_ASYNC<TPayload, TResult>(string url, TPayload payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC<TPayload, TResult>(url, payload, "JSON", querystring, headers, "PUT");
        }

        public async Task<(bool Success, string Content)> DELETE_ASYNC<T>(string url, T payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC(url, payload, "JSON", querystring, headers, "DELETE");
        }

        public async Task<(bool Success, string Content, TResult Model)> DELETE_ASYNC<TPayload, TResult>(string url, TPayload payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC<TPayload, TResult>(url, payload, "JSON", querystring, headers, "DELETE");
        }

        public async Task<(bool Success, string Content)> POST_DATA_ASYNC(string url, Dictionary<string, string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await POST_DATA_ASYNC(url, payload.Select(p => $"{p.Key}={payload[p.Key]}"), querystring, headers);
        }
        public async Task<(bool Success, string Content)> POST_DATA_ASYNC(string url, IEnumerable<string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC(url, string.Join("&", payload), "DATA", querystring, headers, "POST");
        }

        public async Task<(bool Success, string Content, TResult Model)> POST_DATA_ASYNC<TResult>(string url, IEnumerable<string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC<string, TResult>(url, string.Join("&", payload), "DATA", querystring, headers, "POST");
        }

        public async Task<(bool Success, string Content)> PUT_DATA_ASYNC(string url, Dictionary<string, string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await PUT_DATA_ASYNC(url, payload.Select(p => $"{p.Key}={payload[p.Key]}"), querystring, headers);
        }
        public async Task<(bool Success, string Content)> PUT_DATA_ASYNC(string url, IEnumerable<string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC(url, string.Join("&", payload), "DATA", querystring, headers, "PUT");
        }

        public async Task<(bool Success, string Content)> DELETE_DATA_ASYNC(string url, Dictionary<string, string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await DELETE_DATA_ASYNC(url, payload.Select(p => $"{p.Key}={payload[p.Key]}"), querystring, headers);
        }
        public async Task<(bool Success, string Content)> DELETE_DATA_ASYNC(string url, IEnumerable<string> payload, string querystring = null, Dictionary<string, string> headers = null)
        {
            return await UPLOAD_ASYNC(url, string.Join("&", payload), "DATA", querystring, headers, "DELETE");
        }

        protected async Task<(bool Success, string Content)> UPLOAD_ASYNC<T>(string url, T payload, string payloadMode = "JSON" /*JSON|DATA*/, string querystring = null, Dictionary<string, string> headers = null, string method = "POST")
        {
            try
            {
                client.Headers.Clear();
                if (headers != null)
                    foreach (var header in headers)
                        client.Headers.Add(header.Key, header.Value);

                if (!string.IsNullOrEmpty(querystring))
                    url = string.Concat(url, "?", querystring);

                string response = null;

                // as form-variables (aka post-data)
                if (payloadMode == "DATA")
                {
                    response = Encoding.UTF8.GetString(await client.UploadDataTaskAsync(url, method, Encoding.UTF8.GetBytes(payload.ToString())));
                }
                else // as json payload                
                {
                    var payloadType = payload.GetType();
                    string sPayload;
                    if (payloadType.IsPrimitive || payloadType == typeof(System.String))
                        sPayload = payload.ToString();
                    else
                        sPayload = JsonConvert.SerializeObject(payload);

                    response = await client.UploadStringTaskAsync(url, method, sPayload);                    
                }

                return (true, response);
            }
            catch (WebException ex)
            {
                ///var WebRespEX = ((HttpWebResponse)ex.Response);
                ///STATUS = WebRespEX.StatusCode;

                var stream = ex?.Response?.GetResponseStream();
                if (stream == null) return (false, $"{ex.Message}");

                using (var reader = new StreamReader(stream))
                    return (false, $"{ex.Message} | {reader.ReadToEnd()}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        protected async Task<(bool Success, string Content, TResult Model)> UPLOAD_ASYNC<TPayload, TResult>(string url, TPayload payload, string payloadMode = "JSON" /*JSON|DATA*/, string querystring = null, Dictionary<string, string> headers = null, string method = "POST")
        {
            try
            {
                var response = await this.UPLOAD_ASYNC<TPayload>(url, payload, payloadMode, querystring, headers, method);
                return (response.Success, response.Content, response.Success ? JsonConvert.DeserializeObject<TResult>(response.Content) : default(TResult));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, default(TResult));
            }
        }
    }
}
