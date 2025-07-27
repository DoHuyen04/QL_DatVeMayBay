using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;
namespace TestDatVeMayBay.Tests.Controller
{
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public bool IsAvailable => true;

        public string Id => Guid.NewGuid().ToString();

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);

        public void SetString(string key, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            Set(key, bytes);
        }

        public string GetString(string key)
        {
            return _sessionStorage.TryGetValue(key, out var data) ? Encoding.UTF8.GetString(data) : null;
        }

        /// <summary>
        /// ✅ Phương thức tiện dụng để khởi tạo session có sẵn giá trị chuỗi và object
        /// </summary>
        public static ISession CreateSession(
            Dictionary<string, string>? strings = null,
            Dictionary<string, object>? serializedObjects = null)
        {
            var session = new TestSession();

            if (strings != null)
            {
                foreach (var pair in strings)
                {
                    session.SetString(pair.Key, pair.Value);
                }
            }

            if (serializedObjects != null)
            {
                foreach (var pair in serializedObjects)
                {
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(pair.Value, pair.Value.GetType());
                    session.Set(pair.Key, bytes);
                }
            }

            return session;
        }

        internal static ISession CreateSession(Dictionary<string, string> dictionary1, Dictionary<string, byte[]> dictionary2)
        {
            throw new NotImplementedException();
        }
    }
}