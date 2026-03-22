using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Repository.Interfaces;
using Repository.Models;
using Repository.Models.Elastic;
using Repository.Models.Enums;
using RepositoryQuery = Repository.Models.Query;
using SearchQuery = Elastic.Clients.Elasticsearch.QueryDsl.Query;

namespace Repository.Implementations
{
    public class ElasticSearchService : IElasticSearchService
    {
        private const string IndexName = "queries";
        private readonly ElasticsearchClient _client;

        public ElasticSearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        // ── Index management ─────────────────────────────────────────────

        public async Task EnsureIndexAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(IndexName);
            if (existsResponse.Exists) return;

            var createResponse = await _client.Indices.CreateAsync(IndexName);
            if (!createResponse.IsValidResponse)
                throw new InvalidOperationException("Elasticsearch could not create the 'queries' index.");
        }

        // ── Write operations ──────────────────────────────────────────────
        // These accept the base Query (used during bootstrap from QueryRepository.GetAll())

        public async Task IndexQueryAsync(RepositoryQuery query)
        {
            try
            {
                await EnsureIndexAsync();
                var document = ToDocument(query);
                var response = await _client.IndexAsync(document, i => i.Index(IndexName).Id(document.QueryId));
                if (!response.IsValidResponse)
                    Console.WriteLine($"[ES] Failed to index query {query.QueryId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ES] Failed to index query {query.QueryId}: {ex.Message}");
            }
        }

        public async Task UpdateQueryAsync(RepositoryQuery query)
        {
            try
            {
                await EnsureIndexAsync();
                var document = ToDocument(query);
                var existingResponse = await _client.GetAsync<QueryDocument>(IndexName, document.QueryId);
                if (existingResponse.Found && existingResponse.Source != null)
                {
                    document.CompanyName = existingResponse.Source.CompanyName;
                    document.EmployeeName = existingResponse.Source.EmployeeName;
                }

                var response = await _client.IndexAsync(document, i => i.Index(IndexName).Id(document.QueryId));
                if (!response.IsValidResponse)
                    Console.WriteLine($"[ES] Failed to update query {query.QueryId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ES] Failed to update query {query.QueryId}: {ex.Message}");
            }
        }

        public async Task UpsertAdminQueryAsync(AdminQuery query)
        {
            try
            {
                await EnsureIndexAsync();
                var document = ToAdminDocument(query);
                var response = await _client.IndexAsync(document, i => i.Index(IndexName).Id(document.QueryId));
                if (!response.IsValidResponse)
                    Console.WriteLine($"[ES] Failed to upsert admin query {query.QueryId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ES] Failed to upsert admin query {query.QueryId}: {ex.Message}");
            }
        }

        public async Task DeleteQueryAsync(int queryId)
        {
            try
            {
                await EnsureIndexAsync();
                var response = await _client.DeleteAsync(IndexName, queryId);
                if (!response.IsValidResponse)
                    Console.WriteLine($"[ES] Failed to delete query {queryId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ES] Failed to delete query {queryId}: {ex.Message}");
            }
        }

        // ── Employee-side searches (return base Query) ────────────────────

        public async Task<List<RepositoryQuery>> SearchByTitleAsync(string titleKeyword)
        {
            if (string.IsNullOrWhiteSpace(titleKeyword)) return new List<RepositoryQuery>();

            await EnsureIndexAsync();
            var request = new SearchRequest<QueryDocument>(IndexName)
            {
                Size = 100,
                Query = BuildKeywordQuery(titleKeyword, titleOnly: true)
            };
            var response = await _client.SearchAsync<QueryDocument>(request);
            return MapResponseToQuery(response, $"search title '{titleKeyword}'");
        }

        public async Task<List<RepositoryQuery>> SearchEmployeeQueriesAsync(int empId, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<RepositoryQuery>();

            await EnsureIndexAsync();
            var request = new SearchRequest<QueryDocument>(IndexName)
            {
                Size = 200,
                Query = new BoolQuery
                {
                    Must = new List<SearchQuery>
                    {
                        BuildKeywordQuery(keyword),
                        new TermQuery(new Field("empId")) { Value = empId }
                    }
                }
            };
            var response = await _client.SearchAsync<QueryDocument>(request);
            return MapResponseToQuery(response, $"search employee queries for '{keyword}'");
        }

        public async Task<List<RepositoryQuery>> FilterByStatusAsync(string status)
        {
            var normalizedStatus = NormalizeStatus(status);
            if (string.IsNullOrWhiteSpace(normalizedStatus)) return new List<RepositoryQuery>();

            await EnsureIndexAsync();
            var request = new SearchRequest<QueryDocument>(IndexName)
            {
                Size = 100,
                Query = BuildStatusFilterQuery(normalizedStatus)
            };
            var response = await _client.SearchAsync<QueryDocument>(request);
            return MapResponseToQuery(response, $"filter status '{normalizedStatus}'");
        }

