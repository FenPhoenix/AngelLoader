## Important notes on updating targeted .NET version - must read this to make it work!

#### Modern .NET breaks one of the theme hooks, so we have to modify System.Drawing.Primitives.dll to un-break it with every new .NET version targeted. Here are my notes on how to do it, slightly cleaned up but basically as I wrote them:

- Download .NET explicit version branch zip file from the [dotnet repo](https://github.com/dotnet/runtime) repo (eg. release/9.0)

- Extract it to a folder with no spaces in the name - that's required! Also, make sure no personal info is in the path because it will bake the path into the dll - so just put it in for example, `C:\net9.0`

- Remove `[SuppressGCTransition]` attribute from GetSysColor() in `/src/libraries/Common/src/Interop/Windows/User32/Interop.GetSysColor.cs`

- Ensure Python 3 is installed, and by installed I mean:
  - Download and install it from the site, making sure to check "Add to path env var" or whatever
  - After it's done, for some reason you also have to open a command prompt, type "python3" whereupon some dumbass Microsoft Store looking window will pop up and you have to click the "Get" button, whereupon Python will ACTUALLY be installed, as opposed to the half-pretend installed it was before.

- Build .NET with build.cmd in the base source dir. Should be `build.cmd -a x64 -c Release -hc Release -lc Release -rc Release -os windows -rf CoreCLR`. If we need to know or refresh our memory on the options, type "build /?"

The build will take a long time. You can apparently build "subsets" but I vaguely recall playing around with that and wasn't able to get it to work. We could try again at some point, but meh. One full build every year is fine...

---

After build:

- At least for .NET 7, the product version will be 42.42.42424 (higher than default)

- For .NET 7, the x64 dll is here: `/artifacts/obj/Microsoft.NETCore.App.Bundle/Release/net7.0/win-x64/output/shared/Microsoft.NETCore.App/7.0.11/System.Drawing.Primitives.dll`

- For .NET 8, the x64 dll is here: `/artifacts/obj/Microsoft.NETCore.App.Bundle/Release/net8.0/win-x64/output/shared/Microsoft.NETCore.App/8.0.0/System.Drawing.Primitives.dll`

(Note: I guess the path is probably always the same but with differing version numbers?)

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

There should now be errors for anything referencing the System.Drawing.Primitives namespace.

- In VS, "Add project reference..." (you can browse for a dll reference within there, a little unintuitive)
- Browse for System.Drawing.Primitives.dll, add it
- Ensure AngelLoader.deps.json is being copied to dest folder after build (should be in post_build.bat)

Done!
