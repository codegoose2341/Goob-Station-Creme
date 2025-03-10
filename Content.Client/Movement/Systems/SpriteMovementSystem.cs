using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client.Movement.Systems;

/// <summary>
/// Handles setting sprite states based on whether an entity has movement input.
/// </summary>
public sealed class SpriteMovementSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<SpriteComponent> _spriteQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpriteMovementComponent, MoveInputEvent>(OnSpriteMoveInput);
        _spriteQuery = GetEntityQuery<SpriteComponent>();
    }

    private void OnSpriteMoveInput(EntityUid uid, SpriteMovementComponent component, ref MoveInputEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var oldMoving = (SharedMoverController.GetNormalizedMovement(args.OldMovement) & MoveButtons.AnyDirection) != MoveButtons.None;
        var moving = (SharedMoverController.GetNormalizedMovement(args.Component.HeldMoveButtons) & MoveButtons.AnyDirection) != MoveButtons.None;

        var oldWalking = (SharedMoverController.GetNormalizedMovement(args.OldMovement) & MoveButtons.Walk) != MoveButtons.None;
        var walking = (SharedMoverController.GetNormalizedMovement(args.Component.HeldMoveButtons) & MoveButtons.Walk) != MoveButtons.None;

        if ((oldMoving == moving && oldWalking == walking) || !_spriteQuery.TryGetComponent(uid, out var sprite))
            return;

        if (moving)
        {
            foreach (var (layer, state) in component.MovementLayers)
            {
                sprite.LayerSetData(layer, state);
            }

            if (walking)
            {
                foreach (var (layer, state) in component.WalkLayers)
                {
                    sprite.LayerSetData(layer, state);
                }
            }
            else
            {
                foreach (var (layer, state) in component.RunLayers)
                {
                    sprite.LayerSetData(layer, state);
                }
            }
        }
        else
        {
            foreach (var (layer, state) in component.NoMovementLayers)
            {
                sprite.LayerSetData(layer, state);
            }
        }
    }
}
