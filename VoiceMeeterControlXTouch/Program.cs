using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceMeeterWrapper;
using Sanford.Multimedia.Midi;
using System.Threading;

namespace VoiceMeeterControlXTouch
{
    class Program
    {
        private static int GetMidiInputDevice(string partialDeviceName)
        {
            for (int i = 0; i < InputDevice.DeviceCount; i++)
            {
                var info = InputDevice.GetDeviceCapabilities(i);
                if (info.name.Contains(partialDeviceName))
                    return i;
            }
            throw new Exception($"Cannot find input midi device with '{partialDeviceName}' in the name.");
        }
        private static int GetMidiOutputDevice(string partialDeviceName)
        {
            for (int i = 0; i < OutputDeviceBase.DeviceCount; i++)
            {
                var info = OutputDeviceBase.GetDeviceCapabilities(i);
                if (info.name.Contains(partialDeviceName))
                    return i;
            }
            throw new Exception($"Cannot find output midi device with '{partialDeviceName}' in the name.");
        }


        static void Main(string[] args)
        {
            var isCanceled = false;


            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Got the Ctrl-C");
                isCanceled = true;
                eventArgs.Cancel = true;
            };

            var deviceName = "X";
            var vb = new VmClient();
            var od = new OutputDevice(GetMidiOutputDevice(deviceName));
            var id = new InputDevice(GetMidiInputDevice(deviceName));

            for (int channel = 1; channel <= 8; channel++)
            {
                od.Send(new ChannelMessage(ChannelCommand.Controller, 0, channel, 0));
            }

            //UpdateCCVoiceMeter(vb, "Bus[0].Gain", 116);
            //UpdateCCVoiceMeter(vb, "Bus[1].Gain", 89);
            //UpdateCCVoiceMeter(vb, "Bus[2].Gain", 0);
            //UpdateCCVoiceMeter(vb, "Bus[3].Gain", 127, 12.0f);
            //UpdateCCVoiceMeter(vb, "Bus[4].Gain", 127, 12.0f);

            //UpdateCCVoiceMeter(vb, "Strip[0].Gain", 102);
            //UpdateCCVoiceMeter(vb, "Strip[1].Gain", 102);
            //UpdateCCVoiceMeter(vb, "Strip[2].Gain", 127, 12.0f);
            //UpdateCCVoiceMeter(vb, "Strip[3].Gain", 102);
            //UpdateCCVoiceMeter(vb, "Strip[4].Gain", 102);


            id.ChannelMessageReceived += (ob, e) =>
            {
                var m = e.Message;
                if (m.MessageType == MessageType.Channel && m.Command == ChannelCommand.NoteOff)
                {
                    float vmVal;
                    switch (m.Data1)
                    {
                        case 8:
                            ToggleVoiceMeeter(vb, "Strip[0].B1");
                            break;
                        case 16:
                            ToggleVoiceMeeter(vb, "Strip[0].B2");
                            break;

                        case 9:
                            ToggleVoiceMeeter(vb, "Strip[1].B1");
                            break;
                        case 17:
                            ToggleVoiceMeeter(vb, "Strip[1].B2");
                            break;

                        case 10:
                            ToggleVoiceMeeter(vb, "Strip[2].B1");
                            break;
                        case 18:
                            ToggleVoiceMeeter(vb, "Strip[2].B2");
                            break;

                        case 12:
                            ToggleVoiceMeeter(vb, "Strip[3].A1");
                            break;
                        case 20:
                            ToggleVoiceMeeter(vb, "Strip[3].A2");
                            break;

                        case 13:
                            ToggleVoiceMeeter(vb, "Strip[4].A1");
                            break;
                        case 21:
                            ToggleVoiceMeeter(vb, "Strip[4].A2");
                            break;

                        case 14:
                            ToggleVoiceMeeter(vb, "Bus[0].Mute");
                            break;
                        case 15:
                            ToggleVoiceMeeter(vb, "Bus[1].Mute");
                            break;
                        case 23:
                            ToggleVoiceMeeter(vb, "Bus[2].Mute");
                            break;

                    }
                }
                else if (m.MessageType == MessageType.Channel && m.Command == ChannelCommand.Controller)
                {
                    switch (m.Data1)
                    {
                        case 1: // CC1
                            UpdateCCVoiceMeter(vb, "Strip[0].Gain", m.Data2);
                            break;
                        case 2: // CC2
                            UpdateCCVoiceMeter(vb, "Strip[1].Gain", m.Data2);
                            break;
                        case 3:
                            UpdateCCVoiceMeter(vb, "Strip[2].Gain", m.Data2);
                            //UpdateCCVoiceMeter(vb, "Strip[2].Gain", m.Data2, 12.0f);
                            break;

                        case 5:
                            UpdateCCVoiceMeter(vb, "Strip[3].Gain", m.Data2);
                            break;

                        case 6:
                            UpdateCCVoiceMeter(vb, "Strip[4].Gain", m.Data2);
                            break;

                        case 7:
                            UpdateCCVoiceMeter(vb, "Bus[0].Gain", m.Data2);
                            break;
                        case 8:
                            UpdateCCVoiceMeter(vb, "Bus[1].Gain", m.Data2);
                            break;
                        case 9:
                            UpdateCCVoiceMeter(vb, "Bus[2].Gain", m.Data2);
                            break;
                    }
                }

            };
            id.StartRecording();

