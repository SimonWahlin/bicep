[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [int] $BuildId
)

$ErrorActionPreference = 'Stop';

$org = 'https://dev.azure.com/msazure/';
$project = 'One';
$pipelineName = 'BicepMirror-Official'

$buildDefinition = az pipelines show --org $org --project $project --name $pipelineName | ConvertFrom-Json;

if($BuildId)
{
  # TODO we can probably make this better by searching for a specific version instead of build ID
  # get specific build
  $build = az pipelines runs show --org $org --project $project --id $BuildId | ConvertFrom-Json;
  
  # the build could be for any pipeline
  if($build.definition.id -ne $buildDefinition.id) {
    Write-Error "The specified Build ID '$BuildId' belongs to build definition '$($build.definition.name)'. Expected the '$($buildDefinition.name)' build definition instead.";
  }
}
else
{
  # get latest build
  $build = (az pipelines runs list --org $org --project $project --pipeline-ids $buildDefinition.id --top 1 | ConvertFrom-Json)[0];
}

$artifacts = az pipelines runs artifact list --org $org --project $project --run-id $build.id | ConvertFrom-Json;

$artifacts