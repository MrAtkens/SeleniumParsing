using OpenQA.Selenium.Firefox;

namespace Helpers
{
    public static class WebDriverOptionsHelper
    {
        public static FirefoxOptions FirefoxOptions()
        {
            var options = new FirefoxOptions();
            options.SetPreference("dom.webdriver.enabled", false);
            options.SetPreference("dom.webnotifications.enabled", false);
            options.SetPreference("media.volume_seale", "0.0");
            options.SetPreference("general.useragent.override", "Mozilla/5.0 (Android 4.4; Mobile; rv:41.0) Gecko/41.0 Firefox/41.0");
            return options;
        }
    }
}