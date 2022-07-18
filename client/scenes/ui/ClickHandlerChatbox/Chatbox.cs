using Godot;
using System.Linq;
using SharpScape.Game;
using SharpScape.Game.Dto;
using UsernameColors = System.Collections.Generic.Dictionary<string, Godot.Color>;

public class Chatbox : PanelContainer
{
    internal enum ChatMessageType
    {
        Regular,
        UserEvent,
        SystemEvent
    }

    private ScrollContainer _messageRegion;
    private VBoxContainer _messageList;
    private PackedScene _chatMessage;
    private LineEdit _input;
    private Button _submitButton;
    private CheckBox _autoScroll;

    private SharpScapeClient _client;
    private UsernameColors _usernameColors = new UsernameColors();

    public override void _Ready()
    {
        _client = this.GetSingleton<SharpScapeClient>();
        _client.Connect("ChatMessageReceived", this, nameof(_OnChatMessageReceived));
        _client.Connect("PlayerLoginEvent", this, nameof(_OnPlayerActionEvent), new Godot.Collections.Array { "has logged in" });
        _client.Connect("PlayerLogoutEvent", this, nameof(_OnPlayerActionEvent), new Godot.Collections.Array { "has logged out" });

        _messageRegion = GetNode<ScrollContainer>("MarginContainer/Interface/MessageRegion");
        _messageList = GetNode<VBoxContainer>("MarginContainer/Interface/MessageRegion/MessageList");
        _chatMessage = GD.Load<PackedScene>("res://client/scenes/ui/ClickHandlerChatbox/ChatMessage.tscn");

        _input = GetNode<LineEdit>("MarginContainer/Interface/UserControls/Input");
        _submitButton = GetNode<Button>("MarginContainer/Interface/UserControls/SubmitButton");
        _autoScroll = GetNode<CheckBox>("MarginContainer/Interface/UserControls/AutoScroll");

        _input.GrabFocus();
        _submitButton.Connect("pressed", this, nameof(SendChatMessage));
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ChatSendMessage"))
        {
            SendChatMessage();
        }
    }

    private void SendChatMessage()
    {
        var sendText = _input.Text.Trim();
        if (sendText.Length == 0)
            return;

        var message = new MessageDto(MessageEvent.Message, sendText);
        _client.SendData(Utils.ToJson(message));

        _input.Text = string.Empty;
        _input.GrabFocus();
    }

    private void _OnPlayerActionEvent(string playerJson, string action)
    {
        var playerInfo = Utils.FromJson<PlayerInfo>(playerJson);
        AddMessageToList(playerInfo.UserInfo.Username, action, ChatMessageType.SystemEvent);
    }
    private void _OnChatMessageReceived(string username, string content)
    {
        var messageType = ChatMessageType.Regular;
        if (content.StartsWith("/me "))
        {
            content = string.Join(" ", content.Split(" ").Skip(1).ToArray());
            messageType = ChatMessageType.UserEvent;
        }

        AddMessageToList(username, content, messageType);
    }
    private void AddMessageToList(string username, string content, ChatMessageType messageType)
    {
        if (! _usernameColors.ContainsKey(username))
        {
            _usernameColors[username] = Color.FromHsv(
                hue: GD.Randf(),
                saturation: (float) GD.RandRange(0.3f, 0.7f),
                value: (float) GD.RandRange(0.85f, 0.95f)
                );
        }

        var chatMessage = _chatMessage.Instance();
        var usernameLabel = chatMessage.GetNode<Label>("Username");

        var contentLabel = chatMessage.GetNode<Label>("Content");
        switch (messageType)
        {
            case ChatMessageType.UserEvent:
            {
                usernameLabel.QueueFree();
                contentLabel.AddColorOverride("font_color", _usernameColors[username]);
                contentLabel.Text = $"* {username} {content}";
                break;
            }
            case ChatMessageType.SystemEvent:
            {
                usernameLabel.QueueFree();
                contentLabel.AddColorOverride("font_color", Color.FromHsv(0.5f, 0f, 0.4f));
                contentLabel.Text = $"* {username} {content}";
                break;
            }
            default:
            {
                usernameLabel.Text = $"<{username}>";
                usernameLabel.AddColorOverride("font_color", _usernameColors[username]);
                contentLabel.Text = content;
                break;
            }
        }
        _messageList.AddChild(chatMessage);

        if (_autoScroll.Pressed)
        {
            GetTree().Connect("idle_frame", this, nameof(UpdateMessageRegionScroll), flags: (uint)ConnectFlags.Oneshot);
        }
    }
    private void UpdateMessageRegionScroll()
    {
        _messageRegion.ScrollVertical = (int) _messageList.RectSize.y;
    }
}