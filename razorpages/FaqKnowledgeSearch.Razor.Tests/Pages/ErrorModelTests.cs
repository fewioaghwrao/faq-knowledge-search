using System.Diagnostics;
using faq_knowledge_search.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FaqKnowledgeSearch.Razor.Tests.Pages;

public sealed class ErrorModelTests
{
    [Fact]
    public void OnGet_WhenActivityExists_SetsActivityId()
    {
        // Arrange
        using var activity =
            new Activity("ErrorModelTests");

        activity.Start();

        var sut = CreateSut(
            traceIdentifier: "trace-id");

        // Act
        sut.OnGet();

        // Assert
        Assert.NotNull(activity.Id);
        Assert.Equal(activity.Id, sut.RequestId);
        Assert.True(sut.ShowRequestId);
    }

    [Fact]
    public void OnGet_WhenActivityDoesNotExist_SetsTraceIdentifier()
    {
        // Arrange
        var previousActivity = Activity.Current;
        Activity.Current = null;

        try
        {
            var sut = CreateSut(
                traceIdentifier: "test-trace-id");

            // Act
            sut.OnGet();

            // Assert
            Assert.Equal(
                "test-trace-id",
                sut.RequestId);

            Assert.True(sut.ShowRequestId);
        }
        finally
        {
            Activity.Current = previousActivity;
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("request-id", true)]
    public void ShowRequestId_ReturnsExpectedValue(
        string? requestId,
        bool expected)
    {
        // Arrange
        var sut = CreateSut(
            traceIdentifier: "trace-id");

        sut.RequestId = requestId;

        // Act
        var result = sut.ShowRequestId;

        // Assert
        Assert.Equal(expected, result);
    }

    private static ErrorModel CreateSut(
        string traceIdentifier)
    {
        var httpContext =
            new DefaultHttpContext
            {
                TraceIdentifier = traceIdentifier
            };

        return new ErrorModel
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }
}