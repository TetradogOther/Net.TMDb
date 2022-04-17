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
        private sealed class ShowsContext : IShowInfo
		{
			private ServiceClient client;

			internal ShowsContext(ServiceClient client)
			{
				this.client = client;
			}

			public Task<Shows> SearchAsync(string query, string language, DateTime? firstAirDate, bool autocomplete, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "query", query },
					{ "page", page },
					{ "language", language },
					{ "first_air_date_year", firstAirDate }
				};
				if (autocomplete) parameters.Add("search_type", "ngram");

                return client.GetAsync<Shows>("search/tv", parameters, cancellationToken);
			}

			public Task<Shows> DiscoverAsync(string language, int? year, DateTime? minimumDate, DateTime? maximumDate, int? voteCount, decimal? voteAverage, string genres, string networks, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object>
				{
					{ "page", page },
					{ "language", language },
					{ "first_air_date_year", year },
					{ "first_air_date.gte", minimumDate },
					{ "first_air_date.lte", maximumDate },
					{ "vote_count.gte", voteCount },
					{ "vote_average.gte", voteAverage },
					{ "with_genres", genres },
					{ "with_networks", networks },
				};
                return client.GetAsync<Shows>("discover/tv", parameters, cancellationToken);
			}

			public Task<Show> GetAsync(int id, string language, bool appendAll, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "images,credits,keywords,videos,translations,external_ids");

                return client.GetAsync<Show>($"tv/{id}", parameters, cancellationToken);
			}

			public Task<Show> GetLatestAsync(CancellationToken cancellationToken)
			{
                return client.GetAsync<Show>("tv/latest", null, cancellationToken);
            }

            public Task<Season> GetSeasonAsync(int id, int season, string language, bool appendAll, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "images,credits,videos,external_ids");

                return client.GetAsync<Season>($"tv/{id}/season/{season}", parameters, cancellationToken);
			}

			public Task<Episode> GetEpisodeAsync(int id, int season, int episode, string language, bool appendAll, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "language", language } };
				if (appendAll) parameters.Add("append_to_response", "images,credits,videos,external_ids");

                return client.GetAsync<Episode>($"tv/{id}/season/{season}/episode/{episode}", parameters, cancellationToken);
			}

			public Task<ExternalIds> GetIdsAsync(int id, int? season, int? episode, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("tv/{0}", id);

				if (season.HasValue)
				{
					sb.AppendFormat("/season/{0}", season.Value);
					if (episode.HasValue) sb.AppendFormat("/episode/{1}", episode.Value);
				}
				sb.Append("/external_ids");

                return client.GetAsync<ExternalIds>(sb.ToString(), null, cancellationToken);
			}

			public Task<IEnumerable<MediaCredit>> GetCreditsAsync(int id, CancellationToken cancellationToken)
			{
                return client.GetAsync<MediaCredits>($"tv/{id}/credits", null, cancellationToken)
                    .ContinueWith(t => ((IEnumerable<MediaCredit>)t.Result.Cast).Concat(t.Result.Crew), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<Images> GetImagesAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("tv/{0}", id);

				if (season.HasValue)
				{
					sb.AppendFormat("/season/{0}", season.Value);
					if (episode.HasValue) sb.AppendFormat("/episode/{1}", episode.Value);
				}
				sb.Append("/images");
				var parameters = new Dictionary<string, object> { { "language", language } };

                return client.GetAsync<Images>(sb.ToString(), parameters, cancellationToken);
			}

			public Task<Shows> GetSimilarAsync(int id, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>($"tv/{id}/similar", parameters, cancellationToken);
			}

			public Task<IEnumerable<Video>> GetVideosAsync(int id, int? season, int? episode, string language, CancellationToken cancellationToken)
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("tv/{0}", id);

				if (season.HasValue)
				{
					sb.AppendFormat("/season/{0}", season.Value);
					if (episode.HasValue) sb.AppendFormat("/episode/{1}", episode.Value);
				}
				sb.Append("/videos");
				var parameters = new Dictionary<string, object> { { "language", language } };

				return client.GetAsync<Videos>(sb.ToString(), parameters, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<IEnumerable<Translation>> GetTranslationsAsync(int id, CancellationToken cancellationToken)
			{
				return client.GetAsync<Translations>($"tv/{id}/translations", null, cancellationToken)
                    .ContinueWith(t => t.Result.Results, TaskContinuationOptions.OnlyOnRanToCompletion);
            }

			public Task<Shows> GetOnAirAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>("tv/on_the_air ", parameters, cancellationToken);
			}

			public Task<Shows> GetAiringAsync(string language, int page, string timezone, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language }, { "timezone", timezone } };
                return client.GetAsync<Shows>("tv/airing_today", parameters, cancellationToken);
            }

            public Task<Shows> GetPopularAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>("tv/popular", parameters, cancellationToken);
            }

            public Task<Shows> GetTopRatedAsync(string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>("tv/top_rated", parameters, cancellationToken);
            }

            public Task<Changes> GetChangesAsync(DateTime? minimumDate, DateTime? maximumDate, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "start_date", minimumDate }, { "end_date", maximumDate } };
                return client.GetAsync<Changes>("tv/changes", parameters, cancellationToken);
			}

			public Task<string> GetNetworkAsync(int id, CancellationToken cancellationToken)
			{
                return client.GetDynamicAsync($"network/{id}", null, cancellationToken)
                    .ContinueWith(t => (string)t.GetProperty("name"), TaskContinuationOptions.OnlyOnRanToCompletion);
			}
			
			public Task<Shows> GetAccountRatedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>($"account/{session}/rated/tv", parameters, cancellationToken);
            }

            public Task<Shows> GetFavoritedAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>($"account/{session}/favorite/tv", parameters, cancellationToken);
            }

            public Task<Shows> GetWatchlistAsync(string session, string language, int page, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "page", page }, { "language", language } };
                return client.GetAsync<Shows>($"account/{session}/watchlist/tv", parameters, cancellationToken);
            }

            public Task<bool> SetRatingAsync(string session, int id, decimal value, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"value\":\"{0}\"}}", value);

                return client.SendDynamicAsync($"tv/{id}/rating", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 1), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> SetRatingAsync(string session, int id, int season, int episode, decimal value, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"value\":\"{0}\"}}", value);

				return client.SendDynamicAsync($"tv/{id}/season/{season}/episode/{episode}/rating", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 1), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> SetFavoriteAsync(string session, int id, bool value, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_type\":\"tv\",\"media_id\":\"{0}\",\"favorite\":\"{1}\"}}", id, value);

                return client.SendDynamicAsync($"account/{id}/favorite", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
			}

			public Task<bool> SetWatchlistAsync(string session, int id, bool value, CancellationToken cancellationToken)
			{
				var parameters = new Dictionary<string, object> { { "session_id", session } };
				string content = String.Format("{{\"media_type\":\"tv\",\"media_id\":\"{0}\",\"watchlist\":\"{1}\"}}", id, value);

                return client.SendDynamicAsync($"account/{id}/watchlist", parameters, new StringContent(content, null, "application/json"), HttpMethod.Post, cancellationToken)
                    .ContinueWith(t => (bool)((int)t.GetProperty("status_code") == 12), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
		}

		
	}
}

#pragma warning restore 1591