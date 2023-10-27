﻿using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace VRCImageHelper
{
    class RoomInfo
    {
        public string? World_id { get; set; }
        public string? World_name { get; set; }
        public string? Permission { get; set; }
        public string? Organizer { get; set; }
    }

    class State
    {
        public State()
        {
            FocalLength = 0;
            ApertureValue = 0;
            CreationDate = "";
            Players = new List<string>();
            RoomInfo = new RoomInfo();
        }
        [JsonIgnore]
        public string CreationDate { get; set; }
        [JsonIgnore]
        public int FocalLength { get; set; }
        [JsonIgnore]
        public int ApertureValue { get; set; }
        public List<string> Players { get; set; }
        public RoomInfo RoomInfo { get; set; }
    }

    internal class Info
    {
        public static State _state = new();
        public static State State { get { return _state; } }

        public static void WorldId(object sender, NewLineEventArgs e)
        {
            var match = Regex.Match(e.Line, ".*\\[Behaviour\\] Joining (wrld_.*?):(?:.*?(private|friends|hidden|group)\\((.*?)\\))?(~canRequestInvite)?");
            if (match.Success)
            {
                Debug.WriteLine($"Joining {match.Groups[1]}, {match.Groups[2]}, {match.Groups[3]}, {match.Groups[4]}");
                _state.RoomInfo.World_id = match.Groups[1].Value;
                _state.RoomInfo.Permission = match.Groups[2].Value;
                _state.RoomInfo.Organizer = match.Groups[3].Value;
                _state.Players.Clear();
            }
        }

        public static void JoinRoom(object sender, NewLineEventArgs e)
        {
            var match = Regex.Match(e.Line, "Joining or Creating Room: (.*)");
            if (match.Success)
            {
                Debug.WriteLine($"Joining {match.Groups[1]}");
                _state.RoomInfo.World_name = match.Groups[1].Value;
            }
        }

        public static void PlayerJoin(object sender, NewLineEventArgs e)
        {
            var match = Regex.Match(e.Line, "OnPlayerJoined (.*)");
            if (match.Success)
            {
                Debug.WriteLine($"Join {match.Groups[1]}");
                _state.Players.Add(match.Groups[1].Value);
            }
        }

        public static void PlayerLeft(object sender, NewLineEventArgs e)
        {
            var match = Regex.Match(e.Line, "OnPlayerLeft (.*)");
            if (match.Success)
            {
                Debug.WriteLine($"Left {match.Groups[1]}");
                _state.Players.Remove(match.Groups[1].Value);
            }
        }

        public static void Quit(object sender, NewLineEventArgs e)
        {
            var match = Regex.Match(e.Line, "VRCApplication: OnApplicationQuit");
            if (match.Success)
            {
                Debug.WriteLine("Quit");
                _state = new State();
            }
        }

        public static void VL2Zoom(object sender, OscEventArgs e)
        {
            if (e.Path == "/avatar/parameters/VirtualLens2_Zoom")
            {

            }
        }

        public static void VL2Control(object sender, OscEventArgs e)
        {
            if (e.Path == "/avatar/parameters/VirtualLens2_Control")
            {

            }
        }

        public static void VL2Aperture(object sender, OscEventArgs e)
        {
            if (e.Path == "/avatar/parameters/VirtualLens2_Aperture")
            {

            }
        }

        public static void ChangeAvater(object sender, OscEventArgs e)
        {
            if (e.Path == "/avatar/change")
            {

            }
        }
    }
}
