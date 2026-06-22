using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Chingoo.Tests;

public class AccountNavigationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AccountNavigationTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Home_ShowsLoginLinkWithoutTemplateContent()
    {
        var html = await _client.GetStringAsync("/");

        Assert.Contains("href=\"/Account/Login\"", html);
        Assert.DoesNotContain("Welcome", html);
    }

    [Fact]
    public async Task Login_ShowsCredentialsAndRegistrationLink()
    {
        var html = await _client.GetStringAsync("/Account/Login");

        Assert.Contains("name=\"UserId\"", html);
        Assert.Contains("name=\"Password\"", html);
        Assert.Contains("href=\"/Account/Register\"", html);
    }

    [Fact]
    public async Task Register_ShowsRegistrationFields()
    {
        var html = await _client.GetStringAsync("/Account/Register");

        Assert.Contains("name=\"UserId\"", html);
        Assert.Contains("name=\"Password\"", html);
        Assert.Contains("name=\"ConfirmPassword\"", html);
    }

    [Fact]
    public async Task Home_ShowsCollapsibleBoardSidebar()
    {
        var html = await _client.GetStringAsync("/");

        Assert.Contains("class=\"board-sidebar\"", html);
        Assert.Contains("축구 매치", html);
        Assert.Contains("평일", html);
        Assert.Contains("주말", html);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, ">서울<").Count);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, ">경기<").Count);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, ">인천<").Count);
        Assert.Contains("구장 양도", html);
        Assert.Contains("팀원 구함", html);
        Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(html, @"<details\b").Count);
    }
}
