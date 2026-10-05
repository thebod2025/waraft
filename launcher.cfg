$path = 'd:\OneDrive\Desktop\game\1\waraft\versions\V0.05.00-B0013\V0.05.00-B0013.html'
$content = Get-Content $path -Raw
$lines = $content -split "`r?`n"
$hasNonAscii = $false
$lineNo = 0
for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i] -match '[^\x00-\x7F]') {
        $hasNonAscii = $true
        $lineNo = $i + 1
        break
    }
}
if ($hasNonAscii) {
    Write-Host "Non-ASCII at line $lineNo"
} else {
    Write-Host "No non-ASCII found"
}
