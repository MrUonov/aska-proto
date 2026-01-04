using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Whitelist;

namespace Content.Shared.ScavPrototype.Biting;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(BiterSystem))]
public sealed partial class BiterComponent : Component
{
    [DataField]
    public EntProtoId BiteAction = "DuneBiteAction";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? BiteActionEntity;

    [DataField, AutoNetworkedField]
    public float BiteTime = 2f;

    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier BiteDamage = default!;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist = new()
    {
        Components = new[]
        {
            "MobState",
        }
    };

    [DataField, AutoNetworkedField]
    public float TransferAmount = 15f;
}
