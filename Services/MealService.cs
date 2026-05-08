using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using RecetteApp.Models;

namespace RecetteApp.Services;

public class MealService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MealService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>Recherche par nom (fragment). Chaîne vide = liste large renvoyée par TheMealDB.</summary>
    public Task<List<Meal>> GetMeals() => SearchMealsAsync(string.Empty);

    public async Task<List<Meal>> SearchMealsAsync(string query)
    {
        try
        {
            var q = query ?? string.Empty;
            var response = await _httpClient.GetAsync($"search.php?s={Uri.EscapeDataString(q)}");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var msg =
                    $"TheMealDB HTTP {(int)response.StatusCode} {response.ReasonPhrase}{TruncateBody(body)}";
                Debug.WriteLine(msg);
                throw new Exception(msg);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MealResponse>(json, JsonOptions);
            return result?.Meals?.Where(static m => !string.IsNullOrWhiteSpace(m.IdMeal)).ToList()
                   ?? new List<Meal>();
        }
        catch (TaskCanceledException ex)
        {
            var msg = "Délai dépassé (timeout) lors de l’appel à TheMealDB.";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
        catch (HttpRequestException ex)
        {
            var msg = ClarifierErreseau(ex.Message ?? "Erreur réseau lors de l’appel à TheMealDB.");
            Debug.WriteLine($"TheMealDB HttpRequestException: {ex}");
            throw new Exception(msg, ex);
        }
        catch (JsonException ex)
        {
            var msg = "Réponse TheMealDB invalide (JSON non lisible).";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TheMealDB exception: {ex}");
            throw;
        }
    }

    /// <summary>Repas d’une catégorie TheMealDB (<c>filter.php?c=</c>).</summary>
    public async Task<List<Meal>> FilterMealsByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return new List<Meal>();

        try
        {
            var response = await _httpClient.GetAsync($"filter.php?c={Uri.EscapeDataString(category.Trim())}");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var msg =
                    $"TheMealDB HTTP {(int)response.StatusCode} {response.ReasonPhrase}{TruncateBody(body)}";
                Debug.WriteLine(msg);
                throw new Exception(msg);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MealResponse>(json, JsonOptions);
            return result?.Meals?.Where(static m => !string.IsNullOrWhiteSpace(m.IdMeal)).ToList()
                   ?? new List<Meal>();
        }
        catch (TaskCanceledException ex)
        {
            var msg = "Délai dépassé (timeout) lors de l’appel à TheMealDB.";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
        catch (HttpRequestException ex)
        {
            var msg = ClarifierErreseau(ex.Message ?? "Erreur réseau lors de l’appel à TheMealDB.");
            Debug.WriteLine($"TheMealDB HttpRequestException: {ex}");
            throw new Exception(msg, ex);
        }
        catch (JsonException ex)
        {
            var msg = "Réponse TheMealDB invalide (JSON non lisible).";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TheMealDB exception: {ex}");
            throw;
        }
    }

    private static string ClarifierErreseau(string msg)
    {
        if (msg.Contains("hostname", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("servname", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("No such host", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("nodename", StringComparison.OrdinalIgnoreCase))
        {
            return "Pas d'accès Internet/DNS sur l'émulateur (impossible de résoudre themealdb.com).";
        }

        return msg;
    }

    /// <summary>Liste des pays</summary>
    public async Task<List<string>> GetAreaNamesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("list.php?a=list");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var msg = $"TheMealDB HTTP {(int)response.StatusCode} {response.ReasonPhrase}{TruncateBody(body)}";
                Debug.WriteLine(msg);
                throw new Exception(msg);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MealResponse>(json, JsonOptions);
            return result?.Meals?
                       .Select(m => (m.StrArea ?? string.Empty).Trim())
                       .Where(s => s.Length > 0)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                       .ToList()
                   ?? new List<string>();
        }
        catch (TaskCanceledException ex)
        {
            var msg = "Délai dépassé (timeout) lors de l'appel à TheMealDB.";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
        catch (HttpRequestException ex)
        {
            var msg = ClarifierErreseau(ex.Message ?? "Erreur réseau lors de l'appel à TheMealDB.");
            Debug.WriteLine($"TheMealDB HttpRequestException: {ex}");
            throw new Exception(msg, ex);
        }
        catch (JsonException ex)
        {
            var msg = "Réponse TheMealDB invalide (JSON non lisible).";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
    }

    /// <summary>Liste des noms de catégories (cuisines) depuis TheMealDB.</summary>
    public async Task<List<string>> GetCategoryNamesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("categories.php");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var msg =
                    $"TheMealDB HTTP {(int)response.StatusCode} {response.ReasonPhrase}{TruncateBody(body)}";
                Debug.WriteLine(msg);
                throw new Exception(msg);
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<MealCategoriesResponse>(json, options);
            return parsed?.Categories?
                       .Select(c => (c.StrCategory ?? string.Empty).Trim())
                       .Where(s => s.Length > 0)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                       .ToList()
                   ?? new List<string>();
        }
        catch (TaskCanceledException ex)
        {
            var msg = "Délai dépassé (timeout) lors de l’appel à TheMealDB.";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
        catch (HttpRequestException ex)
        {
            var msg = ex.Message ?? "Erreur réseau lors de l’appel à TheMealDB.";
            if (msg.Contains("hostname", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("servname", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("No such host", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("nodename", StringComparison.OrdinalIgnoreCase))
            {
                msg = "Pas d'accès Internet/DNS sur l'émulateur (impossible de résoudre themealdb.com).";
            }

            Debug.WriteLine($"TheMealDB HttpRequestException: {ex}");
            throw new Exception(msg, ex);
        }
        catch (JsonException ex)
        {
            var msg = "Réponse TheMealDB invalide (JSON non lisible).";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
    }

    /// <summary>Récupère une recette complète (ingrédients, instructions…) via TheMealDB.</summary>
    public async Task<Meal?> GetMealByIdAsync(string idMeal)
    {
        if (string.IsNullOrWhiteSpace(idMeal))
            return null;

        try
        {
            var response = await _httpClient.GetAsync($"lookup.php?i={Uri.EscapeDataString(idMeal.Trim())}");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var msg =
                    $"TheMealDB HTTP {(int)response.StatusCode} {response.ReasonPhrase}{TruncateBody(body)}";
                Debug.WriteLine(msg);
                throw new Exception(msg);
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<MealResponse>(json, options);
            return result?.Meals?.FirstOrDefault();
        }
        catch (TaskCanceledException ex)
        {
            var msg = "Délai dépassé (timeout) lors de l’appel à TheMealDB.";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
        catch (HttpRequestException ex)
        {
            var msg = ex.Message ?? "Erreur réseau lors de l’appel à TheMealDB.";
            if (msg.Contains("hostname", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("servname", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("No such host", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("nodename", StringComparison.OrdinalIgnoreCase))
            {
                msg = "Pas d'accès Internet/DNS sur l'émulateur (impossible de résoudre themealdb.com).";
            }

            Debug.WriteLine($"TheMealDB HttpRequestException: {ex}");
            throw new Exception(msg, ex);
        }
        catch (JsonException ex)
        {
            var msg = "Réponse TheMealDB invalide (JSON non lisible).";
            Debug.WriteLine($"{msg}\n{ex}");
            throw new Exception(msg, ex);
        }
    }

    private static string TruncateBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "";

        var t = body.Replace("\r", " ").Replace("\n", " ").Trim();
        if (t.Length > 140)
            t = t[..140] + "...";

        return $" — {t}";
    }
}