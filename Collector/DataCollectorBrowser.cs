using RabotatAgent.Types;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Automation;

namespace RabotatAgent.Collector
{
    class DataCollectorBrowser
    {
        static private string addrRegex = @"^(https?:\/\/)?[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,4}).*$";

        static private string GetBrowserEdgeUrl(IntPtr hWnd)
        {
            try
            {
                // хватаем адресную строку
                var editBox = AutomationElement.FromHandle(hWnd)
                    .FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Microsoft Edge"))
                    .FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "addressEditBox"));

                // не считается, если туда кто-то печатает
                if ((bool)editBox.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) return null;

                // читаем введённый текст
                // это ложь, мы просто не умеем прочитать протокол
                // да даже остальной адрес не настоящий, а просто что-то, написанное в текстовом поле
                var url = ((TextPattern)editBox.GetCurrentPattern(TextPattern.Pattern)).DocumentRange.GetText(Int32.MaxValue);
                if (url == "") return null;
                if (!Regex.IsMatch(url, addrRegex)) return null;
                if (!url.StartsWith("http")) url = "http://" + url;
                return url;
            }
            catch
            {
                return null;
            }
            
        }

        static private string GetBrowserIEUrl(IntPtr hWnd)
        {
            try {
                // хватаемся за адресную строку
                AutomationElement addressBar = AutomationElement.FromHandle(hWnd)
                    .FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "Address Band Root"))
                    .FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

                // не считается, если туда кто-то печатает
                if ((bool)addressBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) return null;

                // пытаемся прочитать адрес и если он похож на настоящий, то возвращаем его
                var url = ((TextPattern)addressBar.GetCurrentPattern(TextPattern.Pattern)).DocumentRange.GetText(Int32.MaxValue);
                if (url == "") return null;
                if (!Regex.IsMatch(url, addrRegex)) return null;
                return url;
            }
            catch
            {
                return null;
            }
        }

        static private string GetBrowserChromeUrl(IntPtr hWnd)
        {
            try
            {
                // хватаемся за адресную строку
                AutomationElement addressBar = AutomationElement.FromHandle(hWnd)
                    .FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"))
                    .FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar))
                    .FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, ""))
                    .FindFirst(TreeScope.Children, new OrCondition(
                        new PropertyCondition(AutomationElement.NameProperty, "Search or enter web address"),
                        new PropertyCondition(AutomationElement.NameProperty, "Адресная строка и строка поиска"),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)));

                // не считается, если туда кто-то печатает
                if ((bool)addressBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) return null;

                // пытаемся прочитать адрес и если он похож на настоящий, то возвращаем его
                var url = ((ValuePattern)addressBar.GetCurrentPattern(ValuePatternIdentifiers.Pattern)).Current.Value;
                if (url == "") return null;
                if (!Regex.IsMatch(url, addrRegex)) return null;
                if (!url.StartsWith("https")) url = "http://" + url;
                return url;
            }
            catch
            {
                return null;
            }
        }

        static private string GetBrowserYandexUrl(IntPtr hWnd)
        {
            // хватаемся за адресную строку
            try
            {
                AutomationElement addressBar = AutomationElement.FromHandle(hWnd)
                    .FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Yandex"))
                    .FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar))
                    .FindFirst(TreeScope.Children, new OrCondition(
                        new PropertyCondition(AutomationElement.NameProperty, "Адресная и поисковая строка"),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Group)));

                // не считается, если туда кто-то печатает
                if ((bool)addressBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) return null;

                // пытаемся прочитать адрес и если он похож на настоящий, то возвращаем его
                var url = ((ValuePattern)addressBar.GetCurrentPattern(ValuePatternIdentifiers.Pattern)).Current.Value;
                if (url == "") return null;
                if (!Regex.IsMatch(url, addrRegex)) return null;
                if (!url.StartsWith("https")) url = "http://" + url;
                return url;
            }
            catch
            {
                return null;
            }
        }

        static private string GetBrowserOperaUrl(IntPtr hWnd)
        {
            // хватаемся за адресную строку
            try
            {
                AutomationElement addressBar = AutomationElement.FromHandle(hWnd)
                    .FindFirst(TreeScope.Subtree, new AndCondition(
                        new PropertyCondition(AutomationElement.NameProperty, "Поле адреса"),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)));

                // не считается, если туда кто-то печатает
                if ((bool)addressBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) return null;

                // пытаемся прочитать адрес и если он похож на настоящий, то возвращаем его
                var url = ((ValuePattern)addressBar.GetCurrentPattern(ValuePatternIdentifiers.Pattern)).Current.Value;
                if (url == "") return null;
                if (!Regex.IsMatch(url, addrRegex)) return null;
                return url;
            }
            catch
            {
                return null;
            }
        }

        static private string GetBrowserFirefoxUrl(IntPtr hWnd)
        {
            // Address and search bar
            // Введите поисковый запрос или адрес
            try
            {
                // хватаемся за адресную строку
                var ff = AutomationElement.FromHandle(hWnd);

                var textCondition = new OrCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"),
                    new PropertyCondition(AutomationElement.NameProperty, "Введите поисковый запрос или адрес"));
                var editCondition = new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit),
                    textCondition);
                var edit = ff.FindFirst(TreeScope.Subtree, editCondition);

                // не считается, если туда кто-то печатает
                if ((bool)edit.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty)) return null;

                // пытаемся прочитать адрес и если он похож на настоящий, то возвращаем его
                var url = ((ValuePattern)edit.GetCurrentPattern(ValuePatternIdentifiers.Pattern)).Current.Value;
                if (url == "") return null;
                if (!Regex.IsMatch(url, addrRegex)) return null;
                if (!url.StartsWith("http")) url = "http://" + url;
                return url;
            }
            catch
            {
                return null;
            }
        }

        static public string GetBrowserUrl(ActiveWindow window, IntPtr hWnd)
        {
            string url = null;
            try
            {
                // Microsoft Edge
                if (window.ProcessName.Equals("ApplicationFrameHost") &&
                    (window.WindowTitle.EndsWith("- Microsoft Edge") || 
                    window.WindowTitle.EndsWith("- [InPrivate] - Microsoft Edge")))
                {
                    url = GetBrowserEdgeUrl(hWnd);
                }

                // Internet Explorer IE 8-11
                if (window.ProcessName.Equals("iexplore") &&
                    (window.WindowTitle.EndsWith("- Internet Explorer") ||
                    window.WindowTitle.EndsWith("- Internet Explorer - [InPrivate]") ||
                    window.WindowTitle.EndsWith("- Windows Internet Explorer") ||
                    window.WindowTitle.EndsWith("- Windows Internet Explorer - [InPrivate]") ||
                    window.WindowTitle.EndsWith("- [InPrivate]")))
                {
                    url = GetBrowserIEUrl(hWnd);
                }

                // Google Chrome 46
                if (window.ProcessName.Equals("chrome") && window.WindowTitle.EndsWith("- Google Chrome"))
                {
                    url = GetBrowserChromeUrl(hWnd);
                }

                // Yandex 15.10
                if (window.ProcessName.Equals("browser") && window.WindowTitle.EndsWith("– Yandex"))
                {
                    url = GetBrowserYandexUrl(hWnd);
                }

                // Opera 33
                if (window.ProcessName.Equals("opera") && window.WindowTitle.EndsWith("— Opera"))
                {
                    url = GetBrowserOperaUrl(hWnd);
                }

                // Mozilla Firefox 42
                // Mozilla Developer Edition 45
                // Tor Browser 5
                if ((window.ProcessName.Equals("firefox") &&
                        (window.WindowTitle.EndsWith("- Mozilla Firefox") || 
                        window.WindowTitle.EndsWith("- Mozilla Firefox (Приватный просмотр)"))) ||
                    (window.ProcessName.Equals("firefox") &&
                        (window.WindowTitle.EndsWith("- Firefox Developer Edition") || 
                        window.WindowTitle.EndsWith("- Firefox Developer Edition (Приватный просмотр)"))) ||
                    (window.ProcessName.Equals("firefox") && window.WindowTitle.EndsWith("- Tor Browser")))
                {
                    url = GetBrowserFirefoxUrl(hWnd);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("UI Automation exception: " + e.ToString());
                url = null;
            }
            
            return url;
        }
    }
}
