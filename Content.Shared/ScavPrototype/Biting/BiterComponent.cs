using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared.Damage;

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
    public float TransferAmount = 15f;

    [DataField, AutoNetworkedField]
    public float HungerAmount = 10f;

    [DataField, AutoNetworkedField]
    public float StrongBiteMultiplier = 3f;
}
