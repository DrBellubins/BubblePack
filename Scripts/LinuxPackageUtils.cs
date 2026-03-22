using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

public static class LinuxPackageUtils
{
    public enum PackageManagerType
    {
        Unknown,
        Apt,       // Debian, Ubuntu, Mint, Pop!_OS, etc.
        Dnf,       // Fedora (newer), RHEL 8+, CentOS Stream
        Yum,       // Older RHEL/CentOS
        Rpm,       // Fallback for rpm-based systems
        Pacman,    // Arch, Manjaro
        Zypper,    // openSUSE
        // Add others as needed (apk → Alpine, xbps → Void, etc.)
    }

    public static PackageManagerType DetectPackageManager()
    {
        if (File.Exists("/usr/bin/apt-get") || File.Exists("/usr/bin/apt")) 
            return PackageManagerType.Apt;
        if (File.Exists("/usr/bin/dnf")) 
            return PackageManagerType.Dnf;
        if (File.Exists("/usr/bin/yum")) 
            return PackageManagerType.Yum;
        if (File.Exists("/usr/bin/pacman")) 
            return PackageManagerType.Pacman;
        if (File.Exists("/usr/bin/zypper")) 
            return PackageManagerType.Zypper;
        if (File.Exists("/usr/bin/rpm")) 
            return PackageManagerType.Rpm;

        return PackageManagerType.Unknown;
    }

    public static string[] GetInstalledPackages()
    {
        var pm = DetectPackageManager();
        
        if (pm == PackageManagerType.Unknown)
            throw new PlatformNotSupportedException("Unsupported Linux distribution/package manager");

        string command = pm switch
        {
            PackageManagerType.Apt    => "dpkg -l | grep '^ii' | awk '{print $2}'",
            PackageManagerType.Dnf    => "dnf list installed --quiet | awk '{print $1}'",
            PackageManagerType.Yum    => "yum list installed | awk '{print $1}' | tail -n +2",
            PackageManagerType.Pacman => "pacman -Qq",
            PackageManagerType.Zypper => "zypper se --installed-only -s | awk '{print $3}' | tail -n +3",
            PackageManagerType.Rpm    => "rpm -qa",
            _                         => throw new NotSupportedException()
        };

        var psi = new ProcessStartInfo
        {
            FileName               = "/bin/bash",
            Arguments              = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };

        using var process = Process.Start(psi);
        using var reader  = process.StandardOutput;
        
        string output = reader.ReadToEnd();
        
        process.WaitForExit();

        return output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(line => line.Trim())
                     .Where(pkg => !string.IsNullOrWhiteSpace(pkg))
                     .ToArray();
    }
}