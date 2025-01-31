using Newtonsoft.Json;
using System.Collections.Generic;

namespace Helpers.CraftPackHelper;
public class CraftItemDataPack
{
    [JsonProperty("WorkbenchCrafts")]
    public Dictionary<string, List<CraftableItem>> WorkbenchCrafts { get; set; } = new();
}
public class CraftableItem
{
    [JsonProperty("craftName")]
    public string CraftName { get; set; }

    [JsonProperty("craftRequiredName")]
    public string CraftRequiredName { get; set; }

    [JsonProperty("craftRequired")]
    public string CraftRequired { get; set; }

    [JsonProperty("craftGiven")]
    public string CraftGiven { get; set; }
}
