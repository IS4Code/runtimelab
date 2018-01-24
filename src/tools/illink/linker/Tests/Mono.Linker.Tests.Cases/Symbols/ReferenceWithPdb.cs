﻿using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;
using Mono.Linker.Tests.Cases.Symbols.Dependencies;

namespace Mono.Linker.Tests.Cases.Symbols {
	[Reference ("LibraryWithPdb.dll")]
	[SandboxDependency ("Dependencies/LibraryWithPdb/LibraryWithPdb.dll", "input/LibraryWithPdb.dll")]
	[SandboxDependency ("Dependencies/LibraryWithPdb/LibraryWithPdb.pdb", "input/LibraryWithPdb.pdb")]
	[SetupLinkerLinkSymbols ("false")]

	[RemovedSymbols ("LibraryWithPdb.dll")]

	[KeptMemberInAssembly ("LibraryWithPdb.dll", typeof (LibraryWithPdb), "SomeMethod()")]
	[RemovedMemberInAssembly ("LibraryWithPdb.dll",typeof (LibraryWithPdb), "NotUsed()")]
	public class ReferenceWithPdb {
		static void Main ()
		{
			// Use some stuff so that we can verify that the linker output correct results
			SomeMethod ();
			LibraryWithPdb.SomeMethod ();
		}

		[Kept]
		static void SomeMethod ()
		{
		}

		static void NotUsed ()
		{
		}
	}
}