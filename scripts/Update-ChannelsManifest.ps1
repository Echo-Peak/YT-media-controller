Param(
  [Parameter(Mandatory = $true)][string]$Tag,
  [string]$Branch = $env:GITHUB_REF_NAME,
  [string]$Repo = $env:GITHUB_REPOSITORY,
  [string]$MetaBranch = 'update-metadata' 
)

$ErrorActionPreference = 'Stop'
$repoRoot = (git rev-parse --show-toplevel)

if (-not $Repo) { throw "Repo (owner/name) not provided and GITHUB_REPOSITORY is empty." }


if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw "GitHub CLI 'gh' is required." }
if (-not $env:GH_TOKEN -and -not $env:GITHUB_TOKEN) { throw "GH_TOKEN or GITHUB_TOKEN must be set." }
if (-not $env:GH_TOKEN) { $env:GH_TOKEN = $env:GITHUB_TOKEN }

$channel = 'alpha'

if ($Tag -like '*-beta.*') {
  $channel = 'beta'
}
elseif ($Tag -like '*-stable.*') {
  $channel = 'stable'
}

Write-Host "Updating manifest for tag=$Tag channel=$channel repo=$Repo"


$rel = gh api "repos/$Repo/releases/tags/$Tag" | ConvertFrom-Json
$publishedAt = $rel.published_at
if (-not $publishedAt) { $publishedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") }


$exe = $rel.assets | Where-Object { $_.name -match '\.exe$' } | Select-Object -First 1
if (-not $exe) { throw "No .exe asset found on release $Tag." }
$downloadUrl = $exe.browser_download_url
$assetName = $exe.name

# Download asset and compute SHA256
$temp = New-Item -ItemType Directory -Path ([IO.Path]::Combine($env:TEMP, "manifest-$([Guid]::NewGuid().Guid)"))
try {
  gh release download $Tag --repo $Repo -p $assetName -D $temp.FullName | Out-Null
  $file = Join-Path $temp.FullName $assetName
  if (-not (Test-Path $file)) { throw "Downloaded file not found: $file" }
  $sha256 = (Get-FileHash -Algorithm SHA256 -Path $file).Hash.ToLower()
}
finally {
  Remove-Item $temp -Recurse -Force -ErrorAction SilentlyContinue
}

$hasRemote = (git ls-remote --heads origin "$MetaBranch") -ne $null

if ($hasRemote) {
  git fetch origin "$MetaBranch" | Out-Null
  git checkout -B "$MetaBranch" "origin/$MetaBranch"
}
else {
  git checkout --orphan "$MetaBranch"
  Get-ChildItem -Force | Where-Object { $_.Name -ne '.git' } |
  Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}


$channelsPath = Join-Path $repoRoot 'channels.json'

if (-not (Test-Path $channelsPath)) {
  '{}' | Out-File -FilePath $channelsPath -Encoding UTF8

  git config user.name  "github-actions"
  git config user.email "actions@users.noreply.github.com"
  git add $channelsPath
  git commit -m "chore: init channels.json on $MetaBranch" | Out-Null
  git push origin "HEAD:$MetaBranch"
}

try {
  $channels = Get-Content $channelsPath -Raw | ConvertFrom-Json -AsHashtable
}
catch {
  $channels = @{}
}


if (-not $channels.ContainsKey($channel)) { $channels[$channel] = @{} }

$versionValue = $Tag.TrimStart('v')

$channels[$channel]['version'] = $versionValue
$channels[$channel]['checksum'] = $sha256
$channels[$channel]['installerUrl'] = $downloadUrl


$channels | ConvertTo-Json -Depth 20 | Out-File -FilePath $channelsPath -Encoding UTF8

git config user.name  "github-actions"
git config user.email "actions@users.noreply.github.com"

$changed = (git status --porcelain $channelsPath)
if ($changed) {
  git add $channelsPath
  git commit -m "update($channel): $Tag"
  git push origin HEAD:$MetaBranch
  Write-Host "Pushed manifest update to 'updates' branch."
}
else {
  Write-Host "No manifest changes to commit."
}