using GXPEngine;

public class Rigidbody : Component
{
    public readonly new string Name = "Rigidbody";

    public float Velocity { get; protected set; } = 0;
    public float AngularVelocity { get; protected set; } = 0;
    public float LinearDrag { get; protected set; } = .5f;
    public float Friction { get; protected set; } = .86f;
    public float Bounciness { get; protected set; }
    public float Mass { get; protected set; }
    public Rigidbody(GameObject owner) : base(owner)
	{

	}
}