using System.Net.Http.Json;

namespace Extensions;

public static class ClientExtensions
{
    public static async Task<bool> ExecutePostAsync<TRequest>(
        this HttpClient client,
        string url,
        TRequest? requestData,
        Func<HttpResponseMessage, Task>? onSuccess = null,
        Action<string>? onError = null
    )
    {
        try
        {
            HttpResponseMessage response;

            response = requestData is null
            ? await client.PostAsync(url, null)
            : await client.PostAsJsonAsync(url, requestData);

            if (response.IsSuccessStatusCode)
            {
                if (onSuccess != null)
                    await onSuccess(response);
                return true;
            }

            // 读取服务端返回的错误信息
            string error = await response.Content.ReadAsStringAsync();
            onError?.Invoke(
                $"Request {url} failed. Error: {error}, StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            return false;
        }
        catch (HttpRequestException ex)
        {
            onError?.Invoke(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// POST 发送 requestData，并把成功响应反序列化为 TResponse。
    /// </summary>
    /// <typeparam name="TRequest">请求体类型。</typeparam>
    /// <typeparam name="TResponse">期望的响应体类型。</typeparam>
    /// <param name="client">HttpClient。</param>
    /// <param name="url">请求地址。</param>
    /// <param name="requestData">请求体，可为 null。</param>
    /// <param name="onError">可选的错误回调。</param>
    /// <returns>成功：反序列化后的 TResponse 对象； 失败：default(TResponse)。</returns>
    public static async Task<TResponse?> PostAsAsync<TRequest, TResponse>(
        this HttpClient client,
        string url,
        TRequest? requestData,
        Action<string>? onError = null)
    {
        try
        {
            HttpResponseMessage response = requestData is null
                ? await client.PostAsync(url, null)
                : await client.PostAsJsonAsync(url, requestData);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            string error = await response.Content.ReadAsStringAsync();
            onError?.Invoke(
                $"Request {url} failed. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}, Error: {error}");
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }

        return default;
    }
}