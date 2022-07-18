using Godot;
using System;

public class SpeechBubble : Node2D
{
	private Node2D _anchor;
	private ColorRect _color_rect;
	private RichTextLabel _rich_text_label;
	private Timer _timer;
	private Tween _tween;
	private const float CHAR_TIME = 0.05F;
	private const float MARGIN = 8.0F;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_anchor = GetNode<Node2D>("Anchor");
		_color_rect = GetNode<ColorRect>("Anchor/ColorRect");
		_rich_text_label = GetNode<RichTextLabel>("Anchor/RichTextLabel");
		_timer = GetNode<Timer>("Timer");
		_tween = GetNode<Tween>("Tween");
		Visible = false;
	}

	public void SetText(string text, float wait_time=3.0F)
	{
		Visible = true;
		_timer.WaitTime = wait_time;
		_timer.Stop();
		
		_rich_text_label.Text = text;

		var text_size = _rich_text_label.GetFont("normal_font").GetStringSize(_rich_text_label.Text);
		_rich_text_label.MarginRight = text_size.x + MARGIN;
		_color_rect.MarginRight = text_size.x + MARGIN;

		float duration = _rich_text_label.Text.Length() * CHAR_TIME; 

		_tween.RemoveAll();
		_tween.InterpolateProperty(_rich_text_label,"percent_visible", 0.0, 1.0, duration);
		_tween.InterpolateProperty(_color_rect,"margin_right", 0.0, text_size.x + MARGIN * 2.0, duration);
		_tween.InterpolateProperty(_anchor,"position", Vector2.Zero, new Vector2(-text_size.x / 2.0F, 0.0F), duration);
		_tween.Start();
	}

	private void _onTweenAllCompleted()
	{
		_timer.Start();
	}

	private void _onTimerTimeout()
	{
		Visible = false;
	}
}
