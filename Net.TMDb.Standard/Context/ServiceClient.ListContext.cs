using Gabriel.Cat.S.Extension;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed partial class ServiceClient
    {
        private sealed class ListContext : IListInfo
		{
			private ServiceClient client;

			internal ListContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<List> GetAsync(string id, CancellationToken cancellationToken)
			{
                return client.GetAsync<List>($"list/{id}", new SortedList<string,object>(), cancellationToken);
			}

			public Task<bool> ContainsAsync(string id, int movieId, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "movie_id", movieId } };
                return client.GetDynamicAsync($"list/{id}/item_status", parameters, cancellationToken)
                    .ContinueWith(t => (bool)t.GetProperty("item_present"), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            public Task<string> CreateAsync(string session, string name, string description, string language, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format((language == null) ? "{{\"name\":\"{0}\",\"description\":\"{1}\"}}" :
					"{{\"name\":\"{0}\",\"description\":\"{1}\",\"language\":\"{2}\"}}", name, description, language);

                return client.SendDynamicAsync("list", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (string)("list_id"), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> InsertAsync(string session, string id, string mediaId, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_id\":\"{0}\"}}", mediaId);

                return client.SendDynamicAsync($"list/{id}/add_item", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> RemoveAsync(string session, string id, string mediaId, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_id\":\"{0}\"}}", mediaId);

                return client.SendDynamicAsync($"list/{id}/remove_item", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            public Task<bool> ClearAsync(string session, string id, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session }, { "confirm", "true" } };
                return client.SendDynamicAsync($"list/{id}/clear", parameters, null, HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public  Task<bool> DeleteAsync(string session, string id, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				return client.SendDynamicAsync($"list/{id}", parameters, null, HttpMethod.Delete, cancellationToken)
							 .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
			}
		}

		
	}
}

#pragma warning restore 1591