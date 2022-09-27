﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Services.Business;

[TestFixture("https://aviata.kz/aviax/booking/a1040d16-438c-4f0f-b90d-f3f1d908b602/1a8c558b-9f05-4d15-90c9-4cd11a6c4f5d/")]
public class UnitTest
{
    private IWebDriver _driver;
    private string _url;
    protected ExtentReports _extent;
    protected ExtentTest _test;

    [OneTimeSetUp]
    protected void Setup()
    {
        var path = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
        var actualPath = path.Substring(0, path.LastIndexOf("bin"));
        var projectPath = new Uri(actualPath).LocalPath;
        Directory.CreateDirectory(projectPath.ToString() + "Reports");
        var reportPath = projectPath + "Reports\\ExtentReport.html";
        var htmlReporter = new ExtentHtmlReporter(reportPath);
        _extent = new ExtentReports();
        _extent.AttachReporter(htmlReporter);
        _extent.AddSystemInfo("Host Name", "LocalHost");
        _extent.AddSystemInfo("Environment", "QA");
        _extent.AddSystemInfo("UserName", "TestUser");
    }
    public UnitTest(string url)
    {
        _url = url;
    }
    
    [SetUp]
    public void Initialize()
    {
        _driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
        _driver.Manage().Window.Maximize();
        _test = _extent.CreateTest(TestContext.CurrentContext.Test.Name);
        _driver.Navigate().GoToUrl(_url);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(8);
    }

    [Test]
    public async Task OpenAppTest()
    {
        UnitTestPOM unitTest = new UnitTestPOM(_driver);
        unitTest.SetFirstName("Raiymbek");
        unitTest.SetLastName("Kaliaskar");
        unitTest.ChoseGender();
        unitTest.SetDate("15-07-2002");
        unitTest.SetDocumentNumber("034234");
        unitTest.SetDocumentDate("04-12-2025");
        unitTest.SetIIN("020715551068");
        unitTest.ClickOffert();
        unitTest.SetPhone("7077227589");
        unitTest.SetMail("r.kaliaskar@mail.ru");
        unitTest.ClickToClose();
        unitTest.ClickToSecond();
        unitTest.ClickToBuy();
        unitTest.ClickToConfirm();
    }
    [TearDown]

    public void EndTest()
    {
        var status = TestContext.CurrentContext.Result.Outcome.Status;
        var stacktrace = string.IsNullOrEmpty(TestContext.CurrentContext.Result.StackTrace) ? "" : string.Format("{0}", TestContext.CurrentContext.Result.StackTrace);
        Status logstatus;
        switch (status)
        {
            case TestStatus.Failed:
                logstatus = Status.Fail;
                DateTime time = DateTime.Now;
                String fileName = "Screenshot_" +time.ToString("h_mm_ss") + ".png";
                String screenShotPath = Capture(_driver, fileName);
                _test.Log(Status.Fail, "Fail");
                _test.Log(Status.Fail, "Snapshot below: " +_test.AddScreenCaptureFromPath("Screenshots\\" +fileName));
                break;
            case TestStatus.Inconclusive:
                logstatus = Status.Warning;
                break;
            case TestStatus.Skipped:
                logstatus = Status.Skip;
                break;
            default:
                logstatus = Status.Pass;
                break;
        }
        _test.Log(logstatus, "Test ended with " +logstatus + stacktrace);
        _extent.Flush();
        _driver.Quit();
    }

    public static string Capture(IWebDriver driver, String screenShotName)
    {
        ITakesScreenshot ts = (ITakesScreenshot)driver;
        Screenshot screenshot = ts.GetScreenshot();
        var pth = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
        var actualPath = pth.Substring(0, pth.LastIndexOf("bin"));
        var reportPath = new Uri(actualPath).LocalPath;
        Directory.CreateDirectory(reportPath + "Reports\\" + "Screenshots");
        var finalpth = pth.Substring(0, pth.LastIndexOf("bin")) + "Reports\\Screenshots\\" +screenShotName;
        var localpath = new Uri(finalpth).LocalPath;
        screenshot.SaveAsFile(localpath, ScreenshotImageFormat.Png);
        return reportPath;
    }
}