$gitVersionOutput = dotnet gitversion /output json | ConvertFrom-Json
"AssemblyVersion=$($gitVersionOutput.AssemblySemVer)" | Out-File gitversion.props
"FileVersion=$($gitVersionOutput.SemVer)" | Out-File gitversion.props -Append
"InformationalVersion=$($gitVersionOutput.InformationalVersion)" | Out-File gitversion.props -Append
   