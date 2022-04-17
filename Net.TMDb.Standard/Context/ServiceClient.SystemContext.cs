using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed partial class ServiceClient
    {
        private sealed class SystemContext : ISystemInfo
		{
			private ServiceClient client;

			internal SystemContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Account> GetAccountAsync(string session, CancellationToken cancellationToken)
			{
                var parameters = new Dictionary<string, object> { { "session_id", session }};
                return client.GetAsync<Account>("account", parameters, cancellationToken);
			}

			public Task<IEnumerable<Certification>> GetCertificationsAsync(DataInfoType type, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder("certification");

				switch (type)
				{
					case DataInfoType.Movie: sb.Append("/movie/list"); break;
					case DataInfoType.Television: sb.Append("/tv/list"); break;
					case DataInfoType.Combined: sb.Append("/list"); break;
				}
				return client.GetAsync<Certifications>(sb.ToString(), null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<object?> GetConfigurationAsync(CancellationToken cancellationToken)
			{
                return client.GetDynamicAsync("configuration", null, cancellationToken);
			}

			public Task<object?> GetTimezonesAsync(CancellationToken cancellationToken)
			{
                return client.GetDynamicAsync("timezones/list", null, cancellationToken);
			}

			public Task<IEnumerable<Job>> GetJobsAsync(CancellationToken cancellationToken)
			{
                return client.GetAsync<Jobs>("job/list", null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}
		}

		
	}
}

#pragma warning restore 1591