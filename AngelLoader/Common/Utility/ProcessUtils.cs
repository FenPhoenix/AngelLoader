using System.Diagnostics;

namespace AngelLoader;

public static partial class Utils
{
    /*
    @NET5(ProcessUtils):
    These wrappers that set UseShellExecute to true are just here for compatibility with .NET Core 3 and
    above:
    In Framework, UseShellExecute defaults to true, but in Core 3 and above, it defaults to false (it's
    something to do with cross-platform concerns). We just want to keep it true to keep behavior the same
    and I think sometimes we want it true because there are behavioral differences and some things only
    work with it true or false. I can't remember the details at the moment but yeah.
    */

    /// <inheritdoc cref="Process.Start(string)"/>
    internal static void ProcessStart_UseShellExecute(string fileName)
    {
        using (Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true }))
        {
        }
    }

    #region Disabled until needed

#if false

    /// <summary>
    /// Starts a process resource by specifying the name of an application and a set of command-line arguments, and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
    /// <para>
    /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
    /// </para>
    /// </summary>
    /// <param name="fileName">The name of an application file to run in the process.</param>
    /// <param name="arguments">Command-line arguments to pass when starting the process.</param>
    /// <exception cref="T:System.InvalidOperationException">The <paramref name="fileName" /> or <paramref name="arguments" /> parameter is <see langword="null" />.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file.
    /// -or-
    /// The sum of the length of the arguments and the length of the full path to the process exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
    /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
    /// <exception cref="T:System.IO.FileNotFoundException">The PATH environment variable has a string containing quotes.</exception>
    internal static void ProcessStart_UseShellExecute(string fileName, string arguments)
    {
        using (Process.Start(new ProcessStartInfo { FileName = fileName, Arguments = arguments, UseShellExecute = true }))
        {
        }
    }

    /// <summary>
    /// Starts a process resource by specifying the name of an application, a set of command-line arguments, a user name, a password, and a domain and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
    /// <para>
    /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
    /// </para>
    /// </summary>
    /// <param name="fileName">The name of an application file to run in the process.</param>
    /// <param name="arguments">Command-line arguments to pass when starting the process.</param>
    /// <param name="userName">The user name to use when starting the process.</param>
    /// <param name="password">A <see cref="T:System.Security.SecureString" /> that contains the password to use when starting the process.</param>
    /// <param name="domain">The domain to use when starting the process.</param>
    /// <exception cref="T:System.InvalidOperationException">No file name was specified.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file.
    /// -or-
    /// The sum of the length of the arguments and the length of the full path to the associated file exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
    /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Method not supported on Linux or macOS (.NET Core only).</exception>
    internal static void ProcessStart_UseShellExecute(string fileName, string arguments, string userName, SecureString password, string domain)
    {
        using (
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UserName = userName,
                Password = password,
                Domain = domain,
                UseShellExecute = true
            }))
        {
        }
    }

    /// <summary>
    /// Starts a process resource by specifying the name of an application, a user name, a password, and a domain and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
    /// <para>
    /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
    /// </para>
    /// </summary>
    /// <param name="fileName">The name of an application file to run in the process.</param>
    /// <param name="userName">The user name to use when starting the process.</param>
    /// <param name="password">A <see cref="T:System.Security.SecureString" /> that contains the password to use when starting the process.</param>
    /// <param name="domain">The domain to use when starting the process.</param>
    /// <exception cref="T:System.InvalidOperationException">No file name was specified.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">There was an error in opening the associated file.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Method not supported on Linux or macOS (.NET Core only).</exception>
    internal static void ProcessStart_UseShellExecute(string fileName, string userName, SecureString password, string domain)
    {
        using (
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                UserName = userName,
                Password = password,
                Domain = domain,
                UseShellExecute = true
            }))
        {
        }
    }

#endif

    #endregion

    /// <summary>
    /// Starts the process resource that is specified by the parameter containing process start information (for example, the file name of the process to start) and associates the resource with a new <see cref="T:System.Diagnostics.Process" /> component.
    /// <para>
    /// *Use this for Framework and Core compatibility: Core has UseShellExecute off by default (but we want it on).
    /// </para>
    /// </summary>
    /// <param name="startInfo">The <see cref="T:System.Diagnostics.ProcessStartInfo" /> that contains the information that is used to start the process, including the file name and any command-line arguments.</param>
    /// <param name="overrideUseShellExecuteToOn">Force UseShellExecute to be <see langword="true"/></param>
    /// <exception cref="T:System.InvalidOperationException">No file name was specified in the <paramref name="startInfo" /> parameter's <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property.
    /// -or-
    /// The <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property of the <paramref name="startInfo" /> parameter is <see langword="true" /> and the <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardInput" />, <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardOutput" />, or <see cref="P:System.Diagnostics.ProcessStartInfo.RedirectStandardError" /> property is also <see langword="true" />.
    /// -or-
    /// The <see cref="P:System.Diagnostics.ProcessStartInfo.UseShellExecute" /> property of the <paramref name="startInfo" /> parameter is <see langword="true" /> and the <see cref="P:System.Diagnostics.ProcessStartInfo.UserName" /> property is not <see langword="null" /> or empty or the <see cref="P:System.Diagnostics.ProcessStartInfo.Password" /> property is not <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="startInfo" /> parameter is <see langword="null" />.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The process object has already been disposed.</exception>
    /// <exception cref="T:System.IO.FileNotFoundException">The file specified in the <paramref name="startInfo" /> parameter's <see cref="P:System.Diagnostics.ProcessStartInfo.FileName" /> property could not be found.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">An error occurred when opening the associated file.
    /// -or-
    /// The sum of the length of the arguments and the length of the full path to the process exceeds 2080. The error message associated with this exception can be one of the following: "The data area passed to a system call is too small." or "Access is denied."</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Method not supported on operating systems without shell support such as Nano Server (.NET Core only).</exception>
    internal static void ProcessStart_UseShellExecute(ProcessStartInfo startInfo, bool overrideUseShellExecuteToOn = true)
    {
        if (overrideUseShellExecuteToOn) startInfo.UseShellExecute = true;
        using (Process.Start(startInfo)) { }
    }
}
