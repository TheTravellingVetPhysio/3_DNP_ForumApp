using System;

namespace BlazorApp.Services;

public class HttpReactionService : IReactionService
{ 
    private readonly HttpClient httpClient;

    public HttpReactionService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

}
