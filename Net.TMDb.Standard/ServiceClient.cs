using Gabriel.Cat.S.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace System.Net.TMDb
{
    /// <summary>
    /// Contains the various API operations for the service client to interact with The Movie Database.
    /// </summary>
    public sealed partial class ServiceClient : IDisposable
	{
		private readonly string apiKey;
		private readonly HttpClient client;
		private bool disposed = false;

		private static readonly JsonSerializerSettings jsonSettings;
		private static readonly string[] externalSources;
		
		#region Constructors

		public ServiceClient(string apiKey)
		{
            this.apiKey = apiKey;
			this.client = new HttpClient(new Internal.ServiceMessageHandler(
				new HttpClientHandler
				{
					AllowAutoRedirect = false,
					PreAuthenticate = true,
					UseDefaultCredentials = true,
					UseCookies = false,
					AutomaticDecompression =  DecompressionMethods.GZip
				}));
			this.client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));
            this.client.BaseAddress = new Uri("http://api.themoviedb.org/3/");

            this.Movies = new MovieContext(this);
			this.Shows = new ShowsContext(this);
			this.Companies = new CompanyContext(this);
			this.Genres = new GenreContext(this);
			this.People = new PeopleContext(this);
			this.Collections = new CollectionContext(this);
			this.Lists = new ListContext(this);
			this.Reviews = new ReviewContext(this);
			this.Settings = new SystemContext(this);
		}

		static ServiceClient()
		{
			ServiceClient.jsonSettings = new JsonSerializerSettings
			{
				Error = new EventHandler<ErrorEventArgs>((s, e) => OnSerializationError(e))
			};
			ServiceClient.jsonSettings.Converters.Add(new Internal.ResourceCreationConverter());
			ServiceClient.externalSources = new string[] { "imdb_id", "freebase_id", "freebase_mid", "tvdb_id", "tvrage_id" };
		}

		#endregion

		public IMovieInfo Movies { get; private set; }

		public IShowInfo Shows { get; private set; }

		public ICompanyInfo Companies { get; private set; }

		public IGenreInfo Genres { get; private set; }

		public IPeopleInfo People { get; private set; }

		public ICollectionInfo Collections { get; private set; }

		public IListInfo Lists { get; private set; }

		public IReviewInfo Reviews { get; private set; }

		public ISystemInfo Settings { get; private set; }

		#region Disposal Implementation
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		private void Dispose(bool disposing)
		{
			if(!this.disposed)
			{
				if (disposing) // dispose aggregated resources
					this.client.Dispose();
				this.disposed = true; // disposing has been done
			}
		}
		
		#endregion
		
		#region Authentication Methods

		/// <summary>
		/// This method is used to generate a valid request token and authenticate user with a TMDb username and password.
		/// </summary>
		public Task<string> LoginAsync(string username, string password, CancellationToken cancellationToken = default(CancellationToken))
		{
            return GetTokenAsync(cancellationToken)
                .ContinueWith(t => ValidateAsync(t.Result, username, password, cancellationToken), TaskContinuationOptions.OnlyOnRanToCompletion)
                .Unwrap();
		}

		/// <summary>
		/// This method is used to generate a session id for user based authentication, or a guest session if a null token is used.
		/// A session id is required in order to use any of the write methods.
		/// </summary>
		public Task<string> GetSessionAsync(string token, CancellationToken cancellationToken = default(CancellationToken))
		{
			return (token == null ? OpenGuestSessionAsync(cancellationToken) : OpenSessionAsync(token, cancellationToken));
		}

        private Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            return GetAsync<AuthenticationResult>("authentication/token/new", null, cancellationToken)
                .ContinueWith(t => t.Result.Token, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private Task<string> ValidateAsync(string token, string username, string password, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object>
            {
                { "request_token", token },
                { "username", username },
                { "password", password }
            };
            return GetAsync<AuthenticationResult>("authentication/token/validate_with_login", parameters, cancellationToken)
                .ContinueWith(t => t.Result.Token, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private Task<string> OpenSessionAsync(string token, CancellationToken cancellationToken)
		{
			var parameters = new Dictionary<string, object> { { "request_token", token } };
            return GetAsync<AuthenticationResult>("authentication/session/new", parameters, cancellationToken)
                .ContinueWith(t => t.Result.Session, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		private Task<string> OpenGuestSessionAsync(CancellationToken cancellationToken)
		{
			return GetAsync<AuthenticationResult>("authentication/guest_session/new", null, cancellationToken)
                .ContinueWith(t => t.Result.Guest, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        #endregion

        #region Image Storage Methods

        public Task<IImageStorage> GetImageStorageAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Settings.GetConfigurationAsync(cancellationToken)
                .ContinueWith(t =>(IImageStorage) new StorageClient((JObject)t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        #endregion

        #region General Resource Search

        /// <summary>
        /// The find method makes it easy to search for objects in our database by an external id. For instance, an IMDB ID.
        /// This will search all objects (movies, TV shows and people) and return the results in a single response.
        /// </summary>
        /// <remarks>
        /// The supported external sources for each object are as follows:
        /// <list type="bullet">
        /// <item><description>Movies: imdb_id</description></item>
        /// <item><description>People: imdb_id, freebase_mid, freebase_id, tvrage_id</description></item>
        /// <item><description>TV Series: imdb_id, freebase_mid, freebase_id, tvdb_id, tvrage_id</description></item>
        /// <item><description>TV Seasons: freebase_mid, freebase_id, tvdb_id, tvrage_id</description></item>
        /// <item><description>TV Episodes: imdb_id, freebase_mid, freebase_id, tvdb_id, tvrage_id</description></item>
        /// </list>
        /// </remarks>
        public Task<Resource> FindAsync(string id, string externalSource, CancellationToken cancellationToken)
		{
			if (String.IsNullOrWhiteSpace(id))
				throw new ArgumentNullException("id");

			if (String.IsNullOrWhiteSpace(externalSource) || !ServiceClient.externalSources.Contains(externalSource))
				throw new ArgumentNullException("externalSource", "A supported external source must be specified.");

			var parameters = new Dictionary<string, object> { { "external_source", externalSource } };

			return GetAsync<ResourceFindResult>($"find/{id}", parameters, cancellationToken)
                .ContinueWith(t => ((IEnumerable<Resource>)t.Result.Movies).Concat(t.Result.People)
                    .Concat(t.Result.Shows).Concat(t.Result.Seasons).Concat(t.Result.Episodes)
                    .FirstOrDefault(), TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		/// <summary>
		/// Search the movie, tv show and person collections with a single query. Each mapped result is the same response you would get from each independent search.
		/// </summary>
		public Task<Resources> SearchAsync(string query, string language, bool includeAdult, int page, CancellationToken cancellationToken)
		{
			var parameters = new Dictionary<string, object>
			{
				{ "query", query },
				{ "page", page },
				{ "include_adult", includeAdult },
				{ "language", language }
			};
			return GetAsync<Resources>("search/multi", parameters, cancellationToken)
                .ContinueWith(t => t.Result, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		#endregion

		#region Request Handling Methods

		private Task<T> GetAsync<T>(string cmd, IDictionary<string, object>? parameters, CancellationToken cancellationToken)
		{
			return this.client.GetAsync(CreateRequestUri(cmd, parameters), HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ContinueWith(t => DeserializeAsync<T>(t.Result))
				.Unwrap();
		}

        private Task<object?> GetDynamicAsync(string cmd, IDictionary<string, object>? parameters, CancellationToken cancellationToken)
        {
            return this.client.GetAsync(CreateRequestUri(cmd, parameters), HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ContinueWith(t => DeserializeDynamicAsync(t.Result))
                .Unwrap();
        }

		private Task<object?> SendDynamicAsync(string cmd, IDictionary<string, object>? parameters, HttpContent content, HttpMethod method, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(method, CreateRequestUri(cmd, parameters)) { Content = content };
            return this.client.SendAsync(request, cancellationToken)
                .ContinueWith(t => DeserializeDynamicAsync(t.Result))
                .Unwrap();
        }
		
		private static Task<T> DeserializeAsync<T>(HttpResponseMessage response)
		{
			return response.Content.ReadAsStringAsync()
                .ContinueWith<T>(t =>
                {
#if DEBUG
        			System.Diagnostics.Debug.WriteLine(t.Result);
#endif
                    return JsonConvert.DeserializeObject<T>(t.Result, jsonSettings);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		private static Task<object?> DeserializeDynamicAsync(HttpResponseMessage response)
		{
			return response.Content.ReadAsStringAsync()
                .ContinueWith<object?>(t =>
                {
#if DEBUG
        			System.Diagnostics.Debug.WriteLine(t.Result);
#endif
                    return JsonConvert.DeserializeObject(t.Result, jsonSettings);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		private string CreateRequestUri(string cmd, IDictionary<string, object> parameters)
		{
			var sb = new System.Text.StringBuilder($"{cmd}?api_key={apiKey}&");

            if (parameters != null)
			{
                sb.Append(String.Join("&", parameters.Where(s => s.Value != null)
                    .Select(s => String.Concat(s.Key, "=", ConvertParameterValue(s.Value)))));
			}
#if DEBUG
			System.Diagnostics.Debug.WriteLine(sb);
#endif
			return sb.ToString();
		}

		private static string ConvertParameterValue(object value)
		{
			Type t = value.GetType();
			t = Nullable.GetUnderlyingType(t) ?? t;
			
			if (t == typeof(DateTime)) return ((DateTime)value).ToString("yyyy-MM-dd");
			else if (t == typeof(Decimal)) return ((Decimal)value).ToString(CultureInfo.InvariantCulture);
			else return Uri.EscapeDataString(value.ToString());
		}

		#endregion

		#region Serialization Event Handlers

		private static void OnSerializationError(Newtonsoft.Json.Serialization.ErrorEventArgs args)
		{
			System.Diagnostics.Debug.WriteLine(args.ErrorContext.Error.Message);
			args.ErrorContext.Handled = true;
		}

#endregion

	}
}

#pragma warning restore 1591