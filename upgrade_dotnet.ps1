# ============================================
# Sentinel Diagnostics - LangVersion Updater
# Adds or updates <LangVersion>latest</LangVersion>
# ============================================

function Update-LangVersion {
    param([string]$FilePath)

    $content = Get-Content $FilePath

    # Check if LangVersion already exists
    if ($content -match "<LangVersion>") {
        Write-Host "Updating LangVersion in: $FilePath"
        $updated = $content -replace "<LangVersion>.*?</LangVersion>", "<LangVersion>latest</LangVersion>"
        Set-Content -Path $FilePath -Value $updated
    }
    else {
        Write-Host "Adding LangVersion to: $FilePath"

        # Insert LangVersion inside the first PropertyGroup
        $inserted = $content -replace "</PropertyGroup>", "  <LangVersion>latest</LangVersion>`n</PropertyGroup>"
        Set-Content -Path $FilePath -Value $inserted
    }
}

Write-Host "Scanning for .csproj files..."

# Recursively find all .csproj files
$csprojFiles = Get-ChildItem -Recurse -Filter *.csproj

foreach ($file in $csprojFiles) {
    Update-LangVersion -FilePath $file.FullName
}

Write-Host "`nAll project files updated to use C# 'latest'."
