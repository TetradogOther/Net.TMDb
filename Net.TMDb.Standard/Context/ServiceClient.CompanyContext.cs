using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed partial class ServiceClient
    {
        private sealed class CompanyContext : ICompanyInfo
		{
			private ServiceClient client;

			internal CompanyContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Company> GetAsync(int id, CancellationToken cancellationToken)
			{
                return client.GetAsync<Company>($"company/{id}", null, cancellationToken);
			}

			public Task<Movies> GetMoviesAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Movies>($"company/{id}/movies", parameters, cancellationToken);
			}

			public Task<Companies> SearchAsync(string query, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "query", query }, { "page", page } };
                return client.GetAsync<Companies>("search/company", parameters, cancellationToken);
			}
		}

	
	}
}

#pragma warning restore 1591