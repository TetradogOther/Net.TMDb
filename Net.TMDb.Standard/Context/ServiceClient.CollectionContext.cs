using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed partial class ServiceClient
    {
        private sealed class CollectionContext : ICollectionInfo
		{
			private ServiceClient client;

			internal CollectionContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Collection> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "images");

                return client.GetAsync<Collection>($"collection/{id}", parameters, cancellationToken);
			}

			public Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "language", language } };
                return client.GetAsync<Images>($"collection/{id}/images", parameters, cancellationToken);
			}

			public Task<Collections> SearchAsync(string query, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "query", query }, { "page", page }, { "language", language } };
                return client.GetAsync<Collections>("search/collection", parameters, cancellationToken);
			}
		}

	}
}

#pragma warning restore 1591