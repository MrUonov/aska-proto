using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Whitelist;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Content.Shared.IdentityManagement;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.ScavPrototype.Biting;

public sealed class BiterSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] protected readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BiterComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<BiterComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<BiterComponent, BiteActionEvent>(OnBiteAction);
        SubscribeLocalEvent<BiterComponent, BiteDoAfterEvent>(OnDoAfter);
    }

    private void OnInit(Entity<BiterComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent.Owner, ref ent.Comp.BiteActionEntity, ent.Comp.BiteAction);
    }

    private void OnShutdown(Entity<BiterComponent> ent, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(ent.Owner, ent.Comp.BiteActionEntity);
    }

    private void OnBiteAction(Entity<BiterComponent> ent, ref BiteActionEvent args)
    {
        if (args.Handled || _whitelistSystem.IsWhitelistFailOrNull(ent.Comp.Whitelist, args.Target)) {
            _popupSystem.PopupClient(Loc.GetString("bite-action-popup-message-fail"), ent.Owner, ent.Owner);
            return;
        }

        args.Handled = true;

        _popupSystem.PopupClient(Loc.GetString("bite-action-popup-message-succes", ("target", Identity.Entity(args.Target, EntityManager))), ent.Owner, ent.Owner);
        _popupSystem.PopupClient(Loc.GetString("bite-action-popup-message-succes-other", ("user", Identity.Entity(ent.Owner, EntityManager))), args.Target, args.Target);

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, ent.Owner, ent.Comp.BiteTime, new BiteDoAfterEvent(), ent.Owner, target: args.Target, used: ent.Owner)
        {
            BreakOnMove = true,
        });
    }

    private void OnDoAfter(Entity<BiterComponent> ent, ref BiteDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        var target = args.Target.Value;

        _popupSystem.PopupEntity(Loc.GetString("bite-complete-popup-message", ("user", Identity.Entity(ent.Owner, EntityManager)), ("target", Identity.Entity(target, EntityManager))), ent.Owner, PopupType.MediumCaution);

        _damageable.TryChangeDamage(target, ent.Comp.BiteDamage, origin: ent.Owner);

        if (!TryComp<BloodstreamComponent>(target, out var streamComp))
            return;

        var (bloodReagent, _) = streamComp.BloodReferenceSolution.Contents[0];
        var bloodInjection = new Solution(bloodReagent.Prototype, ent.Comp.TransferAmount);

        _bloodstreamSystem.TryModifyBloodLevel(target, ent.Comp.TransferAmount);
        _bloodstreamSystem.TryAddToBloodstream(ent.Owner, bloodInjection);
    }
}

public sealed partial class BiteActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class BiteDoAfterEvent : SimpleDoAfterEvent;
