using System;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace VROrchestrator.Extensions
{
    public static class HttpRequestMessageExtension
    {
        public static async Task<Result<HttpResponseMessage>> SendRequest(this HttpRequestMessage requestMessage, HttpClient client)
        {
            try
            {
                var response = await client.SendAsync(requestMessage);
                return Result.Success(response);
            }
            catch (ArgumentNullException)
            {
                return Result.Failure<HttpResponseMessage>("Message was null");
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<HttpResponseMessage>("Message was already sent");
            }
            catch (HttpRequestException)
            {
                return Result.Failure<HttpResponseMessage>("HttpRequest failed");
            }
        }
    }
}