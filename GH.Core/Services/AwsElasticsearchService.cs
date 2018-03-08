using GH.Core.Models;
using GH.Core.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GH.Core.Services
{
    public class AwsElasticsearchService : IAwsElasticsearchService
    {
        public const string HTTP_SCHEME = "https://";
        private readonly string AWS_ES_END_POINT = null;


        public const string SOCIAL_POST_INDEX = "social-posts";
        public const string SOCIAL_POST_INDEX_TYPE = "post";
        public const string ADD_SOCIAL_POST_BASE_CANONICAL_URI = "/social-posts/post/";
        public const string SEARCH_SOCIAL_POST_API_CANONICAL_URI = "/social-posts/_search";
        public const string DELETE_SOCIAL_POST_INDEX_CANONICAL_URI = "/social-posts/";
        public const string PUT_SOCIAL_POST_INDEX_CANONICAL_URI = "/social-posts";

        public const string USER_INDEX = "users";
        public const string USER_INDEX_TYPE = "user";
        public const string SEARCH_USER_API_CANONICAL_URI = "/users/_search";
        public const string DELETE_USER_INDEX_CANONICAL_URI = "/users/";
        public const string PUT_USER_INDEX_CANONICAL_URI = "/users";


        public const string BULK_DOCUMENTS_API_CANONICAL_URI = "/_bulk";
        private const string ACCESS_KEY = "AKIAJNOERU44ZMNGQR6Q";
        private const string SECRET_KEY = "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";

        public AwsElasticsearchService()
        {
            AWS_ES_END_POINT = ConfigurationManager.AppSettings["AWS.ES.END_POINT"];
        }
        
        public async Task DeleteIndex(string canonicalURI)
        {
            var now = DateTime.Now.ToUniversalTime();

            var headers = new AwsElasticsearchHeaders(AWS_ES_END_POINT, RestApiHttpMethod.DELETE, now, AwsElasticsearchService.ACCESS_KEY, AwsElasticsearchService.SECRET_KEY, canonicalURI, null, null);

            HttpClient client = GetClient(headers);
            var response = await client.DeleteAsync(headers.RequestUrl);
            if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task Index(string canonicalURI, string settings = null)
        {
            var now = DateTime.Now.ToUniversalTime();

            var body = settings;

            var headers = new AwsElasticsearchHeaders(AWS_ES_END_POINT, RestApiHttpMethod.PUT, now, AwsElasticsearchService.ACCESS_KEY, AwsElasticsearchService.SECRET_KEY, canonicalURI, null, body);

            HttpClient client = GetClient(headers);
            var response = await client.PutAsync(headers.RequestUrl, new StringContent(body));

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<dynamic> AddOrUpdateDocument(string canonicalURI, string id, dynamic document)
        {
            string body = JsonConvert.SerializeObject(document);

            string canonicalString = AwsElasticsearchService.ADD_SOCIAL_POST_BASE_CANONICAL_URI + id;

            var now = DateTime.Now.ToUniversalTime();

            var headers = new AwsElasticsearchHeaders(AWS_ES_END_POINT, RestApiHttpMethod.PUT, now, AwsElasticsearchService.ACCESS_KEY, AwsElasticsearchService.SECRET_KEY, canonicalString, null, body);

            HttpClient client = GetClient(headers);
            var response = await client.PutAsync(headers.RequestUrl, new StringContent(body));
            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }

        public async Task<dynamic> AddDocuments(string index, string indexType, string idField, params dynamic[] documents)
        {
            StringBuilder body = new StringBuilder();
            foreach (var document in documents)
            {
                CreateDocAction docAction = new CreateDocAction
                {
                    create = new DocActionMeta
                    {
                        _id = document.GetType().GetProperty(idField).GetValue(document).ToString(),
                        _index = index,
                        _type = indexType
                    }
                };


                var actionJson = JsonConvert.SerializeObject(docAction, Formatting.None);
                var dataJson = JsonConvert.SerializeObject(document, Formatting.None);

                body.Append(actionJson + "\n" + dataJson + "\n");
            }

            string canonicalString = AwsElasticsearchService.BULK_DOCUMENTS_API_CANONICAL_URI;

            var now = DateTime.Now.ToUniversalTime();

            var headers = new AwsElasticsearchHeaders(AWS_ES_END_POINT, RestApiHttpMethod.POST, now, AwsElasticsearchService.ACCESS_KEY, AwsElasticsearchService.SECRET_KEY, canonicalString, null, body.ToString());

            HttpClient client = GetClient(headers);
            var response = await client.PostAsync(headers.RequestUrl, new StringContent(body.ToString()));
            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }

        public async Task<dynamic> AddOrUpdateDocuments(string index, string indexType, string idField, params dynamic[] documents)
        {
            StringBuilder body = new StringBuilder();
            foreach (var document in documents)
            {
                IndexDocAction docAction = new IndexDocAction
                {
                    index = new DocActionMeta
                    {
                        _id = document.GetType().GetProperty(idField).GetValue(document).ToString(),
                        _index = index,
                        _type = indexType
                    }
                };

                var actionJson = JsonConvert.SerializeObject(docAction, Formatting.None);
                var dataJson = JsonConvert.SerializeObject(document, Formatting.None);

                body.Append(actionJson + "\n" + dataJson + "\n");
            }

            string canonicalString = AwsElasticsearchService.BULK_DOCUMENTS_API_CANONICAL_URI;

            var now = DateTime.Now.ToUniversalTime();

            var headers = new AwsElasticsearchHeaders(AWS_ES_END_POINT, RestApiHttpMethod.POST, now, AwsElasticsearchService.ACCESS_KEY, AwsElasticsearchService.SECRET_KEY, canonicalString, null, body.ToString());

            HttpClient client = GetClient(headers);
            var response = await client.PostAsync(headers.RequestUrl, new StringContent(body.ToString()));
            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }

        public async Task<EsSearchResult> SearchDocuments(string canonicalURI, dynamic query, int? start, int? length)
        {
            string canonicalString = canonicalURI;

            var now = DateTime.Now.ToUniversalTime();

            var body = JsonConvert.SerializeObject(query);

            string paginationQuery = null;
            if (start.HasValue && start.Value >= 0 && length.HasValue && length > 0)
            {
                paginationQuery = string.Format("size={0}&from={1}", length.Value, start.Value);
            }

            var headers = new AwsElasticsearchHeaders(AWS_ES_END_POINT, RestApiHttpMethod.POST, now, AwsElasticsearchService.ACCESS_KEY, AwsElasticsearchService.SECRET_KEY, canonicalString, paginationQuery, body);

            HttpClient client = GetClient(headers);
            var response = await client.PostAsync(headers.RequestUrl, new StringContent(body));

            var content = await response.Content.ReadAsStringAsync();
            var result = await response.Content.ReadAsAsync<EsSearchResult>();
            return result;
        }

        private HttpClient GetClient(AwsElasticsearchHeaders headers)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Host = headers.Host;
            client.DefaultRequestHeaders.Authorization = headers.Authorization;
            foreach (var item in headers.AddtionalHeaders)
            {
                client.DefaultRequestHeaders.Add(item.Key, item.Value);
            }

            return client;
        }

        internal class AwsElasticsearchHeaders
        {
            public const string ENCRYPT_METHOD = "AWS4-HMAC-SHA256";

            public string Host { get; private set; }
            public string RequestUrl { get; private set; }
            public AuthenticationHeaderValue Authorization { get; private set; }
            public IDictionary<string, string> AddtionalHeaders { get; set; }
            public DateTime Date { get; private set; }
            public DateTime DateVersion { get; set; }

            public AwsElasticsearchHeaders(string host, RestApiHttpMethod method, DateTime dateVersion, string accessKey, string secretKey, string canonicalURI, string queryString, string bodyPayload)
            {
                //setup info to generate headers
                this.Host = host;

                //aws will check this time for prevent over 5 minutes delay request
                this.Date = DateTime.Now.ToUniversalTime();
                this.DateVersion = dateVersion;

                //content headers key-val pair to add to HttpClient
                this.AddtionalHeaders = new Dictionary<string, string>();
                this.AddtionalHeaders.Add("x-amz-date", GetDateTimeString());

                //get HttpMethod name from enum
                string httpRequestMethod = method.ToString();

                //query string or body of request should be process then it should be empty string if it is null
                var parsedQueryString = string.IsNullOrEmpty(queryString) ? "" : EncodeQueryString(queryString); //percent encode
                bodyPayload = string.IsNullOrEmpty(bodyPayload) ? "" : bodyPayload;

                //canonical headers are string combine of header-key:value+"\n" sorted by header key follow ASCII code
                string canonicalHeaders = "host:" + host + "\n" + string.Join("\n", this.AddtionalHeaders.Select(a => a.Key + ":" + a.Value)) + "\n";

                //signed headers are string combine of header-key join by "\n";
                string signedHeaders = "host;" + string.Join("\n", this.AddtionalHeaders.Select(a => a.Key));

                //hash the body by SHA256 then Encode it by Base16
                string hashPayload = BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(bodyPayload))).Replace("-", "").ToLower();

                //Canonical Request is string combine of HttpRequestMethod, CanonicalURI, percent encoded QueryString, CanonicalHeaders, SignedHeaders and Body Payload - JOIN by "\n"
                //The canonical URI is the URI-encoded version of the absolute path component of the URI, which is everything in the URI from the HTTP host to the question mark character ("?") that begins the query string parameters (if any).
                string canonicalRequest = httpRequestMethod + "\n" + canonicalURI + "\n" + parsedQueryString + "\n" + canonicalHeaders + "\n" + signedHeaders + "\n" + hashPayload;

                //Get Base16 encoded string of SHA256(CanonicalRequest)
                //Use for build the Authorization Header
                string hashedCanonicalRequest = BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest))).Replace("-", "").ToLower();

                //process the host to identify the AWS Region
                string awsRegion = host.Split('.')[1];

                //Use for build the Authorization Header
                string credentialScope = GetDateVersionString() + "/" + awsRegion + "/es/aws4_request";

                //Use for build Signature
                string stringToSign = AwsElasticsearchHeaders.ENCRYPT_METHOD + '\n'
                               + GetDateTimeString() + '\n'
                               + credentialScope + '\n' + hashedCanonicalRequest;

                //transfer secretkey to derived signing key for security
                byte[] derivedSigningKey = GetSignatureKey(secretKey, GetDateVersionString(), awsRegion, "es");

                //Get final Signature
                string signature = BitConverter.ToString(HmacSHA256(stringToSign, derivedSigningKey)).Replace("-", "").ToLower();

                string authorizationHeader = "Credential=" + accessKey + "/" + credentialScope + ", SignedHeaders=" + signedHeaders + ", Signature=" + signature;

                this.Authorization = new AuthenticationHeaderValue(AwsElasticsearchHeaders.ENCRYPT_METHOD, authorizationHeader);

                this.RequestUrl = AwsElasticsearchService.HTTP_SCHEME + this.Host + canonicalURI + "?" + queryString;
            }

            private byte[] HmacSHA256(string data, byte[] key)
            {
                string algorithm = "HmacSHA256";
                KeyedHashAlgorithm kha = KeyedHashAlgorithm.Create(algorithm);
                kha.Key = key;

                return kha.ComputeHash(Encoding.UTF8.GetBytes(data));
            }

            private byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
            {
                byte[] kSecret = Encoding.UTF8.GetBytes(("AWS4" + key).ToCharArray());
                byte[] kDate = HmacSHA256(dateStamp, kSecret);
                byte[] kRegion = HmacSHA256(regionName, kDate);
                byte[] kService = HmacSHA256(serviceName, kRegion);
                byte[] kSigning = HmacSHA256("aws4_request", kService);

                return kSigning;
            }

            private string EncodeQueryString(string query)
            {
                var keysVals = query.Split('&');
                IDictionary<string, string> queries = new Dictionary<string, string>();
                foreach (var kv in keysVals)
                {
                    var splits = kv.Split('=');
                    var key = splits[0];
                    var val = splits[1];
                    if (!string.IsNullOrEmpty(val))
                    {
                        val = Uri.EscapeDataString(val);
                    }
                    queries.Add(key, val);
                }

                var encodedQuery = EncodeSpecialCharacters(string.Join("&", queries.OrderBy(q => q.Key).Select(v => Uri.EscapeDataString(v.Key) + (!string.IsNullOrEmpty(v.Value) ? "=" + v.Value : ""))));

                return encodedQuery;
            }

            private string EncodeSpecialCharacters(string data)
            {
                //Do not URI-encode any of the unreserved characters that RFC 3986 defines: A-Z, a-z, 0-9, hyphen ( - ), underscore ( _ ), period ( . ), and tilde ( ~ )
                if (data.Contains("!"))
                    data = data.Replace("!", "%21");
                if (data.Contains("'"))
                    data = data.Replace("'", "%27");
                if (data.Contains("("))
                    data = data.Replace("(", "%28");
                if (data.Contains(")"))
                    data = data.Replace(")", "%29");
                if (data.Contains("*"))
                    data = data.Replace("*", "%2A");
                if (data.Contains(","))
                    data = data.Replace(",", "%2C");

                return data;
            }

            public string GetDateTimeString()
            {
                return this.Date.ToString("yyyyMMddThhmmssZ");
            }

            public string GetDateVersionString()
            {
                return this.DateVersion.ToString("yyyyMMdd");
            }
        }

        internal class CreateDocAction
        {
            public DocActionMeta create { get; set; }
        }

        internal class IndexDocAction
        {
            public DocActionMeta index { get; set; }
        }

        internal class DeleteDocAction
        {
            public DocActionMeta delete { get; set; }
        }

        internal class DocActionMeta
        {
            public string _index { get; set; }
            public string _type { get; set; }
            public string _id { get; set; }
        }

        public enum RestApiHttpMethod
        {
            GET,
            DELETE,
            PATCH,
            POST,
            PUT
        }
    }


    public static class AwsElasticsearchIndexSettings
    {
        public static string GetSocialPostIndexSettingsString()
        {
            return "{\"settings\":{\"analysis\":{\"analyzer\":{\"greenhouse_index_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"standard\",\"filter\":[\"lowercase\",\"asciifolding\",\"word_delimiter\"]},\"greenhouse_search_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"standard\",\"filter\":[\"lowercase\",\"asciifolding\",\"word_delimiter\"]}},\"tokenizer\":{\"nGram\":{\"type\":\"nGram\",\"min_gram\":2,\"max_gram\":20}}}},\"mappings\":{\"post\":{\"properties\":{\"Message\":{\"type\":\"string\",\"analyzer\":\"greenhouse_index_analyzer\",\"search_analyzer\":\"greenhouse_search_analyzer\"}}}}}";
        }

        public static string GetUserIndexSettingsString()
        {
            return "{\"settings\":{\"analysis\":{\"analyzer\":{\"greenhouse_index_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"standard\",\"filter\":[\"lowercase\",\"asciifolding\",\"word_delimiter\"]},\"greenhouse_search_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"standard\",\"filter\":[\"lowercase\",\"asciifolding\",\"word_delimiter\"]}}}},\"mappings\":{\"user\":{\"properties\":{\"Name\":{\"type\":\"string\",\"analyzer\":\"greenhouse_index_analyzer\",\"search_analyzer\":\"greenhouse_search_analyzer\"}}}}}";
        }
    }

    public class EsSearchQueryString
    {
        public string[] fields { get; set; }
        public EsQueryStringType query { get; set; }
        public object[] sort { get; set; }
    }

    public class EsQueryStringType
    {
        public EsQueryStringValue query_string { get; set; }

    }

    public class EsQueryStringValue
    {
        public string query { get; set; }
    }
}