using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Document.Web.Components
{
    /// <summary>
    /// Base component kế thừa <see cref="FluxorComponent"/>,
    /// cung cấp các hook <c>OnInitializedSafe</c> và <c>OnDisposeSafe</c>
    /// để các component con dễ dàng override mà không quên gọi base.
    /// Hỗ trợ cả đồng bộ và bất đồng bộ.
    /// </summary>
    ///
    [Authorize]
    public abstract class BaseComponent : FluxorComponent, IDisposable
    {
        [Inject] protected IDispatcher? Dispatcher { get; set; }

        private bool _disposed;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();
            OnInitializedSafe();
        }

        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await OnInitializedSafeAsync();
        }

        /// <summary>
        /// Logic khởi tạo đồng bộ, cho phép component con override
        /// mà không cần gọi <c>base.OnInitialized()</c>.
        /// </summary>
        protected virtual void OnInitializedSafe()
        {
            // Component con có thể override ở đây
        }

        /// <summary>
        /// Logic khởi tạo bất đồng bộ, cho phép component con override
        /// mà không cần gọi <c>base.OnInitializedAsync()</c>.
        /// </summary>
        protected virtual Task OnInitializedSafeAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gọi khi component được dispose. Cho phép component con override
        /// mà không cần lo xử lý cờ <c>_disposed</c>.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> nếu được gọi từ <see cref="Dispose()"/>;
        /// <c>false</c> nếu từ finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                OnDisposeSafe();
            }

            _disposed = true;
        }

        /// <summary>
        /// Logic dọn dẹp, cho phép component con override.
        /// </summary>
        protected virtual void OnDisposeSafe()
        {
            // Component con có thể override ở đây
        }
    }
}
