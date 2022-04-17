using Gabriel.Cat.S.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    public sealed partial class ServiceClient
    {
     
        private sealed class MovieContext : IMovieInfo
		{
			private ServiceClient client;

			internal MovieContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Movies> SearchAsync(string query, string language, bool includeAdult, int? year, bool autocomplete, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "query", query },
					{ "page", page },
					{ "include_adult", includeAdult },
					{ "language", language },
					{ "year", year }
				};
				if (autocomplete) parameters.Add("search_type", "ngram");

				return client.GetAsync<Movies>("search/movie", parameters, cancellationToken);
			}

			public Task<Movies> DiscoverAsync(string language, bool includeAdult, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string companies, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "page", page },
					{ "include_adult", includeAdult },
					{ "language", language },
					{ "year", year },
					{ "release_date.gte", minimumDate },
					{ "release_date.lte", maximumDate },
					{ "vote_count.gte", voteCount },
					{ "vote_average.gte", voteAverage },
					{ "with_genres", genres },
					{ "with_companies", companies },
				};
				return client.GetAsync<Movies>("discover/movie", parameters, cancellationToken);
			}

			public Task<Movie> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "alternative_titles,images,credits,keywords,releases,videos,translations,reviews,external_ids");

				return client.GetAsync<Movie>($"movie/{id}", parameters, cancellationToken);
			}

			public Task<Images> GetImagesAsync(int id, string language, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "language", language } };
				return client.GetAsync<Images>($"movie/{id}/images", parameters, cancellationToken);
			}

			public Task<IEnumerable<MediaCredit>> GetCreditsAsync(int id, CancellationToken cancellationToken)
			{
                return client.GetAsync<MediaCredits>($"movie/{id}/credits", null, cancellationToken)
                    .ContinueWith(t => ((IEnumerable<MediaCredit>)t.Result.Cast).Concat(t.Result.Crew), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<IEnumerable<Video>> GetVideosAsync(int id, string language, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "language", language } };
                return client.GetAsync<Videos>($"movie/{id}/videos", parameters, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<Reviews> GetReviewsAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Reviews>($"movie/{id}/reviews", parameters, cancellationToken);
			}

			public Task<Lists> GetListsAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Lists>($"movie/{id}/lists", parameters, cancellationToken);
			}

			public Task<Movies> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>($"movie/{id}/similar_movies", parameters, cancellationToken);
			}

			public Task<Movies> GetGuestRatedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>($"guest_session/{session}/rated_movies", parameters, cancellationToken);
            }

            public Task<Movies> GetPopularAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>("movie/popular", parameters, cancellationToken);
            }

            public Task<Movies> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>("movie/top_rated", parameters, cancellationToken);
            }

            public Task<Movies> GetNowPlayingAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>("movie/now_playing", parameters, cancellationToken);
            }

            public Task<Movies> GetUpcomingAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
				return client.GetAsync<Movies>("movie/upcoming", parameters, cancellationToken);
            }

            public Task<IEnumerable<AlternativeTitle>> GetAlternativeTitlesAsync(int id, string language, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "country", language } };
				return client.GetAsync<AlternativeTitles>($"movie/{id}/alternative_titles", parameters, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<IEnumerable<Keyword>> GetKeywordsAsync(int id, CancellationToken cancellationToken)
			{
				return client.GetAsync<Keywords>($"movie/{id}/keywords", null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<IEnumerable<Release>> GetReleasesAsync(int id, CancellationToken cancellationToken)
			{
				return client.GetAsync<Releases>($"movie/{id}/releases", null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken)
			{
				return client.GetAsync<Translations>($"movie/{id}/translations", null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "start_date", minimumDate }, { "end_date", maximumDate } };
				return client.GetAsync<Changes>("movie/changes", parameters, cancellationToken);
			}

			public Task<Movies> GetAccountRatedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Movies>($"account/{session}/rated/movies", parameters, cancellationToken);
			}

			public Task<Movies> GetFavoritedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Movies>($"account/{session}/favorite/movies", parameters, cancellationToken);
            }

            public Task<Movies> GetWatchlistAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Movies>($"account/{session}/watchlist/movies", parameters, cancellationToken);
            }

            public Task<bool> SetRatingAsync(string session, int id, decimal value, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"value\":\"{0}\"}}", value);

                return client.SendDynamicAsync($"movie/{id}/rating", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> SetFavoriteAsync(string session, int id, bool value, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_type\":\"movie\",\"media_id\":\"{0}\",\"favorite\":\"{1}\"}}", id, value);

                return client.SendDynamicAsync($"account/{id}/favorite", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<bool> SetWatchlistAsync(string session, int id, bool value, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_type\":\"movie\",\"media_id\":\"{0}\",\"watchlist\":\"{1}\"}}", id, value);

                return client.SendDynamicAsync($"account/{id}/watchlist", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
		}

	}
}

#pragma warning restore 1591