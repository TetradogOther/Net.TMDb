using Gabriel.Cat.S.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public class ServiceRequestException : HttpRequestException
	{
		internal ServiceRequestException(int statusCode, int serviceCode, string message) : base(message)
		{
			this.ServiceCode = serviceCode;
			this.StatusCode = statusCode;
		}

#if !PORTABLE
		protected ServiceRequestException(SerializationInfo info, StreamingContext context)
		{
			info.GetInt32("ServiceCode");
			info.GetInt32("StatusCode");
		}
#endif
		public int ServiceCode { get; private set; }

		public int StatusCode { get; private set; }

#if !PORTABLE
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ServiceCode", ServiceCode);
			info.AddValue("StatusCode", StatusCode);
		}
#endif
        internal static Task<HttpResponseMessage> ConvertResponseAsync(HttpResponseMessage response)
        {
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            int statusCode = (int)response.StatusCode;

            if (response.Content != null)
            {
                response.Content.ReadAsStringAsync().ContinueWith(t2 =>
                {
                    JObject? status =JsonConvert.DeserializeObject<JObject>(t2.Result);
                    int serviceCode =status!=null? (status.ContainsKey("status_code") ?status["status_code"].Value<int>():0):0;
                    string? message = status != null ? (status.ContainsKey("errors")) ? String.Join(Environment.NewLine, status["errors"].Value<string>()) :status["status_message"].Value<string>():string.Empty ;
					if(message==null)
						message=String.Empty;
					tcs.TrySetException(new ServiceRequestException(statusCode, serviceCode, message));
                });
            }
            else tcs.TrySetException(new ServiceRequestException(statusCode, 0, response.ReasonPhrase));
            return tcs.Task;
        }
	}
}

#pragma warning restore 1591