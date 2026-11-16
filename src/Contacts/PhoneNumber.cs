using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EDPay.Sdk.Contacts
{
    public sealed class PhoneNumberRules
    {
        public string CountryCallingCode { get; set; } = "258";
        public int NationalLength { get; set; } = 9;
        public IReadOnlyList<string> NationalPrefixes { get; set; } = Array.Empty<string>();
        public IReadOnlyList<int> GroupSizes { get; set; } = Array.Empty<int>();

        public static PhoneNumberRules Emola() => new PhoneNumberRules
        {
            CountryCallingCode = "258", NationalLength = 9,
            NationalPrefixes = new[] { "86", "87" }, GroupSizes = new[] { 2, 3, 4 }
        };

        public static PhoneNumberRules Mpesa() => new PhoneNumberRules
        {
            CountryCallingCode = "258", NationalLength = 9,
            NationalPrefixes = new[] { "84", "85" }, GroupSizes = new[] { 2, 3, 4 }
        };
    }

    public static class PhoneNumber
    {
        /// <summary>Returns preset phone rules for known phone methods, or null for non-phone/custom methods.</summary>
        private static string? CanonicalMethod(string? method) => method == null
            ? null
            : method.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");

        public static PhoneNumberRules? RulesForPaymentMethod(string? method) => CanonicalMethod(method) switch
        {
            "emola" => PhoneNumberRules.Emola(),
            "mpesa" => PhoneNumberRules.Mpesa(),
            _ => null
        };

        public static bool RequiresPhoneForPaymentMethod(string? method) => RulesForPaymentMethod(method) != null;

        /// <summary>Uses the API's phone representation for eMola/M-Pesa and passes other gateway values through unchanged.</summary>
        public static string NormalizeForPaymentMethod(string? method, string value)
        {
            var rules = RulesForPaymentMethod(method);
            if (rules == null) return value;
            var national = Normalize(value, rules, international: false);
            return string.Equals(CanonicalMethod(method), "mpesa", StringComparison.Ordinal)
                ? "258" + national
                : national;
        }

        public static string FormatForPaymentMethod(string? method, string value)
        {
            var rules = RulesForPaymentMethod(method);
            return rules == null ? value : Format(value, rules);
        }

        /// <summary>Non-phone and custom methods are not interpreted as phone numbers.</summary>
        public static bool IsValidForPaymentMethod(string? method, string value)
        {
            var rules = RulesForPaymentMethod(method);
            return rules == null || IsValid(value, rules);
        }

        private static (string CountryCode, string National) Parse(string value, PhoneNumberRules? rules)
        {
            rules ??= new PhoneNumberRules();
            if (rules.CountryCallingCode == null || rules.NationalPrefixes == null || rules.GroupSizes == null)
                throw new ArgumentException("Phone number rules cannot contain null values.", nameof(rules));
            var countryCode = rules.CountryCallingCode.StartsWith("+", StringComparison.Ordinal)
                ? rules.CountryCallingCode.Substring(1)
                : rules.CountryCallingCode;
            if (!Regex.IsMatch(countryCode, "^[1-9][0-9]{0,2}$") || rules.NationalLength < 1 || countryCode.Length + rules.NationalLength > 15)
                throw new ArgumentException("Phone number rules are invalid.", nameof(rules));
            if (rules.NationalPrefixes.Any(prefix => string.IsNullOrEmpty(prefix) || !Regex.IsMatch(prefix, "^[0-9]+$")))
                throw new ArgumentException("National prefixes must contain digits only.", nameof(rules));
            if (rules.GroupSizes.Any(size => size < 1))
                throw new ArgumentException("Group sizes must be positive.", nameof(rules));
            value = value?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, @"^\+?[0-9\s().-]+$") || value.Count(c => c == '(') != value.Count(c => c == ')'))
                throw new ArgumentException("Phone number contains unsupported characters.", nameof(value));
            var digits = Regex.Replace(value, "\\D", "");
            var international = value.TrimStart().StartsWith("+", StringComparison.Ordinal);
            if (digits.StartsWith("00", StringComparison.Ordinal)) { digits = digits.Substring(2); international = true; }
            if (!international && digits.StartsWith(countryCode, StringComparison.Ordinal) && digits.Length > rules.NationalLength) international = true;
            if (international)
            {
                if (!digits.StartsWith(countryCode, StringComparison.Ordinal)) throw new ArgumentException("Phone number does not use the configured calling code.", nameof(value));
                digits = digits.Substring(countryCode.Length);
            }
            if (digits.Length != rules.NationalLength || !Regex.IsMatch(digits, "^[0-9]+$")) throw new ArgumentException("Phone number does not match the configured national length.", nameof(value));
            if (rules.NationalPrefixes.Count > 0 && !rules.NationalPrefixes.Any(prefix => digits.StartsWith(prefix, StringComparison.Ordinal))) throw new ArgumentException("Phone number does not match configured prefixes.", nameof(value));
            return (countryCode, digits);
        }

        public static string Normalize(string value, PhoneNumberRules? rules = null, bool international = true)
        {
            var parsed = Parse(value, rules);
            return international ? "+" + parsed.CountryCode + parsed.National : parsed.National;
        }
        public static string AddCountryPrefix(string value, PhoneNumberRules? rules = null) => Normalize(value, rules, true);
        public static string RemoveCountryPrefix(string value, PhoneNumberRules? rules = null) => Normalize(value, rules, false);
        public static bool IsValid(string value, PhoneNumberRules? rules = null)
        {
            try { Parse(value, rules); return true; } catch (ArgumentException) { return false; }
        }
        public static string Format(string value, PhoneNumberRules? rules = null)
        {
            rules ??= new PhoneNumberRules();
            var parsed = Parse(value, rules);
            IReadOnlyList<int> groups = rules.GroupSizes.Count == 0
                ? parsed.CountryCode == "258" && parsed.National.Length == 9
                    ? new[] { 2, 3, 4 }
                    : new[] { parsed.National.Length }
                : rules.GroupSizes;
            var parts = new List<string>();
            var offset = 0;
            foreach (var size in groups)
            {
                if (size < 1) throw new ArgumentException("Group sizes must be positive.", nameof(rules));
                if (offset >= parsed.National.Length) break;
                var count = Math.Min(size, parsed.National.Length - offset);
                parts.Add(parsed.National.Substring(offset, count));
                offset += count;
            }
            if (offset < parsed.National.Length) parts.Add(parsed.National.Substring(offset));
            return "+" + parsed.CountryCode + " " + string.Join(" ", parts);
        }
    }
}
