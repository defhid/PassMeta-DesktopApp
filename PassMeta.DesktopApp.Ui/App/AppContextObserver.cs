namespace PassMeta.DesktopApp.Ui.App
{
    using System;
    using Common.Abstractions;
    using Common.Abstractions.Services;
    using Core;
    using Core.Utils;

    public class AppContextObserver : IObserver<IAppContext>
    {
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();

        private IAppContext? _prev;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public async void OnNext(IAppContext value)
        {
            try
            {
                if (value.User?.Id != _prev?.User?.Id)
                {
                    await PassFileManager.ReloadAsync(true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Processing of app-context-changing event failed");
            }

            _prev = value;
        }
    }
}