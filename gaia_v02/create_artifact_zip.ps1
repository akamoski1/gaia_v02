param(
	[Parameter(Mandatory=$true)] [string] $ArtifactsDir,
	[Parameter(Mandatory=$true)] [string] $PublishDir,
	[Parameter(Mandatory=$true)] [string] $ProjectDir,
	[Parameter(Mandatory=$true)] [string] $Configuration,
	[Parameter(Mandatory=$true)] [string] $TargetFramework
)

try {
	$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
	$temp = Join-Path $ArtifactsDir ("tempzip_$timestamp")
	if (Test-Path $temp) { Remove-Item $temp -Recurse -Force }
	New-Item -ItemType Directory -Path $temp | Out-Null

	# Copy source (exclude common build/artifact folders)
	robocopy "$ProjectDir" "$temp\source" /MIR /XD bin obj .git artifacts .vs /XF *.user *.suo *.cache *.zip | Out-Null

	# Copy published output
	if (Test-Path $PublishDir) {
		robocopy "$PublishDir" "$temp\publish" /MIR | Out-Null
	}

	# Use a static filename (no timestamp) so this artifact overwrites cleanly in source control,
	# letting Git's history track each build instead of accumulating multiple zip files on disk.
	$zipName = "Gaia_v02_${Configuration}_${TargetFramework}.zip"
	$zip = Join-Path $ArtifactsDir $zipName

	# Cleanup: remove any previously produced artifact zips so only the latest packaging is kept.
	Get-ChildItem -Path $ArtifactsDir -Filter 'Gaia_v02_*.zip' -File -ErrorAction SilentlyContinue |
		Where-Object { $_.FullName -ne $zip } |
		Remove-Item -Force

	if (Test-Path $zip) { Remove-Item $zip -Force }

	Compress-Archive -Path (Join-Path $temp '*') -DestinationPath $zip -Force

	Remove-Item $temp -Recurse -Force
	Write-Host "Created ZIP: $zip"
	exit 0
}
catch {
	Write-Error "Failed to create artifact zip: $_"
	exit 1
}
