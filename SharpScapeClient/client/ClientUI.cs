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
	public override void _Ready()
	{
		_utils=GetNode<Utils>("/root/Utils");
		_client = GetNode<Client>("Client");
		_logDest = GetNode<RichTextLabel>("Panel/VBoxContainer/RichTextLabel");
		_lineEdit = GetNode<LineEdit>("Panel/VBoxContainer/Send/LineEdit");
		_host = GetNode<LineEdit>("Panel/VBoxContainer/Connect/Host");
		_writeMode = GetNode<OptionButton>("Panel/VBoxContainer/Settings/Mode");
		_writeMode.Clear();
		_writeMode.AddItem("BINARY");
		_writeMode.SetItemMetadata(0, WebSocketPeer.WriteMode.Binary);
		_writeMode.AddItem("TEXT");
		_writeMode.SetItemMetadata(1, WebSocketPeer.WriteMode.Text);
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
		_client.SendData(_lineEdit.Text);
		_lineEdit.Text = "";
	}
	public void _OnConnectToggled(bool pressed )
	{
		if(pressed)
		{
			if(_host.Text != "")
			{
				_utils._Log(_logDest, $"Connecting to host: {_host.Text}");
				string[] supportedProtocols = {"my-protocol2", "my-protocol", "binary"};
				_client.ConnectToUrl(_host.Text, supportedProtocols);
			}
		}
		else
		{
			_writeMode.Disabled = false;
			_client.DisconnectFromHost();
		}
	}
}