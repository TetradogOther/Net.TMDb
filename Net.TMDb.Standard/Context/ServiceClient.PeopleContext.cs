using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed partial class ServiceClient
    {
        private sealed class PeopleContext : IPeopleInfo
		{
			private ServiceClient client;

			internal PeopleContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Person> GetAsync(int id, bool appendAll, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>();
				if (appendAll) parameters.Add("append_to_response", "images,external_ids");

                return client.GetAsync<Person>($"person/{id}", parameters, cancellationToken);
			}

			public Task<IEnumerable<PersonCredit>> GetCreditsAsync(int id, string language, DataInfoType type, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("person/{0}/", id);

				switch (type)
				{
					case DataInfoType.Movie: sb.Append("movie_credits"); break;
					case DataInfoType.Television: sb.Append("tv_credits"); break;
					case DataInfoType.Combined: sb.Append("combined_credits"); break;
				}
				var parameters = new Dictionary<string, object> { { "language", language } };

                return client.GetAsync<PersonCredits>(sb.ToString(), parameters, cancellationToken)
                    .ContinueWith(t => ((IEnumerable<PersonCredit>)t.Result.Cast).Concat(t.Result.Crew), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<IEnumerable<Image>> GetImagesAsync(int id, CancellationToken cancellationToken)
			{
                return client.GetAsync<PersonImages>($"person/{id}/images", null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<ExternalIds> GetIdsAsync(int id, CancellationToken cancellationToken)
			{
                return client.GetAsync<ExternalIds>($"person/{id}/external_ids", null, cancellationToken);
			}

			public Task<People> SearchAsync(string query, bool includeAdult, bool autocomplete, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "query", query }, { "page", page }, { "include_adult", includeAdult } };
				if (autocomplete) parameters.Add("search_type", "ngram");

                return client.GetAsync<People>("search/person", parameters, cancellationToken);
			}

			public Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "start_date", minimumDate }, { "end_date", maximumDate } };
                return client.GetAsync<Changes>("person/changes", parameters, cancellationToken);
			}
		}

		
	}
}

#pragma warning restore 1591