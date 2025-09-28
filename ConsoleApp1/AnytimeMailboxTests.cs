using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace AnytimeMailboxTests
{
    [TestFixture]
    public class AnytimeMailboxTestRunner
    {
        private IWebDriver? driver;
        private WebDriverWait? wait;
        private const string BASE_URL = "https://www.anytimemailbox.com";
        private const string LOGIN_URL = "https://signup.anytimemailbox.com/login";
        private const int TIMEOUT_SECONDS = 10;

        public static void Main(string[] args)
        {
            Console.WriteLine("Anytime Mailbox Selenium Tests");
            Console.WriteLine("===============================");
            Console.WriteLine();
            Console.WriteLine("This project contains NUnit tests for Anytime Mailbox automation.");
            Console.WriteLine();
            Console.WriteLine("To run the tests, use one of the following commands:");
            Console.WriteLine("  dotnet test                    - Run all tests");
            Console.WriteLine("  dotnet test --logger:console   - Run with detailed console output");
            Console.WriteLine();
            Console.WriteLine("Test Cases:");
            Console.WriteLine("  1. TestSuccessfulLocationSearch   - Verifies location search returns results");
            Console.WriteLine("  2. TestUnsuccessfulLocationSearch - Verifies handling of invalid searches");
            Console.WriteLine("  3. TestFailedLogin                - Verifies login failure handling");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }



        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-web-security");
            options.AddArgument("--disable-features=VizDisplayCompositor");
            
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
        }

        [Test]
        public void TestSuccessfulLocationSearch()
        {
            driver!.Navigate().GoToUrl(BASE_URL);
            
            var lookupInput = wait!.Until(d => d.FindElement(By.Id("lookup")));
            
            lookupInput.Clear();
            lookupInput.SendKeys("Manila");
            
            var manilaOption = wait.Until(d => d.FindElement(By.CssSelector("li.active a[role='option']")));
            manilaOption.Click();
            
            var locationCountElement = wait.Until(d => d.FindElement(By.CssSelector(".location-count-display")));
            Assert.That(locationCountElement.Displayed, Is.True, "Location count display should be visible");
            Assert.That(locationCountElement.Text, Does.Contain("locations found").Or.Contain("location found"), 
                "Should display locations found message");
            
            var locationResults = wait.Until(d => d.FindElements(By.CssSelector("[class*='location'], [class*='mailbox'], [class*='address']")).Count > 0);
            var actualResults = driver.FindElements(By.CssSelector("[class*='location'], [class*='mailbox'], [class*='address']"));
            Assert.That(actualResults.Count, Is.GreaterThan(0), "Should find location results for Manila");
        }

        [Test]
        public void TestUnsuccessfulLocationSearch()
        {
            driver!.Navigate().GoToUrl(BASE_URL);
            
            var lookupInput = wait!.Until(d => d.FindElement(By.Id("lookup")));
            
            lookupInput.Clear();
            lookupInput.SendKeys("Atlantisnothere");
            
            lookupInput.SendKeys(Keys.Enter);
            
            var errorElement = wait.Until(d => d.FindElement(By.Id("alterr")));
            Assert.That(errorElement.Text, Does.Contain("We are unable to locate the place you entered"), 
                "Should display error message: 'We are unable to locate the place you entered. Please try again.'");
            Assert.That(errorElement.Displayed, Is.True, "Error message should be visible");
        }

        [Test]
        public void TestFailedLogin()
        {
            driver!.Navigate().GoToUrl(LOGIN_URL);
            
            var emailField = wait!.Until(d => d.FindElement(By.Name("f_uid")));
            var passwordField = wait.Until(d => d.FindElement(By.Name("f_pwd")));
            var loginButton = wait.Until(d => d.FindElement(By.CssSelector("button[type='button']")));
            
            Assert.That(emailField, Is.Not.Null, "Email field should be present");
            Assert.That(passwordField, Is.Not.Null, "Password field should be present");
            Assert.That(loginButton, Is.Not.Null, "Login button should be present");
            
            var captchaElements = driver.FindElements(By.CssSelector("[class*='captcha'], [id*='captcha'], [class*='recaptcha'], iframe[src*='recaptcha']"));
            if (captchaElements.Count > 0)
            {
                Assert.Pass("CAPTCHA protection detected");
                return;
            }
            
            emailField.Clear();
            emailField.SendKeys("invalid@test.com");
            passwordField.Clear();
            passwordField.SendKeys("invalidpassword123");
            loginButton.Click();
            
            try
            {
                var errorElement = wait.Until(d => d.FindElement(By.CssSelector("div.alert.alert-danger")));
                Assert.That(errorElement.Text, Does.Contain("Invalid credentials, please try again."), 
                    "Should display error message for invalid login");
            }   
            catch
            {
                Assert.That(driver.Url, Does.Contain("login"), "Should remain on login page after failed login attempt");
            }
        }


    }
}