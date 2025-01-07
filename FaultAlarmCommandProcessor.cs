namespace WinFormsSerial
{
    public class FaultAlarmCommandProcessor
    {
        private readonly Action<string> _logCallback;

        public FaultAlarmCommandProcessor(Action<string> logCallback)
        {
            _logCallback = logCallback;
        }

        public void ProcessResponse(byte faultAlarmCommand, byte[] response)
        {
            if (response.Length == 0)
            {
                _logCallback("No response received");
                return;
            }

            byte firstByte = response[0];

            switch (faultAlarmCommand)
            {
                case Constants.IS_THERE_FIRE_ALARM:
                    ProcessFireAlarm(firstByte);
                    break;
                case Constants.IS_THERE_ZONE_LINE_FAULT:
                    ProcessZoneLineFault(firstByte);
                    break;
                case Constants.IS_THERE_CONTROL_PANEL_FAULT:
                    ProcessControlPanelFault(firstByte);
                    break;
                default:
                    _logCallback($"Unknown fault/alarm command {faultAlarmCommand}");
                    break;
                    
            }
        }

        private void ProcessFireAlarm(byte responseFirstByte)
        {
            if (responseFirstByte == 0)
                _logCallback("No fire alarm.");
            else
                _logCallback($"Fire alarm in zones: {GetZoneWithProblemsAsString(responseFirstByte)}");
        }

        private void ProcessZoneLineFault(byte responseFirstByte)
        {
            if (responseFirstByte == 0)
                _logCallback("No zone line fault.");
            else
                _logCallback($"Zone line fault in zones: {GetZoneWithProblemsAsString(responseFirstByte)}");
        }

        private void ProcessControlPanelFault(byte response)
        {
            if (response == 0)
            {
                _logCallback("No control panel fault.");
                return;
            }

            _logCallback("Control panel fault conditions:");
            var faults = new Dictionary<byte, string>
            {
                { 0b00000001, "Batarya yok" },
                { 0b00000010, "Batarya zayıf" },
                { 0b00000100, "Şebeke yok" },
                { 0b00001000, "Şarj zayıf" },
                { 0b00010000, "Siren1 Arıza" },
                { 0b00100000, "Siren2 Arıza" },
                { 0b01000000, "Çıkış arıza" },
                { 0b10000000, "Toprak arıza" }
            };

            foreach (var fault in faults)
            {
                if ((response & fault.Key) != 0)
                    _logCallback(fault.Value);
            }
        }

        private static string GetZoneWithProblemsAsString(byte responseFirstByte)
        {
            List<string> zonesStr = GetZonesWithProblems(responseFirstByte).Select(x => x.ToString()).ToList();            
            return string.Join(" ", zonesStr);
        }

        public static List<int> GetZonesWithProblems(byte responseFirstByte)
        {
            var zones = new List<int>();
            for (int iZone = 0; iZone < Constants.NB_OF_ZONES; iZone++)
            {
                if ((responseFirstByte & (1 << iZone)) != 0)
                    zones.Add(iZone);
            }
            return zones;
        }
    }
}
