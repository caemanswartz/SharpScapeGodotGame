using Godot;
using System;

public class main : Node2D
{
	public override void _Ready()
	{
		if (OS.GetName() == "HTML5")
			GetTree().ChangeScene("res://client/scenes/MainLogin/MainLogin.tscn");
		else
			GetTree().ChangeScene("res://server/scenes/ServerStaging.tscn");
	}
}
