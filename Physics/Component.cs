using GXPEngine;

public class Component 
{
	public readonly string Name = "Component";
	protected GameObject Owner;
	public Component(GameObject owner) => Owner = owner;
}
