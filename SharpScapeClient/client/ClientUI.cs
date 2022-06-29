using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;

public class ClientUI : Control
{
	public Utils _utils;
	public Client _client;
    public RichTextLabel _logDest;
	public LineEdit _lineEdit;
	public LineEdit _host;
	public OptionButton _writeMode;
    private LoginModal _loginModal;

    public override void _Ready()
	{
		_utils=GetNode<Utils>("/root/Utils");
		_client = GetNode<Client>("WebsocketClient");
		_client.Connect("WriteLine", this, "_OnClientWriteLine");
		_logDest = GetNode<RichTextLabel>("Panel/VBoxContainer/MainOutput");
		_lineEdit = GetNode<LineEdit>("Panel/VBoxContainer/Send/LineEdit");
		_host = GetNode<LineEdit>("Panel/VBoxContainer/Connect/Host");
		_writeMode = GetNode<OptionButton>("Panel/VBoxContainer/Settings/Mode");
		_writeMode.Clear();
		_writeMode.AddItem("BINARY");
		_writeMode.SetItemMetadata(0, WebSocketPeer.WriteMode.Binary);
		_writeMode.AddItem("TEXT");
		_writeMode.SetItemMetadata(1, WebSocketPeer.WriteMode.Text);
	}
	private void SpawnLoginModal()
	{
		_loginModal = GD.Load<PackedScene>("res://client/LoginModal.tscn").Instance() as LoginModal;
		_loginModal.Connect("LoginPayloadReady", this, "_OnLoginPayloadReady");
		AddChild(_loginModal);
	}
	private void _OnLoginPayloadReady()
	{
		_utils._Log(_logDest, $"Connecting to host: {_host.Text}");
		string[] supportedProtocols = {"my-protocol2", "my-protocol", "binary"};
		_client.ConnectToUrl(_host.Text, supportedProtocols);
		_client.Websocket.Connect("connection_established", this, "_OnWebsocketConnectionEstablished", flags: (uint)ConnectFlags.Oneshot);
	}
	private void _OnWebsocketConnectionEstablished(string protocol)
	{
		var wm = (WebSocketPeer.WriteMode)_writeMode.GetSelectedMetadata();
		_client.SetWriteMode(WebSocketPeer.WriteMode.Text);
		var loginPayload = new Godot.Collections.Dictionary() {
			["event"] = "login",
			["data"] = _loginModal.SecurePayload
		};
		_client.SendData(JSON.Print(loginPayload));
		_client.SetWriteMode(wm);
		_loginModal.QueueFree();
	}
	private void _OnClientWriteLine(string message)
	{
		_logDest.AddText($"{message}\n");
	}
	public void _OnModeItemSelected(int _id)
	{
		_client.SetWriteMode((WebSocketPeer.WriteMode)_writeMode.GetSelectedMetadata());
	}
	public void _OnSendPressed()
	{
		if(_lineEdit.Text == "")
		{
			return;
		}
		_utils._Log(_logDest, $"Sending data {_lineEdit.Text}");
		_client.SendData(JSON.Print(new Godot.Collections.Dictionary() {
			["event"] = "message",
			["data"] = _lineEdit.Text
		}));
		_lineEdit.Text = "";
	}
	public void _OnConnectToggled(bool pressed )
	{
		if(pressed)
		{
			if(_host.Text != "")
			{
				SpawnLoginModal();
			}
		}
		else
		{
			_writeMode.Disabled = false;
			_client.DisconnectFromHost();
		}
	}
}
