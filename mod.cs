using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace Shovelwaredev.NoFirHideout;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "shovelwaredev-nofirhideout";
    public string Name { get; init; } = "No FIR Hideout";
    public string Author { get; init; } = "shovelwaredev";
    public SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.6");
    public List<string>? Contributors { get; init; } = [];
    public List<string>? Incompatibilities { get; init; } = [];
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = null;
    public string? Url { get; init; } = null;
}

[Injectable]
public class NoFirHideoutMod(HideoutTable hideoutTable) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        int changed = 0;

        if (hideoutTable?.Areas == null)
        {
            Console.WriteLine("[Shovel] No FIR Hideout: hideoutTable.Areas is null!");
            return Task.CompletedTask;
        }

        foreach (var area in hideoutTable.Areas)
        {
            if (area?.Stages == null)
            {
                continue;
            }

            foreach (var stage in area.Stages.Values)
            {
                if (stage?.Requirements == null)
                {
                    continue;
                }

                foreach (var requirement in stage.Requirements)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (requirement == null)
                    {
                        continue;
                    }

                    var property = requirement.GetType().GetProperty("IsSpawnedInSession");

                    if (property?.CanWrite == true)
                    {
                        // Check for both regular bool and nullable bool (bool?)
                        var propType = property.PropertyType;
                        var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

                        if (underlyingType == typeof(bool))
                        {
                            property.SetValue(requirement, false);
                            changed++;
                        }
                    }
                }
            }
        }

        Console.WriteLine($"[Shovel] No FIR Hideout loaded. Updated {changed} requirements.");
        return Task.CompletedTask;
    }
}