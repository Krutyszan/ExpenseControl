using ExpenseControl.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ExpenseControl.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ICurrentUserService _currentUserService;
        public string CurrentUserId => _currentUserService.UserId;
        public CurrentUserService(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public string UserId
        {
            get 
            {
                var authState = _authStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
                return authState.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
        }
    }
}