            while (!isCanceled)
            {
                if (vb.Poll())
                {
                    //strip 0
                    var vmVal = vb.GetParam($"Strip[0].B1");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 0, (int)vmVal));
                    vmVal = vb.GetParam($"Strip[0].B2");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 8, (int)vmVal));

                    //strip 1
                    vmVal = vb.GetParam($"Strip[1].B1");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 1, (int)vmVal));
                    vmVal = vb.GetParam($"Strip[1].B2");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 9, vmVal == 1 ? 2 : 0));

                    //strip 2
                    vmVal = vb.GetParam($"Strip[2].B1");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 2, (int)vmVal));
                    vmVal = vb.GetParam($"Strip[2].B2");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 10, vmVal == 1 ? 2 : 0));

                    //strip 3
                    vmVal = vb.GetParam($"Strip[3].A1");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 4, (int)vmVal));
                    vmVal = vb.GetParam($"Strip[3].A2");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 12, (int)vmVal));

                    //strip 4
                    vmVal = vb.GetParam($"Strip[4].A1");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 5, (int)vmVal));
                    vmVal = vb.GetParam($"Strip[4].A2");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 13, (int)vmVal));

                    //Bus 0
                    vmVal = vb.GetParam($"Bus[0].Mute");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 6, vmVal == 1 ? 2 : 0));
                    vmVal = vb.GetParam($"Bus[1].Mute");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 7, vmVal == 1 ? 2 : 0));
                    vmVal = vb.GetParam($"Bus[2].Mute");
                    od.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 15, vmVal == 1 ? 2 : 0));
                }
                Thread.Sleep(20);
            }

            id.Dispose();
            od.Dispose();
            vb.Dispose();
        }

        private static void UpdateCCVoiceMeter(VmClient client, string stripCommand, int value, float toMax = 0.0f)
        {
            UpdateCCVoiceMeter(client, stripCommand, value, 0, 127, -60, toMax);
        }

        private static void UpdateCCVoiceMeter(VmClient client, string stripCommand, float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            var scaledVal = Scale(value, fromMin, fromMax, toMin, toMax);
            client.SetParam(stripCommand, scaledVal);
        }

        private static void ToggleVoiceMeeter(VmClient client, string paramCommand)
        {
            var vmVal = client.GetParam(paramCommand);
            var currentFlag = vmVal == 1;
            client.SetParam(paramCommand, currentFlag ? 0 : 1);
        }

        private static float Scale(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            var zeroToOne = ((value - fromMin) / (fromMax - fromMin));
            var ans = zeroToOne * (toMax - toMin) + toMin;
            //Console.WriteLine($"Scale {value} from {fromMin}..{fromMax} to {toMin}..{toMax}: {zeroToOne} {ans}");
            return ans;
        }
    }
}
