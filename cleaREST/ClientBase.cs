using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace cleaREST
{
    //TODO: methods for all relevant headers (auth, custom, etc.)
    //ref: https://msdn.microsoft.com/en-us/library/system.net.http.httpclient(v=vs.118).aspx

    //TODO: tests

    //TODO: other verbs

    //TODO: allow for async method calls as well

    public abstract class ClientBase
    {
        #region Properties
        
        private string _baseUri = string.Empty;        
        public string BaseUri
        {
            get
            {
                return _baseUri;

            }
            set
            {
                _baseUri = value;
            }
        }

        private string _uriScheme = string.Empty;
        public string UriScheme
        {
            get
            {
                return _uriScheme;
            }
            set
            {
                _uriScheme = value;
            }
        }

        #endregion

        #region ctors

        public ClientBase(string baseUri)
            : this(Protocols.Http, baseUri) { }

        public ClientBase(string uriScheme, string baseUri)
        {
            this.UriScheme = uriScheme;
            this.BaseUri = baseUri;
        }

        #endregion

        #region Headers

        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        private bool _headersExist = false;
        public void AddHeader(string name, object value)
        {
            //TODO: check for existing
            _headers.Add(name, value.ToString());
            _headersExist = true;
        }

        #endregion

        #region Credentials

        
        private bool _isBasicAuth = false;
        private string _basicAuthCredentials;
        public void SetBasicAuthCredential(string credentials)
        {
            if(UriScheme == Protocols.Https)
            {
                throw new Exception("Basic Auth cannot be used with Http. To use Basic Auth, UriScheme must be Https.");
            }

            _basicAuthCredentials = credentials;

            _isBasicAuth = true;
        }

        #endregion

        #region GET methods

        //HACK: overload exists because you can't mix defaults with params
        public virtual T GetSingle<T>(string path, params object[] args)
        {
            return GetSingle<T>(path, null, args);
        }

        public virtual T GetSingle<T>(string path, Dictionary<string, object> querystringArgs, params object[] args)
        {
            var json = GetJson(path, querystringArgs, args);
            var item = JsonConvert.DeserializeObject<T>(json);
            return item;
        }

        //HACK: overload exists because you can't mix defaults with params
        public virtual IList<T> GetList<T>(string path, params object[] args)
        {
            return GetList<T>(path, null, args);
        }

        public virtual IList<T> GetList<T>(string path, Dictionary<string, object> querystringArgs, params object[] args)
        {
            var json = GetJson(path, querystringArgs, args);
            var list = JsonConvert.DeserializeObject<List<T>>(json);
            return list;
        }

        //HACK: overload exists because you can't mix defaults with params
        public virtual IEnumerable<T> GetEnumerable<T>(string path, params object[] args)
        {
            return GetEnumerable<T>(path, null, args);
        }

        public virtual IEnumerable<T> GetEnumerable<T>(string path, Dictionary<string, object> querystringArgs, params object[] args)
        {
            var json = GetJson(path, querystringArgs, args);
            var enumerable = JsonConvert.DeserializeObject<IEnumerable<T>>(json);
            return enumerable;
        }

        //TODO: explore refactoring to a single SEND method and parameterize the verb
        private string GetJson(string path, Dictionary<string, object> querystringArgs, params object[] args)
        {
            using (var client = new HttpClient())
            {
                var uri = BuildUri(path, querystringArgs, args);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

                if (_isBasicAuth)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _basicAuthCredentials);
                }
                if (_headersExist)
                {
                    foreach(var header in _headers)
                    {
                        httpRequestMessage.Headers.Add(header.Key, header.Value);
                    }                    
                }
                var response = client.SendAsync(httpRequestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = response.Content.ReadAsStringAsync();
                    return responseJson.Result;
                }
                else
                {
                    //TODO: handle fail status code in a meaningful way
                    return string.Empty;
                }
            }
        }

        #endregion

        #region POST methods

        public virtual R PostSingle<T, R>(string path, T item, params object[] args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region private methods

        private Uri BuildUri(string path, Dictionary<string, object> querystringArgs, params object[] args)
        {
            return new UriBuilder()
            {
                Query = BuildQueryString(querystringArgs),
                Scheme = UriScheme,
                Host = BaseUri,
                Path = string.Format(path, args)
            }.Uri;
        }

        private string BuildQueryString(Dictionary<string, object> querystringArgs)
        {
            if (querystringArgs == null || !querystringArgs.Any())
            {
                return string.Empty;
            }

            return string.Join("&", querystringArgs.Select(x => string.Format("{0}={1}", x.Key, x.Value)));
        }

        #endregion
    }
}
