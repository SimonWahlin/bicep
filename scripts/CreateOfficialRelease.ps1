[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string] $WorkingDir,

    [Parameter(Mandatory = $false)]
    [int] $BuildId
)

$ErrorActionPreference = 'Stop';

$org = 'https://dev.azure.com/msazure/';
$project = 'One';
$pipelineName = 'BicepMirror-Official'

$artifacts = @(
  @{
    buildArtifactName = 'drop_build_bicep_linux';
    assets = @(
      @{
        assetName = 'bicep-linux-x64';
        relativePath = 'bicep-Release-linux-x64/bicep';
      }
    )
  },
  @{
    buildArtifactName = 'drop_build_bicep_osx';
    assets = @(
      @{
        assetName = 'bicep-osx-x64';
        relativePath = 'bicep-Release-osx-x64/bicep';
      }
    )
  },
  @{
    buildArtifactName = 'drop_build_bicep_windows';
    assets = @(
      @{
        assetName = 'bicep-setup-win-x64.exe';
        relativePath = 'bicep-setup-win-x64/bicep-setup-win-x64.exe';
      },
      @{
        assetName = 'bicep-win-x64.exe';
        relativePath = 'bicep-Release-win-x64/bicep.exe';
      }#,
      # @{
      #   assetName = 'bicep-langserver.zip';
      #   relativePath = 'bicep.LangServer/**/*';
      # }
    )
  },
  @{
    buildArtifactName = 'drop_build_vsix';
    assets = @(
      @{
        assetName = 'vscode-bicep.vsix';
        relativePath = '';
      }
    )
  }  
)

Write-Output "Creating working dir...";
New-Item -ItemType Directory -Path $WorkingDir -Force;

Write-Output "Resolving build definition...";
$buildDefinition = az pipelines show --org $org --project $project --name $pipelineName | ConvertFrom-Json;

if($BuildId)
{
  # TODO we can probably make this better by searching for a specific version instead of build ID
  # get specific build
  Write-Output "Resolving build by ID...";
  $build = az pipelines runs show --org $org --project $project --id $BuildId | ConvertFrom-Json;
  
  # the build could be for any pipeline
  if($build.definition.id -ne $buildDefinition.id) {
    Write-Error "The specified Build ID '$BuildId' belongs to build definition '$($build.definition.name)'. Expected the '$($buildDefinition.name)' build definition instead.";
  }
}
else
{
  # get latest build
  Write-Output "Resolving latest build...";
  $build = (az pipelines runs list --org $org --project $project --pipeline-ids $buildDefinition.id --top 1 | ConvertFrom-Json)[0];
}

Write-Output "Getting artifact URLs...";
$buildArtifacts = az pipelines runs artifact list --org $org --project $project --run-id $build.id | ConvertFrom-Json;

foreach ($artifact in $artifacts) {
  Write-Output "Processing artifact $($artifact.buildArtifactName)...";

  $artifactDownloadPath = Join-Path -Path $WorkingDir -ChildPath $artifact.buildArtifactName;
  $buildArtifact = $buildArtifacts | Where-Object { $_.name -eq $artifact.buildArtifactName};

  if($buildArtifact)
  {
    az pipelines runs artifact download --org $org --project $project --artifact-name $artifact.buildArtifactName --path $artifactDownloadPath --run-id $build.id
  }
  else
  {
    Write-Warning "The artifact '$($artifact.buildArtifactName)' is not present in the specified build.";
  }
}

