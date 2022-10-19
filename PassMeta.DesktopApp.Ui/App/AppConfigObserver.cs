namespace PassMeta.DesktopApp.Ui.App
{
    using System;
    using Common;
    using Common.Abstractions;
    using Common.Abstractions.Services;
    using Common.Constants;
    using Core;
    using Core.Utils;
    using AppContext = Core.AppContext;

    public class AppConfigObserver : IObserver<IAppConfig>
    {
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();

        private IAppConfig? _prev;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public async void OnNext(IAppConfig value)
        {
            try
            {
                Resources.Culture = AppCulture.Parse(value.CultureCode);

                if (_prev is null)
                {
                    _prev = value;
                    return;
                }

                if (value.ServerUrl != _prev.ServerUrl)
                {
                    await AppContext.RefreshCurrentFromServerAsync();
                    await PassFileManager.ReloadAsync(false);
                }

                if (value.CultureCode != _prev.CultureCode)
                {
                    App.Restart();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Processing of app-config-changing event failed");
            }

            _prev = value;
        }
    }
}