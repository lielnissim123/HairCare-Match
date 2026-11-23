using Model;

namespace View.Services
{
    public class UserSession
    {
        // פרטי המשתמש המחובר. קוראים מכל דף, משנים רק דרך הפעולות של המחלקה
        public int UserId { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public int RoleId { get; private set; }
        public string RoleName { get; private set; } = string.Empty;

        public bool IsLoggedIn => UserId > 0;

        // אירוע שמודיע שהמשתמש המחובר השתנה
        public event Action? OnChange;

        public void Login(User user)
        {
            UserId = user.Id;
            Username = user.Username;
            Email = user.Email;
            RoleId = user.RoleId;
            RoleName = user.RoleName;

            if (OnChange != null)
            {
                OnChange.Invoke();
            }
        }

        public void Logout()
        {
            UserId = 0;
            Username = string.Empty;
            Email = string.Empty;
            RoleId = 0;
            RoleName = string.Empty;

            if (OnChange != null)
            {
                OnChange.Invoke();
            }
            if (_storage != null) { _ = _storage.DeleteAsync("LoggedInUser"); }
        }

        // הוספה בתוך המחלקה UserSession:
        private readonly Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedSessionStorage? _storage;

        public UserSession(Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedSessionStorage storage)
        {
            _storage = storage;
        }

        // קריאה לזה מיד אחרי Session.Login(user) כדי לשמור את המשתמש בדפדפן
        public async Task SaveToStorageAsync(Model.User user)
        {
            if (_storage != null)
            {
                try
                {
                    await _storage.SetAsync("LoggedInUser", user);
                }
                catch
                {
                }
            }
        }

        public async Task LoadFromStorageAsync()
        {
            if (IsLoggedIn || _storage == null)
            {
                return;
            }

            try
            {
                var result = await _storage.GetAsync<Model.User>("LoggedInUser");
                if (result.Success && result.Value != null)
                {
                    Login(result.Value);
                }
            }
            catch
            {
            }
        }
    }
}
