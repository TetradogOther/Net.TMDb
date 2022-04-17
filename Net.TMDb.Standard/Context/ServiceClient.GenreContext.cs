using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed partial class ServiceClient
    {
        private sealed class GenreContext : IGenreInfo
		{
			private ServiceClient client;

			internal GenreContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<IEnumerable<Genre>> GetAsync(DataInfoType type, string language, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder("genre");

                switch (type)
				{
					case DataInfoType.Movie: sb.Append("/movie/list"); break;
					case DataInfoType.Television: sb.Append("/tv/list"); break;
					case DataInfoType.Combined: sb.Append("/list"); break;
				}
                var parameters = new Dictionary<string, object> { { "language", language } };

                return client.GetAsync<Genres>(sb.ToString(), parameters, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<Movies> GetMoviesAsync(int id, string language, bool includeAdult, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "include_adult", includeAdult }, { "language", language } };
                return client.GetAsync<Movies>($"genre/{id}/movies", parameters, cancellationToken);
			}
		}

	
	}
}

#pragma warning restore 1591