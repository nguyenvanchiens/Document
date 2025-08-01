﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Document.Web
{
    public class DataRespon
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class UserWebService()
    {
        public async Task<DataRespon> RefreshTokenAsync(string token)
        {
            return new DataRespon();
        }
    }

    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly UserWebService _userService;

        public CustomAuthStateProvider(ILocalStorageService localStorage, UserWebService userService)
        {
            _localStorage = localStorage;
            _userService = userService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            if (IsTokenExpired(token))
            {
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                var rs = await _userService.RefreshTokenAsync(refreshToken ?? "");
                if (rs != null)
                {
                    token = rs.AccessToken;
                    refreshToken = rs.RefreshToken;
                    await _localStorage.SetItemAsync("authToken", token);
                    await _localStorage.SetItemAsync("refreshToken", refreshToken);
                }
                else
                {
                    await _localStorage.RemoveItemAsync("authToken");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
            }

            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task MarkUserAsAuthenticated(string token, string refreshToken)
        {
            await _localStorage.SetItemAsync("authToken", token);
            await _localStorage.SetItemAsync("refreshToken", refreshToken);

            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");

            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }

        private bool IsTokenExpired(string jwt)
        {
            try
            {
                var payload = jwt.Split('.')[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

                if (keyValuePairs != null && keyValuePairs.TryGetValue("exp", out var expValue))
                {
                    var exp = expValue.GetInt64();
                    return DateTimeOffset.FromUnixTimeSeconds(exp) <= DateTimeOffset.UtcNow;
                }
            }
            catch
            {
                return true;
            }
            return true;
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

            var claims = new List<Claim>();

            // Parse roles
            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles))
            {
                if (roles.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.GetString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.GetString()));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            // Parse permissions
            if (keyValuePairs.TryGetValue("permission", out var permissions))
            {
                if (permissions.ValueKind == JsonValueKind.Array)
                {
                    foreach (var perm in permissions.EnumerateArray())
                    {
                        claims.Add(new Claim("permission", perm.GetString()));
                    }
                }
                else
                {
                    claims.Add(new Claim("permission", permissions.GetString()));
                }

                keyValuePairs.Remove("permission");
            }

            // Parse remaining claims
            foreach (var kvp in keyValuePairs)
            {
                claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            base64 = base64.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}