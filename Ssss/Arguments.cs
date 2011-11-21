using System;
using System.Linq;

namespace Ssss {
    internal static class Arguments {
        private static string[] _Arguments;

        public static void ParseArguments(string[] rawArguments) {
            _Arguments = rawArguments;
            ShowVersion = GetBoolSwitch("v");
            ShowHelp = GetBoolSwitch("h");
            QuietMode = GetBoolSwitch("q");
            ExtraQuietMode = GetBoolSwitch("Q");
            if (ExtraQuietMode) {
                QuietMode = true;
            }
            HexMode = GetBoolSwitch("x");
            SecurityLevel = GetSwitchIntValue("s");
            Threshold = GetSwitchIntValue("t");
            Shares = GetSwitchIntValue("n");
            Token = GetSwitchStringValue("w");
            EnableDiffusion = !GetBoolSwitch("D");
        }

        public static bool ShowVersion { get; private set; }
        public static bool ShowHelp { get; private set; }
        public static bool QuietMode { get; private set; }
        public static bool ExtraQuietMode { get; private set; }
        public static bool HexMode { get; private set; }
        public static int SecurityLevel { get; private set; }

        public static bool HasSpecifiedSecurityLevel {
            get { return SecurityLevel > 0; }
        }

        public static int Threshold { get; private set; }
        public static int Shares { get; private set; }
        public static string Token { get; private set; }
        public static bool EnableDiffusion { get; private set; }

        private static bool GetBoolSwitch(string name) {
            if (_Arguments == null) {
                return false;
            }

            string switchName = GetSwitchName(name);
            return _Arguments.Any(s => switchName.Equals(s, StringComparison.Ordinal));
        }

        private static string GetSwitchStringValue(string switchName) {
            int ixSwitch = Array.IndexOf(_Arguments, GetSwitchName(switchName));
            if ((ixSwitch < 0) || (ixSwitch >= _Arguments.Length - 1)) {
                return null;
            }

            return _Arguments[ixSwitch + 1];
        }

        private static int GetSwitchIntValue(string name) {
            string stringValue = GetSwitchStringValue(name);

            int intValue;
            if (String.IsNullOrEmpty(stringValue) || !Int32.TryParse(stringValue, out intValue)) {
                return 0;
            }

            return intValue;
        }

        private static string GetSwitchName(string name) {
            return "-" + name;
        }
    }
}