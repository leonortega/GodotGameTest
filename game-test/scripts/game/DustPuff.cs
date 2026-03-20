using Godot;

namespace GameTest;

public partial class DustPuff : Node2D
{
    private sealed class DustParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Radius;
        public float Age;
        public float Lifetime;
    }

    private readonly List<DustParticle> _particles = [];
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        ZIndex = 20;
        TopLevel = false;
        _rng.Randomize();
    }

    public void Configure(Vector2 worldPosition, int facing, float strength)
    {
        GlobalPosition = worldPosition;
        _particles.Clear();

        var particleCount = 6 + Mathf.RoundToInt(strength * 4f);
        for (var index = 0; index < particleCount; index++)
        {
            var lateralBias = facing * _rng.RandfRange(8f, 32f);
            var spreadVelocity = _rng.RandfRange(-70f, 70f) + lateralBias;
            var riseVelocity = -_rng.RandfRange(22f, 64f) * (0.7f + strength * 0.4f);
            _particles.Add(new DustParticle
            {
                Position = new Vector2(_rng.RandfRange(-8f, 8f), _rng.RandfRange(-3f, 2f)),
                Velocity = new Vector2(spreadVelocity, riseVelocity),
                Radius = _rng.RandfRange(2.5f, 5.5f) * (0.85f + strength * 0.35f),
                Lifetime = _rng.RandfRange(0.18f, 0.34f) + strength * 0.08f
            });
        }

        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_particles.Count == 0)
        {
            QueueFree();
            return;
        }

        var deltaSeconds = (float)delta;
        for (var index = _particles.Count - 1; index >= 0; index--)
        {
            var particle = _particles[index];
            particle.Age += deltaSeconds;
            if (particle.Age >= particle.Lifetime)
            {
                _particles.RemoveAt(index);
                continue;
            }

            particle.Position += particle.Velocity * deltaSeconds;
            particle.Velocity = new Vector2(
                Mathf.MoveToward(particle.Velocity.X, 0f, 140f * deltaSeconds),
                particle.Velocity.Y + 110f * deltaSeconds);
        }

        if (_particles.Count == 0)
        {
            QueueFree();
            return;
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (var particle in _particles)
        {
            var progress = particle.Age / particle.Lifetime;
            var alpha = (1f - progress) * 0.72f;
            var color = new Color(0.86f, 0.79f, 0.66f, alpha);
            DrawCircle(particle.Position, particle.Radius, color);
        }
    }
}