using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed partial class ServiceClient
    {
        private sealed class ReviewContext : IReviewInfo
		{
			private ServiceClient client;

			internal ReviewContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Review> GetAsync(string id, CancellationToken cancellationToken)
			{
                return client.GetAsync<Review>($"review/{id}", null, cancellationToken);
			}

		}

		
	}
}

#pragma warning restore 1591