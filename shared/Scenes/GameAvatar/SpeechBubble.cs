using Godot;
using System;

public class SpeechBubble : Node2D
{
	private Node2D _anchor;
	private PanelContainer _background;
	private Label _message;
	private Timer _timer;
	private Tween _tween;
	private const float CHAR_TIME = 0.015F;
	private const float MARGIN = 2.0F;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_anchor = GetNode<Node2D>("Anchor");
		_background = GetNode<PanelContainer>("Anchor/Background");
		_message = GetNode<Label>("Anchor/Background/Message");
		_timer = GetNode<Timer>("Timer");
		_tween = GetNode<Tween>("Tween");
		Visible = false;
	}

	public void SetText(string text, float waitTime = 3.0F)
	{
		Show();
		_timer.WaitTime = waitTime;
		_timer.Stop();
		
		_message.Text = text;

		var textSize = _message.GetFont("normal_font").GetStringSize(_message.Text);
		_message.MarginRight = textSize.x + MARGIN;
		_background.MarginRight = textSize.x + MARGIN;

		float duration = _message.Text.Length() * CHAR_TIME;

		float totalTextWidth = textSize.x + MARGIN * 2.0f;
		float maxBackgroundWidth = Mathf.Min(GetViewport().Size.x * 0.75f, totalTextWidth);
		float visibleWidthPercentage = maxBackgroundWidth / totalTextWidth;
		float adjustedDuration = duration * visibleWidthPercentage;

		_tween.RemoveAll();
		_tween.InterpolateProperty(_message,"percent_visible", 0.0, visibleWidthPercentage, adjustedDuration);
		_tween.InterpolateProperty(_background,"margin_right", 15.0, maxBackgroundWidth, adjustedDuration);
		_tween.InterpolateProperty(_anchor,"position", Vector2.Zero, new Vector2(-maxBackgroundWidth / 2.0F, 0.0F), adjustedDuration);
		_tween.Start();
	}

	private void _onTweenAllCompleted()
	{
		_timer.Start();
	}

	private void _onTimerTimeout()
	{
		Hide();
		_message.MarginRight = 0f;
		_background.MarginRight = 0f;
		_anchor.Position = new Vector2();
	}
}
