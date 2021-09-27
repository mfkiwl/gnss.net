using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Asv.Gnss
{
    public static class AssemblyInfoExtentions
    {
        public static Version GetVersion(this Assembly src)
        {
            return src.GetName().Version;
        }

        public static string GetInformationalVersion(this Assembly src)
        {
            var attributes = src.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            return attributes.Length == 0 ? "" : ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;
        }

        public static string GetTitle(this Assembly src)
        {
            var attributes = src.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title.Length > 0) return titleAttribute.Title;
            }
            return System.IO.Path.GetFileNameWithoutExtension(src.CodeBase);
        }

        public static string GetProductName(this Assembly src)
        {
            var attributes = src.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            return attributes.Length == 0 ? "" : ((AssemblyProductAttribute)attributes[0]).Product;
        }

        public static string GetDescription(this Assembly src)
        {
            var attributes = src.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            return attributes.Length == 0 ? "" : ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }

        public static string GetCopyrightHolder(this Assembly src)
        {
            var attributes = src.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            return attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        public static string GetCompanyName(this Assembly src)
        {
            var attributes = src.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            return attributes.Length == 0 ? "" : ((AssemblyCompanyAttribute)attributes[0]).Company;
        }
    }

    public static class ConsoleWelcomePrinter
    {
        public static void PrintWelcomeToConsole(this Assembly src, ConsoleColor color = ConsoleColor.Cyan)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(src.PrintWelcome());
            Console.ForegroundColor = old;
        }

        public static string PrintWelcome(this Assembly src)
        {
            var header = new[]
            {
                src.GetTitle(),
                src.GetDescription(),
                src.GetCopyrightHolder(),
            };
            var values = new Dictionary<string, string>
            {
                {"Version",src.GetInformationalVersion() },
#if DEBUG
                {"Build","Debug" },
#else
                {"Build", "Release" },
#endif
                {"Process",Process.GetCurrentProcess().Id.ToString() },
                {"OS",Environment.OSVersion.ToString() },
                {"Machine",Environment.MachineName },
                {nameof(Environment.UserDomainName), Environment.UserDomainName},
                {nameof(Environment.UserName), Environment.UserName},
                {"Environment",Environment.Version.ToString() },
            };
            return PrintWelcome(header, values);
        }



        private static string PrintWelcome(IEnumerable<string> header, Dictionary<string, string> values,
            int padding = 1)
        {
            var keysWidth = values.Select(_ => _.Key.Length).Max();
            var valueWidth = values.Select(_ => _.Value.Length).Max();
            valueWidth = Math.Max(valueWidth, header.Select(_ => _.Length).Max() - keysWidth);

            return PrintWelcome(header, values, keysWidth, valueWidth, padding);
        }

        public static string PrintWelcome(IEnumerable<string> header,IEnumerable<KeyValuePair<string,string>> values, int keyWidth,int valueWidth, int padding)
        {
            var sb = new StringBuilder();

            var headerWidth = keyWidth + valueWidth + padding * 4+1;
            sb.Append('╔').Append('═', headerWidth).Append('╗').Append(' ').AppendLine();
            foreach (var hdr in header)
            {
                sb.Append("║").Append(' ', padding).Append(hdr.PadLeft(headerWidth-padding*2)).Append(' ', padding).Append("║▒").AppendLine();
            }
            sb.Append('╠').Append('═', padding*2).Append('═', keyWidth).Append('╦').Append('═', valueWidth).Append('═', padding*2).Append("╣▒").AppendLine();
            foreach (var pair in values)
            {
                sb.Append('║').Append(' ', padding).Append(pair.Key.PadLeft(keyWidth)).Append(' ', padding).Append('║').Append(' ', padding).Append(pair.Value.PadRight(valueWidth)).Append(' ', padding).Append("║▒").AppendLine();
            }

            sb.Append('╚').Append('═', padding * 2).Append('═', keyWidth).Append('╩').Append('═', valueWidth).Append('═', padding * 2).Append("╝▒").AppendLine();
            sb.Append(' ').Append('▒', headerWidth + 2);
            return sb.ToString();
        }

        
    }
}
