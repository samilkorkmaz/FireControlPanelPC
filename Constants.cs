namespace FireControlPanelPC
{
    public static class Constants
    {
        public const byte IS_THERE_FIRE_ALARM = 35;
        public const byte IS_THERE_ZONE_LINE_FAULT = 36;
        public const byte IS_THERE_CONTROL_PANEL_FAULT = 37;
        public const byte RESET_CONTROL_PANEL = 38;
        public const byte STOP_BUZZER = 39;
        public const byte GET_ZONE_NAMES_COMMAND = 14;
        public const byte SET_ZONE_NAMES_COMMAND = 13;
        public const int NB_OF_ZONES = 8;
        public const int ZONE_NAME_LENGTH = 16;
        public static readonly byte[] PERIODIC_COMMANDS_ORDER = {
            IS_THERE_FIRE_ALARM,
            IS_THERE_ZONE_LINE_FAULT,
            IS_THERE_FIRE_ALARM,
            IS_THERE_CONTROL_PANEL_FAULT
        };
    }
}
