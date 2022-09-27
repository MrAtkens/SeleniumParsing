
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Services.Business
{
    public class UnitTestPOM
    {
        private IWebDriver _driver;
        
        public UnitTestPOM(IWebDriver driver)
        {
            _driver = driver;
        }

        public void SetLastName(string name)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var lastName = wait.Until(e => e.FindElement(By.CssSelector("#app > div > div.min-h-screen.flex.flex-col > main > div > div.mt-4.rounded-sm.shadow-md.bg-gray-200 > form > section:nth-child(1) > section > div.flex.mt-2.pr-16 > label:nth-child(1) > div > input")));
            lastName.SendKeys(name);
        }
        public void SetFirstName(string name)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var firstName = wait.Until(e => e.FindElement(By.CssSelector("#app > div > div.min-h-screen.flex.flex-col > main > div > div.mt-4.rounded-sm.shadow-md.bg-gray-200 > form > section:nth-child(1) > section > div.flex.mt-2.pr-16 > label:nth-child(2) > div > input")));
            firstName.SendKeys(name);
        }
        public void SetDate(string date)
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[1]/section/div[2]/label[3]/div/input")).SendKeys(date);
        }
        public void ChoseGender()
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[1]/section/div[2]/div/div/label[1]")).Click();
        }
        public void SetDocumentNumber(string document)
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[1]/section/div[2]/label[5]/div/input")).SendKeys(document);
        }

        public void SetDocumentDate(string documentDate)
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[1]/section/div[2]/label[6]/div/input")).SendKeys(documentDate);
        }
        public void SetIIN(string iin)
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[1]/section/div[3]/label/div/input")).SendKeys(iin);
        }
        public void SetPhone(string phone)
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[2]/div/label[2]/div/input")).SendKeys(phone);
        }

        public void SetMail(string email)
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[2]/div/label[3]/div[2]/input")).SendKeys(email);
        }


        public void ClickOffert()
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[1]/section/div[3]/div[2]/label/select/option[2]")).Click();
        }
        public void ClickToClose()
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/div/section[2]/div/div/span/label/div/div")).Click();
        }
        public void ClickToSecond()
        {
            _driver.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div[2]/form/section[4]/label/input")).Click();
        }
        public void ClickToBuy()
        {
            _driver.FindElement(By.Id("btnBook")).Click();
        }
        public void ClickToConfirm()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(50));
            wait.PollingInterval = TimeSpan.FromSeconds(10);
            var button = wait.Until(e => e.FindElement(By.XPath("//*[@id=\"app\"]/div/div[2]/main/div/div/div[2]/div[2]/div[2]/button")));
            button.Click();
        }
    }
}