        public async Task<List<RepositoryQuery>> FilterByDateRangeAsync(DateTime from, DateTime to)
        {
            var start = from <= to ? from.Date : to.Date;
            var end   = from <= to ? to.Date   : from.Date;

            await EnsureIndexAsync();
            var request = new SearchRequest<QueryDocument>(IndexName)
            {
                Size = 100,
                Query = new DateRangeQuery(new Field("queryDate"))
                {
                    Gte = start,
                    Lte = end.AddDays(1).AddTicks(-1)
                }
            };
            var response = await _client.SearchAsync<QueryDocument>(request);
            return MapResponseToQuery(response, "filter date range");
        }

        // ── Admin dashboard search (returns AdminQuery with CompanyName + EmployeeName) ──

        public async Task<List<AdminQuery>> AdminSearchAsync(
            string? keyword, string? status, DateTime? from, DateTime? to)
        {
            await EnsureIndexAsync();

            var mustQueries = new List<SearchQuery>();

            if (!string.IsNullOrWhiteSpace(keyword))
                mustQueries.Add(BuildKeywordQuery(keyword));

            var normalizedStatus = NormalizeStatus(status);
            if (!string.IsNullOrWhiteSpace(normalizedStatus))
                mustQueries.Add(BuildStatusFilterQuery(normalizedStatus));

            if (from.HasValue || to.HasValue)
            {
                var start = (from ?? to)!.Value.Date;
                var end   = (to ?? from)!.Value.Date;
                if (start > end) (start, end) = (end, start);

                mustQueries.Add(new DateRangeQuery(new Field("queryDate"))
                {
                    Gte = start,
                    Lte = end.AddDays(1).AddTicks(-1)
                });
            }

            SearchQuery searchQuery = mustQueries.Count switch
            {
                0 => new MatchAllQuery(),
                1 => mustQueries[0],
                _ => new BoolQuery { Must = mustQueries }
            };

            var request = new SearchRequest<QueryDocument>(IndexName)
            {
                Size = 200,
                Query = searchQuery
            };

            var response = await _client.SearchAsync<QueryDocument>(request);
            return MapResponseToAdminQuery(response, "admin search");
        }

        // ── Mapping helpers ───────────────────────────────────────────────

        // Base Query → QueryDocument (used during bootstrap; no company/emp name available)
        private static QueryDocument ToDocument(RepositoryQuery query)
        {
            return new QueryDocument
            {
                QueryId      = query.QueryId,
                UserId       = query.UserId,
                CompanyName  = null,
                Title        = query.Title,
                Description  = query.Description,
                Priority     = query.Priority.ToString(),
                QueryDate    = query.QueryDate,
                EmpId        = query.EmpId,
                EmployeeName = null,
                Status       = query.Status.ToString(),
                Comments     = query.Comments
            };
        }

        // AdminQuery → QueryDocument (used when indexing after assignment/creation — has company+emp name)
        private static QueryDocument ToAdminDocument(AdminQuery query)
        {
            return new QueryDocument
            {
                QueryId      = query.QueryId,
                UserId       = query.UserId,
                CompanyName  = query.Username,
                Title        = query.Title,
                Description  = query.Description,
                Priority     = query.Priority.ToString(),
                QueryDate    = query.QueryDate,
                EmpId        = query.EmpId,
                EmployeeName = query.EmployeeName,
                Status       = query.Status.ToString(),
                Comments     = query.Comments
            };
        }

        // QueryDocument → base Query
        private static RepositoryQuery FromDocument(QueryDocument doc)
        {
            return new RepositoryQuery
            {
                QueryId     = doc.QueryId,
                UserId      = doc.UserId,
                Title       = doc.Title,
                Description = doc.Description,
                Priority    = ParsePriority(doc.Priority),
                QueryDate   = doc.QueryDate,
                EmpId       = doc.EmpId,
                Status      = ParseStatus(doc.Status),
                Comments    = doc.Comments
            };
        }

        // QueryDocument → AdminQuery (preserves CompanyName + EmployeeName for dashboard grid)
        private static AdminQuery FromAdminDocument(QueryDocument doc)
        {
            return new AdminQuery
            {
                QueryId      = doc.QueryId,
                UserId       = doc.UserId,
                Username     = doc.CompanyName,
                EmployeeName = doc.EmployeeName,
                Title        = doc.Title,
                Description  = doc.Description,
                Priority     = ParsePriority(doc.Priority),
                QueryDate    = doc.QueryDate,
                EmpId        = doc.EmpId,
                Status       = ParseStatus(doc.Status),
                Comments     = doc.Comments
            };
        }

        private static List<RepositoryQuery> MapResponseToQuery(
            SearchResponse<QueryDocument> response, string operation)
        {
            AssertValid(response, operation);
            return response.Documents.Select(FromDocument).ToList();
        }

