namespace WinFormsSerial
{
    public class CommandProcessor
    {
        private readonly Action<string> _logCallback;

        public CommandProcessor(Action<string> logCallback)
        {
            _logCallback = logCallback;
        }

        public void ProcessResponse(byte command, byte[] response)
        {
            if (response.Length == 0)
            {
                _logCallback("No response received");
                return;
            }

            byte firstByte = response[0];

            switch (command)
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
                    // ... other cases
            }
        }

        private void ProcessFireAlarm(byte response)
        {
            if (response == 0)
                _logCallback("No fire alarm.");
            else
                _logCallback($"Fire alarm at zones: {GetZoneNumbers(response)}");
        }

        private void ProcessZoneLineFault(byte response)
        {
            if (response == 0)
                _logCallback("No zone line fault.");
            else
                _logCallback($"Zone line fault at zones: {GetZoneNumbers(response)}");
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
                { 0b00010000, "Siren Arıza" },
                { 0b00100000, "Çıkış arıza" },
                { 0b01000000, "Toprak arıza" }
            };

            foreach (var fault in faults)
            {
                if ((response & fault.Key) != 0)
                    _logCallback(fault.Value);
            }
        }

        private static string GetZoneNumbers(byte response)
        {
            var zones = new List<string>();
            for (int i = 0; i < 8; i++)
            {
                if ((response & (1 << i)) != 0)
                    zones.Add((i + 1).ToString());
            }
            return string.Join(" ", zones);
        }
    }
}
