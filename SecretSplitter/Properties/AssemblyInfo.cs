using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("SecretSplitter")]
[assembly: AssemblyDescription("Splits up secrets using Shamir's thresholding secret-sharing scheme")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Moserware")]
[assembly: AssemblyProduct("SecretSplitter")]
[assembly: AssemblyCopyright("Copyright © Jeff Moser 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
#if DEBUG
[assembly: InternalsVisibleTo("UnitTests")]
#endif

[assembly: Guid("55d68ae9-b6b2-498e-aa05-090d3ee1f47c")]

[assembly: AssemblyVersion(Moserware.Security.Cryptography.Versioning.VersionInfo.CurrentVersionString)]
[assembly: AssemblyFileVersion(Moserware.Security.Cryptography.Versioning.VersionInfo.CurrentVersionString)]
[assembly: InternalsVisibleTo("SecretSplitterConsole")]
