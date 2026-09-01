[CmdletBinding()]
param(
    # Official public equivalent of the act displayed in LEX.
    [string] $Url = 'https://eli.gov.pl/api/acts/DU/2015/1731/text.html',

    [string] $OutputPath = (Join-Path $PSScriptRoot '..\source-data\sejm-2015-kandydaci.csv'),

    # Enables repeatable tests without downloading the document again.
    [string] $InputHtmlPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function ConvertTo-PlainText {
    param([Parameter(Mandatory)][string] $Html)

    $withoutTags = [regex]::Replace($Html, '<[^>]+>', ' ')
    $decoded = [System.Net.WebUtility]::HtmlDecode($withoutTags)
    return [regex]::Replace($decoded, '\s+', ' ').Trim()
}

function Get-CandidateFields {
    param([Parameter(Mandatory)][string] $Text)

    # The surname is uppercase and the final token is the number of votes.
    $match = [regex]::Match(
        $Text,
        '^(?<surname>[\p{Lu}][\p{Lu}\s''-]*?)\s+(?<givenNames>.+?)\s+(?<votes>\d[\d\s\u00A0]*)$')

    if (-not $match.Success) {
        throw "Could not parse candidate row: '$Text'"
    }

    return [pscustomobject]@{
        Surname    = $match.Groups['surname'].Value.Trim()
        GivenNames = $match.Groups['givenNames'].Value.Trim()
        Votes      = [int]($match.Groups['votes'].Value -replace '[^0-9]', '')
    }
}

if ($InputHtmlPath) {
    $html = Get-Content -LiteralPath $InputHtmlPath -Raw -Encoding utf8
}
else {
    # ELI serves this document without a reliable charset header. Do not use
    # Invoke-WebRequest.Content here: Windows PowerShell may decode UTF-8 as
    # a legacy code page (for example, "Józef" becomes "JÃ³zef").
    $webClient = [System.Net.WebClient]::new()
    try {
        $html = [System.Text.Encoding]::UTF8.GetString($webClient.DownloadData($Url))
    }
    finally {
        $webClient.Dispose()
    }
}

$rows = [System.Collections.Generic.List[object]]::new()
$districtMatches = [regex]::Matches($html, 'id="bran_II-chpt_(?<district>\d+)"')

if ($districtMatches.Count -ne 41) {
    throw "Expected 41 districts; found $($districtMatches.Count). The document format changed."
}

for ($districtIndex = 0; $districtIndex -lt $districtMatches.Count; $districtIndex++) {
    $districtMatch = $districtMatches[$districtIndex]
    $district = [int]$districtMatch.Groups['district'].Value
    $districtEnd = $html.Length
    $nextDistrictIndex = $districtIndex + 1
    if ($nextDistrictIndex -lt $districtMatches.Count) {
        $districtEnd = $districtMatches[$nextDistrictIndex].Index
    }
    $districtHtml = $html.Substring($districtMatch.Index, $districtEnd - $districtMatch.Index)

    # Section 2 contains list and candidate results. Each pass_* is one list.
    $listMatches = [regex]::Matches(
        $districtHtml,
        "id=`"bran_II-chpt_$district-schp_2-pass_(?<listNumber>\d+)`"")

    for ($listIndex = 0; $listIndex -lt $listMatches.Count; $listIndex++) {
        $listMatch = $listMatches[$listIndex]
        $listEnd = $districtHtml.Length
        $nextListIndex = $listIndex + 1
        if ($nextListIndex -lt $listMatches.Count) {
            $listEnd = $listMatches[$nextListIndex].Index
        }
        $listHtml = $districtHtml.Substring($listMatch.Index, $listEnd - $listMatch.Index)
        $listNumber = [int]$listMatch.Groups['listNumber'].Value

        $committeeMatch = [regex]::Match(
            $listHtml,
            '(?s)Na list[^<]*?przez\s*(?<committee>.*?)\s*oddano')
        if (-not $committeeMatch.Success) {
            throw "Committee not found for district $district, list $listNumber."
        }
        $committee = ConvertTo-PlainText $committeeMatch.Groups['committee'].Value

        $candidateMatches = [regex]::Matches(
            $listHtml,
            "(?s)id=`"bran_II-chpt_$district-schp_2-pass_$listNumber-pint_\d+`".*?<div[^>]+data-template=`"xText`"[^>]*>(?<candidate>.*?)</div>")

        foreach ($candidateMatch in $candidateMatches) {
            $candidate = Get-CandidateFields (ConvertTo-PlainText $candidateMatch.Groups['candidate'].Value)
            $rows.Add([pscustomobject]@{
                    DistrictNumber = $district
                    ListNumber     = $listNumber
                    Committee      = $committee
                    GivenNames     = $candidate.GivenNames
                    Surname        = $candidate.Surname
                    Votes          = $candidate.Votes
                })
        }
    }
}

if ($rows.Count -ne 7858) {
    throw "Exported $($rows.Count) candidates instead of 7858. CSV was not saved."
}

$resolvedOutputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
$outputDirectory = Split-Path -Parent $resolvedOutputPath
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$csvEncoding = if ($PSVersionTable.PSVersion.Major -lt 6) { 'UTF8' } else { 'utf8BOM' }
$rows |
    Sort-Object DistrictNumber, ListNumber, Surname, GivenNames |
    Export-Csv -LiteralPath $resolvedOutputPath -NoTypeInformation -Encoding $csvEncoding

Write-Host "Saved $($rows.Count) candidates to: $resolvedOutputPath"