        private static List<AdminQuery> MapResponseToAdminQuery(
            SearchResponse<QueryDocument> response, string operation)
        {
            AssertValid(response, operation);
            return response.Documents.Select(FromAdminDocument).ToList();
        }

        private static void AssertValid(SearchResponse<QueryDocument> response, string operation)
        {
            if (response.IsValidResponse) return;

            var code   = response.ApiCallDetails.HttpStatusCode;
            var reason = response.ElasticsearchServerError?.Error?.Reason;

            if (code == 401)
                throw new InvalidOperationException(
                    "Elasticsearch authentication failed. Check appsettings.json Elasticsearch credentials.");
            if (code == 404)
                throw new InvalidOperationException(
                    "Elasticsearch index 'queries' not found. Restart the app to recreate it.");

            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(reason)
                    ? $"Elasticsearch failed to {operation}."
                    : $"Elasticsearch failed to {operation}: {reason}");
        }

        // ── Query builder ─────────────────────────────────────────────────

        private static SearchQuery BuildKeywordQuery(string keyword, bool titleOnly = false)
        {
            var kw = keyword.Trim();
            var normalizedStatus = NormalizeStatus(kw) ?? kw;
            var should = new List<SearchQuery>
            {
                BuildFuzzyMatch("title",         kw, 5),
                BuildWildcard  ("title.keyword", kw, 6)
            };

            if (!titleOnly)
            {
                should.Add(BuildFuzzyMatch("description",         kw, 3));
                should.Add(BuildWildcard  ("description.keyword", kw, 4));
                should.Add(BuildFuzzyMatch("comments",            kw, 2));
                should.Add(BuildWildcard  ("comments.keyword",    kw, 3));
                should.Add(BuildFuzzyMatch("priority",            kw, 1.5f));
                should.Add(BuildWildcard  ("priority.keyword",    kw, 2));
                should.Add(BuildFuzzyMatch("status",              normalizedStatus, 1.5f));
                should.Add(BuildWildcard  ("status.keyword",      normalizedStatus, 2));
                should.Add(BuildFuzzyMatch("companyName",         kw, 4));
                should.Add(BuildWildcard  ("companyName.keyword", kw, 5));
                should.Add(BuildFuzzyMatch("employeeName",        kw, 3));
                should.Add(BuildWildcard  ("employeeName.keyword",kw, 4));
            }

            return new BoolQuery { Should = should, MinimumShouldMatch = 1 };
        }

        private static MatchQuery BuildFuzzyMatch(string field, string value, float boost) =>
            new MatchQuery(new Field(field))
            {
                Query      = value,
                Fuzziness  = new Fuzziness("AUTO"),
                Operator   = Operator.Or,
                Boost      = boost
            };

        private static WildcardQuery BuildWildcard(string field, string value, float boost) =>
            new WildcardQuery(new Field(field))
            {
                Value           = $"*{EscapeWildcard(value)}*",
                CaseInsensitive = true,
                Boost           = boost
            };

        private static string EscapeWildcard(string value) =>
            value.Replace("\\", "\\\\", StringComparison.Ordinal)
                 .Replace("*",  "\\*",  StringComparison.Ordinal)
                 .Replace("?",  "\\?",  StringComparison.Ordinal);

        private static SearchQuery BuildStatusFilterQuery(string normalizedStatus)
        {
            var should = new List<SearchQuery>();

            foreach (var variant in GetStatusVariants(normalizedStatus))
            {
                should.Add(new TermQuery(new Field("status.keyword")) { Value = variant });
            }

            should.Add(new MatchQuery(new Field("status"))
            {
                Query = normalizedStatus,
                Operator = Operator.And
            });

            return new BoolQuery
            {
                Should = should,
                MinimumShouldMatch = 1
            };
        }

        private static IEnumerable<string> GetStatusVariants(string normalizedStatus)
        {
            yield return normalizedStatus;

            if (string.Equals(normalizedStatus, QueryStatus.InProgress.ToString(), StringComparison.OrdinalIgnoreCase))
                yield return "In Progress";
        }

        // ── Enum parsers ──────────────────────────────────────────────────

        private static string? NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return null;
            var normalized = status.Trim()
                .Replace(" ", "", StringComparison.Ordinal)
                .Replace("_", "", StringComparison.Ordinal)
                .Replace("-", "", StringComparison.Ordinal);
            return Enum.TryParse<QueryStatus>(normalized, true, out var parsed)
                ? parsed.ToString() : null;
        }

        private static Priority ParsePriority(string? priority) =>
            Enum.TryParse<Priority>(priority, true, out var p) ? p : Priority.Low;

        private static QueryStatus ParseStatus(string? status)
        {
            var n = NormalizeStatus(status);
            return Enum.TryParse<QueryStatus>(n, true, out var p) ? p : QueryStatus.Open;
        }
    }
}