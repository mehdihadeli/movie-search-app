using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.Test.Fixtures;
using DM.MovieApi.IntegrationTests;
using FluentAssertions;
using MovieSearch.Api;
using MovieSearch.Application.Movies.Dtos;
using MovieSearch.Application.Movies.Features.FindPopularMovies;
using Xunit;

namespace MovieSearch.IntegrationTests.Application.Movies.Features;

public class FindPopularMoviesQueryHandlerTests : IntegrationTestFixture<Program>
{
    public FindPopularMoviesQueryHandlerTests()
    {
        //setup the swaps for our tests
        RegisterTestServices(services => { });
    }

    [Fact]
    public async Task find_popular_movies_query_should_return_a_valid_movie_info_list_dto()
    {
        // Arrange
        var query = new FindPopularMoviesQuery { Page = 1 };

        // Act
        var listResult = (await QueryAsync(query, CancellationToken.None)).MovieList;

        // Assert
        listResult.Should().NotBeNull();
        listResult.Should().BeOfType<ListResultModel<MovieInfoDto>>();
        listResult.Page.Should().Be(1);
        listResult.PageSize.Should().Be(listResult.Items.Count);
        TMDBTestUtil.AssertMovieInformationDtoStructure(listResult.Items);
    }
}
