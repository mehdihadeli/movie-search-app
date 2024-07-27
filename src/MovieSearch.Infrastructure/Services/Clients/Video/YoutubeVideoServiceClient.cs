using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BuildingBlocks.Resiliency.Configs;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using MovieSearch.Application.Services.Clients;
using MovieSearch.Core;
using MovieSearch.Core.Generals;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace MovieSearch.Infrastructure.Services.Clients.Video;

//https://github.com/youtube/api-samples/tree/master/dotnet
public class YoutubeVideoServiceClient : IVideoServiceClient
{
    private static AsyncTimeoutPolicy _timeoutPolicy;
    private static AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private static AsyncBulkheadPolicy _bulkheadPolicy;
    private readonly IMapper _mapper;
    private readonly YoutubeVideoOptions _options;

    private readonly AsyncRetryPolicy _retryPolicy;

    public YoutubeVideoServiceClient(
        IOptions<YoutubeVideoOptions> options,
        IMapper mapper,
        IOptions<PolicyConfig> policyOptions
    )
    {
        _mapper = mapper;
        _options = options.Value;

        _retryPolicy = Policy.Handle<Exception>().RetryAsync(policyOptions.Value.RetryCount);
        _timeoutPolicy = Policy.TimeoutAsync(policyOptions.Value.TimeOutDuration, TimeoutStrategy.Pessimistic);
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                policyOptions.Value.RetryCount + 1,
                TimeSpan.FromSeconds(policyOptions.Value.BreakDuration)
            );
        _bulkheadPolicy = Policy.BulkheadAsync(3, 6);

        _retryPolicy.WrapAsync(_circuitBreakerPolicy).WrapAsync(_timeoutPolicy);
    }

    //https://developers.google.com/youtube/v3/docs/search/list
    public async Task<VideoListResultModel<Core.Generals.Video>> GetTrailers(
        string movieName,
        int pageSize = 20,
        string pageToken = "",
        DateTime? publishedAfter = null,
        DateTime? publishedBefore = null
    )
    {
        var youtubeService = new YouTubeService(
            new BaseClientService.Initializer { ApiKey = _options.ApiKey, ApplicationName = GetType().ToString() }
        );

        var searchListRequest = youtubeService.Search.List(_options.SearchPart);

        var searchTerm =
            movieName.ToLower().Contains("trailer") == false ? $"{movieName.ToLower()} trailer" : movieName.ToLower();

        searchListRequest.PublishedAfter = publishedAfter;
        searchListRequest.PublishedBefore = publishedBefore;
        searchListRequest.Q = searchTerm;
        searchListRequest.Order = _options.Order; //our default config: Relevance - Resources are sorted based on their relevance to the search query.
        searchListRequest.MaxResults = pageSize;
        searchListRequest.PageToken = pageToken;
        searchListRequest.Type = _options.SearchType;
        searchListRequest.VideoEmbeddable = SearchResource.ListRequest.VideoEmbeddableEnum.True__;

        var searchListResponse = await _retryPolicy.ExecuteAsync(() => searchListRequest.ExecuteAsync());

        var result = new VideoListResultModel<Core.Generals.Video>(
            searchListResponse
                .Items.Select(x => new Core.Generals.Video
                {
                    Iso_639_1 = "en",
                    Iso_3166_1 = "US",
                    Id = x.Id.VideoId,
                    Name = x.Snippet.Title,
                    Size = 1080,
                    Site = "YouTube",
                    Key = x.Id.VideoId,
                    PublishedAt = x.Snippet.PublishedAt,
                    Type = "Trailer"
                })
                .ToList(),
            searchListResponse.PageInfo.TotalResults ?? 0,
            pageToken,
            searchListResponse.NextPageToken,
            searchListResponse.PrevPageToken,
            searchListResponse.PageInfo.ResultsPerPage ?? 0
        );

        return result;
    }
}
