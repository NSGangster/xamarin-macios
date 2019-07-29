using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace BCLTestImporter {
	public static class RegisterTypeGenerator {

		static readonly string UsingReplacement = "%USING%";
		static readonly string KeysReplacement = "%KEY VALUES%";
		static readonly string IsxUnitReplacement = "%IS XUNIT%";

		public static string GenerateCode ((string FailureMessage, Dictionary<string, Type> Types) typeRegistration, bool isXunit,
			string templatePath)
		{
			var importStringBuilder = new StringBuilder ();
			var keyValuesStringBuilder = new StringBuilder ();
			var namespaces = new List<string> ();  // keep track of the namespaces to remove warnings
			if (!string.IsNullOrEmpty (typeRegistration.FailureMessage)) {
				keyValuesStringBuilder.AppendLine ($"#error {typeRegistration.FailureMessage}");
			} else {
				foreach (var a in typeRegistration.Types.Keys) {
					var t = typeRegistration.Types [a];
					if (!string.IsNullOrEmpty (t.Namespace)) {
						if (!namespaces.Contains (t.Namespace)) {
							namespaces.Add (t.Namespace);
							importStringBuilder.AppendLine ($"using {t.Namespace};");
						}
						keyValuesStringBuilder.AppendLine ($"\t\t\t{{ \"{a}\", typeof ({t.FullName})}}, ");
					}
				}
			}

			// got the lines we want to add, read the template and substitute
			using (var reader = new StreamReader (templatePath)) {
				var result = reader.ReadToEnd ();
				result = result.Replace (UsingReplacement, importStringBuilder.ToString ());
				result = result.Replace (KeysReplacement, keyValuesStringBuilder.ToString ());
				result = result.Replace (IsxUnitReplacement, (isXunit) ? "true" : "false");
				return result;
			}
		}

		public static void GenerateCodeToFile ((string FailureMessage, Dictionary<string, Type> Types) typeRegistration, bool isXunit,
			string templatePath, string destinationPath)
		{
			var registerCode = GenerateCode (typeRegistration, isXunit, templatePath);
			using (var file = new StreamWriter (destinationPath, false)) { // false is do not append
				file.Write (registerCode);
			}
		}
	}
}