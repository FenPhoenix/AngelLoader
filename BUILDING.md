## Important notes on updating targeted .NET version - must read this to make it work!

#### Modern .NET breaks one of the theme hooks, so we have to modify System.Drawing.Primitives.dll to un-break it with every new .NET version targeted. Here are my notes on how to do it, slightly cleaned up but basically as I wrote them:

- Download .NET explicit version branch zip file from the [dotnet repo](https://github.com/dotnet/runtime) repo (eg. release/11.0)

- Extract it to a folder with no spaces in the name - that's required! Also, make sure no personal info is in the path because it will bake the path into the dll - so just put it in for example, `C:\net11.0`

- Remove `[SuppressGCTransition]` attribute from GetSysColor() in `/src/libraries/Common/src/Interop/Windows/User32/Interop.GetSysColor.cs`

- Ensure Python 3 is installed and added to path environment variable

- Build .NET with build.cmd in the base source dir, with the following command line: `build.cmd -a x64 -c Release -hc Release -lc Release -rc Release -os windows -rf CoreCLR -projects src\libraries\System.Drawing.Primitives\System.Drawing.Primitives.slnx`.

The build may take a couple of minutes.

---

After build:

- For .NET 11 for example, the dll is here: `/artifacts/bin/System.Drawing.Primitives/Release/net11.0-windows/System.Drawing.Primitives.dll`

- Copy it to `/bin_dependencies` in the AL repo dir

- In AngelLoader.csproj, ensure this section exists:
```xml
	<!--
	Override the stock System.Drawing.Primitives.dll with our custom version that removes the [SuppressGCTransition]
    attribute from GetSysColor() in order to make the hook work again.
	
	But this doesn't work with UI designers, it can't find any methods in System.Drawing if this thing is being
	replaced (I guess it gets replaced too late?). So use a separate profile just for UI work.
	-->
	<ItemGroup>
		<Reference Condition="'$(Configuration)'!='Debug_UIWork'" Include="System.Drawing.Primitives">
			<HintPath>..\bin_dependencies\System.Drawing.Primitives.dll</HintPath>
		</Reference>
	</ItemGroup>

	<Target Condition="'$(Configuration)'!='Debug_UIWork'" Name="RemoveSystemDrawingPrimitives" AfterTargets="ResolveTargetingPackAssets" BeforeTargets="ResolvePackageFileConflicts">
		<ItemGroup>
			<ReferencesToExclude Include="@(Reference)" Condition="'%(Filename)' == 'System.Drawing.Primitives'" />
			<Reference Remove="@(ReferencesToExclude)" />
		</ItemGroup>
	</Target>
```

- Ensure AngelLoader.deps.json is being copied to dest folder after build (should be in post_build.bat)

Done!
