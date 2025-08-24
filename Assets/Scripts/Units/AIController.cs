using UnityEngine;

namespace Game.Combat
{
    public enum AIProfile { FrontlineGuardian, BacklineNuker, Healer, Skirmisher }

    public class AIController : UnitController, ITargetAware
    {
        public AIProfile Profile = AIProfile.FrontlineGuardian;
        private float _thinkTimer;
        UnitRuntime _cachedTarget;
        public UnitRuntime GetCurrentTarget() => _cachedTarget;

         public override void Tick(float dt)
    {
        if (U.IsDead) return;
        _thinkTimer -= dt;
        if (_thinkTimer > 0f) return;
        _thinkTimer = 0.2f;

        _cachedTarget = PickTarget();
        if (!_cachedTarget) return;

        foreach (var ab in GetComponents<IAbility>())
        {
            if (ab is BasicAttack) continue;
            if (ab.CanCast(U, _cachedTarget))
            {
                RequestResolve(() => ab.Resolve(U, _cachedTarget));
                return;
            }
        }

        var basic = GetComponent<BasicAttack>();
        if (basic && BasicAttack.TryAuto(U, _cachedTarget, basic.RangeTiles))
            RequestResolve(() => basic.Resolve(U, _cachedTarget));
    }

        private UnitRuntime PickTarget()
        {
            // Simple: focus lowest-HP enemy in same column if possible
            var board = GridBoard.Instance;
            UnitRuntime best = null; float bestHpRatio = 2f;
            for (int r = 0; r < GridBoard.Rows; r++)
            for (int c = 0; c < GridBoard.Cols; c++)
            {
                var e = board.Get(U.Side == Side.Allies ? Side.Enemies : Side.Allies, r, c);
                if (!e) continue;
                float hpRatio = e.HP / Mathf.Max(1f, e.Stats.Derived.MaxHP);
                if (hpRatio < bestHpRatio) { bestHpRatio = hpRatio; best = e; }
            }
            return best;
        }
    }
}
