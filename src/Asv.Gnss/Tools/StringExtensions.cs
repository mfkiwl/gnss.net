﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace Asv.Gnss
{

    /// <summary>
    /// Extension methods for the string data type
    /// </summary>
    public static class StringExtensions
    {

        public static string RightMargin(this string src, int charCount, char fillChar = ' ')
        {
            if (charCount <= 0) return string.Empty;
            var delay = charCount - src.Length;
            if (delay < 0)
            {
                return src.Substring(0, charCount);
            }
            if (delay > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < delay; i++)
                {
                    sb.Append(fillChar);
                }
                sb.Append(src);
                return sb.ToString();
            }
            return src;
        }

        public static string LeftMargin(this string src, int charCount, char fillChar = ' ')
        {
            if (charCount <= 0) return string.Empty;
            var delay = charCount - src.Length;
            if (delay < 0)
            {
                return src.Substring(0, charCount);
            }
            if (delay > 0)
            {
                var sb = new StringBuilder(src);
                for (int i = 0; i < delay; i++)
                {
                    sb.Append(fillChar);
                }
                return sb.ToString();
            }
            return src;
        }

        public static string PadCenter(this string text, int totalWidth, char paddingChar = ' ')
        {

            var length = text.Length;
            var charactersToPad = totalWidth - length;
            if (charactersToPad < 0) throw new ArgumentException("New width must be greater than string length.", nameof(totalWidth));
            var padLeft = charactersToPad / 2 + charactersToPad % 2;
            //add a space to the left if the string is an odd number
            var padRight = charactersToPad / 2;

            var resultBuilder = new StringBuilder(totalWidth);
            for (var i = 0; i < padLeft; i++) resultBuilder.Insert(i, paddingChar);
            for (var i = 0; i < length; i++) resultBuilder.Insert(i + padLeft, text[i]);
            for (var i = totalWidth - padRight; i < totalWidth; i++) resultBuilder.Insert(i, paddingChar);
            return resultBuilder.ToString();
        }


        #region Common string extensions

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Determines whether the specified string is null or empty.
        /// </summary>
        /// <param name="value">The string value to check.</param>
        public static bool IsEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Determines whether the specified string is not null or empty.
        /// </summary>
        /// <param name="value">The string value to check.</param>
        public static bool IsNotEmpty(this string value)
        {
            return (value.IsEmpty() == false);
        }

        /// <summary>
        /// Checks whether the string is empty and returns a default value in case.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Either the string or the default value.</returns>
        public static string IfEmpty(this string value, string defaultValue)
        {
            return (value.IsNotEmpty() ? value : defaultValue);
        }

        /// <summary>
        /// Formats the value with the parameters using string.Format.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static string FormatWith(this string value, params object[] parameters)
        {
            return String.Format(value, parameters);
        }

        public static string TryFormatWith(this string format, object arg0)
        {
            try
            {
                return String.Format(format, arg0);
            }
            catch
            {
                return format;
            }
        }

        public static string TryFormat(this string format, params object[] args)
        {
            try
            {
                return String.Format(format, args);
            }
            catch
            {
                return format;
            }

        }

        public static string TryFormatWith(this string format, object arg0, object arg1)
        {
            try
            {
                return String.Format(format, arg0, arg1);
            }
            catch
            {
                return format;
            }

        }

        public static string TryFormatWith(this string format, object arg0, object arg1, object arg2)
        {
            try
            {
                return String.Format(format, arg0, arg1, arg2);
            }
            catch
            {
                return format;
            }

        }


        /// <summary>
        /// Trims the text to a provided maximum length.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <returns></returns>
        /// <remarks>Proposed by Rene Schulte</remarks>
        public static string TrimToMaxLength(this string value, int maxLength)
        {
            return (value == null || value.Length <= maxLength ? value : value.Substring(0, maxLength));
        }

        /// <summary>
        /// Trims the text to a provided maximum length and adds a suffix if required.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns></returns>
        /// <remarks>Proposed by Rene Schulte</remarks>
        public static string TrimToMaxLength(this string value, int maxLength, string suffix)
        {
            return (value == null || value.Length <= maxLength
                        ? value
                        : String.Concat(value.Substring(0, maxLength), suffix));
        }

        /// <summary>
        /// Determines whether the comparison value strig is contained within the input value string
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="comparisonValue">The comparison value.</param>
        /// <param name="comparisonType">Type of the comparison to allow case sensitive or insensitive comparison.</param>
        /// <returns>
        /// 	<c>true</c> if input value contains the specified value, otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this string inputValue, string comparisonValue, StringComparison comparisonType)
        {
            return (inputValue.IndexOf(comparisonValue, comparisonType) != -1);
        }

      

        /// <summary>
        /// Loads the string into a XML XPath DOM (XPathDocument)
        /// </summary>
        /// <param name="xml">The XML string.</param>
        /// <returns>The XML XPath document object model (XPathNavigator)</returns>
        public static XPathNavigator ToXPath(this string xml)
        {
            var document = new XPathDocument(new StringReader(xml));
            return document.CreateNavigator();
        }

        /// <summary>
        /// Reverses / mirrors a string.
        /// </summary>
        /// <param name="value">The string to be reversed.</param>
        /// <returns>The reversed string</returns>
        public static string Reverse(this string value)
        {
            if (value.IsEmpty() || (value.Length == 1)) return value;

            var chars = value.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Ensures that a string starts with a given prefix.
        /// </summary>
        /// <param name="value">The string value to check.</param>
        /// <param name="prefix">The prefix value to check for.</param>
        /// <returns>The string value including the prefix</returns>
        /// <example>
        /// <code>
        /// var extension = "txt";
        /// var fileName = string.Concat(file.Name, extension.EnsureStartsWith("."));
        /// </code>
        /// </example>
        public static string EnsureStartsWith(this string value, string prefix)
        {
            if (value.StartsWith(prefix))
                return value;
            return String.Concat(prefix, value);
        }

        /// <summary>
        /// Ensures that a string ends with a given suffix.
        /// </summary>
        /// <param name="value">The string value to check.</param>
        /// <param name="suffix">The suffix value to check for.</param>
        /// <returns>The string value including the suffix</returns>
        /// <example>
        /// <code>
        /// var url = "http://www.pgk.de";
        /// url = url.EnsureEndsWith("/"));
        /// </code>
        /// </example>
        public static string EnsureEndsWith(this string value, string suffix)
        {
            if (value.EndsWith(suffix))
                return value;
            return String.Concat(value, suffix);
        }

        #endregion

        #region Regex based extension methods

        /// <summary>
        /// Uses regular expressions to determine if the string matches to a given regex pattern.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <returns>
        /// 	<c>true</c> if the value is matching to the specified pattern; otherwise, <c>false</c>.
        /// </returns>
        /// <example>
        /// <code>
        /// var s = "12345";
        /// var isMatching = s.IsMatchingTo(@"^\d+$");
        /// </code>
        /// </example>
        public static bool IsMatchingTo(this string value, string regexPattern)
        {
            return IsMatchingTo(value, regexPattern, RegexOptions.None);
        }

        /// <summary>
        /// Uses regular expressions to determine if the string matches to a given regex pattern.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <param name="options">The regular expression options.</param>
        /// <returns>
        /// 	<c>true</c> if the value is matching to the specified pattern; otherwise, <c>false</c>.
        /// </returns>
        /// <example>
        /// <code>
        /// var s = "12345";
        /// var isMatching = s.IsMatchingTo(@"^\d+$");
        /// </code>
        /// </example>
        public static bool IsMatchingTo(this string value, string regexPattern, RegexOptions options)
        {
            return Regex.IsMatch(value, regexPattern, options);
        }

        /// <summary>
        /// Uses regular expressions to replace parts of a string.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <param name="replaceValue">The replacement value.</param>
        /// <returns>The newly created string</returns>
        /// <example>
        /// <code>
        /// var s = "12345";
        /// var replaced = s.ReplaceWith(@"\d", m => string.Concat(" -", m.Value, "- "));
        /// </code>
        /// </example>
        public static string ReplaceWith(this string value, string regexPattern, string replaceValue)
        {
            return ReplaceWith(value, regexPattern, replaceValue, RegexOptions.None);
        }

        /// <summary>
        /// Uses regular expressions to replace parts of a string.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <param name="replaceValue">The replacement value.</param>
        /// <param name="options">The regular expression options.</param>
        /// <returns>The newly created string</returns>
        /// <example>
        /// <code>
        /// var s = "12345";
        /// var replaced = s.ReplaceWith(@"\d", m => string.Concat(" -", m.Value, "- "));
        /// </code>
        /// </example>
        public static string ReplaceWith(this string value, string regexPattern, string replaceValue,
                                         RegexOptions options)
        {
            return Regex.Replace(value, regexPattern, replaceValue, options);
        }

        /// <summary>
        /// Uses regular expressions to replace parts of a string.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <param name="evaluator">The replacement method / lambda expression.</param>
        /// <returns>The newly created string</returns>
        /// <example>
        /// <code>
        /// var s = "12345";
        /// var replaced = s.ReplaceWith(@"\d", m => string.Concat(" -", m.Value, "- "));
        /// </code>
        /// </example>
        public static string ReplaceWith(this string value, string regexPattern, MatchEvaluator evaluator)
        {
            return ReplaceWith(value, regexPattern, RegexOptions.None, evaluator);
        }

        /// <summary>
        /// Uses regular expressions to replace parts of a string.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <param name="options">The regular expression options.</param>
        /// <param name="evaluator">The replacement method / lambda expression.</param>
        /// <returns>The newly created string</returns>
        /// <example>
        /// <code>
        /// var s = "12345";
        /// var replaced = s.ReplaceWith(@"\d", m => string.Concat(" -", m.Value, "- "));
        /// </code>
        /// </example>
        public static string ReplaceWith(this string value, string regexPattern, RegexOptions options,
                                         MatchEvaluator evaluator)
        {
            return Regex.Replace(value, regexPattern, evaluator, options);
        }

        /// <summary>
        /// Uses regular expressions to determine all matches of a given regex pattern.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <returns>A collection of all matches</returns>
        public static MatchCollection GetMatches(this string value, string regexPattern)
        {
            return GetMatches(value, regexPattern, RegexOptions.None);
        }

        /// <summary>
        /// Uses regular expressions to determine all matches of a given regex pattern.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <param name="options">The regular expression options.</param>
        /// <returns>A collection of all matches</returns>
        public static MatchCollection GetMatches(this string value, string regexPattern, RegexOptions options)
        {
            return Regex.Matches(value, regexPattern, options);
        }

        /// <summary>
        /// Uses regular expressions to determine all matches of a given regex pattern and returns them as string enumeration.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <returns>An enumeration of matching strings</returns>
        /// <example>
        /// <code>
        /// var s = "12345";
        /// foreach(var number in s.GetMatchingValues(@"\d")) {
        ///  Console.WriteLine(number);
        /// }
        /// </code>
        /// </example>
        public static IEnumerable<string> GetMatchingValues(this string value, string regexPattern)
        {
            return GetMatchingValues(value, regexPattern, RegexOptions.None);
        }

        /// <summary>
        /// Uses regular expressions to determine all matches of a given regex pattern and returns them as string enumeration.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <param name="options">The regular expression options.</param>
        /// <returns>An enumeration of matching strings</returns>
        /// <example>
        /// <code>
        /// var s = "12345";
        /// foreach(var number in s.GetMatchingValues(@"\d")) {
        ///  Console.WriteLine(number);
        /// }
        /// </code>
        /// </example>
        public static IEnumerable<string> GetMatchingValues(this string value, string regexPattern, RegexOptions options)
        {
            foreach (Match match in GetMatches(value, regexPattern, options))
            {
                if (match.Success) yield return match.Value;
            }
        }

        /// <summary>
        /// Uses regular expressions to split a string into parts.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <returns>The splitted string array</returns>
        public static string[] Split(this string value, string regexPattern)
        {
            return value.Split(regexPattern, RegexOptions.None);
        }

        /// <summary>
        /// Uses regular expressions to split a string into parts.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="regexPattern">The regular expression pattern.</param>
        /// <param name="options">The regular expression options.</param>
        /// <returns>The splitted string array</returns>
        public static string[] Split(this string value, string regexPattern, RegexOptions options)
        {
            return Regex.Split(value, regexPattern, options);
        }

        /// <summary>
        /// Splits the given string into words and returns a string array.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <returns>The splitted string array</returns>
        public static string[] GetWords(this string value)
        {
            return value.Split(@"\W");
        }

        /// <summary>
        /// Gets the nth "word" of a given string, where "words" are substrings separated by a given separator
        /// </summary>
        /// <param name="value">The string from which the word should be retrieved.</param>
        /// <param name="index">Index of the word (0-based).</param>
        /// <returns>
        /// The word at position n of the string.
        /// Trying to retrieve a word at a position lower than 0 or at a position where no word exists results in an exception.
        /// </returns>
        /// <remarks>Originally contributed by MMathews</remarks>
        public static string GetWordByIndex(this string value, int index)
        {
            var words = value.GetWords();

            if ((index < 0) || (index > words.Length - 1))
            {
                throw new IndexOutOfRangeException("The word number is out of range.");
            }

            return words[index];
        }

        #endregion



        #region Bytes & Base64

        /// <summary>
        /// Converts the string to a byte-array using the default encoding
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <returns>The created byte array</returns>
        public static byte[] ToBytes(this string value)
        {
            return value.ToBytes(null);
        }

        /// <summary>
        /// Converts the string to a byte-array using the supplied encoding
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="encoding">The encoding to be used.</param>
        /// <returns>The created byte array</returns>
        /// <example><code>
        /// var value = "Hello World";
        /// var ansiBytes = value.ToBytes(Encoding.GetEncoding(1252)); // 1252 = ANSI
        /// var utf8Bytes = value.ToBytes(Encoding.UTF8);
        /// </code></example>
        public static byte[] ToBytes(this string value, Encoding encoding)
        {
            encoding = (encoding ?? Encoding.Default);
            return encoding.GetBytes(value);
        }

        /// <summary>
        /// Encodes the input value to a Base64 string using the default encoding.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>The Base 64 encoded string</returns>
        public static string EncodeBase64(this string value)
        {
            return value.EncodeBase64(null);
        }

        /// <summary>
        /// Encodes the input value to a Base64 string using the supplied encoding.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The Base 64 encoded string</returns>
        public static string EncodeBase64(this string value, Encoding encoding)
        {
            encoding = (encoding ?? Encoding.UTF8);
            var bytes = encoding.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decodes a Base 64 encoded value to a string using the default encoding.
        /// </summary>
        /// <param name="encodedValue">The Base 64 encoded value.</param>
        /// <returns>The decoded string</returns>
        public static string DecodeBase64(this string encodedValue)
        {
            return encodedValue.DecodeBase64(null);
        }

        /// <summary>
        /// Decodes a Base 64 encoded value to a string using the supplied encoding.
        /// </summary>
        /// <param name="encodedValue">The Base 64 encoded value.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The decoded string</returns>
        public static string DecodeBase64(this string encodedValue, Encoding encoding)
        {
            encoding = (encoding ?? Encoding.UTF8);
            var bytes = Convert.FromBase64String(encodedValue);
            return encoding.GetString(bytes);
        }

        #endregion

        #region Cripto

        public static Guid GetMd5Hash(this string s)
        {
            //переводим строку в байт-массим  
            byte[] bytes = Encoding.Unicode.GetBytes(s);

            //создаем объект для получения средст шифрования  
            var csp = new MD5CryptoServiceProvider();

            //вычисляем хеш-представление в байтах  
            byte[] byteHash = csp.ComputeHash(bytes);

            string hash = byteHash.Aggregate(String.Empty, (current, b) => current + String.Format("{0:x2}", b));

            //формируем одну цельную строку из массива  

            return new Guid(hash);
        }

        public static string EncryptAES(this string plainText, string password)
        {
            byte[] Results;
            var utf8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            var hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(password));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            var tdesAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            tdesAlgorithm.Key = tdesKey;
            tdesAlgorithm.Mode = CipherMode.ECB;
            tdesAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] dataToEncrypt = utf8.GetBytes(plainText);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
                Results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(Results);
        }

        public static string DecryptAES(this string plainText, string password)
        {
            byte[] results;
            var utf8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            var hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(password));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            var tdesAlgorithm = new TripleDESCryptoServiceProvider
                {Key = tdesKey, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7};

            // Step 3. Setup the decoder

            // Step 4. Convert the input string to a byte[]
            byte[] dataToDecrypt = Convert.FromBase64String(plainText);

            // Step 5. Attempt to decrypt the string
            try
            {
                var decryptor = tdesAlgorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return utf8.GetString(results);
        }

        #endregion

        public static string DrawProgress(double value, int width)
        {
            return DrawProgress(value, width, "█", "░");
            
        }

        /// <summary>
        /// Возвращает строку с отрисованным индикатором процесса. value = от 0 до 1. width - длинна поля в символах. Символы отрисовки можно задать.
        /// Пример: ██████░░░░░░ 50%
        /// </summary>
        /// <param name="value"></param>
        /// <param name="width"></param>
        /// <param name="fill"></param>
        /// <param name="empty"></param>
        /// <returns></returns>
        public static string DrawProgress(double value, int width, string fill, string empty)
        {
            const int LabelWidth = 4;
            const int MinWidth = LabelWidth + 2;
            if (width < MinWidth)
                throw new Exception($"{nameof(width)} mast be more then {MinWidth}");

            var realWidth = width - LabelWidth;
            var w1 = (int)(value * realWidth);
            var w2 = realWidth - w1;
            var sb = new StringBuilder();
            for (int i = 0; i < w1; i++)
            {
                sb.Append(fill);
            }
            for (int i = 0; i < w2; i++)
            {
                sb.Append(empty);
            }
            sb.Append(((int)(value * 100) + "%").PadLeft(LabelWidth));
            return sb.ToString();
        }
    }

}

