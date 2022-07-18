using Godot;
using System;
using System.Linq;

public class GameAvatar : KinematicBody2D
{
	public int UserId;

	[Export] private float _speed = 200.0f;
	[Signal] public delegate void UpdateGlobalPosition(Vector2 globalPosition);

	public Vector2 Destination;
	public bool InMotion { get; private set; }

	private AnimatedSprite _characterSprite;
	private Label _username;
	public string Username
	{
		get => _username.Text;
		set => _username.Text = value;
	}

	public override void _Ready()
	{
		_username = GetNode<Label>("Username");
		Destination = GlobalPosition;
	}

	public void MoveTo(Vector2 destination)
	{
		Destination = destination;
		if (! InMotion)
		{
			_characterSprite.Play("run");
		}
		InMotion = true;
	}

	public void FocusMe()
	{
		GetNode<Camera2D>("PlayerCamera").Current = true;
	}

	public override void _PhysicsProcess(float delta)
	{
		if (! InMotion) return;

		var movement = GlobalPosition.MoveToward(Destination, _speed * delta) - GlobalPosition;
		var result = MoveAndCollide(movement);
		_characterSprite.FlipH = Vector2.Right.Dot(movement) < 0f;

		if (Mathf.IsEqualApprox(GlobalPosition.DistanceTo(Destination), 0f))
		{
			InMotion = false;
			_characterSprite.Play("idle");
		}

		EmitSignal(nameof(UpdateGlobalPosition), GlobalPosition);
	}

	public void SetAnimatedSpriteFrames(string spriteName)
	{
		var allSprites = GetNode("CharacterSprites").GetChildren().OfType<AnimatedSprite>();
		foreach (var sprite in allSprites)
		{
			sprite.Hide();
		}

		_characterSprite =
			allSprites.FirstOrDefault(s => s.Name.ToLower() == spriteName.ToLower())
			?? GetNode<AnimatedSprite>("CharacterSprites/Default");
		
		_characterSprite.Show();
		_characterSprite.Play("idle");
	}
}
